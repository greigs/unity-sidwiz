using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundColour : MonoBehaviour
{
    private bool change;

    void OnEnable()
    {
        AudioTimeline.Beat += OnBeatHandler;
    }

    void OnDisable()
    {
        AudioTimeline.Beat -= OnBeatHandler;
    }

    private void OnBeatHandler()
    {
        change = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (change)
        {
            var renderer = GetComponent<Renderer>();
            var color = Random.ColorHSV(0f, 1f, 1, 1,1,1);
            renderer.material.SetColor("_ReflectColor", color);
            //renderer.material.SetColor("_Main Color", color);
            renderer.material.color = color;
            change = false;
        }

        //renderer.material.SetColor(1, color);
    }
}
