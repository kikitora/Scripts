using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Game/Prefab/Prefab Catalog")]
public class PrefabCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Tooltip("一意なID（例: Prefab/Body/SlimeA）")]
        public string id;

        [Tooltip("任意の番号（重複注意・未使用は -1）")]
        public int number = -1;

        [Tooltip("Addressables の Prefab 参照")]
        public AssetReferenceT<GameObject> prefab;

        [Tooltip("任意のラベル（先読みグループ用）")]
        public string label;
    }

    [SerializeField] public List<Entry> entries = new();

    Dictionary<string, Entry> _byId;
    Dictionary<int, Entry> _byNum;

    void OnEnable() => Rebuild();
    void OnValidate() => Rebuild();

    void Rebuild()
    {
        _byId = new Dictionary<string, Entry>(StringComparer.Ordinal);
        _byNum = new Dictionary<int, Entry>();
        var seenId = new HashSet<string>(StringComparer.Ordinal);
        var seenNum = new HashSet<int>();

        foreach (var e in entries)
        {
            if (e == null) continue;

            if (!string.IsNullOrWhiteSpace(e.id))
            {
                if (!seenId.Add(e.id))
                    Debug.LogError($"PrefabCatalog: Duplicate id '{e.id}'", this);
                else
                    _byId[e.id] = e;
            }

            if (e.number >= 0)
            {
                if (!seenNum.Add(e.number))
                    Debug.LogError($"PrefabCatalog: Duplicate number '{e.number}'", this);
                else
                    _byNum[e.number] = e;
            }
        }
    }

    public bool TryGet(string id, out Entry e)
    {
        if (_byId != null && id != null && _byId.TryGetValue(id, out e)) return true;
        e = null; return false;
    }

    public bool TryGet(int number, out Entry e)
    {
        if (_byNum != null && number >= 0 && _byNum.TryGetValue(number, out e)) return true;
        e = null; return false;
    }
}
