// WaveSpawnTableSO.cs
// このクラスで何をするか：
// WAVEごとにフィールドへキューブをポップさせるための「出現テーブル」定義。
// - どのランクのエリアで使うか（minMapRank / maxMapRank）
// - 何がどの確率・重みで出るか（entries）
// - Enemyのように複数種類あるキューブはvariantsで重み抽選
// ScriptableObjectなので難易度ごとに別SOを作って差し替える運用を想定。

using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    [CreateAssetMenu(
        fileName = "WaveSpawnTable",
        menuName = "SteraCube/SpaceJourney/WaveSpawnTable")]
    public class WaveSpawnTableSO : ScriptableObject
    {
        [Tooltip("このテーブルが使われるマップランクの範囲")]
        [SerializeField] private List<RankEntry> rankEntries = new List<RankEntry>();

        public IReadOnlyList<RankEntry> RankEntries => rankEntries;

        /// <summary>
        /// 指定したマップランクに対応する RankEntry を返す。
        /// 複数マッチする場合は最初にヒットしたものを返す。
        /// 見つからない場合は null。
        /// </summary>
        public RankEntry GetEntryForRank(int mapRank)
        {
            foreach (var re in rankEntries)
            {
                if (mapRank >= re.MinMapRank && mapRank <= re.MaxMapRank)
                    return re;
            }
            return null;
        }
    }

    // =========================================================
    // ランク × エントリーのセット
    // =========================================================

    [System.Serializable]
    public class RankEntry
    {
        [Header("対応マップランク")]
        [SerializeField] private int minMapRank = 1;
        [SerializeField] private int maxMapRank = 1;

        [Header("地面の種類（エリアの雰囲気）")]
        [Tooltip("このエリアでキューブ本体として使われる地面。weightで重み付き抽選。")]
        [SerializeField] private List<GroundVariant> groundVariants = new List<GroundVariant>();

        [Header("出現エントリー一覧")]
        [SerializeField] private List<SpawnEntry> entries = new List<SpawnEntry>();

        public int MinMapRank => minMapRank;
        public int MaxMapRank => maxMapRank;
        public IReadOnlyList<SpawnEntry> Entries => entries;
        public IReadOnlyList<GroundVariant> GroundVariants => groundVariants;

        /// <summary>
        /// groundVariants の中から weight に従ってランダムに1つ選んで地面IDを返す。
        /// groundVariants が空の場合は null を返す。
        /// </summary>
        public string PickGroundEntryId()
        {
            if (groundVariants == null || groundVariants.Count == 0) return null;

            int total = 0;
            foreach (var g in groundVariants)
                total += Mathf.Max(0, g.Weight);

            if (total <= 0)
                return groundVariants[Random.Range(0, groundVariants.Count)].GroundEntryId;

            int r = Random.Range(0, total);
            foreach (var g in groundVariants)
            {
                int w = Mathf.Max(0, g.Weight);
                if (r < w) return g.GroundEntryId;
                r -= w;
            }

            return groundVariants[groundVariants.Count - 1].GroundEntryId;
        }
    }

    // =========================================================
    // 地面の種類（エリアごとの見た目）
    // =========================================================

    [System.Serializable]
    public class GroundVariant
    {
        [Tooltip("MasterDatabase.cubeGroundEntries のID（例：Grass, Ice, Sand）")]
        [SerializeField] private string groundEntryId;

        [Tooltip("抽選重み。大きいほど出やすい。")]
        [SerializeField] private int weight = 1;

        public string GroundEntryId => groundEntryId;
        public int Weight => weight;
    }

    // =========================================================
    // 1種類のキューブの出現設定
    // =========================================================

    [System.Serializable]
    public class SpawnEntry
    {
        [Header("キューブ種別")]
        [SerializeField] private CubeKind cubeKind;

        [Header("出現確率・数")]
        [Tooltip("このエントリーが抽選に参加する確率（0〜1）")]
        [SerializeField, Range(0f, 1f)] private float spawnRate = 1f;

        [Tooltip("spawnRateに関わらず必ず出す最低数")]
        [SerializeField] private int minCount = 0;

        [Tooltip("このエントリーから出る最大数")]
        [SerializeField] private int maxCount = 1;

        [Header("バリエーション（Enemyなど複数種ある場合）")]
        [Tooltip("cubeKindがEnemyなど複数種ある場合に使う。空の場合はcubeKindだけで決定。")]
        [SerializeField] private List<CubeVariant> variants = new List<CubeVariant>();

        public CubeKind CubeKind => cubeKind;
        public float SpawnRate => spawnRate;
        public int MinCount => minCount;
        public int MaxCount => maxCount;
        public IReadOnlyList<CubeVariant> Variants => variants;

        /// <summary>
        /// variantsの中からweightに従ってランダムに1つ選んで返す。
        /// variantsが空の場合はnullを返す（cubeKindだけで判断する）。
        /// </summary>
        public CubeVariant PickVariant()
        {
            if (variants == null || variants.Count == 0) return null;

            int total = 0;
            foreach (var v in variants)
                total += Mathf.Max(0, v.Weight);

            if (total <= 0)
                return variants[Random.Range(0, variants.Count)];

            int r = Random.Range(0, total);
            foreach (var v in variants)
            {
                int w = Mathf.Max(0, v.Weight);
                if (r < w) return v;
                r -= w;
            }

            return variants[variants.Count - 1];
        }
    }

    // =========================================================
    // 具体的なキューブID × 重み（Enemyの種類など）
    // =========================================================

    [System.Serializable]
    public class CubeVariant
    {
        [Tooltip("EnemyGroupDefinitionSO の groupId（例：EnemyGroup_Goblin_Weak）")]
        [SerializeField] private string enemyGroupId;

        [Tooltip("抽選重み。大きいほど出やすい。")]
        [SerializeField] private int weight = 1;

        public string EnemyGroupId => enemyGroupId;
        public int Weight => weight;
    }
}