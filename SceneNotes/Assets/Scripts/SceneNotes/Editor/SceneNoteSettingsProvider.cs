using System.IO;
using UnityEditor;
using UnityEngine;

namespace SceneNotes.Editor
{
    /// <summary>
    /// Loads, creates, and saves the shared project settings asset for the portable scene note tool.
    /// </summary>
    public static class SceneNoteSettingsProvider
    {
        private const string SETTINGS_FILE_NAME = "SceneNoteSettings.asset";

        private static SceneNoteSettings s_settings;

        public static SceneNoteSettings Settings
        {
            get
            {
                if (s_settings == null)
                    s_settings = LoadOrCreateSettings();

                return s_settings;
            }
        }

        public static void SaveSettings()
        {
            SceneNoteSettings settings = Settings;
            settings.EnsureDefaults();

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            EditorApplication.RepaintHierarchyWindow();
            SceneView.RepaintAll();
        }

        private static SceneNoteSettings LoadOrCreateSettings()
        {
            SceneNoteSettings settings = FindExistingSettings();

            if (settings == null)
            {
                EnsureSettingsFolderExists();
                settings = ScriptableObject.CreateInstance<SceneNoteSettings>();
                settings.EnsureDefaults();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }

            settings.EnsureDefaults();
            EditorUtility.SetDirty(settings);

            return settings;
        }

        private static SceneNoteSettings FindExistingSettings()
        {
            SceneNoteSettings settings = AssetDatabase.LoadAssetAtPath<SceneNoteSettings>(SettingsPath);

            if (settings != null)
                return settings;

            string[] guids = AssetDatabase.FindAssets("t:SceneNoteSettings");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                settings = AssetDatabase.LoadAssetAtPath<SceneNoteSettings>(path);

                if (settings != null)
                    return settings;
            }

            return null;
        }

        private static void EnsureSettingsFolderExists()
        {
            string settingsFolder = SceneNoteAssetLocator.SettingsFolder;

            if (AssetDatabase.IsValidFolder(settingsFolder)) return;

            string[] folderParts = settingsFolder.Split('/');
            string currentPath = folderParts[0];

            for (int i = 1; i < folderParts.Length; i++)
            {
                string nextPath = $"{currentPath}/{folderParts[i]}";

                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, folderParts[i]);

                currentPath = nextPath;
            }

            if (!Directory.Exists(settingsFolder))
                Directory.CreateDirectory(settingsFolder);
        }

        private static string SettingsPath => $"{SceneNoteAssetLocator.SettingsFolder}/{SETTINGS_FILE_NAME}";
    }
}
