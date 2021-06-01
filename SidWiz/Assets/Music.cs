using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using UnityEngine;

public class Music : MonoBehaviour {



    public AudioSource soundSystem;

    // Use this for initialization
    void Start ()
    {
        var file = @"C:\repo\unity-sidwiz\SidWiz\SidWiz\SidWizPlus\bin\Debug\sonic - SN76496 #0.wav";
        var aud = new AudioFileReader(file); 
        var ad = new float[aud.Length];
        aud.Read(ad, 0, (int)aud.Length);

        var clip = AudioClip.Create(Path.GetFileNameWithoutExtension(file), (int)aud.Length, aud.WaveFormat.Channels, aud.WaveFormat.SampleRate, false);
        clip.SetData(ad,0);
        soundSystem.clip = clip;
        soundSystem.Play();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
