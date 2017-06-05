using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Player))]

public class PlayerJumps : MonoBehaviour {

    Player player;    

    void Start ()
    {
		player = GetComponent<Player>();
    }

    public void WallLeap(int wallDirX)
    {
        player.velocity.x = -wallDirX * player.wallLeap.x;
        player.velocity.y = player.wallLeap.y;
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
}
