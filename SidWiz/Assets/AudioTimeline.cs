using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.XR;

public class AudioTimeline : MonoBehaviour, IEventSystemHandler
{

    public Rigidbody2D PlayerMovement;
    public FMOD.Studio.EventInstance musicInstanceMaster;
    public delegate void OnBeat();
    public static OnBeat Beat;
    private static bool triggerBeat = false;

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

        musicInstanceMaster.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER | FMOD.Studio.EVENT_CALLBACK_TYPE.START_EVENT_COMMAND | FMOD.Studio.EVENT_CALLBACK_TYPE.SOUND_PLAYED | FMOD.Studio.EVENT_CALLBACK_TYPE.CREATED | FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND);
        musicInstanceMaster.start();
        musicInstanceMaster.setPaused(true);

    }

    void FixedUpdate()
    {
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

        if (triggerBeat)
        {
            Beat();
            triggerBeat = false;
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
        GUILayout.Box(String.Format("Current Bar = {0}, Last Marker = {1}", timelineInfo.currentMusicBar, (string)timelineInfo.lastMarker));
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
            // Get the object to store beat and marker details
            GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
            TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

            switch (type)
            {
                case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                {
                    var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));
                    var name = (string)parameter.name;
                    if (name == "BeatProgrammerAction")
                    {
                        if (Beat != null)
                        {
                            triggerBeat = true;
                        }
                        timelineInfo.currentMusicBar++;
                    }
                    break;
                }
                case FMOD.Studio.EVENT_CALLBACK_TYPE.START_EVENT_COMMAND:
                {
                    break;
                }
                case FMOD.Studio.EVENT_CALLBACK_TYPE.SOUND_PLAYED:
                {
                    var parameter = (FMOD.Studio.SOUND_INFO)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.SOUND_INFO));
                    break;
                }
                case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATED:
                {
                    break;
                }
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
                {
                    var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
                    //timelineInfo.currentMusicBar = parameter.beat;

                    break;
                }
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                {
                    var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
                    timelineInfo.lastMarker = parameter.name; 
                    break;
                }
            }
        }
        return FMOD.RESULT.OK;
    }
}