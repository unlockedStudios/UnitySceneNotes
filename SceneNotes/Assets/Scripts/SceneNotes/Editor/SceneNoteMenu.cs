using UnityEditor;
using UnityEngine;

namespace SceneNotes.Editor
{
    /// <summary>
    /// Provides the global editor menu actions used to toggle and create scene notes.
    /// </summary>
    internal static class SceneNoteMenu
    {
        private const string SHOW_NOTES_MENU = "Tools/Scene Notes/Show Notes";
        private const string ADD_NOTE_TO_SELECTED_MENU = "Tools/Scene Notes/Add Note To Selected";

        [MenuItem(SHOW_NOTES_MENU, false, 100)]
        private static void ToggleSceneNotes()
        {
            SceneNoteEditorSettings.ShowSceneNotes = !SceneNoteEditorSettings.ShowSceneNotes;
            SceneView.RepaintAll();
        }

        [MenuItem(SHOW_NOTES_MENU, true)]
        private static bool ValidateToggleSceneNotes()
        {
            Menu.SetChecked(SHOW_NOTES_MENU, SceneNoteEditorSettings.ShowSceneNotes);
            return true;
        }

        [MenuItem(ADD_NOTE_TO_SELECTED_MENU, false, 120)]
        private static void AddNoteToSelected()
        {
            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                SceneNote sceneNote = selectedObject.GetComponent<SceneNote>();

                if (sceneNote == null)
                    sceneNote = Undo.AddComponent<SceneNote>(selectedObject);

                Undo.RecordObject(sceneNote, "Enable Scene Note");
                sceneNote.SetNoteEnabled(true);
                EditorUtility.SetDirty(sceneNote);
            }

            EditorApplication.RepaintHierarchyWindow();
            SceneView.RepaintAll();
        }

        [MenuItem(ADD_NOTE_TO_SELECTED_MENU, true)]
        private static bool ValidateAddNoteToSelected()
        {
            return Selection.gameObjects.Length > 0;
        }
    }
}
