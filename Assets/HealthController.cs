using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour {

    [SerializeField] Character character;

    [SerializeField] GameObject prefab;

    [SerializeField] RectTransform parent;

    void Start() {

    }

    private void Update() {
        int count = Mathf.RoundToInt(character.health.current);
        if (parent.childCount > count) {
            for (int i = parent.childCount - 1; i >= count; i--) {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
        if (parent.childCount < count) {
            for (int i = parent.childCount; i < count; i++) {
                Instantiate(prefab, parent);
            }
        }
    }
}
