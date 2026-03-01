using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// SoulInstance の生成を担うFactory。
    /// 旧 SoulInstance.CreateRandomInitialSoul() の中身をここに移植。
    /// SoulInstance 自体はデータ保持に専念する。
    ///
    /// ■ 主要エントリポイント
    ///   SoulFactory.Create(rank, ...)          … ランク指定でランダム生成
    ///   SoulFactory.CreateFromDef(soulDef)     … SoulDefinitionSO から生成（BodyペアリングもここでやるNo.
    /// </summary>
    public static class SoulFactory
    {
        // =====================================================================
        // 公開 API
        // =====================================================================

        /// <summary>
        /// ランク + 傾向だけ指定して、他は全部ランダムで Soul を生成する。
        /// </summary>
        public static SoulInstance Create(int rank, SoulJobTendency soulTendency)
            => Create(rank: rank, soulTendency: soulTendency);

        /// <summary>
        /// フル引数版。null / None = ランダム決定。
        /// SoulInstance.CreateRandomInitialSoul() の後継。
        /// </summary>
        public static SoulInstance Create(
            int rank,
            string soulName = null,
            string iconId = null,
            SoulType? soulType = null,
            TalentRank? talentRank = null,
            SoulJobTendency? soulTendency = null,
            string jobId = null,
            string title = null,
            GrowthType? growthType = null,
            FaceSexCategory? sex = null,
            int? level = null,
            int[] lv1Stats = null,
            float[] growthTargets = null,
            int[] permanentBonuses = null,
            List<ReinEvent> historyEvents = null,
            List<string> learnedSkillIds = null,
            List<OneReinSoulData> reinSouls = null,
            string instanceId = null)
        {
            var db = MasterDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[SoulFactory] MasterDatabase.Instance が見つかりません。");
                return null;
            }

            int clampedRank = Mathf.Max(1, rank);

            // 1) 成長タイプ
            GrowthType finalGrowthType = growthType ?? RollGrowthType();

            // 2) レベル
            int finalLevel = (level.HasValue && level.Value > 0) ? level.Value : 1;

            // 3) ソウルジョブ決定（jobId → soulTendency → 全ジョブ）
            SoulJobDefinition jobDef = ResolveSoulJob(db, jobId, soulTendency);
            if (jobDef == null)
            {
                Debug.LogError("[SoulFactory] 有効な SoulJobDefinition が見つかりません。");
                return null;
            }

            SoulJobTendency finalTendency = soulTendency ?? jobDef.Tendency;

            // 4) 才能ランク
            TalentRank finalTalent = talentRank ?? SpaceJourneyStatMath.RollTalentRank(finalGrowthType);

            // 5) 性別
            FaceSexCategory nameSex = sex ?? (Random.value < 0.5f
                ? FaceSexCategory.Male : FaceSexCategory.Female);

            // 6) 名前
            string finalName = string.IsNullOrEmpty(soulName)
                ? GetRandomSoulName(db, nameSex) : soulName;

            // 7) 顔アイコン
            string finalIconId = string.IsNullOrEmpty(iconId)
                ? db.GetRandomFaceIconId(nameSex) : iconId;

            // 8) 転生データ（1件以上）
            List<OneReinSoulData> finalReins;
            if (reinSouls != null && reinSouls.Count > 0)
            {
                finalReins = reinSouls;
            }
            else
            {
                var rein = OneReinSoulData.CreateFromArgs(
                    rank: clampedRank,
                    growthType: finalGrowthType,
                    jobDef: jobDef,
                    talent: finalTalent,
                    title: title,
                    level: finalLevel,
                    lv1Stats: lv1Stats,
                    growthTargets: growthTargets,
                    permanentBonuses: permanentBonuses,
                    historyEvents: historyEvents,
                    learnedSkillIds: learnedSkillIds);
                finalReins = new List<OneReinSoulData> { rein };
            }

            // 9) InstanceId 発行
            string finalInstanceId = string.IsNullOrEmpty(instanceId)
                ? new WorldState().GenerateUniqueInstanceId()
                : instanceId;

            // 10) SoulInstance 組み立て
            var soul = new SoulInstance(finalInstanceId, finalName, finalIconId, finalReins, 0);
            soul.InitFromFactory(
                nameSex: nameSex,
                sprite: db.GetFaceIconById(finalIconId, nameSex),
                talent: finalTalent,
                tendency: finalTendency,
                type: soulType);

            soul.EnsureInstanceId();
            return soul;
        }

        /// <summary>
        /// SoulDefinitionSO から Soul を生成する。
        /// SoulDef に BodyDefinition が設定されていれば EquipBody まで行う。
        /// </summary>
        public static SoulInstance CreateFromDef(SoulDefinitionSO soulDef)
        {
            if (soulDef == null)
            {
                Debug.LogError("[SoulFactory] CreateFromDef: soulDef が null です。");
                return null;
            }

            // SoulDefinitionSO 側で引数を組み立てて Create() を呼ぶ
            // （SoulDefinitionSO.CreateSoulInstance() はこちらに委譲）
            var soul = soulDef.CreateSoulInstance();
            if (soul == null) return null;

            // Body ペアリング
            var bodyDef = soulDef.BodyDefinition;
            if (bodyDef != null)
            {
                var body = bodyDef.CreateBodyInstance();
                if (body != null)
                    soul.EquipBody(body);
                else
                    Debug.LogWarning($"[SoulFactory] '{bodyDef.name}' からの Body 生成に失敗。Soul のみ返します。");
            }

            return soul;
        }

        // =====================================================================
        // 内部ヘルパー（旧 SoulInstance 内の private static メソッドを移植）
        // =====================================================================

        private static GrowthType RollGrowthType()
        {
            // Normal 40% / Early 32% / Late 25% / UltraLate 3%
            int r = Random.Range(0, 100);
            if (r < 40) return GrowthType.Normal;
            if (r < 72) return GrowthType.Early;
            if (r < 97) return GrowthType.Late;
            return GrowthType.UltraLate;
        }

        internal static SoulJobDefinition ResolveSoulJob(
            MasterDatabase db,
            string jobId,
            SoulJobTendency? soulTendency)
        {
            if (db == null) return null;

            // jobId 優先
            if (!string.IsNullOrEmpty(jobId))
            {
                var found = FindSoulJobById(db, jobId);
                if (found != null) return found;
            }

            // 傾向から抽選
            if (soulTendency.HasValue)
            {
                var found = db.GetRandomSoulJobByTendency(soulTendency.Value);
                if (found != null) return found;
            }

            // 全ジョブから抽選
            return GetRandomSoulJobFromAll(db);
        }

        private static SoulJobDefinition FindSoulJobById(MasterDatabase db, string jobId)
        {
            if (db == null || string.IsNullOrEmpty(jobId)) return null;
            var jobs = db.SoulJobDefinitions;
            if (jobs == null) return null;
            foreach (var def in jobs)
                if (def != null && def.JobId == jobId) return def;
            return null;
        }

        private static SoulJobDefinition GetRandomSoulJobFromAll(MasterDatabase db)
        {
            if (db == null) return null;
            var jobs = db.SoulJobDefinitions;
            if (jobs == null || jobs.Length == 0) return null;

            int totalWeight = 0;
            foreach (var def in jobs)
                if (def != null) totalWeight += Mathf.Max(0, def.JobEasePercent);

            if (totalWeight <= 0)
                return jobs[Random.Range(0, jobs.Length)];

            int r = Random.Range(0, totalWeight);
            foreach (var def in jobs)
            {
                if (def == null) continue;
                int w = Mathf.Max(0, def.JobEasePercent);
                if (r < w) return def;
                r -= w;
            }

            foreach (var def in jobs)
                if (def != null) return def;
            return null;
        }

        private static string GetRandomSoulName(MasterDatabase db, FaceSexCategory sex)
        {
            if (db == null) return "名無し";

            string result = null;
            switch (sex)
            {
                case FaceSexCategory.Male:
                    {
                        int idx = Random.Range(0, Mathf.Max(1, db.RandomNameMaleCount));
                        result = db.GetRandomName(FaceSexCategory.Male, idx);
                        break;
                    }
                case FaceSexCategory.Female:
                    {
                        int idx = Random.Range(0, Mathf.Max(1, db.RandomNameFemaleCount));
                        result = db.GetRandomName(FaceSexCategory.Female, idx);
                        break;
                    }
                default:
                    {
                        int idx = Random.Range(0, Mathf.Max(1, db.RandomNameUnknownCount));
                        result = db.GetRandomName(FaceSexCategory.Unknown, idx);
                        break;
                    }
            }

            if (string.IsNullOrEmpty(result))
                result = "名無し";

            return result;
        }
    }
}