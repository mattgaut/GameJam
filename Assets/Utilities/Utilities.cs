using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities {

    public class Pair<T, U> {
        public Pair() {
        }

        [SerializeField] T _first;
        [SerializeField] U _second;

        public Pair(T first, U second) {
           _first = first;
           _second = second;
        }

        public T first { get { return _first; } set { _first = value; } }
        public U second { get { return _second; } set { _second = value; } }
    };

    public class Lock {
        HashSet<int> locks;

        int next_lock;

        public Lock() {
            locks = new HashSet<int>();
            next_lock = int.MinValue;
        }

        public bool locked {
            get { return locks.Count > 0; }
        }

        public bool unlocked {
            get { return locks.Count == 0; }
        }

        public void Clear() {
            locks.Clear();
        }

        public int AddLock() {
            int lock_value = next_lock++;
            locks.Add(lock_value);
            return lock_value;
        }

        public bool RemoveLock(int lock_value) {
            return locks.Remove(lock_value);
        }
    }

    public static class Utilities {
        public static void ThrowObject<T>(T obj, Transform spawn_point, bool should_instantiate_as_copy = true) where T : MonoBehaviour {
            if (should_instantiate_as_copy) {
                obj = GameObject.Instantiate(obj);
            }
            obj.transform.position = spawn_point.position;
            float angle = Random.Range(0f, 90f) - 45f;
            Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
            if (body == null) {
                body = obj.gameObject.AddComponent<Rigidbody2D>();
            }
            body.AddForce(Quaternion.Euler(0, 0, angle) * Vector2.up * Random.Range(8f, 20f), ForceMode2D.Impulse);
        }

        public static void ThrowObjects<T>(List<T> objs, Transform spawn_point, bool should_instantiate_as_copy = true) where T : MonoBehaviour {
            foreach (T obj in objs) {
                ThrowObject(obj, spawn_point, should_instantiate_as_copy);
            }
        }

        public static void ThrowObject(GameObject obj, Transform spawn_point, bool should_instantiate_as_copy = true) {
            if (should_instantiate_as_copy) {
                obj = GameObject.Instantiate(obj);
            }
            obj.transform.position = spawn_point.position;
            float angle = Random.Range(0f, 90f) - 45f;
            Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
            if (body == null) {
                body = obj.AddComponent<Rigidbody2D>();
            }
            body.AddForce(Quaternion.Euler(0, 0, angle) * Vector2.up * Random.Range(8f, 20f), ForceMode2D.Impulse);
        }

        public static void ThrowObjects(List<GameObject> objs, Transform spawn_point, bool should_instantiate_as_copy = true) {
            foreach (GameObject obj in objs) {
                ThrowObject(obj, spawn_point, should_instantiate_as_copy);
            }
        }
    }
}