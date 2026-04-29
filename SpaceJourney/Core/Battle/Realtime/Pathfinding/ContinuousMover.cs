using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.Pathfinding
{
    /// <summary>
    /// 連続座標 Mover。GridUnitMover (旧) と同じ公開 API を保ちつつ占有テーブルを使わない。
    ///
    /// ・直線シーク (障害物なし時)
    /// ・近接ユニット tangent 回避 (head-on デッドロック解消)
    /// ・接触相手方向の velocity 成分を削る (重なり防止)
    /// ・push-out は RealtimeBattleManager 側で全 unit step 後に一括反復解消
    ///
    /// Python continuous_mover.py の 1:1 C# 移植。
    /// </summary>
    public class ContinuousMover : MonoBehaviour
    {
        // ======================== 公開設定 ========================

        /// <summary>移動目標 world 座標</summary>
        public Vector3 destination;

        /// <summary>到達とみなす距離</summary>
        public float endReachedDistance = 1f;

        /// <summary>最大速度 (world units/sec)</summary>
        public float maxSpeed = 1f;

        /// <summary>false なら停止</summary>
        public bool simulateMovement = false;

        /// <summary>ユニット半径 (重なり判定用)</summary>
        public float radius = 0.36f;

        /// <summary>互換のみ (使用しない)</summary>
        public object ignoreOccupant = null;

        // ======================== read-only プロパティ ========================

        public Vector3 velocity { get; private set; }

        public bool reachedDestination { get; private set; }

        public bool IsInTransit => _attached && simulateMovement && !reachedDestination;

        /// <summary>現在位置のグリッドセル (互換: round(world / cell_size))。</summary>
        public Vector2Int CurrentCell
        {
            get
            {
                if (!_attached || _grid == null) return new Vector2Int(-1, -1);
                float cs = _grid.CellSize;
                if (cs < 1e-6f) return new Vector2Int(-1, -1);
                return new Vector2Int(
                    Mathf.RoundToInt(transform.position.x / cs),
                    Mathf.RoundToInt(transform.position.z / cs));
            }
        }

        /// <summary>push-out で動かしてはいけない場合 True。攻撃中 / フリンチ中のみ。</summary>
        public bool IsAnchored
        {
            get
            {
                if (!_attached || _owner == null) return false;
                float bt = _owner.manager != null ? _owner.manager.BattleTime : 0f;
                if (bt < _owner.attackingUntil) return true;
                if (bt < _owner.flinchUntil) return true;
                return false;
            }
        }

        // ======================== 内部状態 ========================

        private bool _attached = false;
        private SimpleGrid _grid;
        private IBarricadeMap _barricadeMap;
        private SteraCube.SpaceJourney.Realtime.RealtimeBattleUnit _owner;
        private List<Vector3> _path;
        private float _pathComputedAt = -999f;
        private float _stuckTime = 0f;  // velocity ≈ 0 が続いた累積時間 (エスケープ判定用)

        // ======================== 初期化 ========================

        public void Init(SteraCube.SpaceJourney.Realtime.RealtimeBattleUnit owner)
        {
            _owner = owner;
        }

        public void Attach(SimpleGrid g, Vector3 world)
        {
            _grid = g;
            transform.position = world;
            velocity = Vector3.zero;
            reachedDestination = false;
            _attached = true;
            _path = null;
            _pathComputedAt = -999f;
        }

        public void Detach()
        {
            _attached = false;
            velocity = Vector3.zero;
            simulateMovement = false;
            _grid = null;
        }

        public void Teleport(Vector3 world, bool clearVelocity = true)
        {
            transform.position = world;
            if (clearVelocity) velocity = Vector3.zero;
            reachedDestination = false;
            _path = null;
        }

        public void StopMovement()
        {
            simulateMovement = false;
            velocity = Vector3.zero;
            reachedDestination = false;
        }

        public void SetBarricadeMap(IBarricadeMap bm)
        {
            _barricadeMap = bm;
        }

        // ======================== メイン更新 ========================

        /// <summary>
        /// 1 フレーム分の移動計算。RealtimeBattleManager._MovementPhase から呼ばれる。
        /// MonoBehaviour.Update() は使わない (manager 主導で順序制御)。
        /// </summary>
        public void StepBattle(float dt, float currentTime)
        {
            if (!_attached || !simulateMovement)
            {
                velocity = Vector3.zero;
                return;
            }

            // 到達判定 (XZ 平面)
            Vector3 to = destination - transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist <= endReachedDistance)
            {
                reachedDestination = true;
                velocity = Vector3.zero;
                return;
            }
            reachedDestination = false;

            // steer_target 解決 (障害物回避 or 直行)
            Vector3 steer = ResolveSteerTarget(currentTime);

            // 速度: 直線方向 + 近接ユニット tangent 回避
            Vector3 d = steer - transform.position;
            d.y = 0f;
            Vector3 dir = d.sqrMagnitude > 1e-9f ? d.normalized : Vector3.zero;
            dir = ApplyAvoidanceSteer(dir);

            Vector3 v = dir * maxSpeed;

            // 接触している他ユニットへ向かう velocity 成分を削る
            v = ClampVelocityAgainstUnits(v, dt);
            // Barricade に向かう velocity 成分を削る (壁通過防止)
            v = ClampVelocityAgainstBarricades(v);

            // スタック検出 + 強制エスケープ:
            // 動こうとしてるのに velocity が 0 近辺 (= clamp で全部削れた) が 0.8s 続いたら、
            // clamp 無視で横方向に強制ステップ (1 フレ分だけ無理やり押し抜ける)。
            const float stuckEscapeThreshold = 0.8f;
            if (v.sqrMagnitude < 0.01f * maxSpeed * maxSpeed)
            {
                _stuckTime += dt;
                if (_stuckTime > stuckEscapeThreshold)
                {
                    // 90° サイドステップ: ID で左右決定論的に、push-out で次フレに重なり解消
                    Vector3 baseDir = dir.sqrMagnitude > 0.01f ? dir : transform.forward;
                    int preferRight = (GetInstanceID() & 1);
                    float angle = preferRight == 0 ? 90f : -90f;
                    Vector3 escape = Quaternion.AngleAxis(angle, Vector3.up) * baseDir;
                    v = escape.normalized * maxSpeed * 0.6f;
                    _stuckTime = 0f;  // リセット
                }
            }
            else
            {
                _stuckTime = 0f;
            }

            velocity = v;

            // 候補位置 (y 軸は固定)
            Vector3 cand = transform.position + v * dt;
            if (_grid != null) cand = _grid.ClampToField(cand);
            cand.y = transform.position.y;

            transform.position = cand;
        }

        // ======================== steer_target 解決 ========================

        /// <summary>障害物があれば A* 経路の次 waypoint、なければ destination 直接。
        /// Python _resolve_steer_target の 1:1 移植。</summary>
        private Vector3 ResolveSteerTarget(float now)
        {
            var bm = _barricadeMap;
            if (bm == null || !bm.IsPresent)
                return destination;

            if (bm.LineOfSight(transform.position, destination))
            {
                _path = null;
                return destination;
            }

            // 再計画判定
            bool needReplan = (_path == null
                || (now - _pathComputedAt) >= 0.4f
                || (_path != null && _path.Count > 0
                    && Vector3.Distance(_path[_path.Count - 1], destination) > 1f));

            if (needReplan)
            {
                _path = bm.FindPath(transform.position, destination);
                _pathComputedAt = now;
            }

            if (_path == null || _path.Count == 0)
                return destination;

            // 通過済み waypoint を除去
            while (_path.Count > 0)
            {
                Vector3 wp = _path[0];
                Vector3 diff = wp - transform.position;
                diff.y = 0f;
                if (diff.magnitude < 0.3f)
                    _path.RemoveAt(0);
                else
                    break;
            }

            return _path.Count > 0 ? _path[0] : destination;
        }

        // ======================== 回避 steer ========================

        /// <summary>前方の近接ユニットを避けるため、velocity に tangent 成分を加える。
        /// head-on デッドロックは ID で左右を決定論的に選ぶ。
        /// Python _apply_avoidance_steer の 1:1 移植 (XZ 平面)。</summary>
        private Vector3 ApplyAvoidanceSteer(Vector3 desiredDir)
        {
            if (_owner == null || _owner.manager == null || radius <= 0f)
                return desiredDir;

            // 検出半径 / steer 強度を大きめにとる。
            // 「前にキャラがいてルートが塞がっている」場合、早めに横に振って回り込ませる。
            float avoidR = radius * 9f;       // 検出半径拡張 (6r → 9r)
            float weightMul = 6f;              // sideways 重み強化 (3 → 6)
            float biasX = 0f;
            float biasZ = 0f;
            int blockingCount = 0;             // 前方ブロッカー数 (デッドロック検出)
            Vector3 myPos = transform.position;

            int ownerSide = _owner.ownerSide;
            foreach (var u in _owner.manager.AllUnits)
            {
                if (u == null || u == _owner || !u.IsAlive()) continue;
                // 同 side (味方) のみ回避する。敵は head-on で接敵させる。
                if (u.ownerSide != ownerSide) continue;
                var m = u.mover;
                if (m == null || !m._attached) continue;

                float rx = m.transform.position.x - myPos.x;
                float rz = m.transform.position.z - myPos.z;
                float dval = Mathf.Sqrt(rx * rx + rz * rz);
                if (dval < 1e-6f || dval > avoidR) continue;

                // 前方判定: 進行方向との内積が正のときのみ avoid
                float forward = rx * desiredDir.x + rz * desiredDir.z;
                if (forward <= 0f) continue;
                blockingCount++;

                AccumulateAvoidanceBias(desiredDir, rx, rz, dval, forward, avoidR, weightMul,
                    u.mover.GetInstanceID(), myPos, ref biasX, ref biasZ);
            }

            // バリケード回避 (Active barricade を obstacle として扱う、closest point on wall で位置算出)
            if (_owner.manager.barricades != null)
            {
                foreach (var b in _owner.manager.barricades)
                {
                    if (b == null || !b.IsActive) continue;
                    Vector3 closestOnWall = b.ClosestPointOnWall(myPos);
                    float rx = closestOnWall.x - myPos.x;
                    float rz = closestOnWall.z - myPos.z;
                    float dval = Mathf.Sqrt(rx * rx + rz * rz);
                    if (dval < 1e-6f || dval > avoidR) continue;
                    float forward = rx * desiredDir.x + rz * desiredDir.z;
                    if (forward <= 0f) continue;
                    blockingCount++;
                    AccumulateAvoidanceBias(desiredDir, rx, rz, dval, forward, avoidR, weightMul,
                        b.GetInstanceID(), myPos, ref biasX, ref biasZ);
                }
            }

            // デッドロック対策: ブロッカー有るのに左右バイアスが相殺されてゼロに近い場合、
            // ID ベースで決まった preferred side に強制的に横振り
            if (blockingCount > 0)
            {
                float biasMag = Mathf.Sqrt(biasX * biasX + biasZ * biasZ);
                if (biasMag < 0.5f)
                {
                    // 0 or 拮抗 → 強制サイドステップ
                    int preferRight = (GetInstanceID() & 1);
                    float forceTx, forceTz;
                    if (preferRight == 0)
                    {
                        forceTx = -desiredDir.z;
                        forceTz = desiredDir.x;
                    }
                    else
                    {
                        forceTx = desiredDir.z;
                        forceTz = -desiredDir.x;
                    }
                    biasX += forceTx * 1.5f;
                    biasZ += forceTz * 1.5f;
                }
            }

            float newX = desiredDir.x + biasX;
            float newZ = desiredDir.z + biasZ;
            float len = Mathf.Sqrt(newX * newX + newZ * newZ);
            if (len < 1e-9f) return desiredDir;
            return new Vector3(newX / len, 0f, newZ / len);
        }

        /// <summary>obstacle (rx, rz, dval) に対して左右どちらに sidestep するか決め、bias を加算。
        /// field 端で open space が無い側は反対へ自動 flip (デッドエンド回避)。</summary>
        private void AccumulateAvoidanceBias(
            Vector3 desiredDir, float rx, float rz, float dval, float forward,
            float avoidR, float weightMul, int obstacleId, Vector3 myPos,
            ref float biasX, ref float biasZ)
        {
            // cross product で左右決定
            float cross = rx * desiredDir.z - rz * desiredDir.x;
            if (Mathf.Abs(cross) < 0.05f * dval)
            {
                int tie = (GetInstanceID() - obstacleId) & 1;
                cross = (tie == 0) ? -1f : 1f;
            }

            // 左 tangent / 右 tangent
            Vector3 tangentLeft = new Vector3(-desiredDir.z, 0f, desiredDir.x);
            Vector3 tangentRight = new Vector3(desiredDir.z, 0f, -desiredDir.x);
            Vector3 chosen = cross > 0f ? tangentLeft : tangentRight;

            // フィールド端 open space チェック: 1.5m 進んだ位置が field 内かどうか
            // 選んだ側が field 外 → 反対側に flip
            const float probeDist = 1.5f;
            if (_grid != null)
            {
                Vector3 probe = myPos + chosen * probeDist;
                Vector3 clamped = _grid.ClampToField(probe);
                bool chosenBlocked = (Mathf.Abs(probe.x - clamped.x) > 0.1f || Mathf.Abs(probe.z - clamped.z) > 0.1f);
                if (chosenBlocked)
                {
                    Vector3 alt = (chosen == tangentLeft) ? tangentRight : tangentLeft;
                    Vector3 altProbe = myPos + alt * probeDist;
                    Vector3 altClamped = _grid.ClampToField(altProbe);
                    bool altBlocked = (Mathf.Abs(altProbe.x - altClamped.x) > 0.1f || Mathf.Abs(altProbe.z - altClamped.z) > 0.1f);
                    if (!altBlocked) chosen = alt; // 反対側が空いてるなら flip
                    // 両側ブロックされてる場合は元のまま (stuck escape に任せる)
                }
            }

            float closeW = 1f - dval / avoidR;
            float forwardW = forward / Mathf.Max(0.01f, dval);
            float weight = closeW * forwardW * weightMul;
            biasX += chosen.x * weight;
            biasZ += chosen.z * weight;
        }

        // ======================== velocity clamp ========================

        /// <summary>既に接触/ほぼ接触している他ユニットへ向かう velocity 成分を削る。
        /// Python _clamp_velocity_against_units の 1:1 移植 (XZ 平面)。
        /// パラメータ: contact_eps=0.02</summary>
        private Vector3 ClampVelocityAgainstUnits(Vector3 vel, float dt)
        {
            if (_owner == null || _owner.manager == null)
                return vel;

            const float contactEps = 0.02f;
            float px = transform.position.x;
            float pz = transform.position.z;

            foreach (var u in _owner.manager.AllUnits)
            {
                if (u == null || u == _owner || !u.IsAlive()) continue;
                var m = u.mover;
                if (m == null || !m._attached) continue;

                float ox = m.transform.position.x;
                float oz = m.transform.position.z;
                float rx = ox - px;
                float rz = oz - pz;
                float dval = Mathf.Sqrt(rx * rx + rz * rz);
                float minD = radius + m.radius + contactEps;

                if (dval >= minD) continue;
                if (dval < 1e-6f) continue; // 完全一致は push-out に任せる

                // 他ユニット方向の単位ベクトル
                float ux = rx / dval;
                float uz = rz / dval;

                // velocity の他ユニット方向成分 (正なら「向かっている」)
                float vInto = vel.x * ux + vel.z * uz;
                if (vInto > 0f)
                {
                    // この成分を削除 (= tangent 方向だけ残す)
                    vel.x -= vInto * ux;
                    vel.z -= vInto * uz;
                }
            }

            return vel;
        }

        /// <summary>Barricade に向かう velocity 成分を削る。
        /// Python _clamp_velocity_against_barricades の 1:1 移植 (XZ 平面)。</summary>
        private Vector3 ClampVelocityAgainstBarricades(Vector3 vel)
        {
            if (_owner == null || _owner.manager == null) return vel;
            var bars = _owner.manager.barricades;
            if (bars == null || bars.Count == 0) return vel;

            const float contactEps = 0.02f;
            Vector3 myPos = transform.position;

            foreach (var b in bars)
            {
                if (b == null || !b.IsActive) continue;
                Vector3 nearest = b.ClosestPointOnWall(myPos);
                float dx = myPos.x - nearest.x;
                float dz = myPos.z - nearest.z;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                float minD = radius + contactEps;
                if (dist >= minD) continue;
                if (dist < 1e-6f) continue; // 完全一致は push-out に任せる

                float nx = dx / dist;
                float nz = dz / dist;
                float vInto = -(vel.x * nx + vel.z * nz); // 壁方向の成分 (正なら壁に向かっている)
                if (vInto > 0f)
                {
                    vel.x += vInto * nx;
                    vel.z += vInto * nz;
                }
            }
            return vel;
        }

        // ======================== 既定 Update は使わない ========================

        private void Update()
        {
            // 意図的に空。移動は StepBattle(dt, currentTime) で manager から呼ぶ。
        }
    }
}
