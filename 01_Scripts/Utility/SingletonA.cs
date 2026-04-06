using UnityEngine;

/// <summary>
/// Generic MonoBehaviour-based singleton.
///     • First component that reaches Awake() becomes <see cref="Instance"/>
///     • Any later copy destroys itself immediately
///     • (Optional) survives scene changes when <c>persistant</c> is true
/// </summary>
public abstract class SingletonA<T> : Singleton where T : MonoBehaviour
{
    private static T instance;
    private static readonly object _lock = new object();

    [Header("Singleton")]
    [Tooltip("If true, this object lives across scenes (DontDestroyOnLoad).")]
    [SerializeField] private bool persistant = true;

    /// <summary>The one and only instance (null when the app is shutting down).</summary>
    public static T Instance
    {
        get
        {
            if (Quitting)          // app is closing – never create a new object
                return null;

            lock (_lock)
            {
                if (instance != null)
                    return instance;

                // ★ Fallback: find an existing object in the active scenes
                instance = FindObjectOfType<T>(true);
                return instance;
            }
        }
    }

    // ----------------------------------------------------------------------
    // ★ EARLY registration & duplicate check
    // ----------------------------------------------------------------------
    protected virtual void Awake()
    {
        // First copy? → Register myself and optionally persist
        if (instance == null)
        {
            instance = this as T;

            if (persistant)
                DontDestroyOnLoad(gameObject);
        }
        // Another copy appeared? → Destroy it instantly
        else if (instance != this)
        {
            Destroy(gameObject);
            return;                // prevent OnAwake() from running on a clone
        }

        OnAwake();                 // hook for subclasses
    }

    /// <summary>
    /// Override this in derived classes instead of Awake().
    /// Called exactly once on the real singleton.
    /// </summary>
    protected virtual void OnAwake() { }

    // ★ Optional: clear static ref if the singleton is manually destroyed
    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}

/// <summary>
/// Non-generic base – only tracks application quit state.
/// </summary>
public abstract class SingletonA : MonoBehaviour
{
    public static bool Quitting { get; private set; }

    private void OnApplicationQuit() => Quitting = true;
}