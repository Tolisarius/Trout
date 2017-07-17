using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour
{

    Player player;
    PlayerStates playerStates;

    Vector2 directionalInput;
    bool directionLeft, directionRight, directionDown, directionUp;

    public float horizontalInputMinimum,  verticalInputMinimum;

    void Start()
    {
        player = GetComponent<Player>();
        playerStates = GetComponent<PlayerStates>();
    }

    void Update()
    {
        directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (Mathf.Abs(directionalInput.x) < horizontalInputMinimum)
        {
            directionalInput.x = 0;
        }

        player.SetDirectionalInput(directionalInput);
        DirectionsTest();
        ButtonTests();
        
    }

    void DirectionsTest()
    {
        if (directionalInput.x <= -horizontalInputMinimum)
        {
            directionLeft = true;
        } else
        {
            directionLeft = false;
        }
        if (directionalInput.x >= horizontalInputMinimum)
        {
            directionRight = true;
        }
        else
        {
            directionRight = false;
        }
        if (directionalInput.y >= verticalInputMinimum && Mathf.Abs(directionalInput.x)< horizontalInputMinimum)
        {
            directionUp = true;
        }
        else
        {
            directionUp = false;
        }
        if (directionalInput.y <= -verticalInputMinimum && Mathf.Abs(directionalInput.x) < horizontalInputMinimum)
        {
            directionDown = true;
        }
        else
        {
            directionDown = false;
        }
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
            //if (Mathf.Sign(directionalInput.y) == -1)
            if (directionDown)
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
