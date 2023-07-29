using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    [AddComponentMenu("Hermes/FMOD Studio Listener")]
    public class HermesFmodListener : MonoBehaviour
    {
        //TODO: Es posible prescindir de esto y de alguna manera hacer funcionar la kinematic velocity en el listener oficial de FMOD?
        //Si HAY que usar esta clase por narices, podria ser hija del listener normal? O usar extension methods?
        //Si no, se pueden quitar los avisos sobre usar
        //un listener pero sin alterar el codigo del plugin de fmod?

        [SerializeField]
        private GameObject attenuationObject;

        private Rigidbody rigidBody;
        private Rigidbody2D rigidBody2D;

        private static List<HermesFmodListener> listeners = new List<HermesFmodListener>();
        private VelocityVector3 m_kinematicVelocity = null;
        private Vector3 m_positionLastFrame;

        public static int ListenerCount
        {
            get
            {
                return listeners.Count;
            }
        }

        public int ListenerNumber
        {
            get
            {
                return listeners.IndexOf(this);
            }
        }

        public static float DistanceToNearestListener(Vector3 position)
        {
            float result = float.MaxValue;
            for (int i = 0; i < listeners.Count; i++)
            {
                result = Mathf.Min(result, Vector3.Distance(position, listeners[i].transform.position));
            }
            return result;
        }

        private static void AddListener(HermesFmodListener listener)
        {
            // Is the listener already in the list?
            if (listeners.Contains(listener))
            {
                Debug.LogWarning(string.Format(("[FMOD] Listener has already been added at index {0}."), listener.ListenerNumber));
                return;
            }

            // If already at the max numListeners
            if (listeners.Count >= FMOD.CONSTANTS.MAX_LISTENERS)
            {
                Debug.LogWarning(string.Format(("[FMOD] Max number of listeners reached : {0}."), FMOD.CONSTANTS.MAX_LISTENERS));
            }

            listeners.Add(listener);
            RuntimeManager.StudioSystem.setNumListeners(Mathf.Clamp(listeners.Count, 1, FMOD.CONSTANTS.MAX_LISTENERS));
        }

        private static void RemoveListener(HermesFmodListener listener)
        {
            listeners.Remove(listener);
            RuntimeManager.StudioSystem.setNumListeners(Mathf.Clamp(listeners.Count, 1, FMOD.CONSTANTS.MAX_LISTENERS));
        }

        private void OnEnable()
        {
            RuntimeUtils.EnforceLibraryOrder();

            rigidBody = gameObject.GetComponent<Rigidbody>();
            rigidBody2D = gameObject.GetComponent<Rigidbody2D>();

            if (!rigidBody && !rigidBody2D)
            {
                m_kinematicVelocity = new VelocityVector3();
            }

            AddListener(this);
        }

        private void OnDisable()
        {
            RemoveListener(this);
        }

        private void Update()
        {
            if (m_kinematicVelocity != null)
            {
                UpdateKinematicVelocity();
            }

            if (ListenerNumber >= 0 && ListenerNumber < FMOD.CONSTANTS.MAX_LISTENERS)
            {
                SetListenerLocation();
            }
        }

        private void SetListenerLocation()
        {

            if (rigidBody)
            {
                RuntimeManager.SetListenerLocation(ListenerNumber, gameObject, rigidBody, attenuationObject);
            }
            else if (rigidBody2D)
            {
                RuntimeManager.SetListenerLocation(ListenerNumber, gameObject, rigidBody2D, attenuationObject);
            }
            else
            {
                SetKinematicListenerLocation(ListenerNumber, gameObject, m_kinematicVelocity, attenuationObject);
            }
        }

        public void SetKinematicListenerLocation(int listenerIndex, GameObject gameObject, VelocityVector3 kinematicVelocity, GameObject attenuationObject = null)
        {
            if (attenuationObject)
            {
                RuntimeManager.StudioSystem.setListenerAttributes(listenerIndex, ToKinematic3DAttributes(gameObject.transform, kinematicVelocity), RuntimeUtils.ToFMODVector(attenuationObject.transform.position));
            }
            else
            {
                RuntimeManager.StudioSystem.setListenerAttributes(listenerIndex, ToKinematic3DAttributes(gameObject.transform, kinematicVelocity));
            }
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
}

