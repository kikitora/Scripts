using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.Pathfinding
{
    /// <summary>静的障害物 (バリケード) を抽象化する interface。
    /// Phase 1 では EmptyBarricadeMap (障害物無し) のみ。将来 Tilemap or BoxCollider 群から
    /// 構築する GridBarricadeMap などを差し替えで導入する。</summary>
    public interface IBarricadeMap
    {
        bool IsBlocked(Vector3 world);
        bool LineOfSight(Vector3 a, Vector3 b, float step = 0.25f);
        List<Vector3> FindPath(Vector3 start, Vector3 goal, float resolution = 0.5f, int maxIter = 600);
        bool IsPresent { get; }
    }

    public class EmptyBarricadeMap : IBarricadeMap
    {
        public bool IsBlocked(Vector3 world) => false;
        public bool LineOfSight(Vector3 a, Vector3 b, float step = 0.25f) => true;
        public List<Vector3> FindPath(Vector3 start, Vector3 goal, float resolution = 0.5f, int maxIter = 600)
            => new List<Vector3> { goal };
        public bool IsPresent => false;
    }
}
