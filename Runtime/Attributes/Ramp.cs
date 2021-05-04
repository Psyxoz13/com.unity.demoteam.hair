using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.DemoTeam.Attributes
{
	public static class HairGUI
	{
		const float kIndentPerLevel = 15.0f;
		const float kSpacing = 5.0f;

		static readonly int s_idHintRampMin = "RampMin".GetHashCode();
		static readonly int s_idHintRampMax = "RampMax".GetHashCode();

		static Vector3[] s_pointsLinear = new Vector3[4];
		static Vector3[] s_pointsSmooth = new Vector3[16];

		public enum RampStyle
		{
			LinearDecreasing,
			LinearIncreasing,
			SmoothDecreasing,
			SmoothIncreasing,
		}

#if UNITY_EDITOR
		public static Vector2 Ramp(Rect position, Vector2 value, Vector2 limit, RampStyle style = RampStyle.LinearDecreasing)
		{
			GUI.Box(position, GUIContent.none);

			position.xMin += 1.0f;
			position.xMax -= 1.0f;
			position.yMin += 1.0f;
			position.yMax -= 1.0f;

			// sanitize
			if (value.x > value.y)
			{
				value.x = 0.5f * (value.x + value.y);
				value.y = value.x;
			}

			// draw sliders
			var tmin = Mathf.Clamp01((value.x - limit.x) / (limit.y - limit.x));
			var tmax = Mathf.Clamp01((value.y - limit.x) / (limit.y - limit.x));
			{
				var idHot = GUIUtility.hotControl;
				var idMin = GUIUtility.GetControlID(s_idHintRampMin, FocusType.Passive);
				var idMax = GUIUtility.GetControlID(s_idHintRampMax, FocusType.Passive);

				var id = (idHot == idMin || idHot == idMax) ? idHot : 0;

				if (Event.current.type == EventType.MouseDown)
				{
					var xmin = position.xMin + position.width * tmin;
					var xmax = position.xMin + position.width * tmax;

					if (Event.current.mousePosition.x < 0.5f * (xmin + xmax))
						id = idMin;
					else
						id = idMax;
				}

				if (id == idMin)
				{
					value.x = GUI.Slider(position, (float)value.x, 0.0f, limit.x, limit.y, GUIStyle.none, GUIStyle.none, horiz: true, idMin);
					value.y = Mathf.Max((float)value.x, (float)value.y);
				}

				if (id == idMax)
				{
					value.y = GUI.Slider(position, (float)value.y, 0.0f, limit.x, limit.y, GUIStyle.none, GUIStyle.none, horiz: true, idMax);
					value.x = Mathf.Min((float)value.x, (float)value.y);
				}

				tmin = Mathf.Clamp01((value.x - limit.x) / (limit.y - limit.x));
				tmax = Mathf.Clamp01((value.y - limit.x) / (limit.y - limit.x));
			}

			// draw ramp
			using (new Handles.DrawingScope(Color.green))
			{
				var x0 = position.xMin;
				var x1 = position.xMax;
				var y0 = position.yMax;
				var y1 = position.yMin;

				var styleIncreasing = (style == RampStyle.LinearIncreasing || style == RampStyle.SmoothIncreasing);
				if (styleIncreasing)
				{
					y0 = position.yMin;
					y1 = position.yMax;
				}

				s_pointsLinear[0] = new Vector3(x0, y1);
				s_pointsLinear[1] = new Vector3(x0 + tmin * (x1 - x0), y1);
				s_pointsLinear[2] = new Vector3(x0 + tmax * (x1 - x0), y0);
				s_pointsLinear[3] = new Vector3(x1, y0);

				var styleSmooth = (style == RampStyle.SmoothDecreasing || style == RampStyle.SmoothIncreasing);
				if (styleSmooth)
				{
					s_pointsSmooth[0] = s_pointsLinear[0];
					s_pointsSmooth[1] = s_pointsLinear[1];

					var p0 = s_pointsLinear[1];
					var p1 = s_pointsLinear[2];

					var dx = p1.x - p0.x;
					var dy = p1.y - p0.y;
					var dt = 1.0f / (s_pointsSmooth.Length - 3);

					for (int i = 2; i != s_pointsSmooth.Length - 2; i++)
					{
						var t = (i - 1) * dt;
						var s = Mathf.SmoothStep(0.0f, 1.0f, t);

						s_pointsSmooth[i] = new Vector3(p0.x + dx * t, p0.y + dy * s);
					}

					s_pointsSmooth[s_pointsSmooth.Length - 2] = s_pointsLinear[2];
					s_pointsSmooth[s_pointsSmooth.Length - 1] = s_pointsLinear[3];
				}

				Handles.DrawPolyLine(styleSmooth ? s_pointsSmooth : s_pointsLinear);
			}

			// draw markers
			{
				var markerCenter = Matrix4x4.Translate(new Vector3(-11.0f, 0.0f, 0.0f));
				var markerRotate = s_pointsLinear[0].y < s_pointsLinear[3].y ? 90.0f : -90.0f;

				var matrix = GUI.matrix;
				GUI.matrix = Matrix4x4.TRS(s_pointsLinear[1], Quaternion.Euler(0.0f, 0.0f, -markerRotate), 0.8f * Vector3.one) * markerCenter;
				GUI.Label(Rect.zero, GUIContent.none, EditorStyles.foldout);
				GUI.matrix = Matrix4x4.TRS(s_pointsLinear[2], Quaternion.Euler(0.0f, 0.0f, markerRotate), 0.8f * Vector3.one) * markerCenter;
				GUI.Label(Rect.zero, GUIContent.none, EditorStyles.foldout);
				GUI.matrix = matrix;
			}

			// done
			return value;
		}
#endif
	}

	public class RampAttribute : PropertyAttribute
	{
		public float min;
		public float max;
		public HairGUI.RampStyle style;

		public RampAttribute(float min, float max, HairGUI.RampStyle style = HairGUI.RampStyle.LinearDecreasing)
		{
			this.min = min;
			this.max = max;
			this.style = HairGUI.RampStyle.LinearDecreasing;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(RampAttribute))]
	public class RampAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Vector2)
			{
				var ramp = base.attribute as RampAttribute;

				EditorGUI.BeginProperty(position, label, property);
				position = EditorGUI.PrefixLabel(position, label);

				//NOTE: EditorGUIUtility.fieldWidth
				property.vector2Value = HairGUI.Ramp(position, property.vector2Value, new Vector2(ramp.min, ramp.max), ramp.style);

				EditorGUI.EndProperty();
			}
			else
			{
				base.OnGUI(position, property, label);
			}
		}
	}
#endif
}
