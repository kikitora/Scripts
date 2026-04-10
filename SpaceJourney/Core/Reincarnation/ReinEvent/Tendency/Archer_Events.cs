using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // Archer 傾向の生業イベント
    // ================================================================
    // 狩人 / 詐欺師 / カメラマン / 弓道師範 / ゴルファー / MSF外科医
    //
    // ※ このファイルは MigrateReinEventsToCs ツールで自動生成されました。
    //    手動編集してOKです。Claudeと相談しながら追加・変更する想定。
    // ================================================================
    public static class Archer_Events
    {
        public static IEnumerable<ReinLifeEvent> All()
        {
            foreach (var ev in Karyudo()) yield return ev;
            foreach (var ev in Sagishi()) yield return ev;
            foreach (var ev in Cameraman()) yield return ev;
            foreach (var ev in Kyudo()) yield return ev;
            foreach (var ev in Golfer()) yield return ev;
            foreach (var ev in Msf()) yield return ev;
        }

        #region 狩人 (archer_karyudo)
        private static IEnumerable<ReinLifeEvent> Karyudo()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_01",
                sentence = "足跡ひとつで、動物の種類も進んだ方向も言い当てる祖父の姿が、かっこよかった。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "hunt_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                blockedByEventIds = new() { "ev_hunt_01g" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "hunt_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_01g",
                editorMemo = "原体験保証",
                sentence = "足跡ひとつで、動物の種類も進んだ方向も言い当てる祖父の姿が、かっこよかった。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "hunt_call" },
                blockedByEventIds = new() { "ev_hunt_01" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "hunt_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_02",
                sentence = "山に入る許可を得るため、狩猟免許の取得を目指した。",
                startAge = 15,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "hunt_call" },
                grantsLifeTags = new() { "hunt_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "指の感覚がいいと言われた。",
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
                        sentence = "学科の暗記に苦労したが、すべて通過した。",
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
                eventId = "ev_hunt_03",
                sentence = "狩猟免許試験を受けた。",
                startAge = 18,
                endAge = 21,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "hunt_train" },
                blockedByLifeTags = new() { "hunt_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "一発で合格し、猟友会への入会も認められた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "hunt_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 2 },
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
                        grantsLifeTags = new() { "hunt_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_03b",
                sentence = "再受験した。",
                startAge = 19,
                endAge = 22,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "hunt_fail" },
                blockedByLifeTags = new() { "hunt_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
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
                        grantsLifeTags = new() { "hunt_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_03c",
                sentence = "免許試験に再挑戦した。ついに合格した。",
                startAge = 22,
                endAge = 22,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "hunt_fail" },
                blockedByLifeTags = new() { "hunt_pass" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "hunt_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_04",
                sentence = "猟友会のベテランについて山に入り始めた。獲物の気配の読み方を体で覚えていった。",
                startAge = 21,
                endAge = 24,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "hunt_pass" },
                grantsLifeTags = new() { "hunt_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "師匠から「勘がいい」と言われた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "何度も空振りした。",
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
                eventId = "ev_hunt_06",
                sentence = "生業が狩人になった。",
                startAge = 22,
                endAge = 24,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "hunt_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_karyudo" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_05",
                sentence = "初めて獲物を仕留めた瞬間、喜びより先に胸が痛んだ。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "job_karyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "胸は痛んだ。それでも山に戻った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "しばらく山に入れなかった。それでも戻ってきた。",
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
                eventId = "ev_hunt_hunt_ep1",
                sentence = "山で熊の痕跡を見つけた。引き返した。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_karyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_hunt_hunt_ep2",
                sentence = "罠にかかった動物を見た。逃してやった。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_karyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_hunt_hunt_ep3",
                sentence = "雪が積もり、山に入れない日が続いた。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_karyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "-" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_hunt_s1",
                sentence = "風向きで獲物の位置を読み切った。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_karyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_hunt_s2",
                sentence = "読みが外れた。山は思い通りにならない。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_karyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_hunt_s3",
                sentence = "静寂の中で、自分が山の一部になった気がした。",
                startAge = 23,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_karyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_hunt_r1a",
                sentence = "単独猟で大物を仕留めた。手応えが変わった。",
                startAge = 23,
                endAge = 53,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_karyudo" },
                blockedByLifeTags = new() { "hunt_r1a" },
                grantsLifeTags = new() { "hunt_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_r1b",
                sentence = "足跡だけで獲物の行動を読み切った。",
                startAge = 23,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "hunt_r1a" },
                blockedByLifeTags = new() { "hunt_r1b" },
                grantsLifeTags = new() { "hunt_r1b" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "思った通りの場所に出てきた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "読みが外れた。別の技を試した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_hunt_r3a",
                sentence = "猟友会の中でベテランとして扱われるようになった。",
                startAge = 23,
                endAge = 53,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "hunt_r1b" },
                blockedByLifeTags = new() { "hunt_r3a" },
                grantsLifeTags = new() { "hunt_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_r3b",
                sentence = "難しい地形での猟を単独で成功させた。",
                startAge = 23,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "hunt_r3a" },
                blockedByLifeTags = new() { "hunt_r3b" },
                grantsLifeTags = new() { "hunt_r3b" },
                relatedJobIds = new() { "archer_karyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_r5a",
                sentence = "後進の指導を頼まれた。",
                startAge = 23,
                endAge = 53,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "hunt_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "引き受けた。技術を言葉にする難しさを知った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "hunt_r5a_teach" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "断った。山は自分で覚えるものだと思った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "hunt_r5a_solo" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_r5b_a",
                sentence = "教えながら、自分でも気づかなかった技術に言葉が生まれた。",
                startAge = 23,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "hunt_r5a_teach" },
                blockedByLifeTags = new() { "hunt_r5b" },
                grantsLifeTags = new() { "hunt_r5b" },
                relatedJobIds = new() { "archer_karyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_r5b_b",
                sentence = "一人で山に入り続けた。それが自分の流儀だった。",
                startAge = 23,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "hunt_r5a_solo" },
                blockedByLifeTags = new() { "hunt_r5b" },
                grantsLifeTags = new() { "hunt_r5b" },
                relatedJobIds = new() { "archer_karyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_r7a",
                sentence = "足腰が衰えを見せ始めた。それでも山に入った。",
                startAge = 23,
                endAge = 53,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "hunt_r5b" },
                blockedByLifeTags = new() { "hunt_r7a" },
                grantsLifeTags = new() { "hunt_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_karyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_hunt_r7b",
                sentence = "最後の猟を終えた日、山を振り返らずに下りた。",
                startAge = 23,
                endAge = 53,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "hunt_r7a" },
                blockedByLifeTags = new() { "hunt_r7b" },
                grantsLifeTags = new() { "hunt_r7b" },
                relatedJobIds = new() { "archer_karyudo" },
            };
        }
        #endregion

        #region 詐欺師 (archer_sagishi)
        private static IEnumerable<ReinLifeEvent> Sagishi()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_01",
                sentence = "相手の癖や視線で、手札が読めた。負けたことがなかった。",
                startAge = 10,
                endAge = 14,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "scam_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                blockedByEventIds = new() { "ev_scam_01g" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "scam_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_01g",
                editorMemo = "原体験保証",
                sentence = "相手の癖や視線で、手札が読めた。負けたことがなかった。",
                startAge = 14,
                endAge = 14,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "scam_call" },
                blockedByEventIds = new() { "ev_scam_01" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "scam_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_02",
                sentence = "師匠に拾われ、詐欺の手口を教え込まれた。",
                startAge = 15,
                endAge = 18,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "scam_call" },
                grantsLifeTags = new() { "scam_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "師匠の動きを見てすぐに再現できた。手口を覚えるのも早かった。",
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
                        sentence = "師匠の動きを盗んで覚えるしかなかった。それがすべてだった。",
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
                eventId = "ev_scam_03",
                sentence = "途中で見破られたが、追われながらもなんとか逃げ切った。",
                startAge = 18,
                endAge = 22,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "scam_train" },
                blockedByLifeTags = new() { "scam_solo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "次はもっと精度が上がった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "scam_solo" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "逃げ切ったものの、しばらくは何もできなかった。それでも次を仕掛けた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "scam_solo" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_04",
                sentence = "大掛かりな詐欺を仕掛けるため、3ヶ月かけて準備した。",
                startAge = 21,
                endAge = 24,
                baseWeight = 0.65f,
                requiresAnyLifeTag = new() { "scam_solo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "完璧に決まり、誰にも気づかれなかった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 50, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.AGI, threshold = 30, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "scam_done" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "半分は成功したが、残りは感づかれて逃げた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "scam_done" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_05",
                sentence = "生業が詐欺師になった。",
                startAge = 24,
                endAge = 26,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "scam_done" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_sagishi" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_scam_ep1",
                sentence = "相手の動揺を読んで、次の一手を決めた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_sagishi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_scam_scam_ep2",
                sentence = "仕掛けた後、しばらく静かに待った。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_sagishi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_scam_scam_ep3",
                sentence = "追われた。逃げ切った。しばらく動けなかった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_sagishi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "-" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_scam_s1",
                sentence = "人の心の動きが手に取るように分かった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_sagishi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_scam_s2",
                sentence = "相手が予想外の行動を取った。計算が外れた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_sagishi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_scam_s3",
                sentence = "やりすぎたと思う仕掛けがあった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_sagishi" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_scam_r1a",
                sentence = "大掛かりな組織詐欺に誘われ、役割を担った。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_sagishi" },
                blockedByLifeTags = new() { "scam_r1a" },
                grantsLifeTags = new() { "scam_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_r1b",
                sentence = "計画通りに動いた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "scam_r1a" },
                blockedByLifeTags = new() { "scam_r1b" },
                grantsLifeTags = new() { "scam_r1b" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "完璧にこなした。取り分が増えた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "一部が崩れた。逃げた。生きて帰ってきた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_scam_r3a",
                sentence = "自分でチームを組んで仕掛けるようになった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "scam_r1b" },
                blockedByLifeTags = new() { "scam_r3a" },
                grantsLifeTags = new() { "scam_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_r3b",
                sentence = "一件の大仕事が伝説になった。名前は出ていないが。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "scam_r3a" },
                blockedByLifeTags = new() { "scam_r3b" },
                grantsLifeTags = new() { "scam_r3b" },
                relatedJobIds = new() { "archer_sagishi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_r5a",
                sentence = "表の顔を作るか、地下に潜り続けるか。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "scam_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "コンサルタントとして表向きの仕事を始めた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "scam_r5a_ura" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "地下に潜り続けた。安全だった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "scam_r5a_chi" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_r5b_a",
                sentence = "表と裏を使い分けながら、誰にも全体が見えない動き方をした。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "scam_r5a_ura" },
                blockedByLifeTags = new() { "scam_r5b" },
                grantsLifeTags = new() { "scam_r5b" },
                relatedJobIds = new() { "archer_sagishi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_r5b_b",
                sentence = "誰にも見えない場所から、動き続けた。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "scam_r5a_chi" },
                blockedByLifeTags = new() { "scam_r5b" },
                grantsLifeTags = new() { "scam_r5b" },
                relatedJobIds = new() { "archer_sagishi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_r7a",
                sentence = "長年の相棒が足を洗うと言った。",
                startAge = 25,
                endAge = 55,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "scam_r5b" },
                blockedByLifeTags = new() { "scam_r7a" },
                grantsLifeTags = new() { "scam_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_sagishi" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_scam_r7b",
                sentence = "最後の仕掛けは、完璧だった。誰にも気づかれなかった。",
                startAge = 25,
                endAge = 55,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "scam_r7a" },
                blockedByLifeTags = new() { "scam_r7b" },
                grantsLifeTags = new() { "scam_r7b" },
                relatedJobIds = new() { "archer_sagishi" },
            };
        }
        #endregion

        #region カメラマン (archer_cameraman)
        private static IEnumerable<ReinLifeEvent> Cameraman()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_01",
                sentence = "父の一眼レフのファインダーを覗いた。見慣れた景色が、まったく別物に見えた。",
                startAge = 8,
                endAge = 14,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "cam_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                blockedByEventIds = new() { "ev_cam_01g" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "cam_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_01g",
                editorMemo = "原体験保証",
                sentence = "父の一眼レフのファインダーを覗いた。見慣れた景色が、まったく別物に見えた。",
                startAge = 14,
                endAge = 14,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "cam_call" },
                blockedByEventIds = new() { "ev_cam_01" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "cam_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_02",
                sentence = "カメラを手に撮り続けた。街も人も光も、何でも撮った。",
                startAge = 14,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "cam_call" },
                grantsLifeTags = new() { "cam_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "撮った写真をネットに上げると沢山の反応をもらった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 30, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 30, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "撮った写真をネットに上げたが、誰にも見向きされなかった。",
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
                eventId = "ev_cam_03",
                sentence = "写真コンテストや雑誌への投稿を始めた。",
                startAge = 18,
                endAge = 23,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "cam_train" },
                blockedByLifeTags = new() { "cam_notice" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "コンテストで入賞した。編集者から声がかかった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 40, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "cam_notice" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "落選が続いた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "cam_keep" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_03b",
                sentence = "別のコンテストや媒体に再投稿した。",
                startAge = 20,
                endAge = 25,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "cam_keep" },
                blockedByLifeTags = new() { "cam_notice" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "入選した。小さな媒体だが掲載された。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "cam_notice" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
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
                        grantsLifeTags = new() { "cam_keep" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_03c",
                sentence = "作品が初めて外部に認められた。",
                startAge = 26,
                endAge = 26,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "cam_keep" },
                blockedByLifeTags = new() { "cam_notice" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "cam_notice" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_04",
                sentence = "フォトグラファーのアシスタントとして採用された。仕事は機材の運搬から始まった。",
                startAge = 23,
                endAge = 27,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "cam_notice" },
                grantsLifeTags = new() { "cam_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "現場で提案した構図が採用された。師匠がカメラを渡してくれた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "1年間、師匠の後ろで全てを見ていた。",
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
                eventId = "ev_cam_05",
                sentence = "生業がカメラマンになった。",
                startAge = 26,
                endAge = 28,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "cam_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_cameraman" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_cam_ep1",
                sentence = "取材先で、誰も気づかなかった瞬間をとらえた。",
                startAge = 27,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_cameraman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_cam_cam_ep2",
                sentence = "機材トラブルが現場で発生した。その場で対処した。",
                startAge = 27,
                endAge = 65,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_cameraman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_cam_cam_ep3",
                sentence = "撮った写真が没になった。理由を聞いた。",
                startAge = 27,
                endAge = 65,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_cameraman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "-" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_cam_s1",
                sentence = "光の変化を一瞬で読んで、シャッターを切った。",
                startAge = 27,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_cameraman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_cam_s2",
                sentence = "何を撮りたいのか、分からなくなる時期があった。",
                startAge = 27,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_cameraman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_cam_s3",
                sentence = "被写体との距離感が、以前とは違うと感じた。",
                startAge = 27,
                endAge = 65,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_cameraman" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_cam_r1a",
                sentence = "大手媒体から初めて名指しで仕事が来た。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_cameraman" },
                blockedByLifeTags = new() { "cam_r1a" },
                grantsLifeTags = new() { "cam_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_r1b",
                sentence = "海外ロケを一人でこなした。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "cam_r1a" },
                blockedByLifeTags = new() { "cam_r1b" },
                grantsLifeTags = new() { "cam_r1b" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "編集部が想定以上の評価をした。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "想定と違う絵になった。それでも採用された。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_cam_r3a",
                sentence = "フォトジャーナリストとして紛争地への派遣が決まった。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "cam_r1b" },
                blockedByLifeTags = new() { "cam_r3a" },
                grantsLifeTags = new() { "cam_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_r3b",
                sentence = "世界的な賞にノミネートされた。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "cam_r3a" },
                blockedByLifeTags = new() { "cam_r3b" },
                grantsLifeTags = new() { "cam_r3b" },
                relatedJobIds = new() { "archer_cameraman" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_r5a",
                sentence = "写真集を出すか、報道の現場に留まるか。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "cam_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "写真集を出した。自分の仕事を振り返る機会になった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "cam_r5a_book" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "現場に留まった。写真は撮り続けるためにある。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "cam_r5a_field" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_r5b_a",
                sentence = "写真集が若い写真家の間で語られるようになった。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "cam_r5a_book" },
                blockedByLifeTags = new() { "cam_r5b" },
                grantsLifeTags = new() { "cam_r5b" },
                relatedJobIds = new() { "archer_cameraman" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_r5b_b",
                sentence = "現場で撮り続けた。その写真が歴史になっていった。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "cam_r5a_field" },
                blockedByLifeTags = new() { "cam_r5b" },
                grantsLifeTags = new() { "cam_r5b" },
                relatedJobIds = new() { "archer_cameraman" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_r7a",
                sentence = "後輩に、写真家を志すきっかけはあなたの写真だったと言われた。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "cam_r5b" },
                blockedByLifeTags = new() { "cam_r7a" },
                grantsLifeTags = new() { "cam_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_cameraman" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_cam_r7b",
                sentence = "最後のシャッターを切った場所がどこか、覚えていない。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "cam_r7a" },
                blockedByLifeTags = new() { "cam_r7b" },
                grantsLifeTags = new() { "cam_r7b" },
                relatedJobIds = new() { "archer_cameraman" },
            };
        }
        #endregion

        #region 弓道師範 (archer_kyudo)
        private static IEnumerable<ReinLifeEvent> Kyudo()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_01",
                sentence = "神社で奉納弓の演武を見た。矢が放たれた瞬間、音が消えた。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "kyudo_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                blockedByEventIds = new() { "ev_kyudo_01g" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "kyudo_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_01g",
                editorMemo = "原体験保証",
                sentence = "神社で奉納弓の演武を見た。矢が放たれた瞬間、音が消えた。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "kyudo_call" },
                blockedByEventIds = new() { "ev_kyudo_01" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "kyudo_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_02",
                sentence = "弓道部に入った。礼法と射法、的前を順序通りに叩き込まれた。",
                startAge = 13,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "kyudo_call" },
                grantsLifeTags = new() { "kyudo_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "射形が綺麗だと言われ、先生から個別指導を受けた。",
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
                        sentence = "中てることより形を作ることに時間がかかった。",
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
                eventId = "ev_kyudo_03",
                sentence = "射だけでなく、礼法や体配まで採点された。",
                startAge = 18,
                endAge = 24,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "kyudo_train" },
                blockedByLifeTags = new() { "kyudo_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。審査員から所作も評価された。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "kyudo_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
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
                        grantsLifeTags = new() { "kyudo_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_03b",
                sentence = "再審査を受けた。",
                startAge = 21,
                endAge = 26,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "kyudo_fail" },
                blockedByLifeTags = new() { "kyudo_pass" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "kyudo_pass" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
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
                        grantsLifeTags = new() { "kyudo_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_03c",
                sentence = "高段位審査に再挑戦し、ついに合格した。",
                startAge = 27,
                endAge = 27,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "kyudo_fail" },
                blockedByLifeTags = new() { "kyudo_pass" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "kyudo_pass" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_04",
                sentence = "師範免許を取得した。",
                startAge = 30,
                endAge = 38,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "kyudo_pass" },
                grantsLifeTags = new() { "kyudo_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "最初の弟子が半年で形になった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 48, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 40, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "弟子に教えながら、自分の射が変わった。",
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
                eventId = "ev_kyudo_06",
                sentence = "生業が弓道師範になった。",
                startAge = 32,
                endAge = 40,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "kyudo_in" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_kyudo" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_05a",
                sentence = "初めて弟子を取った。射形を見て、どこから直すべきか考えた。",
                startAge = 34,
                endAge = 75,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "半年で弟子の射が変わった。教えることで自分の射も整った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 42, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MDF, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "言葉で伝えられないことが多かった。見せるしかなかった。",
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
                eventId = "ev_kyudo_kyudo_ep1",
                sentence = "弟子の一人が、初めて的に中てた。",
                startAge = 34,
                endAge = 75,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_kyudo_kyudo_ep2",
                sentence = "早朝の道場で、一人で射った。誰も来なかった。",
                startAge = 34,
                endAge = 75,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_kyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_kyudo_kyudo_ep3",
                sentence = "弟子が悩んでいた。言葉をかけられなかった。",
                startAge = 34,
                endAge = 75,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_kyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "-" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_kyudo_s1",
                sentence = "弟子の射形が変わった瞬間を、言葉より先に感じた。",
                startAge = 34,
                endAge = 75,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_kyudo_s2",
                sentence = "自分の射が乱れていた。原因が分からなかった。",
                startAge = 34,
                endAge = 75,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_kyudo_s3",
                sentence = "無言で的を見つめる時間が、すべてを整えた。",
                startAge = 34,
                endAge = 75,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_kyudo" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_kyudo_r1a",
                sentence = "弟子が全国大会で入賞した。",
                startAge = 34,
                endAge = 64,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_kyudo" },
                blockedByLifeTags = new() { "kyudo_r1a" },
                grantsLifeTags = new() { "kyudo_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_r1b",
                sentence = "道場の名が広まり、遠方からも門弟が来るようになった。",
                startAge = 34,
                endAge = 64,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "kyudo_r1a" },
                blockedByLifeTags = new() { "kyudo_r1b" },
                grantsLifeTags = new() { "kyudo_r1b" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "丁寧に選んで受け入れた。道場の質を守った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "全員受け入れた。一人一人と向き合う時間が増えた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_kyudo_r3a",
                sentence = "高段位の審査員を務めるよう依頼された。",
                startAge = 34,
                endAge = 64,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "kyudo_r1b" },
                blockedByLifeTags = new() { "kyudo_r3a" },
                grantsLifeTags = new() { "kyudo_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_r3b",
                sentence = "弓道の普及活動で講演を行った。",
                startAge = 34,
                endAge = 64,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "kyudo_r3a" },
                blockedByLifeTags = new() { "kyudo_r3b" },
                grantsLifeTags = new() { "kyudo_r3b" },
                relatedJobIds = new() { "archer_kyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_r5a",
                sentence = "弓道連盟の要職への就任を打診された。",
                startAge = 34,
                endAge = 64,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "kyudo_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "引き受けた。弓道全体を動かす立場になった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "kyudo_r5a_renmei" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "断った。道場の弟子と向き合い続けることを選んだ。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "kyudo_r5a_dojo" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_r5b_a",
                sentence = "組織として弓道を守り、広める仕事に就いた。",
                startAge = 34,
                endAge = 64,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "kyudo_r5a_renmei" },
                blockedByLifeTags = new() { "kyudo_r5b" },
                grantsLifeTags = new() { "kyudo_r5b" },
                relatedJobIds = new() { "archer_kyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_r5b_b",
                sentence = "道場師範として、一人一人の射に向き合い続けた。",
                startAge = 34,
                endAge = 64,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "kyudo_r5a_dojo" },
                blockedByLifeTags = new() { "kyudo_r5b" },
                grantsLifeTags = new() { "kyudo_r5b" },
                relatedJobIds = new() { "archer_kyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_r7a",
                sentence = "最後の段位審査を受けることを決めた。",
                startAge = 34,
                endAge = 64,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "kyudo_r5b" },
                blockedByLifeTags = new() { "kyudo_r7a" },
                grantsLifeTags = new() { "kyudo_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_kyudo" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_kyudo_r7b",
                sentence = "矢が的に届いたとき、長い間射場に立っていた。",
                startAge = 34,
                endAge = 64,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "kyudo_r7a" },
                blockedByLifeTags = new() { "kyudo_r7b" },
                grantsLifeTags = new() { "kyudo_r7b" },
                relatedJobIds = new() { "archer_kyudo" },
            };
        }
        #endregion

        #region ゴルファー (archer_golfer)
        private static IEnumerable<ReinLifeEvent> Golfer()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_01",
                sentence = "家に置いてあったドライバーを振ってみた。風を切る音が気持ちいいと感じた。",
                startAge = 6,
                endAge = 10,
                baseWeight = 0.09f,
                blockedByLifeTags = new() { "golf_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                blockedByEventIds = new() { "ev_golf_01g" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "golf_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_01g",
                editorMemo = "原体験保証",
                sentence = "家に置いてあったドライバーを振ってみた。風を切る音が気持ちいいと感じた。",
                startAge = 10,
                endAge = 10,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "golf_call" },
                blockedByEventIds = new() { "ev_golf_01" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "golf_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_02",
                sentence = "ゴルフ部に入り、毎日スイングを繰り返した。幼い頃から始めていたライバルとの差は圧倒的だった。",
                startAge = 10,
                endAge = 16,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "golf_call" },
                grantsLifeTags = new() { "golf_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "それでも差は縮まり始めた。センスがあると言われた。",
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
                        sentence = "差は縮まらなかった。",
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
                eventId = "ev_golf_03",
                sentence = "アマチュア大会に出場した。",
                startAge = 17,
                endAge = 21,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "golf_train" },
                blockedByLifeTags = new() { "golf_win" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "上位入賞した。プロを目指すだけの手応えを得た。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 40, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.MAT, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "golf_win" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "予選落ちした。練習内容を見直した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "golf_loss" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_03b",
                sentence = "再び大会に出場した。",
                startAge = 19,
                endAge = 22,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "golf_loss" },
                blockedByLifeTags = new() { "golf_win" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "今度は上位に入った。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "golf_win" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "また結果が出なかった。練習内容を見直した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "golf_loss" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_03c",
                sentence = "アマチュア大会にもう一度出場し、ついに結果を出した。",
                startAge = 23,
                endAge = 23,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "golf_loss" },
                blockedByLifeTags = new() { "golf_win" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "golf_win" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_04",
                sentence = "プロゴルファーの資格試験（QT）を受けた。36ホール連続で規定スコアを出す必要があった。",
                startAge = 22,
                endAge = 26,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "golf_win" },
                blockedByLifeTags = new() { "golf_pro" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 50, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.MAT, threshold = 40, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "golf_pro" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "規定スコアに届かなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "golf_loss2" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_04b",
                sentence = "再挑戦した。",
                startAge = 24,
                endAge = 27,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "golf_loss2" },
                blockedByLifeTags = new() { "golf_pro" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "golf_pro" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
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
                        grantsLifeTags = new() { "golf_loss2" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_04c",
                sentence = "プロ資格試験に再挑戦した。ついに合格した。",
                startAge = 28,
                endAge = 28,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "golf_loss2" },
                blockedByLifeTags = new() { "golf_pro" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "golf_pro" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_05",
                sentence = "生業がプロゴルファーになった。",
                startAge = 26,
                endAge = 29,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "golf_pro" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_golfer" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_golf_ep1",
                sentence = "練習ラウンドでスコアが崩れた。原因を特定した。",
                startAge = 27,
                endAge = 55,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_golfer" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_golf_golf_ep2",
                sentence = "試合前夜、何も考えずに眠れた。",
                startAge = 27,
                endAge = 55,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_golfer" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_golf_golf_ep3",
                sentence = "スランプが来た。練習量を増やした。",
                startAge = 27,
                endAge = 55,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_golfer" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "-" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_golf_s1",
                sentence = "ショットの感覚が研ぎ澄まされた日があった。",
                startAge = 27,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_golfer" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_golf_s2",
                sentence = "コース管理が崩れた。判断が遅かった。",
                startAge = 27,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_golfer" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_golf_s3",
                sentence = "プレッシャーの中でも、呼吸が整っていた。",
                startAge = 27,
                endAge = 55,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_golfer" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_golf_r1a",
                sentence = "ツアーで初優勝を果たした。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_golfer" },
                blockedByLifeTags = new() { "golf_r1a" },
                grantsLifeTags = new() { "golf_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_r1b",
                sentence = "メジャー大会への出場権を得た。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "golf_r1a" },
                blockedByLifeTags = new() { "golf_r1b" },
                grantsLifeTags = new() { "golf_r1b" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "上位でフィニッシュした。自信になった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "予選通過が精一杯だった。経験を積んだ。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_golf_r3a",
                sentence = "賞金ランキング上位に定着するようになった。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "golf_r1b" },
                blockedByLifeTags = new() { "golf_r3a" },
                grantsLifeTags = new() { "golf_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_r3b",
                sentence = "スポンサー契約が増えた。名前が認知されてきた。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "golf_r3a" },
                blockedByLifeTags = new() { "golf_r3b" },
                grantsLifeTags = new() { "golf_r3b" },
                relatedJobIds = new() { "archer_golfer" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_r5a",
                sentence = "海外ツアーへの挑戦か、国内での安定したキャリアか。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "golf_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "海外に出た。言葉も文化も違う場所で戦った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "golf_r5a_over" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "国内に留まった。日本のゴルフを引っ張ることを選んだ。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "golf_r5a_home" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_r5b_a",
                sentence = "世界の舞台で戦いながら、自分のゴルフを磨き続けた。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "golf_r5a_over" },
                blockedByLifeTags = new() { "golf_r5b" },
                grantsLifeTags = new() { "golf_r5b" },
                relatedJobIds = new() { "archer_golfer" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_r5b_b",
                sentence = "国内トップとして後進に道を示した。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "golf_r5a_home" },
                blockedByLifeTags = new() { "golf_r5b" },
                grantsLifeTags = new() { "golf_r5b" },
                relatedJobIds = new() { "archer_golfer" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_r7a",
                sentence = "引退を考える年齢になった。最後の大会を決めた。",
                startAge = 27,
                endAge = 57,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "golf_r5b" },
                blockedByLifeTags = new() { "golf_r7a" },
                grantsLifeTags = new() { "golf_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_golfer" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_golf_r7b",
                sentence = "最終ホールでパットを沈めた。拍手の中を歩いた。",
                startAge = 27,
                endAge = 57,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "golf_r7a" },
                blockedByLifeTags = new() { "golf_r7b" },
                grantsLifeTags = new() { "golf_r7b" },
                relatedJobIds = new() { "archer_golfer" },
            };
        }
        #endregion

        #region MSF外科医 (archer_msf)
        private static IEnumerable<ReinLifeEvent> Msf()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_01",
                sentence = "電気も水もない野戦病院で手術をする医師の映像を見た。",
                startAge = 8,
                endAge = 13,
                baseWeight = 0.08f,
                blockedByLifeTags = new() { "msf_call" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                blockedByEventIds = new() { "ev_msf_01g" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "msf_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_01g",
                editorMemo = "原体験保証",
                sentence = "電気も水もない野戦病院で手術をする医師の映像を見た。",
                startAge = 13,
                endAge = 13,
                baseWeight = 1.0f,
                blockedByLifeTags = new() { "msf_call" },
                blockedByEventIds = new() { "ev_msf_01" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "msf_call" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_02",
                sentence = "学ぶ量は圧倒的に多かった。",
                startAge = 16,
                endAge = 18,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "msf_call" },
                grantsLifeTags = new() { "msf_train" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "模試でA判定が出続けた。現役合格が見えてきた。",
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
                        sentence = "浪人覚悟で勉強を続けた。それでも諦めなかった。",
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
                eventId = "ev_msf_03",
                sentence = "医学部入試を受けた。",
                startAge = 18,
                endAge = 22,
                baseWeight = 0.65f,
                requiresAnyLifeTag = new() { "msf_train" },
                blockedByLifeTags = new() { "msf_med" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "合格した。長い6年間が始まった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 50, weightBonus = 2.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "msf_med" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 3 },
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
                        grantsLifeTags = new() { "msf_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_03b",
                sentence = "再受験した。",
                startAge = 20,
                endAge = 23,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "msf_fail" },
                blockedByLifeTags = new() { "msf_med" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
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
                        grantsLifeTags = new() { "msf_med" },
                        grantsStats = new()
                        {
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
                        grantsLifeTags = new() { "msf_fail" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_03c",
                sentence = "医学部に再挑戦した。ついに合格した。",
                startAge = 25,
                endAge = 25,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "msf_fail" },
                blockedByLifeTags = new() { "msf_med" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "msf_med" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_04",
                sentence = "手術の補助に立ちながら、指先の感覚を覚えていった。",
                startAge = 24,
                endAge = 30,
                baseWeight = 0.75f,
                requiresAnyLifeTag = new() { "msf_med" },
                grantsLifeTags = new() { "msf_spec" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "指導医から「手の感覚が違う」と言われた。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 52, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 40, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "手術の補助に立ちながら、指先の感覚を覚えていった。",
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
                eventId = "ev_msf_05",
                sentence = "国境なき医師団（MSF）の採用試験を受けた。医療技術だけでなく、精神的な耐性も見られた。",
                startAge = 30,
                endAge = 35,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "msf_spec" },
                blockedByLifeTags = new() { "msf_field" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "採用された。最初の派遣先が決まった。",
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
                        grantsLifeTags = new() { "msf_field" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "採用されなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsLifeTags = new() { "msf_wait" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_05b",
                sentence = "MSFに再応募した。",
                startAge = 32,
                endAge = 37,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "msf_wait" },
                blockedByLifeTags = new() { "msf_field" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "採用された。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "msf_field" },
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
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
                        },
                        grantsLifeTags = new() { "msf_wait" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_05c",
                sentence = "MSFに再応募した。ついに採用された。",
                startAge = 38,
                endAge = 38,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "msf_wait" },
                blockedByLifeTags = new() { "msf_field" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "msf_field" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_07",
                sentence = "生業がMSF外科医になった。",
                startAge = 32,
                endAge = 40,
                baseWeight = 0.85f,
                requiresAnyLifeTag = new() { "msf_field" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        grantsLifeTags = new() { "job_msf" },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_06a",
                sentence = "深夜3時に緊急手術となった。患者の容態は急変しており、発電機の音だけが響いていた。",
                startAge = 33,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_msf" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "手を止めずに処置を続け、患者を救った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 58, weightBonus = 2.0f },
                            new StatCondition { stat = StatKind.AGI, threshold = 48, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 10 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 6 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "できることはすべてやったが、患者は亡くなった。それでも次の手術に向かった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
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
                eventId = "ev_msf_06b",
                sentence = "CTなしで診断しなければならない場面が来た。設備のない現場だった。",
                startAge = 33,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_msf" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "触診と聴診だけで判断した。その判断は正しかった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.MAT, threshold = 57, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 45, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                            new StatBonus { stat = StatKind.MAT, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "迷いながらも決断した。結果的に助かった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
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
                eventId = "ev_msf_07a",
                sentence = "MSF外科医として派遣地に向かうようになった。",
                startAge = 33,
                endAge = 60,
                baseWeight = 0.9f,
                requiresAnyLifeTag = new() { "job_msf" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
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
                eventId = "ev_msf_msf_ep1",
                sentence = "夜明け前の手術が終わった。外が少し明るくなっていた。",
                startAge = 33,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_msf" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_msf_msf_ep2",
                sentence = "現地スタッフが献身的に動いていた。頭が下がった。",
                startAge = 33,
                endAge = 60,
                baseWeight = 0.55f,
                requiresAnyLifeTag = new() { "job_msf" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_msf_msf_ep3",
                sentence = "物資が届かなかった。あるもので対応した。",
                startAge = 33,
                endAge = 60,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "job_msf" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "-" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_msf_s1",
                sentence = "限られた設備でも、手だけが正確に動いた。",
                startAge = 33,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_msf" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_msf_s2",
                sentence = "疲弊が限界に達していた。それでも手術台に立った。",
                startAge = 33,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_msf" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.MAT, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_msf_s3",
                sentence = "助けられなかった命を、夜に思い出した。",
                startAge = 33,
                endAge = 60,
                baseWeight = 0.45f,
                requiresAnyLifeTag = new() { "job_msf" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AT, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_msf_r1a",
                sentence = "現地チームのリーダーを任された。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.55f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "job_msf" },
                blockedByLifeTags = new() { "msf_r1a" },
                grantsLifeTags = new() { "msf_r1a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_r1b",
                sentence = "手術件数が同僚の中で最多になった。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 1,
                requiresAnyLifeTag = new() { "msf_r1a" },
                blockedByLifeTags = new() { "msf_r1b" },
                grantsLifeTags = new() { "msf_r1b" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "それが誇りではなく、現実の重さだと思った。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 25, weightBonus = 1.5f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "疲労が限界に達したが、手を止めなかった。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
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
                eventId = "ev_msf_r3a",
                sentence = "MSF内で専門家として講義を行うよう依頼された。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.5f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "msf_r1b" },
                blockedByLifeTags = new() { "msf_r3a" },
                grantsLifeTags = new() { "msf_r3a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_r3b",
                sentence = "困難な地域への派遣に、自ら志願した。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 3,
                requiresAnyLifeTag = new() { "msf_r3a" },
                blockedByLifeTags = new() { "msf_r3b" },
                grantsLifeTags = new() { "msf_r3b" },
                relatedJobIds = new() { "archer_msf" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_r5a",
                sentence = "MSFの幹部職への就任か、現場派遣の継続か。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.5f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "msf_r3b" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
                options = new()
                {
                    new ReinSentenceOption
                    {
                        sentence = "幹部職に就いた。組織全体を動かす判断を下すようになった。",
                        baseWeight = 1.0f,
                        statConditions = new()
                        {
                            new StatCondition { stat = StatKind.AGI, threshold = 45, weightBonus = 1.5f },
                            new StatCondition { stat = StatKind.AGI, threshold = 35, weightBonus = 1.0f },
                        },
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 8 },
                        },
                        grantsLifeTags = new() { "msf_r5a_top" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 4 },
                        },
                    },
                    new ReinSentenceOption
                    {
                        sentence = "現場に残った。手術台の前に立ち続けた。",
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 5 },
                        },
                        grantsLifeTags = new() { "msf_r5a_field" },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 3 },
                        },
                    },
                },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_r5b_a",
                sentence = "組織を動かしながら、現場への支援を続けた。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "msf_r5a_top" },
                blockedByLifeTags = new() { "msf_r5b" },
                grantsLifeTags = new() { "msf_r5b" },
                relatedJobIds = new() { "archer_msf" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_r5b_b",
                sentence = "最後まで現場に立ち続けた。それが自分の答えだった。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 5,
                requiresAnyLifeTag = new() { "msf_r5a_field" },
                blockedByLifeTags = new() { "msf_r5b" },
                grantsLifeTags = new() { "msf_r5b" },
                relatedJobIds = new() { "archer_msf" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_r7a",
                sentence = "派遣先で死にかけたことがあった。それでも戻った。",
                startAge = 33,
                endAge = 63,
                baseWeight = 0.45f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "msf_r5b" },
                blockedByLifeTags = new() { "msf_r7a" },
                grantsLifeTags = new() { "msf_r7a" },
                hasStatWeightConfig = true,
                statWeightConfig = new StatWeightConfig { stat = StatKind.AGI, sign = "+" },
                relatedJobIds = new() { "archer_msf" },
            };
            yield return new ReinLifeEvent
            {
                eventId = "ev_msf_r7b",
                sentence = "最後の手術を終えた夜、発電機の音が止まっていた。",
                startAge = 33,
                endAge = 63,
                baseWeight = 1.0f,
                requireMinRank = 7,
                requiresAnyLifeTag = new() { "msf_r7a" },
                blockedByLifeTags = new() { "msf_r7b" },
                grantsLifeTags = new() { "msf_r7b" },
                relatedJobIds = new() { "archer_msf" },
            };
        }
        #endregion

    }
}
