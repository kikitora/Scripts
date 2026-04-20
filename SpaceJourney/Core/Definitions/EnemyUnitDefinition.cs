using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 専用敵ユニット定義 (ボディ/ソウル分離なし、1体完結)。
    /// ステータスはランク別に固定値で定義し、スキルと行動優先度も固定。
    /// BattleManager で SpaceJourneyUnit (UseFixedStats) として生成される。
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyUnit", menuName = "SteraCube/EnemyUnitDefinition")]
    public class EnemyUnitDefinition : ScriptableObject
    {
        // ════════════════════════════════════════
        // 基本情報
        // ════════════════════════════════════════

        [Header("基本情報")]
        [Tooltip("一意なID (例: enemy_skeleton, enemy_ghost)")]
        public string enemyId;

        [Tooltip("表示名")]
        public string displayName;

        [Tooltip("アイコン")]
        public Sprite icon;

        [Tooltip("3Dモデルの Prefab")]
        public GameObject modelPrefab;

        // ════════════════════════════════════════
        // ランク別ステータス
        // ════════════════════════════════════════

        [Header("ランク別ステータス")]
        [Tooltip("ランク1〜対応のステータス定義。リスト index=0 が rank=1。")]
        public List<RankStats> rankStatsList = new();

        [Serializable]
        public class RankStats
        {
            [Tooltip("対応ランク (表示用。index+1 と一致させる)")]
            public int rank = 1;
            public int maxHp = 100;
            public int at = 20;
            public int df = 15;
            public int mat = 10;
            public int mdf = 10;
            public int agi = 10;
        }

        /// <summary>指定ランクのステータスを返す。範囲外なら最も近いランクにクランプ。</summary>
        public RankStats GetStatsForRank(int rank)
        {
            if (rankStatsList == null || rankStatsList.Count == 0) return null;
            int index = Mathf.Clamp(rank - 1, 0, rankStatsList.Count - 1);
            return rankStatsList[index];
        }

        // ════════════════════════════════════════
        // 固定行動リスト (優先度順)
        // ════════════════════════════════════════

        [Header("行動リスト (上から優先)")]
        [Tooltip("上から順に条件チェック → 最初に条件を満たしたスキルを実行。")]
        public List<BattleActionEntry> actionList = new();

        // ════════════════════════════════════════
        // ユーティリティ
        // ════════════════════════════════════════

        /// <summary>
        /// この定義から SpaceJourneyUnit を生成する (UseFixedStats モード)。
        /// </summary>
        public SpaceJourneyUnit CreateUnit(int rank, float morale = 100f)
        {
            var stats = GetStatsForRank(rank);
            if (stats == null)
            {
                Debug.LogError($"[EnemyUnitDefinition] No stats for rank {rank} in {enemyId}");
                return null;
            }

            var unit = SpaceJourneyUnit.CreateSummonedUnit(
                hp: stats.maxHp,
                at: stats.at,
                df: stats.df,
                agi: stats.agi,
                mat: stats.mat,
                mdf: stats.mdf,
                isBarricade: false
            );
            // 召喚体フラグは外す (通常の敵として扱う)
            unit.IsSummoned = false;
            unit.IsBarricade = false;
            // 士気
            unit.MoraleMultiplier = Mathf.Clamp(morale / 100f, 0.1f, 1f);

            return unit;
        }
    }
}
