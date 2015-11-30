using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

    protected static float SkinWidth { get { return 0.015f; } }

    protected RectangleVertices RaycastOrigins { get; set; }

    protected IntVector2D RayCount { get; set; }
    protected Vector2 RaySpacing { get; set; }

    public BoxCollider2D Collider2D { get; private set; }

    // Use this for initialization
    private void Awake() {
        Collider2D = GetComponent<BoxCollider2D>();

        RaycastOrigins = new RectangleVertices(GetExpandedBounds());
        RayCount = new IntVector2D(4, 4);
    }

    protected virtual void Start () {
        CalculateRaySpacing();
    }

    protected void UpdateRaycast() {
        CalculateRaySpacing();
        UpdateRaycastOrigins();
    }

    private void UpdateRaycastOrigins() {
        AssignRaycastOrigins(GetExpandedBounds());
    }

    private void CalculateRaySpacing() {
        AssaignRaycastCount();

        AssignRaycastSpacing(GetExpandedBounds());
    }

    private void AssignRaycastOrigins(Bounds bounds) {
        RaycastOrigins.AssignBounds(bounds);
    }

    private void AssaignRaycastCount() {
        RayCount = new IntVector2D(Mathf.Clamp(RayCount.X, 2, int.MaxValue),
                                 Mathf.Clamp(RayCount.Y, 2, int.MaxValue));
    }

    private void AssignRaycastSpacing(Bounds bounds) {
        RaySpacing = new Vector2(bounds.size.x / (RayCount.X - 1),
                                 bounds.size.y / (RayCount.Y - 1));
    }

    protected Bounds GetExpandedBounds() {
        Bounds bounds = Collider2D.bounds;
        bounds.Expand(SkinWidth * -2);

        return bounds;
    }

    protected Vector2 GetRayOriginY(int direction) {
        return (direction == -1) ? RaycastOrigins.BottomLeft : RaycastOrigins.TopLeft;
    }
    protected Vector2 GetRayOriginX(int direction) {
        return (direction == -1) ? RaycastOrigins.BottomLeft : RaycastOrigins.BottomRight;
    }
}
