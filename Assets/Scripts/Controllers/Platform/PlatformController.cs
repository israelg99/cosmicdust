using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

    private LayerMask PassengerMask { get; set; }

    /* Public variables to modify in Unity Inspector */
    public Vector3[] LocalWaypoints;
    public float Speed;
    public bool Cyclic;
    public float WaitTime;

    [Range(0, 2)]
    public float EaseAmount;
    /* End of public variables to modify in Unity Inspector */

    private Vector3[] GlobalWaypoints { get; set; }

    private int FromWaypointIndex { get; set; }
    private float PercentBetweenWaypoints { get; set; }
    private float NextMoveTime { get; set; }

    private List<PassengerMovement> PassengerMovement { get; set; }
    private HashSet<Transform> MovedPassengers { get; set; }
    private Dictionary<Transform, Controller2D> PassengerDictionary { get; set; }

    protected override void Start() {
        base.Start();

        PassengerDictionary = new Dictionary<Transform, Controller2D>();

        PassengerMask = LayerMask.GetMask("Player");

        GlobalWaypoints = new Vector3[LocalWaypoints.Length];
        for (int i = 0; i < LocalWaypoints.Length; i++) {
            GlobalWaypoints[i] = LocalWaypoints[i] + transform.position;
        }
    }

    private void Update() {
        UpdateRaycast();

        Vector3 velocity = CalculatePlatformMovement();

        CalculatePassengerMovement(velocity);

        MovePassengers(true);
        MovePlatform(velocity);
        MovePassengers(false);
    }

    private float Ease(float x) {
        float a = EaseAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    private Vector3 CalculatePlatformMovement() {

        if (Time.time < NextMoveTime) {
            return Vector3.zero;
        }

        FromWaypointIndex %= GlobalWaypoints.Length;
        int toWaypointIndex = (FromWaypointIndex + 1) % GlobalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(GlobalWaypoints[FromWaypointIndex], GlobalWaypoints[toWaypointIndex]);
        PercentBetweenWaypoints += Time.deltaTime * Speed / distanceBetweenWaypoints;
        PercentBetweenWaypoints = Mathf.Clamp01(PercentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(PercentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(GlobalWaypoints[FromWaypointIndex], GlobalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if (PercentBetweenWaypoints >= 1) {
            PercentBetweenWaypoints = 0;
            FromWaypointIndex++;

            if (!Cyclic) {
                if (FromWaypointIndex >= GlobalWaypoints.Length - 1) {
                    FromWaypointIndex = 0;
                    System.Array.Reverse(GlobalWaypoints);
                }
            }
            NextMoveTime = Time.time + WaitTime;
        }

        return newPos - transform.position;
    }

    private void MovePassengers(bool beforeMovePlatform) {
        foreach (PassengerMovement passenger in PassengerMovement) {
            if (!PassengerDictionary.ContainsKey(passenger.TransformData)) {
                PassengerDictionary.Add(passenger.TransformData, passenger.TransformData.GetComponent<Controller2D>());
            }

            if (passenger.MoveBeforePlatform == beforeMovePlatform) {
                PassengerDictionary[passenger.TransformData].Move(passenger.Velocity, passenger.StandingOnPlatform);
            }
        }
    }


    private void MovePlatform(Vector3 velocity) {
        transform.Translate(velocity);
    }

    private void CalculatePassengerMovement(Vector3 velocity) {
        MovedPassengers = new HashSet<Transform>();
        PassengerMovement = new List<PassengerMovement>();

        int directionX = System.Math.Sign(velocity.x);
        int directionY = System.Math.Sign(velocity.y);

        float rayLength;

        // Vertically moving platform
        if (velocity.y != 0) {
            rayLength = Mathf.Abs(velocity.y) + SkinWidth;

            for (int i = 0; i < RayCount.X; i++) {
                Vector2 rayOrigin = GetRayOriginY(directionY);
                rayOrigin += Vector2.right * (RaySpacing.x * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, PassengerMask);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

                if (!hit || MovedPassengers.Contains(hit.transform) || hit.distance == 0) {
                    continue;
                }
                
                MovedPassengers.Add(hit.transform);
                float pushX = (directionY == 1) ? velocity.x : 0;
                float pushY = velocity.y - (hit.distance - SkinWidth) * directionY;
                PassengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
            }
        }

        // Horizontally moving platform
        if (velocity.x != 0) {
            rayLength = Mathf.Abs(velocity.x) + SkinWidth;

            for (int i = 0; i < RayCount.Y; i++) {
                Vector2 rayOrigin = GetRayOriginX(directionX);
                rayOrigin += Vector2.up * (RaySpacing.y * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, PassengerMask);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                if (!hit || MovedPassengers.Contains(hit.transform) || hit.distance == 0) {
                    continue;
                }
                MovedPassengers.Add(hit.transform);
                float pushX = velocity.x - (hit.distance - SkinWidth) * directionX;
                float pushY = -SkinWidth;
                PassengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
            }
        }

        // Passenger on top of a horizontally or downward moving platform
        if (directionY != -1 && (velocity.y != 0 || velocity.x == 0)) {
            return;
        }
        rayLength = SkinWidth * 2;

        for (int i = 0; i < RayCount.X; i++) {
            Vector2 rayOrigin = RaycastOrigins.TopLeft + Vector2.right * (RaySpacing.x * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, PassengerMask);
   
            if (!hit || MovedPassengers.Contains(hit.transform) || hit.distance == 0) {
                continue;
            }
            MovedPassengers.Add(hit.transform);
            float pushX = velocity.x;
            float pushY = velocity.y;
            PassengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
        }
    }

    private void OnDrawGizmos() {
        if (LocalWaypoints == null) {
            return;
        }
        Gizmos.color = Color.red;
        const float size = .3f;

        for (int i = 0; i < LocalWaypoints.Length; i++) {
            Vector3 globalWaypointPos = (Application.isPlaying) ? GlobalWaypoints[i] : LocalWaypoints[i] + transform.position;
            Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
            Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
        }
    }
}
