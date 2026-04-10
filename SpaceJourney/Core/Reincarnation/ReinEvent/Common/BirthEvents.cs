using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // BirthEvents (出生)
    // ================================================================
    // age=0 で必ず1つだけ発火する。4つの出生から1つを選ぶ。
    // 各出生には固有のステ補正と固有イベントがぶら下がる。
    //
    //   都会の平凡な家庭 (birth_tokai)   AGI+3, MAT+2, DF-1
    //   田舎の平凡な家庭 (birth_inaka)   AT+3,  DF+3, MAT-2
    //   セレブな家庭     (birth_celeb)   MAT+5, MDF+3, AT-2
    //   貧しい家庭       (birth_poor)    AT+3,  DF+5, MAT-3, MDF-1
    //
    // 超高学歴必須職 (Tier S = 裁判官 / 国際弁護士) の場合、
    // 貧しい家庭からは現実的に到達不能なので、専用の ev_birth_premium を発火させ、
    // 通常の ev_birth は excludedJobIds で発火させない。
    // ================================================================
    public static class BirthEvents
    {
        // Tier S: 貧しい家庭をそもそもブロックするジョブ
        private static readonly List<string> TierSJobs = new()
        {
            "knight_saiban",
            "knight_bengoshi",
        };

        public static IEnumerable<ReinLifeEvent> All()
        {
            foreach (var ev in BirthRoot()) yield return ev;
            foreach (var ev in BirthRootPremium()) yield return ev;
            foreach (var ev in TokaiBranch()) yield return ev;
            foreach (var ev in InakaBranch()) yield return ev;
            foreach (var ev in CelebBranch()) yield return ev;
            foreach (var ev in PoorBranch()) yield return ev;
        }

        // ─────────────────────────────────────────
        // 出生本イベント (age=0, w=1.0, 必ず1択発火)
        // ─────────────────────────────────────────
        #region 出生本イベント
        private static IEnumerable<ReinLifeEvent> BirthRoot()
        {
            // 出生本イベント。本文は無し (オプション側だけが履歴に残る)
            // Tier S 職 (裁判官・国際弁護士) では発火せず、ev_birth_premium が代わりに発火する。
            yield return new ReinLifeEvent
            {
                eventId = "ev_birth",
                startAge = 0,
                endAge = 0,
                baseWeight = 1.0f,
                excludedJobIds = TierSJobs,
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "都会の平凡な家庭に生まれた。",
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "birth_tokai" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.DF,  value = -1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "田舎の平凡な家庭に生まれた。",
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "birth_inaka" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT,  value = 3 },
                            new StatBonus { stat = StatKind.DF,  value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = -2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "セレブな家庭に生まれた。",
                        baseWeight = 0.5f, // 出現少なめ
                        grantsLifeTags = new() { "birth_celeb" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 5 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                            new StatBonus { stat = StatKind.AT,  value = -2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "貧しい家庭に生まれた。",
                        baseWeight = 0.7f,
                        grantsLifeTags = new() { "birth_poor" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT,  value = 3 },
                            new StatBonus { stat = StatKind.DF,  value = 5 },
                            new StatBonus { stat = StatKind.MAT, value = -3 },
                            new StatBonus { stat = StatKind.MDF, value = -1 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 出生本イベント (Tier S 専用: 裁判官・国際弁護士)
        // 貧しい家庭からは現実的に到達不能なので、貧乏オプションを除外する。
        // セレブの出現率を上げて、エリート家庭出身が多めに出るようにする。
        // ─────────────────────────────────────────
        #region 出生本イベント (Tier S)
        private static IEnumerable<ReinLifeEvent> BirthRootPremium()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_birth_premium",
                startAge = 0,
                endAge = 0,
                baseWeight = 1.0f,
                relatedJobIds = TierSJobs,
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "都会の平凡な家庭に生まれた。",
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "birth_tokai" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.DF,  value = -1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "田舎の平凡な家庭に生まれた。",
                        baseWeight = 0.5f,
                        grantsLifeTags = new() { "birth_inaka" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT,  value = 3 },
                            new StatBonus { stat = StatKind.DF,  value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = -2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "セレブな家庭に生まれた。",
                        baseWeight = 1.5f, // Tier S はエリート家庭出身が多め
                        grantsLifeTags = new() { "birth_celeb" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 5 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                            new StatBonus { stat = StatKind.AT,  value = -2 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 都会の家庭 固有イベント
        // ─────────────────────────────────────────
        #region 都会
        private static IEnumerable<ReinLifeEvent> TokaiBranch()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_tokai_train",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "電車を乗り換えて遠出した。地図を見ずに歩けるようになった。",
                startAge = 7,
                endAge = 12,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "birth_tokai" },
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
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_tokai_juku",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "塾に通った。机に向かう時間が増えた。",
                startAge = 9,
                endAge = 14,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "birth_tokai" },
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
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                        },
                    },
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_tokai_crowd",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "人混みの中で迷子になった。誰も助けてくれなかった。",
                startAge = 6,
                endAge = 10,
                baseWeight = 0.4f,
                requiresAnyLifeTag = new() { "birth_tokai" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "自力で家に帰り着いた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 田舎の家庭 固有イベント
        // ─────────────────────────────────────────
        #region 田舎
        private static IEnumerable<ReinLifeEvent> InakaBranch()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_inaka_river",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "川で魚を取って遊んだ。",
                startAge = 6,
                endAge = 12,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "birth_inaka" },
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
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.AT,  value = 1 },
                        },
                    },
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_inaka_mountain",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "山菜取りに連れて行ってもらった。獣道を歩けるようになった。",
                startAge = 7,
                endAge = 13,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "birth_inaka" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 2 },
                        },
                    },
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_inaka_yearning",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "テレビで都会の風景を見た。いつか行ってみたいと思った。",
                startAge = 12,
                endAge = 16,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "birth_inaka" },
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
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // セレブな家庭 固有イベント
        // ─────────────────────────────────────────
        #region セレブ
        private static IEnumerable<ReinLifeEvent> CelebBranch()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_celeb_tutor",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "家庭教師がついた。机の前にいる時間が長くなった。",
                startAge = 6,
                endAge = 12,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "birth_celeb" },
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
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_celeb_overseas",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "夏休みに海外へ連れて行ってもらった。世界が広いと知った。",
                startAge = 10,
                endAge = 16,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "birth_celeb" },
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
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_celeb_pressure",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "親の期待が重くのしかかった。逃げ場がなかった。",
                startAge = 13,
                endAge = 18,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "birth_celeb" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "応えようと努めた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                            new StatBonus { stat = StatKind.AT,  value = -1 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 貧しい家庭 固有イベント
        // ─────────────────────────────────────────
        #region 貧しい
        private static IEnumerable<ReinLifeEvent> PoorBranch()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_poor_chores",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "幼いころから家事を手伝った。手のひらに早くから皮ができた。",
                startAge = 6,
                endAge = 12,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "birth_poor" },
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
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 1 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 2 },
                        },
                    },
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_poor_sibling",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "弟妹の面倒をみるようになった。誰かを守ることに慣れた。",
                startAge = 8,
                endAge = 14,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "birth_poor" },
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
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                            new StatBonus { stat = StatKind.DF,  value = 1 },
                        },
                    },
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_poor_parttime",
                grantsLifeTags = new() { "birth_extra_done" },
                blockedByLifeTags = new() { "birth_extra_done" },
                sentence = "中学に上がるとアルバイトを始めた。働いた金額の重みを知った。",
                startAge = 13,
                endAge = 17,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "birth_poor" },
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
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                        },
                    },
                },
            };
        }
        #endregion
    }
}
