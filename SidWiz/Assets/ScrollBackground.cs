using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBackground : MonoBehaviour
{

    public GameObject followTarget;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
    void Update()
    {
        Vector2 offset = new Vector2((followTarget.transform.position.x) / 100f , 0);
        var renderer = GetComponent<Renderer>();

        renderer.material.mainTextureOffset = offset;
    }

}
