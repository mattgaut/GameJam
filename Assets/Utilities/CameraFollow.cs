using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField] GameObject to_follow;

    private void LateUpdate() {
        if (to_follow) transform.position = to_follow.transform.position + (Vector3.back * 10);
    }
}
