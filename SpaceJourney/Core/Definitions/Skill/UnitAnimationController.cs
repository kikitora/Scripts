using System;
using System.Collections;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// バトルユニットのアニメーション制御と、
    /// AnimationEvent からゲームロジック側へイベントを転送する窓口を担当します。
    /// - Animator へのトリガー送信
    /// - AnimationEvent から呼ばれる共通関数（AnimEvent_◯◯）
    /// - 外部から購読できる event (OnSkillAnimHit など)
    /// </summary>
    public class UnitAnimationController : MonoBehaviour
    {
        [Header("Animator 参照")]
        [SerializeField] private Animator animator;

        [Header("アクションステート設定（必要なら使用）")]
        [SerializeField] private string actionStateTag = "Action";
        [SerializeField] private int actionLayerIndex = 0;

        /// <summary>スキルのキャスト開始タイミング用。</summary>
        public event Action OnSkillAnimCast;

        /// <summary>ヒットタイミング用。</summary>
        public event Action OnSkillAnimHit;

        /// <summary>移動開始タイミング用。</summary>
        public event Action OnSkillAnimMove;

        /// <summary>スキルアニメ終了タイミング用。</summary>
        public event Action OnSkillAnimEnd;

        /// <summary>外部から Animator を直接触りたい場合用のプロパティ。</summary>
        public Animator Animator => animator;

        // ─────────────────────────────
        // AnimationEvent から呼ばれる共通関数
        // （クリップ側ではこれらの関数名をイベントに設定する）
        // ─────────────────────────────

        // 例：キャストモーション中の「手をかざした瞬間」などに打つ
        public void AnimEvent_SkillCast()
        {
            OnSkillAnimCast?.Invoke();
        }

        // 例：攻撃が当たるフレームに打つ
        public void AnimEvent_SkillHit()
        {
            OnSkillAnimHit?.Invoke();
        }

        // 例：前進し始めるフレームなどに打つ
        public void AnimEvent_SkillMove()
        {
            OnSkillAnimMove?.Invoke();
        }

        // 例：モーションの最後に1個だけ打つ
        public void AnimEvent_SkillEnd()
        {
            OnSkillAnimEnd?.Invoke();
        }

        // ─────────────────────────────
        // （おまけ）タグ付きアクションステートの終了待ち
        // ─────────────────────────────

        /// <summary>
        /// actionStateTag が付いているステートが再生開始してから、
        /// 正常終了（normalizedTime >= 1 && 遷移中でない）まで待つ。
        /// AnimationEvent を使わず、ステートの終了だけ待ちたいとき用です。
        /// </summary>
        public IEnumerator WaitForActionAnimEnd()
        {
            if (animator == null)
                yield break;

            bool started = false;

            while (true)
            {
                var info = animator.GetCurrentAnimatorStateInfo(actionLayerIndex);

                if (!started)
                {
                    if (info.IsTag(actionStateTag))
                        started = true;
                }
                else
                {
                    if (info.normalizedTime >= 1f && !animator.IsInTransition(actionLayerIndex))
                        break;
                }

                yield return null;
            }
        }
    }
}
