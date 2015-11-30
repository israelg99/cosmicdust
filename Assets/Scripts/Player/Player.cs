using System;
using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

    private static float Gravity { get; set; }

    private float MoveSpeed { get; set; }

    private float AccelerationTimeAirborne { get; set; }
    private float AccelerationTimeGrounded { get; set; }

    private float MaxJumpHeight { get; set; }
    private float MinJumpHeight { get; set; }

    private float TimeToJumpApex { get; set; }

    private float MaxJumpVelocity { get; set; }
    private float MinJumpVelocity { get; set; }

    /* Public variables to modify in Unity Inspector */
    public Vector2 WallJumpClimb;
    public Vector2 WallJumpOff;
    public Vector2 WallLeap;
    public float WallSlideSpeedMax;
    /* End of public variables to modify in Unity Inspector */

    private float WallStickTime { get { return 0.35f; } }
    private float TimeToWallUnstick { get; set; }

    private float VelocityXSmoothing { get; set; }

    private Vector3 Velocity { get; set; }

    // Controller2D is for applying velocities, not setting them.
    private Controller2D Controller { get; set; }

    // Use this for initialization
    private void Start () {

        MoveSpeed = 6f;

        MaxJumpHeight = 4f;
        MinJumpHeight = 1f;
        TimeToJumpApex = 0.4f;

        AccelerationTimeAirborne = 0.2f;
        AccelerationTimeGrounded = 0.1f;

        Gravity = -((2 * MaxJumpHeight) / Mathf.Pow(TimeToJumpApex, 2));
        MaxJumpVelocity = Mathf.Abs(Gravity) * TimeToJumpApex;
        MinJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * MinJumpHeight);

        Controller = GetComponent<Controller2D>();
	}

    // Update is called once per frame
    private void Update () {
        Vector2 input = GetInput();
        Controller.ControllerInfo.WallDirX = (Controller.ControllerInfo.Left) ? -1 : 1;

        // "ApplyInputX" is first because, "ApplyWallSliding" depends on it for Wall Sliding.
        ApplyInputX(input.x);

        // "ApplyWallSliding" is here because, is should be applied before the potential gravity reset in "ApplyGravity".
        ApplyWallSliding(input.x);

        // "ApplyGravity" is here because it should be applied before jumping, otherwise it may as well just reset the jump in "ApplyInputJump".
        ApplyGravity();

        // "ApplyInputJump" apparently is the most depended except of the "Move" method of course.
        ApplyInputJump(input);

        // "Move" is the most depended, should be the last.
        // "Move" takes input as an argument to store it in the Controller2D class.
        // We will manage the coupling later when things will be more concise.
        Move(input);
        
    }

    private static Vector2 GetInput() {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void ApplyGravity() {
        float velY = Velocity.y;

        if (Controller.ControllerInfo.Top || Controller.ControllerInfo.Bot) {
            velY = 0;
        }

        // We still have to assign a minimal amount of gravity to calculate collisions.
        Velocity = new Vector3(Velocity.x, velY + Gravity * Time.deltaTime);
    }

    private void ApplyWallSliding(float inputX) {
        float velY = Velocity.y;

        Controller.ControllerInfo.IsWallSliding = false;
        if ((Controller.ControllerInfo.Left || Controller.ControllerInfo.Right) &&
            !Controller.ControllerInfo.Bot && Velocity.y < 0 && inputX != 0) {

            Controller.ControllerInfo.IsWallSliding = true;

            // Handling maximum slide speed
            if (Velocity.y < -WallSlideSpeedMax) {
                velY = -WallSlideSpeedMax;
            }

            // Handling the "stick to wall" system for leap jumps and such...
            if (TimeToWallUnstick > 0) {
                ResetXMovement();

                if (Math.Abs(inputX - Controller.ControllerInfo.WallDirX) > 0.9f) {
                    TimeToWallUnstick -= Time.deltaTime;
                } else {
                    TimeToWallUnstick = WallStickTime;
                }
            } else {
                TimeToWallUnstick = WallStickTime;
            }
        }

        Velocity = new Vector3(Velocity.x, velY);
    }

    private void ResetXMovement() {
        VelocityXSmoothing = 0;
        Velocity = new Vector3(0, Velocity.y);
    }

    /* Not used at the moment due to one method only */
    private void ApplyInput(Vector2 input) {
        ApplyInputJump(input);
    }

    private void ApplyInputX(float input) {
        // Raw inputY on the X axis is multiplied by "MoveSpeed" and stored as the Target Velocity X.
        float targetVelocityX = input * MoveSpeed;

        // We smooth our Velocity X, to make turning and such, more gentle.
        float tempXSmoothing = VelocityXSmoothing; // We cannot pass properties as references, therefore we create this temporary variable to hold our "VelocityXSmoothing" value address.
        float smoothVelocityX = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref tempXSmoothing, (Controller.ControllerInfo.Bot) ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        VelocityXSmoothing = tempXSmoothing;

        Velocity = new Vector3(smoothVelocityX, Velocity.y);
    }

    private void ApplyInputJump(Vector2 inputY) {
        float velX = Velocity.x, velY = Velocity.y;

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0)) && !Controller.ControllerInfo.IsJump) {

            Controller.ControllerInfo.IsJump = Controller.ControllerInfo.Bot || Controller.ControllerInfo.IsWallSliding;

            // Regular Jumping
            if (Controller.ControllerInfo.Bot) {
                velY = MaxJumpVelocity;

                // Wall Jumping
            } else if (Controller.ControllerInfo.IsWallSliding) {
                if (Math.Abs(Controller.ControllerInfo.WallDirX - inputY.x) < 0.9f) {
                    velX = -Controller.ControllerInfo.WallDirX * WallJumpClimb.x;
                    velY = WallJumpClimb.y;
                } else if (inputY.x == 0) {
                    velX = -Controller.ControllerInfo.WallDirX * WallJumpOff.x;
                    velY = WallJumpOff.y;
                } else {
                    velX = -Controller.ControllerInfo.WallDirX * WallLeap.x;
                    velY = WallLeap.y;
                }
            }

        } else if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0)) {
            if (Velocity.y > MinJumpVelocity && Controller.ControllerInfo.IsJump) {
                velY = MinJumpVelocity;
            }
            Controller.ControllerInfo.IsJump = false;

        // It means we are just holding the jump button, while we are on the ground.
        } else if (Controller.ControllerInfo.IsJump && (Controller.ControllerInfo.Bot || Controller.ControllerInfo.IsWallSliding)) {
            Controller.ControllerInfo.IsJump = false;
        }

        // We apply all the velocities here at the bottom, to make things more clear and easier to debug.
        Velocity = new Vector3(velX, velY);
    }

    private void Move(Vector2 input) {
        Controller.Move(Velocity * Time.deltaTime, input);
    }
}
