using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SceneNotes.Editor
{
    /// <summary>
    /// Draws the SceneNote inspector with category/section dropdowns sourced from Scene Notes settings.
    /// </summary>
    [CustomEditor(typeof(SceneNote))]
    internal sealed class SceneNoteInspector : UnityEditor.Editor
    {
        private SerializedProperty _isNoteEnabledProperty;
        private SerializedProperty _displayModeProperty;
        private SerializedProperty _noteTitleProperty;
        private SerializedProperty _noteBodyProperty;
        private SerializedProperty _categoryKeyProperty;
        private SerializedProperty _sectionKeyProperty;
        private SerializedProperty _sceneOffsetProperty;

        private void OnEnable()
        {
            _isNoteEnabledProperty = serializedObject.FindProperty("_isNoteEnabled");
            _displayModeProperty = serializedObject.FindProperty("_displayMode");
            _noteTitleProperty = serializedObject.FindProperty("_noteTitle");
            _noteBodyProperty = serializedObject.FindProperty("_noteBody");
            _categoryKeyProperty = serializedObject.FindProperty("_categoryKey");
            _sectionKeyProperty = serializedObject.FindProperty("_sectionKey");
            _sceneOffsetProperty = serializedObject.FindProperty("_sceneOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Scene Note", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_isNoteEnabledProperty, new GUIContent("Is Note Enabled"));
            EditorGUILayout.PropertyField(_displayModeProperty, new GUIContent("Display Mode"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Content", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_noteTitleProperty, new GUIContent("Note Title"));
            EditorGUILayout.PropertyField(_noteBodyProperty, new GUIContent("Note Body"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
            DrawKeyDropdown(_categoryKeyProperty, "Category Key", isCategory: true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Organization", EditorStyles.boldLabel);
            DrawKeyDropdown(_sectionKeyProperty, "Section Key", isCategory: false);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_sceneOffsetProperty, new GUIContent("Scene Offset"));

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawKeyDropdown(SerializedProperty keyProperty, string label, bool isCategory)
        {
            string currentKey = keyProperty.stringValue;
            SceneNoteSettings settings = SceneNoteSettingsProvider.Settings;
            SceneNoteInspectorOption[] options = isCategory
                ? SceneNoteInspectorOptions.GetCategoryOptions(settings, currentKey)
                : SceneNoteInspectorOptions.GetSectionOptions(settings, currentKey);

            if (options.Length == 0)
            {
                EditorGUILayout.PropertyField(keyProperty, new GUIContent(label));
                return;
            }

            int selectedIndex = SceneNoteInspectorOptions.GetSelectedIndex(options, currentKey);
            string[] labels = options.Select(option => option.Label).ToArray();
            int newIndex = EditorGUILayout.Popup(label, selectedIndex, labels);
            keyProperty.stringValue = options[newIndex].Key;
        }
    }

    /// <summary>
    /// Provides key/label dropdown options for SceneNote inspector fields.
    /// </summary>
    public static class SceneNoteInspectorOptions
    {
        public static SceneNoteInspectorOption[] GetCategoryOptions(SceneNoteSettings settings, string currentKey)
        {
            if (settings == null) return BuildFallback(currentKey, SceneNoteSettings.DEFAULT_CATEGORY_KEY);

            List<SceneNoteInspectorOption> options = settings.Categories
                .Where(category => category != null)
                .Select(category => new SceneNoteInspectorOption(category.Key, $"{category.DisplayName} ({category.Key})"))
                .ToList();

            return EnsureCurrentKeyOption(options, currentKey, SceneNoteSettings.DEFAULT_CATEGORY_KEY);
        }

        public static SceneNoteInspectorOption[] GetSectionOptions(SceneNoteSettings settings, string currentKey)
        {
            if (settings == null) return BuildFallback(currentKey, SceneNoteSettings.DEFAULT_SECTION_KEY);

            List<SceneNoteInspectorOption> options = settings.Sections
                .Where(section => section != null)
                .Select(section => new SceneNoteInspectorOption(section.Key, $"{section.DisplayName} ({section.Key})"))
                .ToList();

            return EnsureCurrentKeyOption(options, currentKey, SceneNoteSettings.DEFAULT_SECTION_KEY);
        }

        public static int GetSelectedIndex(SceneNoteInspectorOption[] options, string currentKey)
        {
            string normalizedKey = string.IsNullOrWhiteSpace(currentKey) ? options[0].Key : currentKey;

            for (int i = 0; i < options.Length; i++)
            {
                if (options[i].Key == normalizedKey)
                    return i;
            }

            return 0;
        }

        private static SceneNoteInspectorOption[] EnsureCurrentKeyOption(
            List<SceneNoteInspectorOption> options,
            string currentKey,
            string fallbackKey)
        {
            if (options.Count == 0)
                return BuildFallback(currentKey, fallbackKey);

            string normalizedKey = string.IsNullOrWhiteSpace(currentKey) ? fallbackKey : currentKey;
            bool hasCurrentKey = options.Any(option => option.Key == normalizedKey);

            if (!hasCurrentKey)
                options.Insert(0, new SceneNoteInspectorOption(normalizedKey, $"Missing ({normalizedKey})"));

            return options.ToArray();
        }

        private static SceneNoteInspectorOption[] BuildFallback(string currentKey, string fallbackKey)
        {
            string key = string.IsNullOrWhiteSpace(currentKey) ? fallbackKey : currentKey;
            return new[] { new SceneNoteInspectorOption(key, key) };
        }
    }

    public readonly struct SceneNoteInspectorOption
    {
        public SceneNoteInspectorOption(string key, string label)
        {
            Key = key;
            Label = label;
        }

        public string Key { get; }

        public string Label { get; }
    }
}