using UnityEngine;

// This is used to allow us to seal Awake and OnDestroy so the MonoSingleton functionality cannot be accidentally overriden.
public class MonoSingletonBase : MonoBehaviour {
    protected virtual void Awake() { }
    protected virtual void OnDestroy() { }
}

public abstract class MonoSingleton<T> : MonoSingletonBase where T : MonoSingleton<T> {
    private static bool hasBeenDestroyed;
    private static bool hasDoneStartup;
    private static T instance;

    public virtual bool Persist { get { return true; } }

    public static bool Exists {
        get {
            return instance != null;
        }
    }

    public static T Instance {
        get {
#if UNITY_EDITOR
            // We only pay attention to the destroyed flag at runtime.
            // In the editor, we want to be able to access the instance regardless of whether it's been destroyed previously.
            if(Application.isPlaying && hasBeenDestroyed) {
                return null;
            }
#else
            if(hasBeenDestroyed){
                return null;
            }
#endif

            EnsureInstance();

            return instance;
        }
    }

    public static void EnsureInstance() {
        if(Exists) {
            return;
        }

        // I dislike this because it's slow, but it's the best way to confirm if an instance of this class exists.
        instance = FindObjectOfType<T>();

        // If we have confirmed that an instance does not already exist, then we create a new one.
        if(instance == null) {
            GameObject newObject = new GameObject(typeof(T).ToString());
            instance = newObject.AddComponent<T>();
        }

        // As the instance is now being referenced and the 'instance' property had not been populated yet, we need to call Startup().
        instance.Startup();
    }

    /// <summary>
    /// Do not attempt to override this method. If you need to do something when the instance is initialised, use OnInstanceInitialised() instead.
    /// </summary>
    protected sealed override void Awake() {
        // There can be only one!
        if(instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        // If we're here, then we don't need to run EnsureInstance(). It uses FindObjectOfType<T>() which is slow.
        if(instance == null) {
            instance = this as T;
            Startup();
        }
    }

    /// <summary>
    /// Do not attempt to override this method. If you need to do something when the instance is destroyed, use OnInstanceDestroyed() instead.
    /// </summary>
    protected sealed override void OnDestroy() {
        // Only set hasBeenDestroyed if this is the 'instance' we're destroying.
        if(instance != this) {
            return;
        }

        // Only call OnInstanceDestroyed if Init() has been called. No need to tear down something that was never set up.
        if(!hasBeenDestroyed && hasDoneStartup) {
            OnInstanceDestroyed();
        }

        instance = null;
        hasBeenDestroyed = true;
        hasDoneStartup = false;
    }

    private void Startup() {
        if(hasDoneStartup) {
            return;
        }

        OnInstanceInitialised();

        // If this object is parented then DontDestroyOnLoad() will have no effect, it will be bound to the lifetime of the parent.
        if(Persist && transform.parent == null && Application.isPlaying) {
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// This is called on Awake, or when the Instance is first referenced, whichever happens first. This will only be called once.
    /// </summary>
    protected abstract void OnInstanceInitialised();

    /// <summary>
    /// This is called when the assigned static 'instance' is destroyed. This will only be called once, and will only be called if OnInstanceInitialised() was called on this 'instance' previously.
    /// </summary>
    protected abstract void OnInstanceDestroyed();
}
