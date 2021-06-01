using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public PlayerController2D controller;
    public float RunSpeed = 40f;
    private float horizontalMove = 0;

    private bool jump = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * RunSpeed;
        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
    }

    void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime,false,jump);
        jump = false;

    }
}
