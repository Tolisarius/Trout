using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController
{

    public float maxSlopeAngle = 80;
    public float _afterPlatformBuffer;

    [HideInInspector]
    public Vector2 playerInput;

    public ControllerStates controllerStates;

    Vector2 _smallMovement, _oldPosition;

    bool _wasOnMovingPlatformLastFrame, _wasGrounded;


    public override void Start()
    {
        base.Start();
        controllerStates = new ControllerStates();
        controllerStates.faceDir = 1;

        _smallMovement = new Vector2(0.0001f, 0.0001f);
    }
    private void Update()
    {
        TimeTravellingTests();
    }

    void TimeTravellingTests()
    {
        //has controller just grounded?
        if (!_wasGrounded && controllerStates.IsGrounded)
        {
            controllerStates.JustGotGrounded = true;
            //print("Just got grounded");
        }
        else
        {
            controllerStates.JustGotGrounded = false;
        }

        if (_wasGrounded && !controllerStates.IsGrounded && !controllerStates.IsNormalJumping)
        {
            //print("Just left platform");
            controllerStates.JustLeftPlatform = true;
            Invoke("JustLeftPlatformBuffer", _afterPlatformBuffer);
        }
        /// has controller just left a moving platform
        if (_wasOnMovingPlatformLastFrame == true && controllerStates.isStandingOnPlatform == false)
        {
            //print("prave opustil movable platformu");
        }

        /// is controller falling?
        Vector2 _currentPosition = gameObject.transform.position;
        if ((_currentPosition.y + _smallMovement.y < _oldPosition.y) && controllerStates.isStandingOnPlatform == false)
        {
            controllerStates.IsFalling = true;
        }

        _oldPosition = _currentPosition;
        _wasOnMovingPlatformLastFrame = controllerStates.isStandingOnPlatform;
        _wasGrounded = controllerStates.IsGrounded;
    }

    void JustLeftPlatformBuffer()
    {
        controllerStates.JustLeftPlatform = false;
    }

    public void Move(Vector2 moveAmount, bool standingOnPlatform)
    {
        Move(moveAmount, Vector2.zero, standingOnPlatform);
        
    }

    public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();       
        controllerStates.Reset();       

        controllerStates.isStandingOnPlatform = standingOnPlatform;

        controllerStates.moveAmountOld = moveAmount;
        playerInput = input;

        if (moveAmount.y < 0)
        {
            DescendSlope(ref moveAmount);
        }

        if (moveAmount.x != 0)
        {
            controllerStates.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        HorizontalCollisions(ref moveAmount);
        if (moveAmount.y != 0)
        {
            VerticalCollisions(ref moveAmount);
        }

        transform.Translate(moveAmount);

        if (standingOnPlatform)
        {
            controllerStates.IsCollidingBelow = true;
        }  
    }

    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = controllerStates.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        if (Mathf.Abs(moveAmount.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (hit)
            {

                if (hit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (controllerStates.descendingSlope)
                    {
                        controllerStates.descendingSlope = false;
                        moveAmount = controllerStates.moveAmountOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != controllerStates.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }

                if (!controllerStates.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (controllerStates.climbingSlope)
                    {
                        moveAmount.y = Mathf.Tan(controllerStates.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                    }

                    controllerStates.IsCollidingLeft = directionX == -1;
                    controllerStates.IsCollidingRight = directionX == 1;               
                }
            }
        }
    }

    void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {

            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if (hit)
            {
                if (hit.collider.tag == "Through")
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                    if (controllerStates.fallingThroughPlatform)
                    {
                        continue;
                    }
                    if (playerInput.y == -1)
                    {
                        controllerStates.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", .5f);
                        continue;
                    }
                }

                moveAmount.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (controllerStates.climbingSlope)
                {
                    moveAmount.x = moveAmount.y / Mathf.Tan(controllerStates.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
                }

                controllerStates.IsCollidingBelow = directionY == -1;
                controllerStates.IsCollidingAbove = directionY == 1;    
            }
        }

        if (controllerStates.climbingSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != controllerStates.slopeAngle)
                {
                    moveAmount.x = (hit.distance - skinWidth) * directionX;

                    controllerStates.slopeAngle = slopeAngle;
                    controllerStates.slopeNormal = hit.normal;
                }
            }
        }
    }

    void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (moveAmount.y <= climbmoveAmountY)
        {
            moveAmount.y = climbmoveAmountY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
            controllerStates.IsCollidingBelow = true;
            controllerStates.climbingSlope = true;
            controllerStates.slopeAngle = slopeAngle;
            controllerStates.slopeNormal = slopeNormal;
        }
    }

    void DescendSlope(ref Vector2 moveAmount)
    {

        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
            SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
        }

        if (!controllerStates.slidingDownMaxSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendmoveAmountY;

                            controllerStates.slopeAngle = slopeAngle;
                            controllerStates.descendingSlope = true;
                            controllerStates.IsCollidingBelow = true;
                            controllerStates.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                controllerStates.slopeAngle = slopeAngle;
                controllerStates.slidingDownMaxSlope = true;
                controllerStates.slopeNormal = hit.normal;
            }
        }

    }


    /// <summary>
    /// Resets that are conditional
    /// </summary>

    void ResetFallingThroughPlatform()
    {
        controllerStates.fallingThroughPlatform = false;
    }

}
