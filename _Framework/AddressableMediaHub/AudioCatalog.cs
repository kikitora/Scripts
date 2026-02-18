using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Game/Audio/Audio Catalog")]
public class AudioCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string id; // —á: "BGM_Exploration", "SE_Click"
        public AssetReferenceT<AudioClip> clip;
        public bool isBgm;
        [Range(0f, 1f)] public float defaultVolume = 1f; // Šù’è=1
        public bool loopForBgm = true;
        public AudioMixerGroup mixerGroup;
        public string label; // ”CˆÓ: "BGM"/"SE" ‚â "story:ch1" “™
    }

    public List<Entry> entries = new();

    private Dictionary<string, Entry> _map;

    void OnEnable() => Rebuild();
    void OnValidate()
    {
        // List ‚Ì [+] ’Ç‰Á’¼Œã‚Í 0 ‚Å“ü‚é‚Ì‚Å•ÛŒ¯
        if (entries != null)
        {
            foreach (var e in entries)
            {
                if (e == null) continue;
                if (e.defaultVolume == 0f) e.defaultVolume = 1f;
            }
        }
        Rebuild();
    }

    private void Rebuild()
    {
        _map = new Dictionary<string, Entry>(StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var e in entries)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.id)) continue;
            if (!seen.Add(e.id))
            {
                Debug.LogError($"AudioCatalog: Duplicate id '{e.id}'", this);
                continue;
            }
            _map[e.id] = e;
        }
    }

    public bool TryGet(string id, out Entry e)
    {
        if (_map == null) { e = null; return false; }
        return _map.TryGetValue(id, out e);
    }
}
