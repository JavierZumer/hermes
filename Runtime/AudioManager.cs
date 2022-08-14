using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField]
        public bool DisableAllAudio = false;

        //Banks to load on start.
        [BankRef] public string[] BanksToLoadOnGameStart;
    }
}

