using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Addressables の Sprite を「id（文字列）」で管理するカタログ。
/// 任意で「number（int）」も付けられ、番号からの取得にも対応する。
/// 既存の id 運用はそのまま、番号を使いたいエントリだけ number を設定する。
/// </summary>
[CreateAssetMenu(menuName = "Game/Sprite/Sprite Catalog")]
public class SpriteCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Tooltip("一意なID。例: UI/Icon/Apple")]
        public string id;

        [Tooltip("実体のAddressable Sprite参照")]
        public AssetReferenceT<Sprite> sprite;

        [Tooltip("任意のラベル。先読みなどのグルーピングに使用")]
        public string label;

        [Tooltip("番号運用したい場合だけ設定。未使用なら -1 のままでOK")]
        public int number = -1;
    }

    [SerializeField] public List<Entry> entries = new();

    // 高速引き用マップ
    private Dictionary<string, Entry> _mapById;
    private Dictionary<int, Entry> _mapByNumber;

    private void OnEnable() => Rebuild();
    private void OnValidate() => Rebuild();

    private void Rebuild()
    {
        _mapById = new Dictionary<string, Entry>(StringComparer.Ordinal);
        _mapByNumber = new Dictionary<int, Entry>();

        var seenId = new HashSet<string>(StringComparer.Ordinal);
        var seenNum = new HashSet<int>();

        foreach (var e in entries)
        {
            if (e == null) continue;

            // id マップ
            if (!string.IsNullOrWhiteSpace(e.id))
            {
                if (!seenId.Add(e.id))
                    Debug.LogError($"SpriteCatalog: Duplicate id '{e.id}'", this);
                else
                    _mapById[e.id] = e;
            }

            // number マップ（設定されているときだけ）
            if (e.number >= 0)
            {
                if (!seenNum.Add(e.number))
                    Debug.LogError($"SpriteCatalog: Duplicate number '{e.number}'", this);
                else
                    _mapByNumber[e.number] = e;
            }
        }
    }

    /// <summary>id(文字列)から取得。見つからない場合 false を返し、out は null。</summary>
    public bool TryGet(string id, out Entry e)
    {
        if (_mapById != null && id != null && _mapById.TryGetValue(id, out e))
            return true;

        e = null; // ★ out は必ず代入してから戻る
        return false;
    }

    /// <summary>number(int)から取得。見つからない場合 false を返し、out は null。</summary>
    public bool TryGet(int number, out Entry e)
    {
        if (_mapByNumber != null && number >= 0 && _mapByNumber.TryGetValue(number, out e))
            return true;

        e = null; // ★ out は必ず代入してから戻る
        return false;
    }
}
