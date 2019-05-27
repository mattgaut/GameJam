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

    [SerializeField] float _aggro_range;

    [SerializeField] protected bool bump_damage;
    [SerializeField] protected Vector3 bump_knockback;

    float bump_cooldown = .5f;
    float last_bump;

    protected bool knocked_back_last_frame;

    protected Coroutine ai_routine;

    protected float aggro_range { get { return _aggro_range; } }
    protected CharacterController.CollisionInfo collision_info { get { return cont.collisions; } }

    protected Character character { get; private set; }

    protected Character target;

    protected Vector2 _input;

    public Vector2 input { get { return _input; } set { _input = value; } }

    public bool CanHunt() {
        return target != null && CustomCanHunt() && Vector2.Distance(target.transform.position, transform.position) <= aggro_range && (!need_line_of_sight || HasLineOfSight());
    }

    public virtual bool HasLineOfSight() {
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

    protected virtual bool CustomCanHunt() {
        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (bump_damage && collision.gameObject.layer == LayerMask.NameToLayer("Player") && last_bump > bump_cooldown) {
            ConfirmBump(collision.gameObject.GetComponentInParent<Character>());
        }
    }
    private void OnTriggerStay2D(Collider2D collision) {
        if (bump_damage && collision.gameObject.layer == LayerMask.NameToLayer("Player") && last_bump > bump_cooldown) {
            ConfirmBump(collision.gameObject.GetComponentInParent<Character>());
        }
    }

    protected void ConfirmBump(Character player) {
        Debug.Log("Bumpp");
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
        cont = GetComponent<CharacterController>();
        character = GetComponent<Character>();
    }

    protected virtual void Start() {
        last_bump = bump_cooldown;
        Ini();
        if (active_on_start) {
            target = FindObjectOfType<Character>();
        }
        if (!target) {
            target = FindObjectOfType<Character>();
        }
        SetActive(active_on_start);
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
