using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // Mage 傾向の生業イベント
    // ================================================================
    // 手品師 / 占い師 / 僧侶 / 音楽家 / 研究者 / カルト教祖
    //
    // ※ このファイルは MigrateReinEventsToCs ツールで自動生成されました。
    //    手動編集してOKです。Claudeと相談しながら追加・変更する想定。
    // ================================================================
    public static class Mage_Events
    {
        public static IEnumerable<ReinLifeEvent> All()
        {
            foreach (var ev in Tejinashi()) yield return ev;
            foreach (var ev in Uranai()) yield return ev;
            foreach (var ev in Sou()) yield return ev;
            foreach (var ev in Ongakuka()) yield return ev;
            foreach (var ev in Kenkyusha()) yield return ev;
            foreach (var ev in Kult()) yield return ev;
        }

        #region 手品師 (mage_tejinashi)
        private static IEnumerable<ReinLifeEvent> Tejinashi()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_01",
                sentence = "街角のマジシャンがコインを消した。種が知りたくてその場で30分ほど観察したが、分からなかった。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "magic_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                blockedByEventIds = new() { "ev_magic_01g" },
                relatedJobIds = new() { "mage_tejinashi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "magic_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_01g",
                editorMemo = "原体験保証",
                sentence = "種を見抜こうと30分ほど観察したが、結局分からなかった。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "magic_call" },
                blockedByEventIds = new() { "ev_magic_01" },
                relatedJobIds = new() { "mage_tejinashi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "magic_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_02",
                sentence = "マジックの本を買い、独習を始めた。コインマジックだけで100時間以上練習した。",
                startAge = 13,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "magic_call" },
                grantsLifeTags = new() { "magic_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "指の動きが異常に速くなり、鏡で見ても分からなくなった。",
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
                        sentence = "何度やっても、見切られた。",
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
                eventId = "ev_magic_03",
                sentence = "マジックのオーディションを受けた。プロのパフォーマーを前に演じた。",
                startAge = 18,
                endAge = 23,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "magic_train" },
                blockedByLifeTags = new() { "magic_stage" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "通過した。出演枠を与えられた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 42, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "magic_stage" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "落ちた。何が足りなかったのかを聞き、また練習した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "magic_keep" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_03b",
                sentence = "別の舞台やイベントにオーディションを再挑戦した。",
                startAge = 20,
                endAge = 25,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "magic_keep" },
                blockedByLifeTags = new() { "magic_stage" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "採用された。小さなステージだった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "magic_stage" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = 1 },
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
                        grantsLifeTags = new() { "magic_keep" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_03c",
                sentence = "小さなイベントのステージに出演者として呼ばれた。",
                startAge = 26,
                endAge = 26,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "magic_keep" },
                blockedByLifeTags = new() { "magic_stage" },
                relatedJobIds = new() { "mage_tejinashi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "magic_stage" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_04a",
                sentence = "手品師としてステージに立つようになった。",
                startAge = 25,
                endAge = 65,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "job_tejinashi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
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
                eventId = "ev_magic_04",
                sentence = "生業が手品師になった。",
                startAge = 24,
                endAge = 27,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "magic_stage" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_tejinashi" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_magic_ep1",
                sentence = "ステージ後、子供が目を輝かせて近づいてきた。",
                startAge = 25,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_tejinashi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
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
                eventId = "ev_magic_magic_ep2",
                sentence = "新しいトリックのアイデアが浮かんだ。三日間練習した。",
                startAge = 25,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_tejinashi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
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
                eventId = "ev_magic_magic_ep3",
                sentence = "本番で手が滑った。誰も気づかなかった。",
                startAge = 25,
                endAge = 65,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_tejinashi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "-" },
                relatedJobIds = new() { "mage_tejinashi" },
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
                eventId = "ev_magic_s1",
                sentence = "観客の集中が高まる瞬間が分かった。そこを狙った。",
                startAge = 25,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_tejinashi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
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
                eventId = "ev_magic_s2",
                sentence = "指先の感覚がいつもと違った。調整に時間がかかった。",
                startAge = 25,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_tejinashi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
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
                eventId = "ev_magic_s3",
                sentence = "ステージの上で、時間の流れが変わる瞬間があった。",
                startAge = 25,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_tejinashi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
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
                eventId = "ev_magic_r1a",
                sentence = "テレビ出演のオファーが来た。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_tejinashi" },
                blockedByLifeTags = new() { "magic_r1a" },
                grantsLifeTags = new() { "magic_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_r1b",
                sentence = "生放送でマジックを披露した。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "magic_r1a" },
                blockedByLifeTags = new() { "magic_r1b" },
                grantsLifeTags = new() { "magic_r1b" },
                relatedJobIds = new() { "mage_tejinashi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "完璧に決まった。視聴率が話題になった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "小さなミスがあった。気づいたのは自分だけだった。",
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
                eventId = "ev_magic_r3a",
                sentence = "世界マジック選手権への出場権を得た。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "magic_r1b" },
                blockedByLifeTags = new() { "magic_r3a" },
                grantsLifeTags = new() { "magic_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_r3b",
                sentence = "国際大会で入賞した。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "magic_r3a" },
                blockedByLifeTags = new() { "magic_r3b" },
                grantsLifeTags = new() { "magic_r3b" },
                relatedJobIds = new() { "mage_tejinashi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_r5a",
                sentence = "大きな舞台公演を打つか、テレビ専門に徹するか。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "magic_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "大きな舞台に立った。生のお客さんに向けて演じた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "magic_r5a_stage" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "テレビに特化した。カメラの前でしか出せない技があった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "magic_r5a_tv" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_r5b_a",
                sentence = "劇場を満席にする手品師として名を知られるようになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "magic_r5a_stage" },
                blockedByLifeTags = new() { "magic_r5b" },
                grantsLifeTags = new() { "magic_r5b" },
                relatedJobIds = new() { "mage_tejinashi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_r5b_b",
                sentence = "テレビを通じて、何百万人もの前で演じ続けた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "magic_r5a_tv" },
                blockedByLifeTags = new() { "magic_r5b" },
                grantsLifeTags = new() { "magic_r5b" },
                relatedJobIds = new() { "mage_tejinashi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_r7a",
                sentence = "若い手品師たちから師事を求められるようになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "magic_r5b" },
                blockedByLifeTags = new() { "magic_r7a" },
                grantsLifeTags = new() { "magic_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_tejinashi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_magic_r7b",
                sentence = "最後のステージで、一番好きな手品を最初に演じた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "magic_r7a" },
                blockedByLifeTags = new() { "magic_r7b" },
                grantsLifeTags = new() { "magic_r7b" },
                relatedJobIds = new() { "mage_tejinashi" },
            };
        }
        #endregion

        #region 占い師 (mage_uranai)
        private static IEnumerable<ReinLifeEvent> Uranai()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_01",
                sentence = "夢で見たことが、翌日本当に起きるということがよくあった。",
                startAge = 8,
                endAge = 14,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "fort_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                blockedByEventIds = new() { "ev_fort_01g" },
                relatedJobIds = new() { "mage_uranai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "fort_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_01g",
                editorMemo = "原体験保証",
                sentence = "夢で見たことが、翌日本当に起きるということがよくあった。",
                startAge = 14,
                endAge = 14,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "fort_call" },
                blockedByEventIds = new() { "ev_fort_01" },
                relatedJobIds = new() { "mage_uranai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "fort_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_02",
                sentence = "とある占い師に出会った。全てを見通されているかのような感覚に陥った。運命を感じて弟子入りした。",
                startAge = 16,
                endAge = 20,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "fort_call" },
                grantsLifeTags = new() { "fort_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "師匠はただ頷いた。何かを確かめるような目だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "本当に弟子にしてもらえるのか、実感が湧かなかった。",
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
                eventId = "ev_fort_03",
                sentence = "師匠から独立を勧められた。小さなスペースで鑑定を始めた。",
                startAge = 20,
                endAge = 25,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "fort_trained" },
                grantsLifeTags = new() { "fort_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "口コミで予約が入り始め、3ヶ月待ちになった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "最初は閑散としていた。少しずつ客が来た。",
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
                eventId = "ev_fort_04a",
                sentence = "占い師として鑑定を続けるようになった。",
                startAge = 25,
                endAge = 70,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "job_uranai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
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
                eventId = "ev_fort_04",
                sentence = "生業が占い師になった。",
                startAge = 24,
                endAge = 26,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "fort_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_uranai" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_02b",
                sentence = "師匠のもとで修行を続けた。感覚を磨くだけでなく、技術として体系化する難しさを知った。",
                startAge = 17,
                endAge = 22,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "fort_train" },
                blockedByLifeTags = new() { "fort_trained" },
                grantsLifeTags = new() { "fort_trained" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "感覚と技術が繋がる瞬間があった。師匠が頷いた。",
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
                        sentence = "感覚と技術が噛み合わなかった。",
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
                eventId = "ev_fort_fort_ep1",
                sentence = "依頼人の言葉の裏に、言えないものを感じた。",
                startAge = 25,
                endAge = 70,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_uranai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
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
                eventId = "ev_fort_fort_ep2",
                sentence = "予約が埋まり続けた。断ることが増えた。",
                startAge = 25,
                endAge = 70,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_uranai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
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
                eventId = "ev_fort_fort_ep3",
                sentence = "外れた鑑定のことを、しばらく考えた。",
                startAge = 25,
                endAge = 70,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_uranai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "-" },
                relatedJobIds = new() { "mage_uranai" },
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
                eventId = "ev_fort_s1",
                sentence = "依頼人の本質が、最初の一言で見えることがあった。",
                startAge = 25,
                endAge = 70,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_uranai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
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
                eventId = "ev_fort_s2",
                sentence = "何も見えない日があった。それでも向き合った。",
                startAge = 25,
                endAge = 70,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_uranai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
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
                eventId = "ev_fort_s3",
                sentence = "鑑定が相手の人生を動かしたと後から聞いた。",
                startAge = 25,
                endAge = 70,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_uranai" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
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
                eventId = "ev_fort_r1a",
                sentence = "テレビや雑誌に取り上げられるようになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_uranai" },
                blockedByLifeTags = new() { "fort_r1a" },
                grantsLifeTags = new() { "fort_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_r1b",
                sentence = "予約が半年待ちになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "fort_r1a" },
                blockedByLifeTags = new() { "fort_r1b" },
                grantsLifeTags = new() { "fort_r1b" },
                relatedJobIds = new() { "mage_uranai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "丁寧に一人一人と向き合い続けた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "増えすぎた依頼に追われた。",
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
                eventId = "ev_fort_r3a",
                sentence = "占い師として本を出した。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "fort_r1b" },
                blockedByLifeTags = new() { "fort_r3a" },
                grantsLifeTags = new() { "fort_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_r3b",
                sentence = "政財界の人間が密かに相談に来るようになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "fort_r3a" },
                blockedByLifeTags = new() { "fort_r3b" },
                grantsLifeTags = new() { "fort_r3b" },
                relatedJobIds = new() { "mage_uranai" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_r5a",
                sentence = "学校を開いて後進を育てるか、鑑定専業を続けるか。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "fort_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "学校を開いた。自分の見方を伝えることにした。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "fort_r5a_school" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "鑑定だけを続けた。一対一で向き合うことが自分の仕事だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "fort_r5a_solo" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_r5b_a",
                sentence = "多くの占い師を育て、業界に影響を与えるようになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "fort_r5a_school" },
                blockedByLifeTags = new() { "fort_r5b" },
                grantsLifeTags = new() { "fort_r5b" },
                relatedJobIds = new() { "mage_uranai" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_r5b_b",
                sentence = "一人一人の鑑定に、生涯をかけた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "fort_r5a_solo" },
                blockedByLifeTags = new() { "fort_r5b" },
                grantsLifeTags = new() { "fort_r5b" },
                relatedJobIds = new() { "mage_uranai" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_r7a",
                sentence = "長年の依頼者が「あなたのおかげで」と言いに来た。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "fort_r5b" },
                blockedByLifeTags = new() { "fort_r7a" },
                grantsLifeTags = new() { "fort_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_uranai" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_fort_r7b",
                sentence = "最後の鑑定を終えた日、手相が自分のものに見えなかった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "fort_r7a" },
                blockedByLifeTags = new() { "fort_r7b" },
                grantsLifeTags = new() { "fort_r7b" },
                relatedJobIds = new() { "mage_uranai" },
            };
        }
        #endregion

        #region 僧侶 (mage_sou)
        private static IEnumerable<ReinLifeEvent> Sou()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_01",
                sentence = "祖母の葬儀で僧侶の読経を聞いた。読経が終わると、部屋の空気が変わったように感じられた。",
                startAge = 8,
                endAge = 14,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "sou_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                blockedByEventIds = new() { "ev_sou_01g" },
                relatedJobIds = new() { "mage_sou" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sou_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_01g",
                editorMemo = "原体験保証",
                sentence = "祖母の葬儀で僧侶の読経を聞いた。読経が終わると、部屋の空気が変わったように感じられた。",
                startAge = 14,
                endAge = 14,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "sou_call" },
                blockedByEventIds = new() { "ev_sou_01" },
                relatedJobIds = new() { "mage_sou" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "sou_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_02",
                sentence = "寺に入り、修行を始めた。得度の前に1年間、雑巾がけと読経だけの生活があった。",
                startAge = 17,
                endAge = 20,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "sou_call" },
                grantsLifeTags = new() { "sou_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "何も考えない時間が苦でなかった。師僧が少しずつ話しかけるようになった。",
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
                        sentence = "最初は雑務ばかりだった。",
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
                eventId = "ev_sou_03",
                sentence = "師僧から「よく来たな」と言われた。",
                startAge = 20,
                endAge = 22,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "sou_train" },
                grantsLifeTags = new() { "sou_tokudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MDF, threshold = 40, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 30, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "儀式は静かに終わった。翌日から修行の内容が変わった。",
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
                eventId = "ev_sou_04a",
                sentence = "初めて一人で葬儀の読経を担当した。",
                startAge = 22,
                endAge = 26,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "sou_tokudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "最後まで上手く唱えることができた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "ところどころで詰まるところがあった。",
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
                eventId = "ev_sou_05",
                sentence = "生業が僧侶になった。",
                startAge = 24,
                endAge = 26,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "sou_tokudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_sou" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_sou_ep1",
                sentence = "葬儀の後、遺族が深く頭を下げた。言葉はいらなかった。",
                startAge = 25,
                endAge = 75,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_sou" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
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
                eventId = "ev_sou_sou_ep2",
                sentence = "早朝の読経中、境内が静かだった。",
                startAge = 25,
                endAge = 75,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_sou" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
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
                eventId = "ev_sou_sou_ep3",
                sentence = "若い檀家が悩みを打ち明けてきた。ただ聞いた。",
                startAge = 25,
                endAge = 75,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_sou" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "-" },
                relatedJobIds = new() { "mage_sou" },
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
                eventId = "ev_sou_s1",
                sentence = "読経の中で、何かが整っていく感覚があった。",
                startAge = 25,
                endAge = 75,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_sou" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
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
                eventId = "ev_sou_s2",
                sentence = "自分の迷いが、声に出ていた気がした。",
                startAge = 25,
                endAge = 75,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_sou" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
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
                eventId = "ev_sou_s3",
                sentence = "問いに答えられなかった。しかしそれでいいと思えた。",
                startAge = 25,
                endAge = 75,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_sou" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
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
                eventId = "ev_sou_r1a",
                sentence = "住職を補佐する立場になった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_sou" },
                blockedByLifeTags = new() { "sou_r1a" },
                grantsLifeTags = new() { "sou_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_r1b",
                sentence = "法要の導師を任されるようになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "sou_r1a" },
                blockedByLifeTags = new() { "sou_r1b" },
                grantsLifeTags = new() { "sou_r1b" },
                relatedJobIds = new() { "mage_sou" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "読経の声が安定してきた。師僧が頷いた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "まだ未熟さを感じる場面があった。",
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
                eventId = "ev_sou_r3a",
                sentence = "地域の寺院連合で役を担うようになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "sou_r1b" },
                blockedByLifeTags = new() { "sou_r3a" },
                grantsLifeTags = new() { "sou_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_r3b",
                sentence = "遠方から法話を聞きに来る人が増えた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "sou_r3a" },
                blockedByLifeTags = new() { "sou_r3b" },
                grantsLifeTags = new() { "sou_r3b" },
                relatedJobIds = new() { "mage_sou" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_r5a",
                sentence = "住職を継ぐか、修行を続けるか。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "sou_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "住職を継いだ。寺を守ることが自分の役目になった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "sou_r5a_juushoku" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "修行を続けた。まだ自分には早いと思った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "sou_r5a_train" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_r5b_a",
                sentence = "住職として、寺と地域を守り続けた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "sou_r5a_juushoku" },
                blockedByLifeTags = new() { "sou_r5b" },
                grantsLifeTags = new() { "sou_r5b" },
                relatedJobIds = new() { "mage_sou" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_r5b_b",
                sentence = "修行の深みに入った。言葉では伝えられないものが見えてきた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "sou_r5a_train" },
                blockedByLifeTags = new() { "sou_r5b" },
                grantsLifeTags = new() { "sou_r5b" },
                relatedJobIds = new() { "mage_sou" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_r7a",
                sentence = "長く通い続けた檀家が亡くなった。葬儀で読経した。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "sou_r5b" },
                blockedByLifeTags = new() { "sou_r7a" },
                grantsLifeTags = new() { "sou_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_sou" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_sou_r7b",
                sentence = "最後の法話の日、境内が静かだった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "sou_r7a" },
                blockedByLifeTags = new() { "sou_r7b" },
                grantsLifeTags = new() { "sou_r7b" },
                relatedJobIds = new() { "mage_sou" },
            };
        }
        #endregion

        #region 音楽家 (mage_ongakuka)
        private static IEnumerable<ReinLifeEvent> Ongakuka()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_01",
                sentence = "コンサートで聴いた一音が耳に残った。家に帰ってから何時間も同じ音を鼻歌で繰り返していた。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "ong_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                blockedByEventIds = new() { "ev_ong_01g" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ong_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_01g",
                editorMemo = "原体験保証",
                sentence = "コンサートで聴いた一音が耳に残った。家に帰ってから何時間も同じ音を鼻歌で繰り返していた。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "ong_call" },
                blockedByEventIds = new() { "ev_ong_01" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ong_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_02",
                sentence = "楽器を始めた。音楽学校に通い、毎日何時間も練習した。",
                startAge = 13,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "ong_call" },
                grantsLifeTags = new() { "ong_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "音感が良いと言われた。アンサンブルで役割を与えられた。",
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
                        sentence = "上手くならないまま、時間が過ぎた。",
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
                eventId = "ev_ong_03",
                sentence = "コンクールやオーディションに出場した。",
                startAge = 18,
                endAge = 24,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "ong_train" },
                blockedByLifeTags = new() { "ong_notice" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "入賞した。審査員のコメントが冊子に載った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 48, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.MDF, threshold = 36, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "ong_notice" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "予選で落ちた。何が足りないか分析して、また練習した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "ong_keep" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_03b",
                sentence = "別のコンクールやオーディションに再挑戦した。",
                startAge = 21,
                endAge = 26,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "ong_keep" },
                blockedByLifeTags = new() { "ong_notice" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "入選した。演奏の機会を得た。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "ong_notice" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
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
                        grantsLifeTags = new() { "ong_keep" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_03c",
                sentence = "演奏の場を得た。",
                startAge = 27,
                endAge = 27,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "ong_keep" },
                blockedByLifeTags = new() { "ong_notice" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "ong_notice" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_04",
                sentence = "レコーディングまたは本格的なコンサート出演が決まった。",
                startAge = 24,
                endAge = 28,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "ong_notice" },
                grantsLifeTags = new() { "ong_debut" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "初日のステージが終わった後、主催者から次の依頼が来た。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "終演後、客席の拍手が聞こえた。それだけで十分だった。",
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
                eventId = "ev_ong_06",
                sentence = "生業が音楽家になった。",
                startAge = 25,
                endAge = 30,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "ong_debut" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_ongakuka" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_05a",
                sentence = "スランプが来た。何を弾いても音が死んでいた。",
                startAge = 26,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_ongakuka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "3ヶ月楽器を触らなかった。再び触ったとき、音が素直に出た。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "それでも、毎日弾いた。出口が見つかるまで続けた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 2 },
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_ong_ep1",
                sentence = "本番前、舞台袖で深呼吸した。それだけで全てが整った。",
                startAge = 26,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_ongakuka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
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
                eventId = "ev_ong_ong_ep2",
                sentence = "共演者の演奏に引っ張られて、自分の音が変わった。",
                startAge = 26,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_ongakuka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
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
                eventId = "ev_ong_ong_ep3",
                sentence = "スランプの時期、楽器を持てない夜があった。",
                startAge = 26,
                endAge = 65,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_ongakuka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "-" },
                relatedJobIds = new() { "mage_ongakuka" },
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
                eventId = "ev_ong_s1",
                sentence = "演奏中、会場全体が息を飲む瞬間があった。",
                startAge = 26,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_ongakuka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
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
                eventId = "ev_ong_s2",
                sentence = "何を弾いても、昨日より良くならなかった。",
                startAge = 26,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_ongakuka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
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
                eventId = "ev_ong_s3",
                sentence = "音楽と自分が一致する瞬間があった。",
                startAge = 26,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_ongakuka" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
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
                eventId = "ev_ong_r1a",
                sentence = "単独でのコンサートツアーが組まれた。",
                startAge = 26,
                endAge = 56,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_ongakuka" },
                blockedByLifeTags = new() { "ong_r1a" },
                grantsLifeTags = new() { "ong_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_r1b",
                sentence = "初のソロアルバムがリリースされた。",
                startAge = 26,
                endAge = 56,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "ong_r1a" },
                blockedByLifeTags = new() { "ong_r1b" },
                grantsLifeTags = new() { "ong_r1b" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "評論家から高い評価を得た。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "売れ行きより先に、自分が満足できる作品を作れた。",
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
                eventId = "ev_ong_r3a",
                sentence = "国際的な音楽祭への招待が来た。",
                startAge = 26,
                endAge = 56,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "ong_r1b" },
                blockedByLifeTags = new() { "ong_r3a" },
                grantsLifeTags = new() { "ong_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_r3b",
                sentence = "後輩ミュージシャンとのコラボ作品が注目された。",
                startAge = 26,
                endAge = 56,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "ong_r3a" },
                blockedByLifeTags = new() { "ong_r3b" },
                grantsLifeTags = new() { "ong_r3b" },
                relatedJobIds = new() { "mage_ongakuka" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_r5a",
                sentence = "音楽学校で教えるか、演奏家として活動を続けるか。",
                startAge = 26,
                endAge = 56,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ong_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "教職を引き受けた。音楽を渡すことが演奏の延長だと思った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "ong_r5a_teach" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "演奏を続けた。音楽は作るものではなく、生きるものだと思った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "ong_r5a_play" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_r5b_a",
                sentence = "次の世代の演奏家たちを育て、音楽を繋いだ。",
                startAge = 26,
                endAge = 56,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ong_r5a_teach" },
                blockedByLifeTags = new() { "ong_r5b" },
                grantsLifeTags = new() { "ong_r5b" },
                relatedJobIds = new() { "mage_ongakuka" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_r5b_b",
                sentence = "演奏し続けた。舞台に立つことが自分の存在証明だった。",
                startAge = 26,
                endAge = 56,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "ong_r5a_play" },
                blockedByLifeTags = new() { "ong_r5b" },
                grantsLifeTags = new() { "ong_r5b" },
                relatedJobIds = new() { "mage_ongakuka" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_r7a",
                sentence = "引退コンサートの日程が決まった。",
                startAge = 26,
                endAge = 56,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "ong_r5b" },
                blockedByLifeTags = new() { "ong_r7a" },
                grantsLifeTags = new() { "ong_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_ongakuka" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_ong_r7b",
                sentence = "最後の音が消えたあと、しばらく誰も動かなかった。",
                startAge = 26,
                endAge = 56,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "ong_r7a" },
                blockedByLifeTags = new() { "ong_r7b" },
                grantsLifeTags = new() { "ong_r7b" },
                relatedJobIds = new() { "mage_ongakuka" },
            };
        }
        #endregion

        #region 研究者 (mage_kenkyusha)
        private static IEnumerable<ReinLifeEvent> Kenkyusha()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_01",
                sentence = "大人が答えられないことを図書館で調べ続ける子供だった。答えを見つけるたびに、次の疑問が生まれた。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "res_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                blockedByEventIds = new() { "ev_res_01g" },
                relatedJobIds = new() { "mage_kenkyusha" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "res_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_01g",
                editorMemo = "原体験保証",
                sentence = "大人が答えられないことを図書館で調べ続ける子供だった。答えを見つけるたびに、次の疑問が生まれた。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "res_call" },
                blockedByEventIds = new() { "ev_res_01" },
                relatedJobIds = new() { "mage_kenkyusha" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "res_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_02",
                sentence = "理系進学を決め、高校でも実験や調べ物にのめり込んだ。研究者になりたい気持ちが固まっていった。",
                startAge = 16,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "res_call" },
                grantsLifeTags = new() { "res_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "先生から「大学でも研究を続けろ」と言われた。",
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
                        sentence = "思うような結果は出なかったが、進学の意思は揺らがなかった。",
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
                eventId = "ev_res_03",
                sentence = "3年間、論文は何度もリジェクトされた。",
                startAge = 22,
                endAge = 26,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "res_train" },
                grantsLifeTags = new() { "res_phd" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "それでも改訂を重ね、査読者の指摘を全部潰した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "何度落とされても、博士号を取るまで書き続けた。",
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
                eventId = "ev_res_04",
                sentence = "ポスドクとして研究を続けたが、正規の職が見つからない時期が続いた。",
                startAge = 27,
                endAge = 32,
                baseWeight = 0.65f,
                requiresAnyLifeTag = new() { "res_phd" },
                blockedByLifeTags = new() { "res_job" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "国際学会で発表した論文が注目された。大学からオファーが来た。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 50, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 40, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "res_job" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "また一年、契約延長で食いつないだ。研究だけは続けた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "res_wait" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_04b",
                sentence = "研究所か大学の公募に応募し続けた。",
                startAge = 30,
                endAge = 34,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "res_wait" },
                blockedByLifeTags = new() { "res_job" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "採用通知が届いた。",
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
                        grantsLifeTags = new() { "res_job" },
                        grantsStats = new()
                        {
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
                        grantsLifeTags = new() { "res_wait" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_04c",
                sentence = "ある大学の教員職に採用された。",
                startAge = 35,
                endAge = 35,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "res_wait" },
                blockedByLifeTags = new() { "res_job" },
                relatedJobIds = new() { "mage_kenkyusha" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "res_job" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_05",
                sentence = "生業が研究者になった。",
                startAge = 32,
                endAge = 36,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "res_job" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_kenkyusha" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_res_ep1",
                sentence = "実験が失敗した。データの中に、面白いものが見えた。",
                startAge = 33,
                endAge = 68,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kenkyusha" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
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
                eventId = "ev_res_res_ep2",
                sentence = "学生の質問に答えられなかった。翌日調べて伝えた。",
                startAge = 33,
                endAge = 68,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kenkyusha" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
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
                eventId = "ev_res_res_ep3",
                sentence = "締め切り前、研究室に泊まり込んだ。",
                startAge = 33,
                endAge = 68,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_kenkyusha" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "-" },
                relatedJobIds = new() { "mage_kenkyusha" },
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
                eventId = "ev_res_s1",
                sentence = "仮説が正しいと確信する瞬間があった。証明はこれからだった。",
                startAge = 33,
                endAge = 68,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kenkyusha" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
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
                eventId = "ev_res_s2",
                sentence = "論文の査読に厳しいコメントが届いた。",
                startAge = 33,
                endAge = 68,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kenkyusha" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
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
                eventId = "ev_res_s3",
                sentence = "誰も見ていない時間に、最も深く考えられた。",
                startAge = 33,
                endAge = 68,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kenkyusha" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
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
                eventId = "ev_res_r1a",
                sentence = "論文が国際誌に掲載された。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_kenkyusha" },
                blockedByLifeTags = new() { "res_r1a" },
                grantsLifeTags = new() { "res_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_r1b",
                sentence = "共同研究者として海外から声がかかった。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "res_r1a" },
                blockedByLifeTags = new() { "res_r1b" },
                grantsLifeTags = new() { "res_r1b" },
                relatedJobIds = new() { "mage_kenkyusha" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "新しい知見が得られた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "想定と異なる結果が出た。それが面白かった。",
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
                eventId = "ev_res_r3a",
                sentence = "学術賞を受賞した。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "res_r1b" },
                blockedByLifeTags = new() { "res_r3a" },
                grantsLifeTags = new() { "res_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_r3b",
                sentence = "自分の研究が教科書に引用されるようになった。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "res_r3a" },
                blockedByLifeTags = new() { "res_r3b" },
                grantsLifeTags = new() { "res_r3b" },
                relatedJobIds = new() { "mage_kenkyusha" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_r5a",
                sentence = "大学の学部長職への打診か、研究専念か。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "res_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "学部長になった。研究環境を整える仕事に回った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "res_r5a_kanri" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "断った。研究室にいることが自分の場所だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "res_r5a_lab" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_r5b_a",
                sentence = "組織を動かしながら、研究の質を守り続けた。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "res_r5a_kanri" },
                blockedByLifeTags = new() { "res_r5b" },
                grantsLifeTags = new() { "res_r5b" },
                relatedJobIds = new() { "mage_kenkyusha" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_r5b_b",
                sentence = "最後まで研究者として問い続けた。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "res_r5a_lab" },
                blockedByLifeTags = new() { "res_r5b" },
                grantsLifeTags = new() { "res_r5b" },
                relatedJobIds = new() { "mage_kenkyusha" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_r7a",
                sentence = "長年追いかけてきた仮説が証明された。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "res_r5b" },
                blockedByLifeTags = new() { "res_r7a" },
                grantsLifeTags = new() { "res_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kenkyusha" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_res_r7b",
                sentence = "最後の論文を投稿した日、机の上を少しだけ片付けた。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "res_r7a" },
                blockedByLifeTags = new() { "res_r7b" },
                grantsLifeTags = new() { "res_r7b" },
                relatedJobIds = new() { "mage_kenkyusha" },
            };
        }
        #endregion

        #region カルト教祖 (mage_kult)
        private static IEnumerable<ReinLifeEvent> Kult()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_01",
                sentence = "相談に乗るのが得意な人間だった。",
                startAge = 16,
                endAge = 16,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "cult_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                blockedByEventIds = new() { "ev_cult_01g" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "cult_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_01g",
                editorMemo = "原体験保証",
                sentence = "相談に乗るのが得意な人間だった。",
                startAge = 16,
                endAge = 16,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "cult_call" },
                blockedByEventIds = new() { "ev_cult_01" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "cult_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_02",
                sentence = "ある日、天からの声を受け取った。それを人々に伝える事が自分の使命だと感じた。",
                startAge = 17,
                endAge = 22,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "cult_call" },
                grantsLifeTags = new() { "cult_inner" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "声は徐々にはっきり聞こえるようになっていった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "この声が本当に啓示なのか疑うこともあった。",
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
                eventId = "ev_cult_03",
                sentence = "少しずつ、人が話を聞きに来るようになった。",
                startAge = 20,
                endAge = 25,
                baseWeight = 0.65f,
                requiresAnyLifeTag = new() { "cult_inner" },
                grantsLifeTags = new() { "cult_first" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "言葉が人を動かすと分かった。もっと語ることにした。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 40, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "cult_grow" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "次の月も、また人が来た。集まりが続いた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "cult_grow" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_04",
                sentence = "信者が増えた。教えを体系化し、集会所を設けた。組織になりつつあった。",
                startAge = 24,
                endAge = 28,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "cult_grow" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "教義を文章にまとめた。信者たちが聖典と呼ぶようになった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 50, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 45, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "cult_doctrine" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                            new StatBonus { stat = StatKind.MDF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "組織が大きくなるほど、自分の意図と違う方向へ動き始めた。それでも手放せなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "cult_doctrine" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_05a",
                sentence = "信者の一人が全財産を捧げると言った。",
                startAge = 32,
                endAge = 32,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "job_cult" },
                blockedByLifeTags = new() { "cult_light", "cult_dark" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "受け取らなかった。まだ良心というものが自分の中にあるようだ。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "cult_light" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                            new StatBonus { stat = StatKind.MDF, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "快く受け取った。信者のものは私のものだ！",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "cult_dark" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_05b",
                sentence = "元信者が外部に告発した。組織が揺れた。",
                startAge = 29,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_cult" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "声明を出して信者をまとめ、危機を乗り越えた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 55, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 42, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "一部の信者が離れた。残った者はより深く入り込んだ。",
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
                eventId = "ev_cult_06",
                sentence = "生業がカルト教祖になった。",
                startAge = 28,
                endAge = 31,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "cult_doctrine" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_cult" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_cult_ep1",
                sentence = "信者の一人が、自分の全てをここに捧げたいと言った。",
                startAge = 29,
                endAge = 59,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "cult_light" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
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
                eventId = "ev_cult_cult_ep2",
                sentence = "集会で、誰かが泣いていた。言葉が届いていた。",
                startAge = 29,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_cult" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
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
                eventId = "ev_cult_cult_ep3",
                sentence = "内部で不満の声が上がった。",
                startAge = 29,
                endAge = 60,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_cult" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "-" },
                relatedJobIds = new() { "mage_kult" },
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
                eventId = "ev_cult_s1",
                sentence = "語りが人の心を動かす瞬間が、より鮮明に分かるようになった。",
                startAge = 29,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_cult" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
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
                eventId = "ev_cult_s2",
                sentence = "自分が信じているものが何か、曖昧になる夜があった。",
                startAge = 29,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_cult" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MDF, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
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
                eventId = "ev_cult_s3",
                sentence = "教義と現実が一致した瞬間があった。それを信者に語った。",
                startAge = 29,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_cult" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
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
                eventId = "ev_cult_r1a",
                sentence = "信者が百人を超えた。施設を拡張した。",
                startAge = 29,
                endAge = 59,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_cult" },
                blockedByLifeTags = new() { "cult_r1a" },
                grantsLifeTags = new() { "cult_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_r1b",
                sentence = "組織が形になってきた。内部の統制が問われた。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "cult_dark" },
                blockedByLifeTags = new() { "cult_r1b" },
                grantsLifeTags = new() { "cult_r1b" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "教えが浸透した。信者たちは従った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "内部に不満が生まれた。力で抑えた。",
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
                eventId = "ev_cult_r3a",
                sentence = "メディアが取材に来た。",
                startAge = 29,
                endAge = 59,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "cult_r1b" },
                blockedByLifeTags = new() { "cult_r3a" },
                grantsLifeTags = new() { "cult_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_r3b",
                sentence = "財産が蓄積された。組織の力が増した。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "cult_r3a" },
                blockedByLifeTags = new() { "cult_r3b" },
                grantsLifeTags = new() { "cult_r3b" },
                relatedJobIds = new() { "mage_kult" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_r5a",
                sentence = "政治的な影響力を持つか、純粋な教義に戻るか。",
                startAge = 29,
                endAge = 59,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "cult_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "影響力を広げた。世俗と教義が混ざり始めた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "cult_r5a_seiji" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "教義だけに戻った。信者が半分になった。残った者は熱狂的だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "cult_r5a_kyougi" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_r5b_a",
                sentence = "政治と宗教が混ざり合い、組織は別のものになっていった。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "cult_r5a_seiji" },
                blockedByLifeTags = new() { "cult_r5b" },
                grantsLifeTags = new() { "cult_r5b" },
                relatedJobIds = new() { "mage_kult" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_r5b_b",
                sentence = "純化された教義の中で、核心的な信者だけが残った。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "cult_r5a_kyougi" },
                blockedByLifeTags = new() { "cult_r5b" },
                grantsLifeTags = new() { "cult_r5b" },
                relatedJobIds = new() { "mage_kult" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_r7a",
                sentence = "創設者として語られるようになった。",
                startAge = 29,
                endAge = 59,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "cult_r5b" },
                blockedByLifeTags = new() { "cult_r7a" },
                grantsLifeTags = new() { "cult_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_r7b",
                sentence = "最後の布教を終えた日、何を信じていたのか分からなくなった。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "cult_dark" },
                blockedByLifeTags = new() { "cult_r7b" },
                grantsLifeTags = new() { "cult_r7b" },
                relatedJobIds = new() { "mage_kult" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_05b_light",
                sentence = "元信者が外部に告発した。何が間違っていたのか、自問した。",
                startAge = 27,
                endAge = 32,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "cult_light" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "公開の場で誠実に向き合い、組織を改革した。信者の一部が残った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "謝罪した。組織を縮小した。それでも本質は変えられなかった。",
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
                eventId = "ev_cult_05b_dark",
                sentence = "元信者が外部に告発した。組織が揺れた。",
                startAge = 27,
                endAge = 32,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "cult_dark" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "声明を出して封じ込めた。信者たちは疑わなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "告発者の信用を徹底的に潰した。組織は生き残った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                            new StatBonus { stat = StatKind.AT, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_dark_ep1",
                sentence = "信者の一人が、自分の全てをここに捧げたいと言った。",
                startAge = 29,
                endAge = 59,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "cult_dark" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "快く受け入れた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_r7b_light",
                sentence = "最後の集会を終えた日、信者の一人が「あなたに救われた」と言った。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "cult_r7a" },
                blockedByLifeTags = new() { "cult_r7b" },
                grantsLifeTags = new() { "cult_r7b" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "頷いた。自分が何かを救えたのか、今も分からない。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cult_r1b_light",
                sentence = "組織が形になってきた。信者たちが自発的に動き始めた。",
                startAge = 29,
                endAge = 59,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "cult_light" },
                blockedByLifeTags = new() { "cult_r1b" },
                grantsLifeTags = new() { "cult_r1b" },
                relatedJobIds = new() { "mage_kult" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "教えが生活に根付いていた。誰も強制されていなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "内部で意見が割れた。時間をかけて話し合った。",
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
        }
        #endregion

    }
}
