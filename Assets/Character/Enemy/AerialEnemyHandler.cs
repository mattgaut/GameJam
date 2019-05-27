using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AerialEnemyHandler : EnemyHandler {
    protected Vector2 velocity;

    [SerializeField] protected GameObject flip_object;
    [SerializeField] [Range(-1, 1)] int base_facing;
    [SerializeField] protected float x_close;

    public int facing { get; private set; }

    protected bool can_flip = true;

    Vector2 smooth_damp_velocity;
    float smooth_damp_time = 1f;

    float min_hover_distance = 2f;
    float hover_height = 2.5f;
    float max_hover_distance = 3.5f;

    float circle_speed = (1f);

    protected virtual IEnumerator Wander() {
        Vector2 direction = Vector2.zero;
        if (Random.Range(0f, 1f) < 0.5f) {
            float max_angle = 360, min_angle = 0;
            direction = Vector2.up;
            if (collision_info.left) {
                if (collision_info.above) {
                    max_angle = 180;
                    min_angle = 90;
                } else if (collision_info.below) {
                    max_angle = 90;
                } else {
                    max_angle = 180;
                }
            } else if (collision_info.right) {
                if (collision_info.above) {
                    max_angle = 270;
                    min_angle = 180;
                } else if (collision_info.below) {
                    min_angle = 270;
                } else {
                    min_angle = 180;
                }
            } else {
                if (collision_info.above) {
                    max_angle = 270;
                    min_angle = 90;
                } else if (collision_info.below) {
                    min_angle = -90;
                    max_angle = 90;
                }
            }
            direction = Quaternion.Euler(0, 0, -Random.Range(min_angle, max_angle)) * direction;
        }

        float wander_length = Random.Range(0.5f, 2f);
        while (wander_length > 0) {
            wander_length -= Time.fixedDeltaTime;
            _input = direction;
            if (CanHunt()) {
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        _input = Vector2.zero;
    }

    protected override void Ini() {
        base.Ini();
        hover_height = Random.Range(0, 3f);
        circle_speed *= Mathf.Sign(Random.Range(-1f, 1f));
    }

    protected override void Update() {
        base.Update();
        hover_height += circle_speed * Time.deltaTime;
        if (hover_height > 3f) {
            circle_speed = -1f * Mathf.Abs(circle_speed);
        } else if(hover_height < 0.25f) {
            circle_speed = Mathf.Abs(circle_speed);
        }
        Move();
    }

    protected virtual IEnumerator CircleTarget() {

        if (target != null) {
            Vector3 target_position = -(target.char_definition.center_mass.position - transform.position);
            Vector3 hover_distance_vector = new Vector3(0, hover_height, 0);

            target_position = target.char_definition.center_mass.position + hover_distance_vector;

            if (Mathf.Abs(target_position.x) < x_close) {
                target_position.x = 0;
            }

            _input = (target_position - transform.position).normalized;

            yield return new WaitForFixedUpdate();
        }
    }

    protected virtual IEnumerator CirclePlayer() {
        Character target = GameManager.instance.player.character;

        if (Vector2.Distance(target.transform.position, transform.position) > 12f) {
            transform.position = target.transform.position + Vector3.up * 1f;
        }

        Vector3 target_position = -(target.char_definition.center_mass.position - transform.position);
        Vector3 hover_distance_vector = new Vector3(0, hover_height, 0);

        target_position = target.char_definition.center_mass.position + hover_distance_vector;

        if (Mathf.Abs(target_position.x) < x_close) {
            target_position.x = 0;
        }

        _input = (target_position - transform.position).normalized;

        yield return new WaitForFixedUpdate();
    }

    protected virtual void Move() {
        Vector3 movement = Vector3.zero;
        
        if (!character.is_knocked_back) {
            if (character.is_dashing) {
                movement = character.dash_force;
                character.dash_force = Vector2.zero;
                Face(movement.x);
            } else {
                velocity = Vector2.SmoothDamp(velocity, input * character.speed, ref smooth_damp_velocity, 0.5f);
                movement = velocity * Time.deltaTime;

                Face(movement.x);
            }
            
        } else {
            movement = character.knockback_force;
            character.knockback_force = Vector2.zero;
            velocity = movement;

            Face(movement.x);
        }

        cont.Move(movement);
    }

    protected void Face(float i) {
        Debug.Log(0);
        if (!can_flip || i == 0) return;
        if (i * base_facing < 0) {
            flip_object.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            facing = 1;
        } else if (i * base_facing > 0) {
            flip_object.transform.localRotation = Quaternion.Euler(0, 0, 0);
            facing = -1;
        }
    }
}
