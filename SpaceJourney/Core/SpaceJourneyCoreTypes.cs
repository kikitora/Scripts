// SpaceJourneyCoreTypes.cs
// SpaceJourneyモード全体で使う「用語」「列挙型(enum)」「ゲーム内でほぼ固定の定数」をまとめたクラス群です。

using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public static class SpaceJourneyTerms
    {
        public const string TermMorale = "士気";
        public const string TermCoreHp = "コアHP";
        public const string TermLifeStock = "残基";
        public const string TermEnergy = "EN";

        public const string TermCube = "キューブ";
        public const string TermPlayerCube = "プレイヤーキューブ";
        public const string TermEnemyCube = "エネミーキューブ";
        public const string TermTreasureCube = "宝箱キューブ";
        public const string TermHealCube = "回復キューブ";
        public const string TermEventCube = "イベントキューブ";
        public const string TermCityCube = "シティキューブ";
        public const string TermGateKeeper = "門番キューブ";
        public const string TermBossCube = "ボスキューブ";
    }

    public static class SpaceJourneyConstants
    {
        public const int CubeSideGridSize = 3;

        public const float CubeRotateAnimDuration = 0.5f;
        public const float CubeDirChangeAnimDuration = 0.1f;
        public const float CubeMoveAnimDuration = 0.5f;

        public const bool EnableIdleFloat = true;
        public const float IdleAmplitude = 0.12f;
        public const float IdlePeriod = 1.4f;
        public const bool EnableIdleSway = false;
        public const float IdleSwayAngle = 3.0f;

        public const bool EnableMoveFloat = true;
        public const float MoveAmplitude = 0.12f;
        public const float MoveSwayAngle = 4.0f;

        public const float AgilityCap = 600f;
        public const float MaxCostReductionRate = 0.35f;
        public const float MinCostReductionStep = 1.0f;

        //============================================================
        // レベル / EXP 周りの基礎パラメータ
        //============================================================

        // ソウルレベルの上限（仕様書 8章）
        public const int MaxSoulLevel = 25;

        // 「通常カーブ」として指数成長させるレベル帯（Lv1〜22）
        public const int LevelUpExpNormalMaxLevel = 22;

        // Lv1→2 に必要なEXPの基準値
        public const float LevelUpBaseExp = 120f;

        // Lvごとの必要EXP増加係数（1〜22用）
        // 実効倍率は 1.4〜1.5 の中間くらいを狙って 1.45f にしている（後でBalance調整可）
        public const float LevelUpExpFactor = 1.45f;

        // Lv23〜25 の「同じ指数カーブ＋ボーナス倍率」用
        public const float LevelUpExpBonusLv23 = 2.0f;
        public const float LevelUpExpBonusLv24 = 4.0f;
        public const float LevelUpExpBonusLv25 = 8.0f;

        // ソウルジョブごとの「レベル上がりにくさ」倍率
        // つきやすい：1.0 / 就きにくい：1.3 / 極端に就きにくい：1.8
        public const float SoulJobExpMultiplierEasy = 1.0f;
        public const float SoulJobExpMultiplierHard = 1.3f;
        public const float SoulJobExpMultiplierVeryHard = 1.8f;

        // ソウルジョブランク1段階ごとの必要EXP増加率（+30%）
        public const float SoulJobRankExpPerRank = 0.3f;

        // ダメージ揺れ（±10%）
        public const float DamageRandomMinFactor = 0.90f;
        public const float DamageRandomMaxFactor = 1.10f;

        // ─────────────────────────────────────────────────────────
        // ダメージ計算：AT-DF / MAT-MDF の「差」で決まる通り率（through）
        //
        // ■式（StatMath側）
        // t = tanh( (Attack - Defense) / DamageThroughDeltaScale )   // -1..1（差が大きいと飽和）
        // through = DamageThroughEqual + 0.5 * t
        // through = clamp(through, DamageThroughMin, DamageThroughMax)
        //
        // ■各値が与える影響
        // - DamageThroughEqual：
        //     「同格（差=0）」のときの通り率。上げるほど同格同士の等倍攻撃が痛くなる。
        //     例：0.53 → 同格で“だいたい 0.53 倍”通るイメージ（＋チップ分）
        //
        // - DamageThroughDeltaScale：
        //     “差の効きやすさ”。小さいほど少しの差で通り率が大きく動く（格差が出やすい）。
        //     大きいほど差の影響はマイルド（同格寄りの殴り合いが長くなる）。
        //
        // - DamageThroughMin / DamageThroughMax：
        //     through の下限/上限。極端な硬さ/柔らかさでも破綻しないための安全柵。
        //     Min を上げると「硬くても最低限削れる」、Max を上げると「格上がより溶かす」。
        // ─────────────────────────────────────────────────────────
        public const float DamageThroughEqual = 0.53f;
        public const float DamageThroughDeltaScale = 60f;
        public const float DamageThroughMin = 0.10f;
        public const float DamageThroughMax = 1.10f;

        //============================================================
        // Soul / Reincarnation : Talent & Event (定数はここに集約)
        //============================================================

        // ■転生イベント補正（仮）
        // 本来は転生シミュレーションでイベント成功数/到達段階などから積み上がる係数。
        // いまは転生シミュ未実装の穴埋めとして一律 30%UP を入れておく（仮）。
        public const float TempInitialReincarnationEventFactor = 1.30f;

        // ■才能ランク抽選確率（％）
        // 仕様：成長タイプによる出現分布差は廃止し、一律分布で抽選する。
        // 一律：A2 / B8 / C25 / D40 / E25
        public const int TalentChance_Normal_A = 2;
        public const int TalentChance_Normal_B = 8;
        public const int TalentChance_Normal_C = 25;
        public const int TalentChance_Normal_D = 40;
        public const int TalentChance_Normal_E = 25;

        // ■才能倍率レンジ（全ステ共通で同一倍率を掛ける）
        // ※TalentRankは「格付け」で、実倍率はこのレンジから1回だけ抽選してSoulに固定する。
        public const float TalentFactorMin_A = 1.25f;
        public const float TalentFactorMax_A = 1.32f;

        public const float TalentFactorMin_B = 1.15f;
        public const float TalentFactorMax_B = 1.24f;

        // 以下は据え置き
        public const float TalentFactorMin_C = 1.10f;
        public const float TalentFactorMax_C = 1.24f;

        public const float TalentFactorMin_D = 1.00f;
        public const float TalentFactorMax_D = 1.09f;

        public const float TalentFactorMin_E = 0.90f;
        public const float TalentFactorMax_E = 0.99f;

    }


    #region enum 定義群

    public enum MapStateType
    {
        None,
        Boss,
        BossBefore,
        Heaven,
        Hell
    }

    public enum Dir
    {
        North,
        East,
        South,
        West
    }

    public enum UpperSideNum
    {
        up = 0,
        right = 1,
        dawn = 2,
        left = 3,
        flont = 4,
        back = 5
    }

    public enum StatKind
    {
        HP,
        AT,
        DF,
        AGI,
        MAT,
        MDF,
    }

    #region Soul関連enum

    // どのボディ職向きか
    public enum SoulJobTendency
    {
        Warrior, // 戦士向き
        Knight,  // 騎士向き
        Archer,  // 弓兵向き
        Mage,    // 魔術師向き
        Lancer   // 槍兵向き
    }
    public enum FaceSexCategory
    {
        Male = 0,
        Female = 1,
        Unknown = 2
    }

    public enum SoulState
    {
        None,
        InCube,
        InInventory,
        InStore,
        InStoreStock,
    }

    public enum SoulType
    {
        Normal,
        Unique
    }

    public enum TalentRank
    {
        A,
        B,
        C,
        D,
        E,
    }

    public enum GrowthType
    {
        Early,
        Normal,
        Late,
        UltraLate,
    }
    #endregion

    #region Body関連enum
    #endregion

    #region キューブ関連enum
    public enum CubeKind
    {
        Player,
        EnemyNormal,
        EnemyElite,
        EnemyBoss,
        GateKeeper,
        Treasure,
        Heal,
        Event,
        City,
        RareBoss,
    }

    public enum CubeSideKind
    {
        Battle,
        Facility,
    }

    public enum CubeReactionType
    {
        None = 0,
        AdjacentButton = 1,
        StepOn = 2,
    }

    public enum CubeOccupancyType
    {
        Blocks = 0,
        PassableOnTop = 1,
    }

    public enum CubeReusePolicy
    {
        Repeatable = 0,
        OneShot = 1,
    }
    #endregion

    #region スキル関連enum

    /// <summary>
    /// 【新】スキルの「当て方」を 3 種に固定して迷いを減らす。
    /// - SelfArea   : 自分中心。effectRange で単体にもAoEにもできる（中心1マスだけなら単体）。
    /// - PointArea  : 1点（主にユニット）を選び、その点を中心に effectRange を当てる（いわゆるバースト）。
    /// - MultiSingle: targetRange 内の複数対象へ「単体ヒット」を複数回当てる（マルチ単体）。
    ///   ※AoE（巻き込み）は effectRange のみで表現し、MultiSingle は AoE ではない。
    /// </summary>
    public enum SkillTargetingMode
    {
        SelfArea = 0,
        PointArea = 1,
        MultiSingle = 2,
    }

    /// <summary>
    /// PointArea の中心点（targetRange で選ぶもの）が「ユニット」か「マス」か。
    /// ※当面は Unit だけ運用してもOK。Cell は将来拡張用。
    /// </summary>
    public enum SkillPointTargetKind
    {
        Unit = 0,
        Cell = 1,
    }

    /// <summary>
    /// MultiSingle の「何人に当てるか」。
    /// - MaxTargets : maxTargets まで（選び方は targetSelect）。
    /// - AllInRange : targetRange 内の全員。
    /// </summary>
    public enum MultiSinglePickMode
    {
        MaxTargets = 0,
        AllInRange = 1,
    }

    public enum SkillCategory
    {
        ActiveAttack,
        ActiveSupport,
        ActiveMove,
        Passive,
    }

    /// <summary>
    /// スキルのダメージ/回復方式。
    /// Unityのenumシリアライズ崩壊を防ぐため、数値を固定しています。
    /// ※旧 True(=3) は削除し、3は欠番として残します。
    /// </summary>
    public enum SkillDamageKind
    {
        None = 0,
        Physical = 1,          // AT vs DF（amountは%）
        Magical = 2,           // MAT vs MDF（amountは%）

        // 3 は旧 True で使用していた欠番（残す）
        // True = 3,  ←削除

        PenetratePhysical = 4, // AT vs DF(低下後)（amountは%）
        PenetrateMagical = 5,  // MAT vs MDF(低下後)（amountは%）

        Fixed = 6,             // 固定値（amountはそのまま数値。-で回復）
        MaxHpRate = 7,         // 最大HP割合（amountは%。-で割合回復）
    }

    /// <summary>
    /// 【新】ターゲットの陣営（単一選択で矛盾を防ぐ）
    /// </summary>
    public enum EffectTargetSide
    {
        None,   // NonTarget
        Self,   // 自分/味方側
        Enemy,  // 敵側
        Both,   // 両方
    }

    /// <summary>
    /// 【新】ターゲット選択ルール（Flags）
    /// 例：Enemy + Nearest + maxTargets=2 で「近い敵から2人」。
    /// </summary>
    [System.Flags]
    public enum TargetSelectFlags
    {
        None = 0,

        // 選び方（優先順/抽選）
        Nearest = 1 << 0,
        Farthest = 1 << 1,
        Random = 1 << 2,
        LowestHp = 1 << 3,
        HighestHp = 1 << 4,
        // ★追加：AoE適切化（targetRange内の候補を中心にeffectRangeを当てた時、ヒット数最大を選ぶ）
        MaximizeAoeHits = 1 << 5,

        // 中心点の扱い
        UseTargetPoint = 1 << 8,  // requiresTarget=true の中心点を使う意図（明示）
        UseEffectRange = 1 << 9,  // effectRange を使って範囲化する意図（明示）
    }

    /// <summary>
    /// スキルの種類タグ（Flags）
    /// ※ DamageKind があるので Physical/Magical はここに置かない（重複防止）
    /// </summary>
    [System.Flags]
    public enum SkillTag
    {
        None = 0,
        Ground = 1 << 0,      // 地面依存
        Air = 1 << 1,         // 空中対象/空中属性
        Projectile = 1 << 2,  // 飛び道具
        Melee = 1 << 3,       // 近接
        // ここに「Knockback」「Stun」「Area」等、挙動/ルール系タグを増やしていく想定
    }

    public enum StatusEffectType
    {
        None,
        BuffAt,
        BuffDf,
        BuffAgi,
        DebuffAt,
        DebuffDf,
        DebuffAgi,
        Stun,
        Freeze,
        Burn,
        HealMorale,
        ChainDamage,
        Custom,
    }

    #endregion

    #region Skill発火イベント（新）

    /// <summary>
    /// 「発火の大分類」だけを少数で用意する。
    /// 具体的な発火ポイント（Opportunity / SkillDeclared / TimeStart / HpDelta etc）は、
    /// 下の各enum（Phase/Sourceなど）で詳細化する方針。
    /// </summary>
    public enum SkillTriggerTiming
    {
        None = 0,
        Action = 1,   // 行動機会/スキル宣言など
        Time = 2,     // タイム開始/終了など
        Hp = 3,       // HP増減が発生した
        Status = 4,   // 状態異常の付与/解除/無効化など
        Board = 5,    // 盤面変化（移動/押し出し/地形変化など）
    }

    /// <summary>
    /// Action系の詳細フェーズ。
    /// - Opportunity : 「行動できるタイミングが来た」（硬直中でも来る可能性がある前提）
    /// - SkillDeclared: 「アクションスキルを宣言した瞬間」（コスト確定/拘束開始の起点）
    /// </summary>
    public enum ActionEventPhase
    {
        Opportunity = 0,
        SkillDeclared = 1,
    }

    /// <summary>
    /// HP増減イベントの“原因種別”。
    /// OnHit と DoT を分けたい話をここで吸収する（同じ Hp トリガーでも source で区別）。
    /// </summary>
    public enum HpDeltaSourceType
    {
        Unknown = 0,
        Hit = 1,       // 直接ヒット（通常攻撃/スキルヒット/多段など）
        Dot = 2,       // 継続ダメージ/継続回復
        Reflect = 3,   // 反射
        Env = 4,       // 環境/地形/ギミック
        SelfCost = 5,  // 自傷コスト/HP支払い
    }

    // SpaceJourneyCoreTypes.cs（どこでもいいが SkillTriggerTiming 付近がおすすめ）
    public enum SkillConditionKind
    {
        None = 0,

        // 自分
        SelfHpBelowPercent = 10,
        SelfHpAbovePercent = 11,
        SelfMovedThisTime = 12,
        SelfNotMovedThisTime = 13,

        // 対象（other/target）
        TargetHasAnyStatus = 30,
        TargetHasDebuff = 31,
        TargetHpBelowPercent = 32,

        // 戦況
        EnemyCountAtLeast = 50,
        AllyCountAtLeast = 51,

        // 使用されたスキルの性質
        UsedSkillIsBasic = 70,
        UsedSkillHasTag = 71,
        UsedSkillIsBodySkill = 72,
        UsedSkillIsWeaponSkill = 73,
    }

    #endregion

    #region reincarnation関連enum
    public enum ReinEventType
    {
        None,
        Happy,
        Sad,
        RankUp,
        JobChange
    }
    #endregion

    #endregion
}
