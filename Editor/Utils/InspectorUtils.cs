using System;
using UnityEditor;
using UnityEngine;

namespace DuckovCustomModelTools.Utils
{
    public static class InspectorUtils
    {
        private static GUIStyle _styleBoxNormal;
        private static GUIStyle _styleBoxSelected;

        public static void QuaternionAsEulerField(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();
            var euler = EditorGUILayout.Vector3Field("Rotation", property.quaternionValue.eulerAngles);
            if (!EditorGUI.EndChangeCheck())
                return;
            property.quaternionValue = Quaternion.Euler(euler);
        }

        private static void InitStyles()
        {
            _styleBoxNormal ??= new GUIStyle(GUI.skin.box);
            if (_styleBoxSelected != null)
                return;
            _styleBoxSelected = new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = MakeStyleBackground(new Color(0.0f, 0.5f, 1f, 0.5f)),
                },
            };
        }

        private static Texture2D MakeStyleBackground(Color color)
        {
            var texture2D = new Texture2D(1, 1);
            texture2D.SetPixel(0, 0, color);
            texture2D.Apply();
            return texture2D;
        }

        public static void DrawEditableArray(
            Editor editor,
            EditableArray data,
            ref int selected)
        {
            InitStyles();
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(data.Array.arraySize >= data.MaxElements);
            if (GUILayout.Button("Add"))
            {
                data.Array.InsertArrayElementAtIndex(data.Array.arraySize);
                data.OnNewElement?.Invoke(data.Array, data.Array.arraySize - 1);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(selected < 0);
            if (GUILayout.Button("Up") && selected > 0)
            {
                data.Array.MoveArrayElement(selected, selected - 1);
                --selected;
                editor.Repaint();
            }

            if (GUILayout.Button("Down") && selected < data.Array.arraySize - 1)
            {
                data.Array.MoveArrayElement(selected, selected + 1);
                ++selected;
                editor.Repaint();
            }

            if (GUILayout.Button("Delete"))
            {
                data.Array.DeleteArrayElementAtIndex(selected);
                --selected;
                var onSelect = data.OnSelect;
                if (onSelect != null)
                    onSelect(data.Array, selected);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            for (var index = 0; index < data.Array.arraySize; ++index)
            {
                var num = index == selected ? 1 : 0;
                EditorGUI.BeginChangeCheck();
                var rect = EditorGUILayout.BeginVertical(num != 0
                    ? _styleBoxSelected
                    : _styleBoxNormal);
                data.OnDrawElement(data.Array, index);
                EditorGUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    selected = index;
                    data.OnSelect?.Invoke(data.Array, selected);
                }

                if (Event.current.type != EventType.MouseDown || !rect.Contains(Event.current.mousePosition)) continue;
                selected = selected != index ? index : -1;
                data.OnSelect?.Invoke(data.Array, selected);
                Event.current.Use();
            }
        }

        public struct EditableArray
        {
            public SerializedProperty Array;
            public int MaxElements;
            public Action<SerializedProperty, int> OnDrawElement;
            public Action<SerializedProperty, int> OnNewElement;
            public Action<SerializedProperty, int> OnSelect;
        }
    }
}