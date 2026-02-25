using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// BodyInstance を生成する Factory。
    /// ランク平均（HP用/その他用）→ 揺れ → 職倍率/種族倍率 を掛けて確定値を作り、
    /// その確定値を BodyInstance に保存して返します（ロード時に再計算しない前提）。
    /// </summary>
    public static class BodyFactory
    {
        /// <summary>
        /// BodyInstance をランダム生成する。
        /// raceId / bodyJobId / weaponId は null で抽選。
        /// weaponId を直接指定した場合はそれを使う（minRank チェックはスキップ）。
        /// </summary>
        public static BodyInstance CreateRandom(
            int rank,
            string raceId = null,
            string bodyJobId = null,
            string weaponId = null,
            System.Random rng = null)
        {
            var db = MasterDatabase.Instance;
            if (db == null)
                throw new InvalidOperationException("MasterDatabase.Instance is null");

            rank = SpaceJourneyStatMath.ClampRankMin(rank);
            rng ??= new System.Random();

            // raceId 未指定 → ランダム選択（TODO: 本格抽選ロジックは後で）
            if (string.IsNullOrEmpty(raceId))
            {
                var races = db.RaceDefinitions;
                if (races != null && races.Length > 0)
                    raceId = races[rng.Next(races.Length)].raceId;
            }

            // bodyJobId 未指定 → ランダム選択
            if (string.IsNullOrEmpty(bodyJobId))
            {
                var jobs = db.BodyJobDefinitions;
                if (jobs != null && jobs.Length > 0)
                    bodyJobId = jobs[rng.Next(jobs.Length)].bodyJobId;
            }

            var race = db.GetRaceById(raceId);
            var job = db.GetBodyJobById(bodyJobId);

            if (race == null) throw new Exception($"Race not found: {raceId}");
            if (job == null) throw new Exception($"BodyJob not found: {bodyJobId}");

            // 武器候補は BodyJobDefinition.weaponCandidates（SO直接参照）から取得
            var weaponCandidates = job.weaponCandidates;

            // ランク基礎値（揺れ込み）
            int hpBase = SpaceJourneyStatMath.GenerateBodyHpBase(rank);

            // ステータスごとに個別抽選（全ステ同値にならないよう）
            int atBase = SpaceJourneyStatMath.GenerateBodyOtherBase(rank);
            int dfBase = SpaceJourneyStatMath.GenerateBodyOtherBase(rank);
            int agiBase = SpaceJourneyStatMath.GenerateBodyOtherBase(rank);
            int matBase = SpaceJourneyStatMath.GenerateBodyOtherBase(rank);
            int mdfBase = SpaceJourneyStatMath.GenerateBodyOtherBase(rank);

            int maxHp = ApplyFloatMultipliers(hpBase, job.hpMul, race.hpMul);
            int at = ApplyFloatMultipliers(atBase, job.atMul, race.atMul);
            int df = ApplyFloatMultipliers(dfBase, job.dfMul, race.dfMul);
            int agi = ApplyFloatMultipliers(agiBase, job.agiMul, race.agiMul);
            int mat = ApplyFloatMultipliers(matBase, job.matMul, race.matMul);
            int mdf = ApplyFloatMultipliers(mdfBase, job.mdfMul, race.mdfMul);

            // weaponId 未指定なら候補から抽選
            if (string.IsNullOrEmpty(weaponId))
                weaponId = ChooseWeaponId(weaponCandidates, rank, rng);

            var body = new BodyInstance(
                raceId,
                bodyJobId,
                weaponId,
                maxHp,
                at,
                df,
                agi,
                mat,
                mdf
            );
            body.EnsureInstanceId();
            return body;
        }

        public static BodyInstance CreateFixed(
            string raceId,
            string bodyJobId,
            string weaponId,
            int maxHp,
            int at,
            int df,
            int agi,
            int mat,
            int mdf)
        {
            return new BodyInstance(
                raceId,
                bodyJobId,
                weaponId,
                Mathf.Max(1, maxHp),
                Mathf.Max(0, at),
                Mathf.Max(0, df),
                Mathf.Max(0, agi),
                Mathf.Max(0, mat),
                Mathf.Max(0, mdf)
            );
        }

        private static int ApplyFloatMultipliers(int baseValue, float jobMul, float raceMul)
        {
            float v = baseValue;
            v *= jobMul;
            v *= raceMul;
            return Mathf.Max(1, Mathf.RoundToInt(v));
        }

        /// <summary>
        /// weaponCandidates（SO直接参照リスト）から rank を満たす武器をランダム選択し、
        /// weaponId（string）を返す。
        /// </summary>
        private static string ChooseWeaponId(
            List<WeaponDefinition> weaponCandidates,
            int rank,
            System.Random rng)
        {
            if (weaponCandidates == null || weaponCandidates.Count == 0)
                return string.Empty;

            var valid = new List<WeaponDefinition>();
            foreach (var def in weaponCandidates)
            {
                if (def == null) continue;
                // minRank チェック：ボディのランクが武器の最低ランク未満なら除外
                if (rank >= def.minRank)
                    valid.Add(def);
            }

            if (valid.Count == 0)
                return string.Empty;

            return valid[rng.Next(valid.Count)].weaponId;
        }
    }
}