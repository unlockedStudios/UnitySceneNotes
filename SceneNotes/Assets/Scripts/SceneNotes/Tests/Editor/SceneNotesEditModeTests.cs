using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SceneNotes.Editor.Tests
{
    public sealed class SceneNotesEditModeTests
    {
        private const string ALL_SECTIONS_VIEW_KEY = "__all_sections";

        private readonly List<GameObject> _gameObjects = new List<GameObject>();
        private readonly List<SceneNote> _sceneNotes = new List<SceneNote>();

        private SceneNoteSettings _settings;
        private SceneNoteSectionDefinition _questOneSection;
        private SceneNoteSectionDefinition _questTwoSection;

        [SetUp]
        public void SetUp()
        {
            Selection.objects = Array.Empty<Object>();
            SetSectionViewFilter(ALL_SECTIONS_VIEW_KEY);

            _settings = ScriptableObject.CreateInstance<SceneNoteSettings>();
            _settings.EnsureDefaults();
            _questOneSection = _settings.AddCustomSection("Quest 1");
            _questTwoSection = _settings.AddCustomSection("Quest 2");

            for (int i = 0; i < 8; i++)
            {
                var gameObject = new GameObject($"Scene Note Test {i + 1}");
                SceneNote sceneNote = gameObject.AddComponent<SceneNote>();

                if (i < 3)
                    sceneNote.SetSectionKey(_questOneSection.Key);
                else if (i < 6)
                    sceneNote.SetSectionKey(_questTwoSection.Key);

                _gameObjects.Add(gameObject);
                _sceneNotes.Add(sceneNote);
            }
        }

        [TearDown]
        public void TearDown()
        {
            Selection.objects = Array.Empty<Object>();
            SetSectionViewFilter(ALL_SECTIONS_VIEW_KEY);

            foreach (GameObject gameObject in _gameObjects)
            {
                if (gameObject != null)
                    Object.DestroyImmediate(gameObject);
            }

            if (_settings != null)
                Object.DestroyImmediate(_settings);

            _gameObjects.Clear();
            _sceneNotes.Clear();
        }

        [Test]
        public void DifferentCategoriesUseDifferentColors()
        {
            SetCategoryKey(_sceneNotes[0], "landmark");
            SetCategoryKey(_sceneNotes[1], "warning");

            Color firstColor = _settings.GetCategoryOrDefault(_sceneNotes[0].CategoryKey).Color;
            Color secondColor = _settings.GetCategoryOrDefault(_sceneNotes[1].CategoryKey).Color;

            Assert.That(firstColor, Is.Not.EqualTo(secondColor));
        }

        [Test]
        public void SectionFilterShowsOnlyMatchingSectionNotes()
        {
            SetSectionViewFilter(_questOneSection.Key);

            List<SceneNote> drawableNotes = GetDrawableSceneNotesInDrawOrder();

            Assert.That(drawableNotes, Has.Count.EqualTo(3));
            Assert.That(drawableNotes.All(sceneNote => sceneNote.SectionKey == _questOneSection.Key), Is.True);
        }

        [Test]
        public void AllSectionsFilterShowsEveryEnabledSection()
        {
            SetSectionViewFilter(ALL_SECTIONS_VIEW_KEY);

            List<SceneNote> drawableNotes = GetDrawableSceneNotesInDrawOrder();

            Assert.That(drawableNotes, Has.Count.EqualTo(8));
        }

        [Test]
        public void SelectedSceneNoteDrawsLastWithinFilteredSection()
        {
            SetSectionViewFilter(_questOneSection.Key);
            Selection.activeGameObject = _gameObjects[1];

            List<SceneNote> drawableNotes = GetDrawableSceneNotesInDrawOrder();

            Assert.That(drawableNotes, Has.Count.EqualTo(3));
            Assert.That(drawableNotes[drawableNotes.Count - 1], Is.SameAs(_sceneNotes[1]));
        }

        [Test]
        public void DeletingSectionMovesLoadedSceneNotesToDefault()
        {
            VESceneNotesWindowController controller = CreateController();

            InvokeControllerMethod(controller, "RefreshSelectedSection", _questOneSection.Key);
            InvokeControllerMethod(controller, "RemoveSelectedSection");

            Assert.That(_settings.GetSection(_questOneSection.Key), Is.Null);
            Assert.That(_sceneNotes.Take(3).All(sceneNote => sceneNote.SectionKey == SceneNoteSettings.DEFAULT_SECTION_KEY), Is.True);
            Assert.That(_sceneNotes.Skip(3).Take(3).All(sceneNote => sceneNote.SectionKey == _questTwoSection.Key), Is.True);
        }

        [Test]
        public void DisabledSectionNotesAreHidden()
        {
            _settings.SetSectionEnabled(_questOneSection.Key, false);
            SetSectionViewFilter(ALL_SECTIONS_VIEW_KEY);

            List<SceneNote> drawableNotes = GetDrawableSceneNotesInDrawOrder();

            Assert.That(drawableNotes, Has.Count.EqualTo(5));
            Assert.That(drawableNotes.Any(sceneNote => sceneNote.SectionKey == _questOneSection.Key), Is.False);
        }

        [Test]
        public void HierarchyOnlyNotesAreHiddenFromSceneViewDrawOrder()
        {
            SetDisplayMode(_sceneNotes[0], SceneNoteDisplayMode.HierarchyOnly);

            List<SceneNote> drawableNotes = GetDrawableSceneNotesInDrawOrder();

            Assert.That(drawableNotes, Has.Count.EqualTo(7));
            Assert.That(drawableNotes.Any(sceneNote => sceneNote == _sceneNotes[0]), Is.False);
        }

        [Test]
        public void NotesWithoutTitleOrBodyAreHidden()
        {
            SetText(_sceneNotes[0], "", "");

            List<SceneNote> drawableNotes = GetDrawableSceneNotesInDrawOrder();

            Assert.That(drawableNotes, Has.Count.EqualTo(7));
            Assert.That(drawableNotes.Any(sceneNote => sceneNote == _sceneNotes[0]), Is.False);
        }

        private List<SceneNote> GetDrawableSceneNotesInDrawOrder()
        {
            Type drawerType = typeof(VESceneNotesWindowController).Assembly.GetType(
                "SceneNotes.Editor.SceneNoteSceneViewDrawer");
            MethodInfo methodInfo = drawerType.GetMethod(
                "GetDrawableSceneNotesInDrawOrder",
                BindingFlags.NonPublic | BindingFlags.Static);

            return ((IEnumerable<SceneNote>)methodInfo.Invoke(
                null,
                new object[] { _sceneNotes.ToArray(), null, _settings })).ToList();
        }

        private VESceneNotesWindowController CreateController()
        {
            var root = new VisualElement();
            root.Add(new Toggle { name = "show-all-notes-toggle" });
            root.Add(new FloatField { name = "max-visible-distance-field" });
            root.Add(new DropdownField { name = "category-dropdown" });
            root.Add(new TextField { name = "category-name-field" });
            root.Add(new ColorField { name = "category-color-field" });
            root.Add(new Button { name = "add-category-button" });
            root.Add(new Button { name = "remove-category-button" });
            root.Add(new VisualElement { name = "category-list" });
            root.Add(new DropdownField { name = "section-view-dropdown" });
            root.Add(new DropdownField { name = "section-dropdown" });
            root.Add(new Toggle { name = "section-enabled-toggle" });
            root.Add(new TextField { name = "section-name-field" });
            root.Add(new Button { name = "add-section-button" });
            root.Add(new Button { name = "remove-section-button" });
            root.Add(new Button { name = "enable-all-sections-button" });
            root.Add(new VisualElement { name = "section-list" });

            return new VESceneNotesWindowController(root, _settings);
        }

        private static void InvokeControllerMethod(
            VESceneNotesWindowController controller,
            string methodName,
            params object[] arguments)
        {
            MethodInfo methodInfo = typeof(VESceneNotesWindowController).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(controller, arguments);
        }

        private static void SetSectionViewFilter(string sectionKey)
        {
            Type editorSettingsType = typeof(VESceneNotesWindowController).Assembly.GetType(
                "SceneNotes.Editor.SceneNoteEditorSettings");
            PropertyInfo propertyInfo = editorSettingsType.GetProperty(
                "SectionViewFilterKey",
                BindingFlags.Public | BindingFlags.Static);
            propertyInfo.SetValue(null, sectionKey);
        }

        private static void SetCategoryKey(SceneNote sceneNote, string categoryKey)
        {
            SerializedObject serializedObject = new SerializedObject(sceneNote);
            serializedObject.FindProperty("_categoryKey").stringValue = categoryKey;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetDisplayMode(SceneNote sceneNote, SceneNoteDisplayMode displayMode)
        {
            SerializedObject serializedObject = new SerializedObject(sceneNote);
            serializedObject.FindProperty("_displayMode").enumValueIndex = (int)displayMode;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetText(SceneNote sceneNote, string title, string body)
        {
            SerializedObject serializedObject = new SerializedObject(sceneNote);
            serializedObject.FindProperty("_noteTitle").stringValue = title;
            serializedObject.FindProperty("_noteBody").stringValue = body;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
