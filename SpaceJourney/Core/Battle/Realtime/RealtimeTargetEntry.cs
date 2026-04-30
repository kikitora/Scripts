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

        [Tooltip("-1=フィルタなし。0以上 なら skills[index] の射程内に居る敵だけを候補にする " +
                 "(Archer の「弓射程内の敵を優先」用)。射程内に誰も居なければこの entry は候補なし扱いで次優先度へフォールスルー。")]
        public int rangeFilterSkillIndex = -1;

        [Tooltip("0 以下=フィルタなし。正値ならその距離 (m) 以内の候補のみに絞り込む " +
                 "(NoLongRangeAttack 時の近接2m切替等)。rangeFilterSkillIndex とは独立に併用可。")]
        public float rangeFilterMaxDist = 0f;

        [Tooltip("true の時はグリッド上で経路が確保できる候補のみを対象とする (味方や壁に塞がれて到達不能な対象は除外)。false ならルート無くても選び、迂回路を探す。")]
        public bool requireRoute = false;

        [Tooltip("備考 (デバッグ用)")]
        public string label = "";
    }
}
