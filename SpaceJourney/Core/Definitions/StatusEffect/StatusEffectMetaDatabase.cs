// StatusEffectMetaDatabase.cs
// 状態異常 (StatusEffectType) ごとの slot / rank / 表示メタを Inspector で管理する SO。
// MasterDatabase に登録し、起動時に StatusEffectMeta.Init() で取り込む。

using System;
using System.Collections.Generic;
using UnityEngine;
using SteraCube.SpaceJourney.Realtime;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 状態異常付与中の Animator 速度制御モード。
    /// Hardcc 系 (Stun/Freeze/Petrify) で使う。
    /// </summary>
    public enum AnimatorPauseMode
    {
        None = 0,        // 制御なし (通常)
        FullStop = 1,    // animator.speed = 0 (完全凍結)
        SlowMotion = 2,  // animator.speed = 0.3 (鈍重表現)
    }

    /// <summary>
    /// SO 内のカテゴリ分類。Inspector の Header と 1:1 対応。
    /// Editor の「一括追加」ボタンが各 enum を slot/type に応じてここに振り分ける。
    /// </summary>
    public enum StatusEffectCategoryGroup
    {
        ActionDisrupt = 0,  // 行動妨害系
        Debuff = 1,         // デバフ系 (ステ低下 + 受け側マーク)
        Ailment = 2,        // 状態異常系 (DoT + 即時)
        Buff = 3,           // バフ・自己効果系
    }

    [CreateAssetMenu(fileName = "StatusEffectMetaDatabase", menuName = "SteraCube/StatusEffect/Meta Database")]
    public class StatusEffectMetaDatabase : ScriptableObject
    {
        /// <summary>
        /// 各 StatusEffectType がどのカテゴリに属するかを返す。
        /// 「未登録 enum を一括追加」ボタンで使う振り分け基準。
        /// </summary>
        public static StatusEffectCategoryGroup CategorizeByType(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Stun:
                case StatusEffectType.Freeze:
                case StatusEffectType.Charm:
                case StatusEffectType.Silence:
                    return StatusEffectCategoryGroup.ActionDisrupt;

                case StatusEffectType.DebuffAt:
                case StatusEffectType.DebuffDf:
                case StatusEffectType.DebuffAgi:
                case StatusEffectType.DebuffMat:
                case StatusEffectType.DebuffMdf:
                case StatusEffectType.DamageTakenUp:
                case StatusEffectType.DisableHealing:
                case StatusEffectType.Immobilize:
                    return StatusEffectCategoryGroup.Debuff;

                case StatusEffectType.Taunt:
                case StatusEffectType.Stealth:
                    // 自己バフ。Taunt は発動者本人に付与され、Stealth は敵ターゲット対象外になる。
                    return StatusEffectCategoryGroup.Buff;

                case StatusEffectType.Burn:
                    return StatusEffectCategoryGroup.Ailment;

                default:
                    // BuffAt/Df/Agi/Mat/Mdf, Counter, Invincible, SurviveLethal, AutoRevive,
                    // AoeAbsorb, CoverAlly, Reflect, DamageShare, Regen, RangeBoost,
                    // NextAttackAppliesStun, EffectRangeBoost, IgnoreDefenseReactions, Custom, None
                    return StatusEffectCategoryGroup.Buff;
            }
        }

        [Serializable]
        public class Entry
        {
            [Tooltip("対象の状態異常 enum")]
            public StatusEffectType type;

            [Tooltip("重複制御用の枠。同 slot は 1 つしか持てず、ランク勝負で決着する")]
            public StatusEffectSlot slot = StatusEffectSlot.None;

            [Tooltip("同 slot 内の上書き判定。高い方が勝つ。デフォルト 0 = 同ランク扱い (後勝ち)")]
            public int rank = 0;

            [Header("UI 表示用 (プレイヤーに見せる)")]
            [Tooltip("UI 表示名 (日本語)。例: スタン, 凍結, 炎上")]
            public string displayName;

            [Tooltip("状態付与時にキャラ上へ一瞬表示する文字。空欄なら文字ポップアップは出さない。例: AT+, 回復不能, 被ダメ+")]
            [InspectorName("文字表示")]
            public string popupText;

            [TextArea(2, 4)]
            [Tooltip("プレイヤー向けの効果説明。ゲーム内 UI (バフ枠ホバー等) でそのまま表示する想定。\n" +
                     "雰囲気重視で短めに。例: '氷漬けになり、完全に動けない'")]
            public string description;

            [Header("内部メモ (開発者向け、UI には出さない)")]
            [TextArea(2, 6)]
            [Tooltip("挙動の技術メモ。slot/rank/IsDisruptive/exclusiveWith 等の実装上の注意点を記録する。\n" +
                     "Claude や他開発者が SO を見たときに仕組みを把握しやすくする用途。")]
            public string devNote;

            [Header("ビジュアル (Phase 2)")]
            [Tooltip("バフ/デバフ枠表示用アイコン")]
            public Sprite icon;

            [Tooltip("付与中ずっと unit の子として表示される常駐 VFX (炎、氷殻 等)")]
            public GameObject auraPrefab;

            [Tooltip("付与瞬間に 1 度だけ unit 位置で再生する VFX (任意)")]
            public GameObject hitVfxPrefab;

            [Tooltip("効果が実際に発動/消費された瞬間に unit 位置で再生する VFX。例: HP1耐え、復活")]
            public GameObject resolvedVfxPrefab;

            [Tooltip("効果が実際に発動/消費された瞬間に再生する演出専用 Skill Timeline SO。\n" +
                     "Animation / Effect / PlaySound だけを使う想定。DealDamage / ConditionalCast は実戦側で無視する。")]
            public RealtimeSkillDefinition resolvedTimelineSkill;

            [Tooltip("付与中の Animator 速度制御。Hardcc 系で使う (Freeze/Petrify=FullStop 等)")]
            public AnimatorPauseMode pauseMode = AnimatorPauseMode.None;

            [Header("対立属性 (排他関係)")]
            [Tooltip("ここに登録された StatusEffectType を持っている対象に新規付与すると、" +
                     "既存効果を削除してから自分を付与する (= 後勝ちで上書き)。" +
                     "例: Burn の exclusiveWith に Freeze を入れると、凍結中の敵に炎上を付与した瞬間 凍結が消える。" +
                     "両方向の排他にするには、Burn と Freeze 両方に相手を登録する。")]
            public List<StatusEffectType> exclusiveWith = new();
        }

        // ===========================================================================
        // 4 カテゴリ別 List
        // Inspector で Header ごとに分けて編集できる。Get(type) は 4 リスト横断で検索。
        // ===========================================================================

        [Header("━━━━━━ 行動妨害系 ━━━━━━")]
        [Tooltip("ActionDisrupt 枠。\n" +
                 "対象: Stun, Freeze, Charm, Confusion, Silence")]
        [SerializeField] private List<Entry> actionDisruptEntries = new();

        [Header("━━━━━━ デバフ系 ━━━━━━")]
        [Tooltip("ステ低下 + 特殊マーク (受け側に付くデメリット効果)。\n" +
                 "対象: DebuffAt/Df/Agi/Mat/Mdf, DamageTakenUp, DisableHealing, Immobilize")]
        [SerializeField] private List<Entry> debuffEntries = new();

        [Header("━━━━━━ 状態異常系 ━━━━━━")]
        [Tooltip("DoT (継続ダメ) 系。\n" +
                 "対象: Burn (現状はこれだけ)")]
        [SerializeField] private List<Entry> ailmentEntries = new();

        [Header("━━━━━━ バフ・自己効果系 ━━━━━━")]
        [Tooltip("ステ強化 + 防御効果 + 回復 + ユーティリティ + 武器パッシブ拡張枠。\n" +
                 "対象: BuffAt/Df/Agi/Mat/Mdf, Counter, Invincible, SurviveLethal, AutoRevive,\n" +
                 "AoeAbsorb, CoverAlly, Reflect, DamageShare, Regen, RangeBoost, Stealth,\n" +
                 "NextAttackAppliesStun, EffectRangeBoost, IgnoreDefenseReactions, Custom")]
        [SerializeField] private List<Entry> buffEntries = new();

        [Header("━━━━━━ 共通リアクション VFX ━━━━━━")]
        [Tooltip("HP 回復が実際に入った瞬間に対象位置で再生する VFX。通常回復スキル/吸血/味方全体回復などで使用")]
        public GameObject healReceivedVfxPrefab;

        [Tooltip("ステータス異常解除が実際に起きた瞬間に対象位置で再生する VFX")]
        public GameObject statusCleanseVfxPrefab;

        [Tooltip("ノックバックを受けた瞬間に対象位置で再生する VFX")]
        public GameObject knockbackTakenVfxPrefab;

        [Tooltip("魔法ダメージが魔防UPなどで軽減された瞬間に対象位置で再生する VFX")]
        public GameObject magicDamageReducedVfxPrefab;

        [Tooltip("CT 中スキル延長が実際に入った瞬間に対象位置で再生する VFX")]
        public GameObject cooldownExtendedVfxPrefab;

        // ===========================================================================
        // 全カテゴリ横断のアクセス
        // ===========================================================================

        /// <summary>4 カテゴリの全 Entry を 1 つの IEnumerable として返す。</summary>
        public IEnumerable<Entry> AllEntries
        {
            get
            {
                if (actionDisruptEntries != null) foreach (var e in actionDisruptEntries) if (e != null) yield return e;
                if (debuffEntries != null) foreach (var e in debuffEntries) if (e != null) yield return e;
                if (ailmentEntries != null) foreach (var e in ailmentEntries) if (e != null) yield return e;
                if (buffEntries != null) foreach (var e in buffEntries) if (e != null) yield return e;
            }
        }

        /// <summary>カテゴリに対応する List を返す (Editor 拡張用)。</summary>
        public List<Entry> GetListForCategory(StatusEffectCategoryGroup cat)
        {
            switch (cat)
            {
                case StatusEffectCategoryGroup.ActionDisrupt: return actionDisruptEntries ?? (actionDisruptEntries = new List<Entry>());
                case StatusEffectCategoryGroup.Debuff:        return debuffEntries        ?? (debuffEntries        = new List<Entry>());
                case StatusEffectCategoryGroup.Ailment:       return ailmentEntries       ?? (ailmentEntries       = new List<Entry>());
                case StatusEffectCategoryGroup.Buff:          return buffEntries          ?? (buffEntries          = new List<Entry>());
                default: return buffEntries ?? (buffEntries = new List<Entry>());
            }
        }

        // 起動時 cache (StatusEffectMeta.Init から BuildCache() を呼ぶ)
        private Dictionary<StatusEffectType, Entry> _cache;

        public Entry Get(StatusEffectType type)
        {
            if (_cache == null) BuildCache();
            return _cache.TryGetValue(type, out var e) ? e : null;
        }

        public void BuildCache()
        {
            _cache = new Dictionary<StatusEffectType, Entry>();
            foreach (var e in AllEntries)
            {
                _cache[e.type] = e;
            }
        }

        private void OnValidate()
        {
            // Inspector 編集時にキャッシュを破棄、次の Get で再構築
            _cache = null;
        }
    }
}
