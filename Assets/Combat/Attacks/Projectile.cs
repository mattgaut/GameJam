using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : SingleHitAttack {

    [SerializeField] protected LayerMask break_mask, break_after_timer_mask;
    [SerializeField] protected float speed, ignore_wall_timer, max_lifetime;

    [SerializeField] protected float gravity_force;

    [SerializeField] protected Vector3 base_direction;

    [SerializeField] ParticleSystem particles;

    [SerializeField] SFXInfo explode_sfx;

    Animator anim;
    Rigidbody2D rb;
    bool is_exploded;

    Vector3 gravity_vector;

    public void SetSpeedAndDirection(Vector2 force) {
        speed = force.magnitude;
        base_direction = force.normalized;
    }

    public override void SetSource(Character source) {
        base.SetSource(source);
    }

    public void Flip() {
        base_direction.x = -base_direction.x;
    }

    public void EndLife() {
        Destroy(gameObject);
    }

    public bool CanLaunchTowardsTarget(Vector2 distance) {
        if (gravity_force == 0) {
            return true;
        } else {
            double x_distance = distance.x;
            double y_distance = distance.y;

            double under_root = Math.Pow(speed, 4) - gravity_force * ((gravity_force * Math.Pow(x_distance, 2)) + (2 * y_distance * Math.Pow(speed, 2)));
            double angle = Math.Atan((Math.Pow(speed, 2) + Math.Pow(under_root, 0.5)) / (gravity_force * x_distance));

            return !double.IsNaN(angle);
        }
    }

    public bool LaunchTowardsTarget(Vector2 distance) {
        if (gravity_force == 0) {
            base_direction = Quaternion.FromToRotation(base_direction, distance) * base_direction;
            return true;
        } else {
            double x_distance = distance.x;
            double y_distance = distance.y;

            double under_root = Math.Pow(speed, 4) - gravity_force * ((gravity_force * Math.Pow(x_distance, 2)) + (2 * y_distance * Math.Pow(speed, 2)));

            double angle = Math.Atan((Math.Pow(speed, 2) + Math.Pow(under_root, 0.5)) / (gravity_force * x_distance));
            if (double.IsNaN(angle)) {
                return false;
            }

            double angle2 = Math.Atan((Math.Pow(speed, 2) - Math.Pow(under_root, 0.5)) / (gravity_force * x_distance));

            angle = Math.Max(angle, angle2);
            angle *= (180 / Math.PI);
        
            base_direction = Quaternion.Euler(0,0, (float)angle) * Vector2.right;

            return true;
        }
    }

    protected override void Awake() {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        is_exploded = false;
        base_direction = base_direction.normalized;
    }

    protected override void Update() {
        if (is_exploded) {
            return;
        }
        base.Update();

        float time_step = Time.deltaTime;

        if (ignore_wall_timer >= 0) {
            ignore_wall_timer -= time_step;
            if (ignore_wall_timer < 0) {
                break_mask = break_after_timer_mask | break_mask;
            }
        }
        if (timer > max_lifetime && max_lifetime != 0) {
            Explode();
        }

        Turn();

        transform.position += transform.localRotation * base_direction * speed * time_step;
        transform.position += gravity_vector * time_step;

        gravity_vector += Vector3.down * gravity_force * time_step;
    }

    protected virtual void Turn() {

    }

    protected override void CheckHitboxCollisions(Collider2D collision) {
        base.CheckHitboxCollisions(collision);
        if (((1 << collision.gameObject.layer & break_mask) != 0)) {
            Explode();
        }
    }

    protected override void OnCollisionWithTarget() {
        base.OnCollisionWithTarget();
        Explode();
    }

    protected virtual void Explode() {
        speed = 0;
        SoundManager.instance?.PlaySfx(explode_sfx);
        if (particles) {
            particles.Stop();
            particles.gameObject.transform.SetParent(null);
        }
        if (anim != null && anim.enabled) {
            anim.SetTrigger("Explode");
        } else {
            EndLife();
        }
        is_exploded = true;

        hitbox.enabled = false;
    }

    protected virtual void OnChangeTimeScale(float time_scale) {
        if (anim != null) anim.speed = time_scale;
        if (particles != null) {
            ParticleSystem.MainModule main = particles.main;
            main.simulationSpeed = time_scale;
        }
    }
}
