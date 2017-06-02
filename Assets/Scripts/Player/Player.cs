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
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;


    [Space(10)]
    [Header("Movement")]
    public float moveSpeed = 6;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;
    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;

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
        //controllerStates = GetComponent<ControllerStates>();       

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
        CalculateVelocity();
        HandleWallSliding();

        controller.Move(velocity * Time.deltaTime, directionalInput);
        CharacterDirection();

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

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        if (wallSliding)
        {
            if (wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
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

    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controllerStates.IsCollidingBelow) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    void CharacterDirection()
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

}
