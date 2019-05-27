using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
    public enum Type { honey, pineapple, apple }

    [SerializeField] Type _type;
    [SerializeField] SpriteRenderer sprite_renderer;

    [SerializeField] float tame_chance = 0.5f;
    [SerializeField] string favored_enemy;

    [SerializeField] GameObject tame_success_object;
    
    public Sprite sprite {
        get { return sprite_renderer.sprite; }
    }

    public bool is_taming { get; set; }

    public bool used { get; private set; }

    public Type type {
        get { return _type; }
    }

    public void SetEnabled(bool enabled) {
        sprite_renderer.enabled = enabled;
        GetComponent<CircleCollider2D>().enabled = enabled;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (!is_taming) {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
                EnemyHandler character = collision.gameObject.GetComponentInParent<EnemyHandler>();
                if (!character.is_tamed) {
                    character.AttemptTame(this);
                }
            }
        }
    }

    public bool TryTame(Character character) {
        used = true;
        float chance = tame_chance;
        if (character.char_definition.name == favored_enemy) {
            chance *= 2;
        }

        if (Random.Range(0f, 1f) < chance) {
            Tame(character);
            return true;
        }

        Destroy(gameObject);
        return false;
    }

    void Tame(Character character) {
        GameObject obj = Instantiate(tame_success_object);
        obj.transform.position = character.transform.position;

        Destroy(gameObject);
    }
}
