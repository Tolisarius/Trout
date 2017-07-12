using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_roboDwarfController : MonoBehaviour
{
    E_roboDwarfStates enemyState;
    E_controller enemyController;
    Controller2D controller2D;
    ControllerStates controllerStates;
    Animator animator;
    GameObject player;

    public float knockback, knockUp;

    bool surrogate; // smazat!

    // Use this for initialization
    void Start()
    {
        enemyState = GetComponent<E_roboDwarfStates>();
        controller2D = GetComponent<Controller2D>();
        enemyController = GetComponent<E_controller>();
        controllerStates = GetComponent<ControllerStates>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");


        enemyState.currentState = E_roboDwarfStates.State.idle;

    }

    // Update is called once per frame
    void Update()
    {
        EnemyStates();
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

        animator.SetBool("IsIdle", true);

        if (T_hitReacting())
        {
            print("Budu reagovat!");
            SwitchState("IsIdle", E_roboDwarfStates.State.hitreacting);
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
        animator.SetBool("IsHitReacting", true);
        if (!T_hitReacting())
        {
            SwitchState("IsHitReacting", E_roboDwarfStates.State.idle);
        }
    }

    void Walking()
    {

    }

    bool T_idle()
    {
        return surrogate;
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
        enemyState.currentState = state;
    }

    void TakeDamage(int dmg)
    {
        print("Take damage!");
        if (enemyState.HitPoints > 0)
        {
            print("Hit reaction!");
            enemyState.IsHitReacting = true;
            Knockback();
        }
        else
        {
            enemyState.IsDying = true;
        }
    }


    void JustGotGrounded()
    {
        if (enemyState.IsHitReacting)
        {
           enemyState.IsHitReacting = false;
        }
    }

    void Knockback()
    {
        print("Knockback");
        Vector2 _knockBack = new Vector2(0f, 0f);
        float playerDir = Mathf.Sign(gameObject.transform.position.x - player.transform.position.x);
        print("Player position:"+ playerDir);
        _knockBack.x = knockback * playerDir;
        _knockBack.y = knockUp;
        //_knockBack.y = enemyController.gravity * 0.3f * -1;
        enemyController.velocity = _knockBack;
    }

}

