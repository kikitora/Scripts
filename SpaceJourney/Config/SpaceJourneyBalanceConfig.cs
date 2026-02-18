using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// SpaceJourneyモード全体のゲームバランスを調整するための ScriptableObject。
    /// 難易度やプラグインによって差し替えることを想定しており、
    /// 「ゲームがどれくらい楽か／きついか」に直結するパラメータを集中的に管理する。
    /// </summary>
    [CreateAssetMenu(
        fileName = "SpaceJourneyBalanceConfig",
        menuName = "SteraCube/SpaceJourney/BalanceConfig")]
    public class SpaceJourneyBalanceConfig : ScriptableObject
    {
        // ===== 基本パラメータ =====

        [Header("基本パラメータ")]

        [Tooltip("先制側がタイムライン上でリードするタイム数。0にすると同時スタート。")]
        public int firstStrikeLeadTicks = 3;

        [Tooltip("このモードのレベル上限。成長カーブの Lmax として使用される。")]
        public int maxLevel = 25;

        // ===== 士気とステータス倍率 =====

        [Header("士気とステータス倍率")]

        [Tooltip("士気倍率計算で使用する指数。(morale/100)^moralePower の形で使用される。")]
        public float moralePower = 1.3f;

        // ===== 士気減少・回復 =====

        [Header("士気：戦闘終了時の減少パラメータ（ノーマル基準）")]

        [Tooltip("ダメージ割合(dmgRatio)による士気減少の最大値。")]
        public float damageLossMax = 30f;

        [Tooltip("戦闘中に死亡したキャラのコスト合計1あたりの士気減少量。")]
        public float baseMoraleLossPerCostOnDeath = 2f;

        [Tooltip("その戦闘中に一度でもサイド全滅した場合の追加士気減少量。")]
        public float sideWipeMoraleLoss = 35f;

        [Tooltip("「その戦闘に参加しただけ」で発生する固定士気減少量。")]
        public float participationLoss = 8f;

        [Header("士気：自然回復（フィールドターンごと）")]

        [Tooltip("各フィールドターン終了時に全サイドへ加算される士気回復量（ノーマル基準）。")]
        public float moraleRecoveryPerTurn = 3f;

        // ===== コアHPダメージ係数 =====

        [Header("コアHPダメージ係数（ノーマル基準）")]

        [Tooltip("キャラ死亡時に、死亡キャラのコスト1あたり何ポイントのコアHPを失うか。")]
        public float baseCoreDamagePerCost = 2f;

        [Tooltip("サイド全滅時に、サイドの総コスト1あたり何ポイントのコアHPを失うか。")]
        public float baseSideWipeCoreDamagePerCost = 2f;

        // ===== 敵EXP関連 =====

        [Header("敵EXP（ランク別・敵種別の係数）")]

        [Tooltip("Rank1 ノーマル敵1体が持つ基礎EXP（Exp0）。")]
        public int baseEnemyExpRank1 = 20;

        [Tooltip("ランクが1上がるごとのEXP倍率。3.0なら Rank+1 で約3倍。")]
        public float enemyRankExpFactor = 3.0f;

        [Tooltip("エリート敵のEXP倍率（同ランクノーマル敵に対して何倍か）。")]
        public float eliteExpMultiplier = 2.0f;

        [Tooltip("門番キューブのEXP倍率（同ランクノーマル敵に対して何倍か）。")]
        public float gatekeeperExpMultiplier = 3.0f;

        [Tooltip("ボスのEXP倍率（同ランクノーマル敵に対して何倍か）。")]
        public float bossExpMultiplier = 5.0f;

        // ===== レベルアップ必要EXPカーブ =====

        [Header("レベルアップ必要EXPカーブ")]

        [Tooltip("Lv1→2 に必要なEXPの基礎値。以降のレベルは成長係数に従って増えていく。")]
        public int baseLevelUpExp = 100;

        [Tooltip("Lv1〜22 で使用する指数的な増加係数。おおよそ 1.4〜1.5 を想定。")]
        public float levelUpExpFactor = 1.45f;

        [Tooltip("Lv23 必要EXPを Lv22 基準に対してどれだけ重くするかの倍率。")]
        public float level23BonusMultiplier = 2.0f;

        [Tooltip("Lv24 必要EXPを Lv22 基準に対してどれだけ重くするかの倍率。")]
        public float level24BonusMultiplier = 4.0f;

        [Tooltip("Lv25 必要EXPを Lv22 基準に対してどれだけ重くするかの倍率。")]
        public float level25BonusMultiplier = 8.0f;

        // 今後：タワー攻撃の威力・CT、状態異常コアの効果量なども、
        // 難易度やプラグインで変えたくなったらここへ追加していく。
    }
}
