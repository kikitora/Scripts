using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// 左手を leftHandTarget に IK で吸着させる。両手武器の握り位置合わせ用。
    /// EquipWeapon が target を設定し、武器を外したら null を入れる。
    /// Animator と同じ GameObject にアタッチすること（OnAnimatorIK 仕様）。
    /// 該当 Animator Controller のレイヤーで IK Pass を ON にする必要がある。
    ///
    /// アニメ別 ON/OFF: disableInStates に State 名 (Animator State の short name) を
    /// 並べると、その State 再生中は IK が自動で無効化される。
    /// 「走る (Walk) では IK が浮く / 腕が捻れる」ので走り系を default で無効化している。
    /// 重み変動は smoothSeconds 秒で線形補間。0 にするとスナップ。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class WeaponLeftHandIK : MonoBehaviour
    {
        public Transform leftHandTarget;
        [Range(0f, 1f)] public float weight = 1f;

        [Tooltip("これらの State を再生中は IK を無効化する (Animator State の short name)。" +
                 "Walk / WalkLeft / WalkRight など走り系は IK の手が剣のグリップに引っ張られて " +
                 "腕が捻れて見えるので default 無効。")]
        public string[] disableInStates = { "Walk", "WalkLeft", "WalkRight" };

        [Tooltip("コードから一時的に IK をオフにしたい時 true。Inspector では default false。")]
        [System.NonSerialized] public bool runtimeDisable = false;

        [Tooltip("IK weight を ON/OFF 切り替えるときの補間時間 (秒)。0 でスナップ。")]
        [Range(0f, 0.5f)] public float smoothSeconds = 0.12f;

        private Animator animator;
        private int[] _disableHashes;
        private bool _hashesDirty = true;
        private float _appliedWeight = 0f;
        private bool _logged;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnValidate() { _hashesDirty = true; }

        /// <summary>runtime で disableInStates を差し替える時の入口。
        /// 直接フィールドを書くと内部 hash キャッシュが古いまま動くので、こちら経由で設定する。</summary>
        public void SetDisableInStates(string[] states)
        {
            disableInStates = states ?? System.Array.Empty<string>();
            _hashesDirty = true;
        }

        private void RebuildHashes()
        {
            if (disableInStates == null) { _disableHashes = System.Array.Empty<int>(); _hashesDirty = false; return; }
            _disableHashes = new int[disableInStates.Length];
            for (int i = 0; i < disableInStates.Length; i++)
                _disableHashes[i] = string.IsNullOrEmpty(disableInStates[i])
                    ? 0 : Animator.StringToHash(disableInStates[i]);
            _hashesDirty = false;
        }

        private bool ShouldDisableForCurrentState()
        {
            if (animator == null) return false;
            if (_disableHashes == null || _hashesDirty) RebuildHashes();
            if (_disableHashes.Length == 0) return false;

            // layer 0 のみ判定 (Base Layer)。Transition 中は次 state も評価する。
            var cur = animator.GetCurrentAnimatorStateInfo(0);
            bool inTransition = animator.IsInTransition(0);
            AnimatorStateInfo nxt = inTransition ? animator.GetNextAnimatorStateInfo(0) : default;
            for (int i = 0; i < _disableHashes.Length; i++)
            {
                int h = _disableHashes[i];
                if (h == 0) continue;
                if (cur.shortNameHash == h) return true;
                if (inTransition && nxt.shortNameHash == h) return true;
            }
            return false;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_logged)
            {
                _logged = true;
                Debug.Log($"[WeaponLeftHandIK] OnAnimatorIK called on {name} layer={layerIndex} " +
                    $"target={(leftHandTarget != null ? leftHandTarget.name : "null")} disable=[{string.Join(",", disableInStates ?? new string[0])}]");
            }
            if (animator == null) return;

            bool active = leftHandTarget != null
                       && !runtimeDisable
                       && !ShouldDisableForCurrentState();
            float targetW = active ? weight : 0f;

            // 補間 (フレームスキップで急変しすぎないよう Time.deltaTime ベース)
            if (smoothSeconds > 0f)
            {
                float maxDelta = Time.deltaTime / smoothSeconds;
                _appliedWeight = Mathf.MoveTowards(_appliedWeight, targetW, maxDelta);
            }
            else
            {
                _appliedWeight = targetW;
            }

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _appliedWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, _appliedWeight);
            if (_appliedWeight > 0f && leftHandTarget != null)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }
        }
    }
}
