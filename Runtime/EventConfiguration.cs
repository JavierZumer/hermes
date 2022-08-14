using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    /// <summary>
    /// This describes the rules on how to create, play and release a particular FMOD event.
    /// </summary>
    [Serializable]
    public class EventConfiguration
    {
        //Internal varialbles
        public FmodEventInstanceProvider Provider;
        public EventDescription EventDescription;
        public bool is3D;

        //User facing variables
        [Tooltip("FMOD Event Reference")]
        public EventReference EventReference;

        [Tooltip("Snapshot that would be played as long as the above event plays.")]
        public EventReference HighlightSnapshot;

        [Tooltip("Load Sample data on Initialization. Use this for time sensitive audio. Otherwise it will load when audio is first played.")]
        public bool PreloadSampleData = false;

        //Voice Management
        public Polyphony Polyphony = Polyphony.Monophonic;
        public int NumberOfVoices = 1;
        public bool ReuseEventConfiguration = false;
        public EmitterVoiceStealing EmitterVoiceStealing = EmitterVoiceStealing.Oldest;

        //Instance Creation/Release

        [Tooltip("When to instantiate the Fmod Audio Events")]
        public EventInitializationMode EventInitializationMode = EventInitializationMode.OnEmitterAwake;

        /*[Tooltip("When should we release the AudioEvent(s).")]
        public ReleaseMode ReleaseMode = ReleaseMode.NotUntilExplicitelyTold;*/

        [Tooltip("How many instances of this event should exist. 0 Means no limit.")]
        [Range(0, 30)]
        public int MaxNumberOfInstancesAllowed = 0;

        //[Header("Other Options")]

        [Tooltip("Prevents event instances from being stopped by AudioManager::StopAllEventInstances.")]
        [SerializeField]
        public bool Persistant = false;

        [Tooltip("Allow Fade Out when stopping.")]
        [SerializeField]
        public bool AllowFadeOutWhenStopping = true;

        //public EventDescription m_fmodEventDescription;
    }

    public enum Polyphony
    {
        Monophonic,
        Polyphonic
    }

    public enum EventInitializationMode
    {
        OnEmitterAwake,
        JustInTime,
        NewEventEachTimeWePlay
    }

    public enum EmitterVoiceStealing
    {
        Oldest,
        Quietest,
        Furthest,
        None
    }
}

