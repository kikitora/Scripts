using UnityEngine;

[DisallowMultipleComponent]
public abstract class SingletonPersistent<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _shuttingDown;

    public static T Instance
    {
        get
        {
            if (_shuttingDown) return null;
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)Object.FindFirstObjectByType(typeof(T));
                    if (_instance == null)
                    {
                        var go = new GameObject($"{typeof(T).Name} (Singleton)");
                        _instance = go.AddComponent<T>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnApplicationQuit() { _shuttingDown = true; }
    protected virtual void OnDestroy()
    {
        if (!_shuttingDown && _instance == this) _instance = null;
    }
}
[DisallowMultipleComponent]
public abstract class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _shuttingDown;

    public static T Instance
    {
        get
        {
            if (_shuttingDown) return null;
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)Object.FindFirstObjectByType(typeof(T));
                    if (_instance == null)
                    {
                        var go = new GameObject($"{typeof(T).Name} (SceneSingleton)");
                        _instance = go.AddComponent<T>(); // DontDestroyOnLoad ‚µ‚È‚¢
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this as T;
    }

    protected virtual void OnApplicationQuit() { _shuttingDown = true; }
    protected virtual void OnDestroy()
    {
        if (!_shuttingDown && _instance == this) _instance = null;
    }
}