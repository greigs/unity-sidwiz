using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlatformCreator : MonoBehaviour
{
    public GameObject playerObject;
    private float xOffset = 20f;
    private bool createHihatPlatform;
    private bool createBeatPlatform;
    private bool createJump;

    void OnEnable()
    {
        AudioTimeline.Beat += OnBeatHandler;
        AudioTimeline.Hihat += OnHihatHandler;
        AudioTimeline.Jump += OnJumpHandler;
    }

    void OnDisable()
    {
        AudioTimeline.Beat -= OnBeatHandler;
        AudioTimeline.Hihat -= OnHihatHandler;
        AudioTimeline.Jump -= OnJumpHandler;
    }


    void FixedUpdate()
    {
        if (createHihatPlatform)
        {
            createHihatPlatform = false;
            var platFormPool = ObjectPool.SharedInstance.GetPooledObject();
            platFormPool.transform.SetPositionAndRotation(new Vector3(playerObject.transform.position.x + xOffset, -1, -5), Quaternion.identity);
            platFormPool.transform.localScale = new Vector3(0.2f, 1f);
            platFormPool.SetActive(true);
        }

        if (createJump)
        {
            createJump = false;
            // var platFormPool = ObjectPool.SharedInstance.GetPooledObject();
            // platFormPool.transform.SetPositionAndRotation(new Vector3(playerObject.transform.position.x + xOffset, 5, -5), Quaternion.identity);
            // platFormPool.SetActive(true);
        }

        if (createBeatPlatform)
        {
            createBeatPlatform = false;
            var platFormPool = ObjectPool.SharedInstance.GetPooledObject();
            platFormPool.transform.SetPositionAndRotation(new Vector3(playerObject.transform.position.x + xOffset, 0, -5), Quaternion.identity);
            platFormPool.transform.localScale = new Vector3(1.6f, 1f);
            platFormPool.SetActive(true);
        }
    }

    private void OnJumpHandler()
    {
        createJump = true;
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
