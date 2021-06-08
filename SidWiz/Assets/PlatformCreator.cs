using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlatformCreator : MonoBehaviour
{

    public GameObject myPrefab;
    public GameObject playerObject;
    public int xOffset = 10;
    private int countDownToGap = 4;

    void OnEnable()
    {
        AudioTimeline.Beat += OnBeatHandler;
    }

    void OnDisable()
    {
        AudioTimeline.Beat -= OnBeatHandler;
    }

    void OnBeatHandler()
    {
        countDownToGap--;
        if (countDownToGap == 0)
        {
            countDownToGap = Random.Range(4, 8);
            Instantiate(myPrefab, new Vector3(playerObject.transform.position.x + xOffset, 4, -5), Quaternion.identity);
        }
        else
        {
            Instantiate(myPrefab, new Vector3(playerObject.transform.position.x + xOffset, 0, -5), Quaternion.identity);
        }
        
    }
}
