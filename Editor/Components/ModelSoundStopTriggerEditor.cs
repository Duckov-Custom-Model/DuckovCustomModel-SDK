using DuckovCustomModel.Core.MonoBehaviours.Animators;
using UnityEditor;
using UnityEngine;

namespace DuckovCustomModelTools.Components
{
    [CustomEditor(typeof(ModelSoundStopTrigger))]
    public class ModelSoundStopTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            var stopAllSoundsProp = serializedObject.FindProperty("stopAllSounds");
            EditorGUILayout.PropertyField(stopAllSoundsProp);

            var stopAllSounds = stopAllSoundsProp.boolValue;

            // 只有当 stopAllSounds 为 false 时才显示其他选项
            if (!stopAllSounds)
            {
                EditorGUI.indentLevel++;

                var useBuiltInEventNameProp = serializedObject.FindProperty("useBuiltInEventName");
                EditorGUILayout.PropertyField(useBuiltInEventNameProp);

                var useBuiltInEventName = useBuiltInEventNameProp.boolValue;

                var eventNameProp = serializedObject.FindProperty("eventName");

                // 根据 useBuiltInEventName 显示不同的标签
                var eventNameLabel = useBuiltInEventName
                    ? new GUIContent("Event Name",
                        "Built-in event name (e.g., 'idle'). This field is required when useBuiltInEventName is enabled.")
                    : new GUIContent("Event Name",
                        "Optional name for sound playback management. If empty, a default name will be used (same as ModelSoundTrigger).");

                EditorGUILayout.PropertyField(eventNameProp, eventNameLabel);

                // 显示警告：如果 useBuiltInEventName 为 true 但 eventName 为空
                if (useBuiltInEventName && string.IsNullOrWhiteSpace(eventNameProp.stringValue))
                    EditorGUILayout.HelpBox(
                        "WARNING: useBuiltInEventName is enabled but eventName is empty. " +
                        "Please specify a built-in event name (e.g., 'idle').",
                        MessageType.Warning);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("stopOnEnter"));

            EditorGUILayout.Space();

            // 帮助信息
            var helpMessage = stopAllSounds
                ? "Stop All Sounds: When enabled, all currently playing sounds will be stopped.\n" +
                  "Stop On Enter: Controls when to trigger the stop action (on state enter or exit)."
                : "Stop All Sounds: When disabled, only the sound specified by eventName will be stopped.\n" +
                  "\nUse Built-In Event Name: " +
                  "• If enabled: Use built-in event names directly (e.g., 'idle') without 'CustomModelSoundTrigger:' prefix.\n" +
                  "  WARNING: Only use this for built-in event names like 'idle'. For custom triggers, leave this disabled.\n" +
                  "• If disabled: Use custom trigger event names (same format as ModelSoundTrigger).\n" +
                  "\nEvent Name: " +
                  "• For custom triggers: Optional name for sound playback management. If empty, a default name will be used.\n" +
                  "• For built-in events: Must specify a built-in event name (e.g., 'idle').\n" +
                  "\nStop On Enter: Controls when to trigger the stop action (on state enter or exit).";

            EditorGUILayout.HelpBox(helpMessage, MessageType.Info);

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(target);
        }
    }
}