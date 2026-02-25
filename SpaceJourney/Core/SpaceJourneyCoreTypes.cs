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
        //     例：0.53 → 同格で"だいたい 0.53 倍"通るイメージ（＋チップ分）
        //
        // - DamageThroughDeltaScale：
        //     "差の効きやすさ"。小さいほど少しの差で通り率が大きく動く（格差が出やすい）。
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
        None = 0,
        Warrior, // 戦士向き
        Knight,  // 騎士向き
        Archer,  // 弓兵向き
        Mage,    // 魔術師向き
        Lancer   // 槍兵向き
    }
    public enum FaceSexCategory
    {
        None = -1,
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
        None = 0,
        Normal,
        Unique
    }

    public enum TalentRank
    {
        None = 0,
        A,
        B,
        C,
        D,
        E,
    }

    public enum GrowthType
    {
        None = 0,
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

    #region Skill発火イベント

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
    /// HP増減イベントの"原因種別"。
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

    /// <summary>
    /// パッシブスキルの発動条件の種類。
    /// SkillOccasionCondition.kind に設定し、SkillOccasionEvaluator が評価する。
    ///
    /// ■ 番号帯の割り当て
    ///   0      : None（常に真）
    ///   10-19  : 自分の状態
    ///   30-39  : 対象（other）の状態
    ///   50-59  : 戦況
    ///   70-79  : 使用されたスキルの性質
    ///   80-89  : 戦闘中カウント・直前アクション系（武器スキル用）
    ///   90-    : 将来枠
    /// </summary>
    public enum SkillConditionKind
    {
        None = 0,

        // ─── 自分の状態（10番台）───────────────────────────────
        SelfHpBelowPercent = 10,   // 自分の現在HP% < rateParam
        SelfHpAbovePercent = 11,   // 自分の現在HP% > rateParam
        SelfMovedThisTime = 12,   // このタイムに移動した
        SelfNotMovedThisTime = 13,  // このタイムに移動していない

        // ─── 対象（other）の状態（30番台）────────────────────────
        TargetHasAnyStatus = 30,   // 対象が何らかの状態異常を持っている
        TargetHasDebuff = 31,   // 対象がデバフを持っている
        TargetHpBelowPercent = 32,  // 対象の現在HP% < rateParam

        // ─── 戦況（50番台）──────────────────────────────────────
        EnemyCountAtLeast = 50,   // 残敵数 >= intParam
        AllyCountAtLeast = 51,   // 隣接味方数 >= intParam

        // ─── 使用スキルの性質（70番台）──────────────────────────
        UsedSkillIsBasic = 70,   // 使用スキルが基本攻撃
        UsedSkillHasTag = 71,   // 使用スキルが tagParam のタグを持つ
        UsedSkillIsBodySkill = 72, // 使用スキルがボディスキル
        UsedSkillIsWeaponSkill = 73,// 使用スキルが武器スキル

        // ─── 戦闘中カウント・直前アクション系（80番台）────────────
        // SkillTriggerContext に対応フィールドを追加して評価する。

        /// <summary>
        /// 戦闘中の累計攻撃回数が intParam 回目に達した。
        /// intParam=1 で「初撃」、intParam=3 で「3回目」。
        /// useCountLimit=1 と組み合わせて「N回目の1回だけ」を表現する。
        /// → SkillTriggerContext.selfAttackCount で評価。
        /// </summary>
        AttackCountReached = 80,

        /// <summary>
        /// 前回の自分の行動機会から今回の行動機会までの間に被弾した。
        /// 「被弾後の次の攻撃」を表現するためのフラグ。
        /// → SkillTriggerContext.selfWasHitSinceLastAction で評価。
        /// </summary>
        SelfWasHitSinceLastAction = 81,

        /// <summary>
        /// 直前に使ったスキルが「Defend」スキルだった。
        /// 「Defend後の次の攻撃」を表現するためのフラグ。
        /// → SkillTriggerContext.selfUsedDefendLastAction で評価。
        /// </summary>
        SelfUsedDefendLastAction = 82,

        /// <summary>
        /// 直前の攻撃で敵を撃破した。
        /// 「撃破後の次の攻撃」を表現するためのフラグ。
        /// → SkillTriggerContext.selfKilledEnemyLastAction で評価。
        /// </summary>
        SelfKilledEnemyLastAction = 83,

        /// <summary>
        /// 現在の攻撃が同一対象への intParam 回連続目のヒットである。
        /// intParam=2 で「同一対象への2回目の連続命中」。
        /// → SkillTriggerContext.consecutiveHitCount で評価。
        /// </summary>
        ConsecutiveHitSameTarget = 84,

        /// <summary>
        /// このタイムに、敵が自分に新たに隣接してきた。
        /// 「敵が近づいてきた後の次の攻撃」を表現する。
        /// → SkillTriggerContext.enemyBecameAdjacentThisTime で評価。
        /// </summary>
        EnemyBecameAdjacentThisTime = 85,

        /// <summary>
        /// 使用スキルが「重スキル」（reuseCycle > baseCost）である。
        /// 大技・重い技に対してのみ発動するパッシブに使う。
        /// → SkillTriggerContext.usedSkillIsHeavy で評価。
        /// </summary>
        UsedSkillIsHeavy = 86,

        /// <summary>
        /// 使用スキルが単体対象スキル（PointArea + Unit）である。
        /// AoEスキルでは発動しないパッシブに使う（薙ぎ広げ・跳弾など）。
        /// → SkillTriggerContext.targetingIsSingle で評価。
        /// </summary>
        TargetingIsSingle = 87,

        /// <summary>
        /// 戦闘中の累計魔法使用回数が intParam 回目に達した。
        /// intParam=2 で「2回目の魔法命中」。魔術師専用スキルで使用。
        /// → SkillTriggerContext.selfMagicCount で評価。
        /// </summary>
        MagicCountReached = 88,

        /// <summary>
        /// 攻撃対象が被ダメ増マーク（DamageMarkApply）を持っている。
        /// マーク付き対象への攻撃時にのみ発動するパッシブに使う。
        /// → SkillTriggerContext.targetHasDamageMark で評価。
        /// </summary>
        TargetHasDamageMark = 89,

        // 90番以降 : 将来枠
        // TargetDistanceAtLeast  = 90,  // 対象との距離 >= intParam（Lancer_03「間合い刺し」用）
    }

    #endregion

    #region 武器スキルパッシブ効果

    /// <summary>
    /// 武器スキルパッシブ効果の種類。
    /// SkillDefinition.StatusEffectSpec.weaponEffect に設定する。
    /// effectType = StatusEffectType.Custom のときのみ有効。
    ///
    /// ■ StatusEffectSpec 各フィールドの役割
    ///   weaponEffect  : 何をするか（このenum）
    ///   useCountLimit : 戦闘中の発動上限（0 = 無制限）
    ///   rateParam     : 割合値（回復率・軽減率・マーク率・近接強化倍率）
    ///   heavySkillOnly: true = reuseCycle &gt; baseCost の重スキル時のみ
    ///
    /// ■ 番号帯の割り当て
    ///    1-  9 : 追撃
    ///   10- 19 : 与ダメ強化
    ///   20- 29 : バリア（シールド）
    ///   30- 39 : 回復
    ///   40- 49 : 被ダメ軽減
    ///   50- 59 : マーク
    ///   60- 69 : 状態異常操作
    ///   70- 79 : スキルコスト・回転操作
    ///   80- 89 : 範囲操作
    ///   90- 99 : 追加ヒット系（巻き込み・跳弾）
    ///  100-    : 武器・近接特殊強化（常時パッシブ）
    /// </summary>
    public enum WeaponSkillEffectKind
    {
        None = 0,

        // ─── 追撃（元ヒットとは別の追加ヒット）────────────────────
        /// <summary>
        /// 追撃(小)：元ダメージの45%相当の追加ヒット。
        /// AoEなら各元ヒットで判定し、同一ターゲットへ追撃する。
        /// useCountLimit: 戦闘中の最大発動回数（0=無制限）。
        /// heavySkillOnly: true のとき重スキル命中時のみ発動。
        /// </summary>
        FollowupSmall = 1,

        /// <summary>
        /// 追撃(中)：元ダメージの75%相当の追加ヒット。
        /// useCountLimit: 最大発動回数（複数対象ヒット時はランダム抽選）。
        /// heavySkillOnly: true のとき重スキル命中時のみ発動。
        /// </summary>
        FollowupMedium = 2,

        /// <summary>
        /// 追撃(大)：元ダメージの87%相当の追加ヒット。
        /// useCountLimit: 最大発動回数。
        /// </summary>
        FollowupLarge = 3,


        // ─── 与ダメ強化（元ヒット自体を増幅）──────────────────────
        // 与ダメ+%は元ヒットの最終ダメージにのみ乗算。
        // 追撃・巻き込み・跳弾などの派生ヒットには適用しない。
        // AoEのときは各ターゲットへの元ヒットそれぞれに適用する。

        /// <summary>
        /// 与ダメ+(小)：元ヒットを +10% 強化。
        /// useCountLimit: 発動上限（0=無制限）。
        /// </summary>
        DamageBoostSmall = 10,

        /// <summary>
        /// 与ダメ+(中)：元ヒットを +20% 強化。
        /// useCountLimit: 発動上限（0=無制限）。
        /// </summary>
        DamageBoostMedium = 11,

        /// <summary>
        /// 与ダメ+(大)：元ヒットを +30% 強化。
        /// useCountLimit: 発動上限（0=無制限）。
        /// </summary>
        DamageBoostLarge = 12,

        /// <summary>
        /// 与ダメ+(カスタム%)：customIntValue の値（%）だけ元ヒットを強化。
        /// 小/中/大に当てはまらない数値（+25%など）を使いたいときに使う。
        /// customIntValue: 増加率（例: 25 → +25%）。
        /// useCountLimit: 発動上限。
        /// </summary>
        DamageBoostCustom = 13,


        // ─── バリア（シールド）──────────────────────────────────────
        /// <summary>
        /// 小バリア：自分に最大HPの10%分のシールドを付与。
        /// useCountLimit: 付与上限（通常 1）。
        /// </summary>
        BarrierSmall = 20,

        /// <summary>
        /// 中バリア：自分に最大HPの18%分のシールドを付与。
        /// useCountLimit: 付与上限（通常 1）。
        /// </summary>
        BarrierMedium = 21,

        /// <summary>
        /// 隣接味方バリア(小)：隣接する味方1体に小バリア（HP10%）を付与。
        /// 複数いる場合は最も近い1体（または先頭）を対象にする。
        /// useCountLimit: 付与上限（通常 1）。
        /// </summary>
        AllyBarrierSmall = 22,


        // ─── 回復────────────────────────────────────────────────────
        /// <summary>
        /// 自己回復：自分の最大HPの rateParam 分だけ回復。
        /// rateParam: 回復割合（例: 0.08 → HP8%回復）。
        /// useCountLimit: 発動上限（0=無制限）。
        /// </summary>
        SelfHealRate = 30,


        // ─── 被ダメ軽減──────────────────────────────────────────────
        /// <summary>
        /// 被ダメ軽減×1回：次に受ける被ダメージを1回だけ軽減。
        /// 軽減量は WeaponSkillConst.DamageReduceOnce_DefaultRate（デフォルト50%）。
        /// useCountLimit: 発動上限（通常 1）。
        /// </summary>
        DamageReduceOnce = 40,

        /// <summary>
        /// 被ダメ-% 常時パッシブ：条件を満たしている間、被ダメを rateParam 分軽減し続ける。
        /// rateParam: 軽減率（例: 0.1 → 被ダメ-10%）。
        /// timings = [Hp, Board] の両方を購読し、条件変化のたびに再チェックする。
        /// useCountLimit は使わない（常時適用）。
        /// </summary>
        DamageReducePassive = 41,


        // ─── マーク（付与者の次の元ヒットへの予約効果）──────────────
        /// <summary>
        /// 被ダメ増マーク付与：対象に「マーク」を付与する。
        ///
        /// ■ 確定仕様（SteraCube_BattleFlow_ExtraRules_2026-01-25）
        /// ・マーク系は弓兵（Archer）専用。他職は使わない。
        /// ・「付与者本人が次にその対象へ行う元ヒット」が命中したとき発動・消費。
        /// ・追撃/巻き込み/跳弾/派生ヒットでは発動しない・消費しない。
        /// ・ターン/タイムに依存しない（純粋に"次に当てた時"で発動）。
        /// ・対象に既にマークがある場合、後からの付与は無視（先着優先、上書き不可）。
        /// ・発動時：最終被ダメに rateParam を乗算し、マークを1回消費。
        ///
        /// rateParam: 被ダメ増加率（例: 0.25 → 被ダメ+25%）。
        /// </summary>
        DamageMarkApply = 50,


        // ─── 状態異常操作────────────────────────────────────────────
        /// <summary>
        /// 状態異常解除：自分が受けた状態異常を即時解除する。
        /// useCountLimit: 解除上限（通常 1）。
        /// </summary>
        StatusCleanse = 60,


        // ─── スキルコスト・回転操作──────────────────────────────────
        /// <summary>
        /// reuseCycle -1：次に使う攻撃スキルの再使用周期（reuseCycle）を1短縮。
        /// baseCostは変わらない（周期部分のみ短縮）。
        /// useCountLimit: 発動上限（通常 1）。
        /// heavySkillOnly: true のとき「短縮対象が重スキル」のときのみ有効。
        /// </summary>
        ReuseCycleReduce = 70,


        // ─── 範囲操作────────────────────────────────────────────────
        /// <summary>
        /// 効果範囲 +1 マス：次に使う魔法スキルの effectRange を1マス拡張。
        /// useCountLimit: 発動上限（通常 1）。
        /// heavySkillOnly: true のとき重スキル（重魔法）のみ対象。
        /// </summary>
        RangeExpand = 80,


        // ─── 追加ヒット系（範囲波及・跳弾）────────────────────────
        /// <summary>
        /// 巻き込み（SplashHit）：対象の隣接敵1体以上にミニ追加ヒット。
        /// 固定係数の小ダメージ（WeaponSkillConst.SplashHit_DamageRate を参照）。
        /// 与ダメ+%の適用外（派生ヒット扱い）。
        /// useCountLimit: 戦闘中の発動上限（0=無制限）。
        /// </summary>
        SplashHit = 90,

        /// <summary>
        /// 跳弾（RicochetHit）：別の敵1体に追加ヒット（近い敵優先、候補なしで不発）。
        /// 与ダメ+%の適用外（派生ヒット扱い）。
        /// useCountLimit: 戦闘中の発動上限。
        /// </summary>
        RicochetHit = 91,


        // ─── 武器・近接特殊強化（常時パッシブ）────────────────────
        /// <summary>
        /// 近接攻撃強化：特定の近接基本攻撃スキルの威力係数を上書き（常時）。
        /// 紋章弓(Archer_10)での「短剣倍率 0.6 → 1.0」強化に使用。
        /// rateParam: 新しい倍率（例: 1.0 → 等倍）。
        /// passiveTimings への登録は不要（常時パッシブとして初期化時に適用）。
        /// </summary>
        MeleeBoost = 100,
    }

    /// <summary>
    /// 武器スキル効果の数値定数。
    /// バランス調整はここだけ変えれば全スキルに反映される。
    ///
    /// 出典: SteraCube_BattleFlow_ExtraRules_2026-01-25（確定値）
    /// </summary>
    public static class WeaponSkillConst
    {
        // ─── 追撃 amount%（物理/魔法どちらも同じ係数を使う）────────
        /// <summary>追撃(小)のamount%。基本攻撃の45%相当。</summary>
        public const int FollowupSmall_Amount = 45;

        /// <summary>追撃(中)のamount%。基本攻撃の75%相当。</summary>
        public const int FollowupMedium_Amount = 75;

        /// <summary>追撃(大)のamount%。基本攻撃の87%相当。</summary>
        public const int FollowupLarge_Amount = 87;


        // ─── 与ダメ強化%──────────────────────────────────────────
        /// <summary>与ダメ+(小)：元ヒット +10%。</summary>
        public const int DamageBoost_Small = 10;

        /// <summary>与ダメ+(中)：元ヒット +20%。</summary>
        public const int DamageBoost_Medium = 20;

        /// <summary>与ダメ+(大)：元ヒット +30%。</summary>
        public const int DamageBoost_Large = 30;

        /// <summary>与ダメ+(大 弓専用)：静射など弓特有の +25% ライン。</summary>
        public const int DamageBoost_ArcherLarge = 25;


        // ─── DF弱体（StatusEffectType.DebuffDf の value 値）────────
        /// <summary>DF弱体(小)：DF -10%。</summary>
        public const int DebuffDf_Small = 10;

        /// <summary>DF弱体(中)：DF -20%。</summary>
        public const int DebuffDf_Medium = 20;


        // ─── バリア（最大HPに対する割合）───────────────────────────
        /// <summary>小バリアのシールド量：最大HPの10%。</summary>
        public const float BarrierSmall_HpRate = 0.10f;

        /// <summary>中バリアのシールド量：最大HPの18%。</summary>
        public const float BarrierMedium_HpRate = 0.18f;


        // ─── 自己回復────────────────────────────────────────────────
        /// <summary>自己回復(小)の標準回復率：最大HPの8%。</summary>
        public const float SelfHeal_DefaultRate = 0.08f;


        // ─── 被ダメ軽減──────────────────────────────────────────────
        /// <summary>被ダメ常時軽減（陣形維持）の軽減率：-10%。</summary>
        public const float DamageReducePassive_Rate = 0.10f;

        /// <summary>被ダメ軽減×1回のデフォルト軽減率（value=0のとき使用）：-50%。</summary>
        public const float DamageReduceOnce_DefaultRate = 0.50f;


        // ─── 被ダメ増マーク──────────────────────────────────────────
        /// <summary>
        /// 標的付け・呪印のマーク被ダメ増加率：+25%。
        /// 弓兵専用。元ヒット命中で発動・消費。先着優先で上書き不可。
        /// </summary>
        public const float DamageMark_Rate = 0.25f;


        // ─── 巻き込み・跳弾（実装側で参照する係数）─────────────────
        /// <summary>
        /// 巻き込み(SplashHit)の固定ダメージ係数。
        /// 元ヒットの amount に対してこの割合を掛けた値が追加ヒットの amount になる。
        /// </summary>
        public const float SplashHit_DamageRate = 0.25f;

        /// <summary>跳弾(RicochetHit)の固定ダメージ係数。</summary>
        public const float RicochetHit_DamageRate = 0.30f;
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