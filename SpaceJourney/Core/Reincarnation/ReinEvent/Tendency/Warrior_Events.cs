using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // Warrior 傾向の生業イベント
    // ================================================================
    // 警察官 / サラリーマン / ヤクザ / 剣術師範 / 傭兵 / 宇宙飛行士
    //
    // ※ このファイルは MigrateReinEventsToCs ツールで自動生成されました。
    //    手動編集してOKです。Claudeと相談しながら追加・変更する想定。
    // ================================================================
    public static class Warrior_Events
    {
        public static IEnumerable<ReinLifeEvent> All()
        {
            foreach (var ev in Keisatsu()) yield return ev;
            foreach (var ev in Salaryman()) yield return ev;
            foreach (var ev in Yakuza()) yield return ev;
            foreach (var ev in Kenjutsu()) yield return ev;
            foreach (var ev in Yohei()) yield return ev;
            foreach (var ev in Uchu()) yield return ev;
        }

        #region 警察官 (warrior_keisatsu)
        private static IEnumerable<ReinLifeEvent> Keisatsu()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_01",
                sentence = "強盗犯を警察官が取り押さえる場面を目撃した。憧れを感じた。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "kei_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_kei_01g" },
                relatedJobIds = new() { "warrior_keisatsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "kei_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_01g",
                editorMemo = "原体験保証",
                sentence = "強盗犯を警察官が取り押さえる場面を目撃した。憧れを感じた。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "kei_call" },
                blockedByEventIds = new() { "ev_kei_01" },
                relatedJobIds = new() { "warrior_keisatsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "kei_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_02",
                sentence = "警察官を目指して採用試験の準備を始めた。体力と筆記の両方が必要だと分かった。",
                startAge = 16,
                endAge = 19,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "kei_call" },
                grantsLifeTags = new() { "kei_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "体力測定の記録が規定を余裕で超えた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "体力はついたが、筆記に課題が残った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "両立は難しかった。",
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
                eventId = "ev_kei_03",
                sentence = "都道府県警察の採用試験を受けた。",
                startAge = 18,
                endAge = 21,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "kei_train" },
                blockedByLifeTags = new() { "kei_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
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
                            new StatCondition { stat = StatKind.AT, threshold = 25, weightBonus = 1.0f },
                            new StatCondition { stat = StatKind.MAT, threshold = 25, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "kei_pass" },
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
                        grantsLifeTags = new() { "kei_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_03b",
                sentence = "もう一度、採用試験を受けた。",
                startAge = 19,
                endAge = 21,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "kei_fail" },
                blockedByLifeTags = new() { "kei_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
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
                        grantsLifeTags = new() { "kei_pass" },
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
                        grantsLifeTags = new() { "kei_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_03c",
                sentence = "採用試験に再挑戦した。ついに合格した。",
                startAge = 22,
                endAge = 22,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "kei_fail" },
                blockedByLifeTags = new() { "kei_pass" },
                relatedJobIds = new() { "warrior_keisatsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "kei_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_04",
                sentence = "警察学校に入校した。",
                startAge = 22,
                endAge = 22,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "kei_pass" },
                blockedByLifeTags = new() { "kei_school" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "kei_school" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_04b",
                sentence = "半年間、逮捕術・法学・射撃を懸命に学んだ。",
                startAge = 22,
                endAge = 23,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "kei_school" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "術科と学科の両方で上位に入った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 30, weightBonus = 1.5f },
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
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "何とかついていった。",
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
                eventId = "ev_kei_04c",
                sentence = "警察学校を卒業した。",
                startAge = 23,
                endAge = 23,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "kei_school" },
                blockedByLifeTags = new() { "kei_in" },
                grantsLifeTags = new() { "kei_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "総代として壇上に立った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 30, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 30, weightBonus = 1.0f },
                        },
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
                        sentence = "辞令を受け取り、配属先へ向かった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 1 },
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
                eventId = "ev_kei_06",
                sentence = "正式に警察官として任官した。生業が警察官になった。",
                startAge = 23,
                endAge = 25,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "kei_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_keisatsu" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_05a",
                sentence = "交番勤務の夜、酔っ払いが騒いでいた。",
                startAge = 24,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_keisatsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                grantsLifeTags = new() { "kei_flav_routine" },
                blockedByLifeTags = new() { "kei_flav_routine" },
                eventStage = 1,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "上手く説得できた。先輩が後ろで腕を組んで見ていた。",
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
                    new ReinSentenceOption
                    {
                        sentence = "時間はかかったが、なだめることができた。",
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
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_05b",
                sentence = "引ったくりの場面に遭遇した。",
                startAge = 24,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_keisatsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                grantsLifeTags = new() { "kei_flav_routine" },
                blockedByLifeTags = new() { "kei_flav_routine" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "流れるように制圧した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 35, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 30, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "素早く応援を呼んだ。",
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
                        sentence = "戸惑っている間に、逃げられてしまった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_kei_ep1",
                sentence = "警ら中、不審な人物に職務質問した。指名手配犯だった。署で表彰された。",
                startAge = 24,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_keisatsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                grantsLifeTags = new() { "kei_flav_episode" },
                blockedByLifeTags = new() { "kei_flav_episode" },
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
                eventId = "ev_kei_kei_ep2",
                sentence = "交番に迷子の子供が来た。親が見つかるまでそばにいた。",
                startAge = 24,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_keisatsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                grantsLifeTags = new() { "kei_flav_episode" },
                blockedByLifeTags = new() { "kei_flav_episode" },
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
                eventId = "ev_kei_kei_ep3",
                sentence = "報告書の書き直しを三度命じられた。",
                startAge = 24,
                endAge = 60,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_keisatsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "warrior_keisatsu" },
                grantsLifeTags = new() { "kei_flav_episode" },
                blockedByLifeTags = new() { "kei_flav_episode" },
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
                eventId = "ev_kei_s1",
                sentence = "容疑者追跡の場面で、体が先に動いていた。",
                startAge = 24,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_keisatsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                grantsLifeTags = new() { "kei_flav_serious" },
                blockedByLifeTags = new() { "kei_flav_serious" },
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
                eventId = "ev_kei_s2",
                sentence = "現場の対応が後手に回った。次は同じミスをしないと決めた。",
                startAge = 24,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_keisatsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                grantsLifeTags = new() { "kei_flav_serious" },
                blockedByLifeTags = new() { "kei_flav_serious" },
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
                eventId = "ev_kei_s3",
                sentence = "判決が軽すぎると感じた。それでも法の中で動き続けた。",
                startAge = 24,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_keisatsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                grantsLifeTags = new() { "kei_flav_serious" },
                blockedByLifeTags = new() { "kei_flav_serious" },
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
                eventId = "ev_kei_r1a",
                sentence = "初めて単独で担当事件を持った。",
                startAge = 24,
                endAge = 54,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_keisatsu" },
                blockedByLifeTags = new() { "kei_r1a" },
                grantsLifeTags = new() { "kei_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_r1b",
                sentence = "地道な聞き込みの末、容疑者を絞り込んだ。",
                startAge = 24,
                endAge = 54,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "kei_r1a" },
                blockedByLifeTags = new() { "kei_r1b" },
                grantsLifeTags = new() { "kei_r1b" },
                relatedJobIds = new() { "warrior_keisatsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "逮捕に至った。係長から手柄を称えられた。",
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
                        sentence = "証拠が固まらず、立件を断念した。",
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
                eventId = "ev_kei_r3a",
                sentence = "刑事課への異動辞令が出た。",
                startAge = 24,
                endAge = 54,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "kei_r1b" },
                blockedByLifeTags = new() { "kei_r3a" },
                grantsLifeTags = new() { "kei_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_r3b",
                sentence = "組織犯罪の捜査が実を結んだ。全容が見えてきた。",
                startAge = 24,
                endAge = 54,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "kei_r3a" },
                blockedByLifeTags = new() { "kei_r3b" },
                grantsLifeTags = new() { "kei_r3b" },
                relatedJobIds = new() { "warrior_keisatsu" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_r5a",
                sentence = "管理職への打診が来た。",
                startAge = 24,
                endAge = 54,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "kei_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "受けた。現場を離れても守れるものがある。",
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
                        grantsLifeTags = new() { "kei_r5a_exec" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "断った。現場に居続けることを選んだ。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "kei_r5a_field" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_r5b_a",
                sentence = "管理職として後輩の指導に回るようになった。",
                startAge = 24,
                endAge = 54,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "kei_r5a_exec" },
                blockedByLifeTags = new() { "kei_r5b" },
                grantsLifeTags = new() { "kei_r5b" },
                relatedJobIds = new() { "warrior_keisatsu" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_r5b_b",
                sentence = "ベテランとして現場に立ち続けた。若手が頼りにしてきた。",
                startAge = 24,
                endAge = 54,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "kei_r5a_field" },
                blockedByLifeTags = new() { "kei_r5b" },
                grantsLifeTags = new() { "kei_r5b" },
                relatedJobIds = new() { "warrior_keisatsu" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_r7a",
                sentence = "長年追いかけていた事件の犯人が、ついに逮捕された。",
                startAge = 24,
                endAge = 54,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "kei_r5b" },
                blockedByLifeTags = new() { "kei_r7a" },
                grantsLifeTags = new() { "kei_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_keisatsu" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kei_r7b",
                sentence = "最後の勤務日、制服を脱いだ。後輩たちが廊下に並んでいた。",
                startAge = 24,
                endAge = 54,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "kei_r7a" },
                blockedByLifeTags = new() { "kei_r7b" },
                grantsLifeTags = new() { "kei_r7b" },
                relatedJobIds = new() { "warrior_keisatsu" },
            };
        }
        #endregion

        #region サラリーマン (warrior_salaryman)
        private static IEnumerable<ReinLifeEvent> Salaryman()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_01",
                sentence = "会社に就職した。",
                startAge = 22,
                endAge = 26,
                baseWeight = 0.7f,
                blockedByLifeTags = new() { "sal_work" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sal_work" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_ep1",
                sentence = "難しい仕事を任されたが、なんとか締め切りに間に合わせた。",
                startAge = 22,
                endAge = 30,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "sal_work" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                grantsLifeTags = new() { "sal_flav_episode" },
                blockedByLifeTags = new() { "sal_flav_episode" },
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
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
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
                eventId = "ev_sal_ep2",
                sentence = "担当した顧客から指名で依頼が来るようになった。",
                startAge = 22,
                endAge = 30,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "sal_work" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                grantsLifeTags = new() { "sal_flav_episode" },
                blockedByLifeTags = new() { "sal_flav_episode" },
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
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
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
                eventId = "ev_sal_ep3",
                sentence = "プロジェクトが炎上し、三日間家に帰れなかった。",
                startAge = 22,
                endAge = 30,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "sal_work" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "warrior_salaryman" },
                grantsLifeTags = new() { "sal_flav_episode" },
                blockedByLifeTags = new() { "sal_flav_episode" },
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
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = -2 },
                            new StatBonus { stat = StatKind.AGI, value = -1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_02",
                sentence = "生業がサラリーマンになった。",
                startAge = 25,
                endAge = 31,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "sal_work" },
                blockedByLifeTags = new() { "job_salaryman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_salaryman" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_r1a",
                sentence = "部下が一人ついたが、何を教えるべきか分からなかった。",
                startAge = 25,
                endAge = 44,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_salaryman" },
                blockedByLifeTags = new() { "sal_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sal_r1a" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_r1b",
                sentence = "その部下が初めて一人で案件をまとめてきた。自分のやり方とは全然違っていたが、うまくいった。",
                startAge = 25,
                endAge = 44,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "sal_r1a" },
                blockedByLifeTags = new() { "sal_r1b" },
                relatedJobIds = new() { "warrior_salaryman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sal_r1b" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_r3a",
                sentence = "課長に昇進した。給料は上がったが、残業も増えた。",
                startAge = 25,
                endAge = 44,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "sal_r1b" },
                blockedByLifeTags = new() { "sal_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sal_r3a" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_r3b",
                sentence = "チームが大型案件を取った。全員で残業し、納期に間に合わせた。",
                startAge = 25,
                endAge = 44,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "sal_r3a" },
                blockedByLifeTags = new() { "sal_r3b" },
                relatedJobIds = new() { "warrior_salaryman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sal_r3b" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_r5a",
                sentence = "役員候補に名前が挙がった。",
                startAge = 25,
                endAge = 44,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "sal_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "役員候補の話を受けることにした。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 50, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 45, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 8 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "sal_r5_exec" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 5 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "断った。現場にいたかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "sal_r5_field" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 5 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_r5b_exec",
                sentence = "役員会議で初めて発言した。誰も聞いていないように見えたが、それでも言った。",
                startAge = 25,
                endAge = 44,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "sal_r5_exec" },
                blockedByLifeTags = new() { "sal_r5b" },
                relatedJobIds = new() { "warrior_salaryman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sal_r5b" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_r5b_field",
                sentence = "現場で最年長になった。若い社員が何かあると来るようになった。",
                startAge = 25,
                endAge = 44,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "sal_r5_field" },
                blockedByLifeTags = new() { "sal_r5b" },
                relatedJobIds = new() { "warrior_salaryman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sal_r5b" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_r7a",
                sentence = "部下が泣きながら礼を言いに来た。理由は聞かなかった。",
                startAge = 25,
                endAge = 44,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "sal_r5b" },
                blockedByLifeTags = new() { "sal_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sal_r7a" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_r7b",
                sentence = "定年退職の日、机を片付けながら入社初日のことを思い出した。",
                startAge = 25,
                endAge = 60,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "sal_r7a" },
                blockedByLifeTags = new() { "sal_r7b" },
                relatedJobIds = new() { "warrior_salaryman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sal_r7b" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_s1",
                sentence = "終電を逃した。タクシーの中で窓の外を見ていた。",
                startAge = 25,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_salaryman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                grantsLifeTags = new() { "sal_flav_serious" },
                blockedByLifeTags = new() { "sal_flav_serious" },
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
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_s2",
                sentence = "取引先の担当者が変わった。一から関係を作り直した。",
                startAge = 25,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_salaryman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                grantsLifeTags = new() { "sal_flav_serious" },
                blockedByLifeTags = new() { "sal_flav_serious" },
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
                eventId = "ev_sal_s3",
                sentence = "同僚が突然辞めた。理由は教えてもらえなかった。",
                startAge = 25,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_salaryman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "warrior_salaryman" },
                grantsLifeTags = new() { "sal_flav_serious" },
                blockedByLifeTags = new() { "sal_flav_serious" },
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
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sal_s4",
                sentence = "会議で自分のアイデアが採用された。翌週には別の名前がついていた。",
                startAge = 25,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_salaryman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "-" },
                relatedJobIds = new() { "warrior_salaryman" },
                grantsLifeTags = new() { "sal_flav_serious" },
                blockedByLifeTags = new() { "sal_flav_serious" },
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
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
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
                eventId = "ev_sal_s5",
                sentence = "休日出勤の帰り、公園で子供を連れた家族を見かけた。しばらくベンチに座っていた。",
                startAge = 25,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_salaryman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "-" },
                relatedJobIds = new() { "warrior_salaryman" },
                grantsLifeTags = new() { "sal_flav_serious" },
                blockedByLifeTags = new() { "sal_flav_serious" },
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

        #region ヤクザ (warrior_yakuza)
        private static IEnumerable<ReinLifeEvent> Yakuza()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_01",
                sentence = "ケンカが強いと評判になり、地元の組に声をかけられた。",
                startAge = 12,
                endAge = 16,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "yak_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_yak_01g" },
                relatedJobIds = new() { "warrior_yakuza" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "yak_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_01g",
                editorMemo = "原体験保証",
                sentence = "ケンカが強いと評判になり、地元の組に声をかけられた。",
                startAge = 16,
                endAge = 16,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "yak_call" },
                blockedByEventIds = new() { "ev_yak_01" },
                relatedJobIds = new() { "warrior_yakuza" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "yak_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_02",
                sentence = "組の兄貴分について回るようになり、ミカジメ料の回収や夜の街の仕切りなど、裏側を少しずつ知っていった。",
                startAge = 15,
                endAge = 18,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "yak_call" },
                grantsLifeTags = new() { "yak_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "腕っぷしを評価され、兄貴分に気に入られた。",
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
                        sentence = "使いっ走りを続けた。組の論理に慣れていった。",
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
                eventId = "ev_yak_03",
                sentence = "幼馴染が抗争で捕まり、刑務所に入れられた。",
                startAge = 17,
                endAge = 20,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "yak_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "面会には行かなかった。組の仕事を続けた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "一度だけ面会に行った。それ以来、会っていない。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_04",
                sentence = "親分と盃を交わした。もう後戻りはできない。",
                startAge = 20,
                endAge = 23,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "yak_in" },
                blockedByLifeTags = new() { "yak_grad" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "yak_grad" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_05",
                sentence = "生業がヤクザになった。",
                startAge = 20,
                endAge = 23,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "yak_grad" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                grantsLifeTags = new() { "yak_flav_routine" },
                blockedByLifeTags = new() { "yak_flav_routine" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_yakuza" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_yak_ep1",
                sentence = "組の若い衆がもめた。間に入って収めた。",
                startAge = 21,
                endAge = 55,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_yakuza" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                grantsLifeTags = new() { "yak_flav_episode" },
                blockedByLifeTags = new() { "yak_flav_episode" },
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
                eventId = "ev_yak_yak_ep2",
                sentence = "縄張り内で揉め事があった。相手は引き下がった。",
                startAge = 21,
                endAge = 55,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_yakuza" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                grantsLifeTags = new() { "yak_flav_episode" },
                blockedByLifeTags = new() { "yak_flav_episode" },
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
                eventId = "ev_yak_yak_ep3",
                sentence = "親分の機嫌が悪かった。理由は分からなかった。",
                startAge = 21,
                endAge = 55,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_yakuza" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "warrior_yakuza" },
                grantsLifeTags = new() { "yak_flav_episode" },
                blockedByLifeTags = new() { "yak_flav_episode" },
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
                eventId = "ev_yak_s1",
                sentence = "腕っぷしが必要な場面で、誰も動かなかった。自分が動いた。",
                startAge = 21,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_yakuza" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                grantsLifeTags = new() { "yak_flav_serious" },
                blockedByLifeTags = new() { "yak_flav_serious" },
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
                eventId = "ev_yak_s2",
                sentence = "抗争の後始末が長引いた。疲弊した。",
                startAge = 21,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_yakuza" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                grantsLifeTags = new() { "yak_flav_serious" },
                blockedByLifeTags = new() { "yak_flav_serious" },
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
                eventId = "ev_yak_s3",
                sentence = "組の論理に慣れすぎた自分に気づいた。",
                startAge = 21,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_yakuza" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                grantsLifeTags = new() { "yak_flav_serious" },
                blockedByLifeTags = new() { "yak_flav_serious" },
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
                eventId = "ev_yak_r1a",
                sentence = "組の中で一目置かれるようになった。若い衆を束ねる立場になった。",
                startAge = 21,
                endAge = 51,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_yakuza" },
                blockedByLifeTags = new() { "yak_r1a" },
                grantsLifeTags = new() { "yak_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_r1b",
                sentence = "初めてチームを組んで仕事を仕切った。",
                startAge = 21,
                endAge = 51,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "yak_r1a" },
                blockedByLifeTags = new() { "yak_r1b" },
                grantsLifeTags = new() { "yak_r1b" },
                relatedJobIds = new() { "warrior_yakuza" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "うまくまとめた。兄貴分に認められた。",
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
                        sentence = "まとめきれない部分があった。力でねじ伏せた。",
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
                eventId = "ev_yak_r3a",
                sentence = "若頭に任命された。",
                startAge = 21,
                endAge = 51,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "yak_r1b" },
                blockedByLifeTags = new() { "yak_r3a" },
                grantsLifeTags = new() { "yak_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_r3b",
                sentence = "他組との交渉を単独でまとめた。",
                startAge = 21,
                endAge = 51,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "yak_r3a" },
                blockedByLifeTags = new() { "yak_r3b" },
                grantsLifeTags = new() { "yak_r3b" },
                relatedJobIds = new() { "warrior_yakuza" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_r5a",
                sentence = "組長から後継者として名を挙げられた。",
                startAge = 21,
                endAge = 51,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "yak_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "引き受けることにした。頂点に立つ覚悟を決めた。",
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
                        grantsLifeTags = new() { "yak_r5a_top" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "断った。組を離れることを考え始めた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "yak_r5a_out" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_r5b_a",
                sentence = "組の頂点に立つ準備を始めた。",
                startAge = 21,
                endAge = 51,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "yak_r5a_top" },
                blockedByLifeTags = new() { "yak_r5b" },
                grantsLifeTags = new() { "yak_r5b" },
                relatedJobIds = new() { "warrior_yakuza" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_r5b_b",
                sentence = "組を離れる道を探し始めた。引退は簡単ではなかった。",
                startAge = 21,
                endAge = 51,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "yak_r5a_out" },
                blockedByLifeTags = new() { "yak_r5b" },
                grantsLifeTags = new() { "yak_r5b" },
                relatedJobIds = new() { "warrior_yakuza" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_r7a",
                sentence = "組が揺れる出来事があった。判断を迫られた。",
                startAge = 21,
                endAge = 51,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "yak_r5b" },
                blockedByLifeTags = new() { "yak_r7a" },
                grantsLifeTags = new() { "yak_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yakuza" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_yak_r7b",
                sentence = "組を畳む決断をした日のことは、誰にも話していない。",
                startAge = 21,
                endAge = 51,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "yak_r7a" },
                blockedByLifeTags = new() { "yak_r7b" },
                grantsLifeTags = new() { "yak_r7b" },
                relatedJobIds = new() { "warrior_yakuza" },
            };
        }
        #endregion

        #region 剣術師範 (warrior_kenjutsu)
        private static IEnumerable<ReinLifeEvent> Kenjutsu()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_01",
                sentence = "道場の演武で、老人が木刀を一度振るった。その一振りで風を感じた。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "ken_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_ken_01g" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ken_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_01g",
                editorMemo = "原体験保証",
                sentence = "道場の演武で、老人が木刀を一度振るった。その一振りで風を感じた。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "ken_call" },
                blockedByEventIds = new() { "ev_ken_01" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ken_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_02",
                sentence = "道場に入門した。稽古は素振り1000本から始まった。",
                startAge = 13,
                endAge = 16,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "ken_call" },
                grantsLifeTags = new() { "ken_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "素質があると言われた。型の覚えが早かった。",
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
                        sentence = "毎日通った。それだけだった。",
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
                eventId = "ev_ken_03",
                sentence = "段位審査を受けた。",
                startAge = 17,
                endAge = 22,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "ken_train" },
                blockedByLifeTags = new() { "ken_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "高段位で合格した。師匠が珍しく言葉をかけた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 40, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "ken_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "ken_pass" },
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
                        grantsLifeTags = new() { "ken_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_03b",
                sentence = "もう一度、段位審査を受けた。",
                startAge = 19,
                endAge = 23,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "ken_fail" },
                blockedByLifeTags = new() { "ken_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
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
                        grantsLifeTags = new() { "ken_pass" },
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
                        grantsLifeTags = new() { "ken_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_03c",
                sentence = "段位審査に再挑戦した。ついに合格した。",
                startAge = 24,
                endAge = 24,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "ken_fail" },
                blockedByLifeTags = new() { "ken_pass" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ken_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_04",
                sentence = "師匠から道場を引き継いだ。道場主として剣術を教える立場になった。",
                startAge = 30,
                endAge = 38,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "ken_pass" },
                grantsLifeTags = new() { "ken_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "稽古の組み立てまで任されるようになった。教えることで、自分の構えも変わっていた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 50, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "教える立場になって初めて、自分の癖に気づいた。",
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
                eventId = "ev_ken_06",
                sentence = "生業が剣術師範になった。",
                startAge = 32,
                endAge = 40,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "ken_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_kenjutsu" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_05a",
                sentence = "まだ幼い弟子を初めて預かった。",
                startAge = 34,
                endAge = 70,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kenjutsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                grantsLifeTags = new() { "ken_flav_routine" },
                blockedByLifeTags = new() { "ken_flav_routine" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "一年で弟子の動きが変わった。教えることで自分の構えも変わっていた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 40, weightBonus = 1.5f },
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
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "教えることの難しさに気づいた。言葉にできないことが多すぎた。",
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
                eventId = "ev_ken_ken_ep1",
                sentence = "弟子の一人が試合で接戦を演じた。教えが届いていると感じた。",
                startAge = 34,
                endAge = 70,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kenjutsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                grantsLifeTags = new() { "ken_flav_episode" },
                blockedByLifeTags = new() { "ken_flav_episode" },
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
                eventId = "ev_ken_ken_ep2",
                sentence = "道場の床を磨いていると、弟子が手伝いに来た。",
                startAge = 34,
                endAge = 70,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kenjutsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                grantsLifeTags = new() { "ken_flav_episode" },
                blockedByLifeTags = new() { "ken_flav_episode" },
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
                eventId = "ev_ken_ken_ep3",
                sentence = "稽古で怪我人が出た。自分の指導を振り返った。",
                startAge = 34,
                endAge = 70,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_kenjutsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                grantsLifeTags = new() { "ken_flav_episode" },
                blockedByLifeTags = new() { "ken_flav_episode" },
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
                eventId = "ev_ken_s1",
                sentence = "稽古中、弟子の剣が変わった瞬間があった。",
                startAge = 34,
                endAge = 70,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kenjutsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                grantsLifeTags = new() { "ken_flav_serious" },
                blockedByLifeTags = new() { "ken_flav_serious" },
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
                eventId = "ev_ken_s2",
                sentence = "自分の型が乱れていた。原因が分からなかった。",
                startAge = 34,
                endAge = 70,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kenjutsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                grantsLifeTags = new() { "ken_flav_serious" },
                blockedByLifeTags = new() { "ken_flav_serious" },
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
                eventId = "ev_ken_s3",
                sentence = "無言で弟子と向き合い続けた。言葉より伝わるものがあった。",
                startAge = 34,
                endAge = 70,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kenjutsu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                grantsLifeTags = new() { "ken_flav_serious" },
                blockedByLifeTags = new() { "ken_flav_serious" },
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
                eventId = "ev_ken_r1a",
                sentence = "初めて弟子が大会で入賞した。自分のことのように嬉しかった。",
                startAge = 34,
                endAge = 64,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_kenjutsu" },
                blockedByLifeTags = new() { "ken_r1a" },
                grantsLifeTags = new() { "ken_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_r1b",
                sentence = "道場の評判が地元に広まり、入門希望者が増えた。",
                startAge = 34,
                endAge = 64,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "ken_r1a" },
                blockedByLifeTags = new() { "ken_r1b" },
                grantsLifeTags = new() { "ken_r1b" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "丁寧に選んだ。道場の質を守った。",
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
                        sentence = "全員受け入れた。教える難しさが倍になった。",
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
                eventId = "ev_ken_r3a",
                sentence = "武道連盟から表彰された。",
                startAge = 34,
                endAge = 64,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "ken_r1b" },
                blockedByLifeTags = new() { "ken_r3a" },
                grantsLifeTags = new() { "ken_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_r3b",
                sentence = "他流派の師範と演武交流の機会を持った。刺激を受けた。",
                startAge = 34,
                endAge = 64,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "ken_r3a" },
                blockedByLifeTags = new() { "ken_r3b" },
                grantsLifeTags = new() { "ken_r3b" },
                relatedJobIds = new() { "warrior_kenjutsu" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_r5a",
                sentence = "著書の執筆を依頼された。",
                startAge = 34,
                endAge = 64,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ken_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "受けた。言語化できないと思っていたものを言葉にした。",
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
                        grantsLifeTags = new() { "ken_r5a_write" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "断った。剣は見せるものだと思った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "ken_r5a_deny" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_r5b_a",
                sentence = "書き上げた本が、弟子たちの教材になった。",
                startAge = 34,
                endAge = 64,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ken_r5a_write" },
                blockedByLifeTags = new() { "ken_r5b" },
                grantsLifeTags = new() { "ken_r5b" },
                relatedJobIds = new() { "warrior_kenjutsu" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_r5b_b",
                sentence = "演武で伝え続けた。言葉より深く届くと信じた。",
                startAge = 34,
                endAge = 64,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ken_r5a_deny" },
                blockedByLifeTags = new() { "ken_r5b" },
                grantsLifeTags = new() { "ken_r5b" },
                relatedJobIds = new() { "warrior_kenjutsu" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_r7a",
                sentence = "最後の演武の日取りが決まった。",
                startAge = 34,
                endAge = 64,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "ken_r5b" },
                blockedByLifeTags = new() { "ken_r7a" },
                grantsLifeTags = new() { "ken_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_kenjutsu" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ken_r7b",
                sentence = "道場の床を踏みしめた。ここで何十年も剣を振ってきた。",
                startAge = 34,
                endAge = 64,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "ken_r7a" },
                blockedByLifeTags = new() { "ken_r7b" },
                grantsLifeTags = new() { "ken_r7b" },
                relatedJobIds = new() { "warrior_kenjutsu" },
            };
        }
        #endregion

        #region 傭兵 (warrior_yohei)
        private static IEnumerable<ReinLifeEvent> Yohei()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_01",
                sentence = "爆音と煙の中を動き続ける兵士たちを見た。画面から目が離せなかった。",
                startAge = 10,
                endAge = 15,
                baseWeight = 0.08f,
                blockedByLifeTags = new() { "merc_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_merc_01g" },
                relatedJobIds = new() { "warrior_yohei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "merc_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_01g",
                editorMemo = "原体験保証",
                sentence = "爆音と煙の中を動き続ける兵士たちを見た。画面から目が離せなかった。",
                startAge = 15,
                endAge = 15,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "merc_call" },
                blockedByEventIds = new() { "ev_merc_01" },
                relatedJobIds = new() { "warrior_yohei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "merc_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_02",
                sentence = "規律や銃の扱い、体力を徹底的に体に叩き込まれた。",
                startAge = 18,
                endAge = 20,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "merc_call" },
                blockedByLifeTags = new() { "merc_mil" },
                grantsLifeTags = new() { "merc_mil" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "訓練成績は上位で、狙撃手課程に推薦された。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 35, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 30, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "平均的な成績で訓練を修了した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "下位だった。それでも修了した。",
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
                eventId = "ev_merc_03",
                sentence = "銃声が現実のものとして耳に響いた。",
                startAge = 21,
                endAge = 25,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "merc_mil" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "手は動き、生きて帰った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "手は震えたが、それでも生きて帰った。",
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
                    new ReinSentenceOption
                    {
                        sentence = "動けなかったが、生きて帰った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_04",
                sentence = "除隊後、民間軍事会社（PMC）の採用面接を受けた。",
                startAge = 24,
                endAge = 27,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "merc_mil" },
                blockedByLifeTags = new() { "merc_pmc" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                eventStage = 2,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "迷わず答えた。採用された。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AT, threshold = 50, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 40, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "merc_pmc" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "受け答えはしたが採用されず、別のPMCを探した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "merc_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_04b",
                sentence = "別の民間軍事会社の面接を受けた。",
                startAge = 25,
                endAge = 28,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "merc_fail" },
                blockedByLifeTags = new() { "merc_pmc" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "採用された。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "merc_pmc" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
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
                        grantsLifeTags = new() { "merc_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_04c",
                sentence = "ある小さなPMCが採用してくれた。",
                startAge = 29,
                endAge = 29,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "merc_fail" },
                blockedByLifeTags = new() { "merc_pmc" },
                relatedJobIds = new() { "warrior_yohei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "merc_pmc" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_06",
                sentence = "生業が傭兵になった。",
                startAge = 27,
                endAge = 29,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "merc_pmc" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_yohei" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_05a",
                sentence = "紛争地帯への初派遣。雇い主が誰であろうと、戦場は変わらなかった。",
                startAge = 28,
                endAge = 50,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_yohei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                grantsLifeTags = new() { "merc_flav_routine" },
                blockedByLifeTags = new() { "merc_flav_routine" },
                eventStage = 1,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "任務を完遂した。チームから信頼を得た。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "任務は完遂した。帰りの機内で、ずっと誰とも話さなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                            new StatBonus { stat = StatKind.DF, value = 2 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_05b",
                sentence = "チームの一人が死んだ。翌日、次の任務の指示が来た。",
                startAge = 28,
                endAge = 50,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_yohei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                grantsLifeTags = new() { "merc_flav_routine" },
                blockedByLifeTags = new() { "merc_flav_routine" },
                eventStage = 1,
                statCompareCount = 3,
                statCompareMode = "max",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "翌日の任務で判断は鈍らなかった。生きて帰った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "任務中、一瞬判断が止まった。それでも帰れた。",
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
                eventId = "ev_merc_merc_ep1",
                sentence = "任務前、チームで簡単な打ち合わせをした。全員が無言だった。",
                startAge = 28,
                endAge = 50,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_yohei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                grantsLifeTags = new() { "merc_flav_episode" },
                blockedByLifeTags = new() { "merc_flav_episode" },
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
                eventId = "ev_merc_merc_ep2",
                sentence = "装備の手入れをしながら、何も考えない時間があった。",
                startAge = 28,
                endAge = 50,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_yohei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                grantsLifeTags = new() { "merc_flav_episode" },
                blockedByLifeTags = new() { "merc_flav_episode" },
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
                eventId = "ev_merc_merc_ep3",
                sentence = "帰還後、しばらく何もできなかった。",
                startAge = 28,
                endAge = 50,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_yohei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "warrior_yohei" },
                grantsLifeTags = new() { "merc_flav_episode" },
                blockedByLifeTags = new() { "merc_flav_episode" },
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
                eventId = "ev_merc_s1",
                sentence = "銃声の中でも判断が澄んでいた。",
                startAge = 28,
                endAge = 50,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_yohei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                grantsLifeTags = new() { "merc_flav_serious" },
                blockedByLifeTags = new() { "merc_flav_serious" },
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
                eventId = "ev_merc_s2",
                sentence = "疲労が限界に達していた。それでも任務は続いた。",
                startAge = 28,
                endAge = 50,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_yohei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                grantsLifeTags = new() { "merc_flav_serious" },
                blockedByLifeTags = new() { "merc_flav_serious" },
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
                eventId = "ev_merc_s3",
                sentence = "誰かのために戦っているのか、分からなくなる夜があった。",
                startAge = 28,
                endAge = 50,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_yohei" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                grantsLifeTags = new() { "merc_flav_serious" },
                blockedByLifeTags = new() { "merc_flav_serious" },
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
                eventId = "ev_merc_r1a",
                sentence = "チームリーダーを任された。部下ができた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_yohei" },
                blockedByLifeTags = new() { "merc_r1a" },
                grantsLifeTags = new() { "merc_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_r1b",
                sentence = "困難な任務でチームを率いた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "merc_r1a" },
                blockedByLifeTags = new() { "merc_r1b" },
                grantsLifeTags = new() { "merc_r1b" },
                relatedJobIds = new() { "warrior_yohei" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "全員無事に完遂した。",
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
                        sentence = "一人が負傷した。作戦を見直した。",
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
                eventId = "ev_merc_r3a",
                sentence = "PMC内で指名依頼が増えた。実績が名前を作っていた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "merc_r1b" },
                blockedByLifeTags = new() { "merc_r3a" },
                grantsLifeTags = new() { "merc_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_r3b",
                sentence = "単独指揮の大型任務を完遂した。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "merc_r3a" },
                blockedByLifeTags = new() { "merc_r3b" },
                grantsLifeTags = new() { "merc_r3b" },
                relatedJobIds = new() { "warrior_yohei" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_r5a",
                sentence = "PMC幹部への昇格か、フリーランスへの転向か、選択を迫られた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "merc_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "幹部になった。部下が増えた。判断の重さも増した。",
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
                        grantsLifeTags = new() { "merc_r5a_exec" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "フリーランスになった。案件を自分で選べるようになった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "merc_r5a_free" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_r5b_a",
                sentence = "組織を動かす立場から、戦場を見るようになった。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "merc_r5a_exec" },
                blockedByLifeTags = new() { "merc_r5b" },
                grantsLifeTags = new() { "merc_r5b" },
                relatedJobIds = new() { "warrior_yohei" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_r5b_b",
                sentence = "自分で選んだ任務だけをこなした。それが誇りだった。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "merc_r5a_free" },
                blockedByLifeTags = new() { "merc_r5b" },
                grantsLifeTags = new() { "merc_r5b" },
                relatedJobIds = new() { "warrior_yohei" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_r7a",
                sentence = "戦場を離れる時期が来たと感じた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "merc_r5b" },
                blockedByLifeTags = new() { "merc_r7a" },
                grantsLifeTags = new() { "merc_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_yohei" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_merc_r7b",
                sentence = "最後の任務を終えた。誰も知らない場所で、少しだけ泣いた。",
                startAge = 28,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "merc_r7a" },
                blockedByLifeTags = new() { "merc_r7b" },
                grantsLifeTags = new() { "merc_r7b" },
                relatedJobIds = new() { "warrior_yohei" },
            };
        }
        #endregion

        #region 宇宙飛行士 (warrior_uchu)
        private static IEnumerable<ReinLifeEvent> Uchu()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_prev_a",
                sentence = "会社に就職した。",
                startAge = 22,
                endAge = 27,
                baseWeight = 0.5f,
                blockedByLifeTags = new() { "uchu_work" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_uchu_prev_g" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_work" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_prev_b",
                sentence = "大学院へ進学し、研究者の道に進んだ。",
                startAge = 22,
                endAge = 29,
                baseWeight = 0.4f,
                blockedByLifeTags = new() { "uchu_work" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_uchu_prev_g" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_work" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_prev_c",
                sentence = "自衛隊に入隊し、厳しい訓練を受けた。",
                startAge = 18,
                endAge = 24,
                baseWeight = 0.45f,
                blockedByLifeTags = new() { "uchu_work" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_uchu_prev_g" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_work" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_ep1",
                sentence = "休日も返上して打ち込んでいた。気づけばそれが当たり前になっていた。",
                startAge = 22,
                endAge = 34,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "uchu_work" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                grantsLifeTags = new() { "uchu_flav_episode" },
                blockedByLifeTags = new() { "uchu_flav_episode" },
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
                eventId = "ev_uchu_ep2",
                sentence = "担当した仕事が予想以上の成果を出した。",
                startAge = 22,
                endAge = 34,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "uchu_work" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                grantsLifeTags = new() { "uchu_flav_episode" },
                blockedByLifeTags = new() { "uchu_flav_episode" },
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
                eventId = "ev_uchu_ep3",
                sentence = "限界まで追い込まれたが、それでもやめなかった。",
                startAge = 22,
                endAge = 34,
                baseWeight = 0.033f,
                requiresAnyLifeTag = new() { "uchu_work" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "-" },
                relatedJobIds = new() { "warrior_uchu" },
                grantsLifeTags = new() { "uchu_flav_episode" },
                blockedByLifeTags = new() { "uchu_flav_episode" },
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
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = -1 },
                            new StatBonus { stat = StatKind.AGI, value = -2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_02",
                sentence = "宇宙飛行士の募集要項を見つけた。応募することにした。",
                startAge = 25,
                endAge = 33,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "uchu_work" },
                blockedByLifeTags = new() { "uchu_apply" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_uchu_02g" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_apply" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_03",
                sentence = "宇宙飛行士候補者選抜試験を通過した。",
                startAge = 27,
                endAge = 37,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "uchu_apply" },
                blockedByLifeTags = new() { "uchu_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_uchu_03g" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_04",
                sentence = "生業が宇宙飛行士になった。",
                startAge = 34,
                endAge = 43,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "uchu_pass" },
                blockedByLifeTags = new() { "job_uchu" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                blockedByEventIds = new() { "ev_uchu_04g" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_uchu" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r1a",
                sentence = "初めてのミッションが決まり、長い訓練が始まった。",
                startAge = 34,
                endAge = 53,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_uchu" },
                blockedByLifeTags = new() { "uchu_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_r1a" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r1b",
                sentence = "打ち上げ当日、ロケットのエンジン音が全身に響いた。",
                startAge = 34,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "uchu_r1a" },
                blockedByLifeTags = new() { "uchu_r1b" },
                grantsLifeTags = new() { "space_m1" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_r1b" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r1c",
                sentence = "宇宙から地球を見た。言葉が出なかった。",
                startAge = 34,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "uchu_r1b" },
                blockedByLifeTags = new() { "uchu_r1c", "earth_m1" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_r1c" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r3a",
                sentence = "ISSでのシステム障害が発生した。マニュアルにない対処が必要だった。",
                startAge = 34,
                endAge = 53,
                baseWeight = 0.75f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "space_m2" },
                blockedByLifeTags = new() { "earth_m2" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "判断し問題を解決した。地上管制は言葉を失った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 52, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.AGI, threshold = 46, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 40, weightBonus = 0.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 8 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "uchu_r3_solve" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 5 },
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "地上と連携しながら対処した。時間はかかったが、解決した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "uchu_r3_team" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r3b_solve",
                sentence = "帰還後、NASA長官から直接連絡が来た。",
                startAge = 34,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "uchu_r3_solve" },
                blockedByLifeTags = new() { "uchu_r3b", "space_m1", "space_m2", "space_m3" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_r3b" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r3b_team",
                sentence = "地上管制のオペレーターに礼を言いに行った。相手は驚いていた。",
                startAge = 34,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "uchu_r3_team" },
                blockedByLifeTags = new() { "uchu_r3b", "space_m1", "space_m2", "space_m3" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_r3b" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r5a",
                sentence = "船外活動を任された。宇宙服の中で、呼吸だけが聞こえた。",
                startAge = 34,
                endAge = 53,
                baseWeight = 0.75f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "space_m3" },
                blockedByLifeTags = new() { "uchu_r5a", "earth_m3" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_r5a" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r5b",
                sentence = "二度目のミッションに指揮官として選ばれた。",
                startAge = 34,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "uchu_r5a" },
                blockedByLifeTags = new() { "uchu_r5b", "space_m1", "space_m2", "space_m3" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_r5b" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r7a",
                sentence = "月探査計画の候補者リストに名前が入った。",
                startAge = 34,
                endAge = 53,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "uchu_r5b" },
                blockedByLifeTags = new() { "uchu_r7a", "space_m1", "space_m2", "space_m3" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                eventStage = 5,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_r7a" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r7b",
                sentence = "月面に立った。空が黒かった。地球が青かった。それ以上は何もいらなかった。",
                startAge = 34,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "uchu_r7a" },
                blockedByLifeTags = new() { "uchu_r7b", "space_m1", "space_m2", "space_m3" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_r7b" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_s1",
                sentence = "訓練中に体調を崩した。ミッションから外れかけたが、なんとか戻った。",
                startAge = 34,
                endAge = 58,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_uchu" },
                blockedByLifeTags = new() { "space_m1", "space_m2", "space_m3", "uchu_flav_serious" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                grantsLifeTags = new() { "uchu_flav_serious" },
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
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_s2",
                sentence = "宇宙から家族に手紙を書いた。地上での日常がひどく遠く感じた。",
                startAge = 34,
                endAge = 58,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "space_m1" },
                blockedByLifeTags = new() { "earth_m1", "earth_m2", "earth_m3", "uchu_flav_serious" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.DF, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                grantsLifeTags = new() { "uchu_flav_serious" },
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
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_s3",
                sentence = "学校で講演を頼まれた。子供たちの目は真剣だった。",
                startAge = 34,
                endAge = 58,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "earth_m1" },
                blockedByLifeTags = new() { "space_m1", "space_m2", "space_m3", "uchu_flav_serious" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                grantsLifeTags = new() { "uchu_flav_serious" },
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
                eventId = "ev_uchu_s4",
                sentence = "無重力の中で眠れない夜があった。地球の写真を見ていた。",
                startAge = 34,
                endAge = 58,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "space_m1" },
                blockedByLifeTags = new() { "earth_m1", "earth_m2", "earth_m3", "uchu_flav_serious" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "-" },
                relatedJobIds = new() { "warrior_uchu" },
                grantsLifeTags = new() { "uchu_flav_serious" },
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
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_s5",
                sentence = "帰還後、初めて歩いた一歩が重かった。地球の重力を、体が忘れかけていた。",
                startAge = 34,
                endAge = 58,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "uchu_returned" },
                blockedByLifeTags = new() { "uchu_s5_done", "uchu_flav_serious" },
                grantsLifeTags = new() { "uchu_s5_done", "uchu_flav_serious" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
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
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_02g",
                sentence = "宇宙飛行士の募集要項を見つけた。応募することにした。",
                startAge = 34,
                endAge = 34,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "uchu_work" },
                blockedByLifeTags = new() { "uchu_apply" },
                blockedByEventIds = new() { "ev_uchu_02" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_apply" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_03g",
                sentence = "宇宙飛行士候補者選抜試験を通過した。",
                startAge = 38,
                endAge = 38,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "uchu_apply" },
                blockedByLifeTags = new() { "uchu_pass" },
                blockedByEventIds = new() { "ev_uchu_03" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_04g",
                sentence = "生業が宇宙飛行士になった。",
                startAge = 44,
                endAge = 44,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "uchu_pass" },
                blockedByLifeTags = new() { "job_uchu" },
                blockedByEventIds = new() { "ev_uchu_04" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_uchu" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_prev_g",
                sentence = "会社に就職した。",
                startAge = 29,
                endAge = 29,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "uchu_work" },
                blockedByEventIds = new() { "ev_uchu_prev_a", "ev_uchu_prev_b", "ev_uchu_prev_c" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "uchu_work" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r1_return",
                sentence = "地球に帰還した。",
                startAge = 34,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "uchu_r1c" },
                blockedByLifeTags = new() { "earth_m1" },
                grantsLifeTags = new() { "earth_m1", "uchu_returned" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
                        },
                        grantsLifeTags = new() { "earth_m1", "uchu_returned" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r2_launch",
                sentence = "二度目のミッションへ向けて打ち上げられた。",
                startAge = 34,
                endAge = 58,
                baseWeight = 0.85f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "earth_m1" },
                blockedByLifeTags = new() { "space_m2" },
                grantsLifeTags = new() { "space_m2" },
                minYearsAfterEvents = new()
                {
                    new MinYearsAfterEntry { eventId = "earth_m1", minYears = 2 },
                },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "space_m2" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r2_return",
                sentence = "地球に帰還した。",
                startAge = 34,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "space_m2" },
                blockedByLifeTags = new() { "earth_m2" },
                grantsLifeTags = new() { "earth_m2", "uchu_returned" },
                minYearsAfterEvents = new()
                {
                    new MinYearsAfterEntry { eventId = "space_m2", minYears = 1 },
                },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
                        },
                        grantsLifeTags = new() { "earth_m2", "uchu_returned" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r3_launch",
                sentence = "三度目のミッションへ向けて打ち上げられた。",
                startAge = 34,
                endAge = 58,
                baseWeight = 0.85f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "earth_m2" },
                blockedByLifeTags = new() { "space_m3" },
                grantsLifeTags = new() { "space_m3" },
                minYearsAfterEvents = new()
                {
                    new MinYearsAfterEntry { eventId = "earth_m2", minYears = 2 },
                },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "warrior_uchu" },
                eventStage = 4,
                statCompareCount = 2,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "space_m3" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_uchu_r3_return",
                sentence = "地球に帰還した。",
                startAge = 34,
                endAge = 58,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "space_m3" },
                blockedByLifeTags = new() { "earth_m3" },
                grantsLifeTags = new() { "earth_m3", "uchu_returned" },
                minYearsAfterEvents = new()
                {
                    new MinYearsAfterEntry { eventId = "space_m3", minYears = 2 },
                },
                relatedJobIds = new() { "warrior_uchu" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
                        },
                        grantsLifeTags = new() { "earth_m3", "uchu_returned" },
                    },
                },
            };
        }
        #endregion

    }
}
