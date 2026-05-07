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

        [Header("デバッグ: ジョブ別 prefab 強制上書き (テスト用、null なら通常の race ベース選択)")]
        [Tooltip("戦士のキャラ prefab を強制固定 (例: LH01_Swordsman_1)")]
        public GameObject forceWarriorPrefab;
        [Tooltip("騎士のキャラ prefab を強制固定")]
        public GameObject forceKnightPrefab;
        [Tooltip("ランサーのキャラ prefab を強制固定")]
        public GameObject forceLancerPrefab;
        [Tooltip("射手のキャラ prefab を強制固定 (例: LH04_Archer_1 動作テスト用)")]
        public GameObject forceArcherPrefab;
        [Tooltip("魔術師のキャラ prefab を強制固定")]
        public GameObject forceMagePrefab;

        [Header("Passive VFX")]
        [Tooltip("貫通射撃 (Archer rank3) 発動時、矢に取り付ける竜巻 VFX。Polygon Arsenal/Prefabs/Environment/Tornado/BasicTornado.prefab 等")]
        public GameObject pierceShotVfxPrefab;

        [Header("移動速度 (職別、mass/秒)")]
        [Tooltip("戦士: やや速い")]
        public float warriorWalkSpeed = 1.2f;
        [Tooltip("騎士: 普通")]
        public float knightWalkSpeed = 1.0f;
        [Tooltip("ランサー: 機動")]
        public float lancerWalkSpeed = 1.1f;
        [Tooltip("射手: 機動")]
        public float archerWalkSpeed = 1.1f;
        [Tooltip("魔術師: 遅い")]
        public float mageWalkSpeed = 0.8f;
        [Tooltip("速度全体係数")]
        public float walkSpeedCoefficient = 1f;

        [Header("攻撃CT (職別、秒) — castAnim 込み (発動瞬間からカウント)")]
        [Tooltip("戦士 基本攻撃CT (Slash, 3秒)")]
        public float warriorAttackCooldown = 3.0f;
        [Tooltip("騎士 基本攻撃CT (Slash, 3秒)")]
        public float knightAttackCooldown = 3.0f;
        [Tooltip("ランサー 基本攻撃CT (Thrust, 4秒)")]
        public float lancerAttackCooldown = 4.0f;
        [Tooltip("射手 基本攻撃CT (ArrowShot, 4秒)")]
        public float archerAttackCooldown = 4.0f;
        [Tooltip("魔術師 基本攻撃CT (Burst, 5秒)")]
        public float mageAttackCooldown = 5.0f;

        [Header("AGI 個人差")]
        [Tooltip("AGI の基準値 (この値でCT倍率 1.0)")]
        public int agiBase = 30;
        [Tooltip("AGI 1 あたりのCT短縮率 (0.015 = 1ポイントで 1.5% 短縮)")]
        public float agiCooldownPerPoint = 0.015f;
        [Tooltip("AGI による CT倍率の許容範囲 [最低, 最高]")]
        public float agiCooldownMinMul = 0.7f;
        public float agiCooldownMaxMul = 1.3f;

        [Header("攻撃速度グローバル倍率")]
        [Tooltip("全てのCT (基礎攻撃+スキル) に掛ける倍率。1.0=標準、1.5=1.5倍遅い")]
        public float globalCooldownMul = 1.5f;

        [Header("攻撃距離カテゴリ (mass)")]
        public float rangeCloseMass = 1f;
        public float rangeMidMass = 2f;
        public float rangeFarMass = 5f;
        public float rangeMaxFarMass = 6f;

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

        [Header("武器抽選")]
        [Tooltip("テスト段階の武器プール上限。minAreaLevel <= これの武器が候補")]
        [Range(1, 10)] public int weaponPoolMaxRank = 10;

        // 強制武器抽選 (テスト用): jobId → weaponId のマップ。Bridge 経由で set-force-weapon コマンドで設定。
        // 該当 job のユニットを spawn する時、ランダム抽選より優先してこの武器を装備させる。
        // 自陣 (味方) のみに適用 (= ownerSide=0)。 敵側はランダム抽選を維持。
        // 静的辞書なので Domain Reload 後はリセットされる。
        public static readonly System.Collections.Generic.Dictionary<string, string> ForcedWeaponByJob
            = new System.Collections.Generic.Dictionary<string, string>();

        [Header("職別 AnimatorController (null の場合は Resources から自動ロード試行)")]
        public RuntimeAnimatorController warriorAnimator;
        public RuntimeAnimatorController knightAnimator;
        public RuntimeAnimatorController archerAnimator;
        public RuntimeAnimatorController mageAnimator;
        public RuntimeAnimatorController lancerAnimator;

        // 連続座標移動方式 (ContinuousMover) を使用。GridState/GridUnitMover は old/ に退避済。

        [Header("職別 基礎スキル (skills[0] として割り当て)")]
        public RealtimeSkillDefinition warriorBasicSkill;
        public RealtimeSkillDefinition knightBasicSkill;
        public RealtimeSkillDefinition archerBasicSkill;
        public RealtimeSkillDefinition mageBasicSkill;
        public RealtimeSkillDefinition lancerBasicSkill;

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

            // manager 初期化 (BeginBattle はまだ呼ばない。unit 登録 + SimpleGrid Init 後に呼ぶ)
            if (manager == null) manager = gameObject.AddComponent<RealtimeBattleManager>();

            // SimpleGrid 初期化 (spawn 配置 + 境界クランプのみ)。BeginBattle 前に必ず Active 化
            if (fieldVisualizer != null)
            {
                Pathfinding.SimpleGrid.Init(fieldVisualizer.gridWidth, fieldVisualizer.gridDepth, fieldVisualizer.cellSize, fieldVisualizer);
                Debug.Log($"[RealtimeBattleStarter] SimpleGrid initialized: depth={fieldVisualizer.gridDepth*2} width={fieldVisualizer.gridWidth} cellSize={fieldVisualizer.cellSize}");
            }
            else
            {
                Debug.LogError("[RealtimeBattleStarter] fieldVisualizer is null; SimpleGrid not initialized");
            }

            // unitSpawner を使って3Dモデル配置 + RealtimeBattleUnit 登録
            SpawnSide(data.allyUnits, 0, data.allyMorale);
            SpawnSide(data.enemyUnits, 1, data.enemyMorale);

            // BeginBattle (この時点で unit が全登録済み + SimpleGrid.Active 設定済み)
            // → 内部の AttachAllMovers で全 mover が attach され、移動開始可能になる
            manager.BeginBattle();
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

                // 職別 基本攻撃CT × AGI 個人差倍率 × グローバル倍率
                float baseCd = GetAttackCooldownByJob(jobId);
                float agiMul = Mathf.Clamp(
                    1f - (u.AgiFinal - agiBase) * agiCooldownPerPoint,
                    agiCooldownMinMul, agiCooldownMaxMul);
                float attackCd = baseCd * agiMul * globalCooldownMul;

                // 職業傾向 → 距離カテゴリ
                var range = GetPreferredRange(p);

                // 表示名
                string label = (side == 0 ? "味" : "敵") + GetShortJobLabel(p) + $"({p.battleCell.x},{p.battleCell.y})";

                // AnimationEvent 警告抑制 + Controller 強制セット (prefab 参照が壊れてる場合の救済)
                var animatorComp = go.GetComponentInChildren<Animator>();
                if (animatorComp != null)
                {
                    // Animator 強制 enable (Skill Timeline Editor の AnimationMode 等で
                    // 偶然 disable のまま prefab 保存されることがあるので念のため)
                    if (!animatorComp.enabled) animatorComp.enabled = true;

                    // ルートモーション無効化 (Warrior Slash 等の攻撃アニメで前進するのを防ぐ。
                    // 連続移動は ContinuousMover 側で完全制御するためアニメから transform は触らせない)
                    animatorComp.applyRootMotion = false;

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

                // ContinuousMover アタッチ (連続座標移動。occupant 不使用)
                // Setup() 内でも AddComponent するが、先に存在させることで Setup が GetComponent で取得できる。
                var preMover = go.GetComponent<Pathfinding.ContinuousMover>() ?? go.AddComponent<Pathfinding.ContinuousMover>();
                preMover.maxSpeed = speed;
                preMover.endReachedDistance = 1.0f;
                preMover.simulateMovement = false; // 初期停止、Unit 側で適宜制御

                // RealtimeBattleUnit アタッチ
                var rtu = go.AddComponent<RealtimeBattleUnit>();

                // 状態異常ビジュアルバインダー: aura prefab 装着 + Animator.speed 制御
                go.AddComponent<StatusEffectVfxBinder>().Bind(u);
                // スキル割り当て: BodyJobDefinition.realtimeSkills 優先、なければ基礎スキルのみフォールバック
                var skillList = ResolveSkillsForUnit(u);
                if (skillList != null && skillList.Count > 0)
                {
                    if (rtu.skills == null) rtu.skills = new List<RealtimeSkillDefinition>();
                    rtu.skills.Clear();
                    rtu.skills.AddRange(skillList);
                }
                // 職別カスタム actionList を設定 (なければデフォルトが使われる)
                var customAction = ResolveActionListForJob(jobId);
                if (customAction != null && customAction.Count > 0)
                {
                    if (rtu.actionList == null) rtu.actionList = new List<RealtimeActionEntry>();
                    rtu.actionList.Clear();
                    rtu.actionList.AddRange(customAction);
                }
                // 職別カスタム targetList を設定 (なければ default: Always → Enemy, Nearest)
                var customTarget = ResolveTargetListForJob(jobId);
                if (customTarget != null && customTarget.Count > 0)
                {
                    if (rtu.targetList == null) rtu.targetList = new List<RealtimeTargetEntry>();
                    rtu.targetList.Clear();
                    rtu.targetList.AddRange(customTarget);
                }
                rtu.Setup(u, side, label, range, speed, manager);
                rtu.basicAttackCooldownSec = attackCd;
                rtu.fieldVisualizer = fieldVisualizer;
                // 貫通射撃 VFX (hasPierceShot 立ってる職に対してのみ ArrowEqOff で表示)
                rtu.pierceShotVfxPrefab = pierceShotVfxPrefab;
                // スキルCTにもグローバル倍率を適用 (AGI は将来skill側にも効かせる予定)
                rtu.skillCooldownMul = globalCooldownMul * agiMul;
                // 決定間隔も AGI で短縮 (基底 1.0s × agiMul)
                rtu.decisionIntervalSec = Mathf.Max(0.3f, 1.0f * agiMul);
                // 遠距離職は射程内側 0.5m (= 5.5m) を維持距離に。 ArrowShot/Rock の rangeMax 6m 内に収まる。
                rtu.preferredDistanceOverride = (jobId == "Archer" || jobId == "Mage") ? 5.5f : 0f;

                // Realtime 武器抽選 + 装備 (テスト段階: maxAreaRank=weaponPoolMaxRank で rarity 重み抽選)
                if (!string.IsNullOrEmpty(jobId) && db != null)
                {
                    var rngW = new System.Random();
                    // 強制武器: ForcedWeaponByJob に該当 job が登録されてれば、 ランダム抽選より優先 (味方/敵両側)
                    SteraCube.SpaceJourney.Realtime.RealtimeWeaponDefinition mainW = null;
                    if (ForcedWeaponByJob.TryGetValue(jobId, out string forcedId) && !string.IsNullOrEmpty(forcedId))
                    {
                        mainW = db.GetRealtimeWeaponById(forcedId);
                        if (mainW == null)
                            Debug.LogWarning($"[RealtimeBattleStarter] forced weapon '{forcedId}' for {jobId} not found in DB; falling back to random");
                    }
                    if (mainW == null)
                        mainW = db.PickRealtimeWeaponForJob(jobId, weaponPoolMaxRank, rngW);
                    if (mainW != null)
                    {
                        rtu.EquipWeapon(mainW);
                        Debug.Log($"[RealtimeBattleStarter] {go.name}: equipped {mainW.weaponId} (lv{mainW.minAreaLevel} r{mainW.rarity})");
                    }
                    // 騎士は盾も別途装備
                    if (jobId == "Knight")
                    {
                        var shieldW = db.PickRealtimeWeaponForJobKind(jobId, SteraCube.SpaceJourney.Realtime.WeaponKind.Shield, weaponPoolMaxRank, rngW);
                        if (shieldW != null)
                        {
                            rtu.EquipWeapon(shieldW);
                            Debug.Log($"[RealtimeBattleStarter] {go.name}: equipped shield {shieldW.weaponId} (guard {shieldW.guardChance*100:F0}%/{shieldW.guardMitigation*100:F0}%)");
                        }
                    }
                    // Warrior は Walk アニメが両手剣を握れる Run2 に差し替え済 → Walk 中も IK ON。
                    // 他職 (Knight 片手 / Lancer 槍 / Archer 弓 / Mage 杖) は Walk クリップが両手 IK 前提では
                    // ないので、デフォルトの「Walk/WalkLeft/WalkRight で IK OFF」を維持する。
                    if (jobId == "Warrior")
                    {
                        var ik = go.GetComponentInChildren<SteraCube.SpaceJourney.Realtime.WeaponLeftHandIK>();
                        if (ik != null)
                            ik.SetDisableInStates(System.Array.Empty<string>());
                    }
                    // 種族パッシブを unit.activePassives に追加
                    if (!p.IsEnemyDef && p.body != null)
                    {
                        var race = db.GetRaceById(p.body.RaceId);
                        if (race != null && race.realtimePassives != null)
                        {
                            foreach (var rp in race.realtimePassives)
                            {
                                if (rp != null && !rtu.activePassives.Contains(rp))
                                    rtu.activePassives.Add(rp);
                            }
                            if (race.realtimePassives.Count > 0)
                                Debug.Log($"[RealtimeBattleStarter] {go.name}: race={race.raceId} passives x{race.realtimePassives.Count}");
                        }
                    }
                }

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
                // デバッグ: ジョブ別強制 prefab があれば最優先
                var forced = GetForcedPrefabByJob(p.body.BodyJobId);
                if (forced != null)
                {
                    prefab = forced;
                }
                else
                {
                    var race = db.GetRaceById(p.body.RaceId);
                    var job = db.GetBodyJobById(p.body.BodyJobId);
                    if (race != null && job != null) prefab = race.GetBodyPrefab(job);
                }
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

        private GameObject GetForcedPrefabByJob(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return null;
            switch (jobId)
            {
                case "Warrior": return forceWarriorPrefab;
                case "Knight":  return forceKnightPrefab;
                case "Lancer":  return forceLancerPrefab;
                case "Archer":  return forceArcherPrefab;
                case "Mage":    return forceMagePrefab;
            }
            return null;
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

        /// <summary>職別カスタム targetList を返す。null=デフォルト (Always → Enemy Nearest)</summary>
        private List<RealtimeTargetEntry> ResolveTargetListForJob(string jobId)
        {
            switch (jobId)
            {
                case "Mage":
                    return new List<RealtimeTargetEntry>
                    {
                        new RealtimeTargetEntry { // 3m+ 攻撃スキル無し AND 2m内に敵 → 緊急近接切替 (アルカナマキシマス装備等)
                            condition = RealtimeCondition.NoLongRangeAttack,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            rangeFilterMaxDist = 2f,
                            label = "遠距離手段無+2m内 → 緊急近接切替"
                        },
                        new RealtimeTargetEntry { // 味方 HP 低い時はそちら (ヒール用)
                            condition = RealtimeCondition.AllyHpBelowPercent,
                            conditionParam = 50f,
                            targetSide = RealtimeTargetSide.Ally,
                            targetSelect = RealtimeTargetSelect.LowestHp,
                            label = "味方HP<50 → 最HP低味方"
                        },
                        new RealtimeTargetEntry { // 魔法射程内の最寄敵を最優先 (currentTarget→Burst が即発動)
                            condition = RealtimeCondition.Always,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            rangeFilterSkillIndex = 0,  // Burst 射程
                            label = "魔法射程内 → 最寄敵"
                        },
                        new RealtimeTargetEntry { // 射程内に誰も居なければ最寄敵 (近づいて Bash 用)
                            condition = RealtimeCondition.Always,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            label = "射程外しかいない時 → 最寄敵"
                        },
                    };
                case "Archer":
                    return new List<RealtimeTargetEntry>
                    {
                        new RealtimeTargetEntry { // 弓射程内の最寄敵を最優先 (currentTarget→ArrowShot が即発動)
                            condition = RealtimeCondition.Always,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            rangeFilterSkillIndex = 0,  // ArrowShot 射程
                            label = "弓射程内 → 最寄敵"
                        },
                        new RealtimeTargetEntry { // 射程内に誰も居なければ最寄敵 (近づいて DaggerStab 用)
                            condition = RealtimeCondition.Always,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            label = "射程外しかいない時 → 最寄敵"
                        },
                    };
                case "Warrior":
                case "Knight":
                    return new List<RealtimeTargetEntry>
                    {
                        new RealtimeTargetEntry { // 接敵中 → ルート迂回より先にターゲット変更 (引っ掛かり解消)
                            condition = RealtimeCondition.EnemyInContact,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            rangeFilterMaxDist = SteraCube.SpaceJourney.Realtime.RealtimeBattleUnit.EnemyContactRadius,
                            label = "接敵中 → 最寄敵に切替 (引っ掛かり解消)"
                        },
                        new RealtimeTargetEntry { // 現 currentTarget が攻撃範囲外 AND 攻撃範囲内に他の敵 → 接敵中の敵に切替
                            condition = RealtimeCondition.TargetOutsideSkillRange,
                            conditionSkillIndex = 0,  // 基本スキル (Slash) の射程
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            rangeFilterSkillIndex = 0,  // 攻撃範囲内に居る敵だけ候補
                            label = "currentTarget 範囲外 → 接敵中の敵に切替"
                        },
                        new RealtimeTargetEntry { // 被弾 AND attacker が現ターゲットより近い → 切替
                            condition = RealtimeCondition.AttackerCloserThanCurrentTarget,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.LastAttacker,
                            label = "被弾+attacker近い → 切替"
                        },
                        new RealtimeTargetEntry { // 被弾 AND 射程内に敵なし → 反撃
                            condition = RealtimeCondition.AttackedAndNoEnemyInBasicRange,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.LastAttacker,
                            label = "被弾反撃 (射程内敵なし時)"
                        },
                        new RealtimeTargetEntry {
                            condition = RealtimeCondition.Always,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            label = "通常: 最寄敵"
                        },
                    };
                case "Lancer":
                    // Lancer は機動性を活かして遠めの敵を追う設計だが、押し合いで stuck になる場面では
                    // 接敵切替が必要 (ContinuousMover では敵を obstacle 化せず head-on するため)。
                    return new List<RealtimeTargetEntry>
                    {
                        new RealtimeTargetEntry { // 接敵中 → ルート迂回より先にターゲット変更 (引っ掛かり解消)
                            condition = RealtimeCondition.EnemyInContact,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            rangeFilterMaxDist = SteraCube.SpaceJourney.Realtime.RealtimeBattleUnit.EnemyContactRadius,
                            label = "接敵中 → 最寄敵に切替 (引っ掛かり解消)"
                        },
                        new RealtimeTargetEntry { // 被弾 AND attacker が現ターゲットより近い → 切替
                            condition = RealtimeCondition.AttackerCloserThanCurrentTarget,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.LastAttacker,
                            label = "被弾+attacker近い → 切替"
                        },
                        new RealtimeTargetEntry { // 被弾 AND 射程内に敵なし → 反撃
                            condition = RealtimeCondition.AttackedAndNoEnemyInBasicRange,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.LastAttacker,
                            label = "被弾反撃 (射程内敵なし時)"
                        },
                        new RealtimeTargetEntry {
                            condition = RealtimeCondition.Always,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            label = "通常: 最寄敵"
                        },
                    };
                default:
                    return null;
            }
        }

        /// <summary>職別カスタム actionList を返す。null=デフォルト使用。</summary>
        private List<RealtimeActionEntry> ResolveActionListForJob(string jobId)
        {
            switch (jobId)
            {
                case "Knight":
                    return new List<RealtimeActionEntry>
                    {
                        new RealtimeActionEntry { // 弱い味方が狙われてる → Provoke で引受
                            condition = RealtimeCondition.FragileAllyTargetedByEnemy,
                            action = RealtimeAction.CastSkill, actionSkillIndex = 2,
                            label = "弱い味方が狙われてる → Provoke"
                        },
                        new RealtimeActionEntry { // 接敵 (Slash 射程 = 1m 内) かつ Barricade CT 空 → Barricade 設置
                            condition = RealtimeCondition.TargetInSkillRange,
                            conditionSkillIndex = 0, // Slash の射程 (近接) で接敵判定流用
                            action = RealtimeAction.CastSkill, actionSkillIndex = 3,
                            label = "接敵中 → Barricade"
                        },
                        new RealtimeActionEntry { // 敵2体以上内に AND CT空 → Defend
                            condition = RealtimeCondition.EnemiesHitCountGe,
                            conditionSkillIndex = 2, conditionParam = 2f, // Provokeのshape (自分中心2m) で2体判定流用
                            action = RealtimeAction.CastSkill, actionSkillIndex = 1,
                            label = "周囲2m内に敵2体+ → Defend"
                        },
                        new RealtimeActionEntry(RealtimeCondition.CanBasicAttack, RealtimeAction.BasicAttack, "Slash"),
                        new RealtimeActionEntry(RealtimeCondition.Always, RealtimeAction.MoveToOwnRange, "接近"),
                    };

                case "Archer":
                    return new List<RealtimeActionEntry>
                    {
                        // 1. currentTarget が DaggerStab 範囲 (≤1m) かつ CT 空 → 短剣 (近接対応)
                        // ※ DaggerStab はソウルスキル化予定 (memory: project_archer_skill_redesign)
                        new RealtimeActionEntry {
                            condition = RealtimeCondition.CanCastSkill,
                            conditionSkillIndex = 1,
                            action = RealtimeAction.CastSkill, actionSkillIndex = 1,
                            label = "近接1m内 DaggerStab"
                        },
                        // 2. 弓射程内 + CT 空 → ArrowShot (rangeMin=0 で隣接からも撃てる)
                        new RealtimeActionEntry(RealtimeCondition.CanBasicAttack, RealtimeAction.BasicAttack, "ArrowShot"),
                        // 3. 弓射程内 (CT 中) → 維持 (近寄らず後退もせず)
                        new RealtimeActionEntry {
                            condition = RealtimeCondition.TargetInSkillRange,
                            conditionSkillIndex = 0,
                            action = RealtimeAction.MoveToOwnRange,
                            label = "弓射程内 → 待機"
                        },
                        // 4. 射程外 → preferred (4m) まで詰める。
                        // 「近接 1m まで詰める」は廃止 (ArrowShot rangeMin=0 で接敵時も撃てる + Archer は遠距離型)
                        new RealtimeActionEntry(RealtimeCondition.Always, RealtimeAction.MoveToOwnRange, "preferred まで詰める"),
                    };

                case "Lancer":
                    return new List<RealtimeActionEntry>
                    {
                        new RealtimeActionEntry { // 1m内 + CT空なら KnockbackThrust
                            condition = RealtimeCondition.CanCastSkill,
                            conditionSkillIndex = 1,
                            action = RealtimeAction.CastSkill, actionSkillIndex = 1,
                            label = "1m内 KnockbackThrust"
                        },
                        new RealtimeActionEntry(RealtimeCondition.CanBasicAttack, RealtimeAction.BasicAttack, "Thrust (2m)"),
                        new RealtimeActionEntry(RealtimeCondition.Always, RealtimeAction.MoveToOwnRange, "接近"),
                    };

                case "Mage":
                    return new List<RealtimeActionEntry>
                    {
                        // 1. 味方HP<50% → HealingWave (skill[2])
                        new RealtimeActionEntry {
                            condition = RealtimeCondition.AllyHpBelowPercent,
                            conditionParam = 50f,
                            action = RealtimeAction.CastSkill, actionSkillIndex = 2,
                            label = "味方HP<50 → HealingWave"
                        },
                        // 2. Burst (skill[0]) で 2 体以上巻き込み可 → Burst を使う (CT 空き必須)
                        new RealtimeActionEntry {
                            condition = RealtimeCondition.EnemiesHitCountGe,
                            conditionSkillIndex = 0,
                            conditionParam = 2f,
                            action = RealtimeAction.BasicAttack,
                            label = "Burst (2体以上巻込み)"
                        },
                        // 3. Rock (skill[3]) 単体魔法弾、 射程内 + CT 空 → 単体時の主力
                        new RealtimeActionEntry {
                            condition = RealtimeCondition.CanCastSkill,
                            conditionSkillIndex = 3,
                            action = RealtimeAction.CastSkill, actionSkillIndex = 3,
                            label = "Rock (単体魔法)"
                        },
                        // 4. Burst が 1 体だけでも撃てる (Rock CT 中の fallback)
                        new RealtimeActionEntry(RealtimeCondition.CanBasicAttack, RealtimeAction.BasicAttack, "Burst (単体 fallback)"),
                        // 5. 近接 1m 内に敵 + Bash CT 空 → Bash
                        new RealtimeActionEntry {
                            condition = RealtimeCondition.CanCastSkill,
                            conditionSkillIndex = 1,
                            action = RealtimeAction.CastSkill, actionSkillIndex = 1,
                            label = "近接1m内 Bash"
                        },
                        // 6. 魔法射程内 (CT 中) → 維持
                        new RealtimeActionEntry {
                            condition = RealtimeCondition.TargetInSkillRange,
                            conditionSkillIndex = 0,
                            action = RealtimeAction.MoveToOwnRange,
                            label = "魔法射程内 → 待機"
                        },
                        // 7. 射程外 → 近接距離まで詰める
                        new RealtimeActionEntry(RealtimeCondition.Always, RealtimeAction.MoveToCloseRange, "近接 (1m) まで詰める"),
                    };

                case "Warrior":
                    return new List<RealtimeActionEntry>
                    {
                        new RealtimeActionEntry { // CT 整ったら Charge (次の通常攻撃 1.5x)
                            condition = RealtimeCondition.CanCastSkill,
                            conditionSkillIndex = 1,
                            action = RealtimeAction.CastSkill, actionSkillIndex = 1,
                            label = "Charge (次の通常攻撃 1.5x)"
                        },
                        new RealtimeActionEntry(RealtimeCondition.CanBasicAttack, RealtimeAction.BasicAttack, "Slash"),
                        new RealtimeActionEntry(RealtimeCondition.Always, RealtimeAction.MoveToOwnRange, "接近"),
                    };

                default:
                    return null; // デフォルト使用 (skills[2]→[1]→basic→Move)
            }
        }

        /// <summary>ユニット用スキルリストを取得。BodyJobDefinition.realtimeSkills 優先、なければ基礎スキル1個のみ</summary>
        private List<RealtimeSkillDefinition> ResolveSkillsForUnit(SpaceJourneyUnit u)
        {
            string jobId = u?.Body?.BodyJobId;
            var bj = u?.Body?.BodyJob;
            // cachedBodyJob ([NonSerialized]) が Resolve 未実行で null の場合、MasterDatabase 経由で取得する。
            // (BodyJobId は serialized なので必ず取れる。これで Archer の DaggerStab 等 skill[1+] が欠落する不具合を防ぐ)
            if (bj == null && !string.IsNullOrEmpty(jobId))
            {
                bj = MasterDatabase.Instance?.GetBodyJobById(jobId);
            }
            if (bj != null && bj.realtimeSkills != null && bj.realtimeSkills.Count > 0)
            {
                return new List<RealtimeSkillDefinition>(bj.realtimeSkills);
            }
            var basic = ResolveBasicSkillByJobId(jobId);
            var list = new List<RealtimeSkillDefinition>();
            if (basic != null) list.Add(basic);
            return list;
        }

        private RealtimeSkillDefinition ResolveBasicSkillByJobId(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return null;
            RealtimeSkillDefinition fromInspector = null;
            switch (jobId)
            {
                case "Warrior": fromInspector = warriorBasicSkill; break;
                case "Knight":  fromInspector = knightBasicSkill;  break;
                case "Archer":  fromInspector = archerBasicSkill;  break;
                case "Mage":    fromInspector = mageBasicSkill;    break;
                case "Lancer":  fromInspector = lancerBasicSkill;  break;
            }
            if (fromInspector != null) return fromInspector;

#if UNITY_EDITOR
            string skillName = jobId switch
            {
                "Warrior" => "Slash",
                "Knight" => "Slash",
                "Archer" => "ArrowShot",
                "Lancer" => "Thrust",
                "Mage" => "Burst",
                _ => null
            };
            if (!string.IsNullOrEmpty(skillName))
            {
                var path = $"Assets/0SteraCube/ScriptableObject/Skill/BodySkill/{skillName}.asset";
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<RealtimeSkillDefinition>(path);
                if (asset != null) return asset;
            }
#endif
            return null;
        }

        private RuntimeAnimatorController ResolveControllerByJobId(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return null;
            RuntimeAnimatorController fromInspector = null;
            switch (jobId)
            {
                case "Warrior": fromInspector = warriorAnimator; break;
                case "Knight":  fromInspector = knightAnimator;  break;
                case "Archer":  fromInspector = archerAnimator;  break;
                case "Mage":    fromInspector = mageAnimator;    break;
                case "Lancer":  fromInspector = lancerAnimator;  break;
            }
            if (fromInspector != null) return fromInspector;

            // Inspector 参照が外れた場合のフォールバック (Editor 実行時)
#if UNITY_EDITOR
            var path = $"Assets/0SteraCube/Animators/{jobId}_Animator.controller";
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
            if (asset != null) return asset;
#endif
            // Runtime フォールバック (Resources に置いてあれば拾う)
            return Resources.Load<RuntimeAnimatorController>($"{jobId}_Animator");
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
