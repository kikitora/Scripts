using System;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// ターゲット優先度リストのエントリ。「条件 → 誰を狙うか」の1行。
    /// 再評価タイミング (1秒毎・ターゲット死亡時・挑発解除時) で上から評価し、
    /// 最初にマッチした entry = dominant となる。
    /// dominant が前回と違う場合のみ、その entry の side/select/filter で再選択。
    /// dominant が同じなら現在のターゲットを維持 (振動防止)。
    /// </summary>
    [Serializable]
    public class RealtimeTargetEntry
    {
        [Tooltip("条件: マッチしたらこの entry が候補")]
        public RealtimeCondition condition = RealtimeCondition.Always;

        [Tooltip("条件用スキル index")]
        public int conditionSkillIndex = 0;

        [Tooltip("条件用数値パラメータ (HP% 等)")]
        public float conditionParam = 0f;

        [Header("ターゲット選択")]
        public RealtimeTargetSide targetSide = RealtimeTargetSide.Enemy;
        public RealtimeTargetSelect targetSelect = RealtimeTargetSelect.Nearest;
        [Tooltip("ジョブ絞り込み (空=全職)")]
        public BodyJobDefinition[] jobFilter;

        [Tooltip("備考 (デバッグ用)")]
        public string label = "";
    }
}
