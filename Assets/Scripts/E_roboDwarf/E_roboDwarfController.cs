using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_roboDwarfController : MonoBehaviour
{
    E_roboDwarfStates enemyState;
    Controller2D controller2D;
    ControllerStates controllerStates;
    Animator animator;

    bool surrogate;     //delete


    // Use this for initialization
    void Start()
    {
        enemyState = GetComponent<E_roboDwarfStates>();
        controller2D = GetComponent<Controller2D>();
        controllerStates = GetComponent<ControllerStates>();
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {

    }


    void EnemyStates()
    {
        switch (enemyState.currentState)
        {

            case E_roboDwarfStates.State.idle: Idle(); break;
            case E_roboDwarfStates.State.attacking: Attacking(); break;
            case E_roboDwarfStates.State.dying: Dying(); break;
            case E_roboDwarfStates.State.falling: Falling(); break;
            case E_roboDwarfStates.State.hitreacting: HitReacting(); break;
            case E_roboDwarfStates.State.walking: Walking(); break;



            default: print("SOMETHING IS FUCKED UP WITH ROBODWARF STATE!!!"); break;

        }
    }

    void Idle()
    {
        if (T_hitReacting())
        {

        }
    }
    void Attacking()
    {

    }
    void Dying()
    {

    }
    void Falling()
    {

    }
    void HitReacting()
    {

    }
    void Walking()
    {

    }

    bool T_idle()
    {
        return surrogate ;
    }

    bool T_attacking()
    {
        return surrogate;
    }

    bool T_dying()
    {
        return enemyState.IsDying;
    }

    bool T_falling()
    {
        return controllerStates.IsFalling;
    }

    bool T_hitReacting()
    {
        return enemyState.IsHitReacting;
    }

    bool T_walking()
    {
        return surrogate;
    }




    void SwitchState(string currentAnimStateOff, E_roboDwarfStates.State state)
    {
        animator.SetBool(currentAnimStateOff, false);
        //playerStates.currentState = state;
    }


}

