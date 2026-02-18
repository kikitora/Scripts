using System;

namespace SteraCube.SpaceJourney
{
    // このクラスは戦闘内イベントのハブです。
    // タイミング（イベント種類）は少数に保ち、詳細はContextで表現します。
    public static class BattleEventBus
    {
        public static event Action<SkillTriggerContext> ActionEvent;
        public static event Action<SkillTriggerContext> TimeEvent;
        public static event Action<SkillTriggerContext> HpEvent;
        public static event Func<SkillTriggerContext, bool> StatusAttemptEvent; // falseでキャンセル
        public static event Action<SkillTriggerContext> BoardEvent;

        public static void RaiseAction(in SkillTriggerContext ctx) => ActionEvent?.Invoke(ctx);
        public static void RaiseTime(in SkillTriggerContext ctx) => TimeEvent?.Invoke(ctx);
        public static void RaiseHp(in SkillTriggerContext ctx) => HpEvent?.Invoke(ctx);
        public static bool RaiseStatusAttempt(in SkillTriggerContext ctx)
        {
            if (StatusAttemptEvent == null) return true;

            bool allow = true;
            foreach (Func<SkillTriggerContext, bool> handler in StatusAttemptEvent.GetInvocationList())
            {
                if (!handler.Invoke(ctx))
                    allow = false;
            }
            return allow;
        }
        public static void RaiseBoard(in SkillTriggerContext ctx) => BoardEvent?.Invoke(ctx);
    }
}
