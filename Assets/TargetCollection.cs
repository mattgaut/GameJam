using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollection : MonoBehaviour {
    [SerializeField] EnemyHandler enemy;
    [SerializeField] CircleCollider2D coll;

    List<Character> targets;

    int last_attacking = 0;

    private void Awake() {
        targets = new List<Character>();
        coll.radius = enemy.aggro_range;
    }

    private void Update() {
        if (enemy.target && enemy.target.gameObject.layer != enemy.layer_attacking) {
            targets.Remove(enemy.target);
            enemy.target = null;
        }
        last_attacking = enemy.layer_attacking;
        if (!enemy.CanHuntTarget(enemy.target)) {
            enemy.target = null;
        }
        if (enemy.target == null && targets.Count > 0) {
            targets.Shuffle();
            for (int i = 0; i < targets.Count; i++) {
                if (enemy.CanHuntTarget(targets[i])) {
                    enemy.target = targets[i];
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == enemy.layer_attacking) {
            targets.Add(collision.GetComponentInParent<Character>());
            Debug.Log("Add " + targets[targets.Count - 1].name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == enemy.layer_attacking) {
            targets.Remove(collision.GetComponentInParent<Character>());
        }
    }
}
