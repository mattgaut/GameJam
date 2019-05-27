using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeHandler : GroundedEnemyHandler {

    [SerializeField] float charge_windup;

    IEnumerator Pounce() {
        _input = Vector2.zero;

        float length = 0f;
        while (length < charge_windup) {
            yield return null;
            length += 0;
        }


    }
}
