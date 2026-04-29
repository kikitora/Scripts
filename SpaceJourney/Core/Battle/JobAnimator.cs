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

        private const string PARAM_MOVING = "Moving";
        private const string PARAM_ATTACK = "Attack";
        private const string PARAM_DAMAGE = "Damage";
        private const string PARAM_DIE = "Die";
        private const string PARAM_VICTORY = "Victory";
        private const string PARAM_DEFEAT = "Defeat";

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
    }
}
