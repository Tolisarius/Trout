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
    public float wallLeapAfterWallBuffer,wallLeapAfterButtonBuffer;
    public Vector2 wallLeap;

    float timeToWallUnstick;

    float gravity;
    [HideInInspector]
    public float maxJumpVelocity, minJumpVelocity;
    [HideInInspector]
    public Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;
    public ControllerStates controllerStates;
    PlayerJumps playerJumps;
    PlayerStates playerStates;

    [HideInInspector]
    public Vector2 directionalInput;
    Vector2 _directionalInputBuffer;   
    bool _jumpPressed, _jumpPressedBuffer;
    int _wallDirX, _wallDirXBuffer;

    bool _directionalInputRestrictedX, _directionalInputRestrictedY;

    string currentDirection;

    void Start()
    {
        controller = GetComponent<Controller2D>();
        controllerStates = controller.controllerStates;
        playerJumps = GetComponent<PlayerJumps>();
        playerStates = GetComponent<PlayerStates>();

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
        TimeTravelingTests();
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
        if (_directionalInputRestrictedX)
        {
            directionalInput.x = 0;
        }
        if (_directionalInputRestrictedY)
        {
            directionalInput.y = 0;
        }
        

    }    

    public void OnJumpInputDown()
    {
        _jumpPressed = true;
        /// WALL LEAPS
        /// <summary>
        /// Different behavior for wall leap based on whether the directional input is or is not required
        /// If it is required, Player can both push first jump button and then direction or first give direction and then push button, rarely both at the same time
        /// If direction is given first, there is a buffer after wall slide within Jump stil registers as a wall leap
        /// If jump is given first, there is a buffer for giving input equally
        /// </summary>
        if (playerStates.IsWallSliding && !_jumpPressedBuffer)     //put in buffer if the jump input was given before leaving the wall
        {
            _jumpPressedBuffer = true;
            Invoke("JustPressedButtonOnWallSlide", wallLeapAfterButtonBuffer);
        }

        if (playerStates.IsWallSliding && wallLeapEnabled)      
        {
            if (wallLeapDirectionRequired)
            {
                if (directionalInput.x == _wallDirX * -1)                            //if the input given by Player is away from the wall
                {
                    playerJumps.WallLeap(_wallDirX);
                }
                else                                                                //this for the case that Player presses Jump first, direction later
                {
                    print("wait for directional input buffer");
                }
            }
            else                                                                 //if directional input is not required at all
            {
                playerJumps.WallLeap(_wallDirXBuffer);
            }
        }

        if (playerStates.AfterWallslideBuffer && wallLeapEnabled)                //this is for the case Player presses jump AFTER leaving the wall
        {
            if (wallLeapDirectionRequired) { 
                if (_directionalInputBuffer.x == _wallDirXBuffer * -1)           //if the input that has been given when Player left the wall was away from the it
                {
                    playerJumps.WallLeap(_wallDirXBuffer);
                }
            }
            else                                                                //if directional input is not required at all
            {
                playerJumps.WallLeap(_wallDirXBuffer);
            }
        }
        
        /// NORMAL JUMP
        if (controllerStates.IsCollidingBelow || controllerStates.JustLeftPlatform)
        {
            controllerStates.IsNormalJumping = true;
            playerJumps.NormalJump();          
        }
    }
    public void OnJumpInputUp()
    {
        _jumpPressed = false;
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
        _wallDirX = (controllerStates.IsCollidingLeft) ? -1 : 1;
        playerStates.IsWallSliding = false;
        if ((controllerStates.IsCollidingLeft || controllerStates.IsCollidingRight) && !controllerStates.IsCollidingBelow && velocity.y < 0)
        {
            playerStates.IsWallSliding = true;
            _wallDirXBuffer = _wallDirX;                        //save current wallDirX and current directional input for a buffer



            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x != _wallDirX && directionalInput.x != 0)
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
        if (playerStates.IsWallSliding && _wallDirX == directionalInput.x * -1 && _jumpPressedBuffer)   //if Player gives direction input after pressing jump while wallSliding
        {
            playerJumps.WallLeap(_wallDirX);
        }
        else if (playerStates.IsWallSliding && _wallDirX == directionalInput.x * -1 && !_jumpPressedBuffer) //normal fallOff
        {
            velocity.x = -_wallDirX * wallJumpOff.x;
            velocity.y = wallJumpOff.y;

            if (!playerStates.AfterWallslideBuffer) //putting in buffer for case the jump has not been pressed yet
            { 
                playerStates.AfterWallslideBuffer = true;
            }
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

    /// State tests

    void TimeTravelingTests()
    {
        if (playerStates.WasWallSliding && !playerStates.IsWallSliding)
        {
            //print("Just has left wallsliding");            
            _directionalInputBuffer = directionalInput;
            Invoke("JustLeftWallSlideBuffer", wallLeapAfterWallBuffer);           
        }
        if (!playerStates.WasWallSliding && playerStates.IsWallSliding)
        {
            //print("Just has STARTED wallsliding");
        }
        playerStates.WasWallSliding = playerStates.IsWallSliding;       
    }

    /// <summary>
    ///  BUFFERS
    /// </summary>
    /// 
    void JustLeftWallSlideBuffer()
    {
        playerStates.AfterWallslideBuffer = false;
        _wallDirXBuffer =0;                                //reset wallDirXBuffer for next use
        print("AfterWallsslidebuffer:" + playerStates.AfterWallslideBuffer);
    }
    void JustPressedButtonOnWallSlide()
    {
        _jumpPressedBuffer = false;
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

public void RestrictMovement(bool xAxisrestricted, bool yAxisRestricted)
    {
        if (xAxisrestricted)
        {
            _directionalInputRestrictedX = true;           
        }
        else
        {
            _directionalInputRestrictedX = false;
        }

        if (yAxisRestricted)
        {
            _directionalInputRestrictedY = true;
        }
        else
        {
            _directionalInputRestrictedY = false;
        }
    }

}
