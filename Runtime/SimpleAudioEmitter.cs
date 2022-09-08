using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    public class SimpleAudioEmitter : AbstractAudioEmitter
    {
        public enum PlayMethod3D
        {
            AttachToGameObject,
            PlayOnPosition,
        }

        [SerializeField][Tooltip("If the event is 3D you can choose to play it attached to a game object or just in a position.")]
        private PlayMethod3D m_play3DType = PlayMethod3D.AttachToGameObject;

        [SerializeField][Tooltip("Choose an specific transform to play the audio on.")]
        private Transform m_customTransform;

        [SerializeField]
        private EventConfiguration m_eventConfiguration;

        protected override void Awake()
        {
            base.Awake();
            InitializeEventConfiguration(m_eventConfiguration);
            Play(); //REMOVE
        }

        public void Play()
        {
            if (!IsEvent3D(m_eventConfiguration))
            {
                Play(m_eventConfiguration);
            }
            else if (m_play3DType == PlayMethod3D.AttachToGameObject)
            {
                Play(m_eventConfiguration, m_customTransform ? m_customTransform : transform);
            }
            else
            {
                Play(m_eventConfiguration, m_customTransform ? m_customTransform.transform.position : transform.position);
            }
        }

        public void Stop()
        {
            if (IsEventPlaying(m_eventConfiguration))
            {
                Stop(m_eventConfiguration);
            }
        }

        /// <summary>
        /// Call this method from outside to update parameters when needed.
        /// </summary>
        /*public void SetParameter(AudioParameter parameter, float value)
        {
            SetParameter(m_description, parameter, value);
        }*/
    }
}

