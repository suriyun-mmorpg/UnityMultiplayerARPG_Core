using UnityEngine;
using UnityEditor;

namespace MultiplayerARPG
{
    [CustomPropertyDrawer(typeof(SocketEnhancerTypeTitleAttribute))]
    public class SocketEnhancerTypeTitlePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			string title = label.text;
			SocketEnhancerTypeTitleAttribute indexProp = attribute as SocketEnhancerTypeTitleAttribute;
			int index = indexProp.index;
			if (index >= 0 && index < EditorGlobalData.SocketEnhancerTypeTitles.Length)
				title = EditorGlobalData.SocketEnhancerTypeTitles[index];
			Debug.LogError("here " + title);
			EditorGUI.PropertyField(position, property, new GUIContent(title));
		}
	}
}