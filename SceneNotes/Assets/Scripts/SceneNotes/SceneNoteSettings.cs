using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SceneNotes
{
    /// <summary>
    /// Shared project settings for all scene note editor overlays.
    /// </summary>
    /// <remarks>
    /// The editor provider owns asset creation and persistence. Runtime-facing code reads this asset
    /// so scene notes can resolve category colors and max visible distance from one portable source.
    /// </remarks>
    public class SceneNoteSettings : ScriptableObject
    {
        public const string DEFAULT_CATEGORY_KEY = "note";
        public const string DEFAULT_SECTION_KEY = "default";
        public const float DEFAULT_MAX_VISIBLE_DISTANCE = 40f;

        [SerializeField] private bool _showAllNotes = true;
        [SerializeField] private float _maxVisibleDistance = DEFAULT_MAX_VISIBLE_DISTANCE;
        [SerializeField] private List<SceneNoteCategoryDefinition> _categories = new List<SceneNoteCategoryDefinition>();
        [SerializeField] private List<SceneNoteSectionDefinition> _sections = new List<SceneNoteSectionDefinition>();

        public bool ShowAllNotes
        {
            get => _showAllNotes;
            set => _showAllNotes = value;
        }

        public float MaxVisibleDistance
        {
            get => _maxVisibleDistance;
            set => _maxVisibleDistance = Mathf.Max(0f, value);
        }

        public IReadOnlyList<SceneNoteCategoryDefinition> Categories => _categories;

        public IReadOnlyList<SceneNoteSectionDefinition> Sections => _sections;

        public void EnsureDefaults()
        {
            if (_categories == null)
                _categories = new List<SceneNoteCategoryDefinition>();

            if (_sections == null)
                _sections = new List<SceneNoteSectionDefinition>();

            if (_maxVisibleDistance <= 0f)
                _maxVisibleDistance = DEFAULT_MAX_VISIBLE_DISTANCE;

            EnsureBuiltInSection(DEFAULT_SECTION_KEY, "Default");

            EnsureBuiltInCategory(DEFAULT_CATEGORY_KEY, "Note", new Color(1f, 0.82f, 0.18f, 0.95f));
            EnsureBuiltInCategory("warning", "Warning", new Color(1f, 0.68f, 0.12f, 0.95f));
            EnsureBuiltInCategory("todo", "Todo", new Color(0.3f, 0.72f, 1f, 0.95f));
            EnsureBuiltInCategory("question", "Question", new Color(0.75f, 0.58f, 1f, 0.95f));
            EnsureBuiltInCategory("landmark", "Landmark", new Color(0.35f, 0.9f, 0.55f, 0.95f));

            RemoveInvalidCategories();
            RemoveInvalidSections();
        }

        public SceneNoteCategoryDefinition GetCategoryOrDefault(string categoryKey)
        {
            SceneNoteCategoryDefinition category = GetCategory(categoryKey);
            if (category != null) return category;

            return GetCategory(DEFAULT_CATEGORY_KEY);
        }

        public SceneNoteCategoryDefinition GetCategory(string categoryKey)
        {
            if (string.IsNullOrWhiteSpace(categoryKey)) return null;

            return _categories.FirstOrDefault(category => category.Key == categoryKey);
        }

        public SceneNoteCategoryDefinition AddCustomCategory(string displayName)
        {
            string cleanDisplayName = string.IsNullOrWhiteSpace(displayName) ? "Custom" : displayName.Trim();
            string key = CreateUniqueCategoryKey(cleanDisplayName);
            var category = new SceneNoteCategoryDefinition(
                key, cleanDisplayName, new Color(0.9f, 0.9f, 0.9f, 0.95f), false);

            _categories.Add(category);

            return category;
        }

        public bool RemoveCategory(string categoryKey)
        {
            SceneNoteCategoryDefinition category = GetCategory(categoryKey);

            if (category == null || category.IsBuiltIn) return false;

            return _categories.Remove(category);
        }

        public string NormalizeCategoryKey(string categoryKey)
        {
            SceneNoteCategoryDefinition category = GetCategoryOrDefault(categoryKey);

            return category == null ? DEFAULT_CATEGORY_KEY : category.Key;
        }

        public SceneNoteSectionDefinition GetSectionOrDefault(string sectionKey)
        {
            SceneNoteSectionDefinition section = GetSection(sectionKey);
            if (section != null) return section;

            return GetSection(DEFAULT_SECTION_KEY);
        }

        public SceneNoteSectionDefinition GetSection(string sectionKey)
        {
            if (string.IsNullOrWhiteSpace(sectionKey)) return null;

            return _sections.FirstOrDefault(section => section.Key == sectionKey);
        }

        public SceneNoteSectionDefinition AddCustomSection(string displayName)
        {
            string cleanDisplayName = string.IsNullOrWhiteSpace(displayName) ? "Custom Section" : displayName.Trim();
            string key = CreateUniqueSectionKey(cleanDisplayName);
            var section = new SceneNoteSectionDefinition(key, cleanDisplayName, true, false);

            _sections.Add(section);

            return section;
        }

        public bool RenameSection(string sectionKey, string displayName)
        {
            SceneNoteSectionDefinition section = GetSection(sectionKey);

            if (section == null || section.IsBuiltIn) return false;

            section.DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? "Custom Section"
                : displayName.Trim();

            return true;
        }

        public bool RemoveSection(string sectionKey)
        {
            SceneNoteSectionDefinition section = GetSection(sectionKey);

            if (section == null || section.IsBuiltIn) return false;

            return _sections.Remove(section);
        }

        public bool SetSectionEnabled(string sectionKey, bool isEnabled)
        {
            SceneNoteSectionDefinition section = GetSection(sectionKey);
            if (section == null) return false;

            section.IsEnabled = isEnabled;

            return true;
        }

        public void EnableAllSections()
        {
            foreach (SceneNoteSectionDefinition section in _sections)
            {
                section.IsEnabled = true;
            }
        }

        public string NormalizeSectionKey(string sectionKey)
        {
            SceneNoteSectionDefinition section = GetSectionOrDefault(sectionKey);

            return section == null ? DEFAULT_SECTION_KEY : section.Key;
        }

        private void EnsureBuiltInCategory(string key, string displayName, Color color)
        {
            SceneNoteCategoryDefinition category = GetCategory(key);

            if (category == null)
            {
                _categories.Add(new SceneNoteCategoryDefinition(key, displayName, color, true));
                return;
            }

            if (string.IsNullOrWhiteSpace(category.DisplayName))
                category.DisplayName = displayName;
        }

        private void EnsureBuiltInSection(string key, string displayName)
        {
            SceneNoteSectionDefinition section = GetSection(key);

            if (section == null)
            {
                _sections.Add(new SceneNoteSectionDefinition(key, displayName, true, true));
                return;
            }

            if (string.IsNullOrWhiteSpace(section.DisplayName))
                section.DisplayName = displayName;
        }

        private void RemoveInvalidCategories()
        {
            _categories.RemoveAll(category =>
                category == null ||
                string.IsNullOrWhiteSpace(category.Key) ||
                string.IsNullOrWhiteSpace(category.DisplayName));
        }

        private void RemoveInvalidSections()
        {
            _sections.RemoveAll(section =>
                section == null ||
                string.IsNullOrWhiteSpace(section.Key) ||
                string.IsNullOrWhiteSpace(section.DisplayName));
        }

        private string CreateUniqueCategoryKey(string displayName)
        {
            string baseKey = CreateKey(displayName);
            string key = baseKey;
            int suffix = 2;

            while (GetCategory(key) != null)
            {
                key = $"{baseKey}-{suffix}";
                suffix++;
            }

            return key;
        }

        private string CreateUniqueSectionKey(string displayName)
        {
            string baseKey = CreateKey(displayName);
            string key = baseKey;
            int suffix = 2;

            while (GetSection(key) != null)
            {
                key = $"{baseKey}-{suffix}";
                suffix++;
            }

            return key;
        }

        private static string CreateKey(string displayName)
        {
            string lowerName = displayName.Trim().ToLowerInvariant();
            var characters = new List<char>(lowerName.Length);
            bool previousWasSeparator = false;

            foreach (char character in lowerName)
            {
                if (char.IsLetterOrDigit(character))
                {
                    characters.Add(character);
                    previousWasSeparator = false;
                    continue;
                }

                if (previousWasSeparator) continue;

                characters.Add('-');
                previousWasSeparator = true;
            }

            string key = new string(characters.ToArray()).Trim('-');

            return string.IsNullOrWhiteSpace(key) ? "custom" : key;
        }
    }
}
