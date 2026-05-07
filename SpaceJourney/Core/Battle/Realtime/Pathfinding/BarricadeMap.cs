using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.Pathfinding
{
    /// <summary>静的バリケード (Knight 壁スキル等) の Line-of-Sight を提供する interface。
    /// 設計方針: 迂回しない。 LineOfSight は「ターゲットリスト requireRoute フィルタ」用途のみ。
    /// 経路探索 (FindPath) は廃止。物理衝突は ContinuousMover の velocity clamp + push-out が担う。</summary>
    public interface IBarricadeMap
    {
        bool LineOfSight(Vector3 a, Vector3 b, float step = 0.25f);
        bool IsPresent { get; }
    }

    public class EmptyBarricadeMap : IBarricadeMap
    {
        public bool LineOfSight(Vector3 a, Vector3 b, float step = 0.25f) => true;
        public bool IsPresent => false;
    }

    /// <summary>RealtimeBattleManager の barricades リストを直接見て LineOfSight を判定する実装。
    /// グリッド化や A* は不要 (迂回しない設計)、 各 barricade の OverlapsCircle で線上の障害だけ確認する。</summary>
    public class BarricadeListMap : IBarricadeMap
    {
        private readonly RealtimeBattleManager _mgr;
        public BarricadeListMap(RealtimeBattleManager mgr) { _mgr = mgr; }

        public bool IsPresent => _mgr != null && _mgr.barricades != null && _mgr.barricades.Count > 0;

        /// <summary>線分 a→b 上に Active バリケードが乗っていれば false。
        /// step 間隔でサンプリングして OverlapsCircle で当たり判定。</summary>
        public bool LineOfSight(Vector3 a, Vector3 b, float step = 0.25f)
        {
            if (!IsPresent) return true;
            Vector3 d = b - a; d.y = 0f;
            float dist = d.magnitude;
            if (dist < 1e-6f) return true;
            int n = Mathf.Max(1, Mathf.CeilToInt(dist / step));
            Vector3 inv = d / n;
            const float sampleR = 0.05f;
            Vector3 p = a;
            for (int i = 1; i <= n; i++)
            {
                p += inv;
                foreach (var bar in _mgr.barricades)
                {
                    if (bar == null || !bar.IsActive) continue;
                    if (bar.OverlapsCircle(p, sampleR)) return false;
                }
            }
            return true;
        }
    }
}
