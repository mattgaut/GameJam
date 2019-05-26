using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour {
    [SerializeField] SpriteRenderer sprite_renderer;
    [SerializeField] BoxCollider2D coll;

    public bool has_collider { get { return coll != null; } }

    public void LoadSprite(Sprite sprite) {
        sprite_renderer.sprite = sprite;
    }


}
