using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.Pathfinding
{
    /// <summary>
    /// 連続座標 Mover。
    ///
    /// 設計方針: **destination へ直行**。同 side 味方も非ターゲット敵もバリケードも
    /// path obstacle 化しない (= 迂回しない)。物理的衝突は velocity clamp + push-out で
    /// 防ぎ、詰まったらユーザがターゲット/アクションリストで挙動を決める。
    /// 並走 stuck だけ崩すための極めて軽い tangent avoid steer (currentTarget 除外、
    /// 接触距離ぎりぎりのみ) を持つ。
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

        /// <summary>直前 StepBattle で実際に動いた速度 (m/s)。
        /// velocity (clamp 前後の意図ベクトル) ではなく **押し出し後の真の displacement / dt**。
        /// 押し合いで velocity が tangent に小さく残っても EffectiveSpeed は実際の動きを反映する。</summary>
        public float EffectiveSpeed { get; private set; }

        /// <summary>直前 StepBattle で destination 方向にどれだけ近づいたか (m/s)。
        /// 横滑りで EffectiveSpeed > 0 でも、 destination に向かってない場合は 0 近くになる。
        /// walk / idle anim 切替や「stuck → 攻撃許可」判定に使う。</summary>
        public float EffectiveProgressSpeed { get; private set; }

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
        private SteraCube.SpaceJourney.Realtime.RealtimeBattleUnit _owner;
        private Vector3 _prevStepStartPos;  // 直前 StepBattle 開始時 position (Effective*Speed 算出用)
        private bool _hasPrevStepStartPos = false;

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
            _hasPrevStepStartPos = false;
            EffectiveSpeed = 0f;
            EffectiveProgressSpeed = 0f;
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
            _hasPrevStepStartPos = false;  // テレポート後の偽 EffectiveSpeed を抑止
            EffectiveSpeed = 0f;
            EffectiveProgressSpeed = 0f;
        }

        public void StopMovement()
        {
            simulateMovement = false;
            velocity = Vector3.zero;
            reachedDestination = false;
            EffectiveSpeed = 0f;
            EffectiveProgressSpeed = 0f;
        }

        /// <summary>互換のため残置 (旧 GridBarricadeMap A* 経路用、現在は no-op)。</summary>
        public void SetBarricadeMap(IBarricadeMap bm) { /* no-op: A* 撤廃済 */ }

        /// <summary>互換のため残置 (旧 path キャッシュ無効化、現在は no-op)。</summary>
        public void InvalidatePath() { /* no-op: A* 撤廃済 */ }

        // ======================== メイン更新 ========================

        /// <summary>
        /// 1 フレーム分の移動計算。RealtimeBattleManager._MovementPhase から呼ばれる。
        /// MonoBehaviour.Update() は使わない (manager 主導で順序制御)。
        /// </summary>
        public void StepBattle(float dt, float currentTime)
        {
            // 前フレ StepBattle 開始 → 今フレ StepBattle 開始の差分が
            // 「前フレで実際にどれだけ動けたか」(push-out 込み)。
            Vector3 thisFrameStart = transform.position;
            if (_hasPrevStepStartPos && dt > 1e-6f)
            {
                Vector3 displacement = thisFrameStart - _prevStepStartPos;
                displacement.y = 0f;
                EffectiveSpeed = displacement.magnitude / dt;
                // destination 方向への成分: 横滑りは 0 近く、 直進は EffectiveSpeed と同等。
                Vector3 toDest = destination - _prevStepStartPos;
                toDest.y = 0f;
                if (toDest.sqrMagnitude > 0.01f)
                    EffectiveProgressSpeed = Vector3.Dot(displacement, toDest.normalized) / dt;
                else
                    EffectiveProgressSpeed = EffectiveSpeed;  // destination 至近
            }
            else
            {
                EffectiveSpeed = 0f;
                EffectiveProgressSpeed = 0f;
            }
            _prevStepStartPos = thisFrameStart;
            _hasPrevStepStartPos = true;

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

            // 速度: 直線方向 + 軽量 unit avoid bias
            Vector3 dir = dist > 1e-4f ? to / dist : Vector3.zero;
            dir = ApplyUnitAvoidanceSteer(dir);

            Vector3 v = dir * maxSpeed;

            // 接触している他ユニットへ向かう velocity 成分を削る
            v = ClampVelocityAgainstUnits(v, dt);
            // Barricade に向かう velocity 成分を削る (壁通過防止)
            v = ClampVelocityAgainstBarricades(v);

            velocity = v;

            // 候補位置 (y 軸は固定)
            Vector3 prevPos = transform.position;
            Vector3 cand = prevPos + v * dt;
            if (_grid != null) cand = _grid.ClampToField(cand);
            cand.y = prevPos.y;
            cand = StopBeforeUnitOverlap(prevPos, cand, dt, ref v);

            transform.position = cand;
        }

        // ======================== 軽量 unit avoid steer ========================

        /// <summary>味方/敵問わずユニット間の avoid steer は無効化。
        /// destination へ素直に直進。 押し合い stuck は velocity clamp で停止 + idle anim、
        /// 解決はターゲットリスト/アクションリストで行う設計。</summary>
        private Vector3 ApplyUnitAvoidanceSteer(Vector3 desiredDir)
        {
            return desiredDir;
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
                    // 両側ブロックされてる場合は元のまま
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

        /// <summary>
        /// 通常移動では他ユニットを押さない。候補位置が相手半径内へ入るなら、
        /// 移動する側だけを接触直前で止める。既存の push-out は初期重なり等の保険用。
        /// </summary>
        private Vector3 StopBeforeUnitOverlap(Vector3 prevPos, Vector3 cand, float dt, ref Vector3 vel)
        {
            if (_owner == null || _owner.manager == null)
                return cand;

            Vector3 move = cand - prevPos;
            move.y = 0f;
            if (move.sqrMagnitude < 1e-8f)
                return cand;

            float bestT = 1f;
            bool blocked = false;
            foreach (var u in _owner.manager.AllUnits)
            {
                if (u == null || u == _owner || !u.IsAlive()) continue;
                var m = u.mover;
                if (m == null || !m._attached) continue;

                Vector3 center = m.transform.position;
                center.y = prevPos.y;
                float minD = radius + m.radius;

                Vector3 fromCenter = prevPos - center;
                fromCenter.y = 0f;
                float startDistSq = fromCenter.sqrMagnitude;
                if (startDistSq < minD * minD)
                {
                    blocked = true;
                    bestT = 0f;
                    break;
                }

                Vector3 endFromCenter = cand - center;
                endFromCenter.y = 0f;
                if (endFromCenter.sqrMagnitude >= minD * minD) continue;

                float a = Vector3.Dot(move, move);
                float b = 2f * Vector3.Dot(fromCenter, move);
                float c = startDistSq - minD * minD;
                float disc = b * b - 4f * a * c;
                if (disc < 0f || a < 1e-8f)
                {
                    blocked = true;
                    bestT = 0f;
                    break;
                }

                float sqrt = Mathf.Sqrt(disc);
                float t = (-b - sqrt) / (2f * a);
                if (t >= 0f && t <= 1f && t < bestT)
                {
                    bestT = t;
                    blocked = true;
                }
            }

            if (!blocked) return cand;

            const float contactBackoff = 0.001f;
            float safeT = Mathf.Max(0f, bestT - contactBackoff);
            Vector3 safe = prevPos + move * safeT;
            safe.y = prevPos.y;
            vel = dt > 1e-6f ? (safe - prevPos) / dt : Vector3.zero;
            vel.y = 0f;
            if (safeT <= 0f) vel = Vector3.zero;
            return safe;
        }

        /// <summary>Barricade に向かう velocity 成分を削る (壁通過防止)。
        /// 壁の前で速度ゼロになるので、ユニットは自然に「壁の前で停止」する。
        /// 迂回はしない (steer bias 無し)。</summary>
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
