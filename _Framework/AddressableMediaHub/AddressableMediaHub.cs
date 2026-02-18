using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Addressables の Audio(BGM/SE)、Sprite（一般）、Bodyアイコン、Prefab を
/// すべて「カタログのリスト」で管理するハブ。
/// - すべての取得は「リストの先頭から順に探索して最初に見つかったものを採用」
/// - 先読み(Preload)も複数カタログを横断して実行
/// - 解放(Release)は id/number から逆引きして行う
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Game/Media/Addressable Media Hub")]
public class AddressableMediaHub : SingletonPersistent<AddressableMediaHub>
{
    // ========= Audio Catalogs（リスト）=========
    [Header("Audio Catalogs (Lists, ordered search)")]
    [SerializeField] private List<AudioCatalog> bgmCatalogs = new(); // 先頭優先
    [SerializeField] private List<AudioCatalog> seCatalogs = new(); // 先頭優先

    [Header("BGM Players")]
    [SerializeField] private AudioSource bgmA;
    [SerializeField] private AudioSource bgmB;
    [SerializeField] private float defaultBgmFadeSeconds = 0.75f;

    [Header("SE Pool")]
    [SerializeField] private AudioSource sePrefab;
    [SerializeField] private int sePoolSize = 12;

    private readonly Queue<AudioSource> sePool = new();
    private readonly Dictionary<AudioSource, bool> seBusy = new();
    private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> _audioHandles = new();
    private bool _bgmToggle;
    private Coroutine _bgmFadeCo;

    // ========= Sprite Catalogs（一般）=========
    [Header("Sprite Catalogs (General, ordered search)")]
    [SerializeField] private List<SpriteCatalog> spriteCatalogs = new(); // 例: Common, Unique

    private readonly Dictionary<string, AsyncOperationHandle<Sprite>> _spriteHandles = new();

    // ========= Bodyアイコン（専用）=========
    [Header("Body Icon Sprite Catalogs (ordered search)")]
    [SerializeField] private List<SpriteCatalog> bodySpriteCatalogs = new(); // 今は1つでもOK

    // ========= Prefab Catalogs（3Dモデル）=========
    [Header("Prefab Catalogs (ordered search)")]
    [SerializeField] private List<PrefabCatalog> prefabCatalogs = new(); // 今は1つでもOK

    private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _prefabHandles = new();

    // ========= Lifecycle =========
    protected override void Awake()
    {
        base.Awake();
        EnsureBgmSources();
        BuildSePool();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ReleaseAllAudio();
        ReleaseAllSprites();
        ReleaseAllPrefabs();
    }

    private void EnsureBgmSources()
    {
        if (!bgmA) bgmA = gameObject.AddComponent<AudioSource>();
        if (!bgmB) bgmB = gameObject.AddComponent<AudioSource>();
        foreach (var s in new[] { bgmA, bgmB })
        {
            s.playOnAwake = false;
            s.loop = true;
            s.volume = 1f;
        }
    }

    private void BuildSePool()
    {
        for (int i = 0; i < Mathf.Max(1, sePoolSize); i++)
        {
            var src = sePrefab ? Instantiate(sePrefab, transform)
                               : gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;
            src.volume = 1f;
            sePool.Enqueue(src);
            seBusy[src] = false;
        }
    }

    // =====================================================================
    // Audio: Public
    // =====================================================================
    public void PlayBgm(string id, float fadeSeconds = -1f, float? volumeOverride = null)
    {
        if (!TryGetAudioEntry(bgmCatalogs, id, out var e)) return;
        if (fadeSeconds < 0f) fadeSeconds = defaultBgmFadeSeconds;
        StartCoroutine(CoPlayBgm(e, fadeSeconds, volumeOverride));
    }

    public void StopBgm(float fadeSeconds = -1f)
    {
        if (fadeSeconds < 0f) fadeSeconds = defaultBgmFadeSeconds;
        if (_bgmFadeCo != null) StopCoroutine(_bgmFadeCo);
        _bgmFadeCo = StartCoroutine(CoFadeOutBoth(fadeSeconds));
    }

    public void PlaySe(string id, float? volumeOverride = null)
        => PlaySeAt(id, Vector3.zero, is3D: false, spatialBlend: 0f, volumeOverride);

    public void PlaySeAt(string id, Vector3 position, bool is3D = true, float spatialBlend = 1f, float? volumeOverride = null)
    {
        StartCoroutine(CoPlaySe(id, position, is3D, spatialBlend, volumeOverride));
    }

    /// <summary>ラベルで一括先読み（BGM/SE 両リストを横断）</summary>
    public void PreloadAudioByLabel(string label, Action<float> onProgress = null, Action onCompleted = null)
        => StartCoroutine(CoPreloadAudioByLabel(label, onProgress, onCompleted));

    public void ReleaseAudio(string id)
    {
        if (_audioHandles.TryGetValue(id, out var h) && h.IsValid())
        {
            Addressables.Release(h);
            _audioHandles.Remove(id);
        }
    }

    public void ReleaseAllAudio()
    {
        foreach (var kv in _audioHandles)
            if (kv.Value.IsValid()) Addressables.Release(kv.Value);
        _audioHandles.Clear();
    }

    // =====================================================================
    // Sprite（一般）: Public
    // =====================================================================
    public void GetSprite(string id, Action<Sprite> onLoaded)
        => StartCoroutine(CoGetSprite(id, onLoaded));

    public void SetImage(Image target, string id, bool preserveNativeSize = false)
    {
        if (!target) return;
        StartCoroutine(CoSetImage(target, id, preserveNativeSize));
    }

    public void PreloadSpritesByLabel(string label, Action<float> onProgress = null, Action onCompleted = null)
        => StartCoroutine(CoPreloadSpritesByLabel_List(spriteCatalogs, label, onProgress, onCompleted));

    public void ReleaseSprite(string id)
    {
        if (_spriteHandles.TryGetValue(id, out var h) && h.IsValid())
        {
            Addressables.Release(h);
            _spriteHandles.Remove(id);
        }
    }
    // Sprite番号→id 逆引きして解放（一般スプライト用）
    public void ReleaseSprite(int number)
    {
        if (TryGetSpriteEntryByNumber(spriteCatalogs, number, out var e) && e != null)
        {
            ReleaseSprite(e.id); // 既存の string 版に委譲
        }
    }

    public void ReleaseAllSprites()
    {
        foreach (var kv in _spriteHandles)
            if (kv.Value.IsValid()) Addressables.Release(kv.Value);
        _spriteHandles.Clear();
    }

    // =====================================================================
    // Bodyアイコン: Public（専用カタログのリスト）
    // =====================================================================
    public void GetBodyIcon(int number, Action<Sprite> onLoaded)
    {
        if (!TryGetSpriteEntryByNumber(bodySpriteCatalogs, number, out var e) || e.sprite == null)
        {
            onLoaded?.Invoke(null); return;
        }
        GetSprite(e.id, onLoaded); // string経由でロード（キャッシュ共有）
    }

    public void GetBodyIcon(string id, Action<Sprite> onLoaded)
    {
        if (!TryGetSpriteEntryById(bodySpriteCatalogs, id, out var e) || e.sprite == null)
        {
            onLoaded?.Invoke(null); return;
        }
        GetSprite(e.id, onLoaded);
    }

    public void SetBodyIcon(Image target, int number, bool nativeSize = false)
    {
        if (!TryGetSpriteEntryByNumber(bodySpriteCatalogs, number, out var e) || e.sprite == null) return;
        SetImage(target, e.id, nativeSize);
    }

    public void SetBodyIcon(Image target, string id, bool nativeSize = false)
    {
        if (!TryGetSpriteEntryById(bodySpriteCatalogs, id, out var e) || e.sprite == null) return;
        SetImage(target, e.id, nativeSize);
    }

    public void PreloadBodySpritesByLabel(string label, Action<float> onProgress = null, Action onCompleted = null)
        => StartCoroutine(CoPreloadSpritesByLabel_List(bodySpriteCatalogs, label, onProgress, onCompleted));

    public void ReleaseBodySprite(int number)
    {
        if (TryGetSpriteEntryByNumber(bodySpriteCatalogs, number, out var e)) ReleaseSprite(e.id);
    }

    public void ReleaseBodySprite(string id) => ReleaseSprite(id);

    // =====================================================================
    // Prefab（3Dモデル）: Public（リスト）
    // =====================================================================
    public void GetPrefab(string id, Action<GameObject> onLoaded)
        => StartCoroutine(CoGetPrefabById(id, onLoaded));

    public void GetPrefab(int number, Action<GameObject> onLoaded)
    {
        if (!TryGetPrefabEntryByNumber(prefabCatalogs, number, out var e)) { onLoaded?.Invoke(null); return; }
        GetPrefab(e.id, onLoaded);
    }

    public void InstantiatePrefab(string id, Transform parent,
                                  Vector3? pos = null, Quaternion? rot = null, Action<GameObject> onInstantiated = null)
    {
        GetPrefab(id, prefab =>
        {
            if (prefab == null) { onInstantiated?.Invoke(null); return; }
            var go = Instantiate(prefab, parent);
            if (pos.HasValue) go.transform.localPosition = pos.Value;
            if (rot.HasValue) go.transform.localRotation = rot.Value;
            onInstantiated?.Invoke(go);
        });
    }

    public void InstantiatePrefab(int number, Transform parent,
                                  Vector3? pos = null, Quaternion? rot = null, Action<GameObject> onInstantiated = null)
    {
        if (!TryGetPrefabEntryByNumber(prefabCatalogs, number, out var e)) { onInstantiated?.Invoke(null); return; }
        InstantiatePrefab(e.id, parent, pos, rot, onInstantiated);
    }

    public void PreloadPrefabsByLabel(string label, Action<float> onProgress = null, Action onCompleted = null)
        => StartCoroutine(CoPreloadPrefabsByLabel_List(prefabCatalogs, label, onProgress, onCompleted));

    public void ReleasePrefab(string id)
    {
        if (_prefabHandles.TryGetValue(id, out var h) && h.IsValid())
        {
            Addressables.Release(h);
            _prefabHandles.Remove(id);
        }
    }

    public void ReleasePrefabNumber(int number)
    {
        if (TryGetPrefabEntryByNumber(prefabCatalogs, number, out var e)) ReleasePrefab(e.id);
    }

    public void ReleaseAllPrefabs()
    {
        foreach (var kv in _prefabHandles)
            if (kv.Value.IsValid()) Addressables.Release(kv.Value);
        _prefabHandles.Clear();
    }

    // AddressableMediaHub.cs の public メソッド群のどこかに追加
    public void ReleaseAll()
    {
        ReleaseAllAudio();
        ReleaseAllSprites();
        ReleaseAllPrefabs();
    }

    // =====================================================================
    // Audio: Internals
    // =====================================================================
    private bool TryGetAudioEntry(List<AudioCatalog> cats, string id, out AudioCatalog.Entry e)
    {
        e = null;
        if (cats == null) return false;
        foreach (var cat in cats)
        {
            if (cat != null && cat.TryGet(id, out e) && e != null) return true;
        }
        Debug.LogError($"AddressableMediaHub(Audio): id not found '{id}'", this);
        return false;
    }

    private IEnumerator CoPlayBgm(AudioCatalog.Entry e, float fadeSeconds, float? volumeOverride)
    {
        var h = LoadAudioClip(e.id, e.clip);
        yield return h;
        var clip = h.Result;
        if (clip == null) yield break;

        var srcIn = _bgmToggle ? bgmB : bgmA;
        var srcOut = _bgmToggle ? bgmA : bgmB;
        _bgmToggle = !_bgmToggle;

        if (e.mixerGroup) srcIn.outputAudioMixerGroup = e.mixerGroup;
        srcIn.clip = clip;
        srcIn.loop = e.loopForBgm;
        srcIn.volume = 0f;
        srcIn.Play();

        if (_bgmFadeCo != null) StopCoroutine(_bgmFadeCo);
        float baseVol = Mathf.Max(0.0001f, volumeOverride ?? e.defaultVolume);
        _bgmFadeCo = StartCoroutine(CoCrossFade(srcIn, srcOut, fadeSeconds, baseVol));
    }

    private IEnumerator CoCrossFade(AudioSource fadeIn, AudioSource fadeOut, float seconds, float targetVolume)
    {
        float t = 0f;
        float startOut = fadeOut.isPlaying ? fadeOut.volume : 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = (seconds <= 0f) ? 1f : Mathf.Clamp01(t / seconds);
            fadeIn.volume = Mathf.Lerp(0f, targetVolume, a);
            if (fadeOut.isPlaying) fadeOut.volume = Mathf.Lerp(startOut, 0f, a);
            yield return null;
        }
        fadeIn.volume = targetVolume;
        if (fadeOut.isPlaying) { fadeOut.Stop(); fadeOut.clip = null; }
    }

    private IEnumerator CoFadeOutBoth(float seconds)
    {
        float t = 0f;
        float a0 = bgmA.isPlaying ? bgmA.volume : 0f;
        float a1 = bgmB.isPlaying ? bgmB.volume : 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = (seconds <= 0f) ? 1f : Mathf.Clamp01(t / seconds);
            if (bgmA.isPlaying) bgmA.volume = Mathf.Lerp(a0, 0f, a);
            if (bgmB.isPlaying) bgmB.volume = Mathf.Lerp(a1, 0f, a);
            yield return null;
        }
        if (bgmA.isPlaying) { bgmA.Stop(); bgmA.clip = null; }
        if (bgmB.isPlaying) { bgmB.Stop(); bgmB.clip = null; }
    }

    private IEnumerator CoPlaySe(string id, Vector3 position, bool is3D, float spatialBlend, float? volumeOverride)
    {
        if (!TryGetAudioEntry(seCatalogs, id, out var e)) yield break;

        var h = LoadAudioClip(e.id, e.clip);
        yield return h;
        var clip = h.Result;
        if (clip == null) yield break;

        var src = RentSeSource();
        if (!src) yield break;

        src.transform.position = position;
        src.spatialBlend = is3D ? Mathf.Clamp01(spatialBlend) : 0f;
        src.clip = clip;
        src.volume = Mathf.Max(0.0001f, volumeOverride ?? e.defaultVolume);

        src.Play();
        yield return new WaitWhile(() => src && src.isPlaying);
        ReturnSeSource(src);
    }

    private AudioSource RentSeSource()
    {
        int tries = sePool.Count;
        while (tries-- > 0)
        {
            var src = sePool.Dequeue();
            if (!seBusy[src])
            {
                seBusy[src] = true;
                sePool.Enqueue(src);
                return src;
            }
            sePool.Enqueue(src);
        }
        var f = sePool.Peek();
        f.Stop(); f.clip = null;
        seBusy[f] = true;
        return f;
    }

    private void ReturnSeSource(AudioSource src)
    {
        if (!src) return;
        src.Stop();
        src.clip = null;
        seBusy[src] = false;
    }

    private AsyncOperationHandle<AudioClip> LoadAudioClip(string id, AssetReferenceT<AudioClip> reference)
    {
        if (_audioHandles.TryGetValue(id, out var handle) && handle.IsValid())
            return handle;
        var h = reference.LoadAssetAsync();
        _audioHandles[id] = h;
        return h;
    }

    private IEnumerator CoPreloadAudioByLabel(string label, Action<float> onProgress, Action onCompleted)
    {
        var keys = new List<AssetReference>();
        void Collect(List<AudioCatalog> cats)
        {
            if (cats == null) return;
            foreach (var cat in cats)
            {
                if (cat == null) continue;
                foreach (var e in cat.entries)
                    if (!string.IsNullOrEmpty(e.label) && e.label == label && e.clip != null)
                        keys.Add(e.clip);
            }
        }
        Collect(bgmCatalogs);
        Collect(seCatalogs);

        if (keys.Count == 0) { onCompleted?.Invoke(); yield break; }

        int done = 0;
        foreach (var k in keys)
        {
            var h = Addressables.LoadAssetAsync<AudioClip>(k);
            _audioHandles[GetAudioIdByReference(k)] = h;
            while (!h.IsDone)
            {
                onProgress?.Invoke((done + h.PercentComplete) / keys.Count);
                yield return null;
            }
            done++;
            onProgress?.Invoke((float)done / keys.Count);
        }
        onCompleted?.Invoke();
    }

    private string GetAudioIdByReference(AssetReference r)
    {
        string key = r.RuntimeKey.ToString();

        string Find(List<AudioCatalog> cats)
        {
            if (cats == null) return null;
            foreach (var cat in cats)
            {
                if (cat == null) continue;
                foreach (var e in cat.entries)
                    if (e?.clip != null && e.clip.RuntimeKey.ToString() == key)
                        return e.id;
            }
            return null;
        }

        return Find(bgmCatalogs) ?? Find(seCatalogs);
    }

    // =====================================================================
    // Sprite: Internals（一般／Bodyアイコン共通ヘルパ）
    // =====================================================================
    private bool TryGetSpriteEntryById(List<SpriteCatalog> cats, string id, out SpriteCatalog.Entry e)
    {
        e = null;
        if (cats == null) return false;
        foreach (var cat in cats)
            if (cat != null && cat.TryGet(id, out e) && e != null) return true;
        return false;
    }

    private bool TryGetSpriteEntryByNumber(List<SpriteCatalog> cats, int number, out SpriteCatalog.Entry e)
    {
        e = null;
        if (cats == null) return false;
        foreach (var cat in cats)
            if (cat != null && cat.TryGet(number, out e) && e != null) return true;
        return false;
    }

    private AsyncOperationHandle<Sprite> LoadSprite(string id, AssetReferenceT<Sprite> reference)
    {
        if (_spriteHandles.TryGetValue(id, out var h) && h.IsValid())
            return h;
        var handle = reference.LoadAssetAsync();
        _spriteHandles[id] = handle;
        return handle;
    }

    private IEnumerator CoGetSprite(string id, Action<Sprite> onLoaded)
    {
        if (!TryGetSpriteEntryById(spriteCatalogs, id, out var e))
        {
            Debug.LogError($"AddressableMediaHub(Sprite): id not found '{id}'", this);
            yield break;
        }
        var h = LoadSprite(id, e.sprite);
        yield return h;
        onLoaded?.Invoke(h.Result);
    }

    private IEnumerator CoSetImage(Image target, string id, bool nativeSize)
    {
        if (!TryGetSpriteEntryById(spriteCatalogs, id, out var e))
        {
            Debug.LogError($"AddressableMediaHub(Sprite): id not found '{id}'", this);
            yield break;
        }
        var h = LoadSprite(id, e.sprite);
        yield return h;

        if (!target) yield break;
        target.sprite = h.Result;
        if (nativeSize) target.SetNativeSize();
    }

    private IEnumerator CoPreloadSpritesByLabel_List(List<SpriteCatalog> cats, string label, Action<float> onProgress, Action onCompleted)
    {
        var keys = new List<AssetReference>();
        if (cats != null)
        {
            foreach (var cat in cats)
            {
                if (cat == null) continue;
                foreach (var e in cat.entries)
                    if (!string.IsNullOrEmpty(e.label) && e.label == label && e.sprite != null)
                        keys.Add(e.sprite);
            }
        }

        if (keys.Count == 0) { onCompleted?.Invoke(); yield break; }

        int done = 0;
        foreach (var k in keys)
        {
            var h = Addressables.LoadAssetAsync<Sprite>(k);
            _spriteHandles[GetSpriteIdByReference(k)] = h;
            while (!h.IsDone)
            {
                onProgress?.Invoke((done + h.PercentComplete) / keys.Count);
                yield return null;
            }
            done++;
            onProgress?.Invoke((float)done / keys.Count);
        }
        onCompleted?.Invoke();
    }

    private string GetSpriteIdByReference(AssetReference r)
    {
        string key = r.RuntimeKey.ToString();

        string Find(List<SpriteCatalog> cats)
        {
            if (cats == null) return null;
            foreach (var cat in cats)
            {
                if (cat == null) continue;
                foreach (var e in cat.entries)
                    if (e?.sprite != null && e.sprite.RuntimeKey.ToString() == key)
                        return e.id;
            }
            return null;
        }

        // 一般→Body専用の順で逆引き（順序は用途に合わせて調整可）
        return Find(spriteCatalogs) ?? Find(bodySpriteCatalogs);
    }

    // =====================================================================
    // Prefab: Internals
    // =====================================================================
    private bool TryGetPrefabEntryById(List<PrefabCatalog> cats, string id, out PrefabCatalog.Entry e)
    {
        e = null;
        if (cats == null) return false;
        foreach (var cat in cats)
            if (cat != null && cat.TryGet(id, out e) && e != null) return true;
        return false;
    }

    private bool TryGetPrefabEntryByNumber(List<PrefabCatalog> cats, int number, out PrefabCatalog.Entry e)
    {
        e = null;
        if (cats == null) return false;
        foreach (var cat in cats)
            if (cat != null && cat.TryGet(number, out e) && e != null) return true;
        return false;
    }

    private AsyncOperationHandle<GameObject> LoadPrefab(string cacheKey, AssetReferenceT<GameObject> reference)
    {
        if (_prefabHandles.TryGetValue(cacheKey, out var h) && h.IsValid()) return h;
        var handle = reference.LoadAssetAsync();
        _prefabHandles[cacheKey] = handle;
        return handle;
    }

    private IEnumerator CoGetPrefabById(string id, Action<GameObject> onLoaded)
    {
        if (!TryGetPrefabEntryById(prefabCatalogs, id, out var e) || e.prefab == null) { onLoaded?.Invoke(null); yield break; }
        var h = LoadPrefab(id, e.prefab);
        yield return h;
        onLoaded?.Invoke(h.Result);
    }

    private IEnumerator CoPreloadPrefabsByLabel_List(List<PrefabCatalog> cats, string label, Action<float> onProgress, Action onCompleted)
    {
        var keys = new List<AssetReference>();
        if (cats != null)
        {
            foreach (var cat in cats)
            {
                if (cat == null) continue;
                foreach (var e in cat.entries)
                    if (!string.IsNullOrEmpty(e.label) && e.label == label && e.prefab != null)
                        keys.Add(e.prefab);
            }
        }

        if (keys.Count == 0) { onCompleted?.Invoke(); yield break; }

        int done = 0;
        foreach (var k in keys)
        {
            var h = Addressables.LoadAssetAsync<GameObject>(k);

            // id逆引き（prefabHandlesに紐付け）
            foreach (var cat in cats)
            {
                if (cat == null) continue;
                foreach (var ent in cat.entries)
                {
                    if (ent?.prefab != null && ent.prefab.RuntimeKey.ToString() == k.RuntimeKey.ToString())
                    {
                        _prefabHandles[ent.id] = h;
                        goto Bound;
                    }
                }
            }
        Bound:

            while (!h.IsDone)
            {
                onProgress?.Invoke((done + h.PercentComplete) / keys.Count);
                yield return null;
            }
            done++;
            onProgress?.Invoke((float)done / keys.Count);
        }
        onCompleted?.Invoke();
    }
}
