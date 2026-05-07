// StatusEffectMetaDatabase.cs
// 状態異常 (StatusEffectType) ごとの slot / rank / 表示メタを Inspector で管理する SO。
// MasterDatabase に登録し、起動時に StatusEffectMeta.Init() で取り込む。

using System;
using System.Collections.Generic;
using UnityEngine;

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

    [CreateAssetMenu(fileName = "StatusEffectMetaDatabase", menuName = "SteraCube/StatusEffect/Meta Database")]
    public class StatusEffectMetaDatabase : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("対象の状態異常 enum")]
            public StatusEffectType type;

            [Tooltip("重複制御用の枠。同 slot は 1 つしか持てず、ランク勝負で決着する")]
            public StatusEffectSlot slot = StatusEffectSlot.None;

            [Tooltip("同 slot 内の上書き判定。高い方が勝つ。デフォルト 0 = 同ランク扱い (後勝ち)")]
            public int rank = 0;

            [Header("UI 表示用")]
            [Tooltip("UI 表示名 (日本語)")]
            public string displayName;

            [TextArea]
            [Tooltip("簡易説明")]
            public string description;

            [Header("ビジュアル (Phase 2)")]
            [Tooltip("バフ/デバフ枠表示用アイコン")]
            public Sprite icon;

            [Tooltip("付与中ずっと unit の子として表示される常駐 VFX (炎、氷殻 等)")]
            public GameObject auraPrefab;

            [Tooltip("付与瞬間に 1 度だけ unit 位置で再生する VFX (任意)")]
            public GameObject hitVfxPrefab;

            [Tooltip("付与中の Animator 速度制御。Hardcc 系で使う (Freeze/Petrify=FullStop 等)")]
            public AnimatorPauseMode pauseMode = AnimatorPauseMode.None;
        }

        [SerializeField] private List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;

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
            if (entries == null) return;
            foreach (var e in entries)
            {
                if (e == null) continue;
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
