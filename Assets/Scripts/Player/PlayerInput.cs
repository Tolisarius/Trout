using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour
{

    Player player;
    PlayerStates playerStates;

    Vector2 directionalInput;

    void Start()
    {
        player = GetComponent<Player>();
        playerStates = GetComponent<PlayerStates>();
    }

    void Update()
    {
        directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);
        ButtonTests();
        
    }
    void ButtonTests()
    {
        if (Input.GetButtonDown("Jump") && !playerStates.JumpRestricted)
        {
            player.OnJumpInputDown();
        }
        if (Input.GetButtonUp("Jump"))
        {
            player.OnJumpInputUp();
        }

        if (Input.GetButtonDown("Attack"))
        {
            if (Mathf.Sign(directionalInput.y) == -1)
            {
                player.OnAttackInputDownWithDirectionDown();
            }
            else
            {
                player.OnAttackInputDown();
            }
        }
    }
}
