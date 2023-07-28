using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T s_instance;

        public static T Instance { get; private set; }

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this as T;
            }
        }
    }
}

