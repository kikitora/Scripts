using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // LoveEvents (恋愛・結婚)
    // ================================================================
    // 男女中立で書く。「恋人」「相手」「パートナー」など。
    //
    // 構造:
    //   ev_first_love (初恋)
    //     → ev_relationship_1 (1人目の交際)
    //         → opt: 別れる(rel_1_break) or 結婚(married)
    //   → ev_relationship_2 (2人目の交際, requires=rel_1_break)
    //         → opt: 別れる(rel_2_break) or 結婚(married)
    //   → ev_relationship_3 (3人目の交際, requires=rel_2_break)
    //         → opt: 別れる(rel_3_break) or 結婚(married)
    //
    //   ev_omiai (お見合い): blocked=lover系&married で、奥手な人生
    //
    //   結婚後イベント:
    //     ev_wedding (新婚旅行も含む), ev_first_child, ev_anniversary_silver
    //
    //   離婚:
    //     ev_divorce: married で、stage=-1 (低確率)
    // ================================================================
    public static class LoveEvents
    {
        public static IEnumerable<ReinLifeEvent> All()
        {
            foreach (var ev in FirstLove()) yield return ev;
            foreach (var ev in Relationships()) yield return ev;
            foreach (var ev in Omiai()) yield return ev;
            foreach (var ev in MarriedLife()) yield return ev;
            foreach (var ev in Divorce()) yield return ev;
        }

        // ─────────────────────────────────────────
        // 初恋
        // ─────────────────────────────────────────
        #region 初恋
        private static IEnumerable<ReinLifeEvent> FirstLove()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_first_love",
                sentence = "同じ学校の人を好きになった。想いは誰にも言えなかった。",
                startAge = 11,
                endAge = 16,
                baseWeight = 0.8f,
                grantsLifeTags = new() { "first_love" },
                eventStage = 1,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 1 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 交際 (3人まで連鎖)
        // ─────────────────────────────────────────
        #region 交際
        private static IEnumerable<ReinLifeEvent> Relationships()
        {
            // ─── 1人目 ───
            // ほとんど別れる (約80%)、若いうちに結婚に至るのは少なめ
            yield return new ReinLifeEvent
            {
                eventId = "ev_relationship_1",
                sentence = "勇気を出して告白した。交際が始まった。",
                startAge = 17,
                endAge = 25,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "first_love" },
                blockedByLifeTags = new() { "married" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "恋人と、価値観の違いから別れた。",
                        baseWeight = 4.0f,
                        grantsLifeTags = new() { "rel_1_break" },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "そのままこの人と結婚することに決めた。",
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "married" },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                },
            };

            // ─── 2人目 ───
            // rel_1 で別れた人の多くがここで結婚 (約70%)
            yield return new ReinLifeEvent
            {
                eventId = "ev_relationship_2",
                sentence = "知人の紹介で出会った人と交際を始めた。",
                startAge = 20,
                endAge = 32,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "rel_1_break" },
                blockedByLifeTags = new() { "married" },
                eventStage = 1,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "恋人の転勤を機に破局した。",
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "rel_2_break" },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "この人と今度こそと決意し、結婚することになった。",
                        baseWeight = 3.0f,
                        grantsLifeTags = new() { "married" },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                },
            };

            // ─── 3人目 ───
            // rel_2 で別れた人のうち、ここまで来る人はだいぶ少なめ。
            // たどり着けばほぼ結婚 (~80%) するが、ごく稀に最後まで結ばれない。
            yield return new ReinLifeEvent
            {
                eventId = "ev_relationship_3",
                sentence = "三度目の交際が始まった。今度の恋人とは穏やかな関係だった。",
                startAge = 25,
                endAge = 40,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "rel_2_break" },
                blockedByLifeTags = new() { "married" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "この関係も続かず、恋人と別れた。",
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "rel_3_break" },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "この恋人と生涯を共にすると決め、結婚することになった。",
                        baseWeight = 4.0f,
                        grantsLifeTags = new() { "married" },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // お見合い (奥手な人生用 / 結婚してない年配)
        // ─────────────────────────────────────────
        #region お見合い
        private static IEnumerable<ReinLifeEvent> Omiai()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_omiai",
                sentence = "親類の紹介で見合いの席に着いた。",
                startAge = 30,
                endAge = 42,
                baseWeight = 0.4f,
                // 恋愛経験が一度もない奥手な人生のみ。rel経験者は対象外。
                blockedByLifeTags = new() { "married", "rel_1_break", "rel_2_break", "rel_3_break", "first_love" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "数回会った末、結婚に至った。",
                        baseWeight = 1.5f,
                        grantsLifeTags = new() { "married", "married_omiai" },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "波長が合わず、お断りした。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 結婚後の生活
        // ─────────────────────────────────────────
        #region 結婚後
        private static IEnumerable<ReinLifeEvent> MarriedLife()
        {
            // 結婚式 + 新婚旅行 (まとめて1イベント)
            // married 取得後の確定発火 (式は決まったら必ず挙げる)
            yield return new ReinLifeEvent
            {
                eventId = "ev_wedding",
                sentence = "結婚式を挙げ、新婚旅行に出かけた。穏やかな朝を二人で迎えた。",
                startAge = 22,
                endAge = 45,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "married" },
                blockedByLifeTags = new() { "wedding_done" },
                grantsLifeTags = new() { "wedding_done" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 2 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_first_child",
                sentence = "最初の子供が生まれた。小さな手の温度が忘れられなかった。",
                startAge = 24,
                endAge = 45,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "married" },
                blockedByLifeTags = new() { "parent" },
                grantsLifeTags = new() { "parent" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                            new StatBonus { stat = StatKind.DF,  value = 2 },
                        },
                    },
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_anniversary_silver",
                sentence = "結婚から二十五年が過ぎた。当時の写真を見返した。",
                startAge = 43,
                endAge = 70,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "married" },
                blockedByLifeTags = new() { "anniv_silver", "divorced" },
                grantsLifeTags = new() { "anniv_silver" },
                eventStage = 1,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 離婚 (低確率不幸)
        // ─────────────────────────────────────────
        #region 離婚
        private static IEnumerable<ReinLifeEvent> Divorce()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_divorce",
                sentence = "話し合いの末、離婚届に判を押した。",
                startAge = 22,
                endAge = 60,
                baseWeight = 0.15f,
                requiresAnyLifeTag = new() { "married" },
                blockedByLifeTags = new() { "divorced" },
                grantsLifeTags = new() { "divorced" },
                eventStage = -1,
                statCompareCount = 3,
                statCompareMode = "avg",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                    },
                },
            };
        }
        #endregion
    }
}
