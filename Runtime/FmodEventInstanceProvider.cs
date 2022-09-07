using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    public class EventInstancesGroup
    {
        public EventInstance[] EventInstances;
        public int NumberOfConfigsUsing;
    }

    public class FmodEventInstanceProvider
    {
        private bool m_initialized;
        private int m_totalVoices;
        private int m_voiceIndex = 0;
        private EventConfiguration m_eventConfiguration;
        private EventInstancesGroup m_instanceGroup = null;

        public bool Initialized => m_initialized;
        public EventInstance[] EventInstances => m_instanceGroup.EventInstances;
        public EventInstancesGroup EventInstancesGroup => m_instanceGroup;

        //Constructor
        public FmodEventInstanceProvider(EventConfiguration eventConfiguration)
        {
            m_totalVoices = eventConfiguration.NumberOfVoices;
            m_eventConfiguration = eventConfiguration;
            m_instanceGroup = new EventInstancesGroup();

            if (eventConfiguration.EventInitializationMode == EventInitializationMode.OnEmitterAwake)
            {
                GetFMODEventInstances();
            }
        }

        public void GetFMODEventInstances()
        {
            //If we already have the event instances, just exit.
            if (m_initialized) {return;}

            if (m_eventConfiguration.IsShared)
            {
                //We want to re-use instances so let's check if this fmod event already has instances created
                m_instanceGroup = AudioManager.Instance.CheckIfInstancesAlreadyExist(m_eventConfiguration.EventRef.Path);

                if (m_instanceGroup.EventInstances != null)
                {
                    //We found a valid event config, so let's use those instances.
                    m_initialized = true;
                    m_instanceGroup.NumberOfConfigsUsing++;
                    return;
                }
                else
                {
                    //We didn't find a valid config so we must be the first emitter using this FMOD event. Let's create the event instances.
                }
            }

            //Add proper size to the fmod event array
            m_instanceGroup.EventInstances = new EventInstance[m_eventConfiguration.NumberOfVoices];
            m_instanceGroup.NumberOfConfigsUsing = 1;
            
            //Tell FMOD to create the event instances we need.
            for (int i = 0; i < m_totalVoices; i++)
            {
                if (!m_instanceGroup.EventInstances[i].isValid())
                {
                    m_instanceGroup.EventInstances[i] = GetFmodInstance();
                }
            }

            m_initialized = true;
        }

        private EventInstance GetFmodInstance()
        {
            return RuntimeManager.CreateInstance(m_eventConfiguration.EventRef);
        }

        public EventInstance GetNextInstance()
        {
            if (!m_eventConfiguration.IsPolyphonic)
            {
                //We only have one instance so return that.
                return m_instanceGroup.EventInstances[0];
            }

            //Return the correct event instance depending on the mode.
            switch (m_eventConfiguration.EmitterVoiceStealing)
            {
                case StealingMode.Oldest:
                    return GetOldestInstance();
                case StealingMode.Quietest:
                    return GetQuietestInstance();
                case StealingMode.Furthest:
                    return GetOldestInstance();
                case StealingMode.None:
                    return GetOldestInstance();
                default:
                    return GetOldestInstance();
            }
        }

        //Get oldest voice
        private EventInstance GetOldestInstance()
        {
            int index = m_voiceIndex;
            m_voiceIndex = (m_voiceIndex + 1) % m_eventConfiguration.NumberOfVoices;
            return m_instanceGroup.EventInstances[index];
        }

        //Quietest
        private EventInstance GetQuietestInstance()
        {
            //Hacky...
            float lowestVolume = 1000f;

            EventInstance instanceToReturn = m_instanceGroup.EventInstances[0];

            foreach (EventInstance instance in m_instanceGroup.EventInstances)
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

        //Furthest
        private EventInstance GetFurthestInstance()
        {
            //TODO...
            EventInstance instance = m_instanceGroup.EventInstances[0];
            return instance;
        }

        public void ReleaseInstanceGroup()
        {
            for (int i = 0; i < m_instanceGroup.EventInstances.Length; i++)
            {
                EventInstance instance = m_instanceGroup.EventInstances[i];
                RuntimeManager.DetachInstanceFromGameObject(instance); //Detaches normal (non kinetic) FMOD instances.
                instance.release();
                instance.clearHandle();
            }
            Array.Clear(m_instanceGroup.EventInstances, 0, m_totalVoices);
            m_instanceGroup.NumberOfConfigsUsing = 0;
            m_instanceGroup = null;
            m_initialized = false;
        }
    }
}

