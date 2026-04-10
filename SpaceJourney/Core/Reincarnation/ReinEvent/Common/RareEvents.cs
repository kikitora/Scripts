using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // RareEvents (共通レアイベント / スキル習得チャンス)
    // ================================================================
    // 全傾向で使えるアクティブスキルを習得する低確率イベント。
    //
    // 6グループ × 3イベント = 18イベント。
    // 各グループ内は排他タグ (*_done) で 1 つしか起きない。
    // どのイベントが起きるかでストーリーが変わり、覚えるスキルも変わる。
    //
    // 正グループ (高ランクほど起きやすい):
    //   恩師系 / 修練系 / 自然系 / 救命系
    // 負グループ (低ランクほど起きやすい):
    //   臨死系 / 裏切り系
    //
    // learnsSkillId は後日スキル定義と合わせて設定する。
    // ================================================================
    public static class RareEvents
    {
        public static IEnumerable<ReinLifeEvent> All()
        {
            foreach (var ev in MentorGroup()) yield return ev;
            foreach (var ev in TrainingGroup()) yield return ev;
            foreach (var ev in NatureGroup()) yield return ev;
            foreach (var ev in SaveLifeGroup()) yield return ev;
            foreach (var ev in NearDeathGroup()) yield return ev;
            foreach (var ev in BetrayalGroup()) yield return ev;
        }

        // ─────────────────────────────────────────
        // 恩師系 (stage 3 / バフ・知恵系)
        // ─────────────────────────────────────────
        #region 恩師系
        private static IEnumerable<ReinLifeEvent> MentorGroup()
        {
            // A: 恩師の教え → 知恵系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_mentor_a",
                eventType = ReinEventType.Happy,
                sentence = "人生を変える一言をくれた恩師に出会った。その言葉は何年経っても色褪せなかった。",
                startAge = 15,
                endAge = 30,
                baseWeight = 0.5f,
                blockedByLifeTags = new() { "rare_mentor_done" },
                grantsLifeTags = new() { "rare_mentor_done" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "max",
                // learnsSkillId = "skill_mentor_wisdom", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };

            // B: 恩師の背中 → 鼓舞系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_mentor_b",
                eventType = ReinEventType.Happy,
                sentence = "何も語らず、ただ背中を見せてくれた人がいた。その働きぶりが全てを教えてくれた。",
                startAge = 18,
                endAge = 35,
                baseWeight = 0.45f,
                blockedByLifeTags = new() { "rare_mentor_done" },
                grantsLifeTags = new() { "rare_mentor_done" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "max",
                // learnsSkillId = "skill_mentor_inspire", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 2 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 1 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };

            // C: 恩師の叱責 → 看破系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_mentor_c",
                eventType = ReinEventType.Happy,
                sentence = "厳しく叱られた。理不尽だと思った。だが、あの言葉がなければ今の自分はいない。",
                startAge = 14,
                endAge = 28,
                baseWeight = 0.5f,
                blockedByLifeTags = new() { "rare_mentor_done" },
                grantsLifeTags = new() { "rare_mentor_done" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "max",
                // learnsSkillId = "skill_mentor_insight", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 2 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 1 },
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 修練系 (stage 4 / 攻撃・集中系)
        // ─────────────────────────────────────────
        #region 修練系
        private static IEnumerable<ReinLifeEvent> TrainingGroup()
        {
            // A: 孤独な修練 → 集中系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_train_a",
                sentence = "誰にも言わず、一人で技を磨き続けた時期があった。夜明け前に起きて、誰もいない場所で繰り返した。",
                startAge = 25,
                endAge = 50,
                baseWeight = 0.4f,
                blockedByLifeTags = new() { "rare_training_done" },
                grantsLifeTags = new() { "rare_training_done" },
                requireYearsAfterJob = 3,
                eventStage = 4,
                statCompareCount = 3,
                statCompareMode = "min",
                // learnsSkillId = "skill_train_focus", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                },
            };

            // B: 異分野への挑戦 → 応用系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_train_b",
                sentence = "全く畑違いの分野に手を出した。遠回りに思えたが、やがて本業に活きた。",
                startAge = 28,
                endAge = 50,
                baseWeight = 0.35f,
                blockedByLifeTags = new() { "rare_training_done" },
                grantsLifeTags = new() { "rare_training_done" },
                requireYearsAfterJob = 3,
                eventStage = 4,
                statCompareCount = 3,
                statCompareMode = "min",
                // learnsSkillId = "skill_train_versatile", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.AT, value = 1 },
                        },
                    },
                },
            };

            // C: 限界への挑戦 → 突破系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_train_c",
                sentence = "もう無理だと思った壁を、意地だけで超えた。体が覚えていた。",
                startAge = 22,
                endAge = 45,
                baseWeight = 0.4f,
                blockedByLifeTags = new() { "rare_training_done" },
                grantsLifeTags = new() { "rare_training_done" },
                requireYearsAfterJob = 2,
                eventStage = 4,
                statCompareCount = 3,
                statCompareMode = "min",
                // learnsSkillId = "skill_train_breakthrough", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 3 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 自然系 (stage 3 / 属性・防御系)
        // ─────────────────────────────────────────
        #region 自然系
        private static IEnumerable<ReinLifeEvent> NatureGroup()
        {
            // A: 雷雨体験 → 自然の力系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_nature_a",
                sentence = "山奥で雷雨に遭い、岩陰で一夜を明かした。自然の圧倒的な力を前に、何もできなかった。",
                startAge = 12,
                endAge = 40,
                baseWeight = 0.4f,
                blockedByLifeTags = new() { "rare_nature_done" },
                grantsLifeTags = new() { "rare_nature_done" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "max",
                // learnsSkillId = "skill_nature_storm", // TODO
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
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };

            // B: 満天の星空 → 精神系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_nature_b",
                sentence = "旅先で見た満天の星空に、しばらく動けなかった。自分がどれほど小さいかを知った夜だった。",
                startAge = 10,
                endAge = 50,
                baseWeight = 0.45f,
                blockedByLifeTags = new() { "rare_nature_done" },
                grantsLifeTags = new() { "rare_nature_done" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "max",
                // learnsSkillId = "skill_nature_starlight", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                },
            };

            // C: 海での遭難 → 生存系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_nature_c",
                sentence = "海で流された。波に揉まれながら、必死に岸を目指した。生きて帰れたのは運だけだった。",
                startAge = 14,
                endAge = 45,
                baseWeight = 0.35f,
                blockedByLifeTags = new() { "rare_nature_done" },
                grantsLifeTags = new() { "rare_nature_done" },
                eventStage = 3,
                statCompareCount = 3,
                statCompareMode = "max",
                // learnsSkillId = "skill_nature_survival", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 2 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 救命系 (stage 4 / 回復系)
        // ─────────────────────────────────────────
        #region 救命系
        private static IEnumerable<ReinLifeEvent> SaveLifeGroup()
        {
            // A: 人を救った → 回復スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_save_a",
                sentence = "目の前で人が倒れた。周囲が固まる中、体が勝手に動いていた。命をつないだ。",
                startAge = 20,
                endAge = 55,
                baseWeight = 0.35f,
                blockedByLifeTags = new() { "rare_save_done" },
                grantsLifeTags = new() { "rare_save_done" },
                eventStage = 4,
                statCompareCount = 3,
                statCompareMode = "min",
                // learnsSkillId = "skill_save_heal", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                            new StatBonus { stat = StatKind.DF, value = 2 },
                        },
                    },
                },
            };

            // B: 災害ボランティア → 守護系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_save_b",
                sentence = "被災地にボランティアとして入った。泥を掻き出しながら、誰かのために動くことの意味を知った。",
                startAge = 18,
                endAge = 55,
                baseWeight = 0.35f,
                blockedByLifeTags = new() { "rare_save_done" },
                grantsLifeTags = new() { "rare_save_done" },
                eventStage = 4,
                statCompareCount = 3,
                statCompareMode = "min",
                // learnsSkillId = "skill_save_guard", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };

            // C: 迷子の子供を保護 → 庇護系スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_save_c",
                sentence = "夜道で泣いている子供を見つけた。交番まで手を引いて歩いた。親が来るまでそばにいた。",
                startAge = 22,
                endAge = 60,
                baseWeight = 0.4f,
                blockedByLifeTags = new() { "rare_save_done" },
                grantsLifeTags = new() { "rare_save_done" },
                eventStage = 4,
                statCompareCount = 3,
                statCompareMode = "min",
                // learnsSkillId = "skill_save_protect", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 4 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 臨死系 (stage -1 / 低ランクほど起きやすい / 復活・不屈系)
        // ─────────────────────────────────────────
        #region 臨死系
        private static IEnumerable<ReinLifeEvent> NearDeathGroup()
        {
            // A: 大事故 → 不屈スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_neardeath_a",
                eventType = ReinEventType.Sad,
                sentence = "大きな事故に遭い、意識が戻るまで数日かかった。目が覚めたとき、すべてが違って見えた。",
                startAge = 18,
                endAge = 60,
                baseWeight = 0.20f,
                blockedByLifeTags = new() { "rare_neardeath_done" },
                grantsLifeTags = new() { "rare_neardeath_done" },
                eventStage = -1,
                statCompareCount = 3,
                statCompareMode = "avg",
                // learnsSkillId = "skill_nd_tenacity", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };

            // B: 重病からの回復 → 再生スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_neardeath_b",
                eventType = ReinEventType.Sad,
                sentence = "重い病にかかり、長期間の入院を余儀なくされた。退院の日、外の空気が肺に染みた。",
                startAge = 15,
                endAge = 65,
                baseWeight = 0.18f,
                blockedByLifeTags = new() { "rare_neardeath_done" },
                grantsLifeTags = new() { "rare_neardeath_done" },
                eventStage = -1,
                statCompareCount = 3,
                statCompareMode = "avg",
                // learnsSkillId = "skill_nd_recovery", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 4 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                            new StatBonus { stat = StatKind.DF, value = 1 },
                        },
                    },
                },
            };

            // C: 災害に巻き込まれた → 生存本能スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_neardeath_c",
                eventType = ReinEventType.Sad,
                sentence = "地震で建物の下敷きになった。暗闇の中で助けを待ち続けた。救出されたとき、涙が止まらなかった。",
                startAge = 10,
                endAge = 70,
                baseWeight = 0.15f,
                blockedByLifeTags = new() { "rare_neardeath_done" },
                grantsLifeTags = new() { "rare_neardeath_done" },
                eventStage = -1,
                statCompareCount = 3,
                statCompareMode = "avg",
                // learnsSkillId = "skill_nd_survival", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 4 },
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 3 },
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 裏切り系 (stage -1 / 低ランクほど起きやすい / カウンター・反撃系)
        // ─────────────────────────────────────────
        #region 裏切り系
        private static IEnumerable<ReinLifeEvent> BetrayalGroup()
        {
            // A: 信頼の裏切り → 反撃スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_betray_a",
                eventType = ReinEventType.Sad,
                sentence = "信頼していた人に裏切られた。しばらく何も手につかなかったが、やがて立ち上がった。",
                startAge = 22,
                endAge = 55,
                baseWeight = 0.18f,
                blockedByLifeTags = new() { "rare_betrayal_done" },
                grantsLifeTags = new() { "rare_betrayal_done" },
                eventStage = -1,
                statCompareCount = 3,
                statCompareMode = "avg",
                // learnsSkillId = "skill_betray_counter", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                },
            };

            // B: 金銭トラブル → 警戒スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_betray_b",
                eventType = ReinEventType.Sad,
                sentence = "騙されて大金を失った。悔しさよりも、自分の甘さが情けなかった。二度と同じ目には遭わないと誓った。",
                startAge = 20,
                endAge = 55,
                baseWeight = 0.15f,
                blockedByLifeTags = new() { "rare_betrayal_done" },
                grantsLifeTags = new() { "rare_betrayal_done" },
                eventStage = -1,
                statCompareCount = 3,
                statCompareMode = "avg",
                // learnsSkillId = "skill_betray_vigilance", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 2 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                        },
                    },
                },
            };

            // C: 濡れ衣 → 耐久スキル
            yield return new ReinLifeEvent
            {
                eventId = "ev_rare_betray_c",
                eventType = ReinEventType.Sad,
                sentence = "やっていないことで責められた。誰も味方がいなかった。それでも折れなかった。",
                startAge = 15,
                endAge = 50,
                baseWeight = 0.15f,
                blockedByLifeTags = new() { "rare_betrayal_done" },
                grantsLifeTags = new() { "rare_betrayal_done" },
                eventStage = -1,
                statCompareCount = 3,
                statCompareMode = "avg",
                // learnsSkillId = "skill_betray_endure", // TODO
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.DF, value = 2 },
                            new StatBonus { stat = StatKind.MDF, value = 3 },
                        },
                    },
                },
            };
        }
        #endregion
    }
}
