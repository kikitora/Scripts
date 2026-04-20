using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 生成された戦闘ユニットモデルの Animator を制御する。
    /// SteraCube_Humanoid.controller の "animation" Int パラメータを用いる。
    ///
    /// Animator の animation 値は Controller 次第なので、
    /// Inspector で各状態の数値を設定できるようにする。
    /// </summary>
    public class BattleUnitAnimator : MonoBehaviour
    {
        [Header("animation Int パラメータの値 (SteraCube_Humanoid)")]
        [Tooltip("待機状態 (デフォルト)")]
        public int idleValue = 4;
        [Tooltip("攻撃モーション")]
        public int attackValue = 12;
        [Tooltip("被弾モーション")]
        public int damageValue = 23;
        [Tooltip("死亡モーション")]
        public int deathValue = 29;

        [Header("オプション")]
        [Tooltip("animation 値をリセットしてアイドルに戻すまでの秒数 (0=即時復帰、負=戻さない)")]
        public float autoReturnIdleSec = 1.2f;

        private Animator animator;
        private const string ANIM_PARAM = "animation";
        private const string ONCE_TRIGGER = "once";
        private bool hasAnimParam;
        private bool hasOnceTrigger;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                foreach (var p in animator.parameters)
                {
                    if (p.name == ANIM_PARAM) hasAnimParam = true;
                    if (p.name == ONCE_TRIGGER) hasOnceTrigger = true;
                }
            }
        }

        private void SetAnim(int value, bool reTrigger)
        {
            if (!hasAnimParam) return;
            animator.SetInteger(ANIM_PARAM, value);
            // 同じ値を2回セットしても遷移が起きないので once トリガーで強制再生
            if (reTrigger && hasOnceTrigger)
                animator.SetTrigger(ONCE_TRIGGER);
        }

        public void PlayIdle()
        {
            SetAnim(idleValue, reTrigger: false);
        }

        public void PlayAttack()
        {
            SetAnim(attackValue, reTrigger: true);
            ReturnToIdleLater();
        }

        public void PlayDamage()
        {
            SetAnim(damageValue, reTrigger: true);
            ReturnToIdleLater();
        }

        public void PlayDeath()
        {
            SetAnim(deathValue, reTrigger: true);
            // 死亡は戻さない
        }

        private void ReturnToIdleLater()
        {
            if (autoReturnIdleSec < 0f) return;
            CancelInvoke(nameof(PlayIdle));
            Invoke(nameof(PlayIdle), autoReturnIdleSec);
        }
    }
}
