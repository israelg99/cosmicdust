using UnityEngine;

public class ControllerInfo {
    public bool Left { get; set; }
    public bool Top { get; set; }
    public bool Right { get; set; }
    public bool Bot { get; set; }

    public bool ClimbingSlope { get; set; }
    public bool DescendingSlope { get; set; }

    public bool IsJump { get; set; }

    public float SlopeAngle { get; set; }
    public float SlopeAngleOld { get; set; }

    public Vector3 VelocityOld { get; set; }

    public bool IsWallSliding { get; set; }

    public int Face { get; set; }
    public int WallDirX { get; set; }

    public ControllerInfo() {
        IsWallSliding = false;
        IsJump = false;
        Reset();
    }

    public void Reset() {
        Left = false;
        Top = false;
        Right = false;
        Bot = false;
        ClimbingSlope = false;
        DescendingSlope = false;

        // We reset IsWallSliding in the Player's ApplyWallSliding function

        SlopeAngleOld = SlopeAngle;
        SlopeAngle = 0;
    }
}
