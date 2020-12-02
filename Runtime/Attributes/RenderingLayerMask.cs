using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Unity.DemoTeam.Attributes
{
	public class RenderingLayerMaskAttribute : PropertyAttribute { }

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(RenderingLayerMaskAttribute))]
	public class RenderingLayerMaskAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => -EditorGUIUtility.standardVerticalSpacing;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var pipelineAsset = GraphicsSettings.renderPipelineAsset;
			if (pipelineAsset == null)
				return;

			var layerMaskNames = pipelineAsset.renderingLayerMaskNames;
			if (layerMaskNames == null)
				return;

			property.intValue = EditorGUILayout.MaskField(label, property.intValue, layerMaskNames);
		}
	}
#endif
}
