using UnityEngine;

namespace SteraCube.SpaceJourney
{
    using UnityEngine;
    /// <summary>
    /// キューブアクション実行用の最小コンテキスト
    /// </summary>
    public struct CubeActionContext
    {
        public string actorCubeId;
        public Vector2Int actorPos;
        public int actorFacingIndex;

        public string targetCubeId;
        public Vector2Int targetPos;

        public CubeReactionType reactionType;
    }

    /// <summary>
    /// キューブが起こす反応内容（最小）
    /// </summary>
    public abstract class CubeActionSO : ScriptableObject
    {
        [SerializeField] private string uiLabel = "Action";
        [SerializeField] private bool consumesOnUse = false;

        public string UiLabel => uiLabel;
        public bool ConsumesOnUse => consumesOnUse;

        public abstract void Execute(CubeActionContext ctx);
    }
}