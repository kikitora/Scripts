using System.Collections.Generic;

namespace SteraCube.SpaceJourney.LifeEvents
{
    // ================================================================
    // FriendEvents (友人関係)
    // ================================================================
    // 親友(bff)を1人作り、その後の人生で何度か関連イベントが起きる。
    // ================================================================
    public static class FriendEvents
    {
        public static IEnumerable<ReinLifeEvent> All()
        {
            yield return new ReinLifeEvent
            {
                eventId = "ev_meet_bff",
                sentence = "親友と呼べる友人ができた。くだらない冗談で腹を抱えて笑った夕方だった。",
                startAge = 8,
                endAge = 20,
                baseWeight = 0.7f,
                grantsLifeTags = new() { "bff" },
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

            yield return new ReinLifeEvent
            {
                eventId = "ev_friend_memory_a",
                sentence = "親友と河原で花火をした。火薬の匂いと笑い声が夏の記憶になった。",
                startAge = 14,
                endAge = 35,
                baseWeight = 0.6f,
                requiresAnyLifeTag = new() { "bff" },
                blockedByLifeTags = new() { "friend_mem_a", "friend_memory_done" },
                grantsLifeTags = new() { "friend_mem_a", "friend_memory_done" },
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

            yield return new ReinLifeEvent
            {
                eventId = "ev_friend_help",
                sentence = "親友が会社を解雇され、酒を手に家に転がり込んできた。一晩中話を聞き、翌朝には紹介状を書いた。",
                startAge = 30,
                endAge = 40,
                baseWeight = 0.3f,
                requiresAnyLifeTag = new() { "bff" },
                blockedByLifeTags = new() { "friend_helped" },
                grantsLifeTags = new() { "friend_helped" },
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
                eventId = "ev_friend_loss",
                sentence = "親友と金の貸し借りが原因で衝突し、絶縁した。それ以来、連絡は途絶えた。",
                startAge = 30,
                endAge = 70,
                baseWeight = 0.2f,
                requiresAnyLifeTag = new() { "bff" },
                blockedByLifeTags = new() { "friend_lost" },
                grantsLifeTags = new() { "friend_lost" },
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
    }
}
