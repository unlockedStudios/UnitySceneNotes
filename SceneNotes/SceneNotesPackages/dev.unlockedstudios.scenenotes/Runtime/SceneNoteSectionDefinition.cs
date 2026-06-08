using System;
using UnityEngine;

namespace SceneNotes
{
    /// <summary>
    /// Defines a shared scene note section used to group notes and control Scene view visibility.
    /// </summary>
    [Serializable]
    public class SceneNoteSectionDefinition
    {
        [SerializeField] private string _key;
        [SerializeField] private string _displayName;
        [SerializeField] private bool _isEnabled;
        [SerializeField] private bool _isBuiltIn;

        public SceneNoteSectionDefinition(string key, string displayName, bool isEnabled, bool isBuiltIn)
        {
            _key = key;
            _displayName = displayName;
            _isEnabled = isEnabled;
            _isBuiltIn = isBuiltIn;
        }

        public string Key => _key;

        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public bool IsBuiltIn => _isBuiltIn;

        public void SetKey(string key)
        {
            _key = key;
        }
    }
}
