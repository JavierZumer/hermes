using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Hermes;
using System;
using FMODUnity;
using FMOD.Studio;
using FMOD;
using Debug = UnityEngine.Debug;

[CustomPropertyDrawer(typeof(EventConfiguration))]
public class EventConfigurationDrawer : PropertyDrawer
{
    private SerializedProperty m_eventReference;
    private SerializedProperty m_highlightSnaphsot;
    private SerializedProperty m_preloadSampleData;
    private SerializedProperty m_shareInstances;
    private SerializedProperty m_numberOfVoices;
    private SerializedProperty m_eventInitializationMode;
    private SerializedProperty m_eventReleaseMode;
    private SerializedProperty m_polyphonyModes;
    private SerializedProperty m_steady;
    private SerializedProperty m_allowFadeOutWhenStopping;
    private SerializedProperty m_stealingMode;
    private SerializedProperty m_stopMaxDistance;
    private SerializedProperty m_calculateKinematicVelocity;

    //Optional fields bool
    private bool m_showTransportButtons;
    private bool m_otherOptions;
    private bool m_shareInstancesWarning;
    private bool m_referenceFieldExpanded;
    private bool m_snapshotFieldExpanded;
    private bool m_polyphonic;

    EventConfiguration m_eventConfiguration;

    private readonly float lineHeight = EditorGUIUtility.singleLineHeight;

    //We keep an static reference to global events and how many emitters are using them.
    private static Dictionary<string, int> m_globalEvents = new Dictionary<string, int>();

    //Draw on Inspector Window
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Ref to parent script
        m_eventConfiguration = (EventConfiguration)fieldInfo.GetValue(property.serializedObject.targetObject);

        //Start the property
        EditorGUI.BeginProperty(position, label, property);

        m_eventReference = property.FindPropertyRelative("EventRef");
        m_highlightSnaphsot = property.FindPropertyRelative("HighlightSnapshot");
        m_eventInitializationMode = property.FindPropertyRelative("EventInitializationMode");
        m_numberOfVoices = property.FindPropertyRelative("PolyphonyVoices");
        m_stealingMode = property.FindPropertyRelative("EmitterVoiceStealing");
        m_polyphonyModes = property.FindPropertyRelative("PolyphonyModes");
        m_preloadSampleData = property.FindPropertyRelative("PreloadSampleData");
        m_eventReleaseMode = property.FindPropertyRelative("EventReleaseMode");
        m_steady = property.FindPropertyRelative("Steady");
        m_allowFadeOutWhenStopping = property.FindPropertyRelative("AllowFadeOutWhenStopping");
        m_shareInstances = property.FindPropertyRelative("ShareEventInstances");
        m_stopMaxDistance = property.FindPropertyRelative("StopEventsAtMaxDistance");
        m_calculateKinematicVelocity = property.FindPropertyRelative("CalculateKinematicVelocity");

        Rect drawMainLabel = new Rect(position.min.x, position.min.y, position.size.x, lineHeight);
        EditorGUI.LabelField(drawMainLabel, new GUIContent("Event Configuration"),EditorStyles.boldLabel);

        //Draw the first property.
        DrawEventReference(position,1);

        /* //Check if we need to update our global events dictionary
        if (m_eventConfiguration.LastEventPath != m_eventConfiguration.EventPath || m_shareInstances.boolValue != m_eventConfiguration.LastShareInstances) 
        {
            m_eventConfiguration.LastEventPath = m_eventConfiguration.EventPath;
            m_eventConfiguration.LastShareInstances = m_shareInstances.boolValue;
            m_globalWarning = UpdateGlobalEvents();
        }*/
        
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

        if (!m_eventConfiguration.EventRef.Guid.IsNull)
        {
            m_showTransportButtons = true;
            DrawPlayAndStopButtons(drawArea, refheight);
        }
        else
        {
            m_showTransportButtons = false;
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
            snapheight += 4;
        }

        if (m_eventConfiguration !=  null && m_eventConfiguration.HighlightSnapshot.Path.StartsWith("event:/"))
        {
            //Do things.
            DrawSnapshotHelpBox(drawArea, 1);
        }
        else
        {
            DrawInstancesLabel(drawArea, snapheight);
        }
    }

    private void DrawSnapshotHelpBox(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.HelpBox(drawArea, "You need to select a snapshot here, not an event!", MessageType.Error);
        DrawInstancesLabel(drawArea, 1);
    }

    private void DrawInstancesLabel(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.LabelField(drawArea, new GUIContent("-- Instance Management --"), EditorStyles.boldLabel);
        DrawInstanceSharing(drawArea, 1);
    }

    private void DrawInstanceSharing(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_shareInstances, new GUIContent("Share instances"));

        if (m_shareInstances.boolValue)
        {
            m_shareInstancesWarning = true;
            DrawGlobalWarning(position, 2);
        }
        else
        {
            m_shareInstancesWarning = false;
            DrawInstanceInitialization(drawArea, 1);
        }
    }

    private void DrawGlobalWarning(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 20, position.size.x, lineHeight * 4);
        EditorGUI.HelpBox(drawArea, "Event instances will be shared by all emitters that are using this EventReference. " +
            "Make sure you give all emitters the same settings. The first one to be initialized will dictate the shared behaviour.", MessageType.Info);

        m_shareInstancesWarning = true;
        DrawInstanceInitialization(drawArea, 4);
    }

    private void DrawInstanceInitialization(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_eventInitializationMode, new GUIContent("Instance Initialization"));
        DrawPolyphonyMode(drawArea, 1);
    }

    //TODO: Unused for now, try to find a way to indicate to user global instance re-use in a more clear way.
    //At least, we could show the user all the emitters using a particular event but that seems hard to do in editor (would be simpler at runtime).
    private bool UpdateGlobalEvents()
    {
        if (m_shareInstances.boolValue)
        {
            if (!m_globalEvents.ContainsKey(m_eventConfiguration.EventPath))
            {
                m_globalEvents.Add(m_eventConfiguration.EventPath, 1); //Add entry for the first time
                return false;
                //TODO: Get a reference to the parent emitter so we can show it later?
            }
            else
            {
                //This event is global AND another emitter was already using the same path.
                m_globalEvents[m_eventConfiguration.EventPath]++; //Bump number of emitters using.
                Debug.LogError($"We bumped {m_eventConfiguration.EventPath} to {m_globalEvents[m_eventConfiguration.EventPath]}.");
                return true;
            }
        }
        else
        {
            //Not global anymore
            if (m_globalEvents.TryGetValue(m_eventConfiguration.EventPath, out int n))
            {
                if (n <= 1)
                {
                    m_globalEvents.Remove(m_eventConfiguration.EventPath); //This is the last emitter using this path, so remove.
                    return false;
                }
                else
                {
                    //Not the last emitter so just substract one from index.
                    m_globalEvents[m_eventConfiguration.EventPath]--;
                    Debug.LogError($"We reduced {m_eventConfiguration.EventPath} to {m_globalEvents[m_eventConfiguration.EventPath]}.");
                    return false;
                }
            }
            else
            {
                //We didn't find the path on the dictionary so nothing to do.
                return false;
            }
        }
    }

    private void DrawPolyphonyMode(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_polyphonyModes, new GUIContent("Instances Mode"));

        if (m_polyphonyModes.enumValueIndex == (int)PolyphonyMode.Polyphonic)
        {
            m_polyphonic = true;
            DrawNumberOfVoices(drawArea, 1);
            DrawVoiceStealing(drawArea, 3);
        }
        else
        {
            //Is monophonic
            m_polyphonic = false;
            DrawInstanceRelease(drawArea, 1);
        }
    }

    private void DrawNumberOfVoices(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, EditorGUIUtility.singleLineHeight);
        m_numberOfVoices.intValue = EditorGUI.IntSlider(drawArea, new GUIContent("Number Of Instances"), m_numberOfVoices.intValue, 2, 30);
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
        DrawStopAtMaxDistance(drawArea, 1);
    }

    private void DrawStopAtMaxDistance(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_stopMaxDistance, new GUIContent("Stop Events At Max distance"));
        DrawKinematicVelocity(drawArea, 1);
    }

    private void DrawKinematicVelocity(Rect position, int height)
    {
        Rect drawArea = new Rect(position.min.x, position.min.y + (lineHeight * height) + 10, position.size.x, lineHeight);
        EditorGUI.PropertyField(drawArea, m_calculateKinematicVelocity, new GUIContent("Calculate Kinematic Velocity"));
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

        if(m_showTransportButtons)
        {
            numberOfLines += 1;
        }

        if (m_snapshotFieldExpanded)
        {
            numberOfLines += 4;
        }

        if (m_shareInstancesWarning)
        {
            numberOfLines += 4;
        }

        if (m_polyphonic)
        {
            numberOfLines += 4;
        }

        if (m_otherOptions)
        {
            numberOfLines += 7;
        }

        return EditorGUIUtility.singleLineHeight * numberOfLines;
    }
}
