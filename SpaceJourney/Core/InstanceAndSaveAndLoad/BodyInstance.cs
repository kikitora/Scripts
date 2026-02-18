using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// ボディ個体（JSON保存対象）。
    /// - raceId / bodyJobId / weaponId をIDで保持
    /// - 確定ステータス（maxHp/at/df/agi/mat/mdf）を保存
    /// - weaponCandidateIds もインスタンスに保持（ユーザー方針）
    /// </summary>
    [Serializable]
    public class BodyInstance
    {
        [SerializeField] private string instanceId;
        // ===== ID（JSON保存対象）=====
        [SerializeField] private string raceId;
        [SerializeField] private string bodyJobId;
        [SerializeField] private string weaponId;

        [Header("候補武器（WeaponId一覧）")]
        [Tooltip("この個体が持ちうる武器候補。WeaponDefinition.weaponId を登録する。")]
        [SerializeField] private List<string> weaponCandidateIds = new List<string>();

        // ===== 確定ステ（JSON保存対象）=====
        [SerializeField] private int maxHp;
        [SerializeField] private int at;
        [SerializeField] private int df;
        [SerializeField] private int agi;
        [SerializeField] private int mat;
        [SerializeField] private int mdf;

        // ===== 実行時キャッシュ（保存しない）=====
        [NonSerialized] private RaceDefinition cachedRace;
        [NonSerialized] private BodyJobDefinition cachedBodyJob;
        [NonSerialized] private WeaponDefinition cachedWeapon;

        // ─────────────────────────────
        // 1) セーブ復元用コンストラクタ（保存値そのまま）
        // ─────────────────────────────
        public BodyInstance(
            string raceId,
            string bodyJobId,
            string weaponId,
            List<string> weaponCandidateIds,
            int maxHp,
            int at,
            int df,
            int agi,
            int mat,
            int mdf)
        {
            this.raceId = raceId;
            this.bodyJobId = bodyJobId;
            this.weaponId = weaponId;
            this.weaponCandidateIds = weaponCandidateIds != null ? new List<string>(weaponCandidateIds) : new List<string>();

            this.maxHp = maxHp;
            this.at = at;
            this.df = df;
            this.agi = agi;
            this.mat = mat;
            this.mdf = mdf;
        }

        /// <summary>
        /// WorldStateRuntime が存在すれば、この BodyInstance を WorldState.ExBodys に登録する。
        /// </summary>
        private static void RegisterBodyToCurrentWorld(BodyInstance body)
        {
            if (body == null) return;

            var runtime = UnityEngine.Object.FindObjectOfType<WorldStateRuntime>();
            if (runtime == null) return;

            var world = runtime.CurrentWorld;
            if (world == null) return;

            world.RegisterBodyInstance(body);
        }


        /// <summary>
        /// instanceId が未設定の場合に WorldState 経由で一意なIDを発行してセットする。
        /// 既に値が入っている（セーブ復元済みなど）の場合は上書きしない。
        /// 
        /// 本番運用では、WorldState 側で GenerateUniqueInstanceId() を呼び出し、
        /// SetInstanceIdForWorld(newId) で反映することを推奨する。
        /// このメソッドは WorldState を直接扱わないテストコード向けフォールバック。
        /// </summary>
        public void EnsureInstanceId()
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                var tempWorld = new WorldState();
                instanceId = tempWorld.GenerateUniqueInstanceId();
            }

            // WorldStateRuntime が動いていれば自動的に登録しておく
            RegisterBodyToCurrentWorld(this);
        }


        /// <summary>
        /// WorldState 側など、外部で一意な InstanceId を決めたあとに
        /// それをこのボディに反映するためのヘルパー。
        /// newId が null / 空文字の場合は何もしない。
        /// （セーブ復元時はシリアライズで復元されるので、通常ここは呼ばない想定）
        /// </summary>
        public void SetInstanceIdForWorld(string newId)
        {
            if (!string.IsNullOrEmpty(newId))
            {
                instanceId = newId;
            }
        }

        // ─────────────────────────────
        // 2) 固定値直指定コンストラクタ（確定済みボディ投入用）
        // ─────────────────────────────
        public BodyInstance(
            string raceId,
            string bodyJobId,
            string weaponId,
            int maxHp,
            int at,
            int df,
            int agi,
            int mat,
            int mdf)
            : this(raceId, bodyJobId, weaponId, null, maxHp, at, df, agi, mat, mdf)
        {
        }

        // ─────────────────────────────
        // 定義解決（起動時/ロード後に呼ぶ）
        // ─────────────────────────────
        public void ResolveDefinitions(MasterDatabase db)
        {
            if (db == null) return;
            cachedRace = db.GetRaceById(raceId);
            cachedBodyJob = db.GetBodyJobById(bodyJobId);
            cachedWeapon = db.GetWeaponById(weaponId);
        }

        // ─────────────────────────────
        // 互換：SoulStat にボディを適用して最終値を返す
        // 今回は「加算」で統一（ボディ側は確定ステを持つため）
        // ─────────────────────────────
        public int ApplyToSoulStat(int soulStat, StatKind kind)
        {
            int add = kind switch
            {
                StatKind.AT => at,
                StatKind.DF => df,
                StatKind.AGI => agi,
                StatKind.MAT => mat,
                StatKind.MDF => mdf,
                _ => 0
            };

            return Mathf.Max(0, soulStat + add);
        }

        // ─────────────────────────────
        // 公開プロパティ
        // ─────────────────────────────
        public string InstanceId => instanceId;
        public string RaceId => raceId;
        public string BodyJobId => bodyJobId;
        public string WeaponId => weaponId;

        public List<string> WeaponCandidateIds => weaponCandidateIds;

        public RaceDefinition Race => cachedRace;
        public BodyJobDefinition BodyJob => cachedBodyJob;
        public WeaponDefinition Weapon => cachedWeapon;

        public int MaxHp => maxHp;
        public int AT => at;
        public int DF => df;
        public int AGI => agi;
        public int MAT => mat;
        public int MDF => mdf;

    }
}
