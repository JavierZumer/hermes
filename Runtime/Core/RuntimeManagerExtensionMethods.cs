using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    public static class RuntimeManagerExtensionMethods
    {
        public static void AttachInstanceToKineticGameObject(this RuntimeManager runtimeManager, FMOD.Studio.EventInstance instance, Transform transform, VelocityVector3 velocityVector3)
        {
            /*AttachedInstance attachedInstance = Instance.attachedInstances.Find(x => x.instance.handle == instance.handle);
            if (attachedInstance == null)
            {
                attachedInstance = new AttachedInstance();
                Instance.attachedInstances.Add(attachedInstance);
            }

            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            attachedInstance.transform = transform;
            attachedInstance.instance = instance;*/
        }
    }
}
