using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SceneNotes.Editor
{
    /// <summary>
    /// Draws floating note cards for enabled scene notes in the Unity Scene view.
    /// </summary>
    [InitializeOnLoad]
    internal static class SceneNoteSceneViewDrawer
    {
        private const float NOTE_WIDTH = 260f;
        private const float NOTE_PADDING = 10f;
        private const float TAIL_HALF_WIDTH = 11f;
        private const float MIN_TAIL_LENGTH = 28f;
        private const float MIN_NOTE_HEIGHT = 36f;
        private const float OUTLINE_WIDTH = 1f;
        private const float CORNER_RADIUS = 8f;
        private const float DIVIDER_TOP_MARGIN = 5f;
        private const float DIVIDER_BOTTOM_MARGIN = 7f;
        private const float MINIMIZE_BUTTON_SIZE = 16f;
        private const float MINIMIZE_BUTTON_INSET = 4f;
        private const float MINIMIZED_WIDTH = 190f;
        private const float MINIMIZED_HEIGHT = 32f;

        private static GUIStyle s_noteStyle;

        static SceneNoteSceneViewDrawer()
        {
            SceneView.duringSceneGui -= DrawSceneNotes;
            SceneView.duringSceneGui += DrawSceneNotes;
        }

        private static void DrawSceneNotes(SceneView sceneView)
        {
            if (!SceneNoteEditorSettings.ShowSceneNotes) return;
            Event currentEvent = Event.current;
            if (currentEvent.type != EventType.Repaint &&
                currentEvent.type != EventType.Layout &&
                currentEvent.type != EventType.MouseDown)
            {
                return;
            }

            SceneNoteSettings settings = SceneNoteSettingsProvider.Settings;
            if (!settings.ShowAllNotes) return;

            SceneNote[] sceneNotes = Object.FindObjectsByType<SceneNote>(
                FindObjectsInactive.Include);

            bool hasOpenGuiBlock = false;

            try
            {
                Handles.BeginGUI();
                hasOpenGuiBlock = true;

                foreach (SceneNote sceneNote in GetDrawableSceneNotesInDrawOrder(sceneNotes, sceneView, settings))
                {
                    DrawSceneNote(sceneNote, sceneView, settings);
                }
            }
            finally
            {
                if (hasOpenGuiBlock)
                    Handles.EndGUI();
            }
        }

        private static List<SceneNote> GetDrawableSceneNotesInDrawOrder(
            IEnumerable<SceneNote> sceneNotes,
            SceneView sceneView,
            SceneNoteSettings settings)
        {
            var drawableSceneNotes = new List<SceneNote>();

            foreach (SceneNote sceneNote in sceneNotes)
            {
                if (IsSceneNoteSelected(sceneNote)) continue;
                if (!ShouldDrawSceneNote(sceneNote, sceneView, settings)) continue;

                drawableSceneNotes.Add(sceneNote);
            }

            foreach (SceneNote sceneNote in sceneNotes)
            {
                if (!IsSceneNoteSelected(sceneNote)) continue;
                if (!ShouldDrawSceneNote(sceneNote, sceneView, settings)) continue;

                drawableSceneNotes.Add(sceneNote);
            }

            return drawableSceneNotes;
        }

        private static bool ShouldDrawSceneNote(
            SceneNote sceneNote,
            SceneView sceneView,
            SceneNoteSettings settings)
        {
            if (sceneNote == null || !sceneNote.IsNoteEnabled) return false;
            if (!sceneNote.UsesSceneWidget) return false;
            if (!sceneNote.gameObject.scene.IsValid()) return false;
            if (!sceneNote.HasContent) return false;

            string sectionKey = settings.NormalizeSectionKey(sceneNote.SectionKey);
            SceneNoteSectionDefinition section = settings.GetSectionOrDefault(sectionKey);
            if (section == null || !section.IsEnabled) return false;

            string sectionViewFilterKey = SceneNoteEditorSettings.SectionViewFilterKey;
            bool hasSectionViewFilter = !SceneNoteEditorSettings.IsAllSectionsFilter(sectionViewFilterKey) &&
                settings.GetSection(sectionViewFilterKey) != null;

            if (hasSectionViewFilter && sectionKey != sectionViewFilterKey) return false;

            if (sceneView == null || sceneView.camera == null) return true;

            Vector3 worldPosition = GetWorldPosition(sceneNote);
            if (settings.MaxVisibleDistance <= 0f) return true;

            float cameraDistance = Vector3.Distance(sceneView.camera.transform.position, worldPosition);

            return cameraDistance <= settings.MaxVisibleDistance;
        }

        private static void DrawSceneNote(
            SceneNote sceneNote,
            SceneView sceneView,
            SceneNoteSettings settings)
        {
            Vector3 anchorWorldPosition = GetAnchorWorldPosition(sceneNote);
            Vector3 bubbleWorldPosition = GetWorldPosition(sceneNote);
            Vector3 anchorGuiPosition = HandleUtility.WorldToGUIPointWithDepth(anchorWorldPosition);
            Vector3 bubbleGuiPosition = HandleUtility.WorldToGUIPointWithDepth(bubbleWorldPosition);

            if (sceneView.camera != null && anchorGuiPosition.z <= 0f) return;
            if (sceneView.camera != null && bubbleGuiPosition.z <= 0f) return;

            GUIContent headerContent = new GUIContent(sceneNote.gameObject.name);
            string noteContent = sceneNote.NoteTitle;
            if (string.IsNullOrWhiteSpace(sceneNote.NoteTitle))
                noteContent = sceneNote.NoteBody;
            else if (!string.IsNullOrWhiteSpace(sceneNote.NoteBody))
                noteContent = $"{sceneNote.NoteTitle}\n{sceneNote.NoteBody}";

            GUIContent bodyContent = new GUIContent(noteContent);
            SceneNoteCategoryDefinition category = settings.GetCategoryOrDefault(sceneNote.CategoryKey);
            Color noteColor = category == null ? Color.white : category.Color;
            GUIStyle noteStyle = GetNoteStyle(noteColor.a);
            float noteWidth = sceneNote.IsMinimized ? MINIMIZED_WIDTH : NOTE_WIDTH;
            float contentWidth = noteWidth - NOTE_PADDING * 2f;
            float headerHeight = noteStyle.CalcHeight(headerContent, contentWidth);
            float bodyHeight = noteStyle.CalcHeight(bodyContent, contentWidth);
            float noteHeight = sceneNote.IsMinimized
                ? MINIMIZED_HEIGHT
                : Mathf.Max(
                    MIN_NOTE_HEIGHT,
                    NOTE_PADDING * 2f + headerHeight + DIVIDER_TOP_MARGIN + OUTLINE_WIDTH + DIVIDER_BOTTOM_MARGIN + bodyHeight);

            Rect noteRect = new Rect(
                bubbleGuiPosition.x - noteWidth * 0.5f,
                bubbleGuiPosition.y - noteHeight * 0.5f,
                noteWidth,
                noteHeight);

            noteRect = KeepNoteRectAboveAnchor(noteRect, anchorGuiPosition);

            HandleSceneNoteInput(sceneNote, noteRect);
            DrawBubble(noteRect, noteColor, anchorGuiPosition);

            if (sceneNote.IsMinimized)
                DrawMinimizedText(noteRect, headerContent, noteStyle);
            else
                DrawText(noteRect, headerContent, bodyContent, noteStyle, noteColor);

            DrawMinimizeButton(noteRect, sceneNote.IsMinimized, noteColor);
        }

        private static void DrawBubble(Rect noteRect, Color noteColor, Vector2 anchorGuiPosition)
        {
            Color backgroundColor = new Color(
                Mathf.Lerp(0.05f, noteColor.r, 0.22f),
                Mathf.Lerp(0.05f, noteColor.g, 0.22f),
                Mathf.Lerp(0.05f, noteColor.b, 0.22f),
                noteColor.a);

            Color outlineColor = new Color(noteColor.r, noteColor.g, noteColor.b, noteColor.a);
            GetTailPoints(noteRect, anchorGuiPosition, out Vector2 tailLeft, out Vector2 tailRight);

            Color previousHandleColor = Handles.color;

            Handles.color = backgroundColor;
            Handles.DrawAAConvexPolygon(tailLeft, anchorGuiPosition, tailRight);

            DrawRoundedRect(noteRect, outlineColor, CORNER_RADIUS);

            Rect innerRect = new Rect(
                noteRect.x + OUTLINE_WIDTH,
                noteRect.y + OUTLINE_WIDTH,
                noteRect.width - OUTLINE_WIDTH * 2f,
                noteRect.height - OUTLINE_WIDTH * 2f);

            DrawRoundedRect(innerRect, backgroundColor, CORNER_RADIUS - OUTLINE_WIDTH);

            Handles.color = outlineColor;
            Handles.DrawAAPolyLine(OUTLINE_WIDTH, tailLeft, anchorGuiPosition, tailRight);

            Handles.color = previousHandleColor;
        }

        private static void HandleSceneNoteInput(SceneNote sceneNote, Rect noteRect)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type != EventType.MouseDown) return;
            if (currentEvent.button != 0) return;

            Rect targetRect = sceneNote.IsMinimized
                ? noteRect
                : GetMinimizeButtonRect(noteRect);

            if (!targetRect.Contains(currentEvent.mousePosition)) return;

            Undo.RecordObject(sceneNote, sceneNote.IsMinimized ? "Expand Scene Note" : "Minimize Scene Note");
            sceneNote.SetMinimized(!sceneNote.IsMinimized);
            EditorUtility.SetDirty(sceneNote);
            SceneView.RepaintAll();
            currentEvent.Use();
        }

        private static void DrawText(
            Rect noteRect,
            GUIContent headerContent,
            GUIContent bodyContent,
            GUIStyle noteStyle,
            Color noteColor)
        {
            float contentWidth = noteRect.width - NOTE_PADDING * 2f;
            float headerHeight = noteStyle.CalcHeight(headerContent, contentWidth);
            Color dividerColor = new Color(noteColor.r, noteColor.g, noteColor.b, noteColor.a);

            Rect headerRect = new Rect(
                noteRect.x + NOTE_PADDING,
                noteRect.y + NOTE_PADDING,
                contentWidth,
                headerHeight);

            Rect dividerRect = new Rect(
                headerRect.x,
                headerRect.yMax + DIVIDER_TOP_MARGIN,
                contentWidth,
                OUTLINE_WIDTH);

            Rect bodyRect = new Rect(
                headerRect.x,
                dividerRect.yMax + DIVIDER_BOTTOM_MARGIN,
                contentWidth,
                noteRect.yMax - NOTE_PADDING - dividerRect.yMax - DIVIDER_BOTTOM_MARGIN);

            GUI.Label(headerRect, headerContent, noteStyle);
            EditorGUI.DrawRect(dividerRect, dividerColor);
            GUI.Label(bodyRect, bodyContent, noteStyle);
        }

        private static void DrawMinimizedText(Rect noteRect, GUIContent headerContent, GUIStyle noteStyle)
        {
            Rect textRect = new Rect(
                noteRect.x + NOTE_PADDING,
                noteRect.y + NOTE_PADDING * 0.5f,
                noteRect.width - NOTE_PADDING * 2f - MINIMIZE_BUTTON_SIZE,
                noteRect.height - NOTE_PADDING);

            GUI.Label(textRect, headerContent, noteStyle);
        }

        private static void DrawMinimizeButton(Rect noteRect, bool isMinimized, Color noteColor)
        {
            Rect buttonRect = GetMinimizeButtonRect(noteRect);
            string glyph = isMinimized ? "+" : "-";

            GUIStyle buttonStyle = GetNoteStyle(noteColor.a);
            TextAnchor previousAlignment = buttonStyle.alignment;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(buttonRect, glyph, buttonStyle);
            buttonStyle.alignment = previousAlignment;
        }

        private static Rect GetMinimizeButtonRect(Rect noteRect)
        {
            return new Rect(
                noteRect.xMax - MINIMIZE_BUTTON_SIZE - MINIMIZE_BUTTON_INSET,
                noteRect.y + MINIMIZE_BUTTON_INSET,
                MINIMIZE_BUTTON_SIZE,
                MINIMIZE_BUTTON_SIZE);
        }

        private static Vector3 GetWorldPosition(SceneNote sceneNote)
        {
            return sceneNote.transform.position + sceneNote.SceneOffset;
        }

        private static Vector3 GetAnchorWorldPosition(SceneNote sceneNote)
        {
            Renderer renderer = sceneNote.GetComponentInChildren<Renderer>(true);

            if (renderer != null)
                return GetBoundsTopCenter(renderer.bounds);

            Collider collider = sceneNote.GetComponentInChildren<Collider>(true);

            if (collider != null)
                return GetBoundsTopCenter(collider.bounds);

            return sceneNote.transform.position;
        }

        private static Vector3 GetBoundsTopCenter(Bounds bounds)
        {
            return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }

        private static bool IsSceneNoteSelected(SceneNote sceneNote)
        {
            if (sceneNote == null) return false;

            Object[] selectedObjects = Selection.objects;

            foreach (Object selectedObject in selectedObjects)
            {
                if (selectedObject == sceneNote || selectedObject == sceneNote.gameObject)
                    return true;
            }

            return false;
        }

        private static void GetTailPoints(Rect noteRect, Vector2 anchorGuiPosition, out Vector2 tailLeft, out Vector2 tailRight)
        {
            float tailCenterX = Mathf.Clamp(
                anchorGuiPosition.x,
                noteRect.xMin + TAIL_HALF_WIDTH,
                noteRect.xMax - TAIL_HALF_WIDTH);
            Vector2 tailCenter = new Vector2(tailCenterX, noteRect.yMax);

            tailLeft = new Vector2(tailCenter.x - TAIL_HALF_WIDTH, tailCenter.y);
            tailRight = new Vector2(tailCenter.x + TAIL_HALF_WIDTH, tailCenter.y);
        }

        private static Rect KeepNoteRectAboveAnchor(Rect noteRect, Vector2 anchorGuiPosition)
        {
            float maximumBottomY = anchorGuiPosition.y - MIN_TAIL_LENGTH;

            if (noteRect.yMax <= maximumBottomY)
                return noteRect;

            noteRect.y = maximumBottomY - noteRect.height;

            return noteRect;
        }

        private static void DrawRoundedRect(Rect rect, Color color, float radius)
        {
            Vector4 borderRadius = new Vector4(radius, radius, radius, radius);

            GUI.DrawTexture(
                rect,
                Texture2D.whiteTexture,
                ScaleMode.StretchToFill,
                true,
                0f,
                color,
                Vector4.zero,
                borderRadius);
        }

        private static GUIStyle GetNoteStyle(float alpha)
        {
            if (s_noteStyle == null)
            {
                s_noteStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    richText = false,
                    wordWrap = true
                };
            }

            s_noteStyle.normal.textColor = new Color(1f, 1f, 1f, alpha);

            return s_noteStyle;
        }

    }
}
