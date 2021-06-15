using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlatformCreator : MonoBehaviour
{
    public GameObject playerObject;
    public float xOffset = 10f;
    private bool createHihatPlatform = false;
    private bool createBeatPlatform = false;

    void OnEnable()
    {
        AudioTimeline.Beat += OnBeatHandler;
        AudioTimeline.Hihat += OnHihatHandler;
    }


    void OnDisable()
    {
        AudioTimeline.Beat -= OnBeatHandler;
        AudioTimeline.Hihat -= OnHihatHandler;
    }

    void FixedUpdate()
    {
        if (createHihatPlatform)
        {
            createHihatPlatform = false;
            var platFormPool = ObjectPool.SharedInstance.GetPooledObject();
            platFormPool.transform.SetPositionAndRotation(new Vector3(playerObject.transform.position.x + xOffset, 5, -5), Quaternion.identity);
            platFormPool.transform.localScale = new Vector3(0.2f, 1f);
            platFormPool.SetActive(true);
        }

        if (createBeatPlatform)
        {
            createBeatPlatform = false;
            var platFormPool = ObjectPool.SharedInstance.GetPooledObject();
            platFormPool.transform.SetPositionAndRotation(new Vector3(playerObject.transform.position.x + xOffset, 0, -5), Quaternion.identity);
            platFormPool.SetActive(true);
        }
    }

    void OnBeatHandler()
    {
        createBeatPlatform = true;
    }

    void OnHihatHandler()
    {
        createHihatPlatform = true;
    }
}
