using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Hitbox : MonoBehaviour
{

    public bool normalAttack, smashAttack, aerialAttack;
    public int dmg;
    public string target;
    public string ignoreObjectType;
    public bool deactivateOnHit;
    public bool playersWeaponFreezOnHit;            //is this a Players weapon? Should game freez for few frames on succesfull hit?
    string hitType;

    BoxCollider2D collider;
    bool isTriggered = false;

    void Start()
    {
        collider = gameObject.GetComponent<BoxCollider2D>();
        if (normalAttack)
        {
            hitType = "Normal";
        }
        else if (smashAttack)
        {
            hitType = "Smash";
        }
        else if (aerialAttack)
        {
            hitType = "Aerial";
        }

    }
    // causes damage to target on collision with him

    void OnTriggerEnter2D(Collider2D col)
    {

        if (deactivateOnHit)
        {
            print("Deaktivuju kurva!!!");
            collider.enabled = false;
        }
        if (!isTriggered)
        {
            isTriggered = true;
            StartCoroutine(NextHitEnabled());
            if (col.tag == target == true)
            {
                print("Neco se stalo!!!");
                if (playersWeaponFreezOnHit)
                {
                    //StartCoroutine(Pause());
                }
     
                E_hitreactions e_hitreactions = col.GetComponent<E_hitreactions>();
                if (e_hitreactions != null)
                {
                    e_hitreactions.HitReaction(hitType, dmg, gameObject.transform.parent.gameObject);
                }

            }
          
        }
    }

    IEnumerator NextHitEnabled()
    {

        //yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(0.5f);

        isTriggered = false;
    }


    IEnumerator Pause()
    {
        Time.timeScale = 0.1f;
        float waitTime = Time.realtimeSinceStartup + 0.08f;
        print("Current realtime" + Time.realtimeSinceStartup);
        print("WaitTime:" + waitTime);
        yield return new WaitWhile(() => Time.realtimeSinceStartup < waitTime);
        Time.timeScale = 1f;
    }


}
