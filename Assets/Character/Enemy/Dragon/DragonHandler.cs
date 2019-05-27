using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DragonHandler : AerialEnemyHandler {

    public bool can_fireball { get { return last_used > cooldown; } }

    [SerializeField] float cooldown;
    [SerializeField] float wind_up_time;
    [SerializeField] Vector2 knockback;
    float last_used = 0f;

    [SerializeField] Projectile prefab;


    protected override void Update() {
        base.Update();
        last_used += Time.deltaTime;
    }

    IEnumerator Fireball() {
        if (target != null) {
            Character target = this.target;

            _input = Vector2.zero;

            yield return new WaitForSeconds(wind_up_time);

            if (target != null) {

                Projectile new_projectile = Instantiate(prefab);
                if (is_tamed) {
                    new_projectile.SetTargets(1 << LayerMask.NameToLayer("Enemy"));
                    new_projectile.gameObject.layer = LayerMask.NameToLayer("PlayerAttack");
                    new_projectile.GetComponent<SpriteRenderer>().color = Color.green;
                }
                new_projectile.SetOnHit(OnHit);
                new_projectile.SetSource(character);
                new_projectile.transform.position = transform.position;

                new_projectile.LaunchTowardsTarget(target.char_definition.center_mass.position - transform.position);
                last_used = Random.Range(-3f, 2f);
            }
        }
    }

    protected override IEnumerator Tame() {
        Item tame_item = this.tame_item;
        float difference = transform.position.x - tame_item.transform.position.x;
        float y_difference = transform.position.y - tame_item.transform.position.y;
        while (tame_item != null && difference > 0.4f) {
            _input.x = -difference;
            _input.y = -y_difference;

            _input = _input.normalized;

            yield return new WaitForFixedUpdate();
            if (tame_item == null) break;
            difference = transform.position.x - tame_item.transform.position.x;
            y_difference = transform.position.y - tame_item.transform.position.y;
        }
        _input = Vector2.zero;

        if (tame_item != null) {
            tame_item.is_taming = true;
            yield return new WaitForSeconds(5f);
            tame_item.is_taming = false;
        }

        if (tame_item != null && !tame_item.used && tame_item.TryTame(character)) {
            foreach (Collider2D coll in GetComponentsInChildren<Collider2D>()) {
                if (coll.gameObject.layer == LayerMask.NameToLayer("EnemyAttack")) {
                    coll.gameObject.layer = LayerMask.NameToLayer("PlayerAttack");
                } else if (coll.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
                    coll.gameObject.layer = LayerMask.NameToLayer("Pet");
                }
            }
            gameObject.layer = LayerMask.NameToLayer("Pet");
            layer_attacking = LayerMask.NameToLayer("Enemy");
            bump_hitbox.SetTargets(1 << layer_attacking);

            GameManager.instance.player.health.AddBuff(new HealthBuff());
            transform.localScale *= 1.5f;
            transform.localPosition += 0.5f * Vector3.up;
            bump_knockback *= 2f;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += (a, b) => transform.position = GameManager.instance.transform.position + Vector3.up * 0.5f;
            is_tamed = true;
        }
        this.tame_item = null;
        _input.x = 0;
    }

    void OnHit(Character character, Attack attack) {
        character.TakeDamage(1, this.character);
        Vector2 real_knockback = knockback;
        real_knockback.x *= Mathf.Sign(character.transform.position.x - attack.transform.position.x);
        character.TakeKnockback(this.character, real_knockback);
    }

}
