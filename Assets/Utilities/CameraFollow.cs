using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField] Rect bounds;
    [SerializeField] GameObject to_follow;
    [SerializeField] bool grab_player;

    private void Start() {
        if (grab_player) {
            to_follow = GameManager.instance.player.gameObject;
        }
    }

    private void LateUpdate() {
        if (to_follow) {
            Vector3 v = to_follow.transform.position + (Vector3.back * 10);
            v.z = -10f;
            v.y = Mathf.Clamp(v.y, bounds.min.y, bounds.max.y);
            v.x = Mathf.Clamp(v.x, bounds.min.x, bounds.max.x);

            transform.position = v;
        }
    }
}
