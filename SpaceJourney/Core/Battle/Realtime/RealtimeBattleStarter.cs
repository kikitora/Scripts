using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// リアルタイム戦闘スターター。
    /// TestBattleStarter が作った placement を受け取り、
    /// 3D モデルをスポーン → RealtimeBattleUnit アタッチ → RealtimeBattleManager 開始。
    /// </summary>
    public class RealtimeBattleStarter : MonoBehaviour
    {
        [Header("参照")]
        public BattleFieldVisualizer fieldVisualizer;
        public BattleUnitSpawner unitSpawner;
        public RealtimeBattleManager manager;

        [Header("移動速度 (職別、mass/秒)")]
        [Tooltip("戦士: やや速い")]
        public float warriorWalkSpeed = 1.2f;
        [Tooltip("騎士: 普通")]
        public float knightWalkSpeed = 1.0f;
        [Tooltip("ランサー: 普通")]
        public float lancerWalkSpeed = 1.0f;
        [Tooltip("射手: 普通")]
        public float archerWalkSpeed = 1.0f;
        [Tooltip("魔術師: 遅い")]
        public float mageWalkSpeed = 0.7f;
        [Tooltip("速度全体係数")]
        public float walkSpeedCoefficient = 1f;

        [Header("攻撃CT (職別、秒)")]
        [Tooltip("戦士 基本攻撃CT (元 cost=2 → 2秒)")]
        public float warriorAttackCooldown = 2.0f;
        [Tooltip("騎士 基本攻撃CT (cost=2)")]
        public float knightAttackCooldown = 2.0f;
        [Tooltip("ランサー 基本攻撃CT (cost=3)")]
        public float lancerAttackCooldown = 3.0f;
        [Tooltip("射手 基本攻撃CT (cost=3)")]
        public float archerAttackCooldown = 3.0f;
        [Tooltip("魔術師 基本攻撃CT (cost=4)")]
        public float mageAttackCooldown = 4.0f;

        [Header("AGI 個人差")]
        [Tooltip("AGI の基準値 (この値でCT倍率 1.0)")]
        public int agiBase = 30;
        [Tooltip("AGI 1 あたりのCT短縮率 (0.015 = 1ポイントで 1.5% 短縮)")]
        public float agiCooldownPerPoint = 0.015f;
        [Tooltip("AGI による CT倍率の許容範囲 [最低, 最高]")]
        public float agiCooldownMinMul = 0.7f;
        public float agiCooldownMaxMul = 1.3f;

        [Header("攻撃距離カテゴリ (mass)")]
        public float rangeCloseMass = 1f;
        public float rangeMidMass = 2f;
        public float rangeFarMass = 5f;
        public float rangeMaxFarMass = 8f;

        [Header("最低攻撃距離 (職別、これより近いと攻撃不可)")]
        [Tooltip("戦士: 制限なし")]
        public float warriorMinAttackRange = 0f;
        [Tooltip("騎士: 制限なし")]
        public float knightMinAttackRange = 0f;
        [Tooltip("ランサー: 制限なし")]
        public float lancerMinAttackRange = 0f;
        [Tooltip("射手: 少しだけ距離必要 (短射程でも攻撃可)")]
        public float archerMinAttackRange = 0.8f;
        [Tooltip("魔術師: 近接は不能、離れてから魔法")]
        public float mageMinAttackRange = 2.0f;

        [Header("基本攻撃")]
        [Tooltip("基本攻撃の再使用間隔 (秒)")]
        public float basicAttackCooldownSec = 0.8f;

        [Header("職別 AnimatorController (null の場合は Resources から自動ロード試行)")]
        public RuntimeAnimatorController warriorAnimator;
        public RuntimeAnimatorController knightAnimator;
        public RuntimeAnimatorController archerAnimator;
        public RuntimeAnimatorController mageAnimator;
        public RuntimeAnimatorController lancerAnimator;

        /// <summary>
        /// TestBattleStarter から呼ぶ: placements を元にユニットをスポーン&起動
        /// </summary>
        public void StartRealtimeBattle(BattleStartData data)
        {
            // 範囲テーブルを RealtimeBattleUnit に反映
            RealtimeBattleUnit.RangeCloseMass = rangeCloseMass;
            RealtimeBattleUnit.RangeMidMass = rangeMidMass;
            RealtimeBattleUnit.RangeFarMass = rangeFarMass;
            RealtimeBattleUnit.RangeMaxFarMass = rangeMaxFarMass;

            // manager 初期化
            if (manager == null) manager = gameObject.AddComponent<RealtimeBattleManager>();
            manager.BeginBattle();

            // unitSpawner を使って3Dモデル配置 (内部で BattleManager.InitBattle も必要)
            // ここでは視覚スポーン + ユニット作成のみ自前実装
            SpawnSide(data.allyUnits, 0, data.allyMorale);
            SpawnSide(data.enemyUnits, 1, data.enemyMorale);
        }

        private void SpawnSide(List<BattleUnitPlacement> placements, int side, float morale)
        {
            if (placements == null || unitSpawner == null || fieldVisualizer == null) return;
            var db = MasterDatabase.Instance;
            if (db == null) { Debug.LogError("MasterDatabase not found"); return; }

            float moraleMul = BattleManager.CalcMoraleMultiplier(morale);
            Transform parent = unitSpawner.unitsParent != null ? unitSpawner.unitsParent : unitSpawner.transform;

            foreach (var p in placements)
            {
                // 既存 BattleManager と同じロジックで SpaceJourneyUnit を生成
                SpaceJourneyUnit u;
                if (p.IsEnemyDef)
                {
                    u = p.enemyDef.CreateUnit(p.enemyRank, morale);
                }
                else
                {
                    u = new SpaceJourneyUnit(p.soul, p.body);
                    u.MoraleMultiplier = moraleMul;
                }
                if (u == null) continue;

                // ワールド位置 (グリッドから計算、初期配置のみ)
                Vector3 worldPos = fieldVisualizer.GridToWorldPosition(side, p.battleCell.x, p.battleCell.y);

                // 3Dモデル生成 (UnitSpawner の ResolvePrefab/Fallback を間接呼び出し)
                GameObject go = SpawnModel(p, worldPos, side, db, parent);
                if (go == null) continue;

                go.name = $"RT_{(side == 0 ? "Ally" : "Enemy")}_{GetLabel(p)}";

                // 向き: 敵側を向く
                Vector3 battleFwd = fieldVisualizer.Forward;
                Vector3 faceFwd = (side == 0) ? battleFwd : -battleFwd;
                go.transform.rotation = Quaternion.LookRotation(faceFwd, Vector3.up);

                string jobId = p.IsEnemyDef ? null : p.body?.BodyJobId;

                // 職別 歩行速度
                float speed = GetWalkSpeedByJob(jobId) * walkSpeedCoefficient;

                // 職別 基本攻撃CT × AGI 個人差倍率
                float baseCd = GetAttackCooldownByJob(jobId);
                float agiMul = Mathf.Clamp(
                    1f - (u.AgiFinal - agiBase) * agiCooldownPerPoint,
                    agiCooldownMinMul, agiCooldownMaxMul);
                float attackCd = baseCd * agiMul;

                // 職業傾向 → 距離カテゴリ
                var range = GetPreferredRange(p);

                // 表示名
                string label = (side == 0 ? "味" : "敵") + GetShortJobLabel(p) + $"({p.battleCell.x},{p.battleCell.y})";

                // AnimationEvent 警告抑制 + Controller 強制セット (prefab 参照が壊れてる場合の救済)
                var animatorComp = go.GetComponentInChildren<Animator>();
                if (animatorComp != null)
                {
                    var ag = animatorComp.gameObject;
                    if (ag.GetComponent<AnimationEventReceiver>() == null)
                        ag.AddComponent<AnimationEventReceiver>();

                    // 常に職別 controller を強制代入 (prefab 参照の整合性を保証)
                    // jobId は外側のスコープで宣言済み
                    var ctrl = ResolveControllerByJobId(jobId);
                    string before = animatorComp.runtimeAnimatorController != null
                        ? animatorComp.runtimeAnimatorController.name : "null";
                    if (ctrl != null)
                    {
                        animatorComp.runtimeAnimatorController = ctrl;
                        Debug.Log($"[RealtimeBattleStarter] {go.name}: jobId={jobId} ctrl={ctrl.name} (was={before})");
                    }
                    else
                    {
                        Debug.LogWarning($"[RealtimeBattleStarter] {go.name}: jobId={jobId} ctrl=null! inspector field not set. warrior={(warriorAnimator!=null)} knight={(knightAnimator!=null)} lancer={(lancerAnimator!=null)} archer={(archerAnimator!=null)} mage={(mageAnimator!=null)}");
                    }
                }

                // RealtimeBattleUnit アタッチ
                var rtu = go.AddComponent<RealtimeBattleUnit>();
                rtu.Setup(u, side, label, range, speed, manager);
                rtu.basicAttackCooldownSec = attackCd;
                rtu.fieldVisualizer = fieldVisualizer;

                Debug.Log($"[RealtimeBattleStarter] {go.name}: speed={speed:F2} attackCd={attackCd:F2}s (baseCd={baseCd:F1} agi={u.AgiFinal} mul={agiMul:F2})");

                manager.RegisterUnit(rtu);
            }
        }

        private GameObject SpawnModel(BattleUnitPlacement p, Vector3 pos, int side, MasterDatabase db, Transform parent)
        {
            GameObject prefab = null;
            if (p.IsEnemyDef) prefab = p.enemyDef.modelPrefab;
            else if (p.body != null)
            {
                var race = db.GetRaceById(p.body.RaceId);
                var job = db.GetBodyJobById(p.body.BodyJobId);
                if (race != null && job != null) prefab = race.GetBodyPrefab(job);
            }

            GameObject go;
            if (prefab != null)
                go = Instantiate(prefab, pos, Quaternion.identity, parent);
            else if (unitSpawner.fallbackPrefab != null)
                go = Instantiate(unitSpawner.fallbackPrefab, pos, Quaternion.identity, parent);
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.position = pos;
                go.transform.SetParent(parent, worldPositionStays: true);
            }
            go.transform.localScale = Vector3.one * unitSpawner.modelScale;
            return go;
        }

        private float GetWalkSpeedByJob(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return knightWalkSpeed;
            switch (jobId)
            {
                case "Warrior": return warriorWalkSpeed;
                case "Knight":  return knightWalkSpeed;
                case "Lancer":  return lancerWalkSpeed;
                case "Archer":  return archerWalkSpeed;
                case "Mage":    return mageWalkSpeed;
                default: return knightWalkSpeed;
            }
        }

        private float GetMinAttackRangeByJob(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return 0f;
            switch (jobId)
            {
                case "Warrior": return warriorMinAttackRange;
                case "Knight":  return knightMinAttackRange;
                case "Lancer":  return lancerMinAttackRange;
                case "Archer":  return archerMinAttackRange;
                case "Mage":    return mageMinAttackRange;
                default: return 0f;
            }
        }

        private float GetAttackCooldownByJob(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return knightAttackCooldown;
            switch (jobId)
            {
                case "Warrior": return warriorAttackCooldown;
                case "Knight":  return knightAttackCooldown;
                case "Lancer":  return lancerAttackCooldown;
                case "Archer":  return archerAttackCooldown;
                case "Mage":    return mageAttackCooldown;
                default: return knightAttackCooldown;
            }
        }

        private RealtimeBattleUnit.AttackRangeCategory GetPreferredRange(BattleUnitPlacement p)
        {
            if (p.IsEnemyDef) return RealtimeBattleUnit.AttackRangeCategory.Close;
            if (p.body == null) return RealtimeBattleUnit.AttackRangeCategory.Close;

            // body jobId から tendency 推定 (現状は名前ベースの簡易判定)
            string job = (p.body.BodyJobId ?? "").ToLowerInvariant();
            if (job.Contains("archer")) return RealtimeBattleUnit.AttackRangeCategory.Far;
            if (job.Contains("mage")) return RealtimeBattleUnit.AttackRangeCategory.Far;
            if (job.Contains("lancer")) return RealtimeBattleUnit.AttackRangeCategory.Mid;
            if (job.Contains("knight")) return RealtimeBattleUnit.AttackRangeCategory.Close;
            if (job.Contains("warrior")) return RealtimeBattleUnit.AttackRangeCategory.Close;
            return RealtimeBattleUnit.AttackRangeCategory.Close;
        }

        private string GetLabel(BattleUnitPlacement p)
        {
            if (p.IsEnemyDef) return p.enemyDef.displayName ?? p.enemyDef.enemyId;
            if (p.body == null) return "Unknown";
            return p.body.BodyJobId;
        }

        private RuntimeAnimatorController ResolveControllerByJobId(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return null;
            switch (jobId)
            {
                case "Warrior": return warriorAnimator;
                case "Knight":  return knightAnimator;
                case "Archer":  return archerAnimator;
                case "Mage":    return mageAnimator;
                case "Lancer":  return lancerAnimator;
                default: return null;
            }
        }

        private string GetShortJobLabel(BattleUnitPlacement p)
        {
            if (p.IsEnemyDef) return "Ene";
            if (p.body == null) return "?";
            string j = (p.body.BodyJobId ?? "").ToLowerInvariant();
            if (j.Contains("archer")) return "Arc";
            if (j.Contains("mage")) return "Mag";
            if (j.Contains("lancer")) return "Lan";
            if (j.Contains("knight")) return "Kni";
            if (j.Contains("warrior")) return "War";
            return "?";
        }
    }
}
