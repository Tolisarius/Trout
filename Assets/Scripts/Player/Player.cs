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
    public float wallLeapAfterWallBuffer, wallLeapAfterButtonBuffer;
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

    Animator animator;

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
        animator = GetComponent<Animator>();

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
        PlayerStates();

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
        // WALL CLIMB

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
        if (controllerStates.IsGrounded || controllerStates.JustLeftPlatform)
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
        playerStates.IsWallSliding = false;             //  this is the problem here for the remaining in the wallslide animation state
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
        if (playerStates.WasWallSliding && !playerStates.IsWallSliding)            //print("Just has left wallsliding");   
        {

            _directionalInputBuffer = directionalInput;
            Invoke("JustLeftWallSlideBuffer", wallLeapAfterWallBuffer);
        }
        if (!playerStates.WasWallSliding && playerStates.IsWallSliding)         //print("Just has STARTED wallsliding");
        {

            SendMessage("JustHitTheWallSliding");
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
        _wallDirXBuffer = 0;                                //reset wallDirXBuffer for next use
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

    /// <summary>
    ///  Player FSM
    /// </summary>
    /// 
    void PlayerStates()
    {
        switch (playerStates.currentState)
        {
            case global::PlayerStates.State.standing: Standing(); break;
            case global::PlayerStates.State.walking: Walking(); break;
            case global::PlayerStates.State.falling: Falling(); break;
            case global::PlayerStates.State.jumping: Jumping(); break;
            case global::PlayerStates.State.wallSliding: WallSliding(); break;
            case global::PlayerStates.State.wallLeaping: WallLeap(); break;

            default: break;
        }
    }

    void Standing()
    {
        //print("STANDING");
        animator.SetBool("IsStanding", true);
        if (WalkingTransition())
        {
            animator.SetBool("IsStanding", false);
            playerStates.currentState = global::PlayerStates.State.walking;
        }
        if (JumpTransition())
        {
            animator.SetBool("IsStanding", false);
            playerStates.currentState = global::PlayerStates.State.jumping;
        }
    }
    void Walking()
    {
        //print("WALKING");
        animator.SetBool("IsRunning", true);
        if (StandingTransition())
        {
            SwitchState("IsRunning", global::PlayerStates.State.standing);
        }
        if (FallingTransition())
        {
            SwitchState("IsRunning", global::PlayerStates.State.falling);            
        }
        if (JumpTransition())
        {
            SwitchState("IsRunning", global::PlayerStates.State.jumping);           
        }
    }

    void Falling()
    {
        /*print("FALLING");
        if (playerStates.IsWallLeaping)
        {
            playerStates.IsWallLeaping = false;
        }
        */
        animator.SetBool("IsFalling", true);
        if (WalkingTransition())
        {
            SwitchState("IsFalling", global::PlayerStates.State.walking);
        }
        else if (StandingTransition())
        {
            SwitchState("IsFalling", global::PlayerStates.State.standing);
        }
        if (WallSlidingTransition())
        {
            SwitchState("IsFalling", global::PlayerStates.State.wallSliding);
        }
    }

    void Jumping()
    {
        //print("JUMPING");
        animator.SetBool("IsJumping", true);
        if (FallingTransition())
        {
            SwitchState("IsJumping", global::PlayerStates.State.falling);
        }
        else if (StandingTransition())
        {
            SwitchState("IsJumping", global::PlayerStates.State.standing);
        }
        if (WallSlidingTransition())
        {
            SwitchState("IsJumping", global::PlayerStates.State.wallSliding);
        }

    }
    void WallSliding()
    {
        print("WALL SLIDING PYCO!!!");
        animator.SetBool("IsWallSliding", true);

        if (JumpTransition())
        {
            SwitchState("IsWallSliding", global::PlayerStates.State.jumping);
        }
       
        if (StandingTransition())
        {
            SwitchState("IsWallSliding", global::PlayerStates.State.standing);
        }
        if (WallLeapTransition())
        {
            SwitchState("IsWallSliding", global::PlayerStates.State.wallLeaping);
        }

        else if (FallingTransition())
       {
            SwitchState("IsWallSliding", global::PlayerStates.State.falling);
       }
    }

    void WallLeap()
    {
        print("WALL LEAPING");
        animator.SetBool("IsWallLeaping", true);

        if (StandingTransition())
        {
            SwitchState("IsWallLeaping", global::PlayerStates.State.standing);
        }
        if (WallSlidingTransition())
        {
            SwitchState("IsWallLeaping", global::PlayerStates.State.wallSliding);
        }
     
        if (FallingTransition())
        {
            StartCoroutine(SwitchStateDelayed("IsWallLeaping", global::PlayerStates.State.falling, 0.2f));            
        }     
    }

    /// <summary>
    ///  State transitions
    /// </summary>

    bool StandingTransition()
    {
        return (controllerStates.IsGrounded && Mathf.Abs(velocity.x) < 0.3);
    }
    bool WalkingTransition()
    {
        return (controllerStates.IsGrounded && Mathf.Abs(velocity.x) >= 0.3);
    }

    bool FallingTransition()
    {
        return (controllerStates.IsFalling && !playerStates.IsWallSliding);
    }

    bool JumpTransition()
    {
        return (controllerStates.IsNormalJumping);
    }
    bool WallSlidingTransition()
    {
        return (playerStates.IsWallSliding);
    }
    bool WallLeapTransition()
    {
        return playerStates.IsWallLeaping;
    }

    void SwitchState(string currentAnimStateOff, global::PlayerStates.State state)
    {
        
        animator.SetBool(currentAnimStateOff, false);
        playerStates.currentState = state;
    }
    IEnumerator SwitchStateDelayed(string currentAnimStateOff, global::PlayerStates.State state, float time)
    {
        yield return new WaitForSeconds(time);
        animator.SetBool(currentAnimStateOff, false);
        playerStates.currentState = state;
    }

}
