using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AerialEnemyHandler : EnemyHandler {

    protected bool auto_tilt = true;

    protected Vector2 velocity;

    [SerializeField] protected GameObject pivot_object;

    Vector2 smooth_damp_velocity;
    float smooth_damp_time = 1f;

    float min_hover_distance = 2f;
    float hover_distance = 2.5f;
    float max_hover_distance = 3.5f;

    float circle_speed = (1f / 15f); // Number of rotations per second
    float circle_angle;

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

    protected virtual IEnumerator StartCirclingTarget() {
        Vector3 target_position = -(target.char_definition.center_mass.position - transform.position);
        circle_angle = Vector2.Angle(target_position, Vector2.up);
        yield return null;
    }

    protected virtual IEnumerator CircleTarget() {

        Vector3 target_position = -(target.char_definition.center_mass.position - transform.position);
        Vector3 hover_distance_vector = new Vector3(0, hover_distance, 0);

        target_position = target.char_definition.center_mass.position +  (Quaternion.Euler(0, 0, circle_angle) * hover_distance_vector);

        _input = (target_position - transform.position).normalized;

        circle_angle += circle_speed * 360 * Time.fixedDeltaTime;
        circle_angle = circle_angle % 360;
        yield return new WaitForFixedUpdate();
    }

    protected override void Update() {
        base.Update();
        Move();
    }

    protected virtual void Move() {
        Vector3 movement = Vector3.zero;
        
        if (!character.is_knocked_back) {
            if (character.is_dashing) {
                movement = character.dash_force;
                if (movement != Vector3.zero && auto_tilt) {                   
                    Tilt((movement.x / Time.deltaTime) / character.speed);
                }
                character.dash_force = Vector2.zero;
            } else {
                velocity = Vector2.SmoothDamp(velocity, input * character.speed, ref smooth_damp_velocity, 0.5f);
                if (auto_tilt) Tilt(velocity.x / character.speed);
                movement = velocity * Time.deltaTime;
            }
            
        } else {
            if (auto_tilt) Tilt(character.knockback_force.magnitude / (Time.deltaTime));
            movement = character.knockback_force;
            character.knockback_force = Vector2.zero;
            velocity = movement;
        }

        cont.Move(movement);
    }

    protected void Tilt(float input) {
        pivot_object.transform.localRotation = Quaternion.Euler(0,0, -Mathf.Clamp(input, -1f, 1f) * 45f);
    }

    protected void LerpToUnclampedTilt(float angle_1, float angle_2, float time) {
        pivot_object.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(angle_1, angle_2, time));
    }
    protected void SetUnclampedTilt(float angle) {
        pivot_object.transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
