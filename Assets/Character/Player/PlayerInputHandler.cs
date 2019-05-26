using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerInputHandler : MonoBehaviour {

    CharacterController cont;

    [SerializeField] float speed;

    [SerializeField] [Range(0, 20)] float max_jump_height;
    [SerializeField] [Range(0, 20)] float min_jump_height;
    [SerializeField] [Range(0, 5)] float time_to_jump_apex;

    float gravity;
    float jump_velocity;
    float max_jump_hold;

    float gravity_force;

    Vector2 velocity;

    bool jumping;

    Coroutine drop_routine;

    private void Awake() {
        cont = GetComponent<CharacterController>();

        gravity = -(2 * min_jump_height) / (time_to_jump_apex * time_to_jump_apex);
        jump_velocity = time_to_jump_apex * Mathf.Abs(gravity);
        max_jump_hold = (max_jump_height - min_jump_height) / jump_velocity;

        velocity = Vector2.zero;
        gravity_force = 0;
    }

    private void Update() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
   
        Move(input);
    }

    void Move(Vector2 input) {
        if (cont.collisions.above || cont.collisions.below) {
            gravity_force = 0;
            velocity.y = 0;
        }

        Vector2 adjusted_input = input;

        if (!jumping) gravity_force += gravity * Time.deltaTime;

        if (true/*!player.is_knocked_back*/) {
            if (jumping) {
                velocity.y = jump_velocity;
            } else {
                HandleYInput(adjusted_input.y);
            }

            HandleXInput(adjusted_input.x);

            Debug.Log(velocity + " : " + gravity_force);
            Debug.Log((velocity + (gravity_force * Vector2.up)) * Time.deltaTime);
            cont.Move((velocity + (gravity_force * Vector2.up)) * Time.deltaTime);
        } else {
            HandleKnockedBackInput();
        }

        if (false/*player.is_knocked_back*/) {
            CheckCancelKnockback();
        }
    }

    void CheckCancelKnockback() {
        if (cont.collisions.below) {
            //player.CancelKnockBack();
        } else {
            if (cont.collisions.left || cont.collisions.right) {
                //player.CancelXKnockBack();
            }
            if (cont.collisions.above) {
                //player.CancelYKnockBack();
            }
        }
    }

    void HandleKnockedBackInput() {
        velocity.y = 0;
        //cont.Move(player.knockback_force + (gravity_force * GameManager.GetDeltaTime(player.team)));
        //player.knockback_force = Vector2.zero;
    }

    void HandleYInput(float y_input) {
        if (y_input <= -.5f && drop_routine == null && cont.OverPlatform() && Input.GetButton("Jump")) {
            if (cont.OverPlatform()) {
                drop_routine = StartCoroutine(DropRoutine());
            }
        } else if (drop_routine == null /*&& player.can_move*/ && Input.GetButtonDown("Drop")) {
            drop_routine = StartCoroutine(DropRoutine());
        } else if (/*player.can_move &&*/ Input.GetButtonDown("Jump")) {
            if (cont.collisions.below) {
                Jump();
            }
        }
    }

    void HandleXInput(float x_input) {
        velocity.x = x_input * speed;

        if (x_input != 0 && (cont.collisions.below || cont.collisions.below_last_frame)) {
            //player.animator.SetBool("Running", true);
            //if (player.animator.IsAnimInState("PlayerRun")) {
            //    player.animator.speed = Mathf.Abs(velocity.x / 5f);
            //} else {
            //    player.animator.speed = 1f;
            //}
        } else {
            //player.animator.SetBool("Running", false);
            //player.animator.speed = 1f;
        }

        //if (player.can_change_facing) {
        //    if (x_input < 0) {
        //        facing = -1;
        //        flip_object.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        //    } else if (x_input > 0) {
        //        facing = 1;
        //        flip_object.transform.localRotation = Quaternion.Euler(0, 0, 0);
        //    }
        //}
    }

    IEnumerator DropRoutine() {
        cont.RemovePlatformFromMask();
        float delay = .15f;
        while (delay > 0) {
            yield return new WaitForFixedUpdate();
            delay -= Time.deltaTime;
        }
        while (Input.GetButton("Drop")) {
            yield return new WaitForFixedUpdate();
        }
        cont.AddPlatformToMask();
        drop_routine = null;
    }

    void Jump() {
        gravity_force = 0;
        velocity.y = jump_velocity;
        StartCoroutine(JumpRoutine());
    }

    IEnumerator JumpRoutine() {
        jumping = true;
        float time_left = max_jump_hold;
        bool held = true;
        while (time_left > 0 && held && !cont.collisions.above) {
            time_left -= Time.deltaTime;
            held = held && Input.GetButton("Jump");
            yield return new WaitForFixedUpdate();
        }
        jumping = false;
    }
}
