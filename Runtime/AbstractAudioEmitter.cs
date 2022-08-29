using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    public abstract class AbstractAudioEmitter : MonoBehaviour
    {
        protected AudioManager m_audioManager;
        protected List<EventConfiguration> m_allEvents = new List<EventConfiguration>();

        protected virtual void Awake()
        {
            m_audioManager = AudioManager.Instance;
        }

        protected void InitializeEventConfiguration(EventConfiguration eventConfiguration)
        {
            if (m_audioManager.DisableAllAudio)
            {
                //Audio system is disabled so we destroy this emitter and exit silently.
                Destroy(this);
                return;
            }

            if (eventConfiguration == null || eventConfiguration.EventRef.IsNull || String.IsNullOrEmpty(eventConfiguration.EventRef.Path))
            {
                //Should we log an error here? Would we have empty event reference fields on Event configurations ever?
                return;
            }

            //Add event to emitter event list.
            m_allEvents.Add(eventConfiguration);

            //Add event to the general event list on the audio manager
            m_audioManager.SubscribeEvent(eventConfiguration);

            //Get FMOD description so we can ask FMOD about this event. Banks should be loaded BEFORE we do this.
            eventConfiguration.EventDescription = RuntimeManager.GetEventDescription(eventConfiguration.EventRef);

            eventConfiguration.is3D = IsEvent3D(eventConfiguration);

            //Let's get an FMOD event provider. This will also create the event instances now if the config agrees.
            eventConfiguration.Provider = new FmodEventInstanceProvider(eventConfiguration);
        }

        //Play 2D
        protected void Play(EventConfiguration eventConfiguration)
        {

            if (eventConfiguration == null || eventConfiguration.EventRef.IsNull) { return; }

            if (!eventConfiguration.Provider.Initialized)
            {
                //If we didn't want to create the fmod event instances beforehand, create them now, just before playing.
                eventConfiguration.Provider.GetFMODEventInstances();
            }

            if (eventConfiguration.is3D)
            {
                Debug.LogWarning($"{eventConfiguration} seems to be a 3D event and you are trying to play it in 2D.");
            }

            eventConfiguration.Provider.GetNextInstance().start();
        }

        /// <summary>
        /// Stop this event. This will stop all instances within this emitter.
        /// If the instances are shared, it will stop all instances across all emitters.
        /// </summary>
        protected void Stop(EventConfiguration eventConfiguration)
        {
            if (eventConfiguration == null || eventConfiguration.EventRef.IsNull) { return; }

            foreach (EventInstance instance in eventConfiguration.Provider.EventInstances)
            {
                instance.stop(eventConfiguration.AllowFadeOutWhenStopping ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        protected virtual void ReleaseAllEvents()
        {
            foreach (EventConfiguration eventConfiguration in m_allEvents)
            {
                ReleaseEvent(eventConfiguration);
            }
        }

        protected void ReleaseEvent(EventConfiguration eventConfiguration)
        {
            m_audioManager.ReleaseEvent(eventConfiguration);
        }

        protected virtual void StopAllEventsOnEmitter()
        {
            foreach (EventConfiguration eventConfiguration in m_allEvents)
            {
                Stop(eventConfiguration);
            }
        }

        protected virtual void OnDestroy()
        {
            StopAllEventsOnEmitter();
            ReleaseAllEvents();
        }

        //Utilities
        private bool IsEvent3D(EventConfiguration eventConfiguration)
        {
            eventConfiguration.EventDescription.is3D(out bool is3D);
            return is3D;
        }
    }
}

