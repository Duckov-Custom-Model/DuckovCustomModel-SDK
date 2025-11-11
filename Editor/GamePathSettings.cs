using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;

namespace DuckovCustomModelTools
{
    [Serializable]
    public class GamePathSettings : ScriptableObject
    {
        private static GamePathSettings _instance;
        [SerializeField] private string gameInstallPath = string.Empty;

        public string GameInstallPath
        {
            get => gameInstallPath;
            set
            {
                gameInstallPath = value;
                EditorUtility.SetDirty(this);
            }
        }

        private static string GetSettingsPath()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(GamePathSettings)}");
            if (guids.Length > 0)
            {
                var existingPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                if (!string.IsNullOrEmpty(existingPath)) return existingPath;
            }

            var settingsDir = "Assets/UserSettings";
            if (!Directory.Exists(settingsDir)) Directory.CreateDirectory(settingsDir);
            return Path.Combine(settingsDir, "GamePathSettings.asset").Replace('\\', '/');
        }

        public static GamePathSettings Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var settingsPath = GetSettingsPath();
                _instance = AssetDatabase.LoadAssetAtPath<GamePathSettings>(settingsPath);
                if (_instance != null) return _instance;
                _instance = CreateInstance<GamePathSettings>();
                AssetDatabase.CreateAsset(_instance, settingsPath);
                AssetDatabase.SaveAssets();

                return _instance;
            }
        }

        public static string FindSteamGamePath()
        {
            if (Application.platform != RuntimePlatform.WindowsEditor) return string.Empty;

            try
            {
                const string steamRegKey = @"SOFTWARE\WOW6432Node\Valve\Steam";
                using var key = Registry.LocalMachine.OpenSubKey(steamRegKey);
                if (key == null) return string.Empty;

                var steamPath = key.GetValue("InstallPath") as string;
                if (string.IsNullOrEmpty(steamPath)) return string.Empty;

                var libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(libraryFoldersPath)) return string.Empty;

                var libraryFoldersContent = File.ReadAllText(libraryFoldersPath);
                const string gameName = "Escape from Duckov";
                var gamePath = FindGameInLibraryFolders(libraryFoldersContent, gameName, steamPath);

                return gamePath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"查找 Steam 游戏路径时出错: {ex.Message}");
                return string.Empty;
            }
        }

        private static string FindGameInLibraryFolders(string libraryFoldersContent, string gameName,
            string defaultSteamPath)
        {
            var paths = new[] { defaultSteamPath };

            var lines = libraryFoldersContent.Split('\n');
            foreach (var line in lines)
                if (line.Contains("\"path\""))
                {
                    var pathMatch = Regex.Match(line, @"\""path\""\s+\""([^""]+)\""");
                    if (!pathMatch.Success) continue;
                    var libraryPath = pathMatch.Groups[1].Value.Replace("\\\\", "\\");
                    if (!Directory.Exists(libraryPath)) continue;
                    Array.Resize(ref paths, paths.Length + 1);
                    paths[^1] = libraryPath;
                }

            foreach (var libraryPath in paths)
            {
                var gamePath = Path.Combine(libraryPath, "steamapps", "common", gameName);
                if (Directory.Exists(gamePath)) return gamePath;
            }

            return string.Empty;
        }

        public static string GetModDirectory()
        {
            var gamePath = Instance.GameInstallPath;
            return string.IsNullOrEmpty(gamePath)
                ? string.Empty
                : Path.Combine(gamePath, "Duckov_Data", "Mods").Replace('\\', '/');
        }

        public static string GetModelDirectory()
        {
            var gamePath = Instance.GameInstallPath;
            return string.IsNullOrEmpty(gamePath)
                ? string.Empty
                : Path.Combine(gamePath, "ModConfigs", "DuckovCustomModel", "Models").Replace('\\', '/');
        }
    }
}