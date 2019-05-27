using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {

    void Awake() {
        GameManager.instance.AddDroppedObjects(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
            if (collision.GetComponentInParent<Character>().alive) {
                collision.GetComponentInParent<Character>().AddCoins(1);
                GameManager.instance.RemoveDroppedObjects(gameObject);
                Destroy(gameObject);
            }
        }
    }
}
