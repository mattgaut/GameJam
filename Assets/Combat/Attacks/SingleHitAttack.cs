using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleHitAttack : Attack {
    protected override bool HitCondition(Character d) {
        return !hit_objects.ContainsKey(d);
    }
}
