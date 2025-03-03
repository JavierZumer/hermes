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
        protected List<EventConfiguration> m_allEmitterEvents = new List<EventConfiguration>(); //All events local to this emitter.
        protected VelocityVector3 m_kinematicVelocity;
        protected Vector3 m_positionLastFrame = Vector3.zero;
        private List<KinematicAttachedInstance> m_kinematicAttachedInstances = new List<KinematicAttachedInstance>();

        public bool IsKinematic //Check if there are any kinematic events in this emitter.
        {
            get
            {
                if (m_allEmitterEvents.Count > 0)
                {
                    bool isKinematic = false;
                    foreach (EventConfiguration eventConfiguration in m_allEmitterEvents)
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
            //TODO: This is doing the IsKinematic check every frame. Since an emitter event configurations won't change their kinematic nature in runtime,
            //it would be better to check this once on start and just cache the bool.
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
                //TODO: Should we log an error here? Would we have empty event reference fields on Event configurations ever?
                return;
            }

            //Add event to emitter event list.
            m_allEmitterEvents.Add(eventConfiguration);

            //Add event to the general event list on the audio manager
            m_audioManager.SubscribeEvent(eventConfiguration);

            //Get FMOD description so we can ask FMOD about this event. TODO: Banks should be loaded BEFORE we do this.
            eventConfiguration.EventDescription = RuntimeManager.GetEventDescription(eventConfiguration.EventRef);

            eventConfiguration.is3D = IsEvent3D(eventConfiguration);

            //Let's get an FMOD event provider. This will also create the event instances now if the config agrees.
            eventConfiguration.Provider = new FmodEventInstanceProvider(eventConfiguration);
        }

        //Play 2D
        protected virtual void Play(EventConfiguration eventConfiguration)
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
            ManageRelease(eventConfiguration);
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
                Debug.LogError($"{eventConfiguration} was tried to be played through a null game object.");
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
                AttachKineticInstanceToGameObject(eventInstance, transform, m_kinematicVelocity, eventConfiguration); //Set velocity and position just before we play.
            }
            else
            {
                RuntimeManager.AttachInstanceToGameObject(eventInstance, transform);
            }

            eventInstance.start();
            ManageRelease(eventConfiguration);
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

            ManageRelease(eventConfiguration);
        }

        private void AttachKineticInstanceToGameObject(FMOD.Studio.EventInstance instance, Transform transform, VelocityVector3 kinematicVelocity, EventConfiguration eventConfiguration)
        {
            KinematicAttachedInstance attachedInstance = m_kinematicAttachedInstances.Find(x => x.Instance.handle == instance.handle);
            if (attachedInstance == null)
            {
                attachedInstance = new KinematicAttachedInstance();
                m_kinematicAttachedInstances.Add(attachedInstance);
            }

            instance.set3DAttributes(ToKinematic3DAttributes(transform, kinematicVelocity));
            attachedInstance.Transform = transform;
            attachedInstance.Instance = instance;
            attachedInstance.VelocityVector3 = kinematicVelocity;
            attachedInstance.UseKinematicVelocity = true;
            attachedInstance.EventConfiguration = eventConfiguration;
        }

        private void DetachKineticInstanceFromGameObject(EventConfiguration eventConfiguration)
        {
            for (int i = 0; i < m_kinematicAttachedInstances.Count; i++)
            {
                if (m_kinematicAttachedInstances[i].EventConfiguration == eventConfiguration)
                {
                    m_kinematicAttachedInstances[i] = m_kinematicAttachedInstances[m_kinematicAttachedInstances.Count - 1];
                    m_kinematicAttachedInstances.RemoveAt(m_kinematicAttachedInstances.Count - 1);
                }
            }
        }

        private void ManageRelease(EventConfiguration eventConfiguration)
        {
            if (eventConfiguration.EventReleaseMode == EventReleaseMode.AsSoonAsWePlay)
            {
                ReleaseEvent(eventConfiguration);
            }

            if (eventConfiguration.EventReleaseMode == EventReleaseMode.WhenAudioFinishes)
            {
                m_audioManager.SubscribeReleaseEvent(eventConfiguration);
                //In theory, we would need a way to detach a kinetic event here when it has finished.
                //But the update method on this class should take care of that as soon as all the instances are stopped??
            }
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
            foreach (EventConfiguration eventConfiguration in m_allEmitterEvents)
            {
                ReleaseEvent(eventConfiguration);
            }
        }

        protected void ReleaseEvent(EventConfiguration eventConfiguration)
        {
            m_audioManager.TryToReleaseEvent(eventConfiguration);

            //If the event config was kinetic, we make sure we dettach the instances.
            if (eventConfiguration.CalculateKinematicVelocity)
            {
                DetachKineticInstanceFromGameObject(eventConfiguration);
            }
        }

        protected virtual void StopAllEventsOnEmitter()
        {
            foreach (EventConfiguration eventConfiguration in m_allEmitterEvents)
            {
                Stop(eventConfiguration);
            }
        }

        protected virtual bool IsEventPlaying(EventConfiguration eventConfiguration)
        {
           return eventConfiguration.Provider.IsAnyInstancePlaying;
        }

        protected virtual void OnDestroy()
        {
            StopAllEventsOnEmitter();
            ReleaseAllEvents();
        }

        //Utilities
        protected virtual bool IsEvent3D(EventConfiguration eventConfiguration)
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
            for (int i = 0; i < m_kinematicAttachedInstances.Count; i++)
            {
                FMOD.Studio.PLAYBACK_STATE playbackState = FMOD.Studio.PLAYBACK_STATE.STOPPED;
                if (m_kinematicAttachedInstances[i].Instance.isValid())
                {
                    m_kinematicAttachedInstances[i].Instance.getPlaybackState(out playbackState);
                }

                if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED ||
                    m_kinematicAttachedInstances[i].Transform == null // destroyed game object
                    )
                {
                    m_kinematicAttachedInstances[i] = m_kinematicAttachedInstances[m_kinematicAttachedInstances.Count - 1];
                    m_kinematicAttachedInstances.RemoveAt(m_kinematicAttachedInstances.Count - 1);
                    i--;
                    continue;
                }


                if (m_kinematicAttachedInstances[i].UseKinematicVelocity && m_kinematicAttachedInstances[i].VelocityVector3 != null)
                {
                    m_kinematicAttachedInstances[i].Instance.set3DAttributes(ToKinematic3DAttributes(m_kinematicAttachedInstances[i].Transform, m_kinematicAttachedInstances[i].VelocityVector3));
                }
                else
                {
                    m_kinematicAttachedInstances[i].Instance.set3DAttributes(RuntimeUtils.To3DAttributes(m_kinematicAttachedInstances[i].Transform));
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

    public class KinematicAttachedInstance
    {
        public FMOD.Studio.EventInstance Instance;
        public EventConfiguration EventConfiguration;
        public Transform Transform;
        public VelocityVector3 VelocityVector3;
        public bool UseKinematicVelocity;
    }
}

