using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // LifeEndEvents (死亡)
    // ================================================================
    // 命名規則: ev_death_* で始まる event は ReinSim 側で
    //   ReinEventType.LifeEnd 扱いとなり、発火後 ctx.IsDead=true で
    //   シミュループが打ち切られる。
    //
    // ルール:
    //   - 老衰以外の死亡は requireYearsAfterJob=30 (生業確定 + 30年経過後)。
    //     ランクUPスケジュール完了後にしか起きない。
    //   - 病気/事故死は baseWeight=0.05 (年あたり5%) と低確率。
    //   - 老衰は age=80-99 で年とともに baseWeight 上昇、age=100 で確定。
    // ================================================================
    public static class LifeEndEvents
    {
        public static IEnumerable<ReinLifeEvent> All()
        {
            // ─── 事故死 (生業+30年後、低確率) ───
            yield return new ReinLifeEvent
            {
                eventId = "ev_death_accident",
                sentence = "予期せぬ事故に遭った。何も告げる暇もなかった。",
                startAge = 0,
                endAge = 99,
                baseWeight = 0.05f,
                requireYearsAfterJob = 30,
                eventStage = -2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                    },
                },
            };

            // ─── 病死 (生業+30年後、低確率) ───
            yield return new ReinLifeEvent
            {
                eventId = "ev_death_disease",
                sentence = "病に倒れた。短い闘病の末、静かに息を引き取った。",
                startAge = 0,
                endAge = 99,
                baseWeight = 0.05f,
                requireYearsAfterJob = 30,
                eventStage = -2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                    },
                },
            };

            // ─── 老衰 80代 (確率徐々に上昇) ───
            yield return new ReinLifeEvent
            {
                eventId = "ev_death_natural_80",
                sentence = "老いを感じる日々が長くなり、ある朝、目覚めなかった。",
                startAge = 80,
                endAge = 89,
                baseWeight = 0.07f,
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                    },
                },
            };

            // ─── 老衰 90代 (確率高め) ───
            yield return new ReinLifeEvent
            {
                eventId = "ev_death_natural_90",
                sentence = "穏やかな朝、家族に見守られて息を引き取った。",
                startAge = 90,
                endAge = 99,
                baseWeight = 0.18f,
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                    },
                },
            };

            // ─── 100歳 確定老衰 ───
            yield return new ReinLifeEvent
            {
                eventId = "ev_death_natural_g",
                sentence = "百年の生涯を終えた。長い旅だった。",
                startAge = 100,
                endAge = 100,
                baseWeight = 1.0f,
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                    },
                },
            };
        }
    }
}
