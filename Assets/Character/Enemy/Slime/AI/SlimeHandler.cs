using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeHandler : GroundedEnemyHandler {

    [SerializeField] float pounce_windup, hop_windup;

    [SerializeField] float pounce_range;
    [SerializeField] float hop_strength = 1f;

    [SerializeField] float vomit_cd, vomit_windup;
    [SerializeField] Projectile vomit_projectile;
    [SerializeField] Vector2 projectile_knockback;
    float last_vomit;

    public bool can_vomit {
        get { return last_vomit > vomit_cd; }
    }

    public bool is_in_pounce_range { get {
            return target != null && Vector2.Distance(target.transform.position, transform.position) < pounce_range; } }

    IEnumerator Pounce() {
        if (this.target) {
            Transform target = this.target.transform;
            _input = Vector2.zero;            

            float length = 0f;
            bool cancel = false;
            character.animator.SetBool("Hopping", false);

            while (length < pounce_windup) {
                yield return null;
                if (character.is_knocked_back) {
                    cancel = true;
                    break;
                }
                length += Time.deltaTime;
            }
            character.animator.SetBool("Hopping", true);

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

    IEnumerator Vomit() {
        float length = vomit_windup;

        _input = Vector2.zero;

        character.animator.SetBool("Hopping", false);
        yield return new WaitForSeconds(vomit_windup);
        character.animator.SetBool("Hopping", true);

        int count = Random.Range(4, 7);
        for (int i = 0; i < count; i++) {

            Projectile new_projectile = Instantiate(vomit_projectile);
            if (is_tamed) {
                new_projectile.SetTargets(1 << LayerMask.NameToLayer("Enemy"));
                new_projectile.gameObject.layer = LayerMask.NameToLayer("PlayerAttack");
                new_projectile.GetComponent<SpriteRenderer>().color = Color.green;
            }
            new_projectile.SetOnHit(OnHit);
            new_projectile.SetSource(character);
            new_projectile.transform.position = transform.position + Vector3.up * 0.25f;

            new_projectile.SetSpeedAndDirection(Quaternion.Euler(0, Random.Range(0f, 90f), 0) * Vector2.one * Random.Range(6f, 8f));
            if (Random.Range(0f, 1f) > 0.5f) {
                new_projectile.Flip();
            }
        }

        last_vomit = Random.Range(-2f, 2f);

        yield return new WaitForSeconds(1f);

    }

    void OnHit(Character character, Attack attack) {
        character.TakeDamage(1, this.character);
        Vector2 real_knockback = projectile_knockback;
        real_knockback.x *= Mathf.Sign(character.transform.position.x - attack.transform.position.x);
        character.TakeKnockback(this.character, real_knockback);
    }

    IEnumerator HopAtEnemy() {
        if (this.target) {
            Transform target = this.target.transform;
            _input = Vector2.zero;
            
            float length = 0f;
            bool cancel = false;
            character.animator.SetBool("Hopping", false);
            while (length < hop_windup) {
                yield return null;
                if (character.is_knocked_back) {
                    cancel = true;
                    break;
                }
                length += Time.deltaTime;
            }
            character.animator.SetBool("Hopping", true);


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

        if (is_tamed && Vector2.Distance(target.position, transform.position) > 10f) {
            transform.position = target.position + Vector3.up * 0.5f;
        }

        _input = Vector2.zero;

        float length = 0f;
        bool cancel = false;
        character.animator.SetBool("Hopping", false);       

        while (length < hop_windup) {
            yield return null;
            if (character.is_knocked_back) {
                cancel = true;
                break;
            }
            length += Time.deltaTime;
        }
        character.animator.SetBool("Hopping", true);


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

    protected override void Update() {
        base.Update();
        last_vomit += Time.deltaTime;
    }

    IEnumerator RandomHop() {
        _input = Vector2.zero;

        float length = 0f;
        bool cancel = false;
        character.animator.SetBool("Hopping", false);

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
        character.animator.SetBool("Hopping", true);

        if (!cancel) {
            character.GiveKnockback(character, hop_vector, 0.5f);

            while (character.is_knocked_back) {
                yield return null;
            }
        }
    }
}
