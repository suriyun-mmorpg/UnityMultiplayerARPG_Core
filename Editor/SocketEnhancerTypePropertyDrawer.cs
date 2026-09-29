using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    [CustomPropertyDrawer(typeof(SocketEnhancerType))]
    public class SocketEnhancerTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            List<string> enumNames = new List<string>(property.enumNames);
            for (int i = 0; i < EditorGlobalData.SocketEnhancerTypeTitles.Length; ++i)
                enumNames[i] = EditorGlobalData.SocketEnhancerTypeTitles[i];

            EditorGUI.BeginProperty(position, label, property);
            bool previousShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            try
            {
                EditorGUI.BeginChangeCheck();
                int selectedIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, enumNames.ToArray());
                if (EditorGUI.EndChangeCheck())
                    property.enumValueIndex = selectedIndex;
            }
            finally
            {
                EditorGUI.showMixedValue = previousShowMixedValue;
                EditorGUI.EndProperty();
            }
        }
    }
}
