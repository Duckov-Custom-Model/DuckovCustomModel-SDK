using System;
using System.Collections.Generic;
using System.Reflection;
using DuckovCustomModel.Core.MonoBehaviours.Animators;
using DuckovCustomModelTools.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using ChangeType = DuckovCustomModel.Core.MonoBehaviours.Animators.ModelParameterDriver.ChangeType;

namespace DuckovCustomModelTools.Components
{
    [CustomEditor(typeof(ModelParameterDriver))]
    public class ModelParameterDriverEditor : Editor
    {
        private string[] _parameterNames;
        private AnimatorControllerParameterType[] _parameterTypes;
        private int _selectedParam = -1;

        public void OnEnable()
        {
            UpdateParameters();
        }

        private void UpdateParameters()
        {
            var controller = GetCurrentController();
            if (controller == null) return;

            var names = new List<string>();
            var types = new List<AnimatorControllerParameterType>();
            foreach (var item in controller.parameters)
            {
                names.Add(item.name);
                types.Add(item.type);
            }

            _parameterNames = names.ToArray();
            _parameterTypes = types.ToArray();
        }

        private static AnimatorController GetCurrentController()
        {
            var toolType = Type.GetType("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs");
            var tool = EditorWindow.GetWindow(toolType);

            if (toolType == null) return null;

            var controllerProperty = toolType.GetProperty("animatorController",
                BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Instance);
            if (controllerProperty != null)
                return controllerProperty.GetValue(tool, null) as AnimatorController;

            Debug.LogError("Unable to find animator window.", tool);
            return null;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            if (_parameterNames == null)
                UpdateParameters();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("debugString"));

            var editable = new InspectorUtils.EditableArray
            {
                Array = serializedObject.FindProperty("parameters"),
                MaxElements = int.MaxValue,
                OnDrawElement = DrawParameter,
            };
            InspectorUtils.DrawEditableArray(this, editable, ref _selectedParam);

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(this);
        }

        private void DrawParameter(SerializedProperty parameters, int arrayIndex)
        {
            var param = parameters.GetArrayElementAtIndex(arrayIndex);
            var paramName = param.FindPropertyRelative("name");
            var source = param.FindPropertyRelative("source");
            var changeType = param.FindPropertyRelative("type");
            var value = param.FindPropertyRelative("value");
            var minValue = param.FindPropertyRelative("valueMin");
            var maxValue = param.FindPropertyRelative("valueMax");
            var chance = param.FindPropertyRelative("chance");

            EditorGUILayout.PropertyField(changeType);

            switch ((ChangeType)changeType.enumValueIndex)
            {
                case ChangeType.Set:
                {
                    DrawSet();
                    break;
                }
                case ChangeType.Add:
                {
                    DrawAdd();
                    break;
                }
                case ChangeType.Random:
                {
                    DrawRandom();
                    break;
                }
                case ChangeType.Copy:
                {
                    DrawCopy();
                    break;
                }
            }

            return;

            void DrawSet()
            {
                var destIndex = DrawParameterDropdown(paramName, "Destination");
                var valueType = destIndex >= 0 ? _parameterTypes[destIndex] : AnimatorControllerParameterType.Float;
                switch (valueType)
                {
                    case AnimatorControllerParameterType.Bool:
                    {
                        value.floatValue = EditorGUILayout.Toggle("Value", value.floatValue != 0f) ? 1f : 0f;
                        break;
                    }
                    case AnimatorControllerParameterType.Int:
                    {
                        value.floatValue = EditorGUILayout.IntField("Value", (int)value.floatValue);
                        break;
                    }
                    case AnimatorControllerParameterType.Float:
                    {
                        value.floatValue = EditorGUILayout.FloatField("Value", value.floatValue);
                        break;
                    }
                    case AnimatorControllerParameterType.Trigger:
                    {
                        break;
                    }
                    default:
                    {
                        EditorGUILayout.HelpBox(
                            $"{valueType} parameters don't support the {changeType.enumNames[changeType.enumValueIndex]} type",
                            MessageType.Warning);
                        break;
                    }
                }
            }

            void DrawAdd()
            {
                var destIndex = DrawParameterDropdown(paramName, "Destination");
                var valueType = destIndex >= 0 ? _parameterTypes[destIndex] : AnimatorControllerParameterType.Float;
                switch (valueType)
                {
                    case AnimatorControllerParameterType.Int:
                    {
                        value.floatValue = EditorGUILayout.IntField("Value", (int)value.floatValue);
                        break;
                    }
                    case AnimatorControllerParameterType.Float:
                    {
                        value.floatValue = EditorGUILayout.FloatField("Value", value.floatValue);
                        break;
                    }
                    default:
                    {
                        EditorGUILayout.HelpBox(
                            $"{valueType} parameters don't support the {changeType.enumNames[changeType.enumValueIndex]} type",
                            MessageType.Warning);
                        break;
                    }
                }
            }

            void DrawRandom()
            {
                var destIndex = DrawParameterDropdown(paramName, "Destination");
                var valueType = destIndex >= 0 ? _parameterTypes[destIndex] : AnimatorControllerParameterType.Float;
                switch (valueType)
                {
                    case AnimatorControllerParameterType.Bool:
                    case AnimatorControllerParameterType.Trigger:
                    {
                        EditorGUILayout.PropertyField(chance);
                        break;
                    }
                    case AnimatorControllerParameterType.Int:
                    {
                        minValue.floatValue = EditorGUILayout.IntField("Min Value", (int)minValue.floatValue);
                        maxValue.floatValue = EditorGUILayout.IntField("Max Value", (int)maxValue.floatValue);
                        break;
                    }
                    case AnimatorControllerParameterType.Float:
                    {
                        minValue.floatValue = EditorGUILayout.FloatField("Min Value", minValue.floatValue);
                        maxValue.floatValue = EditorGUILayout.FloatField("Max Value", maxValue.floatValue);
                        break;
                    }
                }
            }

            void DrawCopy()
            {
                var sourceIndex = DrawParameterDropdown(source, "Source");
                var sourceValueType =
                    sourceIndex >= 0 ? _parameterTypes[sourceIndex] : AnimatorControllerParameterType.Float;
                var destIndex = DrawParameterDropdown(paramName, "Destination");
                var destValueType = destIndex >= 0 ? _parameterTypes[destIndex] : AnimatorControllerParameterType.Float;
                switch (destValueType)
                {
                    case AnimatorControllerParameterType.Bool:
                    case AnimatorControllerParameterType.Int:
                    case AnimatorControllerParameterType.Float:
                    {
                        if (sourceIndex >= 0)
                        {
                            if (sourceValueType == AnimatorControllerParameterType.Trigger)
                                EditorGUILayout.HelpBox("Source parameter can't be the Trigger type",
                                    MessageType.Warning);
                            else if (sourceValueType != destValueType)
                                EditorGUILayout.HelpBox(
                                    $"Value will be converted from a {sourceValueType} to a {destValueType}.",
                                    MessageType.Info);
                        }

                        var convertRange = param.FindPropertyRelative("convertRange");
                        EditorGUILayout.PropertyField(convertRange);
                        if (convertRange.boolValue)
                        {
                            EditorGUI.indentLevel += 1;
                            DrawRange("Source", "sourceMin", "sourceMax");
                            DrawRange("Destination", "destMin", "destMax");
                            EditorGUI.indentLevel -= 1;

                            void DrawRange(string label, string min, string max)
                            {
                                var minVal = param.FindPropertyRelative(min);
                                var maxVal = param.FindPropertyRelative(max);

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PrefixLabel(label);
                                EditorGUILayout.LabelField("Min", GUILayout.Width(64));
                                minVal.floatValue = EditorGUILayout.FloatField(minVal.floatValue);
                                EditorGUILayout.LabelField("Max", GUILayout.Width(64));
                                maxVal.floatValue = EditorGUILayout.FloatField(maxVal.floatValue);
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                        break;
                    }
                    default:
                    {
                        EditorGUILayout.HelpBox(
                            $"{destValueType} parameters don't support the {changeType.enumNames[changeType.enumValueIndex]} type",
                            MessageType.Warning);
                        break;
                    }
                }
            }
        }

        private int DrawParameterDropdown(SerializedProperty property, string label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            var index = -1;
            if (_parameterNames != null)
            {
                EditorGUI.BeginChangeCheck();
                index = Array.IndexOf(_parameterNames, property.stringValue);
                index = EditorGUILayout.Popup(index, _parameterNames);
                if (EditorGUI.EndChangeCheck() && index >= 0)
                    property.stringValue = _parameterNames[index];
            }

            property.stringValue = EditorGUILayout.TextField(property.stringValue);
            EditorGUILayout.EndHorizontal();

            if (index < 0)
                EditorGUILayout.HelpBox(
                    $"Parameter '{property.stringValue}' not found. Make sure you defined in the Animator window's Parameters tab.",
                    MessageType.Warning);

            return index;
        }
    }
}