using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour {

    [SerializeField] MultiHitAttack multi_hit_attack;
    [SerializeField] Vector2 knockback;

    private void Awake() {
        multi_hit_attack.SetOnHit(OnHit);
    }

    void OnHit(Character hit, Attack attack) {
        Vector2 real_knockback = knockback;
        real_knockback.x *= Mathf.Sign(hit.transform.position.x - attack.transform.position.x);
        hit.TakeDamage(1, null);
        hit.TakeKnockback(null, real_knockback);
    }
}
