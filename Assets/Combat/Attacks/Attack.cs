using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Attack : MonoBehaviour {

    public delegate void OnHit(Character hit, Attack hit_by);

    protected Collider2D hitbox;
    protected Dictionary<Character, float> hit_objects;
    protected float timer {
        get; private set;
    }
    protected OnHit on_hit {
        get; private set;
    }

    [SerializeField] protected LayerMask targets;
    public Character source { get; private set; }

    protected virtual void Awake() {
        hitbox = GetComponent<Collider2D>();
        hit_objects = new Dictionary<Character, float>();
        if (source == null) source = GetComponentInParent<Character>();
        timer = 0;
    }

    protected virtual void Update() {
        timer += Time.deltaTime;
    }

    public void SetOnHit(OnHit on_hit) {
        this.on_hit = on_hit;
    }
    public void SetTargets(LayerMask mask) {
        targets = mask;
    }
    public virtual void SetSource(Character _source) {
        source = _source;
    }
    public virtual void Enable() {
        hit_objects.Clear();
        StartCoroutine(BumpHitbox());
        timer = 0;
        hitbox.enabled = true;
    }
    public virtual void Disable() {
        hitbox.enabled = false;
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        CheckHitboxCollisions(collision);
    }
    protected void OnTriggerStay2D(Collider2D collision) {
        CheckHitboxCollisions(collision);
    }

    protected virtual void CheckHitboxCollisions(Collider2D collision) {
        if ((1 << collision.gameObject.layer & targets) != 0) {
            ConfirmHit(collision.gameObject.GetComponentInParent<Character>());
        }
    }

    protected abstract bool HitCondition(Character d);

    protected virtual void OnCollisionWithTarget() { }

    protected bool ConfirmHit(Character d) {
        if (!d.invincible && HitCondition(d)) {
            LogHit(d);
            on_hit?.Invoke(d, this);
            OnCollisionWithTarget();
            return true;
        }
        return false;
    }
    void LogHit(Character d) {
        if (hit_objects.ContainsKey(d)) {
            hit_objects[d] = timer;
        } else {
            hit_objects.Add(d, timer);
        }
    }
    IEnumerator BumpHitbox() {
        transform.position += Vector3.one * .001f;
        yield return null;
        transform.position -= Vector3.one * .001f;
    }

    private void OnDisable() {
        Disable();
    }
}
