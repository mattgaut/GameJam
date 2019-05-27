using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnEnter : MonoBehaviour {
    [SerializeField] Collider2D coll;

    private void OnTriggerEnter2D(Collider2D other) {
        if (((1 << other.gameObject.layer) & (1 << LayerMask.NameToLayer("Player"))) != 0) {
            coll.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (((1 << other.gameObject.layer) & (1 << LayerMask.NameToLayer("Player"))) != 0) {
            coll.enabled = true;
        }
    }
}
