using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_controller : MonoBehaviour {
    
    public float walkSpeed;
    public float accelerationTimeGrounded = .1f;

    public float gravity=10;
    Vector2 movementVector;

    [HideInInspector]
    public Vector3 velocity;
    float velocityXSmoothing;


    Controller2D controller;
    // Use this for initialization
    void Start ()
    {
        controller = GetComponent<Controller2D>();
        movementVector = new Vector2(0f, 0f);
    }
	
	// Update is called once per frame
	void Update ()
    {
        EveryFrame();
        controller.Move(velocity * Time.deltaTime, movementVector);
    }

    void EveryFrame()
    {
        CalculateVelocity();
    }

    void CalculateVelocity()
    {
        float targetVelocityX = movementVector.x*walkSpeed;       
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, accelerationTimeGrounded);
        velocity.y += gravity * Time.deltaTime;
    }
  


    

}
