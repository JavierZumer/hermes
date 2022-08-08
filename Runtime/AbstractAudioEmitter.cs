using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    /*[Serializable]
    public struct ActionAndStreams
    {
        public GameAction GameAction;
        public List<AudioEventStream> Stream;
    }*/

    public abstract class AbstractAudioEmitter : MonoBehaviour
    {
        public List<AudioEventStream> m_streams = new List<AudioEventStream>();
    }
}

