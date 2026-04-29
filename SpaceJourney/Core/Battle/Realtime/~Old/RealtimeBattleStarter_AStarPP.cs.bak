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

        [Header("攻撃速度グローバル倍率")]
        [Tooltip("全てのCT (基礎攻撃+スキル) に掛ける倍率。1.0=標準、1.5=1.5倍遅い")]
        public float globalCooldownMul = 1.5f;

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

        [Header("武器抽選")]
        [Tooltip("テスト段階の武器プール上限。minAreaLevel <= これの武器が候補")]
        [Range(1, 10)] public int weaponPoolMaxRank = 10;

        [Header("職別 AnimatorController (null の場合は Resources から自動ロード試行)")]
        public RuntimeAnimatorController warriorAnimator;
        public RuntimeAnimatorController knightAnimator;
        public RuntimeAnimatorController archerAnimator;
        public RuntimeAnimatorController mageAnimator;
        public RuntimeAnimatorController lancerAnimator;

        [Header("A* Pathfinding Graph")]
        [Tooltip("GridGraph セルサイズ (m)。戦闘フィールドのマスと合わせると桂馬移動っぽく見える (1.0 推奨)")]
        public float astarCellSize = 1.0f;
        [Tooltip("collision 判定 capsule 直径 (cellSize 比率)。小さいほど path が狭い gap を通る。cellSize=1 なら 0.5 推奨")]
        public float astarCollisionDiameter = 0.5f;
        [Tooltip("true: 4方向のみ (マス目に沿った移動)、false: 8方向 (斜め含む)")]
        public bool astarFourDirectional = true;
        [Tooltip("フィールド端からの歩行禁止マージン (m)。この幅分 graph を縮めてキューブの縁に立てなくする")]
        public float astarFieldMargin = 0.5f;

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

            // manager 初期化
            if (manager == null) manager = gameObject.AddComponent<RealtimeBattleManager>();
            manager.BeginBattle();

            // unit spawn の前に、現在のフィールド位置から GridGraph を再構築。
            // Play 時に BattleFieldVisualizer がキューブを移動させるので、
            // Edit モードで保存された graph とはズレる。ここで追従させる。
            SetupAstarGraphFromBattleGround();

            // unitSpawner を使って3Dモデル配置 (内部で BattleManager.InitBattle も必要)
            // ここでは視覚スポーン + ユニット作成のみ自前実装
            SpawnSide(data.allyUnits, 0, data.allyMorale);
            SpawnSide(data.enemyUnits, 1, data.enemyMorale);
        }

        /// <summary>Layer "BattleGround" の GameObject から bounds/rotation を算出して
        /// GridGraph を現在のフィールド位置に合わせ、Scan する。</summary>
        private void SetupAstarGraphFromBattleGround()
        {
            var astar = AstarPath.active;
            if (astar == null) { Debug.LogWarning("[RealtimeBattleStarter] AstarPath not in scene; skip graph setup"); return; }
            var gg = astar.data?.gridGraph;
            if (gg == null) { Debug.LogWarning("[RealtimeBattleStarter] GridGraph not found; skip"); return; }

            int layerIdx = LayerMask.NameToLayer("BattleGround");
            if (layerIdx < 0) { Debug.LogWarning("[RealtimeBattleStarter] Layer 'BattleGround' not registered"); return; }

            Bounds? combined = null;
            foreach (var go in FindObjectsOfType<GameObject>())
            {
                if (go.layer != layerIdx) continue;
                var rend = go.GetComponentInChildren<Renderer>();
                if (rend == null) continue;
                if (combined == null) combined = rend.bounds;
                else { var b = combined.Value; b.Encapsulate(rend.bounds); combined = b; }
            }
            if (combined == null) { Debug.LogWarning("[RealtimeBattleStarter] no BattleGround GameObject; skip"); return; }

            Vector3 fwd = fieldVisualizer != null ? fieldVisualizer.Forward : Vector3.forward;
            Vector3 right = fieldVisualizer != null ? fieldVisualizer.RightAxis : Vector3.right;
            if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
            if (right.sqrMagnitude < 0.01f) right = Vector3.right;

            var b2 = combined.Value;
            // cellSize は戦闘フィールドのマス (BattleFieldVisualizer.cellSize、デフォルト 1m) を優先。
            // Inspector の astarCellSize は override 用 (0 以下なら field を使う)。
            float fieldCellSize = fieldVisualizer != null && fieldVisualizer.cellSize > 0.01f
                ? fieldVisualizer.cellSize : 1.0f;
            float cellSize = astarCellSize > 0.01f ? astarCellSize : fieldCellSize;
            float worldWidth = Mathf.Abs(right.x) * b2.size.x + Mathf.Abs(right.z) * b2.size.z;
            float worldDepth = Mathf.Abs(fwd.x)   * b2.size.x + Mathf.Abs(fwd.z)   * b2.size.z;
            // margin 分 (両端) graph を縮める
            float margin = Mathf.Max(0f, astarFieldMargin);
            worldWidth = Mathf.Max(cellSize, worldWidth - margin * 2f);
            worldDepth = Mathf.Max(cellSize, worldDepth - margin * 2f);
            int gridW = Mathf.Max(1, Mathf.RoundToInt(worldWidth / cellSize));
            int gridD = Mathf.Max(1, Mathf.RoundToInt(worldDepth / cellSize));

            gg.center = new Vector3(b2.center.x, b2.max.y, b2.center.z);
            gg.rotation = new Vector3(0f, Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg, 0f);
            gg.SetDimensions(gridW, gridD, cellSize);

            // 4方向 or 8方向。4方向なら unit がマス目に沿ってのみ移動する (斜め不可)
            gg.neighbours = astarFourDirectional
                ? Pathfinding.NumNeighbours.Four
                : Pathfinding.NumNeighbours.Eight;

            // Collision: unit (Layer UnitObstacle) を検知して blocked 化するため明示的に設定。
            // diameter/height は cellSize 比率。diameter 小さいほど path が狭い gap を通れる。
            int obstacleLayer = LayerMask.NameToLayer("UnitObstacle");
            int mask = obstacleLayer >= 0 ? (1 << obstacleLayer) : 0;
            gg.collision.collisionCheck = true;
            gg.collision.heightCheck = false;
            gg.collision.mask = mask;
            gg.collision.type = Pathfinding.Graphs.Grid.ColliderType.Capsule;
            gg.collision.diameter = astarCollisionDiameter;
            gg.collision.height = Mathf.Max(1f, 2.5f / cellSize); // unit height 2.5m を cellSize 比に

            astar.Scan();
            Debug.Log($"[RealtimeBattleStarter] GridGraph rebuilt: {gridW}x{gridD} cellSize={cellSize} center={gg.center} yaw={gg.rotation.y:F1}° mask=0x{mask:X}");
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

                // A* Pathfinding Project コンポーネント追加 (移動は AIPath + RVO 任せ)
                var seeker = go.GetComponent<Pathfinding.Seeker>() ?? go.AddComponent<Pathfinding.Seeker>();
                var aiPath = go.GetComponent<Pathfinding.AIPath>() ?? go.AddComponent<Pathfinding.AIPath>();
                var rvoCtrl = go.GetComponent<Pathfinding.RVO.RVOController>() ?? go.AddComponent<Pathfinding.RVO.RVOController>();
                aiPath.maxSpeed = speed;
                aiPath.endReachedDistance = 0.1f;
                aiPath.slowdownDistance = 0.6f;
                aiPath.enableRotation = false; // 回転は既存ロジックに任せる
                aiPath.simulateMovement = false;       // 初期は停止、Unit が適宜制御
                aiPath.gravity = Vector3.zero; // 重力 OFF (地面 raycast 無効 = 落下防止)
                // constrainInsideGraph は DynamicObstacle 更新と競合して駒送り化するため off。
                // field 外への逸脱は ClampToField が安全ネットとして拾う。
                aiPath.constrainInsideGraph = false;
                rvoCtrl.radius = 0.45f;       // bodyRadius 相当
                // lockWhenNotMoving = true にすると停止中の unit を RVO が固定し、
                // 迂回中の unit がその脇を通れずひっかかる。false で押し退け可能にする。
                rvoCtrl.lockWhenNotMoving = false;

                // A* level 障害物化: CapsuleCollider + Kinematic Rigidbody + DynamicObstacle
                // GridGraph が collisionCheck で unit を「壁」として認識 → 他 unit の path が迂回される
                const int unitObstacleLayer = 6; // "UnitObstacle"
                go.layer = unitObstacleLayer;

                var col = go.GetComponent<CapsuleCollider>();
                if (col == null) col = go.AddComponent<CapsuleCollider>();
                if (col != null)
                {
                    col.radius = 0.45f;
                    col.height = 2.5f;  // graph plane を確実に跨ぐよう拡大
                    col.center = new Vector3(0f, 0.5f, 0f);  // feet の少し上〜上方 1.75m
                    col.direction = 1; // Y軸
                }

                var rb = go.GetComponent<Rigidbody>();
                if (rb == null) rb = go.AddComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true; // 物理影響なし (moving static collider 警告回避)
                    rb.useGravity = false;
                }
                else
                {
                    Debug.LogWarning($"[RealtimeBattleStarter] {go.name}: Rigidbody AddComponent returned null");
                }

                var dynObs = go.GetComponent<Pathfinding.DynamicObstacle>();
                if (dynObs == null) dynObs = go.AddComponent<Pathfinding.DynamicObstacle>();
                if (dynObs != null)
                {
                    dynObs.updateError = 0.5f; // 0.5m 動いたら graph 更新
                    dynObs.checkTime = 0.2f;
                }

                // RealtimeBattleUnit アタッチ
                var rtu = go.AddComponent<RealtimeBattleUnit>();
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
                // スキルCTにもグローバル倍率を適用 (AGI は将来skill側にも効かせる予定)
                rtu.skillCooldownMul = globalCooldownMul * agiMul;
                // 決定間隔も AGI で短縮 (基底 1.0s × agiMul)
                rtu.decisionIntervalSec = Mathf.Max(0.3f, 1.0f * agiMul);
                // 遠距離職は射程の中間付近 (4m) を維持距離に
                rtu.preferredDistanceOverride = (jobId == "Archer" || jobId == "Mage") ? 4f : 0f;

                // Realtime 武器抽選 + 装備 (テスト段階: maxAreaRank=weaponPoolMaxRank で rarity 重み抽選)
                if (!string.IsNullOrEmpty(jobId) && db != null)
                {
                    var rngW = new System.Random();
                    var mainW = db.PickRealtimeWeaponForJob(jobId, weaponPoolMaxRank, rngW);
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

        /// <summary>職別カスタム targetList を返す。null=デフォルト (Always → Enemy Nearest)</summary>
        private List<RealtimeTargetEntry> ResolveTargetListForJob(string jobId)
        {
            switch (jobId)
            {
                case "Mage":
                    return new List<RealtimeTargetEntry>
                    {
                        new RealtimeTargetEntry { // 味方 HP 低い時はそちら (ヒール用)
                            condition = RealtimeCondition.AllyHpBelowPercent,
                            conditionParam = 50f,
                            targetSide = RealtimeTargetSide.Ally,
                            targetSelect = RealtimeTargetSelect.LowestHp,
                            label = "味方HP<50 → 最HP低味方"
                        },
                        new RealtimeTargetEntry { // 現ターゲットが近すぎる場合 → 遠い敵に切替 (遠距離攻撃再開用)
                            condition = RealtimeCondition.TargetWithinClose,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Farthest,
                            label = "敵接近 → 最遠敵に切替"
                        },
                        new RealtimeTargetEntry { // 通常: 最寄敵
                            condition = RealtimeCondition.Always,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            label = "通常: 最寄敵"
                        },
                    };
                case "Archer":
                    return new List<RealtimeTargetEntry>
                    {
                        new RealtimeTargetEntry {
                            condition = RealtimeCondition.TargetWithinClose,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Farthest,
                            label = "敵接近 → 最遠敵に切替"
                        },
                        new RealtimeTargetEntry {
                            condition = RealtimeCondition.Always,
                            targetSide = RealtimeTargetSide.Enemy,
                            targetSelect = RealtimeTargetSelect.Nearest,
                            label = "通常: 最寄敵"
                        },
                    };
                case "Warrior":
                case "Knight":
                case "Lancer":
                    return new List<RealtimeTargetEntry>
                    {
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
                        new RealtimeActionEntry { // 攻撃優先: 1m内 + CT空なら DaggerStab
                            condition = RealtimeCondition.CanCastSkill,
                            conditionSkillIndex = 1,
                            action = RealtimeAction.CastSkill, actionSkillIndex = 1,
                            label = "1m内 DaggerStab"
                        },
                        new RealtimeActionEntry(RealtimeCondition.CanBasicAttack, RealtimeAction.BasicAttack, "ArrowShot"),
                        new RealtimeActionEntry(RealtimeCondition.TargetWithinClose, RealtimeAction.MoveAwayToMid, "攻撃できない + 近接 → 離れる"),
                        new RealtimeActionEntry(RealtimeCondition.Always, RealtimeAction.MoveToOwnRangeKeep, "射程維持 (5m)"),
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
                        new RealtimeActionEntry { // 味方HP<50% → HealingWave (CT無ければ fall-through)
                            condition = RealtimeCondition.AllyHpBelowPercent,
                            conditionParam = 50f,
                            action = RealtimeAction.CastSkill, actionSkillIndex = 1,
                            label = "味方HP<50 → HealingWave"
                        },
                        new RealtimeActionEntry(RealtimeCondition.CanBasicAttack, RealtimeAction.BasicAttack, "Burst (3-5m)"),
                        new RealtimeActionEntry(RealtimeCondition.TargetWithinClose, RealtimeAction.MoveAwayToFar, "攻撃できない + 近接 → 離れる"),
                        new RealtimeActionEntry(RealtimeCondition.Always, RealtimeAction.MoveToOwnRangeKeep, "射程維持 (5m)"),
                    };

                case "Warrior":
                default:
                    return null; // デフォルト使用 (skills[2]→[1]→basic→Move)
            }
        }

        /// <summary>ユニット用スキルリストを取得。BodyJobDefinition.realtimeSkills 優先、なければ基礎スキル1個のみ</summary>
        private List<RealtimeSkillDefinition> ResolveSkillsForUnit(SpaceJourneyUnit u)
        {
            var bj = u?.Body?.BodyJob;
            if (bj != null && bj.realtimeSkills != null && bj.realtimeSkills.Count > 0)
            {
                return new List<RealtimeSkillDefinition>(bj.realtimeSkills);
            }
            string jobId = u?.Body?.BodyJobId;
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
