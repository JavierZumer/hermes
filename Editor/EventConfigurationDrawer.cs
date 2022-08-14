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
    private SerializedProperty m_emitterVoiceStealing;

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
        m_reUseEventConfiguration = property.FindPropertyRelative("ReuseEventConfiguration");
        m_emitterVoiceStealing = property.FindPropertyRelative("EmitterVoiceStealing");

        Rect drawArea = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);

        EditorGUI.LabelField(drawArea, new GUIContent("Event Configuration"),EditorStyles.boldLabel);

        //Draw the first property.
        DrawEventReference(position,1);

        //After drawing the last property, end the Property wrapper.
        EditorGUI.EndProperty();
    }

    private void DrawEventReference(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, EditorGUI.GetPropertyHeight(m_eventReference));
        EditorGUI.PropertyField(drawArea, m_eventReference,new GUIContent("Event Reference"));
        DrawHighlightSnapshot(drawArea,2);
    }

    private void DrawHighlightSnapshot(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(drawArea, m_highlightSnaphsot, new GUIContent("Highlight Snapshot"));
        DrawPreloadSampleData(drawArea,2);
    }

    private void DrawPreloadSampleData(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(drawArea, m_preloadSampleData, new GUIContent("Preload Sample Data"));
        DrawPolyphony(drawArea,1);
    }

    private void DrawPolyphony(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(drawArea, m_polyphony, new GUIContent("Polyphony"));

        if (m_polyphony.enumValueIndex == (int)Polyphony.Polyphonic)
        {
            DrawNumberOfVoices(drawArea, 1);
            DrawVoiceStealing(drawArea, 3);
        }
        else
        {
            DrawReuseVoices(drawArea, 1);
        }
    }

    private void DrawNumberOfVoices(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, EditorGUIUtility.singleLineHeight);
        m_numberOfVoices.intValue = EditorGUI.IntSlider(drawArea, new GUIContent("Number Of Voices"), m_numberOfVoices.intValue, 1, 30);
    }

    private void DrawVoiceStealing(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(drawArea, m_emitterVoiceStealing, new GUIContent("Emitter Voice Stealing"));
        DrawReuseVoices(drawArea, 1);
    }

    private void DrawReuseVoices(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(drawArea, m_reUseEventConfiguration, new GUIContent("Reuse voices"));
    }


    //Set property height
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int numberOfLines = 12;

        if (m_polyphony != null && m_polyphony.enumValueIndex == (int)Polyphony.Polyphonic)
        {
            numberOfLines += 2;
        }

        return EditorGUIUtility.singleLineHeight * numberOfLines;
    }
}
