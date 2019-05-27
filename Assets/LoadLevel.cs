using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour {

    [SerializeField] string scene;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (((1 << collision.gameObject.layer) & (1 << LayerMask.NameToLayer("Player"))) != 0) {
            GameManager.instance.LoadScene(scene);
        }
    }
}
