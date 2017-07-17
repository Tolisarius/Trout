using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_hitreactions : MonoBehaviour {

    E_roboDwarfStates enemyState;
    E_controller enemyController;
    public float knockback, knockUp;
    ParticleSystem particleBleeding;

    public float smashKnockback, smashKnockUp;

    GameObject splashPokus;

    void Start()
    {
        enemyState = GetComponent<E_roboDwarfStates>();
        enemyController = GetComponent<E_controller>();
        particleBleeding = gameObject.transform.FindChild("particle_bleeding").gameObject.GetComponent<ParticleSystem>();       
    }

    // Update is called once per frame
    void Update() {

    }

/*
    public void NormalHitreaction(int dmg, GameObject sender)
    {
        print("Take damage!");
        if (enemyState.HitPoints > 0)
        {
            print("Hit reaction!");
            particleBleeding.Play();

            enemyState.IsHitReacting = true;

            Vector2 _knockBack = new Vector2(0f, 0f);
            float playerDir = Mathf.Sign(gameObject.transform.position.x - sender.transform.position.x);
            print("Player position:" + playerDir);
            _knockBack.x = knockback * playerDir;
            _knockBack.y = knockUp;
            enemyController.velocity = _knockBack;
        }
        else
        {
            enemyState.IsDying = true;
        }
    }

    public void SmashAttackHitreaction(int dmg, GameObject sender)
    {
        print("Take smash damage!");
        if (enemyState.HitPoints > 0)
        {
            print("Hit reaction!");
            enemyState.IsHitReacting = true;

            Vector2 _knockBack = new Vector2(0f, 0f);
            float playerDir = Mathf.Sign(gameObject.transform.position.x - sender.transform.position.x);
            print("Player position:" + playerDir);
            _knockBack.x = smashKnockback * playerDir;
            _knockBack.y = smashKnockUp;
            enemyController.velocity = _knockBack;
        }
        else
        {
            enemyState.IsDying = true;
        }
    }

    public void AerialAttackHitreaction(int dmg, GameObject sender)
    {

    }
    */

    public void HitReaction(string type, int dmg, GameObject sender)
    {
        Vector2 _knockBack = new Vector2(0f, 0f);
        //enemyState.HitPoints -= dmg;
        if (enemyState.HitPoints > 0)
        {
            if (type=="Normal")
            {
                particleBleeding.Play();
                enemyState.IsHitReacting = true;               
                float playerDir = Mathf.Sign(gameObject.transform.position.x - sender.transform.position.x);           
                _knockBack.x = knockback * playerDir;
                _knockBack.y = knockUp;              
            }
            else if (type == "Smash")
            {
                particleBleeding.Play();
                enemyState.IsHitReacting = true;
                _knockBack.x = smashKnockback;
                _knockBack.y = smashKnockUp;
            }
            else if (type == "Aerial")
            {
                particleBleeding.Play();
                enemyState.IsHitReacting = true;
                float playerDir = Mathf.Sign(gameObject.transform.position.x - sender.transform.position.x);
                _knockBack.x = knockback * playerDir;
                _knockBack.y = knockUp;
            }


            enemyController.velocity = _knockBack;

        }
        else
        {
            enemyState.IsDying = true;
        }



    }



}
