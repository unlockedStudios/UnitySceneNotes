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
        public const float DEFAULT_MAX_VISIBLE_DISTANCE = 40f;

        [SerializeField] private bool _showAllNotes = true;
        [SerializeField] private float _maxVisibleDistance = DEFAULT_MAX_VISIBLE_DISTANCE;
        [SerializeField] private List<SceneNoteCategoryDefinition> _categories = new List<SceneNoteCategoryDefinition>();

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

        public void EnsureDefaults()
        {
            if (_maxVisibleDistance <= 0f)
                _maxVisibleDistance = DEFAULT_MAX_VISIBLE_DISTANCE;

            EnsureBuiltInCategory(DEFAULT_CATEGORY_KEY, "Note", new Color(1f, 0.82f, 0.18f, 0.95f));
            EnsureBuiltInCategory("warning", "Warning", new Color(1f, 0.68f, 0.12f, 0.95f));
            EnsureBuiltInCategory("todo", "Todo", new Color(0.3f, 0.72f, 1f, 0.95f));
            EnsureBuiltInCategory("question", "Question", new Color(0.75f, 0.58f, 1f, 0.95f));
            EnsureBuiltInCategory("landmark", "Landmark", new Color(0.35f, 0.9f, 0.55f, 0.95f));

            RemoveInvalidCategories();
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
            string key = CreateUniqueKey(cleanDisplayName);
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

        private void RemoveInvalidCategories()
        {
            _categories.RemoveAll(category =>
                category == null ||
                string.IsNullOrWhiteSpace(category.Key) ||
                string.IsNullOrWhiteSpace(category.DisplayName));
        }

        private string CreateUniqueKey(string displayName)
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
