using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 職別 AnimatorController (Idle/Walk/Attack/Damage/Die) を制御する。
    /// JobAnimatorBuilder で生成された controller を前提。
    /// パラメータ: Bool Moving, Trigger Attack/Damage/Die
    /// </summary>
    public class JobAnimator : MonoBehaviour
    {
        private Animator animator;
        private bool hasMoving, hasAttack, hasDamage, hasDie, hasVictory, hasDefeat;
        private int blockedLookAroundLayer = -1;

        private const string PARAM_MOVING = "Moving";
        private const string PARAM_ATTACK = "Attack";
        private const string PARAM_DAMAGE = "Damage";
        private const string PARAM_DIE = "Die";
        private const string PARAM_VICTORY = "Victory";
        private const string PARAM_DEFEAT = "Defeat";
        private const string LAYER_BLOCKED_LOOK_AROUND = "BlockedLookAround";

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            DetectParams();
            LogStatus("Awake");
        }

        public void RefreshParams()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            DetectParams();
            LogStatus("RefreshParams");
        }

        private void DetectParams()
        {
            hasMoving = hasAttack = hasDamage = hasDie = hasVictory = hasDefeat = false;
            blockedLookAroundLayer = -1;
            if (animator == null || animator.runtimeAnimatorController == null) return;
            foreach (var p in animator.parameters)
            {
                if (p.name == PARAM_MOVING) hasMoving = true;
                if (p.name == PARAM_ATTACK) hasAttack = true;
                if (p.name == PARAM_DAMAGE) hasDamage = true;
                if (p.name == PARAM_DIE)    hasDie = true;
                if (p.name == PARAM_VICTORY) hasVictory = true;
                if (p.name == PARAM_DEFEAT) hasDefeat = true;
            }
            blockedLookAroundLayer = animator.GetLayerIndex(LAYER_BLOCKED_LOOK_AROUND);
            if (blockedLookAroundLayer >= 0)
                animator.SetLayerWeight(blockedLookAroundLayer, 0f);
        }

        private void LogStatus(string phase)
        {
            string cName = animator?.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "null";
            string aName = animator != null ? animator.name : "null";
            string avName = animator?.avatar != null ? animator.avatar.name : "null";
            Debug.Log($"[JobAnimator:{name}] {phase}: animator={aName} controller={cName} avatar={avName} " +
                      $"params[M={hasMoving} A={hasAttack} D={hasDamage} Die={hasDie} V={hasVictory} Def={hasDefeat}]");
        }

        public void SetMoving(bool moving)
        {
            if (!hasMoving || animator == null) return;
            animator.SetBool(PARAM_MOVING, moving);
        }

        public void SetBlockedLookAround(bool active)
        {
            if (animator == null || blockedLookAroundLayer < 0) return;
            animator.SetLayerWeight(blockedLookAroundLayer, active ? 1f : 0f);
        }

        /// <summary>方向転換アニメ。WalkRight/WalkLeft state があれば CrossFade、無ければ Walk fallback。
        /// 戻り値: 専用 state を再生できたら true、Walk fallback なら false。</summary>
        public bool PlayTurn(bool right)
        {
            if (animator == null) return false;
            string state = right ? "WalkRight" : "WalkLeft";
            if (HasState(state))
            {
                // 専用 state 再生 (Walk と同レイヤーなので Moving=false にして競合回避)
                if (hasMoving) animator.SetBool(PARAM_MOVING, false);
                animator.CrossFadeInFixedTime(state, 0.08f);
                return true;
            }
            return false;
        }

        public void PlayAttack()
        {
            if (!hasAttack || animator == null) return;
            animator.SetTrigger(PARAM_ATTACK);
        }

        /// <summary>スキル別アニメ用の発火。
        /// Skill SO の <c>animStateName</c> が指定されていれば CrossFade (State 名直接指定)、
        /// 無ければ <c>animTriggerName</c> の Trigger を立てる (Animator Controller 側で AnyState→State 遷移を作っておく前提)。
        /// どちらも空 or 該当 Parameter/State がない場合はデフォルト Attack に fallback。</summary>
        public void PlayAttackForSkill(SteraCube.SpaceJourney.Realtime.RealtimeSkillDefinition skill)
        {
            if (animator == null) return;

            // 1) animStateName が指定されてて、その State が Animator に存在するなら CrossFade で直接遷移
            string stateName = skill?.animStateName;
            if (!string.IsNullOrEmpty(stateName) && HasState(stateName))
            {
                if (hasAttack) animator.ResetTrigger(PARAM_ATTACK);
                animator.CrossFadeInFixedTime(stateName, 0.08f);
                return;
            }

            // 2) animTriggerName が指定されてて、Parameter が存在するなら Trigger を立てる
            string triggerName = skill?.animTriggerName;
            if (!string.IsNullOrEmpty(triggerName) && HasParameter(triggerName))
            {
                animator.SetTrigger(triggerName);
                return;
            }

            // 3) fallback: 既定の Attack Trigger
            if (hasAttack) animator.SetTrigger(PARAM_ATTACK);
        }

        /// <summary>Animator に指定名の Parameter があるか。AnimatorController の parameters を線形探索。</summary>
        private bool HasParameter(string name)
        {
            if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrEmpty(name)) return false;
            foreach (var p in animator.parameters)
            {
                if (p.name == name) return true;
            }
            return false;
        }

        public void PlayDamage()
        {
            if (!hasDamage || animator == null) return;
            animator.SetTrigger(PARAM_DAMAGE);
        }

        public void PlayDie()
        {
            if (animator == null) return;
            // 他 trigger を全部 Reset してから Die を立てる (同フレーム内の Damage 遷移が優先されるのを防ぐ)
            if (hasMoving) animator.SetBool(PARAM_MOVING, false);
            if (hasAttack) animator.ResetTrigger(PARAM_ATTACK);
            if (hasDamage) animator.ResetTrigger(PARAM_DAMAGE);
            if (hasVictory) animator.ResetTrigger(PARAM_VICTORY);
            if (hasDefeat) animator.ResetTrigger(PARAM_DEFEAT);
            if (hasDie) animator.SetTrigger(PARAM_DIE);
            else animator.CrossFade("Death", 0.1f);
        }

        /// <summary>勝利ポーズ</summary>
        public void PlayVictory()
        {
            if (animator == null) return;
            if (hasMoving) animator.SetBool(PARAM_MOVING, false);
            if (hasAttack) animator.ResetTrigger(PARAM_ATTACK);
            if (hasDamage) animator.ResetTrigger(PARAM_DAMAGE);
            if (hasDie) animator.ResetTrigger(PARAM_DIE);
            if (hasDefeat) animator.ResetTrigger(PARAM_DEFEAT);
            if (hasVictory) { animator.SetTrigger(PARAM_VICTORY); return; }
            if (HasState("Victory")) animator.CrossFade("Victory", 0.2f);
            else animator.CrossFade("Idle", 0.2f);
        }

        /// <summary>敗北ポーズ</summary>
        public void PlayDefeat()
        {
            if (animator == null) return;
            if (hasMoving) animator.SetBool(PARAM_MOVING, false);
            if (hasAttack) animator.ResetTrigger(PARAM_ATTACK);
            if (hasDamage) animator.ResetTrigger(PARAM_DAMAGE);
            if (hasDie) animator.ResetTrigger(PARAM_DIE);
            if (hasVictory) animator.ResetTrigger(PARAM_VICTORY);
            if (hasDefeat) { animator.SetTrigger(PARAM_DEFEAT); return; }
            if (HasState("Defeat")) animator.CrossFade("Defeat", 0.2f);
            else animator.CrossFade("Idle", 0.2f);
        }

        private bool HasState(string stateName)
        {
            if (animator == null) return false;
            int hash = Animator.StringToHash(stateName);
            return animator.HasState(0, hash);
        }

        // ──────────────────────────────────────
        // 「移動可能か判定」用ヘルパ
        // ──────────────────────────────────────

        // Idle / Walk 系 (移動可能 state) の hash キャッシュ
        private static readonly int H_Idle = Animator.StringToHash("Idle");
        private static readonly int H_Walk = Animator.StringToHash("Walk");
        private static readonly int H_WalkLeft = Animator.StringToHash("WalkLeft");
        private static readonly int H_WalkRight = Animator.StringToHash("WalkRight");

        /// <summary>Animator が移動と両立しない state (Attack / Damage / Die / Victory / Defeat 等) に
        /// 居れば true。Idle / Walk / WalkLeft / WalkRight のどれかに居れば false。
        /// 「攻撃アニメが終わってないのに移動し始める」を防ぐため、castAnimSec 経過後でもこの flag が
        /// true なら呼び元 (RealtimeBattleUnit) は移動を抑止する。
        /// Transition 中は遷移先 state が Idle/Walk 系なら抜け始めとみなして false。</summary>
        public bool IsBusyState()
        {
            if (animator == null) return false;
            var cur = animator.GetCurrentAnimatorStateInfo(0);
            int curH = cur.shortNameHash;
            bool curBusy = !(curH == H_Idle || curH == H_Walk || curH == H_WalkLeft || curH == H_WalkRight);
            if (!curBusy) return false;
            // Transition で Idle/Walk 系へ抜けつつあるなら busy 解除
            if (animator.IsInTransition(0))
            {
                var nxt = animator.GetNextAnimatorStateInfo(0);
                int nxtH = nxt.shortNameHash;
                bool nxtMovable = (nxtH == H_Idle || nxtH == H_Walk || nxtH == H_WalkLeft || nxtH == H_WalkRight);
                if (nxtMovable) return false;
            }
            return true;
        }
    }
}
