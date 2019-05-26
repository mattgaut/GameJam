using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiHitAttack : Attack {

    [SerializeField] float time_between_hits;

    protected override bool HitCondition(Character d) {
        if (hit_objects.ContainsKey(d)) {
            return timer - hit_objects[d] > time_between_hits;
        } else {
            return true;
        }
    }
}
