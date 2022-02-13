using UnityEditor;
using UnityEngine;

namespace Helpers.AutoObject.Editor
{
    [CustomEditor(typeof(AutoObject), true)]
    public class AutoObjectEditor : UnityEditor.Editor
    {
        private AutoObject creator;
        private SerializedProperty areReferencesSet;

        private void OnEnable()
        {
            creator = target as AutoObject;
            areReferencesSet = serializedObject.FindProperty("areReferencesSet");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.TextArea(
                areReferencesSet.boolValue ? "Initialized" : "Indicator isn't initialized properly!",
                new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 18, fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState { textColor = areReferencesSet.boolValue ? Color.green : Color.red },
                });
        }
    }
}