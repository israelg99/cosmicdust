using UnityEngine;

public class FocusArea {
    public Vector2 Centre { get; private set; }
    public Vector2 Velocity { get; private set; }
    private float Left { get; set; }
    private float Right { get; set; }
    private float Top { get; set; }
    private float Bottom { get; set; }

    public FocusArea(Bounds targetBounds, Vector2 size) {
        Left = targetBounds.center.x - size.x / 2;
        Right = targetBounds.center.x + size.x / 2;
        Bottom = targetBounds.min.y;
        Top = targetBounds.min.y + size.y;

        Velocity = Vector2.zero;
        Centre = new Vector2((Left + Right) / 2, (Top + Bottom) / 2);
    }

    public void Update(Bounds targetBounds) {
        float shiftX = 0;
        if (targetBounds.min.x < Left) {
            shiftX = targetBounds.min.x - Left;
        } else if (targetBounds.max.x > Right) {
            shiftX = targetBounds.max.x - Right;
        }
        Left += shiftX;
        Right += shiftX;

        float shiftY = 0;
        if (targetBounds.min.y < Bottom) {
            shiftY = targetBounds.min.y - Bottom;
        } else if (targetBounds.max.y > Top) {
            shiftY = targetBounds.max.y - Top;
        }
        Top += shiftY;
        Bottom += shiftY;
        Centre = new Vector2((Left + Right) / 2, (Top + Bottom) / 2);
        Velocity = new Vector2(shiftX, shiftY);
    }
}
