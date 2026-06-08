using UnityEditor;

namespace SceneNotes.Editor
{
    /// <summary>
    /// Stores user-specific editor visibility preferences for the portable scene note drawers.
    /// </summary>
    internal static class SceneNoteEditorSettings
    {
        public const string ALL_SECTIONS_VIEW_NAME = "All Sections";
        public const string ALL_SECTIONS_VIEW_KEY = "__all_sections";

        private const string SHOW_SCENE_NOTES_KEY = "SceneNotes.ShowSceneNotes";
        private const string SECTION_VIEW_FILTER_KEY = "SceneNotes.SectionViewFilter";

        public static bool ShowSceneNotes
        {
            get => EditorPrefs.GetBool(SHOW_SCENE_NOTES_KEY, true);
            set => EditorPrefs.SetBool(SHOW_SCENE_NOTES_KEY, value);
        }

        public static string SectionViewFilterKey
        {
            get => EditorPrefs.GetString(SECTION_VIEW_FILTER_KEY, ALL_SECTIONS_VIEW_KEY);
            set => EditorPrefs.SetString(
                SECTION_VIEW_FILTER_KEY,
                string.IsNullOrWhiteSpace(value) ? ALL_SECTIONS_VIEW_KEY : value);
        }

        public static bool IsAllSectionsFilter(string sectionViewFilterKey)
        {
            return string.IsNullOrWhiteSpace(sectionViewFilterKey) ||
                sectionViewFilterKey == ALL_SECTIONS_VIEW_KEY;
        }
    }
}
