using UnityEngine;

public class PassengerMovement {
    public Transform TransformData { get; set; }
    public Vector3 Velocity { get; set; }
    public bool StandingOnPlatform { get; set; }
    public bool MoveBeforePlatform { get; set; }

    public PassengerMovement(Transform transformData, Vector3 velocity, bool standingOnPlatform, bool moveBeforePlatform) {
        TransformData = transformData;
        Velocity = velocity;
        StandingOnPlatform = standingOnPlatform;
        MoveBeforePlatform = moveBeforePlatform;
    }
}
