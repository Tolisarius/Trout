using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    [Space(10)]
    [Header("Jumps")]
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;

    [Space(10)]
    [Header("Movement")]
    public float moveSpeed = 6;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;

    [Space(10)]
    [Header("Wall")]
    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    public Vector2 wallJumpOff;

    [Header("Wall climb")]
    public bool wallClimbEnabled;
    public Vector2 wallJumpClimb;


    [Header("Wall leap")]
    public bool wallLeapEnabled;
    public bool wallLeapDirectionRequired;
    public Vector2 wallLeap;

    float timeToWallUnstick;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;
    ControllerStates controllerStates;

    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;
    string currentDirection;

    void Start()
    {
        controller = GetComponent<Controller2D>();
        controllerStates = controller.controllerStates;

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        if (controllerStates.faceDir == -1)
        {
            currentDirection = "left";
        }
        else
        {
            currentDirection = "right";
        }
    }

    void Update()
    {
        EveryFrame();
    }

    void EveryFrame()
    {
        CalculateVelocity();
        HandleWallSliding();
        HandleWallFallOff();

        controller.Move(velocity * Time.deltaTime, directionalInput);

        VerticalCollisionsHandling();       // must be AFTER controller.Move
        HandleCharacterDirection();
    }

    /// <summary>
    /// Input reactions
    /// </summary>

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }    

    public void OnJumpInputDown()
    {
        /// first what to do if Player is wallsliding
        if (wallSliding)        
        {
            if (wallClimbEnabled && (wallDirX == directionalInput.x))
            {
                WallClimb();
            }

            if (wallLeapEnabled)
            {
                if (wallLeapDirectionRequired && wallDirX == directionalInput.x * -1)
                {
                    WallLeap();
                }
                else if ((!wallLeapDirectionRequired && !wallClimbEnabled) || (wallClimbEnabled && (wallDirX != directionalInput.x || directionalInput.x == 0)))
                {
                    WallLeap();
                }
            }
        }
        
        /// normal jump
        if (controllerStates.IsCollidingBelow || controllerStates.JustLeftPlatform)
        {
            controllerStates.IsNormalJumping = true;
            if (controllerStates.slidingDownMaxSlope)
            {
                if (directionalInput.x != -Mathf.Sign(controllerStates.slopeNormal.x))
                {
                    // not jumping against max slope
                    velocity.y = maxJumpVelocity * controllerStates.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controllerStates.slopeNormal.x;
                }
            }
            else
            {
                print("Skacu!!!");
                velocity.y = maxJumpVelocity;
            }
        }
    }
    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }

    /// <summary>
    /// Physics stuff
    /// </summary>
    void HandleCharacterDirection()
    {
        if (controllerStates.faceDir == -1)
        {
            SwitchFaceOrientation("left");
        }
        if (controllerStates.faceDir == 1)
        {
            SwitchFaceOrientation("right");
        }
    }
    void HandleWallSliding()
    {
        wallDirX = (controllerStates.IsCollidingLeft) ? -1 : 1;
        wallSliding = false;
        if ((controllerStates.IsCollidingLeft || controllerStates.IsCollidingRight) && !controllerStates.IsCollidingBelow && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }
    void HandleWallFallOff()    // when Player is sliding the wall and gives input away from it
    {
        if (wallSliding && wallDirX == directionalInput.x * -1)
        {
            velocity.x = -wallDirX * wallJumpOff.x;
            velocity.y = wallJumpOff.y;
        }
    }
    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controllerStates.IsCollidingBelow) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }
    void VerticalCollisionsHandling()
    {
        if (controllerStates.IsCollidingAbove || controllerStates.IsCollidingBelow)
        {
            if (controllerStates.slidingDownMaxSlope)
            {
                velocity.y += controllerStates.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
    }

    /// <summary>
    ///  Player methods
    /// </summary>
    /// 
    public void SwitchFaceOrientation(string dir)

    {
        Vector2 newScale = gameObject.transform.localScale;
        if (dir == "left")
        {
            newScale.x = Mathf.Abs(newScale.x);
            currentDirection = "left";
        }

        else if (dir == "right")
        {
            newScale.x = Mathf.Abs(newScale.x) * -1;
            currentDirection = "right";
        }
        gameObject.transform.localScale = newScale;
    }
    void WallLeap()
    {
        velocity.x = -wallDirX * wallLeap.x;
        velocity.y = wallLeap.y;
    }
    void WallClimb()
    {
        velocity.x = -wallDirX * wallJumpClimb.x;
        velocity.y = wallJumpClimb.y;
    }

}
