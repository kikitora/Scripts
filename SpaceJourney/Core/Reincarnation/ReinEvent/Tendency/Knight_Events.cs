using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // Knight 傾向の生業イベント
    // ================================================================
    // 自衛官 / 消防士 / 救急救命士 / ボディガード / 裁判官 / 国際弁護人
    //
    // ※ このファイルは MigrateReinEventsToCs ツールで自動生成されました。
    //    手動編集してOKです。Claudeと相談しながら追加・変更する想定。
    // ================================================================
    public static class Knight_Events
    {
        public static IEnumerable<ReinLifeEvent> All()
        {
            foreach (var ev in Jieitai()) yield return ev;
            foreach (var ev in Shobo()) yield return ev;
            foreach (var ev in Kyumei()) yield return ev;
            foreach (var ev in Bodyguard()) yield return ev;
            foreach (var ev in Saiban()) yield return ev;
            foreach (var ev in Bengoshi()) yield return ev;
        }

        #region 自衛官 (knight_jieitai)
        private static IEnumerable<ReinLifeEvent> Jieitai()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_01",
                sentence = "川の氾濫現場で、泥だらけのまま人を担いでいく自衛官を見た。あの背中が忘れられなかった。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "jdf_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                blockedByEventIds = new() { "ev_jdf_01g" },
                relatedJobIds = new() { "knight_jieitai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "jdf_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_01g",
                editorMemo = "原体験保証",
                sentence = "川の氾濫現場で、泥だらけのまま人を担いでいく自衛官を見た。あの背中が忘れられなかった。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "jdf_call" },
                blockedByEventIds = new() { "ev_jdf_01" },
                relatedJobIds = new() { "knight_jieitai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "jdf_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_02",
                sentence = "自衛隊を目指して体力錬成を始めた。毎朝5時に起きて走った。",
                startAge = 16,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "jdf_call" },
                grantsLifeTags = new() { "jdf_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "1500mのタイムが規定を大きく上回り、陸上部の記録まで超えた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "体力はついてきたが、規定まであと少し足りなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "思うように伸びなかった。",
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
                eventId = "ev_jdf_03",
                sentence = "一般曹候補生の採用試験を受けた。筆記・体力・適性検査が課された。",
                startAge = 18,
                endAge = 21,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "jdf_train" },
                blockedByLifeTags = new() { "jdf_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格通知が届いた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 25, weightBonus = 1.0f },
                            new StatCondition { stat = StatKind.AGI, threshold = 25, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "jdf_pass" },
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
                        grantsLifeTags = new() { "jdf_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_03b",
                sentence = "もう一度、採用試験を受けた。",
                startAge = 19,
                endAge = 21,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "jdf_fail" },
                blockedByLifeTags = new() { "jdf_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "今度は合格した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 20, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "jdf_pass" },
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
                        grantsLifeTags = new() { "jdf_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_03c",
                sentence = "採用試験に再挑戦した。ついに合格した。",
                startAge = 22,
                endAge = 22,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "jdf_fail" },
                blockedByLifeTags = new() { "jdf_pass" },
                relatedJobIds = new() { "knight_jieitai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "jdf_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_04",
                sentence = "4月、前期教育が始まった。起床、点呼、訓練、消灯を3ヶ月間ひたすら繰り返した。",
                startAge = 22,
                endAge = 22,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "jdf_pass" },
                blockedByLifeTags = new() { "jdf_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "jdf_in" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_07",
                sentence = "生業が自衛官になった。",
                startAge = 25,
                endAge = 26,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "jdf_grad" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_jieitai" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_05a",
                sentence = "夜間訓練で隊員の一人が脱走した。翌朝、全員が連帯責任で10km走らされた。",
                startAge = 22,
                endAge = 24,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "jdf_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                grantsLifeTags = new() { "jdf_flav_routine" },
                blockedByLifeTags = new() { "jdf_flav_routine" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "先頭を走り続けた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "途中で動けなくなってしまった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "足が動かなくなっても、倒れるまで走った。",
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
                eventId = "ev_jdf_05b",
                sentence = "射撃訓練でゼロインを合わせ、50m先の的を狙った。",
                startAge = 23,
                endAge = 25,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "jdf_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                grantsLifeTags = new() { "jdf_flav_routine" },
                blockedByLifeTags = new() { "jdf_flav_routine" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "初弾から中心部に入れた。教官の目つきが変わった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "3発目から安定した。教官に頷かれた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "弾は散らばった。繰り返すしかなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_06",
                sentence = "前後期教育を終え、部隊配属になった。",
                startAge = 24,
                endAge = 26,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "jdf_in" },
                blockedByLifeTags = new() { "jdf_grad" },
                grantsLifeTags = new() { "jdf_grad" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "普通科に配属され、新しい部隊章を付けた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "需品科に配属された。まずは倉庫の場所を覚えるところから始まった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_07a",
                sentence = "自衛官として任務に就くようになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "job_jieitai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
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
                eventId = "ev_jdf_jdf_ep1",
                sentence = "演習で予定外の状況が発生した。判断して切り抜けた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_jieitai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                grantsLifeTags = new() { "jdf_flav_episode" },
                blockedByLifeTags = new() { "jdf_flav_episode" },
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
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_jdf_ep2",
                sentence = "夜間警戒中、星が異常なほどきれいだった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_jieitai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                grantsLifeTags = new() { "jdf_flav_episode" },
                blockedByLifeTags = new() { "jdf_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_jdf_ep3",
                sentence = "隊員の一人が体調を崩した。フォローに回った。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_jieitai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "-" },
                relatedJobIds = new() { "knight_jieitai" },
                grantsLifeTags = new() { "jdf_flav_episode" },
                blockedByLifeTags = new() { "jdf_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_s1",
                sentence = "長時間の訓練を最後まで完遂した。限界を超えていた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_jieitai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                grantsLifeTags = new() { "jdf_flav_serious" },
                blockedByLifeTags = new() { "jdf_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_s2",
                sentence = "体力の衰えを感じる場面があった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_jieitai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                grantsLifeTags = new() { "jdf_flav_serious" },
                blockedByLifeTags = new() { "jdf_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_s3",
                sentence = "仲間のために動くとき、疲れを感じなかった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_jieitai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                grantsLifeTags = new() { "jdf_flav_serious" },
                blockedByLifeTags = new() { "jdf_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_r1a",
                sentence = "班長を任された。部下が7人ついた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_jieitai" },
                blockedByLifeTags = new() { "jdf_r1a" },
                grantsLifeTags = new() { "jdf_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_r1b",
                sentence = "演習で班を率いた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "jdf_r1a" },
                blockedByLifeTags = new() { "jdf_r1b" },
                grantsLifeTags = new() { "jdf_r1b" },
                relatedJobIds = new() { "knight_jieitai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "全員が想定以上の成果を出した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "一部の隊員が遅れた。立て直して完遂した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_r3a",
                sentence = "幹部候補生として選抜された。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "jdf_r1b" },
                blockedByLifeTags = new() { "jdf_r3a" },
                grantsLifeTags = new() { "jdf_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_r3b",
                sentence = "幹部としての初任務を終えた。責任の重さが変わった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "jdf_r3a" },
                blockedByLifeTags = new() { "jdf_r3b" },
                grantsLifeTags = new() { "jdf_r3b" },
                relatedJobIds = new() { "knight_jieitai" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_r5a",
                sentence = "海外派遣任務への抜擢か、国内での指導職か。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "jdf_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "派遣に手を挙げた。国際の現場に立った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "jdf_r5a_over" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "国内に残り、次世代の育成に回った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "jdf_r5a_home" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_r5b_a",
                sentence = "海外での任務を通じて、自衛官としての軸ができた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "jdf_r5a_over" },
                blockedByLifeTags = new() { "jdf_r5b" },
                grantsLifeTags = new() { "jdf_r5b" },
                relatedJobIds = new() { "knight_jieitai" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_r5b_b",
                sentence = "後輩たちを育てることに、自分の使命を見出した。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "jdf_r5a_home" },
                blockedByLifeTags = new() { "jdf_r5b" },
                grantsLifeTags = new() { "jdf_r5b" },
                relatedJobIds = new() { "knight_jieitai" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_r7a",
                sentence = "将官への推薦が来た。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "jdf_r5b" },
                blockedByLifeTags = new() { "jdf_r7a" },
                grantsLifeTags = new() { "jdf_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_jieitai" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_jdf_r7b",
                sentence = "制服組の最上位に立った日、訓練場をもう一度走った。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "jdf_r7a" },
                blockedByLifeTags = new() { "jdf_r7b" },
                grantsLifeTags = new() { "jdf_r7b" },
                relatedJobIds = new() { "knight_jieitai" },
            };
        }
        #endregion

        #region 消防士 (knight_shobo)
        private static IEnumerable<ReinLifeEvent> Shobo()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_01",
                sentence = "隣の家が燃えた。炎の中に何度も飛び込む消防士を見た。その背中が忘れられなかった。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "fire_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                blockedByEventIds = new() { "ev_fire_01g" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "fire_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_01g",
                editorMemo = "原体験保証",
                sentence = "隣の家が燃えた。炎の中に何度も飛び込む消防士を見た。その背中が忘れられなかった。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "fire_call" },
                blockedByEventIds = new() { "ev_fire_01" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "fire_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_02",
                sentence = "消防士を目指して体力トレーニングを始めた。倍率10倍を超える試験に向けて準備を進めた。",
                startAge = 16,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "fire_call" },
                grantsLifeTags = new() { "fire_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "体力測定の記録が着実に伸びた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "体力はついた。まだ不安が残った。",
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
                eventId = "ev_fire_03",
                sentence = "消防士の採用試験を受けた。体力・筆記・面接の三関門だった。",
                startAge = 18,
                endAge = 21,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "fire_train" },
                blockedByLifeTags = new() { "fire_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格通知が届いた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 25, weightBonus = 1.0f },
                            new StatCondition { stat = StatKind.AGI, threshold = 25, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "fire_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 2 },
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
                        grantsLifeTags = new() { "fire_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_03b",
                sentence = "もう一度採用試験を受けた。",
                startAge = 19,
                endAge = 21,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "fire_fail" },
                blockedByLifeTags = new() { "fire_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "今度は合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "fire_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
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
                        grantsLifeTags = new() { "fire_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_03c",
                sentence = "採用試験に再挑戦した。ついに合格した。",
                startAge = 22,
                endAge = 22,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "fire_fail" },
                blockedByLifeTags = new() { "fire_pass" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "fire_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_04",
                sentence = "消防学校に入校した。基礎訓練や救急、火災対応を一から学び始めた。",
                startAge = 22,
                endAge = 22,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "fire_pass" },
                blockedByLifeTags = new() { "fire_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "fire_in" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_05a",
                sentence = "火点突破訓練に臨んだ。熱気と煙の中でホースを握った。",
                startAge = 22,
                endAge = 24,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "fire_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                grantsLifeTags = new() { "fire_flav_routine" },
                blockedByLifeTags = new() { "fire_flav_routine" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "一歩も退かなかった。教官に「根性がある」と言われた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "熱さで一瞬手が止まった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "倒れて担ぎ出された。",
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
                eventId = "ev_fire_05b",
                sentence = "救急救命の実技訓練で、人工呼吸とAEDの手順を徹底的に叩き込まれた。",
                startAge = 23,
                endAge = 25,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "fire_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                grantsLifeTags = new() { "fire_flav_routine" },
                blockedByLifeTags = new() { "fire_flav_routine" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "手順を完璧に覚えた。同期の中で一番早かった。",
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
                        sentence = "焦って手順が飛んだ。何十回も繰り返した。",
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
                eventId = "ev_fire_06",
                sentence = "消防学校を修了した。",
                startAge = 24,
                endAge = 26,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "fire_in" },
                blockedByLifeTags = new() { "fire_grad" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "fire_grad" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_07",
                sentence = "生業が消防士になった。",
                startAge = 24,
                endAge = 26,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "fire_grad" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_shobo" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_07a",
                sentence = "初めての火災現場に出動した。煙の中で声を出し続けた。",
                startAge = 25,
                endAge = 27,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "fire_normal" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
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
                eventId = "ev_fire_07r",
                sentence = "特別救助隊の訓練が始まった。通常の消防訓練とは比べ物にならない負荷だった。",
                startAge = 24,
                endAge = 27,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "fire_rescue" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "全ての訓練をクリアし、正式にレスキュー隊員として認められた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                            new StatBonus { stat = StatKind.DF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "訓練は過酷でついていくことが出来なかった。残念ながらレスキュー隊員としては認められなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "fire_normal" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_06a",
                sentence = "特別救助隊（レスキュー）の志望候補に選ばれた。",
                startAge = 24,
                endAge = 26,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "fire_grad" },
                blockedByLifeTags = new() { "fire_rescue", "fire_normal" },
                grantsLifeTags = new() { "fire_rescue" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "fire_rescue" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_06b",
                sentence = "正式に消防隊員として配属が決まった。",
                startAge = 24,
                endAge = 26,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "fire_grad" },
                blockedByLifeTags = new() { "fire_rescue", "fire_normal" },
                grantsLifeTags = new() { "fire_normal" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "fire_normal" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_fire_ep1",
                sentence = "出動先で子供が泣いていた。火が消えたあと、少し話した。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_shobo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                grantsLifeTags = new() { "fire_flav_episode" },
                blockedByLifeTags = new() { "fire_flav_episode" },
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
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_fire_ep2",
                sentence = "訓練中、後輩が倒れた。自分が担ぎ出した。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_shobo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                grantsLifeTags = new() { "fire_flav_episode" },
                blockedByLifeTags = new() { "fire_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_fire_ep3",
                sentence = "夜勤明け、朝日が眩しかった。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_shobo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "-" },
                relatedJobIds = new() { "knight_shobo" },
                grantsLifeTags = new() { "fire_flav_episode" },
                blockedByLifeTags = new() { "fire_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_s1",
                sentence = "煙の中でも呼吸が乱れなかった。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_shobo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                grantsLifeTags = new() { "fire_flav_serious" },
                blockedByLifeTags = new() { "fire_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_s2",
                sentence = "重い装備での長時間活動が続いた。体にきた。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_shobo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                grantsLifeTags = new() { "fire_flav_serious" },
                blockedByLifeTags = new() { "fire_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_s3",
                sentence = "仲間が危険な場所に入った。後ろから支えた。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_shobo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                grantsLifeTags = new() { "fire_flav_serious" },
                blockedByLifeTags = new() { "fire_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_r1a",
                sentence = "初めて現場指揮を任された。部隊全体を見る立場になった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_shobo" },
                blockedByLifeTags = new() { "fire_r1a" },
                grantsLifeTags = new() { "fire_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_r1b",
                sentence = "複雑な現場で判断を迫られた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "fire_r1a" },
                blockedByLifeTags = new() { "fire_r1b" },
                grantsLifeTags = new() { "fire_r1b" },
                relatedJobIds = new() { "knight_shobo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "的確な指示で全員無事に完遂した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "後退の判断が遅れた。誰も怪我はなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_r3a",
                sentence = "主任に昇格した。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "fire_r1b" },
                blockedByLifeTags = new() { "fire_r3a" },
                grantsLifeTags = new() { "fire_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_r3b",
                sentence = "大規模火災の対応で指揮を執った。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "fire_r3a" },
                blockedByLifeTags = new() { "fire_r3b" },
                grantsLifeTags = new() { "fire_r3b" },
                relatedJobIds = new() { "knight_shobo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_r5a",
                sentence = "消防司令補への昇任か、技術職として現場に残るか。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "fire_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "昇任した。判断と責任の重さが増した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "fire_r5a_kanri" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "現場に残った。体が動ける限り、ここに立ち続けた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "fire_r5a_genba" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_r5b_a",
                sentence = "組織全体を守る立場から、現場を見るようになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "fire_r5a_kanri" },
                blockedByLifeTags = new() { "fire_r5b" },
                grantsLifeTags = new() { "fire_r5b" },
                relatedJobIds = new() { "knight_shobo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_r5b_b",
                sentence = "ベテランとして現場の最前線に立ち続けた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "fire_r5a_genba" },
                blockedByLifeTags = new() { "fire_r5b" },
                grantsLifeTags = new() { "fire_r5b" },
                relatedJobIds = new() { "knight_shobo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_r7a",
                sentence = "後輩たちが送別会を開いてくれた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "fire_r5b" },
                blockedByLifeTags = new() { "fire_r7a" },
                grantsLifeTags = new() { "fire_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_shobo" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fire_r7b",
                sentence = "退官の日、後輩たちに見送られて消防署を後にした。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "fire_r7a" },
                blockedByLifeTags = new() { "fire_r7b" },
                grantsLifeTags = new() { "fire_r7b" },
                relatedJobIds = new() { "knight_shobo" },
            };
        }
        #endregion

        #region 救急救命士 (knight_kyumei)
        private static IEnumerable<ReinLifeEvent> Kyumei()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_01",
                sentence = "祖父が倒れた時、駆け付けてくれた救急救命士が迅速に処置するところを見た。ヒーローだと思った。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "emt_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                blockedByEventIds = new() { "ev_emt_01g" },
                relatedJobIds = new() { "knight_kyumei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "emt_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_01g",
                editorMemo = "原体験保証",
                sentence = "祖父が倒れた時、駆け付けてくれた救急救命士が迅速に処置するところを見た。ヒーローだと思った。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "emt_call" },
                blockedByEventIds = new() { "ev_emt_01" },
                relatedJobIds = new() { "knight_kyumei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "emt_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_02",
                sentence = "救急救命士の養成校への進学を決めた。",
                startAge = 16,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "emt_call" },
                grantsLifeTags = new() { "emt_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "解剖や薬理の知識が面白かった。成績も上位に入った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "暗記の量は想像以上だった。ついていくのがやっとだった。",
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
                eventId = "ev_emt_03",
                sentence = "救急救命士の国家試験を受けた。",
                startAge = 18,
                endAge = 22,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "emt_train" },
                blockedByLifeTags = new() { "emt_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "emt_pass" },
                        grantsStats = new()
                        {
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
                        grantsLifeTags = new() { "emt_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_03b",
                sentence = "国家試験に再挑戦した。",
                startAge = 20,
                endAge = 22,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "emt_fail" },
                blockedByLifeTags = new() { "emt_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "emt_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
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
                        grantsLifeTags = new() { "emt_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_03c",
                sentence = "国家試験に再挑戦し、ついに合格した。",
                startAge = 23,
                endAge = 23,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "emt_fail" },
                blockedByLifeTags = new() { "emt_pass" },
                relatedJobIds = new() { "knight_kyumei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "emt_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_04",
                sentence = "消防本部に採用され、救急隊員として配属された。",
                startAge = 23,
                endAge = 24,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "emt_pass" },
                grantsLifeTags = new() { "emt_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "最初の出動で、習ったことが全部つながった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "最初の出動では、どう動けばいいのかわからずあたふたしてしまった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_05",
                sentence = "生業が救急救命士になった。",
                startAge = 24,
                endAge = 26,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "emt_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                grantsLifeTags = new() { "emt_flav_routine" },
                blockedByLifeTags = new() { "emt_flav_routine" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_kyumei" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_emt_ep1",
                sentence = "搬送中、患者の手を握り続けた。病院に着くまで離さなかった。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kyumei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                grantsLifeTags = new() { "emt_flav_episode" },
                blockedByLifeTags = new() { "emt_flav_episode" },
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
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_emt_ep2",
                sentence = "後輩が処置に詰まっていた。一言だけ声をかけた。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kyumei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                grantsLifeTags = new() { "emt_flav_episode" },
                blockedByLifeTags = new() { "emt_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_emt_ep3",
                sentence = "続けて重篤搬送が重なった。心が疲れた。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_kyumei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "-" },
                relatedJobIds = new() { "knight_kyumei" },
                grantsLifeTags = new() { "emt_flav_episode" },
                blockedByLifeTags = new() { "emt_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_s1",
                sentence = "長時間の緊張状態でも判断が鈍らなかった。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kyumei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                grantsLifeTags = new() { "emt_flav_serious" },
                blockedByLifeTags = new() { "emt_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_s2",
                sentence = "処置に迷う場面があった。経験を積むしかなかった。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kyumei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                grantsLifeTags = new() { "emt_flav_serious" },
                blockedByLifeTags = new() { "emt_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_s3",
                sentence = "助けられなかった搬送のことを、夜に思い出した。",
                startAge = 25,
                endAge = 58,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kyumei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                grantsLifeTags = new() { "emt_flav_serious" },
                blockedByLifeTags = new() { "emt_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_r1a",
                sentence = "後輩の指導担当になった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_kyumei" },
                blockedByLifeTags = new() { "emt_r1a" },
                grantsLifeTags = new() { "emt_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_r1b",
                sentence = "後輩が初めて一人で対応した現場で、後ろから見ていた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "emt_r1a" },
                blockedByLifeTags = new() { "emt_r1b" },
                grantsLifeTags = new() { "emt_r1b" },
                relatedJobIds = new() { "knight_kyumei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "しっかりこなした。自分のことのように嬉しかった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "途中で詰まった。フォローに入った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_r3a",
                sentence = "高度救命救急センターへの配属が決まった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "emt_r1b" },
                blockedByLifeTags = new() { "emt_r3a" },
                grantsLifeTags = new() { "emt_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_r3b",
                sentence = "難易度の高い搬送案件が続いた。一件一件やり切った。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "emt_r3a" },
                blockedByLifeTags = new() { "emt_r3b" },
                grantsLifeTags = new() { "emt_r3b" },
                relatedJobIds = new() { "knight_kyumei" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_r5a",
                sentence = "チーフパラメディックへの昇格か、教育担当への転換か。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "emt_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "チーフになった。最前線に立ち続けた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "emt_r5a_chief" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "教育担当になった。経験を次に渡す仕事だと思った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "emt_r5a_edu" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_r5b_a",
                sentence = "チーフとして、現場全体を支える立場になった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "emt_r5a_chief" },
                blockedByLifeTags = new() { "emt_r5b" },
                grantsLifeTags = new() { "emt_r5b" },
                relatedJobIds = new() { "knight_kyumei" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_r5b_b",
                sentence = "次の世代に経験を伝えることが、自分の使命になった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "emt_r5a_edu" },
                blockedByLifeTags = new() { "emt_r5b" },
                grantsLifeTags = new() { "emt_r5b" },
                relatedJobIds = new() { "knight_kyumei" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_r7a",
                sentence = "通算何千件という出動を重ねた。記録は数字でしかない。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "emt_r5b" },
                blockedByLifeTags = new() { "emt_r7a" },
                grantsLifeTags = new() { "emt_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_kyumei" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_emt_r7b",
                sentence = "最後の出動から戻ったとき、救急車を降りた足が少し重かった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "emt_r7a" },
                blockedByLifeTags = new() { "emt_r7b" },
                grantsLifeTags = new() { "emt_r7b" },
                relatedJobIds = new() { "knight_kyumei" },
            };
        }
        #endregion

        #region ボディガード (knight_bodyguard)
        private static IEnumerable<ReinLifeEvent> Bodyguard()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_01",
                sentence = "いじめられている同級生をかばって殴られた。",
                startAge = 10,
                endAge = 15,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "guard_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                blockedByEventIds = new() { "ev_guard_01g" },
                relatedJobIds = new() { "knight_bodyguard" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "guard_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_01g",
                editorMemo = "原体験保証",
                sentence = "いじめられている同級生をかばって殴られた。",
                startAge = 15,
                endAge = 15,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "guard_call" },
                blockedByEventIds = new() { "ev_guard_01" },
                relatedJobIds = new() { "knight_bodyguard" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "guard_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_02",
                sentence = "ボディガードという仕事に興味を持ち、必要な資格を調べた。格闘技の段位と射撃関連の資格取得を目指した。",
                startAge = 17,
                endAge = 20,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "guard_call" },
                grantsLifeTags = new() { "guard_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "格闘技の段位を早期に取得できた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "時間はかかったが、必要な資格を揃えた。",
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
                eventId = "ev_guard_03",
                sentence = "警護会社の採用試験を受けた。",
                startAge = 20,
                endAge = 24,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "guard_train" },
                blockedByLifeTags = new() { "guard_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "迷わず答え、その場で採用された。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 35, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 30, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "guard_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "受け答えはしたが採用されず、別の会社に応募した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "guard_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_03b",
                sentence = "別の警護会社に応募した。",
                startAge = 22,
                endAge = 25,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "guard_fail" },
                blockedByLifeTags = new() { "guard_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "採用された。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "guard_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "落ちた。また次を探した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "guard_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_03c",
                sentence = "別の警護会社の審査を受けた。ついに採用された。",
                startAge = 26,
                endAge = 26,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "guard_fail" },
                blockedByLifeTags = new() { "guard_pass" },
                relatedJobIds = new() { "knight_bodyguard" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "guard_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_05",
                sentence = "生業がボディガードになった。",
                startAge = 25,
                endAge = 28,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "guard_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                grantsLifeTags = new() { "guard_flav_routine" },
                blockedByLifeTags = new() { "guard_flav_routine" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_bodyguard" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_04",
                sentence = "初めて単独で依頼人に同行した。ただ隣を歩いているように見えて、意識は常に周囲へ向いていた。",
                startAge = 26,
                endAge = 55,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "job_bodyguard" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                eventStage = 1,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "接触してきた不審者に素早く対処した。依頼人は何も気づかなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "問題はなかった。ただ、ひどく疲弊した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_05a",
                sentence = "ボディガードとして依頼人に同行するようになった。",
                startAge = 26,
                endAge = 55,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "job_bodyguard" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                grantsLifeTags = new() { "guard_flav_routine" },
                blockedByLifeTags = new() { "guard_flav_routine" },
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
                eventId = "ev_guard_guard_ep1",
                sentence = "依頼人が気づかないところで不審者を遠ざけた。",
                startAge = 26,
                endAge = 55,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_bodyguard" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                grantsLifeTags = new() { "guard_flav_episode" },
                blockedByLifeTags = new() { "guard_flav_episode" },
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
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_guard_ep2",
                sentence = "深夜の警護が続いた。静かな時間だった。",
                startAge = 26,
                endAge = 55,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_bodyguard" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                grantsLifeTags = new() { "guard_flav_episode" },
                blockedByLifeTags = new() { "guard_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_guard_ep3",
                sentence = "任務中、自分の判断に迷う瞬間があった。",
                startAge = 26,
                endAge = 55,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_bodyguard" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "-" },
                relatedJobIds = new() { "knight_bodyguard" },
                grantsLifeTags = new() { "guard_flav_episode" },
                blockedByLifeTags = new() { "guard_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_s1",
                sentence = "反応速度が上がっていた。訓練の積み重ねが出た。",
                startAge = 26,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_bodyguard" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                grantsLifeTags = new() { "guard_flav_serious" },
                blockedByLifeTags = new() { "guard_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_s2",
                sentence = "長期警護で集中力の維持が難しくなってきた。",
                startAge = 26,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_bodyguard" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                grantsLifeTags = new() { "guard_flav_serious" },
                blockedByLifeTags = new() { "guard_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_s3",
                sentence = "依頼人の信頼を感じた瞬間があった。",
                startAge = 26,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_bodyguard" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                grantsLifeTags = new() { "guard_flav_serious" },
                blockedByLifeTags = new() { "guard_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_r1a",
                sentence = "高リスクの要人警護を初めて担当した。",
                startAge = 26,
                endAge = 56,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_bodyguard" },
                blockedByLifeTags = new() { "guard_r1a" },
                grantsLifeTags = new() { "guard_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_r1b",
                sentence = "事前偵察から当日の警護まで、単独で計画を立てた。",
                startAge = 26,
                endAge = 56,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "guard_r1a" },
                blockedByLifeTags = new() { "guard_r1b" },
                grantsLifeTags = new() { "guard_r1b" },
                relatedJobIds = new() { "knight_bodyguard" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "問題なく終わった。依頼人に信頼された。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "予期しない接触があった。対処した。依頼人は何も気づかなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_r3a",
                sentence = "海外要人の警護を任された。",
                startAge = 26,
                endAge = 56,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "guard_r1b" },
                blockedByLifeTags = new() { "guard_r3a" },
                grantsLifeTags = new() { "guard_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_r3b",
                sentence = "同業者から信頼できる名前として紹介されるようになった。",
                startAge = 26,
                endAge = 56,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "guard_r3a" },
                blockedByLifeTags = new() { "guard_r3b" },
                grantsLifeTags = new() { "guard_r3b" },
                relatedJobIds = new() { "knight_bodyguard" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_r5a",
                sentence = "警護会社を立ち上げるか、フリーのまま続けるか。",
                startAge = 26,
                endAge = 56,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "guard_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "会社を立ち上げた。後輩を雇った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "guard_r5a_co" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "フリーを続けた。自分一人で完璧にやりたかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "guard_r5a_free" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_r5b_a",
                sentence = "会社を育てながら、警護の質を守り続けた。",
                startAge = 26,
                endAge = 56,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "guard_r5a_co" },
                blockedByLifeTags = new() { "guard_r5b" },
                grantsLifeTags = new() { "guard_r5b" },
                relatedJobIds = new() { "knight_bodyguard" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_r5b_b",
                sentence = "フリーとして、自分の基準だけで仕事を選んだ。",
                startAge = 26,
                endAge = 56,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "guard_r5a_free" },
                blockedByLifeTags = new() { "guard_r5b" },
                grantsLifeTags = new() { "guard_r5b" },
                relatedJobIds = new() { "knight_bodyguard" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_r7a",
                sentence = "引退を考える年齢になった。後輩が育っていた。",
                startAge = 26,
                endAge = 56,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "guard_r5b" },
                blockedByLifeTags = new() { "guard_r7a" },
                grantsLifeTags = new() { "guard_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bodyguard" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_guard_r7b",
                sentence = "最後の依頼を終えた。依頼人が初めて名前を聞いてきた。",
                startAge = 26,
                endAge = 56,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "guard_r7a" },
                blockedByLifeTags = new() { "guard_r7b" },
                grantsLifeTags = new() { "guard_r7b" },
                relatedJobIds = new() { "knight_bodyguard" },
            };
        }
        #endregion

        #region 裁判官 (knight_saiban)
        private static IEnumerable<ReinLifeEvent> Saiban()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_01",
                sentence = "法廷を傍聴した。裁判官が静かな声で主文を読み上げ、法廷が静まり返った。",
                startAge = 10,
                endAge = 15,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "saiban_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                blockedByEventIds = new() { "ev_saiban_01g" },
                relatedJobIds = new() { "knight_saiban" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "saiban_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_01g",
                editorMemo = "原体験保証",
                sentence = "法廷を傍聴した。裁判官が静かな声で主文を読み上げ、法廷が静まり返った。",
                startAge = 15,
                endAge = 15,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "saiban_call" },
                blockedByEventIds = new() { "ev_saiban_01" },
                relatedJobIds = new() { "knight_saiban" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "saiban_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_02",
                sentence = "大学の法学部に進んだ後、司法試験を目指して判例と条文を読み込んだ。",
                startAge = 19,
                endAge = 22,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "saiban_call" },
                grantsLifeTags = new() { "saiban_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "論理を積み上げる作業が苦にならなかった。模試の成績も安定した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "量の多さに圧倒されたが、毎日続けた。",
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
                eventId = "ev_saiban_03",
                sentence = "司法試験を受験した。",
                startAge = 24,
                endAge = 28,
                baseWeight = 0.65f,
                requiresAnyLifeTag = new() { "saiban_train" },
                blockedByLifeTags = new() { "saiban_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。合格者一覧で自分の番号を見つけた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 45, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.MDF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "saiban_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
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
                        grantsLifeTags = new() { "saiban_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_03b",
                sentence = "司法試験を再受験した。",
                startAge = 23,
                endAge = 27,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "saiban_fail" },
                blockedByLifeTags = new() { "saiban_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "saiban_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
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
                        grantsLifeTags = new() { "saiban_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_03c",
                sentence = "司法試験に再挑戦した。ついに合格した。",
                startAge = 28,
                endAge = 28,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "saiban_fail" },
                blockedByLifeTags = new() { "saiban_pass" },
                relatedJobIds = new() { "knight_saiban" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "saiban_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_04",
                sentence = "司法修習を終え、裁判官任官の選考を受けた。誰でもなれるわけではなかった。",
                startAge = 27,
                endAge = 30,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "saiban_pass" },
                blockedByLifeTags = new() { "saiban_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "任官が認められた。任命式で宣誓した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MDF, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "saiban_in" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 4 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "見送られた。弁護士として経験を積み、再度志願することにした。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "saiban_wait" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_04b",
                sentence = "弁護士経験を積んだ後、裁判官任官に再度志願した。",
                startAge = 30,
                endAge = 35,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "saiban_wait" },
                blockedByLifeTags = new() { "saiban_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "任官が認められた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MDF, threshold = 40, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 30, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "saiban_in" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "また見送られた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "saiban_wait" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_04c",
                sentence = "任官をもう一度志願し、ついに認められた。",
                startAge = 36,
                endAge = 36,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "saiban_wait" },
                blockedByLifeTags = new() { "saiban_in" },
                relatedJobIds = new() { "knight_saiban" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "saiban_in" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_06",
                sentence = "生業が裁判官になった。",
                startAge = 30,
                endAge = 38,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "saiban_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_saiban" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_05",
                sentence = "死刑求刑がなされた事件の合議体に加わった。",
                startAge = 30,
                endAge = 65,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "job_saiban" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                grantsLifeTags = new() { "saiban_flav_routine" },
                blockedByLifeTags = new() { "saiban_flav_routine" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "評議室で何日も議論した。判決文を書き終えた夜は、食事が喉を通らなかった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MDF, threshold = 50, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 42, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "判決を出した。それが仕事だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_saiban_ep1",
                sentence = "長い評議の末、全員が納得する判断に至った。",
                startAge = 30,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_saiban" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                grantsLifeTags = new() { "saiban_flav_episode" },
                blockedByLifeTags = new() { "saiban_flav_episode" },
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
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_saiban_ep2",
                sentence = "廊下で当事者とすれ違った。目が合った。",
                startAge = 30,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_saiban" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                grantsLifeTags = new() { "saiban_flav_episode" },
                blockedByLifeTags = new() { "saiban_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_saiban_ep3",
                sentence = "判決文を書き終えた夜、窓の外を長い間見ていた。",
                startAge = 30,
                endAge = 65,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_saiban" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "-" },
                relatedJobIds = new() { "knight_saiban" },
                grantsLifeTags = new() { "saiban_flav_episode" },
                blockedByLifeTags = new() { "saiban_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_s1",
                sentence = "証拠の細部に、他の誰も気づかなかった矛盾を見つけた。",
                startAge = 30,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_saiban" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                grantsLifeTags = new() { "saiban_flav_serious" },
                blockedByLifeTags = new() { "saiban_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_s2",
                sentence = "重圧が続いた。それでも法廷に立ち続けた。",
                startAge = 30,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_saiban" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                grantsLifeTags = new() { "saiban_flav_serious" },
                blockedByLifeTags = new() { "saiban_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_s3",
                sentence = "正しいことをしたのか、確信が持てない判決があった。",
                startAge = 30,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_saiban" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                grantsLifeTags = new() { "saiban_flav_serious" },
                blockedByLifeTags = new() { "saiban_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_r1a",
                sentence = "初めて合議体の主任裁判官になった。",
                startAge = 30,
                endAge = 60,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_saiban" },
                blockedByLifeTags = new() { "saiban_r1a" },
                grantsLifeTags = new() { "saiban_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_r1b",
                sentence = "証拠の評価で難しい判断を迫られた。",
                startAge = 30,
                endAge = 60,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "saiban_r1a" },
                blockedByLifeTags = new() { "saiban_r1b" },
                grantsLifeTags = new() { "saiban_r1b" },
                relatedJobIds = new() { "knight_saiban" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "評議を重ね、合議体全員が納得する判決に至った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "評議が割れた。多数決で決めた。その夜は眠れなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_r3a",
                sentence = "重大事件を担当するようになった。",
                startAge = 30,
                endAge = 60,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "saiban_r1b" },
                blockedByLifeTags = new() { "saiban_r3a" },
                grantsLifeTags = new() { "saiban_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_r3b",
                sentence = "長期裁判が結審した。判決文を読み上げた。",
                startAge = 30,
                endAge = 60,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "saiban_r3a" },
                blockedByLifeTags = new() { "saiban_r3b" },
                grantsLifeTags = new() { "saiban_r3b" },
                relatedJobIds = new() { "knight_saiban" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_r5a",
                sentence = "最高裁判所調査官への打診が来た。",
                startAge = 30,
                endAge = 60,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "saiban_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "受けた。法の解釈に深く関わる仕事だった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "saiban_r5a_high" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "断った。地裁の現場に残ることを選んだ。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "saiban_r5a_local" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_r5b_a",
                sentence = "法の解釈という場所から、判例の形成に関わるようになった。",
                startAge = 30,
                endAge = 60,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "saiban_r5a_high" },
                blockedByLifeTags = new() { "saiban_r5b" },
                grantsLifeTags = new() { "saiban_r5b" },
                relatedJobIds = new() { "knight_saiban" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_r5b_b",
                sentence = "地裁で一件一件の事実に向き合い続けた。",
                startAge = 30,
                endAge = 60,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "saiban_r5a_local" },
                blockedByLifeTags = new() { "saiban_r5b" },
                grantsLifeTags = new() { "saiban_r5b" },
                relatedJobIds = new() { "knight_saiban" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_r7a",
                sentence = "裁判官として何百という判決を書いてきた。",
                startAge = 30,
                endAge = 60,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "saiban_r5b" },
                blockedByLifeTags = new() { "saiban_r7a" },
                grantsLifeTags = new() { "saiban_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_saiban" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_saiban_r7b",
                sentence = "最後の法廷を退出した日、廊下が静かだった。",
                startAge = 30,
                endAge = 60,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "saiban_r7a" },
                blockedByLifeTags = new() { "saiban_r7b" },
                grantsLifeTags = new() { "saiban_r7b" },
                relatedJobIds = new() { "knight_saiban" },
            };
        }
        #endregion

        #region 国際弁護人 (knight_bengoshi)
        private static IEnumerable<ReinLifeEvent> Bengoshi()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_01",
                sentence = "海外ニュースで、裁判もなく拘束される民間人の映像を見た。何度も見返した。",
                startAge = 10,
                endAge = 15,
                baseWeight = 0.08f,
                blockedByLifeTags = new() { "intl_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                blockedByEventIds = new() { "ev_intl_01g" },
                relatedJobIds = new() { "knight_bengoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "intl_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_01g",
                editorMemo = "原体験保証",
                sentence = "海外のニュースで、裁判もなく拘束される民間人の映像を見た。何度も見返した。",
                startAge = 15,
                endAge = 15,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "intl_call" },
                blockedByEventIds = new() { "ev_intl_01" },
                relatedJobIds = new() { "knight_bengoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "intl_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_02",
                sentence = "法学部に進み、国際法と人権法を専攻した。英語と第二外国語の習得も同時に進めた。",
                startAge = 17,
                endAge = 20,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "intl_call" },
                grantsLifeTags = new() { "intl_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "国際法の複雑な構造に惹かれた。教授から大学院進学を勧められた。",
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
                        sentence = "量が膨大だった。それでも全部読んだ。",
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
                eventId = "ev_intl_03",
                sentence = "司法試験を受験した。大学院修了後も受験を続けた。",
                startAge = 22,
                endAge = 28,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "intl_train" },
                blockedByLifeTags = new() { "intl_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
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
                            new StatCondition { stat = StatKind.MAT, threshold = 50, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.MDF, threshold = 40, weightBonus = 1.0f },
                            new StatCondition { stat = StatKind.DF, threshold = 30, weightBonus = 0.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "intl_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 4 },
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
                        grantsLifeTags = new() { "intl_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_03b",
                sentence = "再受験した。",
                startAge = 24,
                endAge = 30,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "intl_fail" },
                blockedByLifeTags = new() { "intl_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
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
                            new StatCondition { stat = StatKind.MAT, threshold = 42, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.MDF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "intl_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 4 },
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
                        grantsLifeTags = new() { "intl_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_03c",
                sentence = "司法試験に再挑戦した。ついに合格した。",
                startAge = 31,
                endAge = 31,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "intl_fail" },
                blockedByLifeTags = new() { "intl_pass" },
                relatedJobIds = new() { "knight_bengoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "intl_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_04",
                sentence = "国際刑事裁判所（ICC）や国連関連機関で実務を始めた。最初の仕事は記録整理だった。",
                startAge = 30,
                endAge = 35,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "intl_pass" },
                grantsLifeTags = new() { "intl_field" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "現地語での証人尋問を任された。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 52, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 45, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "補助として動きながら、国際法廷の構造を体で覚えた。",
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
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_06",
                sentence = "生業が国際弁護人になった。",
                startAge = 32,
                endAge = 35,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "intl_field" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_bengoshi" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_05a",
                sentence = "戦争犯罪の被告を弁護することになった。世論は有罪を前提にしていた。",
                startAge = 33,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_bengoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                grantsLifeTags = new() { "intl_flav_routine" },
                blockedByLifeTags = new() { "intl_flav_routine" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "証拠を積み上げ、その結果、一部の訴因で無罪が認められた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 55, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.MDF, threshold = 48, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 35, weightBonus = 0.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 10 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 6 },
                            new StatBonus { stat = StatKind.MDF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "できることはすべてやった。それでも判決は有罪だった。だが、記録は残った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                            new StatBonus { stat = StatKind.MDF, value = 4 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_05b",
                sentence = "証人が脅迫を受けて証言を撤回し、裁判の核心が崩れた。",
                startAge = 33,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_bengoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                grantsLifeTags = new() { "intl_flav_routine" },
                blockedByLifeTags = new() { "intl_flav_routine" },
                eventStage = -1,
                statCompareCount = 3,
                statCompareMode = "avg",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "代替証拠を探した。2ヶ月で別の証言者を確保した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MDF, threshold = 50, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 42, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "手詰まりになった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_intl_ep1",
                sentence = "証人の証言に矛盾を見つけた。静かに、しかし確実に突いた。",
                startAge = 33,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_bengoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                grantsLifeTags = new() { "intl_flav_episode" },
                blockedByLifeTags = new() { "intl_flav_episode" },
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
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_intl_ep2",
                sentence = "現地の通訳と夜遅くまで証拠を確認した。",
                startAge = 33,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_bengoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                grantsLifeTags = new() { "intl_flav_episode" },
                blockedByLifeTags = new() { "intl_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_intl_ep3",
                sentence = "裁判所の外で報道陣に囲まれた。何も話さなかった。",
                startAge = 33,
                endAge = 65,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_bengoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "-" },
                relatedJobIds = new() { "knight_bengoshi" },
                grantsLifeTags = new() { "intl_flav_episode" },
                blockedByLifeTags = new() { "intl_flav_episode" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_s1",
                sentence = "国際法の抜け穴を見つけた。それが被告を守った。",
                startAge = 33,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_bengoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                grantsLifeTags = new() { "intl_flav_serious" },
                blockedByLifeTags = new() { "intl_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_s2",
                sentence = "圧力がかかり続けた。それでも弁護を続けた。",
                startAge = 33,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_bengoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                grantsLifeTags = new() { "intl_flav_serious" },
                blockedByLifeTags = new() { "intl_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_s3",
                sentence = "判決が出た夜、何が正義か分からなくなった。",
                startAge = 33,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_bengoshi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                grantsLifeTags = new() { "intl_flav_serious" },
                blockedByLifeTags = new() { "intl_flav_serious" },
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
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_r1a",
                sentence = "初めて独立した弁護チームを率いた。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_bengoshi" },
                blockedByLifeTags = new() { "intl_r1a" },
                grantsLifeTags = new() { "intl_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_r1b",
                sentence = "国際法廷での審理を主導した。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "intl_r1a" },
                blockedByLifeTags = new() { "intl_r1b" },
                grantsLifeTags = new() { "intl_r1b" },
                relatedJobIds = new() { "knight_bengoshi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "証拠を積み上げ、評価された。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "反論に押される場面があった。それでも継続した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_r3a",
                sentence = "国連関連機関から顧問依頼が来た。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "intl_r1b" },
                blockedByLifeTags = new() { "intl_r3a" },
                grantsLifeTags = new() { "intl_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_r3b",
                sentence = "国際法の解釈を巡る議論で、主要な論者として名が上がった。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "intl_r3a" },
                blockedByLifeTags = new() { "intl_r3b" },
                grantsLifeTags = new() { "intl_r3b" },
                relatedJobIds = new() { "knight_bengoshi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_r5a",
                sentence = "大規模な国際刑事裁判の主任弁護人に指名された。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "intl_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "引き受けた。世界が注目する裁判だった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.DF, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.DF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "intl_r5a_big" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "別の案件を優先した。規模より実質を選んだ。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "intl_r5a_small" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_r5b_a",
                sentence = "世界的な注目の中で弁護を全うした。記録が残った。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "intl_r5a_big" },
                blockedByLifeTags = new() { "intl_r5b" },
                grantsLifeTags = new() { "intl_r5b" },
                relatedJobIds = new() { "knight_bengoshi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_r5b_b",
                sentence = "実質的な変化をもたらす案件に集中した。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "intl_r5a_small" },
                blockedByLifeTags = new() { "intl_r5b" },
                grantsLifeTags = new() { "intl_r5b" },
                relatedJobIds = new() { "knight_bengoshi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_r7a",
                sentence = "後進の弁護士から師事を乞われるようになった。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "intl_r5b" },
                blockedByLifeTags = new() { "intl_r7a" },
                grantsLifeTags = new() { "intl_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "knight_bengoshi" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_intl_r7b",
                sentence = "最後の法廷で最終弁論を行った。判決がどうであれ、記録は残った。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "intl_r7a" },
                blockedByLifeTags = new() { "intl_r7b" },
                grantsLifeTags = new() { "intl_r7b" },
                relatedJobIds = new() { "knight_bengoshi" },
            };
        }
        #endregion

    }
}
