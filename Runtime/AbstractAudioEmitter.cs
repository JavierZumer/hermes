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
        protected List<EventConfiguration> m_allEvents = new List<EventConfiguration>(); //All events local to this emitter.
        protected VelocityVector3 m_kinematicVelocity;
        protected Vector3 m_positionLastFrame = Vector3.zero;

        public bool IsKinematic //Check if there are any kinematic events in this emitter.
        {
            get
            {
                if (m_allEvents.Count > 0)
                {
                    bool isKinematic = false;
                    foreach (EventConfiguration eventConfiguration in m_allEvents)
                    {
                        if (eventConfiguration.CalculateKinematicVelocity)
                        {
                            isKinematic = true;
                        }
                    }
                    return isKinematic;
                }
                else
                {
                    return false;
                }
            }
        }

        protected virtual void Awake()
        {
            m_audioManager = AudioManager.Instance;
        }

        protected virtual void Update()
        {
            if (IsKinematic && m_kinematicVelocity != null) //We check if any event needs kinematic velocity and wait until we have already started playing that event.
            {
                UpdateKinematicVelocity();
                UpdateInstancesVelocities();
            }
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
                Debug.LogWarning($"{eventConfiguration} is a 3D event and you are trying to play it in 2D!");
            }

            eventConfiguration.Provider.GetNextInstance().start();
        }

        //Play 3D Attached to GameObject
        protected void Play(EventConfiguration eventConfiguration, Transform transform)
        {

            if (eventConfiguration == null || eventConfiguration.EventRef.IsNull) { return; }

            if (!eventConfiguration.Provider.Initialized)
            {
                //If we didn't want to create the fmod event instances beforehand, create them now, just before playing.
                eventConfiguration.Provider.GetFMODEventInstances();
            }

            if (!eventConfiguration.is3D)
            {
                Debug.LogWarning($"{eventConfiguration} is a 2D event and you are trying to play it in 3D!");
            }

            GameObject gameObject = transform.gameObject;

            if (gameObject == null)
            {
                Debug.LogError($"{eventConfiguration} is being played through a null game object");
                return;
            }

            var rigidBody = gameObject.GetComponent<Rigidbody>();
            var rigidBody2D = gameObject.GetComponent<Rigidbody2D>();

            EventInstance eventInstance = eventConfiguration.Provider.GetNextInstance();

            if (rigidBody && !eventConfiguration.CalculateKinematicVelocity)
            {
                RuntimeManager.AttachInstanceToGameObject(eventInstance, transform, rigidBody);
            }
            else if (rigidBody2D && !eventConfiguration.CalculateKinematicVelocity)
            {
                RuntimeManager.AttachInstanceToGameObject(eventInstance, transform, rigidBody2D);
            }
            else if (eventConfiguration.CalculateKinematicVelocity) //No rigidbody AND we want to calculate kinematic velocity
            {
                m_kinematicVelocity = new VelocityVector3();
                eventConfiguration.transform = transform;
                RuntimeManager.AttachInstanceToGameObject(eventInstance, transform);
                eventInstance.set3DAttributes(ToKinematic3DAttributes(transform, m_kinematicVelocity)); //Set velocity and position just before we play.
            }
            else
            {
                RuntimeManager.AttachInstanceToGameObject(eventInstance, transform);
            }

            eventInstance.start();
        }

        //Play 3D on a position
        protected void Play(EventConfiguration eventConfiguration, Vector3 position)
        {

            if (eventConfiguration == null || eventConfiguration.EventRef.IsNull) { return; }

            if (!eventConfiguration.Provider.Initialized)
            {
                //If we didn't want to create the fmod event instances beforehand, create them now, just before playing.
                eventConfiguration.Provider.GetFMODEventInstances();
            }

            if (!eventConfiguration.is3D)
            {
                Debug.LogWarning($"{eventConfiguration} is a 2D event and you are trying to play it in 3D!");
            }

            EventInstance eventInstance = eventConfiguration.Provider.GetNextInstance();
            eventInstance.set3DAttributes(position.To3DAttributes());
            eventInstance.start();
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

        private void UpdateKinematicVelocity()
        {
            //Get current velocity
            Vector3 currentVel;
            currentVel.x = m_kinematicVelocity.x;
            currentVel.y = m_kinematicVelocity.y;
            currentVel.z = m_kinematicVelocity.z;

            //Update to new velocity
            currentVel = Vector3.Lerp(currentVel, (transform.position - m_positionLastFrame) / Time.deltaTime, Time.deltaTime * 15);

            //Reassign to kinematic velocity class
            m_kinematicVelocity.x = currentVel.x;
            m_kinematicVelocity.y = currentVel.y;
            m_kinematicVelocity.z = currentVel.z;

            //Store world position for next frame
            m_positionLastFrame = transform.position;
        }

        private void UpdateInstancesVelocities()
        {
            foreach (EventConfiguration eventConfiguration in m_allEvents)
            {
                if (eventConfiguration.CalculateKinematicVelocity)
                {
                    foreach (EventInstance instance in eventConfiguration.Provider.EventInstances)
                    {
                        //We find each instance and update its position and velocity by hand.
                        //Keep in mind that for any other event configs, not using kinematic velocity, this is done by FMOD's RuntimeManager.
                        instance.set3DAttributes(ToKinematic3DAttributes(eventConfiguration.transform, m_kinematicVelocity));
                    }
                }
            }
        }

        public static FMOD.ATTRIBUTES_3D ToKinematic3DAttributes(Transform transform, VelocityVector3 kinematicVelocity)
        {
            FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D();
            attributes.forward = transform.forward.ToFMODVector();
            attributes.up = transform.up.ToFMODVector();
            attributes.position = transform.position.ToFMODVector();

            FMOD.VECTOR vel;
            vel.x = kinematicVelocity.x;
            vel.y = kinematicVelocity.y;
            vel.z = kinematicVelocity.z;

            attributes.velocity = vel;

            return attributes;
        }
    }

    public class VelocityVector3
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
    }
}

