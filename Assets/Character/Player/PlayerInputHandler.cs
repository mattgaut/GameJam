using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerInputHandler : MonoBehaviour {

    public event Action on_drop;

    CharacterController cont;

    [SerializeField] GameObject flip_object;

    [SerializeField] [Range(0, 20)] float max_jump_height;
    [SerializeField] [Range(0, 20)] float min_jump_height;
    [SerializeField] [Range(0, 5)] float time_to_jump_apex;

    [SerializeField] Character player;

    [SerializeField] SoundEffects sfxs;

    [SerializeField] AudioSource footsteps;

    float gravity;
    float jump_velocity;
    float max_jump_hold;

    float gravity_force;

    Vector2 velocity;

    bool jumping;
    bool grounded_jump_used;

    int jumps_used;

    int facing;

    Coroutine drop_routine;

    private void Awake() {
        cont = GetComponent<CharacterController>();

        gravity = -(2 * min_jump_height) / (time_to_jump_apex * time_to_jump_apex);
        jump_velocity = time_to_jump_apex * Mathf.Abs(gravity);
        max_jump_hold = (max_jump_height - min_jump_height) / jump_velocity;

        velocity = Vector2.zero;
        gravity_force = 0;
    }

    private void Start() {
        footsteps.clip = sfxs.walking.clip.clip;
        footsteps.volume = sfxs.walking.clip.volume;
    }

    private void Update() {
        if (Input.GetButtonDown("Attack")) {
            player.Dash(new Vector2(1 * facing, 0), 0.05f);
            SoundManager.instance.PlaySfx(sfxs.slash);
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
   
        Move(input);
    }

    void Move(Vector2 input) {
        if (cont.collisions.above || cont.collisions.below) {
            gravity_force = 0;
            velocity.y = 0;
        }
        if (cont.collisions.below) {
            jumps_used = 0;
            grounded_jump_used = false;
        }

        Vector2 adjusted_input = input;
        if (!player.can_input || !player.can_move) {
            adjusted_input = Vector2.zero;
        }

        if (!jumping) gravity_force += gravity * Time.deltaTime;

        if (player.CheckCancelVelocityFlag()) {
            velocity = Vector3.zero;
            jumping = false;
            gravity_force = 0;
            cont.Move((velocity + gravity_force * Vector2.up) * Time.deltaTime);
        } else if(!player.is_knocked_back) {
            if (player.is_dashing) {
                HandleDashingInput();
            } else {
                if (jumping) {
                    velocity.y = jump_velocity;
                } else if (player.can_input) {
                    HandleYInput(adjusted_input.y);
                }

                HandleXInput(adjusted_input.x);

                cont.Move((velocity + gravity_force * Vector2.up) * Time.deltaTime);
            }
        } else {
            HandleKnockedBackInput();
        }

        if (player.is_knocked_back) {
            CheckCancelKnockback();
        }
    }

    void CheckCancelKnockback() {
        if (cont.collisions.below) {
            player.CancelKnockBack();
        } else {
            if (cont.collisions.left || cont.collisions.right) {
                player.CancelXKnockBack();
            }
            if (cont.collisions.above) {
                player.CancelYKnockBack();
            }
        }
    }

    void HandleDashingInput() {
        if (player.animator) {
            player.animator.SetBool("Running", false);
            player.animator.speed = 1f;
        }

        cont.Move(player.dash_force);
        player.dash_force = Vector2.zero;
        velocity = Vector3.zero;
        gravity_force = 0;
    }

    void HandleKnockedBackInput() {
        velocity.y = 0;
        cont.Move(player.knockback_force + (gravity_force * Vector2.up * Time.deltaTime));
        player.knockback_force = Vector2.zero;
    }

    void HandleYInput(float y_input) {
        if (y_input <= -.5f && drop_routine == null && cont.OverPlatform() && Input.GetButton("Jump")) {
            if (cont.OverPlatform()) {
                drop_routine = StartCoroutine(DropRoutine());
            } else {
                on_drop?.Invoke();
            }
        } else if (drop_routine == null && player.can_move && Input.GetButtonDown("Drop")) {
            drop_routine = StartCoroutine(DropRoutine());
        } else if (player.can_move && Input.GetButtonDown("Jump")) {
            if (cont.collisions.below) {
                Jump(true);
            } else {
                Jump(false);
            }
        }
    }

    void HandleXInput(float x_input) {
        velocity.x = x_input * player.speed;


        if (x_input != 0 && (cont.collisions.below || cont.collisions.below_last_frame)) {
            if (!footsteps.isPlaying) footsteps.Play();
            if (player.animator) {
                player.animator.SetBool("Running", true);
                if (player.animator.IsAnimInState("PlayerRun")) {
                    player.animator.speed = Mathf.Abs(velocity.x / 5f);
                } else {
                    player.animator.speed = 1f;
                }
            }
        } else {
            footsteps.Stop();
            if (player.animator) {
                player.animator.SetBool("Running", false);
                player.animator.speed = 1f;
            }
        }

        if (player.can_change_facing) {
            if (x_input < 0) {
                facing = -1;
                flip_object.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            } else if (x_input > 0) {
                facing = 1;
                flip_object.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    IEnumerator DropRoutine() {
        on_drop?.Invoke();
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

    void Jump(bool grounded) {
        gravity_force = 0;
        velocity.y = jump_velocity;
        jumps_used += 1;
        if (grounded) {
            StartCoroutine(JumpRoutine());
            grounded_jump_used = true;
        } else {
            StartCoroutine(ForceJumpRoutine(.08f));
        }
    }

    IEnumerator JumpRoutine() {
        jumping = true;
        float time_left = max_jump_hold;
        bool held = true;
        while (time_left > 0 && held && !cont.collisions.above) {
            time_left -= Time.fixedDeltaTime;
            held = held && Input.GetButton("Jump");
            yield return new WaitForFixedUpdate();
        }
        jumping = false;
    }

    IEnumerator ForceJumpRoutine(float force) {
        jumping = true;
        float time_left = force;
        while (time_left > 0 && !cont.collisions.above) {
            time_left -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        jumping = false;
    }

    [System.Serializable]
    class SoundEffects {
        public SFXInfo walking;
        public SFXInfo slash;
    }
}
