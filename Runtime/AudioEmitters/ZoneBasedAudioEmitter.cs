using Hermes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hermes
{
    /// <summary>
    /// Plays 2D ambiences, mix and reverb snapshots based on a 3D or 2D collider.
    /// </summary>
    public class ZoneBasedAudioEmitter : AbstractAudioEmitter
    {
        [SerializeField]
        private EventConfiguration m_eventConfiguration;

        protected override void Awake()
        {
            base.Awake();
            InitializeEventConfiguration(m_eventConfiguration);
        }

        //TODO 3D Version
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (CollisionCheck(other))
            {
                PlayerEnteredZone(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (CollisionCheck(other))
            {
                PlayerExitedZone(other);
            }
        }

        private void PlayerEnteredZone(Collider2D other)
        {
            Play(m_eventConfiguration); //TODO: Create a 2D Ambience manager or this give responsability to AudioManager
        }

        private void PlayerExitedZone(Collider2D other)
        {
            Stop(m_eventConfiguration); //TODO: Create a 2D Ambience manager or this give responsability to AudioManager
        }

        /// <summary>
        /// Replace this on your children with the player collision check logic that suits your game
        /// </summary>
        protected virtual bool CollisionCheck(Collider2D other) //TODO: Collider3D Overload.
        {
            bool check = false;
            if (other.gameObject.tag == "Player")
            {
                check = true;
            }

            return check;
        }
    }
}
