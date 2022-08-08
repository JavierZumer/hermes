using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hermes
{
    [Serializable]
    public class AudioEventStream : MonoBehaviour
    {

        public GameAction GameAction;

        public enum AudioAction
        {
            Play,
            Stop,
            SendParameter,
        }

        public AudioAction Action;

        [Tooltip("FMOD Event Reference")]
        public EventReference EventReference;
        [Tooltip("Snapshot that would be played as long as the above event plays.")]
        public EventReference HighlightSnapshot;

        //[Header("Instances")]

        [Tooltip("Load Sample data on Initialization. Use this for time sensitive audio. Otherwise it will load when audio is first played.")]
        public bool PreloadSampleData = false;

        /*[Tooltip("When to instance the Audio Event")]
        public AudioEventInstancing CreateAudioEventInstance = AudioEventInstancing.OnCallerStart;

        [Tooltip("If true, we will try to use a voice already created for this same description.")]
        public bool ReuseVoices = false;

        [Tooltip("How Many instances of this event should play at the same time from a single source.")]
        [Range(1, 30)]
        public int EventPolyphony = 1;

        [Tooltip("When should we create the FMOD instance.")]
        public FmodEventInstancing CreateFmodInstance = FmodEventInstancing.JustInTime;

        [Tooltip("When should we release the AudioEvent(s).")]
        public ReleaseMode ReleaseMode = ReleaseMode.NotUntilExplicitelyTold;

        [Tooltip("How many instances of this event should exist. 0 Means no limit.")]
        [Range(0, 30)]
        public int MaxNumberOfInstancesAllowed = 0;

        [Header("Other Options")]

        [Tooltip("Prevents event instances from being stopped by AudioManager::StopAllEventInstances.")]
        [SerializeField]
        public bool Persistant = false;

        [Tooltip("Allow Fade Out when stopping.")]
        [SerializeField]
        public bool AllowFadeOutWhenStopping = true;

        public EventDescription m_fmodEventDescription;*/
    }
}

