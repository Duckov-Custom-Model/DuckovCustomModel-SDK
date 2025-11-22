using System;
using System.Collections.Generic;
using System.Reflection;
using DuckovCustomModel.Core.MonoBehaviours.Animators;
using Newtonsoft.Json;
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
        private bool _parametersLoaded;
        private AnimatorControllerParameterType[] _parameterTypes;
        private int _selectedParam = -1;

        public void OnEnable()
        {
            UpdateParameters();
            _parametersLoaded = false;
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
            var driver = (ModelParameterDriver)target;

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            if (_parameterNames == null)
                UpdateParameters();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("debugString"));

            var parametersDataProp = serializedObject.FindProperty("parametersData");

            if (!_parametersLoaded)
            {
                if (!string.IsNullOrEmpty(parametersDataProp.stringValue))
                    try
                    {
                        driver.Parameters =
                            JsonConvert.DeserializeObject<ModelParameterDriver.Parameter[]>(parametersDataProp
                                .stringValue) ?? Array.Empty<ModelParameterDriver.Parameter>();
                    }
                    catch
                    {
                        driver.Parameters = Array.Empty<ModelParameterDriver.Parameter>();
                    }

                _parametersLoaded = true;
            }

            driver.Parameters ??= Array.Empty<ModelParameterDriver.Parameter>();

            var parametersList = new List<ModelParameterDriver.Parameter>(driver.Parameters);
            var changed = false;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(parametersList.Count >= int.MaxValue);
            if (GUILayout.Button("Add"))
            {
                parametersList.Add(new ModelParameterDriver.Parameter());
                changed = true;
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(_selectedParam < 0);
            if (GUILayout.Button("Up") && _selectedParam > 0)
            {
                (parametersList[_selectedParam], parametersList[_selectedParam - 1]) = (
                    parametersList[_selectedParam - 1], parametersList[_selectedParam]);
                _selectedParam--;
                changed = true;
            }

            if (GUILayout.Button("Down") && _selectedParam < parametersList.Count - 1)
            {
                (parametersList[_selectedParam], parametersList[_selectedParam + 1]) = (
                    parametersList[_selectedParam + 1], parametersList[_selectedParam]);
                _selectedParam++;
                changed = true;
            }

            if (GUILayout.Button("Delete"))
            {
                parametersList.RemoveAt(_selectedParam);
                _selectedParam = Mathf.Max(-1, _selectedParam - 1);
                changed = true;
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            for (var i = 0; i < parametersList.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                if (DrawParameter(parametersList[i], i))
                    changed = true;

                EditorGUILayout.EndVertical();

                var rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type != EventType.MouseDown || !rect.Contains(Event.current.mousePosition)) continue;
                _selectedParam = _selectedParam == i ? -1 : i;
                Repaint();
            }

            if (changed)
            {
                driver.Parameters = parametersList.ToArray();
                parametersDataProp.stringValue = JsonConvert.SerializeObject(driver.Parameters);
            }

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(target);
        }

        private bool DrawParameter(ModelParameterDriver.Parameter param, int arrayIndex)
        {
            var changed = false;

            EditorGUI.BeginChangeCheck();
            param.type = (ChangeType)EditorGUILayout.EnumPopup("Type", param.type);
            if (EditorGUI.EndChangeCheck()) changed = true;

            switch (param.type)
            {
                case ChangeType.Set:
                {
                    if (DrawSet(param)) changed = true;
                    break;
                }
                case ChangeType.Add:
                {
                    if (DrawAdd(param)) changed = true;
                    break;
                }
                case ChangeType.Random:
                {
                    if (DrawRandom(param)) changed = true;
                    break;
                }
                case ChangeType.Copy:
                {
                    if (DrawCopy(param)) changed = true;
                    break;
                }
            }

            return changed;
        }

        private bool DrawSet(ModelParameterDriver.Parameter param)
        {
            var changed = false;
            var destIndex = DrawParameterDropdown(ref param.name, "Destination");
            var valueType = destIndex >= 0 ? _parameterTypes[destIndex] : AnimatorControllerParameterType.Float;

            EditorGUI.BeginChangeCheck();
            switch (valueType)
            {
                case AnimatorControllerParameterType.Bool:
                {
                    param.value = EditorGUILayout.Toggle("Value", param.value != 0f) ? 1f : 0f;
                    break;
                }
                case AnimatorControllerParameterType.Int:
                {
                    param.value = EditorGUILayout.IntField("Value", (int)param.value);
                    break;
                }
                case AnimatorControllerParameterType.Float:
                {
                    param.value = EditorGUILayout.FloatField("Value", param.value);
                    break;
                }
                case AnimatorControllerParameterType.Trigger:
                {
                    break;
                }
                default:
                {
                    EditorGUILayout.HelpBox(
                        $"{valueType} parameters don't support the Set type",
                        MessageType.Warning);
                    break;
                }
            }

            if (EditorGUI.EndChangeCheck()) changed = true;
            return changed;
        }

        private bool DrawAdd(ModelParameterDriver.Parameter param)
        {
            var changed = false;
            var destIndex = DrawParameterDropdown(ref param.name, "Destination");
            var valueType = destIndex >= 0 ? _parameterTypes[destIndex] : AnimatorControllerParameterType.Float;

            EditorGUI.BeginChangeCheck();
            switch (valueType)
            {
                case AnimatorControllerParameterType.Int:
                {
                    param.value = EditorGUILayout.IntField("Value", (int)param.value);
                    break;
                }
                case AnimatorControllerParameterType.Float:
                {
                    param.value = EditorGUILayout.FloatField("Value", param.value);
                    break;
                }
                default:
                {
                    EditorGUILayout.HelpBox(
                        $"{valueType} parameters don't support the Add type",
                        MessageType.Warning);
                    break;
                }
            }

            if (EditorGUI.EndChangeCheck()) changed = true;
            return changed;
        }

        private bool DrawRandom(ModelParameterDriver.Parameter param)
        {
            var changed = false;
            var destIndex = DrawParameterDropdown(ref param.name, "Destination");
            var valueType = destIndex >= 0 ? _parameterTypes[destIndex] : AnimatorControllerParameterType.Float;

            EditorGUI.BeginChangeCheck();
            switch (valueType)
            {
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                {
                    param.chance = EditorGUILayout.Slider("Chance", param.chance, 0f, 1f);
                    break;
                }
                case AnimatorControllerParameterType.Int:
                {
                    param.valueMin = EditorGUILayout.IntField("Min Value", (int)param.valueMin);
                    param.valueMax = EditorGUILayout.IntField("Max Value", (int)param.valueMax);
                    break;
                }
                case AnimatorControllerParameterType.Float:
                {
                    param.valueMin = EditorGUILayout.FloatField("Min Value", param.valueMin);
                    param.valueMax = EditorGUILayout.FloatField("Max Value", param.valueMax);
                    break;
                }
            }

            if (EditorGUI.EndChangeCheck()) changed = true;
            return changed;
        }

        private bool DrawCopy(ModelParameterDriver.Parameter param)
        {
            var changed = false;
            var sourceIndex = DrawParameterDropdown(ref param.source, "Source");
            var sourceValueType =
                sourceIndex >= 0 ? _parameterTypes[sourceIndex] : AnimatorControllerParameterType.Float;
            var destIndex = DrawParameterDropdown(ref param.name, "Destination");
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

                    EditorGUI.BeginChangeCheck();
                    param.convertRange = EditorGUILayout.Toggle("Convert Range", param.convertRange);
                    if (EditorGUI.EndChangeCheck()) changed = true;

                    if (param.convertRange)
                    {
                        EditorGUI.indentLevel += 1;
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Source");
                        EditorGUILayout.LabelField("Min", GUILayout.Width(64));
                        param.sourceMin = EditorGUILayout.FloatField(param.sourceMin);
                        EditorGUILayout.LabelField("Max", GUILayout.Width(64));
                        param.sourceMax = EditorGUILayout.FloatField(param.sourceMax);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Destination");
                        EditorGUILayout.LabelField("Min", GUILayout.Width(64));
                        param.destMin = EditorGUILayout.FloatField(param.destMin);
                        EditorGUILayout.LabelField("Max", GUILayout.Width(64));
                        param.destMax = EditorGUILayout.FloatField(param.destMax);
                        EditorGUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck()) changed = true;
                        EditorGUI.indentLevel -= 1;
                    }

                    break;
                }
                default:
                {
                    EditorGUILayout.HelpBox(
                        $"{destValueType} parameters don't support the Copy type",
                        MessageType.Warning);
                    break;
                }
            }

            return changed;
        }

        private int DrawParameterDropdown(ref string value, string label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            var index = -1;
            if (_parameterNames != null)
            {
                EditorGUI.BeginChangeCheck();
                index = Array.IndexOf(_parameterNames, value);
                index = EditorGUILayout.Popup(index, _parameterNames);
                if (EditorGUI.EndChangeCheck() && index >= 0)
                    value = _parameterNames[index];
            }

            value = EditorGUILayout.TextField(value);
            EditorGUILayout.EndHorizontal();

            if (index < 0 && !string.IsNullOrEmpty(value))
                EditorGUILayout.HelpBox(
                    $"Parameter '{value}' not found. Make sure you defined in the Animator window's Parameters tab.",
                    MessageType.Warning);

            return index;
        }
    }
}