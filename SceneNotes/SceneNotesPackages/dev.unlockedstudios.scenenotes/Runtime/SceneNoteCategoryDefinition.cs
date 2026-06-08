using System;
using UnityEngine;

namespace SceneNotes
{
    /// <summary>
    /// Defines a shared scene note category and the color used to draw notes assigned to it.
    /// </summary>
    [Serializable]
    public class SceneNoteCategoryDefinition
    {
        [SerializeField] private string _key;
        [SerializeField] private string _displayName;
        [SerializeField] private Color _color;
        [SerializeField] private bool _isBuiltIn;

        public SceneNoteCategoryDefinition(string key, string displayName, Color color, bool isBuiltIn)
        {
            _key = key;
            _displayName = displayName;
            _color = color;
            _isBuiltIn = isBuiltIn;
        }

        public string Key => _key;

        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }

        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        public bool IsBuiltIn => _isBuiltIn;

        public void SetKey(string key)
        {
            _key = key;
        }
    }
}
