using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SceneNotes.Editor.Tests
{
    public sealed class SceneNoteInspectorOptionsTests
    {
        private SceneNoteSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SceneNoteSettings>();
            _settings.EnsureDefaults();
            _settings.AddCustomCategory("Quest Landmark");
            _settings.AddCustomSection("Quest Branch A");
        }

        [TearDown]
        public void TearDown()
        {
            if (_settings != null)
                Object.DestroyImmediate(_settings);
        }

        [Test]
        public void CategoryOptionsIncludeEverySettingsCategoryKey()
        {
            SceneNoteInspectorOption[] options = SceneNoteInspectorOptions.GetCategoryOptions(_settings, "");
            string[] optionKeys = options.Select(option => option.Key).ToArray();

            foreach (SceneNoteCategoryDefinition category in _settings.Categories)
            {
                Assert.That(optionKeys.Contains(category.Key), Is.True);
            }
        }

        [Test]
        public void SectionOptionsIncludeEverySettingsSectionKey()
        {
            SceneNoteInspectorOption[] options = SceneNoteInspectorOptions.GetSectionOptions(_settings, "");
            string[] optionKeys = options.Select(option => option.Key).ToArray();

            foreach (SceneNoteSectionDefinition section in _settings.Sections)
            {
                Assert.That(optionKeys.Contains(section.Key), Is.True);
            }
        }

        [Test]
        public void MissingCategoryKeyIsPreservedAsFirstOption()
        {
            const string missingKey = "legacy-category";

            SceneNoteInspectorOption[] options = SceneNoteInspectorOptions.GetCategoryOptions(_settings, missingKey);

            Assert.That(options[0].Key, Is.EqualTo(missingKey));
            Assert.That(options[0].Label, Is.EqualTo($"Missing ({missingKey})"));
        }

        [Test]
        public void MissingSectionKeyIsPreservedAsFirstOption()
        {
            const string missingKey = "legacy-section";

            SceneNoteInspectorOption[] options = SceneNoteInspectorOptions.GetSectionOptions(_settings, missingKey);

            Assert.That(options[0].Key, Is.EqualTo(missingKey));
            Assert.That(options[0].Label, Is.EqualTo($"Missing ({missingKey})"));
        }
    }
}