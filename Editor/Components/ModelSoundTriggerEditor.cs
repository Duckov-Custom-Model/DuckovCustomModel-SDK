using DuckovCustomModel.Core.MonoBehaviours.Animators;
using UnityEditor;
using UnityEngine;

namespace DuckovCustomModelTools.Components
{
    [CustomEditor(typeof(ModelSoundTrigger))]
    public class ModelSoundTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            var soundTagsProp = serializedObject.FindProperty("soundTags");
            EditorGUILayout.PropertyField(soundTagsProp, new GUIContent("Sound Tags"), true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("playOrder"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eventName"));

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Sound Tags: Specify one or more sound tags to play. Multiple tags can be configured.\n" +
                "Play Order: Random - randomly selects from tags; Sequential - plays tags in order.\n" +
                "\nPlay Mode:\n" +
                "• Normal: Normal playback, allows multiple sounds to play simultaneously.\n" +
                "• StopPrevious: Stops previously playing sounds with the same event name before playing new one.\n" +
                "• SkipIfPlaying: Skips playback if a sound with the same event name is already playing.\n" +
                "• UseTempObject: Creates a separate temporary object at the current position to play the sound. " +
                "This prevents the sound from stopping when the character dies, but has lower performance.\n" +
                "\nEvent Name: Optional name for sound playback management. If empty, a default name will be generated.",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(target);
        }
    }
}