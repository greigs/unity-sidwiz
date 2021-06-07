using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public PlayerController2D controller;
    public AudioTimeline audioTimeline;
    public float RunSpeed = 0.01f;
    public float currentSpeed;
    private float horizontalMove = 0f;


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
        currentSpeed = horizontalMove * Time.fixedDeltaTime;
        controller.Move(currentSpeed,false,jump);
        jump = false;

    }
}
