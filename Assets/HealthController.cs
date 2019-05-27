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
        if (parent.childCount != Mathf.CeilToInt(character.health)) {
            for (int i = 0;) {

            }
        }
    }
}
