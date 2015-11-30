using UnityEngine;

public class RectangleVertices {
    public Vector2 TopLeft { get; set; }
    public Vector2 TopRight { get; set; }
    public Vector2 BottomLeft { get; set; }
    public Vector2 BottomRight { get; set; }

    public RectangleVertices(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight) {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
    }
    public RectangleVertices(Bounds bounds) {
        AssignBounds(bounds);
    }

    public void AssignBounds(Bounds bounds) {
        BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }
}