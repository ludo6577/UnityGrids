using UnityEngine;


/// <summary>
/// Source: http://wiki.unity3d.com/index.php/Singleton
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
#if UNITY_EDITORR
public class Singleton<T> where T : new()
#else
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour, new()
#endif
{
    private static T _instance;

    private static object _lock = new object();

#if UNITY_EDITORR

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new T();
                    Debug.Log("[Singleton] An instance of " + typeof(T) +
                            " is needed in the Editor, so " +
                            " it was created with DontDestroyOnLoad.");
                    _instance.Init();
                }
                return _instance;
            }
        }
    }

#else

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogWarning("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopening the scene might fix it or delete it manually.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = typeof(T).ToString() + " (singleton)";

                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(singleton);

                            Debug.Log("[Singleton] An instance of " + typeof(T) +
                                      " is needed in the Scene, so '" + singleton +
                                      "' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            Debug.Log("[Singleton] An instance of " + typeof(T) +
                                      " is needed in the Editor, so '" + singleton +
                                      "' was created.");
                        }
                    }
                    else
                    {
                        if (Application.isPlaying)
                        {
                            Debug.Log("[Singleton] Using instance already created: " +
                                      _instance.gameObject.name +
                                      " setted DontDestroyOnLoad");
                            DontDestroyOnLoad(_instance);
                        }
                        else
                        {
                            Debug.Log("[Singleton] Using instance already created: " +
                                  _instance.gameObject.name);
                        }
                    }
                }

                return _instance;
            }
        }
    }

#endif
}