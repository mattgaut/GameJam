using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedEnemyHandler : EnemyHandler, IInputHandler {

    [SerializeField] GameObject flip_object;
    [SerializeField] [Range(-1, 1)] int base_facing;
    
    /*[SerializeField] [Range(0, 20)] */float jump_height = 1.6f;
    /*[SerializeField] [Range(0, 5)] */float time_to_jump_apex = 0.24f;
    [SerializeField] bool no_gravity;

    protected bool can_flip = true;

    public int facing { get; private set; }

    protected float max_x_movement_per_tick {
        get { return character.speed * Time.deltaTime; }
    }

    bool frozen_last_frame, frozen_last_last_frame;

    float gravity;
    float jump_velocity;

    Vector2 gravity_force;
    Vector2 velocity;

    Coroutine drop_routine;

    public event Action<bool> on_jump;
    public event Action on_land;

    protected void Face(float i) {
        if (!can_flip) return;
        if (i * base_facing < 0) {
            flip_object.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            facing = 1;
        } else if (i * base_facing > 0) {
            flip_object.transform.localRotation = Quaternion.Euler(0, 0, 0);
            facing = -1;
        }
    }

    protected bool ShouldStopMoving(int direction) {
        return ShouldStopMoving((float)direction);
    }
    protected virtual bool ShouldStopMoving(float direction) {
        return collision_info.past_max;
    }

    protected override void Ini() {
        base.Ini();

        gravity = -(2 * jump_height) / (time_to_jump_apex * time_to_jump_apex);
        if (no_gravity) gravity = 0;
        jump_velocity = time_to_jump_apex * Mathf.Abs(gravity);

        if (flip_object.transform.localRotation.eulerAngles.y == 180) {
            facing = -base_facing;
        } else {
            facing = base_facing;
        }

        character.on_take_knockback += (a, b, c) => gravity_force.y = 0;
    }

    protected override void Update() {
        base.Update();
        Move();
    }

    protected override void Deactivate() {
        base.Deactivate();
        can_flip = true;
    }

    protected IEnumerator Wander() {
        float timer = 0;
        while (!CanHunt()) {
            yield return null;
            timer += Time.deltaTime;
            if (timer > 0) {
                _input = new Vector2(UnityEngine.Random.Range(-1, 2), 0);
                timer = UnityEngine.Random.Range(-4f, -0.5f);
            }
        }
    }

    protected IEnumerator MoveTo(Transform target_transform) {
        float difference = target_transform.position.x - transform.position.x;
        while (Mathf.Abs(difference) > max_x_movement_per_tick) {
            _input.x = Mathf.Sign(difference);
            yield return new WaitForFixedUpdate();
            difference = target_transform.transform.position.x - transform.position.x;
        }
        _input.x = difference / max_x_movement_per_tick;
        yield return new WaitForFixedUpdate();
        _input.x = 0;
    }

    protected IEnumerator MoveTo(Vector2 position) {
        float difference = position.x - transform.position.x;
        while (Mathf.Abs(difference) > max_x_movement_per_tick) {
            _input.x = Mathf.Sign(difference);
            yield return new WaitForFixedUpdate();
            difference = position.x - transform.position.x;
        }
        _input.x = difference / max_x_movement_per_tick;
        yield return new WaitForFixedUpdate();
        _input.x = 0;
    }

    protected IEnumerator MoveTo(Transform target_transform, float horizontal_distance) {
        float difference = target_transform.position.x - transform.position.x;
        while (Mathf.Abs(difference) > horizontal_distance) {
            _input.x = Mathf.Sign(difference);
            yield return new WaitForFixedUpdate();
            difference = target_transform.transform.position.x - transform.position.x;
        }
        _input.x = 0;
    }

    void Move() {
        Vector2 movement = Vector2.zero;
        if (cont.collisions.above || cont.collisions.below) {
            velocity.y = 0;
            gravity_force.y = 0;
        }
        gravity_force.y += gravity * Time.deltaTime;

        if (character.is_knocked_back) {
            velocity.y = 0;
            movement = character.knockback_force + (gravity_force * Time.deltaTime);
            character.knockback_force = Vector2.zero;
        } else {
            if (character.can_move) {
                if (_input.y > 0 && cont.collisions.below) {
                    velocity.y = jump_velocity;
                    on_jump?.Invoke(true);
                }
                if (_input.y < 0 && drop_routine == null) {
                    drop_routine = StartCoroutine(DropRoutine());
                }

                velocity.x = _input.x;
                movement = ((velocity * character.speed) + gravity_force) * Time.deltaTime;
                Face(movement.x);
            }
        }

        cont.Move(movement);

        if (character.is_knocked_back && (cont.collisions.left || cont.collisions.right)) {
            character.CancelXKnockBack();
        }
        if (character.is_knocked_back && cont.collisions.above) {
            character.CancelYKnockBack();
        }

        if (character.is_knocked_back && cont.collisions.below && !frozen_last_last_frame) {
            character.CancelKnockBack();
        }
    }

    IEnumerator DropRoutine() {
        cont.RemovePlatformFromMask();
        float delay = .15f;
        while (delay > 0) {
            yield return new WaitForFixedUpdate();
            delay -= Time.fixedDeltaTime;
        }
        while (input.y < 0) {
            yield return new WaitForFixedUpdate();
        }
        cont.AddPlatformToMask();
        drop_routine = null;
    }
}
