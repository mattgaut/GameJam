using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {

    SFXInfo info = new SFXInfo("sfx_coin");

    void Awake() {
        GameManager.instance.AddDroppedObjects(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
            if (collision.GetComponentInParent<Character>().alive) {
                collision.GetComponentInParent<Character>().AddCoins(1);
                GameManager.instance.RemoveDroppedObjects(gameObject);
                SoundManager.instance.PlaySfx(info);
                Destroy(gameObject);
            }
        }
    }
}
