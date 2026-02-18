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
        public static BodyInstance CreateRandom(
            string raceId,
            string bodyJobId,
            int rank,
            List<string> weaponCandidateIds = null,
            System.Random rng = null)
        {
            var db = MasterDatabase.Instance;
            if (db == null)
                throw new InvalidOperationException("MasterDatabase.Instance is null");

            rank = SpaceJourneyStatMath.ClampRankMin(rank);
            rng ??= new System.Random();

            var race = db.GetRaceById(raceId);
            var job = db.GetBodyJobById(bodyJobId);

            if (race == null) throw new Exception($"Race not found: {raceId}");
            if (job == null) throw new Exception($"BodyJob not found: {bodyJobId}");

            // ランク基礎値（揺れ込み）
            int hpBase = SpaceJourneyStatMath.GenerateBodyHpBase(rank);

            // ここが修正点：
            // 「その他ステ」は1回だけ抽選すると全ステ同値になるので、ステータスごとに抽選する
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

            string weaponId = ChooseWeaponId(db, weaponCandidateIds, rng);

            var body = new BodyInstance(
    raceId,
    bodyJobId,
    weaponId,
    weaponCandidateIds,
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
            int mdf,
            List<string> weaponCandidateIds = null)
        {
            return new BodyInstance(
                raceId,
                bodyJobId,
                weaponId,
                weaponCandidateIds,
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

        private static string ChooseWeaponId(
            MasterDatabase db,
            List<string> weaponCandidateIds,
            System.Random rng)
        {
            if (weaponCandidateIds == null || weaponCandidateIds.Count == 0)
                return string.Empty;

            var valid = new List<string>();
            foreach (var id in weaponCandidateIds)
            {
                if (!string.IsNullOrWhiteSpace(id) && db.GetWeaponById(id) != null)
                    valid.Add(id);
            }

            if (valid.Count == 0)
                return string.Empty;

            return valid[rng.Next(valid.Count)];
        }
    }
}
