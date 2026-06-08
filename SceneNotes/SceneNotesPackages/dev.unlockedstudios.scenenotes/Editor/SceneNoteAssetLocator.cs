using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneNotes.Editor
{
    /// <summary>
    /// Resolves Scene Notes editor assets relative to this tool's current folder.
    /// </summary>
    internal static class SceneNoteAssetLocator
    {
        private const string EDITOR_ASMDEF_FILE_NAME = "SceneNotes.Editor.asmdef";
        private const string FALLBACK_ROOT_FOLDER = "Assets/Scripts/SceneNotes";

        public static string SettingsFolder => $"{RootFolder}/Settings";

        public static VisualTreeAsset LoadWindowVisualTree()
        {
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                $"{RootFolder}/UIToolkit/UIDocuments/SceneNotesWindow.uxml");
        }

        public static StyleSheet LoadWindowStyleSheet()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(
                $"{RootFolder}/UIToolkit/StyleSheets/SceneNotesWindow.uss");
        }

        private static string RootFolder
        {
            get
            {
                string editorFolder = FindEditorFolder();

                return string.IsNullOrEmpty(editorFolder)
                    ? FALLBACK_ROOT_FOLDER
                    : NormalizeAssetPath(Path.GetDirectoryName(editorFolder));
            }
        }

        private static string FindEditorFolder()
        {
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(EDITOR_ASMDEF_FILE_NAME));

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (Path.GetFileName(path) == EDITOR_ASMDEF_FILE_NAME)
                    return NormalizeAssetPath(Path.GetDirectoryName(path));
            }

            return null;
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrEmpty(path)
                ? path
                : path.Replace('\\', '/');
        }
    }
}
