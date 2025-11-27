using System.IO;
using DuckovCustomModelTools.Utils;
using UnityEditor;
using UnityEngine;

namespace DuckovCustomModelTools.Windows
{
    public class GamePathSettingsWindow : EditorWindow
    {
        private void OnGUI()
        {
            EditorGUILayout.LabelField("游戏安装目录设置", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (Application.platform == RuntimePlatform.WindowsEditor)
                if (GUILayout.Button("自动查找"))
                {
                    var foundPath = GamePathSettings.FindSteamGamePath();
                    if (!string.IsNullOrEmpty(foundPath))
                    {
                        GamePathSettings.GameInstallPath = foundPath.Replace('\\', '/');
                        EditorUtility.DisplayDialog("成功", $"已找到游戏路径:\n{GamePathSettings.GameInstallPath}", "确定");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("未找到", "无法自动找到游戏路径，请手动设置。", "确定");
                    }
                }

            if (GUILayout.Button("浏览..."))
            {
                var originalCwd = Directory.GetCurrentDirectory();
                string path;
                try
                {
                    path = EditorUtility.OpenFolderPanel("选择游戏安装目录", GamePathSettings.GameInstallPath, "");
                }
                finally
                {
                    Directory.SetCurrentDirectory(originalCwd);
                }

                if (!string.IsNullOrEmpty(path)) GamePathSettings.GameInstallPath = path.Replace('\\', '/');
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("路径信息", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("游戏安装路径:");
            EditorGUILayout.LabelField(
                string.IsNullOrEmpty(GamePathSettings.GameInstallPath) ? "未设置" : GamePathSettings.GameInstallPath,
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();

            var modDirectory = GamePathSettings.GetModDirectory();
            EditorGUILayout.LabelField("Mod 目录:");
            EditorGUILayout.LabelField(string.IsNullOrEmpty(modDirectory) ? "未设置" : modDirectory,
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();

            var modelDirectory = GamePathSettings.GetModelDirectory();
            EditorGUILayout.LabelField("模型目录:");
            EditorGUILayout.LabelField(string.IsNullOrEmpty(modelDirectory) ? "未设置" : modelDirectory,
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("设置游戏安装目录后，Mod 可以自动复制到游戏目录。", MessageType.Info);
        }

        [MenuItem("Duckov Custom Model/游戏路径设置")]
        public static void ShowWindow()
        {
            var window = GetWindow<GamePathSettingsWindow>("游戏路径设置");
            window.Show();
        }
    }
}