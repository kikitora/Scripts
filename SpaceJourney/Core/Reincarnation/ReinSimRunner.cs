using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 転生シミュレーションの実行エントリーポイント。
    ///
    /// 役割：
    ///   1. job_*タグ → SoulJobDefinitionのマッピングを構築
    ///   2. 主魂・守護霊のSoulInstanceからReinSimInputを組み立てる（jobDef=null）
    ///   3. ReinSimulator.Run() でシミュを走らせる
    ///   4. 人生イベントから自然に確定したジョブを結果から取得
    ///   5. 結果をOneReinSoulData.CreateFromArgs() でデータ化
    ///   6. 主魂のreinSoulsリストに追加（または上書き）する
    ///
    /// 使い方：
    ///   var reinData = ReinSimRunner.Run(mainSoul, guardians, allEvents);
    /// </summary>
    public static class ReinSimRunner
    {
        // ============================================================
        // job_*タグ → jobId のマッピング（arc_*_finalが付与するタグに対応）
        // ============================================================
        private static readonly Dictionary<string, string> JobTagToId = new()
        {
            // ── WARRIOR ──
            { "job_keisatsu",  "warrior_keisatsu" },
            { "job_salaryman", "warrior_salaryman" },
            { "job_yakuza",    "warrior_yakuza"   },
            { "job_kenjutsu",  "warrior_kenjutsu" },
            { "job_yohei",     "warrior_yohei"    },
            { "job_uchu",      "warrior_uchu"     },
            // ── KNIGHT ──
            { "job_jieitai",   "knight_jieitai"   },
            { "job_shobo",     "knight_shobo"     },
            { "job_kyumei",    "knight_kyumei"    },
            { "job_bodyguard", "knight_bodyguard" },
            { "job_saiban",    "knight_saiban"    },
            { "job_bengoshi",  "knight_bengoshi"  },
            // ── ARCHER ──
            { "job_karyudo",   "archer_karyudo"   },
            { "job_sagishi",   "archer_sagishi"   },
            { "job_cameraman", "archer_cameraman" },
            { "job_kyudo",     "archer_kyudo"     },
            { "job_golfer",    "archer_golfer"    },
            { "job_msf",       "archer_msf"       },
            // ── MAGE ──
            { "job_tejinashi", "mage_tejinashi"   },
            { "job_uranai",    "mage_uranai"      },
            { "job_sou",       "mage_sou"         },
            { "job_ongakuka",  "mage_ongakuka"    },
            { "job_kenkyusha", "mage_kenkyusha"   },
            { "job_cult",      "mage_kult"        },
            // ── LANCER ──
            { "job_ryoshi",    "lancer_ryoshi"    },
            { "job_tatekiya",  "lancer_tatekiya"  },
            { "job_haikan",    "lancer_haikan"    },
            { "job_kussaku",   "lancer_kussaku"   },
            { "job_kigyoka",   "lancer_kigyoka"   },
            { "job_rocket",    "lancer_rocket"    },
        };

        /// <summary>
        /// 転生シミュを実行し、結果を主魂のOneReinSoulDataとして追加する。
        /// ジョブは人生イベントの流れから自然に確定する。
        /// </summary>
        public static OneReinSoulData Run(
            SoulInstance mainSoul,
            SoulInstance[] guardians,
            IReadOnlyList<ReinLifeEvent> allEvents,
            int replaceIndex = -1)
        {
            // --------------------------------------------------
            // 1) バリデーション
            // --------------------------------------------------
            if (mainSoul == null)
            {
                Debug.LogError("[ReinSimRunner] mainSoul が null です。");
                return null;
            }

            var db = MasterDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[ReinSimRunner] MasterDatabase が見つかりません。");
                return null;
            }

            if (allEvents == null || allEvents.Count == 0)
                Debug.LogWarning("[ReinSimRunner] allEvents が空です。イベントなしでシミュを実行します。");

            // --------------------------------------------------
            // 2) job_*タグ → SoulJobDefinitionのマッピング構築
            // --------------------------------------------------
            var jobTagMap = BuildJobTagMap(db);

            // --------------------------------------------------
            // 3) ジョブ先行抽選（傾向に合ったジョブをjobTierで重み付き選択）
            // --------------------------------------------------
            var selectedJob = SelectJobByTendency(db, mainSoul.SoulTendency);
            if (selectedJob == null)
            {
                Debug.LogWarning($"[ReinSimRunner] 傾向 {mainSoul.SoulTendency} に対応するジョブが見つかりません。フォールバックを使用します。");
                selectedJob = GetFallbackJob(db);
            }
            else
            {
                Debug.Log($"[ReinSimRunner] ジョブ先行抽選: {selectedJob.JobName}（{mainSoul.SoulTendency}系）");
            }

            // --------------------------------------------------
            // 4) ReinSimInput の組み立て（先行抽選したジョブを渡す）
            // --------------------------------------------------
            var input = ReinSimInput.Build(
                main: mainSoul,
                guardians: guardians,
                jobDef: selectedJob,
                talent: mainSoul.Talent,
                jobTagToJobDef: jobTagMap);

            // --------------------------------------------------
            // 5) シミュ実行
            // --------------------------------------------------
            var result = ReinSimulator.Run(input, allEvents ?? new List<ReinLifeEvent>());

            // --------------------------------------------------
            // 6) 確定したジョブを取得（先行抽選済みなので通常は一致）
            // --------------------------------------------------
            var destJob = result.DestinyJob;
            if (destJob == null)
            {
                destJob = GetFallbackJob(db);
                Debug.LogWarning($"[ReinSimRunner] ジョブが人生から確定しませんでした。フォールバック: {destJob?.JobName}");
            }
            else
            {
                Debug.Log($"[ReinSimRunner] ジョブ確定: {destJob.JobName}");
            }

            Debug.Log($"[ReinSimRunner] シミュ完了: FinalRank={result.FinalRank}, " +
                      $"Skills={result.LearnedSkillIds.Count}件, " +
                      $"Events={result.HistoryEvents.Count}件");

            // --------------------------------------------------
            // 7) OneReinSoulData の生成
            // --------------------------------------------------
            var reinData = OneReinSoulData.CreateFromArgs(
                rank: result.FinalRank,
                growthType: mainSoul.GrowthType,
                jobDef: destJob,
                talent: mainSoul.Talent,
                title: null,
                level: 1,
                lv1Stats: null,
                growthTargets: null,
                permanentBonuses: null,
                historyEvents: result.HistoryEvents,
                learnedSkillIds: result.LearnedSkillIds,
                eventFactors: result.EventFactors
            );

            // --------------------------------------------------
            // 8) デバッグ出力
            // --------------------------------------------------
            DebugPrint(mainSoul, destJob, result, reinData);

            // --------------------------------------------------
            // 9) 主魂の reinSouls に書き込む
            // --------------------------------------------------
            ApplyToSoul(mainSoul, reinData, replaceIndex);

            return reinData;
        }

        // ============================================================
        // C経路：固定ランク量産用エントリ
        // ============================================================
        // 来歴あり・ランク固定でソウルを量産する。
        // 主ソウル/守護霊は使わず、ランクと傾向と才能だけからシードソウルを合成する。
        // 用途：初期ソウル / 敵ソウル / NPC / ソウルスカウト雇用ソウルなど
        // ============================================================
        public static OneReinSoulData RunFixedRank(
            int fixedRank,
            SoulJobTendency tendency,
            TalentRank talentRank,
            IReadOnlyList<ReinLifeEvent> allEvents)
        {
            // --------------------------------------------------
            // 1) バリデーション
            // --------------------------------------------------
            if (fixedRank < 1)
            {
                Debug.LogError($"[ReinSimRunner] RunFixedRank: fixedRank({fixedRank}) は1以上が必要です。");
                return null;
            }
            if (talentRank == TalentRank.None)
            {
                Debug.LogWarning("[ReinSimRunner] RunFixedRank: talentRank=None なので C 扱いにフォールバックします。");
                talentRank = TalentRank.C;
            }

            var db = MasterDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[ReinSimRunner] MasterDatabase が見つかりません。");
                return null;
            }

            if (allEvents == null || allEvents.Count == 0)
                Debug.LogWarning("[ReinSimRunner] allEvents が空です。イベントなしでシミュを実行します。");

            // --------------------------------------------------
            // 2) job_*タグマップ構築
            // --------------------------------------------------
            var jobTagMap = BuildJobTagMap(db);

            // --------------------------------------------------
            // 3) ジョブ先行抽選（傾向に合ったジョブを jobTier で重み付き選択）
            // --------------------------------------------------
            var selectedJob = SelectJobByTendency(db, tendency);
            if (selectedJob == null)
            {
                Debug.LogWarning($"[ReinSimRunner] 傾向 {tendency} に対応するジョブが見つかりません。フォールバックを使用します。");
                selectedJob = GetFallbackJob(db);
            }
            else
            {
                Debug.Log($"[ReinSimRunner] 固定ランク量産: {selectedJob.JobName}（{tendency}系・rank{fixedRank}）");
            }

            if (selectedJob == null)
            {
                Debug.LogError("[ReinSimRunner] ジョブが取得できません。中断。");
                return null;
            }

            // --------------------------------------------------
            // 4) 成長タイプ抽選（C経路でも個性が出るように）
            // --------------------------------------------------
            GrowthType seedGrowth = RollGrowthTypeForFixed();

            // --------------------------------------------------
            // 5) シードソウルの転生内ステ生成（揺らぎ ±10%）
            // --------------------------------------------------
            int[] seedStats = BuildSeedStats(fixedRank, selectedJob, talentRank);

            // --------------------------------------------------
            // 6) ReinSimInput 組み立て（守護霊なし＝seedStats×100%）
            // --------------------------------------------------
            var input = ReinSimInput.BuildFromSeed(
                seedStats: seedStats,
                rank: fixedRank,
                growthType: seedGrowth,
                jobDef: selectedJob,
                talent: talentRank,
                jobTagToJobDef: jobTagMap);

            // --------------------------------------------------
            // 7) シミュ実行（fixedRank を渡してC経路）
            // --------------------------------------------------
            var result = ReinSimulator.Run(input, allEvents ?? new List<ReinLifeEvent>(), fixedRank);

            // --------------------------------------------------
            // 8) DestinyJob のフォールバック
            // --------------------------------------------------
            var destJob = result.DestinyJob ?? selectedJob;

            Debug.Log($"[ReinSimRunner] 固定ランク量産完了: rank={result.FinalRank}, " +
                      $"Skills={result.LearnedSkillIds.Count}件, " +
                      $"Events={result.HistoryEvents.Count}件");

            // --------------------------------------------------
            // 9) OneReinSoulData の生成（rankはfixedRankで強制）
            // --------------------------------------------------
            var reinData = OneReinSoulData.CreateFromArgs(
                rank: fixedRank,
                growthType: seedGrowth,
                jobDef: destJob,
                talent: talentRank,
                title: null,
                level: 1,
                lv1Stats: null,
                growthTargets: null,
                permanentBonuses: null,
                historyEvents: result.HistoryEvents,
                learnedSkillIds: result.LearnedSkillIds,
                eventFactors: result.EventFactors
            );

            return reinData;
        }

        // ============================================================
        // 内部：シードソウルのステータス生成
        // ============================================================
        // 「ランク到達閾値」（rank → rank+1 の判定式と同じ計算）をベースに、
        // ±10% の揺らぎを乗せて個体差を出す。
        // 結果は MaxStats × 100% としてシミュ用 MainStats に使われる。
        // ============================================================
        private static int[] BuildSeedStats(int rank, SoulJobDefinition jobDef, TalentRank talentRank)
        {
            // ReinSim.cs と同期している必要がある定数群（重複定義を避けるため
            // ローカルに定義。値が変わったら両方更新する）
            const int StatMaxLevel = 25;
            const float GrowthNormal = 6.25f;
            const float GrowthPower = 1.0f;
            int[] NormalLevels = { 4, 6, 7, 9, 11, 13, 15, 17, 19, 21 };
            float[] RankBaseStats = { 45f, 55f, 65f, 75f, 85f, 95f, 105f, 115f, 125f, 135f };

            int rankIdx = Mathf.Clamp(rank - 1, 0, NormalLevels.Length - 1);
            int targetLv = NormalLevels[rankIdx];

            // 才能倍率は1度だけ抽選（5ステ共通）
            float talentFactor = SpaceJourneyStatMath.GetTalentFactor(talentRank);

            // 成長係数（rank到達時のLvでの伸び）
            float s = (targetLv - 1f) / (StatMaxLevel - 1f);
            float growthFactor = 1f + (GrowthNormal - 1f) * Mathf.Pow(s, GrowthPower);

            float[] muls = jobDef != null ? jobDef.GetStatMultipliers() : new[] { 1f, 1f, 1f, 1f, 1f };
            float baseRank = RankBaseStats[rankIdx];

            int[] result = new int[5];
            for (int i = 0; i < 5; i++)
            {
                float jobMul = muls[i];
                float lv1Stat = baseRank * jobMul * talentFactor * 0.1f;
                int threshold = Mathf.RoundToInt(lv1Stat * growthFactor);
                // ±10% 揺らぎ
                float jitter = UnityEngine.Random.Range(0.90f, 1.10f);
                int statValue = Mathf.RoundToInt(threshold * jitter);
                result[i] = Mathf.Max(1, statValue);
            }

            return result;
        }

        // ============================================================
        // 内部：C経路の成長タイプ抽選
        // ============================================================
        private static GrowthType RollGrowthTypeForFixed()
        {
            int r = UnityEngine.Random.Range(0, 100);
            if (r < 40) return GrowthType.Normal;
            if (r < 72) return GrowthType.Early;
            if (r < 97) return GrowthType.Late;
            return GrowthType.UltraLate;
        }

        // ============================================================
        // 内部：job_*タグ → SoulJobDefinitionのマップを構築
        // ============================================================
        private static IReadOnlyDictionary<string, SoulJobDefinition> BuildJobTagMap(MasterDatabase db)
        {
            var map = new Dictionary<string, SoulJobDefinition>();
            if (db.SoulJobDefinitions == null) return map;

            foreach (var jobDef in db.SoulJobDefinitions)
            {
                if (jobDef == null) continue;
                foreach (var kv in JobTagToId)
                {
                    if (kv.Value == jobDef.JobId)
                    {
                        map[kv.Key] = jobDef;
                        break;
                    }
                }
            }
            return map;
        }

        // ============================================================
        // 内部：傾向に合ったジョブを jobTier で重み付き抽選（先行抽選方式）
        // ============================================================
        private static SoulJobDefinition SelectJobByTendency(MasterDatabase db, SoulJobTendency tendency)
        {
            var jobs = db.SoulJobDefinitions;
            if (jobs == null || jobs.Length == 0) return null;

            // 傾向一致のジョブだけ抽出
            var candidates = new List<SoulJobDefinition>();
            var weights = new List<float>();

            foreach (var j in jobs)
            {
                if (j == null) continue;
                if (j.Tendency != tendency) continue;
                candidates.Add(j);
                // jobTier が公開されている想定。なければ均等重み
                float w = j.JobTier > 0 ? j.JobTier : 1f;
                weights.Add(w);
            }

            if (candidates.Count == 0) return null;

            float total = 0f;
            foreach (var w in weights) total += w;

            float rnd = UnityEngine.Random.value * total;
            for (int i = 0; i < candidates.Count; i++)
            {
                rnd -= weights[i];
                if (rnd <= 0f) return candidates[i];
            }
            return candidates[candidates.Count - 1];
        }

        // ============================================================
        // 内部：フォールバック（アークが完走しなかった場合）
        // ============================================================
        private static SoulJobDefinition GetFallbackJob(MasterDatabase db)
        {
            var jobs = db.SoulJobDefinitions;
            if (jobs == null || jobs.Length == 0) return null;
            return jobs[UnityEngine.Random.Range(0, jobs.Length)];
        }

        // ============================================================
        // 内部：デバッグ出力
        // ============================================================
        private static void DebugPrint(
            SoulInstance soul,
            SoulJobDefinition job,
            ReinSimResult simResult,
            OneReinSoulData reinData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== 転生シミュレーション結果 ===");
            sb.AppendLine($"ジョブ    : {job?.JobName ?? "未確定"}（{soul.SoulTendency}系）");
            sb.AppendLine($"才能      : {soul.Talent}");
            sb.AppendLine($"到達ランク: {simResult.FinalRank}");
            sb.AppendLine();

            sb.AppendLine("--- Lv1 ステータス ---");
            string[] statNames = { "AT ", "DF ", "AGI", "MAT", "MDF" };
            StatKind[] kinds = { StatKind.AT, StatKind.DF, StatKind.AGI, StatKind.MAT, StatKind.MDF };
            for (int i = 0; i < 5; i++)
            {
                float evFactor = simResult.EventFactors[i];
                int ptApprox = Mathf.RoundToInt((evFactor - 1.0f) / 0.8f * 40f);
                int lv1 = reinData.GetSoulStat(kinds[i]);
                sb.AppendLine($"  {statNames[i]}: {lv1,4}  (eventFactor={evFactor:F3}x / ~{ptApprox}pt)");
            }
            sb.AppendLine();

            if (simResult.LearnedSkillIds.Count > 0)
            {
                sb.AppendLine("--- 習得スキル ---");
                foreach (var sk in simResult.LearnedSkillIds)
                    sb.AppendLine($"  {sk}");
                sb.AppendLine();
            }

            sb.AppendLine("--- 転生来歴 ---");
            foreach (var ev in simResult.HistoryEvents)
            {
                string tag = ev.EventType switch
                {
                    ReinEventType.Birth => "[誕]",
                    ReinEventType.Happy => "[★]",
                    ReinEventType.Sad => "[涙]",
                    ReinEventType.Shock => "[!!]",
                    ReinEventType.RankUp => "[↑]",
                    ReinEventType.JobChange => "[転]",
                    ReinEventType.LifeEnd => "[終]",
                    _ => "   "
                };
                string ageStr = ev.HideAge ? "      " : $"{ev.Age,3}歳 ";
                sb.AppendLine($"  {ageStr}{tag} {ev.Text}");
            }

            Debug.Log(sb.ToString());
        }

        // ============================================================
        // 内部：OneReinSoulDataを主魂に適用
        // ============================================================
        private static void ApplyToSoul(
            SoulInstance mainSoul,
            OneReinSoulData reinData,
            int replaceIndex)
        {
            if (replaceIndex >= 0 && replaceIndex < mainSoul.ReinSouls.Count)
            {
                mainSoul.ReplaceReinSoul(replaceIndex, reinData);
                Debug.Log($"[ReinSimRunner] スロット{replaceIndex}を上書きしました。");
            }
            else
            {
                mainSoul.AddReinSoul(reinData);
                Debug.Log($"[ReinSimRunner] スロット{mainSoul.ReinSouls.Count - 1}に追加しました。");
            }
        }
    }
}