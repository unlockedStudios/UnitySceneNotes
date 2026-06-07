using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneNotes.Editor
{
    /// <summary>
    /// UI Toolkit editor window for shared Scene Notes settings.
    /// </summary>
    internal class SceneNotesWindow : EditorWindow
    {
        private const string WINDOW_MENU = "Tools/Scene Notes/Settings";
        private VESceneNotesWindowController _controller;

        [MenuItem(WINDOW_MENU, false, 90)]
        private static void OpenWindow()
        {
            SceneNotesWindow window = GetWindow<SceneNotesWindow>();
            window.titleContent = new GUIContent("Scene Notes");
            window.minSize = new Vector2(360f, 360f);
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();

            VisualTreeAsset visualTreeAsset = SceneNoteAssetLocator.LoadWindowVisualTree();
            StyleSheet styleSheet = SceneNoteAssetLocator.LoadWindowStyleSheet();

            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);

            if (visualTreeAsset != null)
            {
                visualTreeAsset.CloneTree(rootVisualElement);
            }
            else
            {
                rootVisualElement.Add(new Label("Scene Notes window UXML could not be loaded."));
                return;
            }

            _controller = new VESceneNotesWindowController(rootVisualElement, SceneNoteSettingsProvider.Settings);
        }
    }
}
