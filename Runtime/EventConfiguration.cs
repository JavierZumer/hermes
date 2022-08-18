using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    /// <summary>
    /// This describes the rules on how to create, play and release a particular FMOD event on this emitter.
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

        //Voice Management
        public Polyphony Polyphony = Polyphony.Monophonic;
        public int NumberOfVoices = 2;
        public EmitterVoiceStealing EmitterVoiceStealing = EmitterVoiceStealing.Oldest;

        //Instance Management
        [Tooltip("When to instantiate the Fmod Audio Events")]
        public EventInitializationMode EventInitializationMode = EventInitializationMode.OnEmitterAwake;

        [Tooltip("When should we release the AudioEvent(s).")]
        public EventReleaseMode EventReleaseMode = EventReleaseMode.NotUntilExplicitelyTold;

        public bool ReuseInstances = false;

        [Tooltip("Max instances for this emitter. " +
            "If you need to limit total instances for this event, you will need to set it on FMOD Studio.")]
        [Range(0, 30)]
        public int MaxNumberOfInstancesOnThisEmitter = 0;

        //Other Options
        [Tooltip("Load Sample data on Initialization. Use this for time sensitive audio. Otherwise it will load when audio is first played.")]
        public bool PreloadSampleData = false;

        [Tooltip("Prevents event instances from being stopped by AudioManager::StopAllEventInstances.")]
        public bool Steady = false;

        [Tooltip("Allow Fade Out when stopping.")]
        public bool AllowFadeOutWhenStopping = true;
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

    public enum EventReleaseMode
    {
        NotUntilExplicitelyTold,
        WhenAudioFinishes,
        AfterPlaying,
    }

    public enum EmitterVoiceStealing
    {
        Oldest,
        Quietest,
        Furthest,
        None
    }
}

