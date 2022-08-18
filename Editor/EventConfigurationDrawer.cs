using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Hermes;
using System;

[CustomPropertyDrawer(typeof(EventConfiguration))]
public class EventConfigurationDrawer : PropertyDrawer
{
    private SerializedProperty m_eventReference;
    private SerializedProperty m_highlightSnaphsot;
    private SerializedProperty m_preloadSampleData;
    private SerializedProperty m_polyphony;
    private SerializedProperty m_numberOfVoices;
    private SerializedProperty m_reUseEventConfiguration;
    private SerializedProperty m_eventInitializationMode;
    private SerializedProperty m_eventReleaseMode;
    private SerializedProperty m_emitterVoiceStealing;
    private SerializedProperty m_steady;
    private SerializedProperty m_allowFadeOutWhenStopping;

    private bool m_otherOptions; 

    private readonly float lineHeight = EditorGUIUtility.singleLineHeight;

    //Draw on Inspector Window
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Calculate space needed to draw stuff.
        EditorGUI.BeginProperty(position, label, property);

        m_eventReference = property.FindPropertyRelative("EventReference");
        m_highlightSnaphsot = property.FindPropertyRelative("HighlightSnapshot");
        m_preloadSampleData = property.FindPropertyRelative("PreloadSampleData");
        m_polyphony = property.FindPropertyRelative("Polyphony");
        m_numberOfVoices = property.FindPropertyRelative("NumberOfVoices");
        m_reUseEventConfiguration = property.FindPropertyRelative("ReuseInstances");
        m_eventInitializationMode = property.FindPropertyRelative("EventInitializationMode");
        m_eventReleaseMode = property.FindPropertyRelative("EventReleaseMode");
        m_emitterVoiceStealing = property.FindPropertyRelative("EmitterVoiceStealing");
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
        EditorGUI.PropertyField(drawArea, m_eventReference,new GUIContent("Event Reference"));
        DrawHighlightSnapshot(drawArea,2);
    }

    private void DrawHighlightSnapshot(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_highlightSnaphsot, new GUIContent("Highlight Snapshot"));
        DrawVoiceLabel(drawArea, 2);
    }

    private void DrawVoiceLabel(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.LabelField(drawArea, new GUIContent("--Voice Management--"),EditorStyles.boldLabel);
        DrawPolyphony(drawArea, 1);
    }

    private void DrawPolyphony(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_polyphony, new GUIContent("Polyphony Mode"));

        if (m_polyphony.enumValueIndex == (int)Polyphony.Polyphonic)
        {
            DrawNumberOfVoices(drawArea, 1);
            DrawVoiceStealing(drawArea, 3);
        }
        else
        {
            DrawInstancesLabel(drawArea, 1);
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
        EditorGUI.PropertyField(drawArea, m_emitterVoiceStealing, new GUIContent("Emitter Voice Stealing"));
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
        DrawInstanceRelease(drawArea,1);
    }

    private void DrawInstanceRelease(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_eventReleaseMode, new GUIContent("Instance Release"));
        DrawReuseVoices(drawArea, 1);
    }

    private void DrawReuseVoices(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_reUseEventConfiguration, new GUIContent("Reuse emitter instances"));
        DrawOtherOptionsLabel(drawArea, 1);
    }

    private void DrawOtherOptionsLabel(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        //EditorGUI.LabelField(drawArea, new GUIContent("-- Other Options --"), EditorStyles.boldLabel);
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
        int numberOfLines = 18;

        if (m_polyphony != null && m_polyphony.enumValueIndex == (int)Polyphony.Polyphonic)
        {
            numberOfLines += 2;
        }

        if (m_otherOptions)
        {
            numberOfLines += 4;
        }

        return EditorGUIUtility.singleLineHeight * numberOfLines;
    }
}
