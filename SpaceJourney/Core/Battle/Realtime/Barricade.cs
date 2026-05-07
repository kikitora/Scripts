using UnityEngine;
using SteraCube.SpaceJourney.Realtime.Pathfinding;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>Knight rank 5 Barricade。3m × 0.4m × 1m の壁。
    /// Python tools/battle_sim/barricade.py の 1:1 移植。
    /// 仮プレファブ: Unity primitive Cube を動的生成 (見た目調整は今後)。
    /// XZ 平面で動作 (Y は固定 = caster の Y)。</summary>
    public class Barricade : MonoBehaviour
    {
        public const float LENGTH = 3.0f;       // 壁の長さ (横方向)
        public const float THICKNESS = 0.4f;    // 壁の厚さ (前後方向)
        public const float HEIGHT = 1.5f;       // 視覚高さ
        public const int DEFAULT_HP = 4;
        public const float DEFAULT_LIFETIME = 5.0f;

        public RealtimeBattleUnit owner;
        public Vector3 center;       // ワールド座標
        public Vector3 facing;       // caster の向き (XZ 平面)
        public float spawnTime;
        public float expireTime;
        public int hp;
        public int maxHp;
        public bool destroyed = false;

        public int Side => owner != null ? owner.ownerSide : -1;
        public bool IsActive => hp > 0 && !destroyed;
        public bool IsAlive => IsActive;
        public Vector3 Position => center;

        /// <summary>残り寿命 (秒)</summary>
        public float RemainingSec(float currentTime) => Mathf.Max(0f, expireTime - currentTime);

        public bool IsExpired(float currentTime) => currentTime >= expireTime || !IsActive;

        public static Barricade Spawn(
            RealtimeBattleUnit caster, Vector3 center, Vector3 facing,
            float currentTime, int hp = DEFAULT_HP, float lifetime = DEFAULT_LIFETIME)
        {
            // ビジュアルは Resources/Barricade/Barricade_Visual.prefab があればそれを使う。
            // prefab 製作者は scale=(LENGTH, HEIGHT, THICKNESS)=(3, 1.5, 0.4) で見栄え良くなるよう設計する。
            // prefab が無ければ primitive Cube + 砂岩色 Material のフォールバック表示。
            GameObject go;
            var visualPrefab = Resources.Load<GameObject>("Barricade/Barricade_Visual");
            if (visualPrefab != null)
            {
                go = Object.Instantiate(visualPrefab);
                go.name = $"Barricade_{caster.displayName}";
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Barricade_{caster.displayName}";
                var rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    var mat = new Material(Shader.Find("Standard"));
                    mat.color = (caster != null && caster.ownerSide == 0)
                        ? new Color(0.7f, 0.6f, 0.5f)
                        : new Color(0.4f, 0.3f, 0.3f);
                    rend.material = mat;
                }
            }

            // 親を caster と同じ親 (シーン整理)
            if (caster != null && caster.transform.parent != null)
                go.transform.SetParent(caster.transform.parent, true);
            // 位置: caster の y に合わせる、壁の中心が地面から HEIGHT/2 に
            float baseY = caster != null ? caster.transform.position.y : 0f;
            go.transform.position = new Vector3(center.x, baseY + HEIGHT * 0.5f, center.z);
            // 回転: facing 方向 = +Z (Cube/visualPrefab の前面)
            Vector3 facingFlat = new Vector3(facing.x, 0f, facing.z).normalized;
            if (facingFlat.sqrMagnitude < 0.001f) facingFlat = Vector3.forward;
            go.transform.rotation = Quaternion.LookRotation(facingFlat, Vector3.up);
            // スケール:
            // - visualPrefab 経由: prefab 自身の scale を尊重 (prefab 制作者は (LENGTH, HEIGHT, THICKNESS)
            //   で見栄え良くなるよう作る前提だが、別 scale で作りたい場合も上書きしない)
            // - primitive Cube fallback: 単位 1m なので (LENGTH, HEIGHT, THICKNESS) でストレッチ
            if (visualPrefab == null)
                go.transform.localScale = new Vector3(LENGTH, HEIGHT, THICKNESS);
            // 物理コライダ不要 (mover が独自に避ける) — 自身と全子の Collider を除去
            foreach (var c in go.GetComponentsInChildren<Collider>(true))
                Object.Destroy(c);

            var b = go.AddComponent<Barricade>();
            b.owner = caster;
            b.center = new Vector3(center.x, baseY, center.z);
            b.facing = facingFlat;
            b.spawnTime = currentTime;
            b.expireTime = currentTime + lifetime;
            b.hp = hp;
            b.maxHp = hp;
            return b;
        }

        /// <summary>Active でない壁を破棄 (manager から呼ばれる)</summary>
        public void DestroySelf()
        {
            destroyed = true;
            if (gameObject != null) Object.Destroy(gameObject);
        }

        /// <summary>攻撃 1 回 = 1 ダメ固定。HP 0 で破壊。戻り値: 与えたダメ。</summary>
        public int ApplyFixedHit()
        {
            if (!IsActive) return 0;
            hp -= 1;
            if (hp <= 0)
            {
                hp = 0;
                destroyed = true;
            }
            return 1;
        }

        // ============= 衝突判定 ===============

        /// <summary>点 p に最も近い壁上の点を返す。XZ 平面のみ。</summary>
        public Vector3 ClosestPointOnWall(Vector3 p)
        {
            // 壁ローカル軸: along = facing 垂直 (壁の長さ方向)、across = facing (厚さ方向)
            Vector3 along = new Vector3(-facing.z, 0f, facing.x);
            Vector3 rel = new Vector3(p.x - center.x, 0f, p.z - center.z);
            float a = rel.x * along.x + rel.z * along.z;
            float c = rel.x * facing.x + rel.z * facing.z;
            float aClamped = Mathf.Clamp(a, -LENGTH * 0.5f, LENGTH * 0.5f);
            float cClamped = Mathf.Clamp(c, -THICKNESS * 0.5f, THICKNESS * 0.5f);
            return new Vector3(
                center.x + aClamped * along.x + cClamped * facing.x,
                p.y,
                center.z + aClamped * along.z + cClamped * facing.z
            );
        }

        /// <summary>点 p から壁までの距離 (壁内なら 0)。XZ 平面のみ。</summary>
        public float DistanceTo(Vector3 p)
        {
            Vector3 nearest = ClosestPointOnWall(p);
            float dx = p.x - nearest.x;
            float dz = p.z - nearest.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>円 (中心 p, 半径 radius) と壁の重なり判定 (XZ)。</summary>
        public bool OverlapsCircle(Vector3 p, float radius) => DistanceTo(p) < radius;

        /// <summary>点 p が「壁の footprint + 前方 1m 以内」のノックバック範囲内に居るか。
        /// 居る場合 (true, push_dist)。push_dist は壁の遠側 + radius + 0.05 までの距離。
        /// 「前面」= caster と反対側 (= facing 方向側、along_facing > 0)。
        /// 範囲外 (壁から遠い、壁の後ろ等) の敵は影響を受けない。</summary>
        public bool TryGetPushCorridor(Vector3 p, Vector3 ownerPos, float radius, out float pushDist)
        {
            pushDist = 0f;
            Vector3 along = new Vector3(-facing.z, 0f, facing.x);
            Vector3 rel = new Vector3(p.x - center.x, 0f, p.z - center.z);
            float lateral = rel.x * along.x + rel.z * along.z;
            float alongFacing = rel.x * facing.x + rel.z * facing.z;

            // 横軸: 壁の長さ範囲内 (3m 幅) + radius
            if (Mathf.Abs(lateral) > LENGTH * 0.5f + radius) return false;

            // 縦軸: 壁の back face (-THICKNESS/2 - radius) 〜 壁の前方 1m + radius
            // (壁の後ろの敵は対象外、前方 1 マスを超える敵も対象外)
            const float pushBufferForward = 1.0f;
            float backLimit = -THICKNESS * 0.5f - radius;
            float frontLimit = THICKNESS * 0.5f + radius + pushBufferForward;
            if (alongFacing < backLimit || alongFacing > frontLimit) return false;

            // 押し先: 壁の遠い側 + radius + 0.05
            float targetAlong = THICKNESS * 0.5f + radius + 0.05f;
            float dist = targetAlong - alongFacing;
            if (dist <= 0f) return false;
            pushDist = dist;
            return true;
        }

        // ============= 配置検証 ===============

        public enum PlacementResult { Ok, FieldBound, OverlapBarricade, AllyInPlace, EnemyPushOutOfBound, EnemyPushCollide }

        /// <summary>設置可否チェック。Python Barricade.can_place の 1:1 移植。</summary>
        public static PlacementResult CanPlace(
            Vector3 center, Vector3 facing, RealtimeBattleUnit owner,
            RealtimeBattleManager mgr, float ownerRadius = 0.36f)
        {
            Vector3 facingFlat = new Vector3(facing.x, 0f, facing.z).normalized;
            if (facingFlat.sqrMagnitude < 0.001f) facingFlat = Vector3.forward;

            // フィールド境界 (4 隅)
            var grid = SimpleGrid.Active;
            if (grid != null)
            {
                Vector3 along = new Vector3(-facingFlat.z, 0f, facingFlat.x);
                float l = LENGTH * 0.5f;
                float t = THICKNESS * 0.5f;
                float[] sas = { -l, l };
                float[] sts = { -t, t };
                foreach (float sa in sas)
                {
                    foreach (float st in sts)
                    {
                        float px = center.x + sa * along.x + st * facingFlat.x;
                        float pz = center.z + sa * along.z + st * facingFlat.z;
                        Vector3 corner = new Vector3(px, 0f, pz);
                        Vector3 clamped = grid.ClampToField(corner);
                        if (Mathf.Abs(clamped.x - px) > 0.01f || Mathf.Abs(clamped.z - pz) > 0.01f)
                            return PlacementResult.FieldBound;
                    }
                }
            }

            // 既存 Barricade との重なり
            if (mgr != null && mgr.barricades != null)
            {
                foreach (var b in mgr.barricades)
                {
                    if (b == null || !b.IsActive) continue;
                    float cx = b.center.x - center.x;
                    float cz = b.center.z - center.z;
                    if (Mathf.Sqrt(cx * cx + cz * cz) < LENGTH)
                    {
                        if (b.DistanceTo(center) < THICKNESS)
                            return PlacementResult.OverlapBarricade;
                    }
                }
            }

            // 仮の Barricade を作って ally/enemy チェック (実体生成せず計算のみ)
            // TryGetPushCorridor / OverlapsCircle 用に center/facing だけセット
            var tmp = new TempCheck { center = center, facing = facingFlat };

            Vector3 ownerPos = owner != null ? owner.transform.position : center;

            if (mgr != null)
            {
                foreach (var u in mgr.AllUnits)
                {
                    if (u == null || !u.IsAlive()) continue;
                    if (u == owner) continue;
                    Vector3 up = u.transform.position;
                    bool inCorridor = tmp.TryGetPushCorridor(up, ownerPos, ownerRadius, out float pushDist);
                    if (u.ownerSide == owner.ownerSide)
                    {
                        if (tmp.OverlapsCircle(up, ownerRadius))
                            return PlacementResult.AllyInPlace;
                        continue;
                    }
                    if (!inCorridor) continue;
                    Vector3 newPos = up + facingFlat * pushDist;
                    if (grid != null)
                    {
                        Vector3 cl = grid.ClampToField(newPos);
                        if (Mathf.Abs(cl.x - newPos.x) > 0.01f || Mathf.Abs(cl.z - newPos.z) > 0.01f)
                            return PlacementResult.EnemyPushOutOfBound;
                    }
                    foreach (var other in mgr.AllUnits)
                    {
                        if (other == null || other == u || !other.IsAlive()) continue;
                        Vector3 op = other.transform.position;
                        float dx = newPos.x - op.x;
                        float dz = newPos.z - op.z;
                        float od = Mathf.Sqrt(dx * dx + dz * dz);
                        float otherR = other.mover != null ? other.mover.radius : 0.36f;
                        if (od < (ownerRadius + otherR) - 0.01f)
                            return PlacementResult.EnemyPushCollide;
                    }
                }
            }
            return PlacementResult.Ok;
        }

        /// <summary>設置直後、corridor 内の敵を facing 方向に押し出す。</summary>
        public static void PushEnemiesAside(RealtimeBattleManager mgr, Barricade barricade, float ownerRadius = 0.36f)
        {
            if (mgr == null || barricade == null || barricade.owner == null) return;
            Vector3 pushDir = barricade.facing;
            Vector3 ownerPos = barricade.owner.transform.position;
            foreach (var u in mgr.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                if (u == barricade.owner) continue;
                if (u.ownerSide == barricade.owner.ownerSide) continue;
                Vector3 up = u.transform.position;
                if (!barricade.TryGetPushCorridor(up, ownerPos, ownerRadius, out float pushDist)) continue;
                Vector3 newPos = up + pushDir * pushDist;
                u.transform.position = new Vector3(newPos.x, up.y, newPos.z);
            }
        }

        /// <summary>can_place 内の corridor 計算用ヘルパ (実 Barricade 生成しない)</summary>
        private struct TempCheck
        {
            public Vector3 center;
            public Vector3 facing;

            public bool OverlapsCircle(Vector3 p, float radius)
            {
                Vector3 along = new Vector3(-facing.z, 0f, facing.x);
                Vector3 rel = new Vector3(p.x - center.x, 0f, p.z - center.z);
                float a = rel.x * along.x + rel.z * along.z;
                float c = rel.x * facing.x + rel.z * facing.z;
                float aClamped = Mathf.Clamp(a, -LENGTH * 0.5f, LENGTH * 0.5f);
                float cClamped = Mathf.Clamp(c, -THICKNESS * 0.5f, THICKNESS * 0.5f);
                Vector3 nearest = new Vector3(
                    center.x + aClamped * along.x + cClamped * facing.x,
                    0f,
                    center.z + aClamped * along.z + cClamped * facing.z);
                float dx = p.x - nearest.x;
                float dz = p.z - nearest.z;
                return Mathf.Sqrt(dx * dx + dz * dz) < radius;
            }

            public bool TryGetPushCorridor(Vector3 p, Vector3 ownerPos, float radius, out float pushDist)
            {
                pushDist = 0f;
                Vector3 along = new Vector3(-facing.z, 0f, facing.x);
                Vector3 rel = new Vector3(p.x - center.x, 0f, p.z - center.z);
                float lateral = rel.x * along.x + rel.z * along.z;
                float alongFacing = rel.x * facing.x + rel.z * facing.z;
                float ox = ownerPos.x - center.x;
                float oz = ownerPos.z - center.z;
                float ownerAlongFacing = ox * facing.x + oz * facing.z;
                if (Mathf.Abs(lateral) > LENGTH * 0.5f + radius) return false;
                float frontLimit = THICKNESS * 0.5f + radius;
                float backLimit = ownerAlongFacing - radius;
                if (alongFacing < backLimit || alongFacing > frontLimit) return false;
                float targetAlong = THICKNESS * 0.5f + radius + 0.05f;
                float dist = targetAlong - alongFacing;
                if (dist <= 0f) return false;
                pushDist = dist;
                return true;
            }
        }
    }
}
