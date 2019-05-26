using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

    static T _instance;
    public static bool has_instance {
        get { return _instance != null; }
    }
    public static T instance {
        get {
            if (_instance == null)
                Debug.LogError("Singleton of type " + typeof(T) + " not loaded.");
            return _instance;
        }
        set { _instance = value; }
    }


    void Awake() {
        if (_instance == null) {            
            instance = GetComponent<T>();
            DontDestroyOnLoad(instance.transform.root);
            OnAwake();
        } else {
            Debug.Log("Destroy " + name + " : " + typeof(T));
            Destroy(gameObject);
        }
    }

    protected virtual void OnAwake() { }
}
