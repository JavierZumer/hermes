using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Hermes;

[CustomEditor(typeof(AudioEventStream))]
public class AudioEventStreamEditor : Editor
{

    AudioEventStream eventStream;

    public SerializedProperty
        audioAction,
        eventReference;

    private void OnEnable()
    {
        //eventStream = (AudioEventStream)target;

        //Setup SerializedProperties
        audioAction = serializedObject.FindProperty("Action");
        eventReference = serializedObject.FindProperty("EventReference");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        //DrawDefaultInspector();
    }
}
