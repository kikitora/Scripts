using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// 武器の種類。装備ボーンの既定はこの enum + WeaponKindExtensions で決まる。
    /// </summary>
    public enum WeaponKind
    {
        Sword,    // 片手剣  → 右手
        Dagger,   // 短剣    → 右手
        Axe,      // 斧      → 右手
        Mace,     // 棍棒    → 右手
        Staff,    // 杖      → 右手
        Spear,    // 槍      → 右手 (両手)
        GreatSword, // 両手剣 → 右手 (両手)
        Bow,      // 弓      → 左手 (右手は矢)
        Crossbow, // クロスボウ → 左手
        Wand,     // ワンド  → 右手
        Shield,   // 盾      → 左手
    }

    /// <summary>
    /// リアルタイム戦闘向け武器定義。旧 WeaponDefinition.cs とは別物。
    /// MasterDatabase.realtimeWeapons に登録する。
    /// </summary>
    [CreateAssetMenu(fileName = "RealtimeWeapon", menuName = "SteraCube/Realtime/Weapon")]
    public class RealtimeWeaponDefinition : ScriptableObject
    {
        [Header("基本")]
        public string weaponId;
        public string displayName;
        [TextArea] public string description;

        [Header("出現情報")]
        [Tooltip("この武器を持ったボディが出現し始めるエリアランク (1=序盤エリア, 7=最上位エリア)")]
        [Min(1)] public int minAreaLevel = 1;

        [Tooltip("レア度。出現プール内での抽選確率に影響する (1=コモン / 2=アンコモン / 3=レア / 4=最大レア・各職1本のみ)")]
        [Range(1, 4)] public int rarity = 1;

        [Header("種類 / 装備位置")]
        public WeaponKind kind;

        [Tooltip("空なら kind の既定ボーン使用 (Sword→Weapon_Root_R, Bow→Weapon_Root_L 等)")]
        public string attachBoneNameOverride = "";
        public Vector3 attachPositionOffset;
        public Vector3 attachEulerOffset;
        public Vector3 attachScale = Vector3.one;

        [Header("3D モデル")]
        public GameObject modelPrefab;

        [Header("両手持ち：左手IK（GreatSword 等で使用）")]
        [Tooltip("modelPrefab 内の左手グリップ Transform 名。空なら IK 無効。例: LeftHandGrip")]
        public string leftHandGripName = "";

        [Header("トレイル取付点 (ブレード先端)")]
        [Tooltip("EquipWeapon で武器インスタンスの子に \"TrailTip\" GameObject を自動生成、ここで指定したローカル座標に配置。" +
                 "Skill 側の Effect イベント (attachBoneName: TrailTip) でトレイルなどをこの位置に固定できる。" +
                 "Vector3.zero (デフォルト) なら TrailTip 生成なし → トレイル無効。" +
                 "例: 大剣で刃先まで約 1.5m なら (0, 1.5, 0) (武器の local +Y が刃方向の場合)")]
        public Vector3 trailTipLocalPosition = Vector3.zero;

        [Header("使用可能ジョブ (body job id)")]
        public List<string> allowedJobIds = new List<string>();

        [Header("武器スキル (装備時に使えるアクティブスキル、将来用)")]
        public List<RealtimeSkillDefinition> weaponSkills = new List<RealtimeSkillDefinition>();

        [Header("武器パッシブ (装備時に自動発動する効果)")]
        public List<RealtimePassiveDefinition> weaponPassives = new List<RealtimePassiveDefinition>();

        [Header("盾専用: ガード設定 (kind=Shield のみ有効)")]
        [Tooltip("被弾時にガードが発動する確率 (0-1)。0.2=20%")]
        [Range(0f, 1f)] public float guardChance = 0f;
        [Tooltip("ガード発動時、ダメージをこの割合カット (0-1)。0.3=30%軽減")]
        [Range(0f, 1f)] public float guardMitigation = 0f;
        [Tooltip("ガード発動時に追加発動するパッシブ (足早 / 攻撃者炎上 など)。盾の guardChance で判定")]
        public List<RealtimePassiveDefinition> onGuardEffects = new List<RealtimePassiveDefinition>();

        /// <summary>実際に使うボーン名 (override 優先)</summary>
        public string ResolveBoneName()
        {
            return WeaponKindExtensions.ResolveBoneName(kind, attachBoneNameOverride);
        }
    }

    public static class WeaponKindExtensions
    {
        /// <summary>kind に対応する既定ボーン名を返す。override 優先。
        /// 新しい子 root 系 (Spear_Root / Bow_Root / Sheild_Root) を優先。
        /// RealtimeBattleUnit.ResolveAttachTransform が見つからない場合 Weapon_Root_R/L に自動 fallback。</summary>
        public static string ResolveBoneName(WeaponKind k, string overrideName = null)
        {
            if (!string.IsNullOrEmpty(overrideName)) return overrideName;
            switch (k)
            {
                case WeaponKind.Spear:
                    return "Spear_Root";
                case WeaponKind.Bow:
                case WeaponKind.Crossbow:
                    return "Bow_Root";
                case WeaponKind.Shield:
                    return "Sheild_Root";
                default:
                    return "Weapon_Root_R";
            }
        }
    }
}
