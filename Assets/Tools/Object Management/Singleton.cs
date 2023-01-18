using UnityEngine;

/// <summary>
/// Class used for representing a singleton design pattern. 
/// </summary>
/// <typeparam name="T">The type of class that you want a singleton of.</typeparam>
public abstract class Singleton<T> : OrderedInitializer where T : OrderedInitializer {
	private static T INSTANCE;

	private static object LOCK_OBJECT = new object();

	/// <summary>
	/// The instance of this singleton.
	/// </summary>
	public static T Instance {
		get {
			// if we're already quitting and the instance has been destroyed, forget about it and return null
			if (IS_QUITTING) {
				Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit.");
				return null;
			}

			// lock for thread safety
			lock (LOCK_OBJECT) {
				// if there is no existing instance
				if (INSTANCE == null) {
					// go find the instance in the scene
					INSTANCE = (T)FindObjectOfType(typeof(T));

					// if we can't find an instance in the scene, just make one since we need it
					if (INSTANCE == null) {
						GameObject newSingleton = new GameObject();
						INSTANCE = newSingleton.AddComponent<T>();
						newSingleton.name = "(Singleton) " + typeof(T).ToString();
						INSTANCE.enabled = true;
						INSTANCE.Initialize();
						Debug.LogWarning("[Singleton] An instance of " + typeof(T) + " is needed in the scene, so '" + newSingleton + "' was created with DontDestroyOnLoad.");
					}
					// however, if there's already more than one of the same singleton, we have a problem
					else if (FindObjectsOfType(typeof(T)).Length > 1) {
						Debug.LogError("[Singleton] You somehow have two Singletons of the same type in this scene. Unclear which one is desired, going with first one found.");
					}

					// make sure to preserve the newly set instance's heirarchy between scene loads
					DontDestroyOnLoad(INSTANCE.gameObject.transform.root);
				}
				return INSTANCE;
			}
		}
	}

	private static bool IS_QUITTING = false;
	/// <summary>
	/// When Unity quits, it destroys objects in a random order. 
	/// To avoid automatically creating a new instance once the old one was destroyed during quit, we set this flag in OnDestroy.
	/// </summary>
	public void OnDestroy() {
		IS_QUITTING = true;
	}
}