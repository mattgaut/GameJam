using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    const float skin_width = .015f;
    [SerializeField] [Range(2, 10)] int horizontal_ray_count = 4, vertical_ray_count = 4;
    [SerializeField] bool can_travel_slopes = true;

    float vertical_ray_spacing, horizontal_ray_spacing;

    float max_climb_angle = 80;
    float max_descend_angle = 90;

    float max_slope_correction = .4f;

    [SerializeField] Collider2D boundary_collider;
    [SerializeField] LayerMask collision_mask;
    [SerializeField] bool uses_platforms;

    LayerMask floor_collision_mask;
    RaycastOrigins origins;
    public CollisionInfo collisions;

    private void Start() {
        SetRayOrigins();
        CalcRaySpacing();

        floor_collision_mask.value = collision_mask.value;
        AddPlatformToMask();
    }

    public bool AddPlatformToMask() {
        if (!uses_platforms) return false;
        if ((floor_collision_mask & (1 << LayerMask.NameToLayer("Platform"))) == 0) {
            floor_collision_mask += 1 << LayerMask.NameToLayer("Platform");
            collisions.droping_platform = false;
            return true;
        }
        return false;
    }
    public bool RemovePlatformFromMask() {
        if (!uses_platforms) return false;
        if ((floor_collision_mask & (1 << LayerMask.NameToLayer("Platform"))) != 0) {
            floor_collision_mask -= 1 << LayerMask.NameToLayer("Platform");
            collisions.droping_platform = true;
            return true;
        }
        return false;
    }

    public void Move(Vector2 velocity) {
        SetRayOrigins();
        collisions.Reset();
        collisions.velocity_old = velocity;
        Vector2 before = velocity;
        if (can_travel_slopes && velocity.y < 0) {
            DescendSlope(ref velocity);
        }
        if (velocity.x != 0) {
            HorizontalCollisions(ref velocity);
        }
        VerticalCollisions(ref velocity);
        Vector2 after = velocity;

        transform.Translate(velocity);
    }

    void HorizontalCollisions(ref Vector2 velocity) {
        float direction_x = Mathf.Sign(velocity.x);
        float ray_length = Mathf.Abs(velocity.x) + skin_width;
        for (int i = 0; i < horizontal_ray_count; i++) {
            Vector2 ray_origin = (direction_x == -1) ? origins.bottom_left : origins.bottom_right;
            ray_origin += Vector2.up * (horizontal_ray_spacing * i);
            RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.right * direction_x, ray_length, collision_mask);

            if (hit) {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (can_travel_slopes && i == 0 && angle <= max_climb_angle) {
                    if (collisions.descending_slope) {
                        collisions.descending_slope = false;
                        velocity = collisions.velocity_old;
                    }
                    float distance_to_slope_start = 0;
                    if (angle != collisions.last_slope_angle) {
                        distance_to_slope_start = hit.distance - skin_width;
                        velocity.x -= distance_to_slope_start * direction_x;
                    }

                    ClimbSlope(ref velocity, angle);
                    velocity.x += distance_to_slope_start * direction_x;
                }

                if (!collisions.climbing_slope || collisions.slope_angle > max_climb_angle) {
                    velocity.x = (hit.distance - skin_width) * direction_x;
                    ray_length = hit.distance;

                    if (collisions.climbing_slope) {
                        velocity.y = Mathf.Tan(collisions.slope_angle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    collisions.left = direction_x == -1;
                    collisions.right = direction_x == 1;
                }
            }
        }
    }
    void VerticalCollisions(ref Vector2 velocity) {
        float direction_y = Mathf.Sign(velocity.y);
        float ray_length = Mathf.Abs(velocity.y) + skin_width;

        bool add_platform_back = false;
        if (direction_y != -1) {
            add_platform_back = RemovePlatformFromMask();
        }

        if (collisions.descended_last_frame && direction_y == -1) {
            ray_length = ray_length + max_slope_correction;
        }
        for (int i = 0; i < vertical_ray_count; i++) {
            Vector2 ray_origin = (direction_y == -1) ? origins.bottom_left : origins.top_left;
            if (direction_y == -1) {
                ray_origin = origins.bottom_left;
            } else {
                ray_origin = origins.top_left;
            }
            ray_origin += Vector2.right * (vertical_ray_spacing * i + velocity.x);
            
            RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.up * direction_y, ray_length, direction_y == 1 ? collision_mask : floor_collision_mask);

            if (hit) {
                velocity.y = (hit.distance - skin_width) * direction_y;
                ray_length = hit.distance;

                if (collisions.climbing_slope) {
                    //velocity.x = velocity.y / Mathf.Tan(collisions.slope_angle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collisions.below = direction_y == -1;
                collisions.above = direction_y == 1;
            } else {
                if (i == 0 && direction_y == -1) {
                    hit = Physics2D.Raycast(ray_origin, Vector2.down, 1f, floor_collision_mask);
                    if (hit) {
                        collisions.over_slope_left = true;
                    } else {
                        collisions.hanging_left = true;
                    }
                }
                if (i == vertical_ray_count - 1 && direction_y == -1) {
                    hit = Physics2D.Raycast(ray_origin, Vector2.down, 1f, floor_collision_mask);
                    if (hit) {
                        collisions.over_slope_right = true;
                    } else {
                        collisions.hanging_right = true;
                    }
                }
            }
        }

        if (collisions.climbing_slope) {
            float direction_x = Mathf.Sign(velocity.x);
            ray_length = Mathf.Abs(velocity.x) + skin_width;

            Vector2 origin = (direction_x == -1 ? origins.bottom_left : origins.bottom_right) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction_x, ray_length, collision_mask);

            if (hit) {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle != collisions.slope_angle) {
                    velocity.x = (hit.distance - skin_width) * direction_x;
                    collisions.slope_angle = angle;
                }
            }
        }
        if (add_platform_back) {
            AddPlatformToMask();
        }
    }
    void ClimbSlope(ref Vector2 velocity, float slope_angle) {
        float move_distance = Mathf.Abs(velocity.x);
        float climb_velocity_y = Mathf.Tan(slope_angle * Mathf.Deg2Rad) * move_distance;
        if (velocity.y <= climb_velocity_y) {
            velocity.y = climb_velocity_y;
            //velocity.x = Mathf.Cos(slope_angle * Mathf.Deg2Rad) * move_distance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbing_slope = true;
            collisions.slope_angle = slope_angle;
        }
    }

    void DescendSlope(ref Vector2 velocity) {
        float direction_x = Mathf.Sign(velocity.x);
        Vector2 ray_origin = (direction_x == -1) ? origins.bottom_right : origins.bottom_left;
        RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.down, Mathf.Infinity, floor_collision_mask);
        if (hit) {
            if (hit.distance <= Mathf.Abs(velocity.y) + max_slope_correction) {
                float slope_angle = Vector2.Angle(hit.normal, Vector2.up);
                if (slope_angle != 0 && slope_angle <= max_descend_angle) {
                    if (Mathf.Sign(hit.normal.x) == direction_x) {
                        float move_distance = Mathf.Abs(velocity.x);
                        float descend_velocity_y = Mathf.Sin(slope_angle * Mathf.Deg2Rad) * move_distance;
                        //velocity.x = Mathf.Cos(slope_angle * Mathf.Deg2Rad) * move_distance * Mathf.Sign(velocity.x);
                        velocity.y -= descend_velocity_y;
                        collisions.slope_angle = slope_angle;
                        collisions.descending_slope = true;
                        collisions.below = true;                    
                    }
                }
            }
        }
    }
    public bool OverPlatform() {
        if (collisions.below) {
            RaycastHit2D hit = Physics2D.Raycast(origins.bottom_right, Vector2.down, .3f, floor_collision_mask);
            if (!hit || (hit && (hit.collider.gameObject.layer == LayerMask.NameToLayer("Platform"))) ) {
                hit = Physics2D.Raycast(origins.bottom_left, Vector2.down, .3f, floor_collision_mask);
                return !hit || (hit && (hit.collider.gameObject.layer == LayerMask.NameToLayer("Platform")));
            }
        }
        return false;
    }

    void SetRayOrigins() {
        Bounds bounds = boundary_collider.bounds;
        bounds.Expand(skin_width * -2f);

        origins.bottom_left = new Vector2(bounds.min.x, bounds.min.y);
        origins.top_left = new Vector2(bounds.min.x, bounds.max.y);
        origins.bottom_right = new Vector2(bounds.max.x, bounds.min.y);
        origins.top_right = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalcRaySpacing() {
        Bounds bounds = boundary_collider.bounds;
        bounds.Expand(skin_width * -2);

        horizontal_ray_spacing = bounds.size.y / (horizontal_ray_count - 1);
        vertical_ray_spacing = bounds.size.x / (vertical_ray_count - 1);
    }
    public struct CollisionInfo {
        public bool above, below, left, right;

        public bool below_last_frame;

        public bool climbing_slope, descending_slope, descended_last_frame;

        public bool hanging_left, hanging_right;
        public bool over_slope_left, over_slope_right;
        public bool droping_platform;
        public bool past_max;

        public float slope_angle, last_slope_angle;

        public Vector3 velocity_old;

        public void Reset() {
            descended_last_frame = descending_slope;
            below_last_frame = below;
            past_max = over_slope_left = over_slope_right = hanging_left = hanging_right = descending_slope = climbing_slope = above = below = left = right = false;

            last_slope_angle = slope_angle;
            slope_angle = 0;
        }
    }

    struct RaycastOrigins {
        public Vector2 top_left, top_right, bottom_left, bottom_right;
    }
}
