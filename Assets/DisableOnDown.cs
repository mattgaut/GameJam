using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnDown : MonoBehaviour {

    bool in_in = false;
    [SerializeField] Collider2D coll;
    private void OnTriggerEnter2D(Collider2D other) {
        if (((1 << other.gameObject.layer) & (1 << LayerMask.NameToLayer("Player"))) != 0) {
            PlayerInputHandler input = other.transform.root.GetComponent<PlayerInputHandler>();
            if (input) {
                in_in = true;
                input.on_drop += Drop;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (((1 << other.gameObject.layer) & (1 << LayerMask.NameToLayer("Player"))) != 0) {
            PlayerInputHandler input = other.transform.root.GetComponent<PlayerInputHandler>();
            if (input) {
                in_in = false;
                input.on_drop -= Drop;
            }
        }
    }


    void Drop() {
        coll.enabled = false;

        Invoke("EndDrop", 0.25f);
    }

    void EndDrop() {
        coll.enabled = true;
    }

    private void OnDisable() {
        if (in_in) GameManager.instance.player.GetComponent<PlayerInputHandler>().on_drop -= Drop;
    }
}
