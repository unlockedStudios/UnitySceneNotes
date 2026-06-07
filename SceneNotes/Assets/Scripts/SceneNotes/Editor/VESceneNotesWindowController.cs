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
        private readonly VisualElement _categoryList;
        private readonly Toggle _showAllNotesToggle;
        private readonly FloatField _maxVisibleDistanceField;
        private readonly DropdownField _categoryDropdown;
        private readonly TextField _categoryNameField;
        private readonly ColorField _categoryColorField;
        private readonly Button _addCategoryButton;
        private readonly Button _removeCategoryButton;

        private string _selectedCategoryKey;

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

            _categoryColorField.showAlpha = true;
            BindControls();
            RefreshAll(SceneNoteSettings.DEFAULT_CATEGORY_KEY);
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
        }

        private void OnShowAllNotesChanged(ChangeEvent<bool> changeEvent)
        {
            Undo.RecordObject(_settings, "Toggle Scene Notes Show All");
            _settings.ShowAllNotes = changeEvent.newValue;
            SceneNoteSettingsProvider.SaveSettings();
        }

        private void OnMaxVisibleDistanceChanged(ChangeEvent<float> changeEvent)
        {
            Undo.RecordObject(_settings, "Change Scene Note Max Visible Distance");
            _settings.MaxVisibleDistance = changeEvent.newValue;
            SceneNoteSettingsProvider.SaveSettings();
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

            SceneNoteSettingsProvider.SaveSettings();
            RefreshAll(category.Key);
        }

        private void OnCategoryColorChanged(ChangeEvent<Color> changeEvent)
        {
            SceneNoteCategoryDefinition category = _settings.GetCategory(_selectedCategoryKey);
            if (category == null) return;

            Undo.RecordObject(_settings, "Change Scene Note Category Color");
            category.Color = changeEvent.newValue;
            SceneNoteSettingsProvider.SaveSettings();
            RefreshCategoryList();
        }

        private void AddCategory()
        {
            Undo.RecordObject(_settings, "Add Scene Note Category");
            SceneNoteCategoryDefinition category = _settings.AddCustomCategory("Custom Category");
            SceneNoteSettingsProvider.SaveSettings();
            RefreshAll(category.Key);
        }

        private void RemoveSelectedCategory()
        {
            SceneNoteCategoryDefinition category = _settings.GetCategory(_selectedCategoryKey);

            if (category == null || category.IsBuiltIn) return;

            Undo.RecordObject(_settings, "Remove Scene Note Category");
            _settings.RemoveCategory(category.Key);
            SceneNoteSettingsProvider.SaveSettings();
            RefreshAll(SceneNoteSettings.DEFAULT_CATEGORY_KEY);
        }

        private void RefreshAll(string selectedCategoryKey)
        {
            _settings.EnsureDefaults();
            _selectedCategoryKey = _settings.NormalizeCategoryKey(selectedCategoryKey);

            _showAllNotesToggle.SetValueWithoutNotify(_settings.ShowAllNotes);
            _maxVisibleDistanceField.SetValueWithoutNotify(_settings.MaxVisibleDistance);
            RefreshDropdown();
            RefreshSelectedCategory(_selectedCategoryKey);
            RefreshCategoryList();
        }

        private void RefreshDropdown()
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
    }
}
