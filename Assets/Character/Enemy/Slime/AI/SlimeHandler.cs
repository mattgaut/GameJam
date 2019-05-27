using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeHandler : GroundedEnemyHandler {

    [SerializeField] float pounce_windup, hop_windup;

    [SerializeField] float pounce_range;
    [SerializeField] float hop_strength = 1f;

    public bool is_in_pounce_range { get {
            return target != null && Vector2.Distance(target.transform.position, transform.position) < pounce_range; } }

    IEnumerator Pounce() {
        if (this.target) {
            Transform target = this.target.transform;
            _input = Vector2.zero;            

            float length = 0f;
            bool cancel = false;
            character.animator.SetBool("Hopping", true);

            while (length < pounce_windup) {
                yield return null;
                if (character.is_knocked_back) {
                    cancel = true;
                    break;
                }
                length += Time.deltaTime;
            }
            character.animator.SetBool("Hopping", false);

            if (!cancel && target != null) {
                Vector2 dash_vector = Vector2.one * 4f * hop_strength;
                dash_vector.x *= Mathf.Sign(target.position.x - transform.position.x);
                character.GiveKnockback(character, dash_vector, 1f);

                while (character.is_knocked_back) {
                    yield return null;
                }
            }
        }
    }

    IEnumerator HopAtEnemy() {
        if (this.target) {
            Transform target = this.target.transform;
            _input = Vector2.zero;
            
            float length = 0f;
            bool cancel = false;
            character.animator.SetBool("Hopping", true);
            while (length < hop_windup) {
                yield return null;
                if (character.is_knocked_back) {
                    cancel = true;
                    break;
                }
                length += Time.deltaTime;
            }
            character.animator.SetBool("Hopping", false);


            if (!cancel && target != null) {
                Vector2 hop_vector = Vector2.one * Random.Range(0.75f, 2f);
                hop_vector.x *= Mathf.Sign(target.position.x - transform.position.x);
                hop_vector *= hop_strength;
                character.GiveKnockback(character, hop_vector, 0.5f);

                while (character.is_knocked_back) {
                    yield return null;
                }
            }
        }
    }

    IEnumerator HopAtPlayer() {
        Transform target = GameManager.instance.player.transform;

        if (Vector2.Distance(target.position, transform.position) > 10f) {
            transform.position = target.position + Vector3.up * 0.5f;
        }

        _input = Vector2.zero;

        float length = 0f;
        bool cancel = false;
        character.animator.SetBool("Hopping", true);       

        while (length < hop_windup) {
            yield return null;
            if (character.is_knocked_back) {
                cancel = true;
                break;
            }
            length += Time.deltaTime;
        }
        character.animator.SetBool("Hopping", false);


        if (!cancel) {
            Vector2 hop_vector = Vector2.one * Random.Range(0.75f, 2f);
            hop_vector.x *= Mathf.Sign(target.position.x - transform.position.x);
            hop_vector *= hop_strength;
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
        character.animator.SetBool("Hopping", true);

        Vector2 hop_vector = Vector2.one * Random.Range(0.75f, 2f);
        hop_vector.x *= Random.Range(-1f, 1f);
        hop_vector *= hop_strength;

        while (length < hop_windup) {
            yield return null;
            if (character.is_knocked_back) {
                cancel = true;
                break;
            }
            length += Time.deltaTime;
        }
        character.animator.SetBool("Hopping", false);

        if (!cancel) {
            character.GiveKnockback(character, hop_vector, 0.5f);

            while (character.is_knocked_back) {
                yield return null;
            }
        }
    }
}
