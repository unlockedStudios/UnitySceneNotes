using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SceneNotes
{
    /// <summary>
    /// Stores portable editor-only annotation text directly on a GameObject.
    /// </summary>
    /// <remarks>
    /// - Add this component to scene or prefab objects that need designer-facing context.
    /// - The editor drawer shows Scene view note blocks from this data.
    /// - Runtime code should treat this component as inert documentation data.
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("Scene Notes/Scene Note")]
    [InfoBox("Portable editor note. The component stores the note; the SceneNotes editor assembly draws the hierarchy icon and Scene view block.")]
    public class SceneNote : MonoBehaviour, ISerializationCallbackReceiver
    {
        [TitleGroup("Scene Note")]
        [ToggleLeft]
        [SerializeField]
        private bool _isNoteEnabled = true;

        [TitleGroup("Scene Note")]
        [EnumToggleButtons]
        [SerializeField]
        private SceneNoteDisplayMode _displayMode = SceneNoteDisplayMode.SelectedOnly;

        [TitleGroup("Content")]
        [LabelText("Title")]
        [SerializeField]
        private string _noteTitle = "Scene Note";

        [TitleGroup("Content")]
        [HideLabel]
        [MultiLineProperty(6)]
        [SerializeField]
        private string _noteBody = "";

        [TitleGroup("Visuals")]
        [ValueDropdown(nameof(GetCategoryDropdown))]
        [SerializeField]
        private string _categoryKey = "";

        [SerializeField, HideInInspector]
        private SceneNoteCategory _category = SceneNoteCategory.Note;

        [TitleGroup("Scene Widget")]
        [ShowIf(nameof(UsesSceneWidget))]
        [SerializeField]
        private Vector3 _sceneOffset = new Vector3(0f, 1.5f, 0f);

        [SerializeField, HideInInspector]
        private bool _isMinimized;

        public bool IsNoteEnabled => _isNoteEnabled;

        public SceneNoteDisplayMode DisplayMode => _displayMode;

        public string NoteTitle => _noteTitle;

        public string NoteBody => _noteBody;

        public string CategoryKey => string.IsNullOrWhiteSpace(_categoryKey)
            ? SceneNoteSettings.DEFAULT_CATEGORY_KEY
            : _categoryKey;

        public Vector3 SceneOffset => _sceneOffset;

        public bool IsMinimized => _isMinimized;

        public bool HasContent => !string.IsNullOrWhiteSpace(_noteTitle) || !string.IsNullOrWhiteSpace(_noteBody);

        public bool UsesSceneWidget => _displayMode != SceneNoteDisplayMode.HierarchyOnly;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            MigrateLegacyCategory();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            MigrateLegacyCategory();
            EditorApplication.RepaintHierarchyWindow();
            SceneView.RepaintAll();
        }
#endif

        public void SetNoteEnabled(bool isEnabled)
        {
            _isNoteEnabled = isEnabled;
        }

        public void SetMinimized(bool isMinimized)
        {
            _isMinimized = isMinimized;
        }

        [TitleGroup("Actions")]
        [HorizontalGroup("Actions/Buttons")]
        [Button(ButtonSizes.Medium)]
        private void ResetSceneOffset()
        {
            _sceneOffset = new Vector3(0f, 1.5f, 0f);
        }

#if UNITY_EDITOR
        [HorizontalGroup("Actions/Buttons")]
        [Button(ButtonSizes.Medium)]
        private void FrameInSceneView()
        {
            Selection.activeGameObject = gameObject;

            if (SceneView.lastActiveSceneView == null) return;

            SceneView.lastActiveSceneView.FrameSelected();
        }
#endif

        private void MigrateLegacyCategory()
        {
            if (!string.IsNullOrWhiteSpace(_categoryKey)) return;

            _categoryKey = GetLegacyCategoryKey(_category);
        }

        private static string GetLegacyCategoryKey(SceneNoteCategory category)
        {
            switch (category)
            {
                case SceneNoteCategory.Warning:
                    return "warning";
                case SceneNoteCategory.Todo:
                    return "todo";
                case SceneNoteCategory.Question:
                    return "question";
                case SceneNoteCategory.Landmark:
                    return "landmark";
                default:
                    return SceneNoteSettings.DEFAULT_CATEGORY_KEY;
            }
        }

#if UNITY_EDITOR
        private static IEnumerable<ValueDropdownItem<string>> GetCategoryDropdown()
        {
            SceneNoteSettings settings = LoadSettingsForInspector();

            if (settings == null)
            {
                yield return new ValueDropdownItem<string>("Note", SceneNoteSettings.DEFAULT_CATEGORY_KEY);
                yield break;
            }

            settings.EnsureDefaults();

            foreach (SceneNoteCategoryDefinition category in settings.Categories)
            {
                yield return new ValueDropdownItem<string>(category.DisplayName, category.Key);
            }
        }

        private static SceneNoteSettings LoadSettingsForInspector()
        {
            string[] guids = AssetDatabase.FindAssets("t:SceneNoteSettings");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SceneNoteSettings settings = AssetDatabase.LoadAssetAtPath<SceneNoteSettings>(path);

                if (settings != null)
                    return settings;
            }

            return null;
        }
#endif
    }
}
