using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    public class SimpleAudioEmitter : AbstractAudioEmitter
    {
        [SerializeField]
        private EventConfiguration m_eventConfiguration;

        /*[SerializeField]
        private EventConfiguration[] m_array;*/

        //public bool m_costa;

        /*[SerializeField]
        private EventReference m_ref;*/

        /*[SerializeField]
        private EventReference[] m_refArray;*/

        protected override void Awake()
        {
            base.Awake();
            InitializeEventConfiguration(m_eventConfiguration);
            Play(m_eventConfiguration,transform);
        }

    }
}

