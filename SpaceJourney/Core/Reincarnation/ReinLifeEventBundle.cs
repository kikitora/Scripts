using System.Collections.Generic;

namespace SteraCube.SpaceJourney
{
    // ================================================================
    // ReinLifeEventBundle
    // ================================================================
    // 全ライフイベントを束ねる単一エントリ。
    //
    // ・データソースは ReinEvent/ 配下の C# ファイル群（傾向別 / 共通別）
    // ・MasterDatabase はこの All を参照する
    // ・初回アクセス時に Build() で全イベントを集めてキャッシュする
    //
    // 配下構成：
    //   ReinEvent/Tendency/Warrior_Events.cs   など 傾向別5ファイル
    //   ReinEvent/Common/                       共通イベント（誕生・幼少期・人生節目など）
    // ================================================================
    public static class ReinLifeEventBundle
    {
        private static List<ReinLifeEvent> _cache;

        /// <summary>
        /// 全ライフイベントのキャッシュ済みリスト（初回アクセスで Build される）。
        /// </summary>
        public static IReadOnlyList<ReinLifeEvent> All
        {
            get
            {
                if (_cache == null) _cache = Build();
                return _cache;
            }
        }

        /// <summary>
        /// キャッシュを破棄する。Editor でホットリロードしたい時などに使う。
        /// </summary>
        public static void ClearCache()
        {
            _cache = null;
        }

        private static List<ReinLifeEvent> Build()
        {
            var list = new List<ReinLifeEvent>(1024);

            // ── 生業イベント（傾向別） ──
            list.AddRange(LifeEvents.Warrior_Events.All());
            list.AddRange(LifeEvents.Knight_Events.All());
            list.AddRange(LifeEvents.Archer_Events.All());
            list.AddRange(LifeEvents.Mage_Events.All());
            list.AddRange(LifeEvents.Lancer_Events.All());

            // ── 共通イベント ──
            list.AddRange(LifeEvents.BirthEvents.All());
            list.AddRange(LifeEvents.SchoolEvents.All());
            list.AddRange(LifeEvents.LoveEvents.All());
            list.AddRange(LifeEvents.FriendEvents.All());
            list.AddRange(LifeEvents.LifeEndEvents.All());

            return list;
        }
    }
}
