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
        private FmodEventInstanceProvider m_provider = null;
        private EventInstance EditorInstance;
        public EventDescription EventDescription;
        public bool LastShareInstances = false;
        public string LastEventPath = "";

        [NonSerialized]
        public bool is3D;

        //Properties
        public bool IsShared
        {
            get
            {
                return ShareEventInstances;
            }
        }

        public bool IsPolyphonic
        {
            get
            {
                if (PolyphonyModes == PolyphonyMode.Polyphonic)
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

        public FmodEventInstanceProvider Provider
        {
            get
            {
                return m_provider;
            }
            set
            {
                m_provider = value;
            }
        }

        //Path Assignments
        [Tooltip("FMOD Event Reference")]
        public EventReference EventRef;

        [Tooltip("Snapshot that would be played as long as the above event plays.")]
        public EventReference HighlightSnapshot;

        //Instance Management

        public bool ShareEventInstances;

        [Tooltip("When to instantiate the Fmod Audio Events")]
        public EventInitializationMode EventInitializationMode = EventInitializationMode.OnEmitterAwake;

        public PolyphonyMode PolyphonyModes = PolyphonyMode.Monophonic;

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

        public bool StopEventsAtMaxDistance;

        public bool CalculateKinematicVelocity;

        public EventConfiguration ThisClassInstance
        {
            get
            {
                return this;
            }
        }

        public void PlayEventInEditor()
        {
            if (String.IsNullOrEmpty(EventPath))
            {
                return;
            }

            EditorInstance.getPlaybackState(out PLAYBACK_STATE state);

            if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING)
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
        JustBeforePlayingEvent,
    }

    public enum EventReleaseMode
    {
        NotUntilExplicitelyTold,
        WhenAudioFinishes,
        AsSoonAsWePlay,
    }

    public enum StealingMode
    {
        Oldest,
        Quietest,
        Furthest,
        None
    }

    public enum PolyphonyMode
    {
        Monophonic,
        Polyphonic,
    }
}

