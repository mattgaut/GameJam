using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Character))]
public abstract class EnemyHandler : StateMachineController {

    protected CharacterController cont;
    [SerializeField] bool active_on_start;
    [SerializeField] Transform line_of_sight_origin;
    [SerializeField] protected LayerMask line_of_sight_blocking_mask;

    [SerializeField] bool need_line_of_sight;

    [SerializeField] MultiHitAttack bump_hitbox;

    [SerializeField] float _aggro_range;

    [SerializeField] protected bool bump_damage;
    [SerializeField] protected Vector3 bump_knockback;

    float bump_cooldown = .5f;
    float last_bump;

    protected bool knocked_back_last_frame;

    protected Coroutine ai_routine;

    public float aggro_range { get { return _aggro_range; } }
    protected CharacterController.CollisionInfo collision_info { get { return cont.collisions; } }

    protected Character character { get; private set; }

    public Character target { get; set; }

    protected Vector2 _input;

    public Vector2 input { get { return _input; } set { _input = value; } }

    public bool can_tame { get { return tame_item != null && is_tamed == false; } }

    public bool is_tamed { get; private set; }

    Item tame_item;

    public int layer_attacking {
        get; private set;
    }

    public void AttemptTame(Item tame) {
        if (tame_item == null) tame_item = tame;
    }

    public bool CanHunt() {
        return target != null && CustomCanHuntTarget(target) && Vector2.Distance(target.transform.position, transform.position) <= aggro_range && (!need_line_of_sight || HasLineOfSight());
    }

    public bool CanHuntTarget(Character character) {
        return character != null && CustomCanHuntTarget(character) && Vector2.Distance(character.transform.position, transform.position) <= aggro_range && (!need_line_of_sight || HasLineOfSightTarget(character));
    }

    public virtual bool HasLineOfSight() {
        return HasLineOfSightTarget(target);
    }
    public virtual bool HasLineOfSightTarget(Character target) {
        CharacterDefinition target_definition = target.char_definition;
        RaycastHit2D hit = Physics2D.Linecast(line_of_sight_origin.position, target_definition.center_mass.position, line_of_sight_blocking_mask);
        if (hit) {
            hit = Physics2D.Linecast(line_of_sight_origin.position, target_definition.head.position, line_of_sight_blocking_mask);
            if (hit) {
                hit = Physics2D.Linecast(line_of_sight_origin.position, target_definition.feet.position, line_of_sight_blocking_mask);
            }
        }
        return !hit;
    }

    protected virtual bool CustomCanHuntTarget(Character target) {
        return true;
    }

    protected void ConfirmBump(Character player) {
        if (player.invincible) {
            last_bump = 0;
        } else {
            Bump(player);
        }
    }

    protected virtual void Bump(Character player) {
        character.DealDamage(BumpDamage(), player);
        if (bump_knockback != Vector3.zero) {
            Vector3 real_knockback = bump_knockback;
            if (player.transform.position.x < transform.position.x) {
                real_knockback.x *= -1;
            }
            character.GiveKnockback(player, real_knockback);
        }
        last_bump = 0;
    }

    protected override void Awake() {
        base.Awake();
        bump_hitbox.SetOnHit((a, b) => ConfirmBump(a));
        cont = GetComponent<CharacterController>();
        character = GetComponent<Character>();
    }

    protected IEnumerator Tame() {
        Item tame_item = this.tame_item;
        float difference = transform.position.x - tame_item.transform.position.x;
        while (tame_item != null && difference > 0.4f) {
            _input.x = -difference;
            _input.y = 0;
            _input = _input.normalized;
            
            yield return new WaitForFixedUpdate();
            if (tame_item == null) break;
            difference = transform.position.x - tame_item.transform.position.x;
        }
        _input = Vector2.zero;

        if (tame_item != null) {
            tame_item.is_taming = true;
            yield return new WaitForSeconds(5f);
            tame_item.is_taming = false;
        }

        if (tame_item != null && tame_item.TryTame(character)) {
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
            transform.localScale *= 1.5f;
            transform.localPosition += 0.5f * Vector3.up;
            bump_knockback *= 2f;
            is_tamed = true;
        }
        this.tame_item = null;
        _input.x = 0;
    }

    protected virtual void Start() {
        last_bump = bump_cooldown;
        Ini();        
        if (!target) {
            target = GameManager.instance.player;
        }
        SetActive(active_on_start);
        layer_attacking = LayerMask.NameToLayer("Player");
    }

    protected virtual void Ini() {}

    protected virtual float BumpDamage() {
        return 1;
    }

    protected override void Deactivate() {
        base.Deactivate();
        character.animator.Rebind();
        _input = Vector2.zero;
        character.health.current = character.health;
    }

    protected virtual void Update() {
        last_bump += Time.deltaTime;
    }
}
