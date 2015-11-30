using UnityEngine;

public class Controller2D : RaycastController {

    // Unity5 doesn't support '=>' at the moment, which is sad.
    private static float MaxClimbAngle { get { return 80f; } }
    private static float MaxDescendAngle { get { return 80f; } }

    public ControllerInfo ControllerInfo { get; private set; }
    public Vector2 PlayerInput { get; private set; }

    protected LayerMask CollisionMask { get; set; }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        CollisionMask = LayerMask.GetMask("Collidable");

        PlayerInput = Vector2.zero;

        ControllerInfo = new ControllerInfo {
            Face = 1
        };
    }

    public void Move(Vector3 velocity, bool standingOnPlatform = false) {
        Move(velocity, Vector2.zero, standingOnPlatform);
    }

    public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false) {
        UpdateRaycast();
        ControllerInfo.Reset();
        ControllerInfo.VelocityOld = velocity;
        PlayerInput = input;

        if (velocity.x != 0) {
            ControllerInfo.Face = (int)Mathf.Sign(velocity.x);
        }

        if (velocity.y < 0) {
            DescendSlope(ref velocity);
        }

        HorizontalCollisions(ref velocity);
        if (velocity.y != 0) {
            VerticalCollisions(ref velocity);
        }

        if (standingOnPlatform) {
            ControllerInfo.Bot = true;
        }

        transform.Translate(velocity);
    }

    private void HorizontalCollisions(ref Vector3 velocity) {
        int directionX = ControllerInfo.Face;
        float rayLength = Mathf.Abs(velocity.x) + SkinWidth;

        if (Mathf.Abs(velocity.x) < SkinWidth) {
            rayLength = 2 * SkinWidth;
        }

        for (int i = 0; i < RayCount.Y; i++) {
            Vector2 rayOrigin = GetRayOriginX(directionX);
            rayOrigin += Vector2.up * (RaySpacing.y * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (!hit || hit.distance == 0) {
                continue;
            }

            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (i == 0 && slopeAngle <= MaxClimbAngle) {
                if (ControllerInfo.DescendingSlope) {
                    ControllerInfo.DescendingSlope = false;
                    velocity = ControllerInfo.VelocityOld;
                }
                float distanceToSlopeStart = 0;
                if (slopeAngle != ControllerInfo.SlopeAngleOld) {
                    distanceToSlopeStart = hit.distance - SkinWidth;
                    velocity.x -= distanceToSlopeStart * directionX;
                }
                ClimbSlope(ref velocity, slopeAngle);
                velocity.x += distanceToSlopeStart * directionX;
            }

            if (ControllerInfo.ClimbingSlope && !(slopeAngle > MaxClimbAngle)) {
                continue;
            }
            velocity.x = (hit.distance - SkinWidth) * directionX;
            rayLength = hit.distance;

            if (ControllerInfo.ClimbingSlope) {
                velocity.y = Mathf.Tan(ControllerInfo.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
            }

            ControllerInfo.Left = directionX == -1;
            ControllerInfo.Right = directionX == 1;
        }
    }

    private void VerticalCollisions(ref Vector3 velocity) {
        int directionY = System.Math.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SkinWidth;

        Vector2 rayOrigin;
        RaycastHit2D hit;
        for (int i = 0; i < RayCount.X; i++) {
            rayOrigin = GetRayOriginY(directionY);
            rayOrigin += Vector2.right * (RaySpacing.x * i + velocity.x);
            hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, CollisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (!hit) {
                continue;
            }

            if (hit.collider.tag == "Through" &&
                (directionY == 1 || hit.distance == 0)) {
                    continue;
            }

            velocity.y = (hit.distance - SkinWidth) * directionY;
            rayLength = hit.distance;

             if (ControllerInfo.ClimbingSlope) {
                velocity.x = velocity.y / Mathf.Tan(ControllerInfo.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
             }

            ControllerInfo.Bot = directionY == -1;
            ControllerInfo.Top = directionY == 1;
        }

        if (!ControllerInfo.ClimbingSlope) {
            return;
        }
        int directionX = System.Math.Sign(velocity.x);
        rayLength = Mathf.Abs(velocity.x) + SkinWidth;
        rayOrigin = (GetRayOriginX(directionX)) + Vector2.up * velocity.y;
        hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);

        if (!hit) {
            return;
        }
        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

        if (slopeAngle == ControllerInfo.SlopeAngle) {
            return;
        }
        velocity.x = (hit.distance - SkinWidth) * directionX;
        ControllerInfo.SlopeAngle = slopeAngle;
    }

    private void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y > climbVelocityY) {
            return;
        }
        velocity.y = climbVelocityY;
        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
        ControllerInfo.Bot = true; // We are unable to check bot collision due to the Velocity Y being positive when climbing a slope, therefore we automatically assume we collide bot, which we do.
        ControllerInfo.ClimbingSlope = true;
        ControllerInfo.SlopeAngle = slopeAngle;
    }

    private void DescendSlope(ref Vector3 velocity) {
        int directionX = System.Math.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? RaycastOrigins.BottomRight : RaycastOrigins.BottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, CollisionMask);

        if (!hit) {
            return;
        }
        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

        if ((slopeAngle == 0 || !(slopeAngle <= MaxDescendAngle)) ||
            (Mathf.Sign(hit.normal.x) != directionX) ||
            (!(hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)))) {
            return;
        }
        float moveDistance = Mathf.Abs(velocity.x);
        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
        velocity.y -= descendVelocityY;

        ControllerInfo.SlopeAngle = slopeAngle;
        ControllerInfo.DescendingSlope = true;
        ControllerInfo.Bot = true;
    }
}
