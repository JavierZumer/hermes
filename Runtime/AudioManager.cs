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

        private List<EventConfiguration> m_allEventsConfigsInitialized = new List<EventConfiguration>();

        public void SubscribeEvent (EventConfiguration eventConfiguration)
        {
            m_allEventsConfigsInitialized.Add(eventConfiguration);
        }

        public void UnsubscribeEvent(EventConfiguration eventConfiguration)
        {
            m_allEventsConfigsInitialized.Remove(eventConfiguration);
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
            if (eventConfiguration == null || eventConfiguration.EventReference.IsNull) { return; }

            foreach (EventInstance instance in eventConfiguration.Provider.EventInstances)
            {
                instance.stop(eventConfiguration.AllowFadeOutWhenStopping ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        public EventInstance[] CheckIfInstancesAlreadyExist(string path)
        {
            foreach (EventConfiguration eventConfiguration in m_allEventsConfigsInitialized)
            {
                if (eventConfiguration.EventReference.Path == path)
                {
                    return eventConfiguration.Provider.EventInstances;
                }
            }
            return null;
        }

        public  void ReleaseEvent (EventConfiguration eventConfiguration)
        {
            //TODO: Do checks related to event config re-use to figure out if we can clear this event config.
            //If event config has one or less users -> Release it, remove it from m_allEventsConfigsInitialized.
            //else if event config is not global -> Debe haber otros emitters usando las instancias, que hacemos?
            //else --> hay mas emitters usando esta voz...
        }
    }

    public class GlobalEventConfig
    {

    }

}

