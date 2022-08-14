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

        private void Awake()
        {
            m_audioManager = AudioManager.Instance;
        }

        protected void InitializeEventConfiguration(EventConfiguration eventConfiguration)
        {
            //Create FMOD instances if required
            if (m_audioManager.DisableAllAudio)
            {
                //Audio system is disabled so we destroy this emitter and exit silently.
                Destroy(this);
                return;
            }

            if (eventConfiguration.EventReference.IsNull || String.IsNullOrEmpty(eventConfiguration.EventReference.Path))
            {
                //Should we log an error here? Would we have empty event reference fields on Event configurations ever?
                return;
            }

            //Get FMOD description so we can ask FMOD about this event.
            eventConfiguration.EventDescription = RuntimeManager.GetEventDescription(eventConfiguration.EventReference);

            eventConfiguration.is3D = IsEvent3D(eventConfiguration);

            //Let's get an FMOD event provider. This will also create the event instances if the configuration is set to do this.
            eventConfiguration.Provider = new FmodEventInstanceProvider(eventConfiguration);
        }

        //Play 2D
        protected void Play(EventConfiguration eventConfiguration)
        {
            if (!eventConfiguration.Provider.Initialized)
            {
                //If we didn't want to create the fmod event instances beforehand, create them now, just before playing.
                eventConfiguration.Provider.CreateFMODEventInstances();
            }

            if (eventConfiguration.is3D)
            {
                Debug.LogWarning($"{eventConfiguration} seems to be a 3D event and you are trying to play it in 2D.");
            }

            eventConfiguration.Provider.GetNextVoice().start();
        }

        //Utilities
        public bool IsEvent3D(EventConfiguration eventConfiguration)
        {
            eventConfiguration.EventDescription.is3D(out bool is3D);
            return is3D;
        }
    }
}

