﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Player))]

public class PlayerJumps : MonoBehaviour {

    Player player;
    PlayerStates playerStates;

    void Start ()
    {
		player = GetComponent<Player>();      
        playerStates = GetComponent<PlayerStates>();
    }

    private void Update()
    {

        if (playerStates.IsWallLeaping)
        {
                      
            print("Velocity:" + player.velocity.x);
            if (Mathf.Abs(player.velocity.x)< player.wallLeap.x*0.2f)
            {
                WallLeapEnd();
            }
        }
    }

    public void WallLeap(int wallDirX)
    {
        playerStates.IsWallLeaping = true;
        player.velocity.x = -wallDirX * player.wallLeap.x;
        player.velocity.y = player.wallLeap.y;
        player.RestrictMovement(true, false,false);
    }

    void WallLeapEnd()
    {
        playerStates.IsWallLeaping = false;
        player.RestrictMovement(false, false,false);

    }

    public void WallClimb(int wallDirX)
    {
        player.velocity.x = -wallDirX * player.wallJumpClimb.x;
        player.velocity.y = player.wallJumpClimb.y;
    }

    public void NormalJump()
    {
        if (player.controllerStates.slidingDownMaxSlope)
        {
            if (player.directionalInput.x != -Mathf.Sign(player.controllerStates.slopeNormal.x))
            {
                // not jumping against max slope
                player.velocity.y = player.maxJumpVelocity * player.controllerStates.slopeNormal.y;
                player.velocity.x = player.maxJumpVelocity * player.controllerStates.slopeNormal.x;
            }
        }
        else
        {
            player.velocity.y = player.maxJumpVelocity;
        }
    }

    // REACTIONS ON EVENTS

    void JustHitTheWallSliding()
    {

        if (playerStates.IsWallLeaping)
        {
            WallLeapEnd();
        }
    }
    void JustGotGrounded()
    {
        if (playerStates.IsWallLeaping)
        {
            WallLeapEnd();
        }       
    }

}
