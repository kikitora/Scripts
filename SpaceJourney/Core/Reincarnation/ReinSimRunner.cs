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
            { "job_yakuza",        "warrior_yakuza"   },
            { "job_fraud",         "archer_sagishi"   },
            { "job_collector",     "lancer_tatekiya"  },
            { "job_cult",          "mage_kult"        },
            { "job_fighter",       "warrior_kakutou"  },
            { "job_golfer",        "archer_golfer"    },
            { "job_rugby",         "lancer_rugby"     },
            { "job_researcher",    "mage_kenkyusha"   },
            { "job_surgeon",       "archer_gekai"     },
            { "job_lawyer",        "knight_bengoshi"  },
            { "job_prosecutor",    "knight_kensatu"   },
            { "job_emt",           "knight_kyumei"    },
            { "job_bodyguard",     "knight_bodyguard" },
            { "job_detective",     "knight_keiji"     },
            { "job_entrepreneur",  "lancer_kigyoka"   },
            { "job_revolutionary", "lancer_kakumeika" },
            { "job_adventurer",    "lancer_boukenja"  },
            { "job_jieitai",       "warrior_jieitai"  },
            { "job_shobo",         "warrior_shobo"    },
            { "job_yohei",         "warrior_yohei"    },
            { "job_karyudo",       "archer_karyudo"   },
            { "job_kanteishi",     "archer_kanteishi" },
            { "job_sakka",         "mage_sakka"       },
            { "job_uranai",        "mage_uranai"      },
            { "job_artist",        "mage_geijutsu"    },
        };

        /// <summary>
        /// 転生シミュを実行し、結果を主魂のOneReinSoulDataとして追加する。
        /// ジョブは人生イベントの流れから自然に確定する。
        /// </summary>
        public static OneReinSoulData Run(
            SoulInstance mainSoul,
            SoulInstance[] guardians,
            IReadOnlyList<ReinLifeEventSO> allEvents,
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
            // 3) ジョブ先行抽選（傾向に合ったジョブをjobEasePercentで重み付き選択）
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
            var result = ReinSimulator.Run(input, allEvents ?? new List<ReinLifeEventSO>());

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
        // 内部：傾向に合ったジョブを jobEasePercent で重み付き抽選（先行抽選方式）
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
                // jobEasePercent が公開されている想定。なければ均等重み
                float w = j.JobEasePercent > 0 ? j.JobEasePercent : 1f;
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