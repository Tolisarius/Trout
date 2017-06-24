using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates : MonoBehaviour {

    // States
    public bool IsWalking { get; set; }
    public bool IsJumping { get; set; }
    public bool IsFalling { get; set; }

    

    public bool IsSlopeSliging { get; set; }

    public bool IsWallSliding { get; set; }
    public bool WasWallSliding { get; set; }    //was wall sliding last frame
    public bool WillWallSliding { get; set; }   //just touched the wall is about to wall slide

    public bool IsWallLeaping { get; set; }

    public bool AfterWallslideBuffer { get; set; }

    ControllerStates controllerStates;
    Player player;

    public enum State
    {
        current,
        standing,
        walking,
        jumping,
        falling,

        wallSliding,
        wallLeaping
    }

    public State currentState=State.standing;


    void Start()
    {
        controllerStates = GetComponent<ControllerStates>();
        player = GetComponent<Player>();
    }

    private void Reset()
    {
        AfterWallslideBuffer = false;
    }



}

