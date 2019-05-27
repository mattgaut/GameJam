using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour {
    [SerializeField] Character player;

    [SerializeField] Attack slash;
    [SerializeField] Vector2 slash_knockback;

    [SerializeField] float slash_cooldown, slash_hitbox_length;
    float slash_timer;

    [SerializeField] SoundEffects sfxs;

    public void TryUseSlash(int direction) {
        if (slash_timer > slash_cooldown) {
            UseSlash(direction);
        }
    }

    private void UseSlash(int direction) {
        slash_timer = 0;
        slash.gameObject.SetActive(true);
        player.Dash(.25f * direction * Vector2.right, 0.1f);
        slash.Enable();
        SoundManager.instance.PlaySfx(sfxs.slash);
        Invoke("EndSlash", slash_hitbox_length);
    }

    void EndSlash() {
        slash.Disable();
        slash.gameObject.SetActive(false);
    }

    private void Awake() {
        slash.SetSource(player);
        slash.SetOnHit(OnHit);
    }

    private void Update() {
        slash_timer += Time.deltaTime;
    }

    void OnHit(Character hit, Attack hit_by) {
        hit.TakeDamage(1, player);
        SoundManager.instance.PlaySfx(sfxs.hit);
        Vector2 real_knockback = slash_knockback;
        real_knockback.x *= Mathf.Sign(hit.transform.position.x - hit_by.transform.position.x);
        hit.TakeKnockback(player, real_knockback);
    }

    [System.Serializable]
    class SoundEffects {
        public SFXInfo slash, hit;
    }
}
