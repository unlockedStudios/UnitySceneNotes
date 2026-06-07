using UnityEditor;

namespace SceneNotes.Editor
{
    /// <summary>
    /// Stores user-specific editor visibility preferences for the portable scene note drawers.
    /// </summary>
    internal static class SceneNoteEditorSettings
    {
        private const string SHOW_SCENE_NOTES_KEY = "SceneNotes.ShowSceneNotes";

        public static bool ShowSceneNotes
        {
            get => EditorPrefs.GetBool(SHOW_SCENE_NOTES_KEY, true);
            set => EditorPrefs.SetBool(SHOW_SCENE_NOTES_KEY, value);
        }
    }
}
