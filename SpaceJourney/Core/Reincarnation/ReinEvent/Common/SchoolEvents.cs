using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // SchoolEvents (学校・学校生活)
    // ================================================================
    // 義務教育(小中) は必ず通る確定発火イベント。
    // 高校以降は通常確定だが「貧しい家庭」の場合は確率発火になる可能性あり。
    // 大学進学/院進学は「高学歴必須職業」の場合のみルート最終年強制発火。
    //
    // ライフタグ:
    //   school_elem_in / school_elem_grad
    //   school_jr_in   / school_jr_grad
    //   school_high_in / school_high_grad
    //   school_univ_in / school_univ_grad
    //   school_grad_school_grad
    //
    // 文化祭は起業家ルート(ent_call保有)なら発火しない (blockedByLifeTags)。
    // ================================================================
    public static class SchoolEvents
    {
        // 大学進学が事実上必須のジョブ (Tier S + Tier A)
        // - Tier S: 裁判官 / 国際弁護士 (貧乏出身もブロック)
        // - Tier A: MSF外科医 / 研究者 / ロケットエンジニア / 宇宙飛行士
        // ev_school_no_high はこれらのジョブでは発火させない (高校中退ルート不可)
        // 代わりに ev_school_poor_struggle (アルバイト勉強) で逆境ストーリーを表現する
        private static readonly List<string> HighAcademicJobs = new()
        {
            "knight_saiban",
            "knight_bengoshi",
            "archer_msf",
            "mage_kenkyusha",
            "lancer_rocket",
            "warrior_uchu",
        };

        public static IEnumerable<ReinLifeEvent> All()
        {
            foreach (var ev in RouteEvents()) yield return ev;
            foreach (var ev in PoorStruggle()) yield return ev;
            foreach (var ev in HighSchoolElectives()) yield return ev;
            foreach (var ev in UniversityRoute()) yield return ev;
        }

        // ─────────────────────────────────────────
        // ルート (必ず通る) 入学/卒業
        // ─────────────────────────────────────────
        #region 義務教育ルート
        private static IEnumerable<ReinLifeEvent> RouteEvents()
        {
            // 小学校入学
            yield return new ReinLifeEvent
            {
                eventId = "ev_school_elem_in",
                sentence = "小学校に入学した。",
                startAge = 6,
                endAge = 6,
                baseWeight = 1.0f,
                grantsLifeTags = new() { "school_elem_in" },
            };

            // 小学校卒業
            yield return new ReinLifeEvent
            {
                eventId = "ev_school_elem_grad",
                sentence = "小学校を卒業した。",
                startAge = 12,
                endAge = 12,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "school_elem_in" },
                grantsLifeTags = new() { "school_elem_grad" },
            };

            // 中学校入学
            yield return new ReinLifeEvent
            {
                eventId = "ev_school_jr_in",
                sentence = "中学校に入学した。",
                startAge = 12,
                endAge = 12,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "school_elem_grad" },
                grantsLifeTags = new() { "school_jr_in" },
            };

            // 中学校卒業
            yield return new ReinLifeEvent
            {
                eventId = "ev_school_jr_grad",
                sentence = "中学校を卒業した。",
                startAge = 15,
                endAge = 15,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "school_jr_in" },
                grantsLifeTags = new() { "school_jr_grad" },
            };

            // 高校入学 (貧しい家庭が高校進学を諦めた場合は発火しない)
            yield return new ReinLifeEvent
            {
                eventId = "ev_school_high_in",
                sentence = "高校に入学した。",
                startAge = 15,
                endAge = 15,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "school_jr_grad" },
                blockedByLifeTags = new() { "school_no_high" },
                grantsLifeTags = new() { "school_high_in" },
            };

            // 貧しい家庭の場合、低確率で進学を諦める
            // ただし高学歴必須職 (Tier S + Tier A) では発火しない
            // (これらのジョブでは ev_school_poor_struggle で「苦学生ルート」として表現する)
            yield return new ReinLifeEvent
            {
                eventId = "ev_school_no_high",
                sentence = "家計を支えるために働き始めた。高校には行かなかった。",
                startAge = 15,
                endAge = 15,
                baseWeight = 0.15f,
                requiresAnyLifeTag = new() { "birth_poor" },
                blockedByLifeTags = new() { "school_high_in" },
                grantsLifeTags = new() { "school_no_high" },
                excludedJobIds = HighAcademicJobs,
            };

            // 高校卒業
            yield return new ReinLifeEvent
            {
                eventId = "ev_school_high_grad",
                sentence = "高校を卒業した。",
                startAge = 18,
                endAge = 18,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "school_high_in" },
                grantsLifeTags = new() { "school_high_grad" },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 苦学ルート (高学歴必須職 × 貧しい家庭)
        // ─────────────────────────────────────────
        // 大学進学が必須のジョブで、しかも貧しい家庭に生まれた場合、
        // アルバイトで家計を支えながら勉学に励む苦学エピソードを発火させる。
        // 高校在学中の age 15-17 で発火し、本人の精神力(MDF)と知力(MAT)を伸ばす。
        #region 苦学ルート
        private static IEnumerable<ReinLifeEvent> PoorStruggle()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_school_poor_struggle",
                sentence = "家計を支えるためにアルバイトをしながら、夜は遅くまで勉強を続けた。学費の心配は常にあった。",
                startAge = 15,
                endAge = 17,
                baseWeight = 1.0f, // 条件揃えば確定発火
                requiresAnyLifeTag = new() { "birth_poor" },
                blockedByLifeTags = new() { "school_poor_struggle" },
                grantsLifeTags = new() { "school_poor_struggle" },
                relatedJobIds = HighAcademicJobs,
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.MAT, eventFactorPt = 3 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MAT, value = 2 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                            new StatBonus { stat = StatKind.AT,  value = 1 },
                        },
                    },
                },
            };
        }
        #endregion

        // ─────────────────────────────────────────
        // 高校時代の選択イベント
        // ─────────────────────────────────────────
        #region 高校イベント
        private static IEnumerable<ReinLifeEvent> HighSchoolElectives()
        {
            // 部活 (中高通して1イベントに統合)
            yield return new ReinLifeEvent
            {
                eventId = "ev_club_high",
                sentence = "学生時代を通じて部活に打ち込んだ。練習量が一気に増えた。",
                startAge = 14,
                endAge = 17,
                baseWeight = 0.7f,
                requiresAnyLifeTag = new() { "school_jr_in" },
                grantsLifeTags = new() { "club_high" },
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
                            new StatEffect { stat = StatKind.AGI, eventFactorPt = 1 },
                            new StatEffect { stat = StatKind.DF, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 2 },
                            new StatBonus { stat = StatKind.AGI, value = 1 },
                        },
                    },
                },
            };

            // 全国大会出場 (地方大会はここに統合)
            yield return new ReinLifeEvent
            {
                eventId = "ev_tournament_national",
                sentence = "部活で地方大会を勝ち抜き、全国の舞台に立った。",
                startAge = 17,
                endAge = 18,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "club_high" },
                grantsLifeTags = new() { "tournament_national" },
                eventStage = 4,
                statCompareCount = 3,
                statCompareMode = "min",
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
                            new StatBonus { stat = StatKind.AT, value = 3 },
                            new StatBonus { stat = StatKind.AGI, value = 2 },
                        },
                    },
                },
            };

            // 全国優勝
            yield return new ReinLifeEvent
            {
                eventId = "ev_tournament_winner",
                sentence = "全国大会で優勝した。表彰台のいちばん高い場所に立った。",
                startAge = 17,
                endAge = 18,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "tournament_national" },
                eventStage = 5,
                statCompareCount = 3,
                statCompareMode = "min",
                options = new()
                {
                    new ReinSentenceOption
                    {
                        baseWeight = 1.0f,
                        statEffects = new()
                        {
                            new StatEffect { stat = StatKind.AT, eventFactorPt = 5 },
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 3 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.AT, value = 4 },
                            new StatBonus { stat = StatKind.MDF, value = 2 },
                        },
                    },
                },
            };

            // 文化祭 (起業家ルートでは発火しない)
            yield return new ReinLifeEvent
            {
                eventId = "ev_culture_festival",
                grantsLifeTags = new() { "school_event_done" },
                sentence = "文化祭でクラスの出し物に汗を流した。打ち上げの空は赤かった。",
                startAge = 14,
                endAge = 17,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "school_jr_in" },
                blockedByLifeTags = new() { "ent_call", "ent_train", "school_event_done" }, // 起業家ルートでは発火しない
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
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                        grantsStats = new()
                        {
                            new StatBonus { stat = StatKind.MDF, value = 1 },
                        },
                    },
                },
            };

            // 修学旅行 (中学)
            yield return new ReinLifeEvent
            {
                eventId = "ev_school_trip_jr",
                grantsLifeTags = new() { "school_event_done" },
                blockedByLifeTags = new() { "school_event_done" },
                sentence = "中学校の修学旅行で友人と夜更かしした。先生に怒られた。",
                startAge = 14,
                endAge = 14,
                baseWeight = 0.8f,
                requiresAnyLifeTag = new() { "school_jr_in" },
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
                            new StatEffect { stat = StatKind.MDF, eventFactorPt = 1 },
                        },
                    },
                },
            };

            // 球技大会
            yield return new ReinLifeEvent
            {
                eventId = "ev_class_tournament",
                grantsLifeTags = new() { "school_event_done" },
                blockedByLifeTags = new() { "school_event_done" },
                sentence = "球技大会でクラスのために走った。",
                startAge = 13,
                endAge = 17,
                baseWeight = 0.5f,
                requiresAnyLifeTag = new() { "school_jr_in" },
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
        }
        #endregion

        // ─────────────────────────────────────────
        // 大学進学 (高学歴必須職業のときのみ強制発火)
        // ─────────────────────────────────────────
        // ev_school_university の relatedJobIds に高学歴職業を入れて、
        // ルート最終年強制発火で確実に発火させる。
        // 該当ジョブ:
        //   knight_saiban (裁判官) / knight_bengoshi (国際弁護人) /
        //   archer_msf (MSF外科医) / mage_kenkyusha (研究者) /
        //   warrior_uchu (宇宙飛行士) / lancer_rocket (ロケットエンジニア) /
        //   archer_golfer (ゴルファー: スポーツ推薦想定) / mage_kult (カルト教祖: 知識家想定)
        // ─────────────────────────────────────────
        #region 大学進学
        private static IEnumerable<ReinLifeEvent> UniversityRoute()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_school_univ_in",
                sentence = "大学に進学した。専門書を抱えて初めてのキャンパスを歩いた。",
                startAge = 18,
                endAge = 19,
                baseWeight = 1.0f, // 高学歴必須ジョブでは確定発火
                requiresAnyLifeTag = new() { "school_high_grad" },
                grantsLifeTags = new() { "school_univ_in" },
                // archer_msf は医学部に直行するので generic 大学進学を使わない (ev_msf_03 で代替)
                relatedJobIds = new()
                {
                    "knight_saiban",
                    "knight_bengoshi",
                    "mage_kenkyusha",
                    "warrior_uchu",
                    "lancer_rocket",
                },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_school_univ_grad",
                sentence = "大学を卒業した。",
                startAge = 22,
                endAge = 23,
                baseWeight = 1.0f,
                requiresAnyLifeTag = new() { "school_univ_in" },
                grantsLifeTags = new() { "school_univ_grad" },
            };

            yield return new ReinLifeEvent
            {
                eventId = "ev_school_grad_school",
                sentence = "大学院に進学した。",
                startAge = 22,
                endAge = 23,
                baseWeight = 1.0f, // 大学院必須ジョブでは確定発火
                requiresAnyLifeTag = new() { "school_univ_grad" },
                grantsLifeTags = new() { "school_grad_school_in" },
                // 大学院進学が必須のジョブ。MSF は医学部 6 年のため大学院に行かない (専攻医研修へ)
                relatedJobIds = new()
                {
                    "mage_kenkyusha",
                    "lancer_rocket",
                },
            };
        }
        #endregion
    }
}
