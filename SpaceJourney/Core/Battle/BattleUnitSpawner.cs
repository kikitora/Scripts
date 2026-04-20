using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// BattleUnitPlacement のリストから 3D モデルをシーン上に配置する。
    /// RaceDefinition.GetBodyPrefab() でモデルを解決し、
    /// BattleFieldVisualizer でグリッド座標をワールド座標に変換。
    /// アニメーションや状態同期はしない (静的配置のみ)。
    /// </summary>
    public class BattleUnitSpawner : MonoBehaviour
    {
        [Header("参照")]
        [Tooltip("グリッド座標→ワールド座標変換用 (同じシーンに置く)")]
        public BattleFieldVisualizer fieldVisualizer;

        [Tooltip("生成先の親 (null なら自身)")]
        public Transform unitsParent;

        [Header("フォールバック")]
        [Tooltip("Race/Job prefab が未登録時の代替モデル (任意)")]
        public GameObject fallbackPrefab;

        [Tooltip("fallback が null なら PrimitiveCapsule を生成")]
        public bool usePrimitiveFallback = true;

        [Header("見た目調整")]
        [Tooltip("全モデル共通のスケール倍率")]
        public float modelScale = 1f;

        [Tooltip("敵を味方方向に向けるか (180度回転)")]
        public bool faceOpponent = true;

        private readonly List<GameObject> spawnedModels = new();
        // Unit → GameObject マッピング (位置同期用)
        private readonly Dictionary<SpaceJourneyUnit, GameObject> unitToModel = new();
        // Unit → BattleUnitAnimator マッピング (アニメ再生用)
        private readonly Dictionary<SpaceJourneyUnit, BattleUnitAnimator> unitToAnimator = new();

        [Header("アニメーション")]
        [Tooltip("Animator を持つモデルに自動で BattleUnitAnimator を追加し、攻撃/被弾/死亡を再生する")]
        public bool enableAnimation = true;

        [Header("チェス駒モード (実験)")]
        [Tooltip("true: アニメを初期ポーズで凍結し、移動時はチェス駒のように持ち上げ→弧→着地で動く")]
        public bool chessMode = false;

        [Tooltip("チェス駒移動の弧の高さ (ワールド単位)")]
        public float chessArcHeight = 0.8f;

        [Tooltip("凍結までの猶予 (秒、ポーズに入るまで)")]
        public float chessFreezeDelaySec = 0.1f;

        [Header("移動補間")]
        [Tooltip("位置同期時に滑らかに移動する (秒)。0なら瞬間移動")]
        public float moveInterpolationSec = 0.2f;

        [Tooltip("強制移動 (ノックバック等) の移動秒数。通常より速く。向きも変えない")]
        public float forcedMoveSec = 0.15f;

        [Tooltip("死亡時、モデルを倒す (X軸90度回転)")]
        public bool rotateOnDeath = true;

        [Tooltip("死亡時、モデルを非表示にするなら true (false=倒れる表現)")]
        public bool hideOnDeath = false;

        /// <summary>生成済みモデルを全削除</summary>
        public void ClearAll()
        {
            foreach (var go in spawnedModels)
            {
                if (go == null) continue;
                if (Application.isPlaying) Destroy(go);
                else DestroyImmediate(go);
            }
            spawnedModels.Clear();
            unitToModel.Clear();
            unitToAnimator.Clear();
        }

        /// <summary>BattleStartData から両サイド一括生成</summary>
        public void SpawnAll(BattleStartData data)
        {
            ClearAll();
            SpawnFromPlacements(data.allyUnits, 0);
            SpawnFromPlacements(data.enemyUnits, 1);
        }

        /// <summary>
        /// BattleManager からスポーン (Init 後に呼ぶ)。
        /// Field 上の全ユニットを走査してモデル生成、unit→GameObject マッピングを保持。
        /// 以降 UpdatePositions() で Transform を同期できる。
        /// </summary>
        public void SpawnAll(BattleManager manager, BattleStartData data)
        {
            ClearAll();
            if (manager == null)
            {
                Debug.LogError("[BattleUnitSpawner] manager が null");
                return;
            }

            // placements と field の units を順番で突合
            SpawnSideMapped(manager, data.allyUnits, 0);
            SpawnSideMapped(manager, data.enemyUnits, 1);
        }

        private void SpawnSideMapped(BattleManager manager, List<BattleUnitPlacement> placements, int side)
        {
            if (placements == null) return;
            if (fieldVisualizer == null) { Debug.LogError("[BattleUnitSpawner] fieldVisualizer 未設定"); return; }

            var db = MasterDatabase.Instance;
            Transform parent = unitsParent != null ? unitsParent : transform;

            Vector3 battleForward = fieldVisualizer.Forward;
            Quaternion allyRot = Quaternion.LookRotation(battleForward, Vector3.up);
            Quaternion enemyRot = Quaternion.LookRotation(-battleForward, Vector3.up);

            // Field 上の該当 side の全ユニットを、placement 順に並んでいる前提で拾う
            var sideUnits = manager.Field.GetAllUnits(side);

            int idx = 0;
            foreach (var p in placements)
            {
                if (idx >= sideUnits.Count) break;
                var unit = sideUnits[idx++];

                Vector3 worldPos = fieldVisualizer.GridToWorldPosition(side, p.battleCell.x, p.battleCell.y);

                GameObject prefab = ResolvePrefab(p, db);
                GameObject go = InstantiateWithFallback(prefab, worldPos, parent);
                if (go == null) continue;

                go.transform.localScale = Vector3.one * modelScale;
                if (faceOpponent)
                    go.transform.rotation = (side == 0) ? allyRot : enemyRot;

                go.name = $"{(side == 0 ? "Ally" : "Enemy")}_{GetLabel(p)}_{p.battleCell.x},{p.battleCell.y}";
                spawnedModels.Add(go);
                unitToModel[unit] = go;

                // Animator 付きモデルに BattleUnitAnimator 追加
                if (enableAnimation)
                {
                    var animatorComp = go.GetComponentInChildren<Animator>();
                    if (animatorComp != null && animatorComp.runtimeAnimatorController != null)
                    {
                        var ua = go.GetComponent<BattleUnitAnimator>();
                        if (ua == null) ua = go.AddComponent<BattleUnitAnimator>();
                        ua.PlayIdle();
                        unitToAnimator[unit] = ua;

                        // AnimationEvent の空打ち警告抑制用レシーバを Animator と同じ GameObject に追加
                        var animGO = animatorComp.gameObject;
                        if (animGO.GetComponent<AnimationEventReceiver>() == null)
                            animGO.AddComponent<AnimationEventReceiver>();

                        // チェス駒モード: 少し待ってから Animator を凍結
                        if (chessMode)
                            StartCoroutine(FreezeAnimatorAfterDelay(animatorComp, chessFreezeDelaySec));
                    }
                }
            }
        }

        /// <summary>
        /// 戦闘の現在状態から全モデルの位置/向き/死亡状態を更新する。
        /// RunStepByStep の各 tick 後に呼ぶ。
        /// </summary>
        public void UpdatePositions(BattleManager manager)
        {
            if (manager == null || fieldVisualizer == null) return;

            Vector3 battleForward = fieldVisualizer.Forward;
            Quaternion allyRot = Quaternion.LookRotation(battleForward, Vector3.up);
            Quaternion enemyRot = Quaternion.LookRotation(-battleForward, Vector3.up);

            // アニメーション: 今 tick にスキル発動/被弾したユニットにトリガー
            // チェス駒モードでは Animator 凍結中なので、死亡時のみ Destroy 相当の処理
            if (enableAnimation && !chessMode)
            {
                foreach (var actor in manager.SkillActorsThisTick)
                {
                    if (actor == null || actor.IsDead) continue;
                    if (unitToAnimator.TryGetValue(actor, out var ua) && ua != null)
                        ua.PlayAttack();
                }
                foreach (var victim in manager.DamagedUnitsThisTick)
                {
                    if (victim == null) continue;
                    if (unitToAnimator.TryGetValue(victim, out var ua) && ua != null)
                    {
                        if (victim.IsDead) ua.PlayDeath();
                        else ua.PlayDamage();
                    }
                }
            }

            // 攻撃時の向き: actor を primary target セルの方向に回転
            foreach (var actor in manager.SkillActorsThisTick)
            {
                if (actor == null || actor.IsDead) continue;
                if (!unitToModel.TryGetValue(actor, out var actorGo) || actorGo == null) continue;
                if (!manager.ActorPrimaryTargetCell.TryGetValue(actor, out var targetPos)) continue;

                Vector3 actorWorld = actorGo.transform.position;
                Vector3 targetWorld = fieldVisualizer.GridToWorldPosition(targetPos.side, targetPos.cell.x, targetPos.cell.y);
                Vector3 dir = targetWorld - actorWorld;
                dir.y = 0;
                if (dir.sqrMagnitude < 0.01f) continue;

                actorGo.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }

            foreach (var kvp in unitToModel)
            {
                var unit = kvp.Key;
                var go = kvp.Value;
                if (go == null) continue;

                // 死亡処理 (アニメ優先、hideOnDeath の場合は一定秒後に非表示)
                if (unit.IsDead)
                {
                    if (hideOnDeath)
                    {
                        if (go.activeSelf) go.SetActive(false);
                        continue;
                    }
                    if (rotateOnDeath)
                    {
                        // 既に倒れてる場合はスキップ
                        if (Mathf.Approximately(go.transform.eulerAngles.x, 90f)) continue;
                        go.transform.rotation *= Quaternion.Euler(90, 0, 0);
                    }
                    continue;
                }

                // 現在位置を取得 (物理位置 = どの side のマスにいるか)
                var pos = manager.Field.FindUnit(unit);
                if (pos.x < 0) continue; // 盤外

                int physicalSide = pos[0];
                // 向きは所属陣営で決める (敵陣地に侵入しても反転しない)
                int ownerSide = manager.Field.GetSide(unit);
                Vector3 targetPos = fieldVisualizer.GridToWorldPosition(physicalSide, pos[1], pos[2]);
                Quaternion targetRot = (ownerSide == 0) ? allyRot : enemyRot;

                // 強制移動 (ノックバック等) か判定
                bool isForced = manager.ForcedMovedThisTick.Contains(unit);

                // 位置が変わっていない場合は move コルーチンを起動しない
                // (XZ平面で比較。Yは arc で変動するので無視)
                Vector3 currentPos = go.transform.position;
                Vector2 currentXZ = new(currentPos.x, currentPos.z);
                Vector2 targetXZ = new(targetPos.x, targetPos.z);
                bool positionChanged = (currentXZ - targetXZ).sqrMagnitude > 0.01f;

                if (!go.activeSelf) go.SetActive(true);

                // このユニットが今tick攻撃中か (攻撃中は target 向きを保持)
                bool isActingAttacker = manager.SkillActorsThisTick.Contains(unit);

                if (!positionChanged)
                {
                    // 位置変わらず → 向きは触らない (攻撃時の target 向きを維持)
                    // Y のみ arc 残留対策で同期
                    var p = go.transform.position;
                    p.y = targetPos.y;
                    go.transform.position = p;
                    continue;
                }

                if (moveInterpolationSec > 0f)
                {
                    StartMoveCoroutine(go, targetPos, targetRot, isForced);
                }
                else
                {
                    go.transform.position = targetPos;
                    if (faceOpponent && !isActingAttacker) go.transform.rotation = targetRot;
                }
            }
        }

        private void StartMoveCoroutine(GameObject go, Vector3 target, Quaternion targetRot, bool isForced)
        {
            var mover = go.GetComponent<UnitMover>();
            if (mover == null) mover = go.AddComponent<UnitMover>();

            // チェス駒モードでは弧を追加、向き変更もスキップ (駒は向きを変えない)
            float arc = chessMode ? chessArcHeight : 0f;
            bool noTurn = chessMode || isForced;

            if (isForced)
            {
                mover.StartMove(target, targetRot, forcedMoveSec, useRotation: faceOpponent, forceNoTurning: noTurn, arcHeight: arc);
            }
            else
            {
                mover.StartMove(target, targetRot, moveInterpolationSec, useRotation: faceOpponent, forceNoTurning: noTurn, arcHeight: arc);
            }
        }

        private System.Collections.IEnumerator FreezeAnimatorAfterDelay(Animator animatorComp, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (animatorComp != null) animatorComp.speed = 0f;
        }

        /// <summary>指定 side の placements を生成</summary>
        public void SpawnFromPlacements(List<BattleUnitPlacement> placements, int side)
        {
            if (fieldVisualizer == null)
            {
                Debug.LogError("[BattleUnitSpawner] fieldVisualizer が未設定");
                return;
            }
            if (placements == null) return;

            var db = MasterDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[BattleUnitSpawner] MasterDatabase が見つからない");
                return;
            }

            Transform parent = unitsParent != null ? unitsParent : transform;

            // 戦場の向き (ランダム4方向) を考慮して rotation を計算
            // Player (side=0) は forward 方向 (敵を向く)
            // Enemy  (side=1) は -forward 方向 (味方を向く)
            Vector3 battleForward = fieldVisualizer.Forward;
            Quaternion allyRot = Quaternion.LookRotation(battleForward, Vector3.up);
            Quaternion enemyRot = Quaternion.LookRotation(-battleForward, Vector3.up);

            foreach (var p in placements)
            {
                Vector3 worldPos = fieldVisualizer.GridToWorldPosition(
                    side, p.battleCell.x, p.battleCell.y);

                GameObject prefab = ResolvePrefab(p, db);
                GameObject go = InstantiateWithFallback(prefab, worldPos, parent);
                if (go == null) continue;

                go.transform.localScale = Vector3.one * modelScale;

                // 向きをセット (ランダム戦場方向に対応)
                if (faceOpponent)
                    go.transform.rotation = (side == 0) ? allyRot : enemyRot;

                go.name = $"{(side == 0 ? "Ally" : "Enemy")}_{GetLabel(p)}_{p.battleCell.x},{p.battleCell.y}";
                spawnedModels.Add(go);
            }
        }

        private GameObject InstantiateWithFallback(GameObject prefab, Vector3 pos, Transform parent)
        {
            if (prefab != null)
                return Instantiate(prefab, pos, Quaternion.identity, parent);

            if (fallbackPrefab != null)
                return Instantiate(fallbackPrefab, pos, Quaternion.identity, parent);

            if (usePrimitiveFallback)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.position = pos;
                go.transform.SetParent(parent, worldPositionStays: true);
                return go;
            }

            return null;
        }

        private GameObject ResolvePrefab(BattleUnitPlacement p, MasterDatabase db)
        {
            // 専用敵定義ルート
            if (p.IsEnemyDef)
                return p.enemyDef.modelPrefab;

            // Soul+Body ルート: RaceDefinition.GetBodyPrefab()
            if (p.body == null) return null;
            var race = db.GetRaceById(p.body.RaceId);
            if (race == null) return null;
            var bodyJob = db.GetBodyJobById(p.body.BodyJobId);
            if (bodyJob == null) return null;
            return race.GetBodyPrefab(bodyJob);
        }

        private string GetLabel(BattleUnitPlacement p)
        {
            if (p.IsEnemyDef)
                return !string.IsNullOrEmpty(p.enemyDef.displayName) ? p.enemyDef.displayName : p.enemyDef.enemyId;
            if (p.body == null) return "Unknown";
            return $"{p.body.RaceId}_{p.body.BodyJobId}";
        }
    }

    /// <summary>
    /// ユニットを指定位置まで補間移動する補助コンポーネント (BattleUnitSpawner が使う)。
    /// 移動中は進行方向を向き、到着直前で targetRot (敵方向) にブレンドする。
    /// </summary>
    public class UnitMover : MonoBehaviour
    {
        private Vector3 startPos, targetPos;
        private Quaternion targetRot;
        private Quaternion moveDirRot;
        private bool hasMoveDir;
        private float elapsed;
        private float duration;
        private bool useRotation;
        private bool moving;

        private bool forceNoTurning;
        private float arcHeight;

        public void StartMove(Vector3 target, Quaternion rot, float duration, bool useRotation, bool forceNoTurning = false, float arcHeight = 0f)
        {
            this.startPos = transform.position;
            this.targetPos = target;
            this.targetRot = rot;
            this.duration = Mathf.Max(0.01f, duration);
            this.useRotation = useRotation;
            this.forceNoTurning = forceNoTurning;
            this.arcHeight = arcHeight;
            this.elapsed = 0f;
            this.moving = true;

            // 移動方向を計算 (xz平面)。強制移動モードでは向き変更しないのでスキップ
            if (forceNoTurning)
            {
                hasMoveDir = false;
            }
            else
            {
                Vector3 dir = target - startPos;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                {
                    moveDirRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    hasMoveDir = true;
                }
                else
                {
                    hasMoveDir = false;
                }
            }
        }

        private void Update()
        {
            if (!moving) return;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
            // 弧移動 (チェス駒モード): Sin カーブで持ち上げ→着地
            if (arcHeight > 0f)
                pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = pos;

            if (useRotation && !forceNoTurning)
            {
                if (hasMoveDir)
                {
                    // 前半 80%: 移動方向を向く
                    // 後半 20%: targetRot (敵方向) へブレンド
                    if (t < 0.8f)
                        transform.rotation = moveDirRot;
                    else
                    {
                        float b = (t - 0.8f) / 0.2f;
                        transform.rotation = Quaternion.Slerp(moveDirRot, targetRot, b);
                    }
                }
                else
                {
                    transform.rotation = targetRot;
                }
            }
            // forceNoTurning=true: 回転を触らない (攻撃時の target 向き/ノックバック時の現在向きを維持)

            if (t >= 1f) moving = false;
        }
    }
}
