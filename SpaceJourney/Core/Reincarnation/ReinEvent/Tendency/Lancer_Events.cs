using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // Lancer 傾向の生業イベント
    // ================================================================
    // 漁師 / 取り立て屋 / 配管工 / 掘削技師 / 起業家 / ロケットエンジニア
    //
    // ※ このファイルは MigrateReinEventsToCs ツールで自動生成されました。
    //    手動編集してOKです。Claudeと相談しながら追加・変更する想定。
    // ================================================================
    public static class Lancer_Events
    {
        public static IEnumerable<ReinLifeEvent> All()
        {
            foreach (var ev in Ryoshi()) yield return ev;
            foreach (var ev in Tatekiya()) yield return ev;
            foreach (var ev in Haikan()) yield return ev;
            foreach (var ev in Kussaku()) yield return ev;
            foreach (var ev in Kigyoka()) yield return ev;
            foreach (var ev in Rocket()) yield return ev;
        }

        #region 漁師 (lancer_ryoshi)
        private static IEnumerable<ReinLifeEvent> Ryoshi()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_01",
                sentence = "祖父の船に乗った。港を出ると陸が見えなくなった。祖父は何も言わず、前を見ていた。",
                startAge = 8,
                endAge = 14,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "ryo_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_ryo_01g" },
                relatedJobIds = new() { "lancer_ryoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ryo_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_01g",
                editorMemo = "原体験保証",
                sentence = "祖父の船に乗った。港を出ると陸が見えなくなった。祖父は前を向いたまま何も言わなかった。",
                startAge = 14,
                endAge = 14,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "ryo_call" },
                blockedByEventIds = new() { "ev_ryo_01" },
                relatedJobIds = new() { "lancer_ryoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ryo_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_02",
                sentence = "漁船に乗り込み、仕事を覚え始めた。網の扱い、潮の読み方、エンジンの整備を体で覚えた。",
                startAge = 15,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "ryo_call" },
                grantsLifeTags = new() { "ryo_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "体が仕事に慣れるのが早かった。ベテランに「筋がいい」と言われた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 30, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 28, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "船酔いが続いた。3ヶ月で止まった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_03",
                sentence = "小型船舶操縦士の免許試験を受けた。",
                startAge = 18,
                endAge = 22,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "ryo_train" },
                blockedByLifeTags = new() { "ryo_license" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "一発合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "ryo_license" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "学科で落ちた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "ryo_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_03b",
                sentence = "再受験した。",
                startAge = 20,
                endAge = 23,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "ryo_fail" },
                blockedByLifeTags = new() { "ryo_license" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "ryo_license" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_03c",
                sentence = "免許試験に再挑戦した。ついに合格した。",
                startAge = 24,
                endAge = 24,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "ryo_fail" },
                blockedByLifeTags = new() { "ryo_license" },
                relatedJobIds = new() { "lancer_ryoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ryo_license" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_04",
                sentence = "嵐の中で網を引いた。波が甲板を打ちつけた。",
                startAge = 22,
                endAge = 25,
                baseWeight = 0.65f,
                requiresAnyLifeTag = new() { "ryo_license" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "引き続けた。港に戻ったとき、船長が何も言わずに頷いた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 38, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 33, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                            new StatBonus { stat = StatKind.DF, value = 2 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "途中で網を手放したが、帰れた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_05",
                sentence = "生業が漁師になった。",
                startAge = 22,
                endAge = 24,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "ryo_license" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                grantsLifeTags = new() { "ryo_flav_routine" },
                blockedByLifeTags = new() { "ryo_flav_routine" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_ryoshi" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_ryo_ep1",
                sentence = "海が荒れた。それでも出た。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_ryoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                grantsLifeTags = new() { "ryo_flav_episode" },
                blockedByLifeTags = new() { "ryo_flav_episode" },
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
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_ryo_ep2",
                sentence = "古い漁師仲間が引退すると言った。飯を食った。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_ryoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                grantsLifeTags = new() { "ryo_flav_episode" },
                blockedByLifeTags = new() { "ryo_flav_episode" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_ryo_ep3",
                sentence = "水揚げが続けて悪かった。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_ryoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "lancer_ryoshi" },
                grantsLifeTags = new() { "ryo_flav_episode" },
                blockedByLifeTags = new() { "ryo_flav_episode" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_s1",
                sentence = "波の読みが体に染みついてきた。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_ryoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                grantsLifeTags = new() { "ryo_flav_serious" },
                blockedByLifeTags = new() { "ryo_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_s2",
                sentence = "体が思い通りに動かない日があった。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_ryoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                grantsLifeTags = new() { "ryo_flav_serious" },
                blockedByLifeTags = new() { "ryo_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_s3",
                sentence = "海の上でだけ、余計なことを考えなかった。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_ryoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                grantsLifeTags = new() { "ryo_flav_serious" },
                blockedByLifeTags = new() { "ryo_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_r1a",
                sentence = "自分の船を持った。海に出る度に、重みが違った。",
                startAge = 23,
                endAge = 53,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_ryoshi" },
                blockedByLifeTags = new() { "ryo_r1a" },
                grantsLifeTags = new() { "ryo_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_r1b",
                sentence = "豊漁の時期を読み切った。",
                startAge = 23,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "ryo_r1a" },
                blockedByLifeTags = new() { "ryo_r1b" },
                grantsLifeTags = new() { "ryo_r1b" },
                relatedJobIds = new() { "lancer_ryoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "港で最高の水揚げを記録した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "読みが外れた。それでも次の海に出た。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_r3a",
                sentence = "漁協の役員を任された。",
                startAge = 23,
                endAge = 53,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "ryo_r1b" },
                blockedByLifeTags = new() { "ryo_r3a" },
                grantsLifeTags = new() { "ryo_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_r3b",
                sentence = "若い漁師たちの相談相手になった。",
                startAge = 23,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "ryo_r3a" },
                blockedByLifeTags = new() { "ryo_r3b" },
                grantsLifeTags = new() { "ryo_r3b" },
                relatedJobIds = new() { "lancer_ryoshi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_r5a",
                sentence = "漁協の組合長への打診が来た。",
                startAge = 23,
                endAge = 53,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ryo_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "引き受けた。海全体のことを考えるようになった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "ryo_r5a_kumi" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "断った。自分は海に出る人間だと思った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "ryo_r5a_umi" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_r5b_a",
                sentence = "組合長として、漁師たちの生業を守る仕事に就いた。",
                startAge = 23,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ryo_r5a_kumi" },
                blockedByLifeTags = new() { "ryo_r5b" },
                grantsLifeTags = new() { "ryo_r5b" },
                relatedJobIds = new() { "lancer_ryoshi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_r5b_b",
                sentence = "最後まで漁師として海に出続けた。",
                startAge = 23,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ryo_r5a_umi" },
                blockedByLifeTags = new() { "ryo_r5b" },
                grantsLifeTags = new() { "ryo_r5b" },
                relatedJobIds = new() { "lancer_ryoshi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_r7a",
                sentence = "体が言うことを聞かない日が増えてきた。それでも出た。",
                startAge = 23,
                endAge = 53,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "ryo_r5b" },
                blockedByLifeTags = new() { "ryo_r7a" },
                grantsLifeTags = new() { "ryo_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_ryoshi" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ryo_r7b",
                sentence = "最後の出港の日、港を振り返った。",
                startAge = 23,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "ryo_r7a" },
                blockedByLifeTags = new() { "ryo_r7b" },
                grantsLifeTags = new() { "ryo_r7b" },
                relatedJobIds = new() { "lancer_ryoshi" },
            };
        }
        #endregion

        #region 取り立て屋 (lancer_tatekiya)
        private static IEnumerable<ReinLifeEvent> Tatekiya()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_01",
                sentence = "父の借金の影響で、友人も仕事も、すべて失った。金のある者とない者では、人の扱いがまるで違った。",
                startAge = 12,
                endAge = 17,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "coll_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_coll_01g" },
                relatedJobIds = new() { "lancer_tatekiya" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "coll_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_01g",
                editorMemo = "原体験保証",
                sentence = "父の借金の影響で、友人も仕事も、すべて失った。金のある者とない者では、人の扱いがまるで違った。",
                startAge = 17,
                endAge = 17,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "coll_call" },
                blockedByEventIds = new() { "ev_coll_01" },
                relatedJobIds = new() { "lancer_tatekiya" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "coll_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_02",
                sentence = "消費者金融の債権回収部門に就職した。電話対応から訪問まで、取り立て方を体で覚えた。",
                startAge = 20,
                endAge = 24,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "coll_call" },
                grantsLifeTags = new() { "coll_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "圧力のかけ方が上手いと上司に言われた。成績は上位に入った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "最初は断られ続けたが、通い続ければ払うと分かった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_03",
                sentence = "合法スレスレで動く術を、身につけていった。",
                startAge = 24,
                endAge = 28,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "coll_train" },
                grantsLifeTags = new() { "coll_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "最初の依頼で全額回収した。口コミで次が来た。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 40, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "最初は小さな案件だった。でも仕事は途切れなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_04",
                sentence = "生業が取り立て屋になった。",
                startAge = 27,
                endAge = 29,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "coll_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_tatekiya" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_04a",
                sentence = "取り立て屋として依頼を受けるようになった。",
                startAge = 28,
                endAge = 60,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "job_tatekiya" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                eventStage = 1,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_coll_ep1",
                sentence = "相手が開口一番に言い訳を並べた。静かに聞いた。",
                startAge = 28,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_tatekiya" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                grantsLifeTags = new() { "coll_flav_episode" },
                blockedByLifeTags = new() { "coll_flav_episode" },
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
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_coll_ep2",
                sentence = "粘り強く通い続けたら、ある日払ってきた。",
                startAge = 28,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_tatekiya" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                grantsLifeTags = new() { "coll_flav_episode" },
                blockedByLifeTags = new() { "coll_flav_episode" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_coll_ep3",
                sentence = "厄介な案件が続いた。消耗した。",
                startAge = 28,
                endAge = 60,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_tatekiya" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "lancer_tatekiya" },
                grantsLifeTags = new() { "coll_flav_episode" },
                blockedByLifeTags = new() { "coll_flav_episode" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_s1",
                sentence = "圧力の加減が体で分かるようになった。",
                startAge = 28,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_tatekiya" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                grantsLifeTags = new() { "coll_flav_serious" },
                blockedByLifeTags = new() { "coll_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_s2",
                sentence = "相手の本音が見えなかった。読み間違えた。",
                startAge = 28,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_tatekiya" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                grantsLifeTags = new() { "coll_flav_serious" },
                blockedByLifeTags = new() { "coll_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_s3",
                sentence = "この仕事をいつまで続けるのか、ふと考えた。",
                startAge = 28,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_tatekiya" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                grantsLifeTags = new() { "coll_flav_serious" },
                blockedByLifeTags = new() { "coll_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_r1a",
                sentence = "大口案件を単独で担当した。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_tatekiya" },
                blockedByLifeTags = new() { "coll_r1a" },
                grantsLifeTags = new() { "coll_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_r1b",
                sentence = "交渉が難航したが、最終的に全額回収した。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "coll_r1a" },
                blockedByLifeTags = new() { "coll_r1b" },
                grantsLifeTags = new() { "coll_r1b" },
                relatedJobIds = new() { "lancer_tatekiya" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "粘り強く交渉した末、相手が折れた。強引なやり方はしなかった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "時間はかかった。それでも回収した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_r3a",
                sentence = "業界内で名が通るようになった。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "coll_r1b" },
                blockedByLifeTags = new() { "coll_r3a" },
                grantsLifeTags = new() { "coll_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_r3b",
                sentence = "法的措置まで視野に入れた複雑な案件を解決した。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "coll_r3a" },
                blockedByLifeTags = new() { "coll_r3b" },
                grantsLifeTags = new() { "coll_r3b" },
                relatedJobIds = new() { "lancer_tatekiya" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_r5a",
                sentence = "事務所を構えるか、フリーを続けるか。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "coll_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "事務所を構えた。仕事が安定した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "coll_r5a_jm" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "フリーを続けた。身軽な方が好きだった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "coll_r5a_free" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_r5b_a",
                sentence = "事務所を育て、後輩を育てた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "coll_r5a_jm" },
                blockedByLifeTags = new() { "coll_r5b" },
                grantsLifeTags = new() { "coll_r5b" },
                relatedJobIds = new() { "lancer_tatekiya" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_r5b_b",
                sentence = "フリーとして、自分のやり方を貫いた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "coll_r5a_free" },
                blockedByLifeTags = new() { "coll_r5b" },
                grantsLifeTags = new() { "coll_r5b" },
                relatedJobIds = new() { "lancer_tatekiya" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_r7a",
                sentence = "長年やってきたことが、自分の顔になっていた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "coll_r5b" },
                blockedByLifeTags = new() { "coll_r7a" },
                grantsLifeTags = new() { "coll_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_tatekiya" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_coll_r7b",
                sentence = "最後の案件を終えた日、何も残っていなかった。それでいい。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "coll_r7a" },
                blockedByLifeTags = new() { "coll_r7b" },
                grantsLifeTags = new() { "coll_r7b" },
                relatedJobIds = new() { "lancer_tatekiya" },
            };
        }
        #endregion

        #region 配管工 (lancer_haikan)
        private static IEnumerable<ReinLifeEvent> Haikan()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_01",
                sentence = "台所の蛇口が壊れた。職人さんが見事な手際で修理してくれた。",
                startAge = 8,
                endAge = 14,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "haikan_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_haikan_01g" },
                relatedJobIds = new() { "lancer_haikan" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "haikan_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_01g",
                editorMemo = "原体験保証",
                sentence = "台所の蛇口が壊れた。職人さんが見事な手際で修理してくれた。",
                startAge = 14,
                endAge = 14,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "haikan_call" },
                blockedByEventIds = new() { "ev_haikan_01" },
                relatedJobIds = new() { "lancer_haikan" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "haikan_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_02",
                sentence = "配管工の会社に就職した。最初は資材の搬入や掃除から始まった。",
                startAge = 18,
                endAge = 21,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "haikan_call" },
                grantsLifeTags = new() { "haikan_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "手先が器用だと言われた。早めに現場作業を任された。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 28, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 28, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "先輩の後ろで見ているだけの時間が長かったが、それでも覚えていった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_04",
                sentence = "生業が配管工になった。",
                startAge = 23,
                endAge = 30,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "haikan_cert" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_haikan" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_03",
                sentence = "管工事施工管理技士の資格試験を受けた。",
                startAge = 21,
                endAge = 25,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "haikan_train" },
                blockedByLifeTags = new() { "haikan_cert" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 30, weightBonus = 1.0f },
                            new StatCondition { stat = StatKind.MAT, threshold = 25, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "haikan_cert" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "不合格だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "haikan_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_03b",
                sentence = "再受験した。",
                startAge = 23,
                endAge = 26,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "haikan_fail" },
                blockedByLifeTags = new() { "haikan_cert" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "haikan_cert" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_03c",
                sentence = "資格試験に再挑戦した。ついに合格した。",
                startAge = 27,
                endAge = 27,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "haikan_fail" },
                blockedByLifeTags = new() { "haikan_cert" },
                relatedJobIds = new() { "lancer_haikan" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "haikan_cert" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_haikan_ep1",
                sentence = "図面通りにいかない場所があった。その場で解決した。",
                startAge = 25,
                endAge = 62,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_haikan" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                grantsLifeTags = new() { "haikan_flav_episode" },
                blockedByLifeTags = new() { "haikan_flav_episode" },
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
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_haikan_ep2",
                sentence = "完成した配管を確認したとき、音が静かだった。",
                startAge = 25,
                endAge = 62,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_haikan" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                grantsLifeTags = new() { "haikan_flav_episode" },
                blockedByLifeTags = new() { "haikan_flav_episode" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_haikan_ep3",
                sentence = "若い作業員のミスをカバーした。何も言わなかった。",
                startAge = 25,
                endAge = 62,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_haikan" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "lancer_haikan" },
                grantsLifeTags = new() { "haikan_flav_episode" },
                blockedByLifeTags = new() { "haikan_flav_episode" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_s1",
                sentence = "壁の中の配管の状態が、音だけで分かった。",
                startAge = 25,
                endAge = 62,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_haikan" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                grantsLifeTags = new() { "haikan_flav_serious" },
                blockedByLifeTags = new() { "haikan_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_s2",
                sentence = "細かい作業に時間がかかった。精度が足りなかった。",
                startAge = 25,
                endAge = 62,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_haikan" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                grantsLifeTags = new() { "haikan_flav_serious" },
                blockedByLifeTags = new() { "haikan_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_s3",
                sentence = "手を動かしているとき、他のことを考えなかった。",
                startAge = 25,
                endAge = 62,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_haikan" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                grantsLifeTags = new() { "haikan_flav_serious" },
                blockedByLifeTags = new() { "haikan_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_r1a",
                sentence = "大型物件の現場監督補佐を任された。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_haikan" },
                blockedByLifeTags = new() { "haikan_r1a" },
                grantsLifeTags = new() { "haikan_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_r1b",
                sentence = "施工図の問題を見つけ、設計変更を提案した。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "haikan_r1a" },
                blockedByLifeTags = new() { "haikan_r1b" },
                grantsLifeTags = new() { "haikan_r1b" },
                relatedJobIds = new() { "lancer_haikan" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "採用された。現場がスムーズになった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "却下された。それでも正しいと思った手順で進めた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_r3a",
                sentence = "一級管工事施工管理技士の試験に合格した。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "haikan_r1b" },
                blockedByLifeTags = new() { "haikan_r3a" },
                grantsLifeTags = new() { "haikan_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_r3b",
                sentence = "大型公共工事の配管設計を任された。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "haikan_r3a" },
                blockedByLifeTags = new() { "haikan_r3b" },
                grantsLifeTags = new() { "haikan_r3b" },
                relatedJobIds = new() { "lancer_haikan" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_r5a",
                sentence = "独立して自分の会社を作るか、職人として働き続けるか。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "haikan_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "独立した。自分のやり方でできるようになった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "haikan_r5a_co" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "職人として残った。手を動かすことが好きだった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "haikan_r5a_stay" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_r5b_a",
                sentence = "会社を育て、自分のやり方を形にした。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "haikan_r5a_co" },
                blockedByLifeTags = new() { "haikan_r5b" },
                grantsLifeTags = new() { "haikan_r5b" },
                relatedJobIds = new() { "lancer_haikan" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_r5b_b",
                sentence = "職人として、技術を磨き続けた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "haikan_r5a_stay" },
                blockedByLifeTags = new() { "haikan_r5b" },
                grantsLifeTags = new() { "haikan_r5b" },
                relatedJobIds = new() { "lancer_haikan" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_r7a",
                sentence = "弟子が現場監督として独り立ちした。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "haikan_r5b" },
                blockedByLifeTags = new() { "haikan_r7a" },
                grantsLifeTags = new() { "haikan_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_haikan" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_haikan_r7b",
                sentence = "最後に手がけた現場の完工式に呼んでもらった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "haikan_r7a" },
                blockedByLifeTags = new() { "haikan_r7b" },
                grantsLifeTags = new() { "haikan_r7b" },
                relatedJobIds = new() { "lancer_haikan" },
            };
        }
        #endregion

        #region 掘削技師 (lancer_kussaku)
        private static IEnumerable<ReinLifeEvent> Kussaku()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_01",
                sentence = "掘削機が地面を砕くのを見た。振動が足の裏まで伝わり、見えなくなるまで立っていた。",
                startAge = 8,
                endAge = 14,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "kuss_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_kuss_01g" },
                relatedJobIds = new() { "lancer_kussaku" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "kuss_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_01g",
                editorMemo = "原体験保証",
                sentence = "掘削機が地面を砕くのを見た。振動が足の裏まで来た。見えなくなるまで立っていた。",
                startAge = 14,
                endAge = 14,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "kuss_call" },
                blockedByEventIds = new() { "ev_kuss_01" },
                relatedJobIds = new() { "lancer_kussaku" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "kuss_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_02",
                sentence = "土木系の専門学校に進み、大型機械の操作や地盤工学を学んだ。",
                startAge = 18,
                endAge = 21,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "kuss_call" },
                grantsLifeTags = new() { "kuss_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "実技の評価が高かった。重機の扱いが体に馴染んだ。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "学科は難しかったが、それでも全科目を通過した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_03",
                sentence = "土木施工管理技士および掘削系の資格試験を受けた。",
                startAge = 21,
                endAge = 25,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "kuss_train" },
                blockedByLifeTags = new() { "kuss_cert" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格し、大手ゼネコンから採用通知が届いた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 35, weightBonus = 1.0f },
                            new StatCondition { stat = StatKind.MAT, threshold = 30, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "kuss_cert" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "不合格だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "kuss_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_03b",
                sentence = "再受験した。",
                startAge = 23,
                endAge = 26,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "kuss_fail" },
                blockedByLifeTags = new() { "kuss_cert" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "kuss_cert" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "また落ちた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "kuss_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_03c",
                sentence = "資格試験に再挑戦した。ついに合格した。",
                startAge = 27,
                endAge = 27,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "kuss_fail" },
                blockedByLifeTags = new() { "kuss_cert" },
                relatedJobIds = new() { "lancer_kussaku" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "kuss_cert" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_04",
                sentence = "大規模トンネル掘削現場に配属された。地下数十メートルで毎日働いた。",
                startAge = 24,
                endAge = 27,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "kuss_cert" },
                grantsLifeTags = new() { "kuss_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "機械の癖と現場の流れを掴むのが早かった。先輩に「筋がいい」と言われた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 42, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "ひたすら掘り続けた。それ以上のことは何も考えなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_06",
                sentence = "生業が掘削技師になった。",
                startAge = 26,
                endAge = 29,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "kuss_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_kussaku" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_05a",
                sentence = "先輩の現場監督の指示のもと、初めて掘削ラインの設定を担当した。",
                startAge = 27,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kussaku" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                grantsLifeTags = new() { "kuss_flav_routine" },
                blockedByLifeTags = new() { "kuss_flav_routine" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "計算通りに進んだ。現場監督に「任せられる」と言われた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 38, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 30, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "途中で修正が入った。先輩の判断で乗り切った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_06a",
                sentence = "掘削技師として現場を指揮するようになった。",
                startAge = 27,
                endAge = 60,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "job_kussaku" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                eventStage = 1,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_kuss_ep1",
                sentence = "地盤が予想と違った。対処策を即座に考えた。",
                startAge = 27,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kussaku" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                grantsLifeTags = new() { "kuss_flav_episode" },
                blockedByLifeTags = new() { "kuss_flav_episode" },
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
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_kuss_ep2",
                sentence = "機械が止まった。自分で原因を特定した。",
                startAge = 27,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kussaku" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                grantsLifeTags = new() { "kuss_flav_episode" },
                blockedByLifeTags = new() { "kuss_flav_episode" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_kuss_ep3",
                sentence = "工期が遅れ始めた。プレッシャーが続いた。",
                startAge = 27,
                endAge = 60,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_kussaku" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "lancer_kussaku" },
                grantsLifeTags = new() { "kuss_flav_episode" },
                blockedByLifeTags = new() { "kuss_flav_episode" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_s1",
                sentence = "機械の振動から、地盤の状態が読めるようになった。",
                startAge = 27,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kussaku" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                grantsLifeTags = new() { "kuss_flav_serious" },
                blockedByLifeTags = new() { "kuss_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_s2",
                sentence = "精密な計算が必要な場面で、焦りが出た。",
                startAge = 27,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kussaku" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                grantsLifeTags = new() { "kuss_flav_serious" },
                blockedByLifeTags = new() { "kuss_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_s3",
                sentence = "深い地下で作業しているとき、不思議な集中があった。",
                startAge = 27,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kussaku" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                grantsLifeTags = new() { "kuss_flav_serious" },
                blockedByLifeTags = new() { "kuss_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_r1a",
                sentence = "プロジェクトリーダーを任された。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_kussaku" },
                blockedByLifeTags = new() { "kuss_r1a" },
                grantsLifeTags = new() { "kuss_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_r1b",
                sentence = "想定外の地盤に対応した。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "kuss_r1a" },
                blockedByLifeTags = new() { "kuss_r1b" },
                grantsLifeTags = new() { "kuss_r1b" },
                relatedJobIds = new() { "lancer_kussaku" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "的確に修正指示を出した。工期は守れた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "遅延が出た。それでも安全を優先した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_r3a",
                sentence = "海外プロジェクトへの参加が決まった。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "kuss_r1b" },
                blockedByLifeTags = new() { "kuss_r3a" },
                grantsLifeTags = new() { "kuss_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_r3b",
                sentence = "業界誌に技術論文が掲載された。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "kuss_r3a" },
                blockedByLifeTags = new() { "kuss_r3b" },
                grantsLifeTags = new() { "kuss_r3b" },
                relatedJobIds = new() { "lancer_kussaku" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_r5a",
                sentence = "技術コンサルタントとして独立するか、現場を続けるか。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "kuss_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "独立した。世界中の現場に呼ばれるようになった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "kuss_r5a_con" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "現場に残った。機械の横に立ち続けることが自分の仕事だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "kuss_r5a_field" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_r5b_a",
                sentence = "世界各地の掘削現場に呼ばれ、知見を広めた。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "kuss_r5a_con" },
                blockedByLifeTags = new() { "kuss_r5b" },
                grantsLifeTags = new() { "kuss_r5b" },
                relatedJobIds = new() { "lancer_kussaku" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_r5b_b",
                sentence = "現場の職人として、技術を磨き続けた。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "kuss_r5a_field" },
                blockedByLifeTags = new() { "kuss_r5b" },
                grantsLifeTags = new() { "kuss_r5b" },
                relatedJobIds = new() { "lancer_kussaku" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_r7a",
                sentence = "引退後の講演依頼が来た。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "kuss_r5b" },
                blockedByLifeTags = new() { "kuss_r7a" },
                grantsLifeTags = new() { "kuss_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kussaku" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kuss_r7b",
                sentence = "最後の現場を終えた日、地面を叩いた。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "kuss_r7a" },
                blockedByLifeTags = new() { "kuss_r7b" },
                grantsLifeTags = new() { "kuss_r7b" },
                relatedJobIds = new() { "lancer_kussaku" },
            };
        }
        #endregion

        #region 起業家 (lancer_kigyoka)
        private static IEnumerable<ReinLifeEvent> Kigyoka()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_01",
                sentence = "文化祭のたこ焼き屋が大繁盛した。閉店後、売上を数えながら、来年の企画を考えていた。",
                startAge = 10,
                endAge = 15,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "ent_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_ent_01g" },
                relatedJobIds = new() { "lancer_kigyoka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ent_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_01g",
                editorMemo = "原体験保証",
                sentence = "文化祭のたこ焼き屋が大繁盛した。閉店後、売上を数えながら、来年の企画を考えていた。",
                startAge = 15,
                endAge = 15,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "ent_call" },
                blockedByEventIds = new() { "ev_ent_01" },
                relatedJobIds = new() { "lancer_kigyoka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ent_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_02",
                sentence = "高校を出てすぐ、最初の会社を起こし、友人を巻き込んで夢中でやった。",
                startAge = 18,
                endAge = 22,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "ent_call" },
                grantsLifeTags = new() { "ent_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "最初の3ヶ月で売上が立った。手応えがあった。",
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
                    new ReinSentenceOption
                    {
                        sentence = "なかなかうまくことは運ばなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_03",
                sentence = "会社が1年で倒産した。借金と罪悪感とともに教訓を得た。",
                startAge = 20,
                endAge = 24,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "ent_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "失敗を全部書き出したら、ノート3冊になった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "ent_learn" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "しばらく何もできなかった。でも動かないでいられなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "ent_learn" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_04",
                sentence = "投資家へのピッチを繰り返し、何十回も断られた。",
                startAge = 23,
                endAge = 27,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "ent_learn" },
                blockedByLifeTags = new() { "ent_fund" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "一人の投資家が頷いた。翌週、契約書が届いた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 45, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.MDF, threshold = 40, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "ent_fund" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "また断られた。ピッチを磨いてまた挑んだ。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "ent_nofund" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_04b",
                sentence = "資金調達を再挑戦した。",
                startAge = 26,
                endAge = 29,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "ent_nofund" },
                blockedByLifeTags = new() { "ent_fund" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "ついに投資家の了解を得た。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "ent_fund" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "また断られた。自己資金で動くことにした。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "ent_nofund" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_04c",
                sentence = "何らかの形で資金を確保し、事業を動かした。",
                startAge = 30,
                endAge = 30,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "ent_nofund" },
                blockedByLifeTags = new() { "ent_fund" },
                relatedJobIds = new() { "lancer_kigyoka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ent_fund" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_05",
                sentence = "生業が起業家になった。",
                startAge = 28,
                endAge = 31,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "ent_fund" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                grantsLifeTags = new() { "ent_flav_routine" },
                blockedByLifeTags = new() { "ent_flav_routine" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_kigyoka" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_ent_ep1",
                sentence = "採用した社員が初めて大きな成果を出した。",
                startAge = 29,
                endAge = 70,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kigyoka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                grantsLifeTags = new() { "ent_flav_episode" },
                blockedByLifeTags = new() { "ent_flav_episode" },
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
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_ent_ep2",
                sentence = "競合が現れた。それが自分の正しさを証明した。",
                startAge = 29,
                endAge = 70,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kigyoka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                grantsLifeTags = new() { "ent_flav_episode" },
                blockedByLifeTags = new() { "ent_flav_episode" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_ent_ep3",
                sentence = "資金繰りに追われる時期があった。",
                startAge = 29,
                endAge = 70,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_kigyoka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "lancer_kigyoka" },
                grantsLifeTags = new() { "ent_flav_episode" },
                blockedByLifeTags = new() { "ent_flav_episode" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_s1",
                sentence = "課題を数字で見たとき、解決策が見えた。",
                startAge = 29,
                endAge = 70,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kigyoka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                grantsLifeTags = new() { "ent_flav_serious" },
                blockedByLifeTags = new() { "ent_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_s2",
                sentence = "市場の読みが外れた。仮説を立て直した。",
                startAge = 29,
                endAge = 70,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kigyoka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                grantsLifeTags = new() { "ent_flav_serious" },
                blockedByLifeTags = new() { "ent_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_s3",
                sentence = "誰もやっていないことをやるとき、迷いがなかった。",
                startAge = 29,
                endAge = 70,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kigyoka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                grantsLifeTags = new() { "ent_flav_serious" },
                blockedByLifeTags = new() { "ent_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_r1a",
                sentence = "会社が初めて黒字になった。",
                startAge = 29,
                endAge = 59,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_kigyoka" },
                blockedByLifeTags = new() { "ent_r1a" },
                grantsLifeTags = new() { "ent_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_r1b",
                sentence = "投資家からの評価が上がり、次のラウンドが決まった。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "ent_r1a" },
                blockedByLifeTags = new() { "ent_r1b" },
                grantsLifeTags = new() { "ent_r1b" },
                relatedJobIds = new() { "lancer_kigyoka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "大型調達に成功した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "規模より実質を優先した調達になった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_r3a",
                sentence = "業界誌の表紙を飾った。",
                startAge = 29,
                endAge = 59,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "ent_r1b" },
                blockedByLifeTags = new() { "ent_r3a" },
                grantsLifeTags = new() { "ent_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_r3b",
                sentence = "競合との差別化が明確になった。市場での地位が固まった。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "ent_r3a" },
                blockedByLifeTags = new() { "ent_r3b" },
                grantsLifeTags = new() { "ent_r3b" },
                relatedJobIds = new() { "lancer_kigyoka" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_r5a",
                sentence = "IPOを目指すか、買収オファーを受けるか。",
                startAge = 29,
                endAge = 59,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ent_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "IPOを選んだ。上場の日、創業時のことを思い出した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "ent_r5a_ipo" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "買収を受けた。その資金で次のことを始めた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "ent_r5a_buy" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_r5b_a",
                sentence = "上場企業の経営者として、新しいステージに立った。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ent_r5a_ipo" },
                blockedByLifeTags = new() { "ent_r5b" },
                grantsLifeTags = new() { "ent_r5b" },
                relatedJobIds = new() { "lancer_kigyoka" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_r5b_b",
                sentence = "資金を手に、また新しいことを始めた。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ent_r5a_buy" },
                blockedByLifeTags = new() { "ent_r5b" },
                grantsLifeTags = new() { "ent_r5b" },
                relatedJobIds = new() { "lancer_kigyoka" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_r7a",
                sentence = "起業家として語られることが増えた。自分では実感がなかった。",
                startAge = 29,
                endAge = 59,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "ent_r5b" },
                blockedByLifeTags = new() { "ent_r7a" },
                grantsLifeTags = new() { "ent_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_kigyoka" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ent_r7b",
                sentence = "また新しいことを始めた。それが自分の生き方だった。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "ent_r7a" },
                blockedByLifeTags = new() { "ent_r7b" },
                grantsLifeTags = new() { "ent_r7b" },
                relatedJobIds = new() { "lancer_kigyoka" },
            };
        }
        #endregion

        #region ロケットエンジニア (lancer_rocket)
        private static IEnumerable<ReinLifeEvent> Rocket()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_01",
                sentence = "ロケットの打ち上げ中継を見た。音より先に光が届いた。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.08f,
                blockedByLifeTags = new() { "rocket_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_rocket_01g" },
                relatedJobIds = new() { "lancer_rocket" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "rocket_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_01g",
                editorMemo = "原体験保証",
                sentence = "ロケットの打ち上げ中継を見た。音より先に光が来た。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "rocket_call" },
                blockedByEventIds = new() { "ev_rocket_01" },
                relatedJobIds = new() { "lancer_rocket" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "rocket_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_02",
                sentence = "物理と数学に集中し、推進力の計算にのめり込んだ。",
                startAge = 15,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "rocket_call" },
                grantsLifeTags = new() { "rocket_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "全国模試で上位に入った。担任が国立理工学部を勧めた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 40, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 30, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "成績は良かった。それで足りるかどうかは分からなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_03",
                sentence = "宇宙工学または航空工学の大学・大学院に進み、推進系の研究室に入った。",
                startAge = 18,
                endAge = 24,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "rocket_train" },
                grantsLifeTags = new() { "rocket_phd" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "修士論文が学会で発表された。JAXA関連の研究者と話す機会を得た。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 52, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 38, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "博士号を取った。それだけで精一杯だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "修士で出た。業界に入ってから追いついた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_04",
                sentence = "宇宙機関や民間宇宙企業の採用試験を受けた。競争率は高かった。",
                startAge = 24,
                endAge = 30,
                baseWeight = 0.65f,
                requiresAnyLifeTag = new() { "rocket_phd" },
                blockedByLifeTags = new() { "rocket_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "採用された。最初の配属はエンジン設計チームだった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 55, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.AGI, threshold = 40, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "rocket_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "落ちた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "rocket_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_04b",
                sentence = "別の機関に再挑戦した。",
                startAge = 27,
                endAge = 32,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "rocket_fail" },
                blockedByLifeTags = new() { "rocket_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "採用された。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 48, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.AGI, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "rocket_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "また落ちた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "rocket_fail" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_04c",
                sentence = "採用試験に再挑戦した。ついに採用された。",
                startAge = 33,
                endAge = 33,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "rocket_fail" },
                blockedByLifeTags = new() { "rocket_pass" },
                relatedJobIds = new() { "lancer_rocket" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "rocket_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_06",
                sentence = "生業がロケットエンジニアになった。",
                startAge = 26,
                endAge = 36,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "rocket_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_rocket" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_05a",
                sentence = "担当した機体が打ち上げ失敗した。海に落ちた。",
                startAge = 28,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_rocket" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                grantsLifeTags = new() { "rocket_flav_routine" },
                blockedByLifeTags = new() { "rocket_flav_routine" },
                eventStage = -1,
                statCompareCount = 3,
                statCompareMode = "avg",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "データを全部解析して原因を特定し、次の設計に叩き込んだ。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 58, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 45, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 10 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
                            new StatBonus { stat = StatKind.MAT, value = 6 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "原因究明に半年かかった。その間、何もできなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_05b",
                sentence = "打ち上げ成功の瞬間、管制室全員が立ち上がった。",
                startAge = 28,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_rocket" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                grantsLifeTags = new() { "rocket_flav_routine" },
                blockedByLifeTags = new() { "rocket_flav_routine" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "次のミッションの計画書がすでに机の上にあった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 55, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 42, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "しばらく声が出なかった。隣の同僚が無言で労ってくれた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_rocket_ep1",
                sentence = "シミュレーションが現実と一致した。手が震えた。",
                startAge = 28,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_rocket" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                grantsLifeTags = new() { "rocket_flav_episode" },
                blockedByLifeTags = new() { "rocket_flav_episode" },
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
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_rocket_ep2",
                sentence = "同僚の設計に問題を見つけた。一緒に修正した。",
                startAge = 28,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_rocket" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                grantsLifeTags = new() { "rocket_flav_episode" },
                blockedByLifeTags = new() { "rocket_flav_episode" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_rocket_ep3",
                sentence = "打ち上げが延期になった。原因究明に追われた。",
                startAge = 28,
                endAge = 65,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_rocket" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "lancer_rocket" },
                grantsLifeTags = new() { "rocket_flav_episode" },
                blockedByLifeTags = new() { "rocket_flav_episode" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_s1",
                sentence = "計算が現実と一致するとき、宇宙が近く感じた。",
                startAge = 28,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_rocket" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                grantsLifeTags = new() { "rocket_flav_serious" },
                blockedByLifeTags = new() { "rocket_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_s2",
                sentence = "理論は正しいが、現実に合わない場面があった。",
                startAge = 28,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_rocket" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                grantsLifeTags = new() { "rocket_flav_serious" },
                blockedByLifeTags = new() { "rocket_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_s3",
                sentence = "打ち上げを見るたびに、まだやれると思った。",
                startAge = 28,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_rocket" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                grantsLifeTags = new() { "rocket_flav_serious" },
                blockedByLifeTags = new() { "rocket_flav_serious" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_r1a",
                sentence = "自分の設計が初めて実際の機体に採用された。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_rocket" },
                blockedByLifeTags = new() { "rocket_r1a" },
                grantsLifeTags = new() { "rocket_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_r1b",
                sentence = "打ち上げ当日、管制室で見守った。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "rocket_r1a" },
                blockedByLifeTags = new() { "rocket_r1b" },
                grantsLifeTags = new() { "rocket_r1b" },
                relatedJobIds = new() { "lancer_rocket" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "成功した。画面の前で立ち上がれなかった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "部分的な問題が出た。原因はすぐに分かった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_r3a",
                sentence = "プロジェクトリーダーとして機体開発を率いた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "rocket_r1b" },
                blockedByLifeTags = new() { "rocket_r3a" },
                grantsLifeTags = new() { "rocket_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_r3b",
                sentence = "世界的に注目された打ち上げを成功させた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "rocket_r3a" },
                blockedByLifeTags = new() { "rocket_r3b" },
                grantsLifeTags = new() { "rocket_r3b" },
                relatedJobIds = new() { "lancer_rocket" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_r5a",
                sentence = "研究機関に転じて次世代技術を追うか、開発の現場に留まるか。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "rocket_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "研究に移った。まだ誰も解いていない問題がそこにあった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "rocket_r5a_res" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "開発に残った。飛ばすことが自分の仕事だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "rocket_r5a_dev" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_r5b_a",
                sentence = "次世代の推進技術の研究に没頭した。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "rocket_r5a_res" },
                blockedByLifeTags = new() { "rocket_r5b" },
                grantsLifeTags = new() { "rocket_r5b" },
                relatedJobIds = new() { "lancer_rocket" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_r5b_b",
                sentence = "機体開発の最前線で、打ち上げを積み重ねた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "rocket_r5a_dev" },
                blockedByLifeTags = new() { "rocket_r5b" },
                grantsLifeTags = new() { "rocket_r5b" },
                relatedJobIds = new() { "lancer_rocket" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_r7a",
                sentence = "自分が関わった機体の数を数えたことがなかった。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "rocket_r5b" },
                blockedByLifeTags = new() { "rocket_r7a" },
                grantsLifeTags = new() { "rocket_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "lancer_rocket" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_rocket_r7b",
                sentence = "引退後も、打ち上げの日は必ず見に行った。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "rocket_r7a" },
                blockedByLifeTags = new() { "rocket_r7b" },
                grantsLifeTags = new() { "rocket_r7b" },
                relatedJobIds = new() { "lancer_rocket" },
            };
        }
        #endregion

    }
}
