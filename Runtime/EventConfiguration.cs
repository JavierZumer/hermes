using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        private EventInstance EditorInstance;
        public EventDescription EventDescription;

        [NonSerialized]
        public bool is3D;

        public int EmittersUsing;

        //Properties
        public bool IsGlobal
        {
            get
            {
                if (InstanceShareMode == InstanceShareMode.GlobalMonophonic || InstanceShareMode == InstanceShareMode.GlobalPolyphonic)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsPolyphonic
        {
            get
            {
                if (InstanceShareMode == InstanceShareMode.LocalPolyphonic || InstanceShareMode == InstanceShareMode.GlobalPolyphonic)
                {
                    return true;
                }
                return false;
            }
        }

        public int NumberOfVoices
        {
            get
            {
                if (IsPolyphonic)
                {
                    return PolyphonyVoices;
                }
                else
                {
                    return 1;
                }
            }
        }

        public string EventPath
        {
            get
            {
                if (!EventRef.IsNull)
                {
                    return EventRef.Path;
                }
                return string.Empty;
            }
        }

        public bool ValidPath
        {
            get
            {
                return !String.IsNullOrEmpty(EventPath);
            }
        }

        //User facing variables
        [Tooltip("FMOD Event Reference")]
        public EventReference EventRef;

        [Tooltip("Snapshot that would be played as long as the above event plays.")]
        public EventReference HighlightSnapshot;

        //Instance Management

        [Tooltip("When to instantiate the Fmod Audio Events")]
        public EventInitializationMode EventInitializationMode = EventInitializationMode.OnEmitterAwake;

        public InstanceShareMode InstanceShareMode = InstanceShareMode.LocalMonophonic;

        public int PolyphonyVoices = 2;
        public StealingMode EmitterVoiceStealing = StealingMode.Oldest;

        [Tooltip("When should we release the AudioEvent(s).")]
        public EventReleaseMode EventReleaseMode = EventReleaseMode.NotUntilExplicitelyTold;

        //Other Options
        [Tooltip("Load Sample data on Initialization. Use this for time sensitive audio. Otherwise it will load when audio is first played.")]
        public bool PreloadSampleData = false;

        [Tooltip("Prevents event instances from being stopped by AudioManager::StopAllEventInstances.")]
        public bool Steady = false;

        [Tooltip("Allow Fade Out when stopping.")]
        public bool AllowFadeOutWhenStopping = true;

        //Add Stops events outside max distance.

        public void PlayEventInEditor()
        {
            if (String.IsNullOrEmpty(EventPath))
            {
                return;
            }
            EditorInstance = EditorFmodSystem.PreviewEvent(EventRef);
        }

        public void StopEventInEditor()
        {
            if (String.IsNullOrEmpty(EventPath))
            {
                return;
            }
            EditorFmodSystem.PreviewStop(EditorInstance, AllowFadeOutWhenStopping ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    public enum EventInitializationMode
    {
        OnEmitterAwake,
        JustInTime,
    }

    public enum EventReleaseMode
    {
        NotUntilExplicitelyTold,
        WhenAudioFinishes,
        AfterPlaying,
    }

    public enum StealingMode
    {
        Oldest,
        Quietest,
        Furthest,
        None
    }

    public enum InstanceShareMode
    {
        LocalMonophonic,
        GlobalMonophonic,
        LocalPolyphonic,
        GlobalPolyphonic,
        NewEventEachTimeWePlay
    }
}

