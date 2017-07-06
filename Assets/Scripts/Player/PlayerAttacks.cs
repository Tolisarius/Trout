using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour {
    Player player;
    PlayerStates playerStates;
    Animator animator;
    BoxCollider2D normalAttackHitbox;
    GameObject areialAttackHitbox;
    GameObject smashAttackHitbox;
    
 

    [Header("Normal attack")]
    public float normalAttackHitLenght;
    public float shiftMin, shiftMax;
    public LayerMask collisionMask, specificColMask;


    [Header("Aerial attack")]
    public float aerialAttackLift;
    public float aerialAttackHitLenght;
    public float aerialAttackHorizontalSpeedMult;

    [Header("Smash attack")]
    public float gravityAcceleration;
    public float smashAttackLayOnGround;


    float _tempGravity, _tempVelocity;

    // Use this for initialization
    void Start()
    {

        player = GetComponent<Player>();
        playerStates = GetComponent<PlayerStates>();
        animator = GetComponent<Animator>();

        normalAttackHitbox = gameObject.transform.FindChild("normalAttackHitbox").gameObject.GetComponent<BoxCollider2D>();
        //areialAttackHitbox = gameObject.transform.FindChild("areialAttackHitbox").gameObject;
        //smashAttackHitbox = gameObject.transform.FindChild("smashAttackHitbox").gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NormalAttack()
    {
        /*
         set player state that it is normal attacking
         get the direction of Player
         cast raycast in that direction to a distance of attackSlide
         if there is an object with tag enemy attackSlide is enabled
         if attack slide is enabled, slide towards enemy while while still in attack pose, anly after that keep attack animation for the attack duration
         if attack slide is not enabled, just finish the attack aniamation (duration set as open variable)            
        */       
        player.RestrictMovement(true, false, true);
        TargetTest();
        playerStates.IsAttackingNormal = true;
        Invoke("NormalAttackEnd", normalAttackHitLenght);
    }

    void NormalAttackEnd()
    {
        playerStates.IsAttackingNormal = false;
        normalAttackHitbox.enabled = false;
        player.shiftVelocity = 0f;
        player.RestrictMovement(false, false, false);

    }
    public void AerialAttack()
    {

        playerStates.IsAttackingAerial = true;
        _tempGravity = player.gravity;
        player.velocity.y = aerialAttackLift;
        player.gravity = 0;
        player.velocity.x *= aerialAttackHorizontalSpeedMult;
        player.RestrictMovement(true, false, true);

        Invoke("AerialAttackEnd", aerialAttackHitLenght);

    }
    void AerialAttackEnd()
    {
        playerStates.IsAttackingAerial = false;
        player.gravity = _tempGravity;
        player.RestrictMovement(false, false, false);
    }

    public void SmashAttack()
    {
        player.RestrictMovement(true, false, true);
        playerStates.IsAttackingSmash = true;
        _tempGravity = player.gravity;
        player.gravity *= gravityAcceleration;

    }

    void SmashAttackLanded()
    {
        player.gravity = _tempGravity;
        animator.SetBool("IsAttackingSmash", false);      //first disable the animation state here, so character stands up but only then it should go to Player state .standing
    }

    void SmashAttackEnd()
    {
        playerStates.IsAttackingSmash = false;
    }

    void JustGotGrounded()      //on event
    {
        if (playerStates.IsAttackingSmash)
        {
            Invoke("SmashAttackLanded", smashAttackLayOnGround);
        }
    }

    void TargetTest()
    {
        //Vector2 rayOrigin = normalAttackHitbox.transform.position;
        Bounds bounds = normalAttackHitbox.bounds;
        Vector2 rayOrigin =  new Vector2((player.currentDirection=="left")?bounds.min.x:bounds.max.x, bounds.center.y);
        
        Vector2 sightDir = new Vector2((player.currentDirection == "left") ? -1f : 1f, 0f);
        float rayLengt = shiftMax;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, sightDir, rayLengt, collisionMask | specificColMask);

            if (hit)
            {
                rayLengt = hit.distance;
                if (hit.transform.gameObject.tag == "Enemy")
                {
                    print("enemy reachable");
                    NormalAttackShift(rayLengt);
                    
                }
            }
            else
            {
                print("normal shift");
                NormalAttackShift(shiftMin);
            }
       Debug.DrawRay(rayOrigin, sightDir * shiftMax, Color.blue);
     }


    void NormalAttackShift(float shift)
    {
        //player.shiftVelocity = shift * ((player.currentDirection == "left") ? -1f : 1f);
        //player.velocity.x = shift *((player.currentDirection == "left") ? -1f : 1f);
        shift*= ((player.currentDirection == "left") ? -1f : 1f);
        gameObject.transform.Translate(shift, 0f, 0f);

    }

}
