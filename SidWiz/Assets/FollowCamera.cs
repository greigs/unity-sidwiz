using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    public GameObject followTarget;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.SetPositionAndRotation(new Vector3(followTarget.transform.position.x, followTarget.transform.position.y, transform.position.z), Quaternion.identity);;
        
    }
}
