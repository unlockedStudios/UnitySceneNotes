using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneNotes.Editor
{
    /// <summary>
    /// Binds the Scene Notes UI Toolkit settings window to the shared settings asset.
    /// </summary>
    public class VESceneNotesWindowController
    {
        private readonly SceneNoteSettings _settings;
        private readonly Toggle _showAllNotesToggle;
        private readonly FloatField _maxVisibleDistanceField;
        private readonly DropdownField _categoryDropdown;
        private readonly TextField _categoryNameField;
        private readonly ColorField _categoryColorField;
        private readonly Button _addCategoryButton;
        private readonly Button _removeCategoryButton;
        private readonly VisualElement _categoryList;
        private readonly DropdownField _sectionViewDropdown;
        private readonly DropdownField _sectionDropdown;
        private readonly Toggle _sectionEnabledToggle;
        private readonly TextField _sectionNameField;
        private readonly Button _addSectionButton;
        private readonly Button _removeSectionButton;
        private readonly Button _enableAllSectionsButton;
        private readonly VisualElement _sectionList;

        private string _selectedCategoryKey;
        private string _selectedSectionKey;

        public VESceneNotesWindowController(VisualElement root, SceneNoteSettings settings)
        {
            _settings = settings;
            _showAllNotesToggle = root.Q<Toggle>("show-all-notes-toggle");
            _maxVisibleDistanceField = root.Q<FloatField>("max-visible-distance-field");
            _categoryDropdown = root.Q<DropdownField>("category-dropdown");
            _categoryNameField = root.Q<TextField>("category-name-field");
            _categoryColorField = root.Q<ColorField>("category-color-field");
            _addCategoryButton = root.Q<Button>("add-category-button");
            _removeCategoryButton = root.Q<Button>("remove-category-button");
            _categoryList = root.Q<VisualElement>("category-list");
            _sectionViewDropdown = root.Q<DropdownField>("section-view-dropdown");
            _sectionDropdown = root.Q<DropdownField>("section-dropdown");
            _sectionEnabledToggle = root.Q<Toggle>("section-enabled-toggle");
            _sectionNameField = root.Q<TextField>("section-name-field");
            _addSectionButton = root.Q<Button>("add-section-button");
            _removeSectionButton = root.Q<Button>("remove-section-button");
            _enableAllSectionsButton = root.Q<Button>("enable-all-sections-button");
            _sectionList = root.Q<VisualElement>("section-list");

            _categoryColorField.showAlpha = true;
            BindControls();
            RefreshAll(SceneNoteSettings.DEFAULT_CATEGORY_KEY, SceneNoteSettings.DEFAULT_SECTION_KEY);
        }

        private void BindControls()
        {
            _showAllNotesToggle.RegisterValueChangedCallback(OnShowAllNotesChanged);
            _maxVisibleDistanceField.RegisterValueChangedCallback(OnMaxVisibleDistanceChanged);
            _categoryDropdown.RegisterValueChangedCallback(OnCategoryDropdownChanged);
            _categoryNameField.RegisterValueChangedCallback(OnCategoryNameChanged);
            _categoryColorField.RegisterValueChangedCallback(OnCategoryColorChanged);
            _addCategoryButton.clicked += AddCategory;
            _removeCategoryButton.clicked += RemoveSelectedCategory;
            _sectionViewDropdown.RegisterValueChangedCallback(OnSectionViewDropdownChanged);
            _sectionDropdown.RegisterValueChangedCallback(OnSectionDropdownChanged);
            _sectionEnabledToggle.RegisterValueChangedCallback(OnSectionEnabledChanged);
            _sectionNameField.RegisterValueChangedCallback(OnSectionNameChanged);
            _addSectionButton.clicked += AddSection;
            _removeSectionButton.clicked += RemoveSelectedSection;
            _enableAllSectionsButton.clicked += EnableAllSections;
        }

        private void OnShowAllNotesChanged(ChangeEvent<bool> changeEvent)
        {
            Undo.RecordObject(_settings, "Toggle Scene Notes Show All");
            _settings.ShowAllNotes = changeEvent.newValue;
            SceneNoteSettingsProvider.SaveSettings(_settings);
        }

        private void OnMaxVisibleDistanceChanged(ChangeEvent<float> changeEvent)
        {
            Undo.RecordObject(_settings, "Change Scene Note Max Visible Distance");
            _settings.MaxVisibleDistance = changeEvent.newValue;
            SceneNoteSettingsProvider.SaveSettings(_settings);
        }

        private void OnCategoryDropdownChanged(ChangeEvent<string> changeEvent)
        {
            SceneNoteCategoryDefinition category = _settings.Categories
                .FirstOrDefault(candidate => candidate.DisplayName == changeEvent.newValue);

            if (category == null) return;

            RefreshSelectedCategory(category.Key);
        }

        private void OnCategoryNameChanged(ChangeEvent<string> changeEvent)
        {
            SceneNoteCategoryDefinition category = _settings.GetCategory(_selectedCategoryKey);

            if (category == null || category.IsBuiltIn) return;

            Undo.RecordObject(_settings, "Rename Scene Note Category");
            category.DisplayName = string.IsNullOrWhiteSpace(changeEvent.newValue)
                ? "Custom"
                : changeEvent.newValue.Trim();

            SceneNoteSettingsProvider.SaveSettings(_settings);
            RefreshAll(category.Key, _selectedSectionKey);
        }

        private void OnCategoryColorChanged(ChangeEvent<Color> changeEvent)
        {
            SceneNoteCategoryDefinition category = _settings.GetCategory(_selectedCategoryKey);
            if (category == null) return;

            Undo.RecordObject(_settings, "Change Scene Note Category Color");
            category.Color = changeEvent.newValue;
            SceneNoteSettingsProvider.SaveSettings(_settings);
            RefreshCategoryList();
        }

        private void AddCategory()
        {
            Undo.RecordObject(_settings, "Add Scene Note Category");
            SceneNoteCategoryDefinition category = _settings.AddCustomCategory("Custom Category");
            SceneNoteSettingsProvider.SaveSettings(_settings);
            RefreshAll(category.Key, _selectedSectionKey);
        }

        private void RemoveSelectedCategory()
        {
            SceneNoteCategoryDefinition category = _settings.GetCategory(_selectedCategoryKey);

            if (category == null || category.IsBuiltIn) return;

            Undo.RecordObject(_settings, "Remove Scene Note Category");
            _settings.RemoveCategory(category.Key);
            SceneNoteSettingsProvider.SaveSettings(_settings);
            RefreshAll(SceneNoteSettings.DEFAULT_CATEGORY_KEY, _selectedSectionKey);
        }

        private void OnSectionViewDropdownChanged(ChangeEvent<string> changeEvent)
        {
            if (changeEvent.newValue == SceneNoteEditorSettings.ALL_SECTIONS_VIEW_NAME)
            {
                SceneNoteEditorSettings.SectionViewFilterKey = SceneNoteEditorSettings.ALL_SECTIONS_VIEW_KEY;
                SceneView.RepaintAll();
                return;
            }

            SceneNoteSectionDefinition section = _settings.Sections
                .FirstOrDefault(candidate => candidate.DisplayName == changeEvent.newValue);

            if (section == null) return;

            SceneNoteEditorSettings.SectionViewFilterKey = section.Key;
            SceneView.RepaintAll();
        }

        private void OnSectionDropdownChanged(ChangeEvent<string> changeEvent)
        {
            SceneNoteSectionDefinition section = _settings.Sections
                .FirstOrDefault(candidate => candidate.DisplayName == changeEvent.newValue);

            if (section == null) return;

            RefreshSelectedSection(section.Key);
        }

        private void OnSectionEnabledChanged(ChangeEvent<bool> changeEvent)
        {
            Undo.RecordObject(_settings, "Toggle Scene Note Section");
            _settings.SetSectionEnabled(_selectedSectionKey, changeEvent.newValue);
            SceneNoteSettingsProvider.SaveSettings(_settings);
            RefreshSectionList();
        }

        private void OnSectionNameChanged(ChangeEvent<string> changeEvent)
        {
            SceneNoteSectionDefinition section = _settings.GetSection(_selectedSectionKey);

            if (section == null || section.IsBuiltIn) return;

            Undo.RecordObject(_settings, "Rename Scene Note Section");
            _settings.RenameSection(section.Key, changeEvent.newValue);
            SceneNoteSettingsProvider.SaveSettings(_settings);
            RefreshAll(_selectedCategoryKey, section.Key);
        }

        private void AddSection()
        {
            Undo.RecordObject(_settings, "Add Scene Note Section");
            SceneNoteSectionDefinition section = _settings.AddCustomSection("Custom Section");
            SceneNoteSettingsProvider.SaveSettings(_settings);
            RefreshAll(_selectedCategoryKey, section.Key);
        }

        private void RemoveSelectedSection()
        {
            SceneNoteSectionDefinition section = _settings.GetSection(_selectedSectionKey);

            if (section == null || section.IsBuiltIn) return;

            MoveLoadedSceneNotesToDefaultSection(section.Key);

            Undo.RecordObject(_settings, "Delete Scene Note Section");
            _settings.RemoveSection(section.Key);

            if (SceneNoteEditorSettings.SectionViewFilterKey == section.Key)
                SceneNoteEditorSettings.SectionViewFilterKey = SceneNoteEditorSettings.ALL_SECTIONS_VIEW_KEY;

            SceneNoteSettingsProvider.SaveSettings(_settings);
            RefreshAll(_selectedCategoryKey, SceneNoteSettings.DEFAULT_SECTION_KEY);
        }

        private void EnableAllSections()
        {
            Undo.RecordObject(_settings, "Enable All Scene Note Sections");
            _settings.EnableAllSections();
            SceneNoteSettingsProvider.SaveSettings(_settings);
            RefreshAll(_selectedCategoryKey, _selectedSectionKey);
        }

        private void RefreshAll(string selectedCategoryKey, string selectedSectionKey)
        {
            _settings.EnsureDefaults();
            _selectedCategoryKey = _settings.NormalizeCategoryKey(selectedCategoryKey);
            _selectedSectionKey = _settings.NormalizeSectionKey(selectedSectionKey);

            _showAllNotesToggle.SetValueWithoutNotify(_settings.ShowAllNotes);
            _maxVisibleDistanceField.SetValueWithoutNotify(_settings.MaxVisibleDistance);
            RefreshCategoryDropdown();
            RefreshSelectedCategory(_selectedCategoryKey);
            RefreshCategoryList();
            RefreshSectionViewDropdown();
            RefreshSectionDropdown();
            RefreshSelectedSection(_selectedSectionKey);
            RefreshSectionList();
        }

        private void RefreshCategoryDropdown()
        {
            var choices = new List<string>();

            foreach (SceneNoteCategoryDefinition category in _settings.Categories)
            {
                choices.Add(category.DisplayName);
            }

            _categoryDropdown.choices = choices;

            SceneNoteCategoryDefinition selectedCategory = _settings.GetCategoryOrDefault(_selectedCategoryKey);
            _categoryDropdown.SetValueWithoutNotify(selectedCategory.DisplayName);
        }

        private void RefreshSectionViewDropdown()
        {
            var choices = new List<string> { SceneNoteEditorSettings.ALL_SECTIONS_VIEW_NAME };

            foreach (SceneNoteSectionDefinition section in _settings.Sections)
            {
                choices.Add(section.DisplayName);
            }

            _sectionViewDropdown.choices = choices;

            string sectionViewFilterKey = SceneNoteEditorSettings.SectionViewFilterKey;
            SceneNoteSectionDefinition selectedSection = _settings.GetSection(sectionViewFilterKey);

            if (selectedSection == null)
            {
                SceneNoteEditorSettings.SectionViewFilterKey = SceneNoteEditorSettings.ALL_SECTIONS_VIEW_KEY;
                _sectionViewDropdown.SetValueWithoutNotify(SceneNoteEditorSettings.ALL_SECTIONS_VIEW_NAME);
                return;
            }

            _sectionViewDropdown.SetValueWithoutNotify(selectedSection.DisplayName);
        }

        private void RefreshSectionDropdown()
        {
            var choices = new List<string>();

            foreach (SceneNoteSectionDefinition section in _settings.Sections)
            {
                choices.Add(section.DisplayName);
            }

            _sectionDropdown.choices = choices;

            SceneNoteSectionDefinition selectedSection = _settings.GetSectionOrDefault(_selectedSectionKey);
            _sectionDropdown.SetValueWithoutNotify(selectedSection.DisplayName);
        }

        private void RefreshSelectedCategory(string categoryKey)
        {
            SceneNoteCategoryDefinition category = _settings.GetCategoryOrDefault(categoryKey);

            _selectedCategoryKey = category.Key;
            _categoryNameField.SetValueWithoutNotify(category.DisplayName);
            _categoryNameField.SetEnabled(!category.IsBuiltIn);
            _categoryColorField.SetValueWithoutNotify(category.Color);
            _removeCategoryButton.SetEnabled(!category.IsBuiltIn);
            RefreshCategoryList();
        }

        private void RefreshSelectedSection(string sectionKey)
        {
            SceneNoteSectionDefinition section = _settings.GetSectionOrDefault(sectionKey);

            _selectedSectionKey = section.Key;
            _sectionEnabledToggle.SetValueWithoutNotify(section.IsEnabled);
            _sectionNameField.SetValueWithoutNotify(section.DisplayName);
            _sectionNameField.SetEnabled(!section.IsBuiltIn);
            _removeSectionButton.SetEnabled(!section.IsBuiltIn);
            RefreshSectionList();
        }

        private void RefreshCategoryList()
        {
            _categoryList.Clear();

            foreach (SceneNoteCategoryDefinition category in _settings.Categories)
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("scene-notes-window__category-row");

                VisualElement swatch = new VisualElement();
                swatch.AddToClassList("scene-notes-window__swatch");
                swatch.style.backgroundColor = category.Color;

                Label label = new Label(category.DisplayName);
                label.AddToClassList("scene-notes-window__category-label");

                Label key = new Label(category.Key);
                key.AddToClassList("scene-notes-window__category-key");

                row.Add(swatch);
                row.Add(label);
                row.Add(key);
                _categoryList.Add(row);
            }
        }

        private void RefreshSectionList()
        {
            _sectionList.Clear();

            foreach (SceneNoteSectionDefinition section in _settings.Sections)
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("scene-notes-window__section-row");

                Label label = new Label(section.DisplayName);
                label.AddToClassList("scene-notes-window__section-label");

                Label state = new Label(section.IsEnabled ? "Enabled" : "Disabled");
                state.AddToClassList("scene-notes-window__section-state");

                Label key = new Label(section.Key);
                key.AddToClassList("scene-notes-window__section-key");

                row.Add(label);
                row.Add(state);
                row.Add(key);
                _sectionList.Add(row);
            }
        }

        private static void MoveLoadedSceneNotesToDefaultSection(string sectionKey)
        {
            SceneNote[] sceneNotes = Object.FindObjectsByType<SceneNote>(FindObjectsInactive.Include);

            foreach (SceneNote sceneNote in sceneNotes)
            {
                if (sceneNote.SectionKey != sectionKey) continue;

                Undo.RecordObject(sceneNote, "Move Scene Note To Default Section");
                sceneNote.SetSectionKey(SceneNoteSettings.DEFAULT_SECTION_KEY);
                EditorUtility.SetDirty(sceneNote);
            }
        }
    }
}
