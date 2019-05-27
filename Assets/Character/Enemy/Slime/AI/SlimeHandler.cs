using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeHandler : GroundedEnemyHandler {

    [SerializeField] float pounce_windup, hop_windup;

    [SerializeField] float pounce_range;

    public bool is_in_pounce_range { get { return Vector2.Distance(target.transform.position, transform.position) < pounce_range; } }

    IEnumerator Pounce() {
        _input = Vector2.zero;

        float length = 0f;
        bool cancel = false;
        while (length < pounce_windup) {
            yield return null;
            if (character.is_knocked_back) {
                cancel = true;
                break;
            }
            length += Time.deltaTime;
        }
        if (!cancel) {
            Vector2 dash_vector = Vector2.one * 4f;
            dash_vector.x *= Mathf.Sign(target.transform.position.x - transform.position.x);
            character.GiveKnockback(character, dash_vector, 1f);

            while (character.is_knocked_back) {
                yield return null;
            }
        }        
    }

    IEnumerator HopAtEnemy() {
        _input = Vector2.zero;

        float length = 0f;
        bool cancel = false;
        while (length < hop_windup) {
            yield return null;
            if (character.is_knocked_back) {
                cancel = true;
                break;
            }
            length += Time.deltaTime;
        }

        if (!cancel) {
            Vector2 hop_vector = Vector2.one * Random.Range(0.75f, 2f);
            hop_vector.x *= Mathf.Sign(target.transform.position.x - transform.position.x);
            character.GiveKnockback(character, hop_vector, 0.5f);

            while (character.is_knocked_back) {
                yield return null;
            }
        }
    }

    IEnumerator RandomHop() {
        _input = Vector2.zero;

        float length = 0f;
        bool cancel = false;
        while (length < hop_windup) {
            yield return null;
            if (character.is_knocked_back) {
                cancel = true;
                break;
            }
            length += Time.deltaTime;
        }

        if (!cancel) {
            Vector2 hop_vector = Vector2.one * Random.Range(0.75f, 2f);
            hop_vector.x *= Random.Range(-1f, 1f);
            character.GiveKnockback(character, hop_vector, 0.5f);

            while (character.is_knocked_back) {
                yield return null;
            }
        }
    }
}
