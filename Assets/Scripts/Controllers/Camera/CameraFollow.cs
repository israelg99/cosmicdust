using UnityEngine;

public class CameraFollow : MonoBehaviour {

    /* Public variables to modify in Unity Inspector */
    public Controller2D Target;
    /* End of public variables to modify in Unity Inspector */

    private float VerticalOffset { get; set; }

    private float VerticalSmoothTime { get; set; }

    private Vector2 FocusAreaSize { get; set; }
    private FocusArea FocusArea { get; set; }

    private float CurrentLookAheadX { get; set; }
    private float TargetLookAheadX { get; set; }
    private float LookAheadDirX { get; set; }
    private float SmoothLookVelocityX { get; set; }
    private float LookAheadDstX { get; set; }
    private float LookSmoothTimeX { get; set; }

    private bool LookAheadStopped { get; set; }

    private float SmoothVelocityY { get; set; }

    void Start() {

        VerticalOffset = 1;

        LookAheadDstX = 4;

        LookSmoothTimeX = 0.5f;
        VerticalSmoothTime = 0.2f;

        FocusAreaSize = new Vector2(3,5);

        FocusArea = new FocusArea(Target.Collider2D.bounds, FocusAreaSize);
    }

    void LateUpdate() {
        FocusArea.Update(Target.Collider2D.bounds);

        Vector2 focusPosition = FocusArea.Centre + Vector2.up * VerticalOffset;

        if (FocusArea.Velocity.x != 0) {
            LookAheadDirX = Mathf.Sign(FocusArea.Velocity.x);
            if (Mathf.Sign(Target.PlayerInput.x) == Mathf.Sign(FocusArea.Velocity.x) && Target.PlayerInput.x != 0) {
                LookAheadStopped = false;
                TargetLookAheadX = LookAheadDirX * LookAheadDstX;
            } else {
                // If the player stopped moving in the X axis, the camera will stop the look ahead feature.
                if (!LookAheadStopped) {
                    LookAheadStopped = true;
                    TargetLookAheadX = CurrentLookAheadX + (LookAheadDirX * LookAheadDstX - CurrentLookAheadX) / 4f;
                }
            }
        }

        // Temp variable to store the SmoothDamp float, we assign it to the desired variable in the next few lines.
        float tempSLVX = SmoothLookVelocityX;
        CurrentLookAheadX = Mathf.SmoothDamp(CurrentLookAheadX, TargetLookAheadX, ref tempSLVX, LookSmoothTimeX);
        SmoothLookVelocityX = tempSLVX;

        // Temp variable to store the SmoothDamp float, we assign it to the desired variable in the next few lines.
        float tempSVY = SmoothVelocityY;
        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref tempSVY, VerticalSmoothTime);
        SmoothVelocityY = tempSVY;

        focusPosition += Vector2.right * CurrentLookAheadX;
        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }

    void OnDrawGizmos() {
        Gizmos.color = new Color(1, 0, 0, .5f);
        Gizmos.DrawCube(FocusArea.Centre, FocusAreaSize);
    }
}