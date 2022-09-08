using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    public class AudioManager : Singleton<AudioManager>
    {
        //This is the default name FMOD gives to the master bus and it is always the same.
        private const string c_masterBus = "bus:/";

        [SerializeField]
        public bool DisableAllAudio = false;

        //Banks to load on start.
        [BankRef] public string[] BanksToLoadOnGameStart;

        [SerializeField]
        [Range(1f, 30f)]
        [Tooltip("Unused instances will be cleaned up this often (seconds).")]
        public float m_cleanUpFrequency = 10f;

        private float m_cleanUpTimer = 0f;

        private List<EventConfiguration> m_allEventsConfigsInitialized = new List<EventConfiguration>();
        private List<EventConfiguration> m_eventsToRelease = new List<EventConfiguration>();

        void Update()
        {
            if (m_cleanUpTimer >= m_cleanUpFrequency)
            {
                for (int i = 0; i < m_eventsToRelease.Count; i++)
                {
                    if (!m_eventsToRelease[i].Provider.IsAnyInstancePlaying)
                    {
                        TryToReleaseEvent(m_eventsToRelease[i]);
                    }
                }

                if (m_eventsToRelease.Count > 0)
                {
                    m_eventsToRelease.Clear();
                }

                m_cleanUpTimer = 0.0f;
            }

            m_cleanUpTimer += Time.unscaledDeltaTime;
        }

        public void SubscribeEvent(EventConfiguration eventConfiguration)
        {
            m_allEventsConfigsInitialized.Add(eventConfiguration);
        }

        public void UnsubscribeEvent(EventConfiguration eventConfiguration)
        {
            m_allEventsConfigsInitialized.Remove(eventConfiguration);
        }

        public void SubscribeReleaseEvent(EventConfiguration eventConfiguration)
        {
            m_eventsToRelease.Add(eventConfiguration);
        }

        public void UnsubscribeReleaseEvent(EventConfiguration eventConfiguration)
        {
            m_eventsToRelease.Remove(eventConfiguration);
        }

        /// <summary>
        /// Stop all events, as long as they are not Steady,
        /// </summary>
        public void StopAllEvents()
        {
            foreach (EventConfiguration eventConfiguration in m_allEventsConfigsInitialized)
            {
                if (!eventConfiguration.Steady)
                {
                    StopEvent(eventConfiguration);
                }
            }
        }

        /// <summary>
        /// This will strictly stop ALL events. Use with caution.
        /// </summary>
        public void StopAllEvents(bool allowFadeOut)
        {
            RuntimeManager.GetBus(c_masterBus).stopAllEvents(allowFadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            //m_snapshotManager.StopAllRunningIncidentalSnaphots(); TODO
        }


        private void StopEvent(EventConfiguration eventConfiguration)
        {
            if (eventConfiguration == null || eventConfiguration.EventRef.IsNull) { return; }

            foreach (EventInstance instance in eventConfiguration.Provider.EventInstances)
            {
                instance.stop(eventConfiguration.AllowFadeOutWhenStopping ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        public EventInstancesGroup CheckIfInstancesAlreadyExist(string path)
        {
            foreach (EventConfiguration eventConfiguration in m_allEventsConfigsInitialized)
            {
                if (eventConfiguration.EventRef.Path == path) //We already initialized this event path 
                {
                    return eventConfiguration.Provider.EventInstancesGroup;
                }
            }
            return null;
        }

        public void TryToReleaseEvent (EventConfiguration eventConfiguration)
        {
            if (eventConfiguration.Provider.EventInstancesGroup.NumberOfConfigsUsing <= 1) //This is the only emitter using this, so we can clear everything.
            {
                eventConfiguration.Provider.ReleaseInstanceGroup();
                eventConfiguration.EventDescription.unloadSampleData();
            }
            else
            {
                //Other emitters are still using this EventInstancesGroup but we are not, so we reduce the number of configs using.
                eventConfiguration.Provider.EventInstancesGroup.NumberOfConfigsUsing--;
            }

            UnsubscribeEvent(eventConfiguration);
        }
    }
}

