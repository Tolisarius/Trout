using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour {
    Player player;
    PlayerStates playerStates;

    [Header("Normal attack")]
    public float normalAttackHitLenght;

    [Header("Aerial attack")]
    public float aerialAttackLift;
    public float aerialAttackHitLenght;

    [Header("Smash attack")]
    public float gravityAcceleration;



    float _tempGravity;

    // Use this for initialization
    void Start()
    {
        
        player = GetComponent<Player>();
        playerStates = GetComponent<PlayerStates>();
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

        Invoke("AerialAttackEnd", aerialAttackHitLenght);

    }
    void AerialAttackEnd()
    {
        playerStates.IsAttackingAerial = false;
        player.gravity = _tempGravity;
    }

    public void SmashAttack()
    {
        playerStates.IsAttackingSmash = true;
        _tempGravity = player.gravity;
        player.gravity *= gravityAcceleration;
        
    }

    void SmashAttackEnd()
    {
        playerStates.IsAttackingSmash = false;
        player.gravity = _tempGravity;
    }

    void JustGotGrounded()      //on event
    {
        if (playerStates.IsAttackingSmash)
        { 
        Invoke("SmashAttackEnd", aerialAttackHitLenght);
        //SmashAttackEnd();
        }
    }

}
