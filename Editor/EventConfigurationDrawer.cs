using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Hermes;
using System;
using FMODUnity;
using FMOD.Studio;
using FMOD;

[CustomPropertyDrawer(typeof(EventConfiguration))]
public class EventConfigurationDrawer : PropertyDrawer
{
    private SerializedProperty m_eventReference;
    private string m_eventPath;
    private SerializedProperty m_highlightSnaphsot;
    private SerializedProperty m_preloadSampleData;
    private SerializedProperty m_numberOfVoices;
    private SerializedProperty m_eventInitializationMode;
    private SerializedProperty m_eventReleaseMode;
    private SerializedProperty m_instanceShareMode;
    private SerializedProperty m_steady;
    private SerializedProperty m_allowFadeOutWhenStopping;
    private SerializedProperty m_stealingMode;

    private bool m_otherOptions;
    private bool m_referenceFieldExpanded;
    private bool m_snapshotFieldExpanded;
    private bool m_polyphonic;

    EventConfiguration m_eventConfiguration;

    private EventInstance m_editorInstance;

    private readonly float lineHeight = EditorGUIUtility.singleLineHeight;

    //Draw on Inspector Window
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Ref to parent script
        m_eventConfiguration = (EventConfiguration)fieldInfo.GetValue(property.serializedObject.targetObject);

        //Start the property
        EditorGUI.BeginProperty(position, label, property);

        m_eventReference = property.FindPropertyRelative("EventRef");
        m_eventPath = m_eventConfiguration.EventPath;
        m_highlightSnaphsot = property.FindPropertyRelative("HighlightSnapshot");
        m_eventInitializationMode = property.FindPropertyRelative("EventInitializationMode");
        m_numberOfVoices = property.FindPropertyRelative("PolyphonyVoices");
        m_stealingMode = property.FindPropertyRelative("EmitterVoiceStealing");
        m_instanceShareMode = property.FindPropertyRelative("InstanceShareMode");
        m_preloadSampleData = property.FindPropertyRelative("PreloadSampleData");
        m_eventReleaseMode = property.FindPropertyRelative("EventReleaseMode");
        m_steady = property.FindPropertyRelative("Steady");
        m_allowFadeOutWhenStopping = property.FindPropertyRelative("AllowFadeOutWhenStopping");

        Rect drawMainLabel = new Rect(position.min.x, position.min.y, position.size.x, lineHeight);
        EditorGUI.LabelField(drawMainLabel, new GUIContent("Event Configuration"),EditorStyles.boldLabel);

        //Draw the first property.
        DrawEventReference(position,1);

        //After drawing the last property, end the Property wrapper.
        EditorGUI.EndProperty();
    }

    private void DrawEventReference(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_eventReference, new GUIContent("Event Reference"), false);
        m_referenceFieldExpanded = m_eventReference.isExpanded;

        int refheight = 2;

        if (m_referenceFieldExpanded)
        {
            refheight = +6;
        }

        if (!String.IsNullOrEmpty(m_eventPath))
        {
            DrawPlayAndStopButtons(drawArea, refheight);
        }
        else
        {
            DrawHighlightSnapshot(drawArea, 2);
        }
    }

    //Add Play and Stop Buttons here?
    private void DrawPlayAndStopButtons(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height), position.size.x, lineHeight);
        Rect drawPlay = new Rect(position.min.x, position.min.y + (lineHeight * height), position.size.x*0.47f, lineHeight);
        if (GUI.Button(drawPlay, new GUIContent("Play Event")))
        {
            m_eventConfiguration.PlayEventInEditor();
        }

        Rect drawStop = new Rect((position.min.x+position.size.x/2), position.min.y + (lineHeight * height), position.size.x / 2, lineHeight);
        if (GUI.Button(drawStop, new GUIContent("Stop Event")))
        {
            m_eventConfiguration.StopEventInEditor();
        }

        DrawHighlightSnapshot(drawArea, 1);
    }

    private void DrawHighlightSnapshot(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_highlightSnaphsot, new GUIContent("Highlight Snapshot"));
        m_snapshotFieldExpanded = m_highlightSnaphsot.isExpanded;

        int snapheight = 2;

        if (m_snapshotFieldExpanded)
        {
            snapheight += 5;
        }

        if (m_eventConfiguration != null && !m_eventConfiguration.HighlightSnapshot.IsNull)
        {
            RuntimeManager.GetEventDescription(m_eventConfiguration.HighlightSnapshot).isSnapshot(out bool isSnapshot);
            if (!isSnapshot)
            {
                DrawSnapshotHelpBox(drawArea, 1);
            }
            else
            {
                DrawInstancesLabel(drawArea, snapheight);
            }
        }
        else
        {
            DrawInstancesLabel(drawArea, snapheight);
        }
    }

    private void DrawSnapshotHelpBox(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.HelpBox(drawArea, "You need to select a snapshot here!", MessageType.Error);
        DrawInstancesLabel(drawArea, 1);
    }

    private void DrawInstancesLabel(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.LabelField(drawArea, new GUIContent("-- Instance Management --"), EditorStyles.boldLabel);
        DrawInstanceInitialization(drawArea, 1);
    }

    private void DrawInstanceInitialization(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_eventInitializationMode, new GUIContent("Instance Initialization"));
        DrawInstanceShareMode(drawArea, 1);
    }

    private void DrawInstanceShareMode(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_instanceShareMode, new GUIContent("Instance share mode"));

        if (m_instanceShareMode.enumValueIndex == (int)InstanceShareMode.LocalPolyphonic ||
            m_instanceShareMode.enumValueIndex == (int)InstanceShareMode.GlobalPolyphonic)
        {
            m_polyphonic = true;
            DrawNumberOfVoices(drawArea, 1);
            DrawVoiceStealing(drawArea, 3);
        }
        else
        {
            m_polyphonic = false;
            DrawInstanceRelease(drawArea, 1);
        }
    }

    private void DrawNumberOfVoices(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, EditorGUIUtility.singleLineHeight);
        m_numberOfVoices.intValue = EditorGUI.IntSlider(drawArea, new GUIContent("Number Of Voices"), m_numberOfVoices.intValue, 2, 30);
    }

    private void DrawVoiceStealing(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(drawArea, m_stealingMode, new GUIContent("Emitter Voice Stealing"));
        DrawInstanceRelease(drawArea, 1);
    }

    private void DrawInstanceRelease(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_eventReleaseMode, new GUIContent("Instance Release"));
        DrawOtherOptionsLabel(drawArea, 1);
    }

    private void DrawOtherOptionsLabel(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        m_otherOptions = EditorGUI.Foldout(drawArea, m_otherOptions, new GUIContent("-- Other Options --"));

        if (m_otherOptions)
        {
            DrawPreloadSampleData(drawArea, 1);
        }
    }

    private void DrawPreloadSampleData(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_preloadSampleData, new GUIContent("Preload Sample Data"));
        DrawSteady(drawArea, 1);
    }

    private void DrawSteady(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_steady, new GUIContent("Steady"));
        DrawFadeOut(drawArea, 1);
    }

    private void DrawFadeOut(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_allowFadeOutWhenStopping, new GUIContent("Fadeout When Stopping"));
    }

    //Set property height
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //Instead of doing this, as for each property height and return that?

        int numberOfLines = 16;

        if (m_referenceFieldExpanded)
        {
            numberOfLines += 4;
        }

        if (m_snapshotFieldExpanded)
        {
            numberOfLines += 4;
        }

        if (m_polyphonic)
        {
            numberOfLines += 3;
        }

        if (m_otherOptions)
        {
            numberOfLines += 4;
        }

        return EditorGUIUtility.singleLineHeight * numberOfLines;
    }
}
