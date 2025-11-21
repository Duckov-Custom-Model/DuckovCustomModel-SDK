using DuckovCustomModel.Core.MonoBehaviours.Animators;
using UnityEditor;

namespace DuckovCustomModelTools.Components
{
    [CustomEditor(typeof(ModelDialogueTrigger))]
    public class ModelDialogueTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("fileName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueId"));

            var defaultLanguageProp = serializedObject.FindProperty("defaultLanguage");
            EditorGUILayout.PropertyField(defaultLanguageProp);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "The dialogue definition file should be placed in the model directory with the format: {fileName}_{language}.json\n" +
                "For example: dialogues_English.json, dialogues_Chinese.json",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(target);
        }
    }
}