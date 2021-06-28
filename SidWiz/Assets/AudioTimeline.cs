using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;
using EventInstance = FMOD.Studio.EventInstance;

public class AudioTimeline : MonoBehaviour, IEventSystemHandler
{

    public Rigidbody2D PlayerMovement;
    public EventInstance musicInstanceMaster;
    public delegate void OnBeat();
    public delegate void OnHihat();
    public delegate void OnJump();
    public static OnBeat Beat;
    public static OnHihat Hihat;
    public static OnJump Jump;
    public static Stopwatch sw;

    //private static bool triggerBeat = false;

    // Variables that are modified in the callback need to be part of a seperate class.
    // This class needs to be 'blittable' otherwise it can't be pinned in memory.
    [StructLayout(LayoutKind.Sequential)]
    class TimelineInfo
    {
        public int currentMusicBar = 0;
        public FMOD.StringWrapper lastMarker = new FMOD.StringWrapper();
    }

    TimelineInfo timelineInfo;
    GCHandle timelineHandle;

    FMOD.Studio.EVENT_CALLBACK beatCallback;
    private static readonly Queue<DelayedEvent> DelayedEventTriggers = new Queue<DelayedEvent>();

    void Start()
    {
        timelineInfo = new TimelineInfo();

        // Explicitly create the delegate object and assign it to a member so it doesn't get freed
        // by the garbage collected while it's being used
        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);

        musicInstanceMaster = FMODUnity.RuntimeManager.CreateInstance("event:/MasterTimeline");
       
        // Pin the class that will store the data modified during the callback
        timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
        // Pass the object through the userdata of the instance
        musicInstanceMaster.setUserData(GCHandle.ToIntPtr(timelineHandle));

        musicInstanceMaster.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND);
        musicInstanceMaster.start();
        sw = Stopwatch.StartNew();
    }

    void FixedUpdate()
    {
        while (DelayedEventTriggers.Any() && DelayedEventTriggers.Peek().IsFinished(sw))
        {
            var eventObj = DelayedEventTriggers.Dequeue();
            switch (eventObj.Name)
            {
                case "Beat":
                {
                    Beat?.Invoke();
                    break;
                }
                case "Hihat":
                {
                    Hihat?.Invoke();
                    break;
                }
                case "Jump":
                {
                    Jump?.Invoke();
                    break;
                }
            }
        }

        bool isPaused;
        var isMoving =  PlayerMovement.velocity.x > 0.1f || PlayerMovement.velocity.x < -0.1f;
        musicInstanceMaster.getPaused(out isPaused);
        if (isMoving && isPaused)
        {
            musicInstanceMaster.setPaused(false);
        }
        else if (!isMoving && !isPaused)
        {
           musicInstanceMaster.setPaused(true);
        }

        if (isMoving)
        {
            musicInstanceMaster.setParameterByName("Pitch", PlayerMovement.velocity.x / 1.6f);
        }
    }

    void OnDestroy()
    {
        musicInstanceMaster.setUserData(IntPtr.Zero);
        musicInstanceMaster.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstanceMaster.release();
        timelineHandle.Free();
    }

    void OnGUI()
    {
        var position = 0;
        musicInstanceMaster.getTimelinePosition(out position);
        GUILayout.Box(position.ToString());
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

        // Retrieve the user data
        IntPtr timelineInfoPtr;
        FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("Timeline Callback error: " + result);
        }
        else if (timelineInfoPtr != IntPtr.Zero)
        {
            const int delayMillis = 180;
            switch (type)
            {
                case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                {
                    var parameter = (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));
                    var name = (string)parameter.name;
                    DelayedEventTriggers.Enqueue(new DelayedEvent(sw, delayMillis, name));
                    break;
                }
            }
        }
        return FMOD.RESULT.OK;
    }
}

internal class DelayedEvent
{
    public readonly string Name;
    private readonly long _endTimestamp;

    public DelayedEvent(Stopwatch sw, int delay, string name)
    {
        _endTimestamp = sw.ElapsedMilliseconds + delay;
        Name = name;
    }

    public bool IsFinished(Stopwatch sw)
    {
        return sw.ElapsedMilliseconds > _endTimestamp;
    }
}