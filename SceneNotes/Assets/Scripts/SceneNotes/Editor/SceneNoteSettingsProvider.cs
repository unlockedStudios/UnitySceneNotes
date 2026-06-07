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
        private const string SETTINGS_FOLDER = "Assets/_GameFiles/Scripts/_BackendApplications/SceneNotes/Settings";
        private const string SETTINGS_PATH = SETTINGS_FOLDER + "/SceneNoteSettings.asset";

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
                AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
                AssetDatabase.SaveAssets();
            }

            settings.EnsureDefaults();
            EditorUtility.SetDirty(settings);

            return settings;
        }

        private static SceneNoteSettings FindExistingSettings()
        {
            SceneNoteSettings settings = AssetDatabase.LoadAssetAtPath<SceneNoteSettings>(SETTINGS_PATH);

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
            if (AssetDatabase.IsValidFolder(SETTINGS_FOLDER)) return;

            string[] folderParts = SETTINGS_FOLDER.Split('/');
            string currentPath = folderParts[0];

            for (int i = 1; i < folderParts.Length; i++)
            {
                string nextPath = $"{currentPath}/{folderParts[i]}";

                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, folderParts[i]);

                currentPath = nextPath;
            }

            if (!Directory.Exists(SETTINGS_FOLDER))
                Directory.CreateDirectory(SETTINGS_FOLDER);
        }
    }
}
