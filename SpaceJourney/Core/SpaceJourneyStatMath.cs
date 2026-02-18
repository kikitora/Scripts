// SpaceJourneyStatMath.cs
// このクラスで何をするか：
// SteraCube の SpaceJourney モードにおける「ソウル成長」「ボディ生成」「スキルHP増減（ダメージ/回復）」
// 「AGIによる行動コスト短縮」など、数式まわりの処理をまとめた静的ヘルパークラスです。

using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public static class SpaceJourneyStatMath
    {
        //============================================================
        // 共通：ランク補正ユーティリティ
        //============================================================

        /// <summary>
        /// ランク値を 1 以上に丸めます。
        /// </summary>
        public static int ClampRankMin(int rank) => Mathf.Max(1, rank);

        /// <summary>
        /// テーブル + 外挿（必要なら）でランク平均を返す。
        /// rank=1 は table[0]、rank=2 は table[1]...
        /// rank がテーブル範囲外なら最後の差分で線形外挿。
        /// </summary>
        private static float GetFromTableWithExtrapolate(float[] table, int rank)
        {
            rank = ClampRankMin(rank);
            int idx = rank - 1;

            if (table == null || table.Length == 0) return 0f;
            if (idx < table.Length) return table[idx];

            // 外挿（最後の差分で伸ばす）
            if (table.Length == 1) return table[0];

            float last = table[table.Length - 1];
            float prev = table[table.Length - 2];
            float delta = last - prev;

            int over = idx - (table.Length - 1);
            return last + delta * over;
        }

        //============================================================
        // 7-2-1. ランク別の基礎ステータス rankBaseStat（ソウル側）
        //============================================================

        private static readonly float[] SoulRankBaseStatTable =
        {
            45f, 55f, 65f, 75f, 85f, 95f, 105f, 115f, 125f, 135f
        };

        public static float GetRankBaseStat(int rank)
        {
            return GetFromTableWithExtrapolate(SoulRankBaseStatTable, rank);
        }

        public static float CalcBaseStat(int rank, float jobMultiplier)
        {
            var rankBase = GetRankBaseStat(rank);
            return rankBase * jobMultiplier;
        }

        //============================================================
        // 7-2-4. 才能ランク・転生イベント補正 → potentialStat
        //============================================================

        /// <summary>
        /// 才能ランク → 倍率（レンジは SpaceJourneyConstants に集約）
        /// ※この倍率は「1転生分につき1回だけ」抽選し、全ステ共通で使う想定。
        /// </summary>
        public static float GetTalentFactor(TalentRank talent)
        {
            switch (talent)
            {
                case TalentRank.A: return Random.Range(SpaceJourneyConstants.TalentFactorMin_A, SpaceJourneyConstants.TalentFactorMax_A);
                case TalentRank.B: return Random.Range(SpaceJourneyConstants.TalentFactorMin_B, SpaceJourneyConstants.TalentFactorMax_B);
                case TalentRank.C: return Random.Range(SpaceJourneyConstants.TalentFactorMin_C, SpaceJourneyConstants.TalentFactorMax_C);
                case TalentRank.D: return Random.Range(SpaceJourneyConstants.TalentFactorMin_D, SpaceJourneyConstants.TalentFactorMax_D);
                case TalentRank.E: return Random.Range(SpaceJourneyConstants.TalentFactorMin_E, SpaceJourneyConstants.TalentFactorMax_E);
                default: return 1.0f;
            }
        }


        public static float CalcPotentialStat(float baseStat, float talentFactor, float eventFactor)
        {
            return baseStat * talentFactor * eventFactor;
        }

        //============================================================
        // 7-3. Lv1 ステータス
        //============================================================

        public static int CalcLv1Stat(float potentialStat)
        {
            return Mathf.FloorToInt(potentialStat * 0.1f);
        }

        //============================================================
        // 7-4 / 7-5. 成長タイプ＆成長カーブ
        //============================================================

        public static void GetGrowthTargetRange(GrowthType type, out float gMin, out float gMax)
        {
            switch (type)
            {
                case GrowthType.Early: gMin = 5.0f; gMax = 6.5f; return;
                case GrowthType.Normal: gMin = 5.5f; gMax = 7.0f; return;
                case GrowthType.Late: gMin = 6.0f; gMax = 8.0f; return;
                case GrowthType.UltraLate: gMin = 6.5f; gMax = 9.0f; return;
                default: gMin = 6.0f; gMax = 7.0f; return;
            }
        }

        public static float GetGrowthCurveExponent(GrowthType type)
        {
            switch (type)
            {
                case GrowthType.Early: return 0.7f;
                case GrowthType.Normal: return 1.0f;
                case GrowthType.Late: return 1.3f;
                case GrowthType.UltraLate: return 1.6f;
                default: return 1.0f;
            }
        }

        public static float GetRandomGrowthTarget(GrowthType type)
        {
            GetGrowthTargetRange(type, out var gMin, out var gMax);
            return Random.Range(gMin, gMax);
        }

        public static float CalcGrowthFactor(int level, GrowthType type, float gTarget, int maxLevel = 25)
        {
            if (level <= 1) return 1f;
            if (level >= maxLevel) return gTarget;

            float s = (level - 1) / (float)(maxLevel - 1);
            float p = GetGrowthCurveExponent(type);
            float sp = Mathf.Pow(s, p);

            return 1f + (gTarget - 1f) * sp;
        }

        /// <summary>
        /// ★SoulInstance が参照するAPI
        /// Lv1ステ × 成長係数 → ソウルの素ステを返す
        /// </summary>
        public static int CalcSoulStat(int lv1Stat, int level, GrowthType type, float gTarget, int maxLevel = 25)
        {
            float factor = CalcGrowthFactor(level, type, gTarget, maxLevel);
            float soulStatFloat = lv1Stat * factor;
            return Mathf.RoundToInt(soulStatFloat);
        }

        /// <summary>
        /// 才能ランク抽選（確率は SpaceJourneyConstants に集約）
        /// 仕様：成長タイプによらず一律分布
        /// </summary>
        public static TalentRank RollTalentRank(GrowthType type)
        {
            // 成長タイプによる分布差は廃止し、一律で抽選する（互換のため type 引数は残す）
            int a = SpaceJourneyConstants.TalentChance_Normal_A;
            int b = SpaceJourneyConstants.TalentChance_Normal_B;
            int c = SpaceJourneyConstants.TalentChance_Normal_C;
            int d = SpaceJourneyConstants.TalentChance_Normal_D;
            int e = SpaceJourneyConstants.TalentChance_Normal_E;

            // 念のため合計100でなくても動くように（ただし期待は100）
            int total = Mathf.Max(1, a + b + c + d + e);
            int r = Random.Range(0, total);

            if (r < a) return TalentRank.A; r -= a;
            if (r < b) return TalentRank.B; r -= b;
            if (r < c) return TalentRank.C; r -= c;
            if (r < d) return TalentRank.D;
            return TalentRank.E;
        }

        //============================================================
        // 7-6. ボディ合成 → 最終ステータス（ソウル×ボディ倍率）
        //============================================================

        public static int CalcFinalStatWithBody(int soulStat, float bodyPercent)
        {
            float finalFloat = soulStat * bodyPercent;
            return Mathf.RoundToInt(finalFloat);
        }

        //============================================================
        // ボディ生成：ランク平均テーブル（HP/他）＋ 右寄り揺れ
        //============================================================

        private static readonly float[] BodyHpRankAverageTable =
        {
            100f, 120f, 145f, 175f, 210f, 250f, 295f, 345f, 410f, 500f
        };

        private static readonly float[] BodyOtherRankAverageTable =
        {
            10f, 16f, 24f, 33f, 44f, 56f, 69f, 82f, 92f, 100f
        };

        public static float GetBodyHpRankAverage(int rank)
        {
            return GetFromTableWithExtrapolate(BodyHpRankAverageTable, rank);
        }

        public static float GetBodyOtherRankAverage(int rank)
        {
            return GetFromTableWithExtrapolate(BodyOtherRankAverageTable, rank);
        }

        private static void CalcRightShiftRange(
            float prevAvg,
            float curAvg,
            float nextAvg,
            bool isFirst,
            bool isLast,
            out float low,
            out float high,
            float lowerT = 0.25f,
            float upperT = 0.75f,
            float rankLastOverflow = 0.20f)
        {
            if (isFirst)
            {
                low = curAvg;
                high = curAvg + (nextAvg - curAvg) * upperT;
                return;
            }

            if (isLast)
            {
                low = curAvg + (prevAvg - curAvg) * lowerT;
                high = curAvg * (1f + rankLastOverflow);
                return;
            }

            low = curAvg + (prevAvg - curAvg) * lowerT;
            high = curAvg + (nextAvg - curAvg) * upperT;

            if (high < low) (low, high) = (high, low);
        }

        public static int GenerateBodyHpBase(int rank, float lowerT = 0.25f, float upperT = 0.75f, float lastRankOverflow = 0.20f)
        {
            rank = ClampRankMin(rank);

            float cur = GetBodyHpRankAverage(rank);

            int lastTableRank = BodyHpRankAverageTable.Length;
            bool isFirst = rank == 1;
            bool isLast = rank == lastTableRank;

            float prev = isFirst ? cur : GetBodyHpRankAverage(rank - 1);
            float next = isLast ? cur : GetBodyHpRankAverage(rank + 1);

            CalcRightShiftRange(prev, cur, next, isFirst, isLast, out var low, out var high, lowerT, upperT, lastRankOverflow);

            float sampled = Random.Range(low, high);
            return Mathf.Max(1, Mathf.RoundToInt(sampled));
        }

        public static int GenerateBodyOtherBase(int rank, float lowerT = 0.25f, float upperT = 0.75f, float lastRankOverflow = 0.20f)
        {
            rank = ClampRankMin(rank);

            float cur = GetBodyOtherRankAverage(rank);

            int lastTableRank = BodyOtherRankAverageTable.Length;
            bool isFirst = rank == 1;
            bool isLast = rank == lastTableRank;

            float prev = isFirst ? cur : GetBodyOtherRankAverage(rank - 1);
            float next = isLast ? cur : GetBodyOtherRankAverage(rank + 1);

            CalcRightShiftRange(prev, cur, next, isFirst, isLast, out var low, out var high, lowerT, upperT, lastRankOverflow);

            float sampled = Random.Range(low, high);
            return Mathf.Max(1, Mathf.RoundToInt(sampled));
        }

        public static int ApplyJobMultiplierPercent(int baseValue, int mulPercent)
        {
            float v = baseValue * (mulPercent / 100f);
            return Mathf.Max(1, Mathf.RoundToInt(v));
        }

        //============================================================
        // ダメージ計算（新：AT-DF 差分で通り率を決める）
        //============================================================

        private static float CalcThroughByDelta(float attackValue, float defenseValue)
        {
            float delta = attackValue - defenseValue;

            // ★定数は SpaceJourneyConstants へ移動（APIは減らさない）
            float t = (float)System.Math.Tanh(delta / SpaceJourneyConstants.DamageThroughDeltaScale); // -1..1
            float through = SpaceJourneyConstants.DamageThroughEqual + 0.5f * t;         // 中心=Equal, 振れ幅=±0.5

            return Mathf.Clamp(through, SpaceJourneyConstants.DamageThroughMin, SpaceJourneyConstants.DamageThroughMax);
        }

        public static int GetRandomFixedChipDamage(int minInclusive = 2, int maxInclusive = 5)
        {
            if (maxInclusive < minInclusive) (minInclusive, maxInclusive) = (maxInclusive, minInclusive);
            return Random.Range(minInclusive, maxInclusive + 1);
        }

        public static int CalcPhysicalDamage(float baseAttackDamage, float defenderDF, int fixedChipDamage = 3, float c = 100f)
        {
            if (baseAttackDamage <= 0f) return 0;

            float through = CalcThroughByDelta(baseAttackDamage, defenderDF);
            float dmg = fixedChipDamage + baseAttackDamage * through;

            // ±10% 揺れ
            float factor = Random.Range(SpaceJourneyConstants.DamageRandomMinFactor, SpaceJourneyConstants.DamageRandomMaxFactor);
            dmg *= factor;

            int result = Mathf.RoundToInt(dmg);
            return Mathf.Max(1, result);
        }

        public static int CalcMagicalDamage(float baseMagicDamage, float defenderMDF, int fixedChipDamage = 3, float c = 100f)
        {
            if (baseMagicDamage <= 0f) return 0;

            float through = CalcThroughByDelta(baseMagicDamage, defenderMDF);
            float dmg = fixedChipDamage + baseMagicDamage * through;

            // ±10% 揺れ
            float factor = Random.Range(SpaceJourneyConstants.DamageRandomMinFactor, SpaceJourneyConstants.DamageRandomMaxFactor);
            dmg *= factor;

            int result = Mathf.RoundToInt(dmg);
            return Mathf.Max(1, result);
        }

        //============================================================
        // ★スキル用：HP増減（ダメージ/回復）統一API
        //============================================================

        public static int CalcSkillHpDelta(
            SkillDamageKind kind,
            int amount,
            int defenseIgnorePercent,
            int attackerAt,
            int attackerMat,
            int targetDf,
            int targetMdf,
            int targetMaxHp)
        {
            // 回復は Fixed / MaxHpRate だけ
            bool healAllowed = (kind == SkillDamageKind.Fixed || kind == SkillDamageKind.MaxHpRate);
            if (!healAllowed && amount < 0)
            {
                amount = 0;
            }

            switch (kind)
            {
                case SkillDamageKind.None:
                    return 0;

                case SkillDamageKind.Fixed:
                    return amount;

                case SkillDamageKind.MaxHpRate:
                    {
                        float v = targetMaxHp * (amount / 100f);
                        return Mathf.RoundToInt(v);
                    }

                case SkillDamageKind.Physical:
                    return CalcSkillPhysical(attackerAt, targetDf, amount);

                case SkillDamageKind.Magical:
                    return CalcSkillMagical(attackerMat, targetMdf, amount);

                case SkillDamageKind.PenetratePhysical:
                    {
                        float df2 = ApplyIgnorePercent(targetDf, defenseIgnorePercent);
                        return CalcSkillPhysical(attackerAt, df2, amount);
                    }

                case SkillDamageKind.PenetrateMagical:
                    {
                        float mdf2 = ApplyIgnorePercent(targetMdf, defenseIgnorePercent);
                        return CalcSkillMagical(attackerMat, mdf2, amount);
                    }

                default:
                    return 0;
            }
        }

        private static float ApplyIgnorePercent(float defense, int ignorePercent)
        {
            int p = Mathf.Clamp(ignorePercent, 0, 100);
            float rate = 1f - (p / 100f);
            return Mathf.Max(0f, defense * rate);
        }

        private static int CalcSkillPhysical(int attackerAt, float defenderDf, int amountPercent)
        {
            if (amountPercent == 0) return 0;

            float scaled = attackerAt * (amountPercent / 100f);
            int raw = Mathf.RoundToInt(scaled);

            if (raw <= 0) return 0;

            int chip = GetRandomFixedChipDamage();
            return CalcPhysicalDamage(raw, defenderDf, chip);
        }

        private static int CalcSkillMagical(int attackerMat, float defenderMdf, int amountPercent)
        {
            if (amountPercent == 0) return 0;

            float scaled = attackerMat * (amountPercent / 100f);
            int raw = Mathf.RoundToInt(scaled);

            if (raw <= 0) return 0;

            int chip = GetRandomFixedChipDamage();
            return CalcMagicalDamage(raw, defenderMdf, chip);
        }


        //============================================================
        // 8. レベルアップ必要EXP（ソウル用）
        //============================================================

        /// <summary>
        /// ソウルジョブランクによる「必要EXP倍率」を返す。
        /// ランク1を1.0倍とし、1段階上がるごとに +30% ずつ増加させる。
        ///   Rank1: 1.0
        ///   Rank2: 1.3
        ///   ...
        ///   Rank10: 3.7
        /// </summary>
        public static float CalcSoulJobRankExpMultiplier(int soulJobRank)
        {
            int r = Mathf.Max(1, soulJobRank);
            return 1.0f + SpaceJourneyConstants.SoulJobRankExpPerRank * (r - 1);
        }

        /// <summary>
        /// ソウル共通の「レベルアップ必要EXP（基準値）」を返す。
        /// level は「現在レベル」を指定する（Lv1→2 に必要なEXPを知りたい場合は level=1）。
        /// レベル上限に達している場合は 0 を返す。
        /// </summary>
        public static int CalcBaseRequiredExpForLevel(int level)
        {
            if (level < 1)
            {
                level = 1;
            }

            // 上限レベルに達している場合はこれ以上レベルアップしない
            if (level >= SpaceJourneyConstants.MaxSoulLevel)
            {
                return 0;
            }

            // 1〜22 は指数的に増加する通常カーブ
            int exponentIndex = Mathf.Clamp(
                level - 1,
                0,
                SpaceJourneyConstants.LevelUpExpNormalMaxLevel - 1
            );

            float exp = SpaceJourneyConstants.LevelUpBaseExp *
                        Mathf.Pow(SpaceJourneyConstants.LevelUpExpFactor, exponentIndex);

            // 23〜25 は「同じ指数カーブ＋ボーナス倍率」
            int targetLevel = Mathf.Clamp(level + 1, 2, SpaceJourneyConstants.MaxSoulLevel);
            switch (targetLevel)
            {
                case 23:
                    exp *= SpaceJourneyConstants.LevelUpExpBonusLv23;
                    break;
                case 24:
                    exp *= SpaceJourneyConstants.LevelUpExpBonusLv24;
                    break;
                case 25:
                    exp *= SpaceJourneyConstants.LevelUpExpBonusLv25;
                    break;
            }

            return Mathf.CeilToInt(exp);
        }

        /// <summary>
        /// ソウルジョブの「上がりにくさ倍率」と「ソウルジョブランク」を考慮した
        /// 実際のレベルアップ必要EXPを返す。
        ///
        /// jobExpFactor:
        ///   つきやすい職     → 1.0f
        ///   就きにくい職     → 1.3f
        ///   極端に就きにくい → 1.8f
        ///
        /// soulJobRank:
        ///   現在のソウルジョブランク（1〜）
        /// </summary>
        public static int CalcRequiredExpForLevelWithSoulJob(
            int level,
            float jobExpFactor,
            int soulJobRank
        )
        {
            int baseExp = CalcBaseRequiredExpForLevel(level);
            if (baseExp <= 0)
            {
                return 0;
            }

            float rankMul = CalcSoulJobRankExpMultiplier(soulJobRank);
            float exp = baseExp * jobExpFactor * rankMul;

            return Mathf.CeilToInt(exp);
        }


        //============================================================
        // 6-5. AGI と行動コスト短縮
        //============================================================

        public static int CalcEffectiveActionCost(int baseCost, float agiFinal)
        {
            if (baseCost <= 0) return 0;

            float agiUsed = agiFinal * Random.Range(0.95f, 1.05f);
            float agilityRatio = Mathf.Clamp01(agiUsed / SpaceJourneyConstants.AgilityCap);
            float reductionRate = SpaceJourneyConstants.MaxCostReductionRate * agilityRatio;

            float rawCost = baseCost * (1f - reductionRate);

            float costGain = baseCost - rawCost;
            if (costGain < SpaceJourneyConstants.MinCostReductionStep)
            {
                return baseCost;
            }

            int rounded = Mathf.RoundToInt(rawCost);
            return Mathf.Max(1, rounded);
        }
    }
}
