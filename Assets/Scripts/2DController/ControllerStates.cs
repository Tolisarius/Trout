using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerStates : MonoBehaviour
{


    ///All properties regarding controller and controller alone- collisions, movement states etc.
    /// is the character colliding right ?

    public bool IsCollidingRight { get; set; }
    /// is the character colliding left ?
    public bool IsCollidingLeft { get; set; }
    /// is the character colliding with something above it ?
    public bool IsCollidingAbove { get; set; }
    /// is the character colliding with something above it ?
    public bool IsCollidingBelow { get; set; }
    /// is the character colliding with anything ?
    public bool HasCollisions { get { return IsCollidingRight || IsCollidingLeft || IsCollidingAbove || IsCollidingBelow; } }


    public bool climbingSlope { get; set; }
    public bool descendingSlope { get; set; }
    public bool slidingDownMaxSlope { get; set; }

    public float slopeAngle { get; set; }
    public float slopeAngleOld { get; set; }
    [HideInInspector]
    public Vector2 slopeNormal;
    public Vector2 moveAmountOld { get; set; }
    public int faceDir { get; set; }
    public bool fallingThroughPlatform { get; set; }
    public bool isStandingOnPlatform { get; set; }

    public bool OnAMovingPlatform { get; set; }

    /// Is the character grounded ? 
    public bool IsGrounded { get { return IsCollidingBelow; } }
    /// is the character falling right now ?
    public bool IsFalling { get; set; }

    /// was the character grounded last frame ?
    public bool WasGroundedLastFrame { get; set; }
    /// was the character grounded last frame ?
    public bool WasTouchingTheCeilingLastFrame { get; set; }
    /// did the character just become grounded ?
    public bool JustGotGrounded { get; set; }
    ///has charcter just left platform
    public bool JustLeftPlatform { get; set; }
    /// is character performing a normal jump?
    public bool IsNormalJumping { get; set; }


    /// Reset all collision states to false   
    public virtual void Reset()
    {       
        IsCollidingLeft =
        IsCollidingRight =
        IsCollidingAbove =


        climbingSlope = false;
        descendingSlope = false;
        slidingDownMaxSlope = false;
        slopeNormal = Vector2.zero;

        slopeAngleOld = slopeAngle;
        slopeAngle = 0;

        /// ground collision must be reset only when it is not on the ground or moving platform, ie falling, jumping, aerial attacks etc.
        if (IsFalling || IsNormalJumping)
        {
            IsCollidingBelow = false;
        }

        ///reseting falling
        if (JustGotGrounded || IsGrounded)
        {
            IsFalling = false;
        }

        ///reseting jumping
        if (JustGotGrounded || IsGrounded || IsFalling)
        {
            IsNormalJumping = false;
        }
    }


}
