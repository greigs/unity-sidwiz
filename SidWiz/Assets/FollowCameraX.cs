using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCameraX : MonoBehaviour
{

    public float offset = 0f;
    public GameObject followTarget;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.SetPositionAndRotation(new Vector3(followTarget.transform.position.x + offset, transform.position.y, transform.position.z), Quaternion.identity); ;

	}
}
