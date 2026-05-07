using System;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// リアルタイム戦闘用スキル定義。距離/秒/形状ベース。
    /// 形状はターゲット方向を向くように発動 (絶対方位ではなく、相対)。
    /// </summary>
    [CreateAssetMenu(
        fileName = "RealtimeSkill",
        menuName = "SteraCube/Realtime/Skill Definition")]
    public class RealtimeSkillDefinition : ScriptableObject
    {
        [Header("識別")]
        public string skillId;
        public string skillName;
        [TextArea] public string description;

        [Header("分類")]
        public RealtimeSkillType skillType = RealtimeSkillType.Attack;

        [Header("形状/範囲")]
        public RealtimeSkillShape shape = new RealtimeSkillShape();

        [Header("対象選択")]
        public RealtimeTargetSide targetSide = RealtimeTargetSide.Enemy;
        public RealtimeTargetSelect targetSelect = RealtimeTargetSelect.Nearest;
        [Tooltip("対象ジョブ絞り込み (空=全職)。例: [Warrior] なら戦士のみから選ぶ")]
        public BodyJobDefinition[] targetJobFilter;

        [Header("タイミング")]
        [Tooltip("再使用までの待機秒")]
        public float cooldownSec = 2f;
        [Tooltip("戦闘開始時の初回発動ディレイ秒")]
        public float openingCooldownSec = 0f;
        [Tooltip("発動モーション長。この間ユニットは停止する")]
        public float castAnimSec = 0.8f;

        [Header("効果")]
        public SkillDamageKind damageKind = SkillDamageKind.Physical;
        [Tooltip("ダメージ倍率。Fixed の場合は直接ダメージ値として使用")]
        public float powerMul = 1.0f;
        [Tooltip("回復量 (Heal 系)")]
        public int healAmount = 0;
        [Tooltip("最大HP割合回復 (0.075 = 7.5%)。0 のとき healAmount を使用、>0 のとき優先される")]
        public float healMaxHpRate = 0f;
        [Tooltip("ノックバック距離 (m)。0=なし")]
        public float knockbackMass = 0f;
        [Tooltip("挑発時間 (秒)。>0 で対象を強制的にこちらを狙わせる")]
        public float tauntDurationSec = 0f;
        [Tooltip("付与ステータス")]
        public SkillDefinition.StatusEffectSpec[] statusEffects;

        [Header("アニメ")]
        [Tooltip("発動時の Animator Trigger 名 (空なら Attack)")]
        public string animTriggerName = "Attack";
        [Tooltip("Trigger 後に明示遷移するステート名 (空なら Trigger 依存)")]
        public string animStateName = "";
        [Tooltip("アニメ再生倍率")]
        public float animSpeedMul = 1f;

        [Header("投射体設定 (Projectile 系スキル用)")]
        [Tooltip("0=瞬間ヒット (非投射体)。>0 なら DealDamage 時刻に [距離/速度] 秒を自動加算。Projectile prefab の speed にも同期")]
        public float projectileSpeed = 0f;
        [Tooltip("射線上の敵ブロック挙動。PassThrough=爆発魔法系、FirstEnemyInLine=弓矢系 (味方は素通り)")]
        public ProjectileHitMode projectileHitMode = ProjectileHitMode.PassThrough;

        [Header("手装備アタッチ (弓射の矢等、起動時にボーン子に自動配置)")]
        [Tooltip("装備プレハブ (矢のモデル等)。指定があればキャラ生成時にボーン配下に spawn (初期非表示)")]
        public GameObject handAttachmentPrefab;
        [Tooltip("装着先ボーン名。デフォルト: Weapon_Root_R")]
        public string handAttachmentBoneName = "Weapon_Root_R";
        [Tooltip("ボーンローカル位置オフセット")]
        public Vector3 handAttachmentOffset;
        [Tooltip("最終回転 (Euler度)。キャスト t=rotationEndTime でこの値に到達")]
        public Vector3 handAttachmentEulerOffset;
        [Tooltip("開始回転 (Euler度)。キャスト t=0 の時の回転。Start==End ならアニメなし")]
        public Vector3 handAttachmentEulerStart;
        [Tooltip("Start→End 到達までの秒数。0 なら即時 End。例: 0.182")]
        public float handAttachmentRotationEndTime = 0f;
        [Tooltip("発射後、nock姿勢→aim方向 へ回転補間する秒数。0=即時aim、0.2推奨")]
        public float flyRotationBlendTime = 0.2f;

        [Header("装備の時間ベース軌道 (空なら静的 offset/rotation 使用)")]
        [Tooltip("各時刻での装備の位置/回転キーフレーム。設定あれば上の静的値は無視、補間で再生。")]
        public System.Collections.Generic.List<AttachmentKeyframe> attachmentKeyframes
            = new System.Collections.Generic.List<AttachmentKeyframe>();

        [Header("タイムライン (キャスト開始からの相対時刻でイベント発火)")]
        [Tooltip("DealDamage が含まれる場合はその時刻までダメージ遅延、含まれなければ即時適用。Effect/PlaySound は指定時刻に発火")]
        public System.Collections.Generic.List<RealtimeSkillEvent> timeline = new System.Collections.Generic.List<RealtimeSkillEvent>();

        [Header("エディタプレビュー専用 (実行時未使用)")]
        [Tooltip("SkillTimelineEditor でプレビュー再生する AnimationClip")]
        public AnimationClip previewAnimClip;

        [Header("ジョブ紐付け (任意)")]
        [Tooltip("このスキルを使用可能なジョブ (空なら全職可)")]
        public BodyJobDefinition[] allowedBodyJobs;

        [Header("SubSkill 専用挙動 (ConditionalCast から発動された時のみ)")]
        [Tooltip("DealDamage イベントが発火した時に通常 ApplySkillEffect の代わりに発動する特殊挙動。\n" +
                 "AoE 範囲やノックバック距離など SO で表現できないものはコード側で hardcode。\n" +
                 "None なら通常の damageKind/powerMul で計算。")]
        public SubSkillBehavior subSkillBehavior = SubSkillBehavior.None;
    }

    /// <summary>SubSkill 専用挙動。 SO で表現できない AoE 範囲などはコード側 hardcode。</summary>
    public enum SubSkillBehavior
    {
        None,                // 通常の damageKind/powerMul で計算
        AoeAroundTarget,     // 対象中心 1.5m AoE、 powerMul で各敵にダメ (朱雀爆炎 / 三星の輝き)
        PullEnemies,         // 対象を自分方向へ powerMul 倍 (m) 引き寄せ (重力の調べ)
        KnockbackAoE,        // 自前方 2x2 (2m 幅 × 2m 奥行) BOX 内の敵を powerMul 倍 (m) 押し戻す (さざ波)
    }

    /// <summary>
    /// スキル発動中の 1 イベント (エフェクト/SE/ダメージ発生)
    /// </summary>
    [Serializable]
    public class RealtimeSkillEvent
    {
        [Tooltip("キャスト開始からの時刻 (秒)")]
        public float timeSec = 0f;
        public RealtimeSkillEventKind kind = RealtimeSkillEventKind.Effect;
        [Tooltip("エフェクト Prefab (kind=Effect 時)")]
        public GameObject effectPrefab;
        [Tooltip("出現位置基準")]
        public RealtimeEffectOrigin spawnOrigin = RealtimeEffectOrigin.Self;
        [Tooltip("基準位置からのオフセット (ローカル: x=右, y=上, z=前)")]
        public Vector3 offset;
        [Tooltip("Effect の生存秒 (0 以下なら Destroy しない)")]
        public float lifeSec = 2f;
        [Tooltip("親子化する子オブジェクト名 (例: WeaponPoint)。空なら world 座標に固定")]
        public string attachBoneName = "";
        [Tooltip("true: attachBoneName が見つからない場合 Effect spawn を skip (fallback しない)。" +
                 "false: 見つからない場合 Weapon_Root_R 等に fallback。" +
                 "TrailTip 等「装備武器に依存」する Effect では true 推奨。")]
        public bool requireBone = false;
        [Tooltip("SE (kind=PlaySound 時)")]
        public AudioClip sound;
        public string label;

        // ===== ConditionalCast 用 =====
        [Tooltip("発動条件 (kind=ConditionalCast 時)。 null なら無条件 (procChance のみで判定)。\n" +
                 "RealtimePassive SO 参照で、そのパッシブが unit.activePassives に登録されてれば true。\n" +
                 "戦士 rank3 自動追撃マーカー / 武器パッシブ 等を区別なく扱える。")]
        public RealtimePassiveDefinition branchRequiredPassive;
        [Tooltip("発動枠 (N 回中 1 回確定で発動、 kind=ConditionalCast 時)。\n" +
                 "1=毎回、 2=2 回中 1 回、 3=3 回中 1 回、... 0 や負値は \"使わない\" (= branchProcChance を使う)。\n" +
                 "周期内位置はランダム → ユニット間で同期しない、 周期境界で稀に 2 連発する味あり。")]
        public int branchTriggerInterval = 0;
        [Tooltip("発動確率 (kind=ConditionalCast 時、 0-1)。1.0 で必ず発動。\n" +
                 "branchTriggerInterval > 0 の時は無視される。")]
        [Range(0f, 1f)]
        public float branchProcChance = 1f;
        [Tooltip("発動するサブスキル SO (kind=ConditionalCast 時)。 自身の anim/timeline で再生")]
        public RealtimeSkillDefinition branchSubSkill;
    }

    public enum RealtimeSkillEventKind
    {
        Effect,           // エフェクト Prefab を spawn
        PlaySound,        // SE 再生
        DealDamage,       // 指定時刻にダメージ/効果適用
        UnequipWeapon,    // 装備武器を一時非表示 (cast 終了で自動復帰)
        ConditionalCast,  // 条件満たせばサブスキルを発動 (追撃/分岐)
    }

    public enum RealtimeEffectOrigin
    {
        Self,         // 自分位置
        SelfForward,  // 自分前方 (shape.lengthMass × 0.5 の位置)
        Target,       // primary target 位置
        ImpactPoint,  // Line/CircleAtTarget の着弾点 (rangeMax 先)
        CasterHand,   // 手ボーン位置 (未実装の場合は Self にフォールバック)
    }

    /// <summary>
    /// スキル形状パラメータ。shape 種別により使うフィールドが変わる。
    /// </summary>
    [Serializable]
    public class RealtimeSkillShape
    {
        public RealtimeTargetShape shape = RealtimeTargetShape.Single;

        [Tooltip("最小射程 (これ未満の距離では使えない)")]
        public float rangeMin = 0f;
        [Tooltip("最大射程 (Single/CircleAtTarget=狙える距離、他形状=リーチとして使用)")]
        public float rangeMax = 1f;

        [Tooltip("幅 or 半径 (Line=幅、Fan/Circle*=半径、Square/Cross=腕幅)")]
        public float widthMass = 1f;
        [Tooltip("奥行/長さ (Line/Square/Cross/Diamond で使用)")]
        public float lengthMass = 1f;
        [Tooltip("扇角度 degrees (Fan のみ)")]
        public float angleDeg = 90f;

        [Header("連鎖 (雷撃型)")]
        [Tooltip("連鎖回数。0=連鎖なし。1=初撃+1回ジャンプ")]
        public int chainMaxJumps = 0;
        [Tooltip("前ヒット先から次敵へ飛べる最大距離 (m)")]
        public float chainDistance = 3f;
        [Tooltip("連鎖ごとのダメージ倍率 (1=減衰なし、0.7=30%減ずつ)")]
        public float chainDamageMul = 0.7f;
    }

    /// <summary>
    /// 装備 (矢等) の時間ベース軌道 1 キーフレーム。bone local 座標。
    /// </summary>
    [Serializable]
    public class AttachmentKeyframe
    {
        public float time;
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
    }

    public enum RealtimeSkillType
    {
        Attack, Heal, Buff, Debuff, Control, Move,
    }

    public enum ProjectileHitMode
    {
        PassThrough,      // 間の敵を無視して指定 target に命中 (爆発/着弾型)
        FirstEnemyInLine, // 射線上の最初の敵に命中。味方は素通り (弓矢/投擲)
    }

    public enum RealtimeTargetShape
    {
        Single,          // 単体
        CircleAtSelf,    // 自分中心円 (widthMass=半径)
        CircleAtTarget,  // 着弾点中心円 (rangeMax=射程、widthMass=爆発半径)
        Line,            // 前方直線 (widthMass=幅、lengthMass=長さ)
        Fan,             // 前方扇形 (widthMass=半径、angleDeg=角度)
        Diamond,         // 自分中心菱形 (widthMass=横半径、lengthMass=前後半径)
        Square,          // 前方矩形 (widthMass=幅、lengthMass=奥行)
        CrossAtSelf,     // 自分中心十字 (widthMass=腕幅、lengthMass=腕長)
    }

    public enum RealtimeTargetSide
    {
        Enemy,
        Ally,
        Self,
        AnyAllyIncludingSelf,
    }

    public enum RealtimeTargetSelect
    {
        Nearest,
        Farthest,
        LowestHp,
        HighestHp,
        Random,
        Self,
        LastAttacker,  // 直近の自分を攻撃したユニット
    }
}
