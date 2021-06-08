using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCreator : MonoBehaviour
{

    public GameObject myPrefab;
    public GameObject playerObject;
    public int xOffset = 10;

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
        Instantiate(myPrefab, new Vector3(playerObject.transform.position.x + xOffset, 0, -5), Quaternion.identity);
    }
}
