using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour {
    Player player;
    PlayerStates playerStates;
    Animator animator;

    [Header("Normal attack")]
    public float normalAttackHitLenght;

    [Header("Aerial attack")]
    public float aerialAttackLift;
    public float aerialAttackHitLenght;
    public float aerialAttackHorizontalSpeedMult;

    [Header("Smash attack")]
    public float gravityAcceleration;
    public float smashAttackLayOnGround;


    float _tempGravity,_tempVelocity;

    // Use this for initialization
    void Start()
    {
        
        player = GetComponent<Player>();
        playerStates = GetComponent<PlayerStates>();
        animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update ()
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
        playerStates.IsAttackingNormal = true;
        Invoke("NormalAttackEnd", normalAttackHitLenght);
    }

    void NormalAttackEnd()
    {
        playerStates.IsAttackingNormal = false;
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
        player.RestrictMovement(true, false,true);
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

}
