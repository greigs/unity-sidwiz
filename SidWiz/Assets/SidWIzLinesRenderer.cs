using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SidWizPlus;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SidWIzLinesRenderer : MonoBehaviour
{
    public LineRenderer LineRenderer;
    public EdgeCollider2D EdgeCollider;
    public PolygonCollider2D PolyCollider;
    Program pr;

    public AudioSource soundSystem;
    private Stopwatch sw;
    private double audioLengthMillis;


    void Awake()
    {
        pr = new Program();
        pr.Main();
        StartMusic();

        sw = Stopwatch.StartNew();
    }

    void StartMusic()
    {
        var file = @"WavGen\sonic - SN76496 #0.wav";
        var aud = new AudioFileReader(file);
        audioLengthMillis = aud.TotalTime.TotalMilliseconds;
        var ad = new float[aud.Length];
        aud.Read(ad, 0, (int)aud.Length);

        var clip = AudioClip.Create(Path.GetFileNameWithoutExtension(file), (int)aud.Length, aud.WaveFormat.Channels, aud.WaveFormat.SampleRate, false);
        clip.SetData(ad, 0);
        soundSystem.clip = clip;
        soundSystem.Play();
    }

    // Increase the number of calls to Update.
    void FixedUpdate()
    {
        var lines = pr.Renderer.RenderFrameLines((float)((sw.ElapsedMilliseconds) / audioLengthMillis));
        
        LineRenderer.positionCount = lines[0].Length;
        LineRenderer.SetPositions(lines[0].Select(x => new Vector3(x.X / 7f, x.Y / 7f)).ToArray());
        //EdgeCollider.points = lines[0].Select(x => new Vector2(x.X / 7f, x.Y / 7f)).ToArray();
        var points = lines[0].Select(x => new Vector2(x.X / 7f, x.Y / 7f)).ToList();
        PolyCollider.points = new List<Vector2>() {new Vector2(0, 0)}.Concat(points)
            .Concat(new List<Vector2>(){ new Vector2(100, 0), new Vector2(50, 0)}).ToArray();
    }


}