using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Trig_DmgTarget : MonoBehaviour
{

    public int dmg;
    public string target;
    public string ignoreObjectType;
    public bool selfDestroyOnHit, selfDeactivateOnHit;
    public bool playersWeaponFreezOnHit;            //is this a Players weapon? Should game freez for few frames on succesfull hit?

    BoxCollider2D collider;
    bool isTriggered = false;

    void Start()
    {
        collider = gameObject.GetComponent<BoxCollider2D>();
        

    }
    // causes damage to target on collision with him
    
    void OnTriggerEnter2D(Collider2D col)
    {
        


        if (selfDeactivateOnHit)
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
                
                col.SendMessageUpwards("TakeDamage",dmg);      
                
            }
            if (col.tag != ignoreObjectType && selfDestroyOnHit)
            {
                Destroy(gameObject);
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
            print("Current realtime"+Time.realtimeSinceStartup);
            print("WaitTime:"+waitTime);
            yield return new WaitWhile(() => Time.realtimeSinceStartup < waitTime);
            Time.timeScale = 1f;
        }
    

}
