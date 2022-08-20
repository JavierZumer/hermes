using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    public class FmodEventInstanceProvider
    {
        private bool m_initialized;
        private int m_totalVoices;
        private int m_voiceIndex = 0;
        private EventConfiguration m_eventConfiguration;
        private EventInstance[] m_eventInstances;

        public bool Initialized => m_initialized;
        public EventInstance[] EventInstances => m_eventInstances;

        //Constructor
        public FmodEventInstanceProvider(EventConfiguration eventConfiguration)
        {
            m_totalVoices = eventConfiguration.NumberOfVoices;
            m_eventConfiguration = eventConfiguration;

            if (eventConfiguration.EventInitializationMode == EventInitializationMode.OnEmitterAwake)
            {
                CreateFMODEventInstances();
            }
        }

        public void CreateFMODEventInstances()
        {
            //If we already created the event instances, just exit.
            if (m_initialized) {return;}

            if (m_eventConfiguration.IsGlobal)
            {
                //We want to re-use voices so let's check if this fmod event already has instances created
                m_eventInstances = AudioManager.Instance.CheckIfInstancesAlreadyExist(m_eventConfiguration.EventReference.Path);

                if (m_eventInstances != null)
                {
                    //We found a valid event config, so let's use those events.
                    m_initialized = true;
                    return;
                }
                else
                {
                    //We didn't find a valid config so we must be the first using this FMOD event. Let's create the events as normal.
                }
            }
            
            //Add proper size to the fmod event array
            m_eventInstances = new EventInstance[m_eventConfiguration.NumberOfVoices];
            
            //Tell FMOD to create the event instances we need.
            for (int i = 0; i < m_totalVoices; i++)
            {
                if (!m_eventInstances[i].isValid())
                {
                    m_eventInstances[i] = GetFmodInstance();
                }
            }

            m_initialized = true;
        }

        private EventInstance GetFmodInstance()
        {
            return RuntimeManager.CreateInstance(m_eventConfiguration.EventReference);
        }

        public EventInstance GetNextVoice()
        {
            if (!m_eventConfiguration.IsPolyphonic)
            {
                //We only have one event so return that.
                return m_eventInstances[0];
            }

            //Return the correct event instance depending on the mode.
            switch (m_eventConfiguration.EmitterVoiceStealing)
            {
                case StealingMode.Oldest:
                    return GetOldestvoice();
                case StealingMode.Quietest:
                    return GetQuietestVoice();
                case StealingMode.Furthest:
                    return GetOldestvoice();
                case StealingMode.None:
                    return GetOldestvoice();
                default:
                    return GetOldestvoice();
            }
        }

        //Get oldest voice
        private EventInstance GetOldestvoice()
        {
            int index = m_voiceIndex;
            m_voiceIndex = (m_voiceIndex + 1) % m_eventConfiguration.NumberOfVoices;
            return m_eventInstances[index];
        }

        private EventInstance GetQuietestVoice()
        {
            //Hacky...
            float lowestVolume = 1000f;

            EventInstance instanceToReturn = m_eventInstances[0];

            foreach (EventInstance instance in m_eventInstances)
            {
                instance.getVolume(out float volume);
                if (volume < lowestVolume)
                {
                    lowestVolume = volume;
                    instanceToReturn = instance;
                }
            }

            return instanceToReturn;
        }
    }
}

