using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour {

    public Vector2Int position {
        get { return _position; }
    }
    public bool is_background_tile;

    public bool has_collider { get { return coll != null; } }

    [SerializeField] Vector2Int _position;

    [SerializeField] SpriteRenderer sprite_renderer;
    [SerializeField] BoxCollider2D coll;

    public void LoadSprite(Sprite sprite) {
        sprite_renderer.sprite = sprite;
    }

    public void SetPosition(Vector2Int position) {
        _position = position;
    }

}
