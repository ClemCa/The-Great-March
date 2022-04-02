using UnityEditor;
using UnityEngine;
using ClemCAddons;
using System.Collections.Generic;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor
{

	private const int stepsPerCurve = 10;
	private const float directionScale = 0.5f;
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	private static Color[] modeColors = {
		Color.white,
		Color.yellow,
		Color.cyan
	};

	private BezierSpline spline;
	private Transform handleTransform;
	private Quaternion handleRotation;
	private int selectedIndex = -1;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		spline = target as BezierSpline;
		if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
		{
			DrawSelectedPointInspector();
		}
		DrawAllCurvesInspector();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		if (GUILayout.Button("Add Curve"))
		{
			Undo.RecordObject(spline, "Add Curve");
			spline.AddCurve();
			EditorUtility.SetDirty(spline);
		}
		if (GUILayout.Button("Duplicate Last Curve"))
		{
			Undo.RecordObject(spline, "Duplicate Last Curve");
			spline.Duplicate();
			EditorUtility.SetDirty(spline);
		}
		if (GUILayout.Button("Remove Curve"))
		{
			Undo.RecordObject(spline, "Remove Curve");
			spline.RemoveCurve();
			EditorUtility.SetDirty(spline);
		}
		EditorGUI.BeginChangeCheck();
		bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(spline, "Toggle Loop");
			EditorUtility.SetDirty(spline);
			spline.Loop = loop;
		}
	}

	private void DrawSelectedPointInspector()
	{
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(spline, "Move Point");
			EditorUtility.SetDirty(spline);
			spline.SetControlPoint(selectedIndex, point);
		}
		EditorGUI.BeginChangeCheck();
		BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(spline, "Change Point Mode");
			spline.SetControlPointMode(selectedIndex, mode);
			EditorUtility.SetDirty(spline);
		}
		if (GUILayout.Button("Unselect"))
		{
			selectedIndex = -1;
		}
	}

	public bool fold = true;
	public KeyValuePair<bool, bool>[] folds = new KeyValuePair<bool, bool>[] { };

	private void DrawAllCurvesInspector()
	{
		var property = serializedObject.FindProperty("curves");
		fold = EditorGUILayout.Foldout(fold, "Curves", true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
		if (fold)
		{
			EditorGUI.indentLevel += 1;

			for (int i = 0; i < property.arraySize; i++)
			{
				folds = folds.CreateIfNotAt(new KeyValuePair<bool, bool>(true, false), i);
				folds[i] = new KeyValuePair<bool, bool>(EditorGUILayout.Foldout(folds[i].Key, "Curve " + i), folds[i].Value);
				if (folds[i].Key)
				{
					EditorGUI.indentLevel += 1;
					var points = property.GetArrayElementAtIndex(i).FindPropertyRelative("Points");
					folds[i] = new KeyValuePair<bool, bool>(folds[i].Key, EditorGUILayout.Foldout(folds[i].Value, "Points"));
					if (folds[i].Value)
					{
						EditorGUI.indentLevel += 1;
						for (int t = 0; t < points.arraySize; t++)
						{
							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField(points.GetArrayElementAtIndex(t), new GUIContent("Point " + t));
							if (EditorGUI.EndChangeCheck())
							{
								serializedObject.ApplyModifiedProperties();
								spline.RegisterChange();
							}
						}
						EditorGUI.indentLevel -= 1;
					}
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i).FindPropertyRelative("Frequency"));
					if (EditorGUI.EndChangeCheck())
					{
						serializedObject.ApplyModifiedProperties();
						spline.RegisterChange();
					}
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i).FindPropertyRelative("Spawn"));
					if (EditorGUI.EndChangeCheck())
					{
						serializedObject.ApplyModifiedProperties();
						spline.RegisterChange();
					}
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Duplicate"))
					{
						Undo.RecordObject(spline, "Duplicate");
						spline.Duplicate(i);
						EditorUtility.SetDirty(spline);
					}
					GUILayout.EndHorizontal();
					EditorGUI.indentLevel -= 1;
				}
			}
		}
	}

	private void OnSceneGUI()
	{
		spline = target as BezierSpline;
		handleTransform = spline.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;

		Vector3 p0 = ShowPoint(0);
		for (int i = 1; i < spline.ControlPointCount; i += 3)
		{
			Vector3 p1 = ShowPoint(i);
			Vector3 p2 = ShowPoint(i + 1);
			Vector3 p3 = ShowPoint(i + 2);

			Handles.color = Color.gray;
			Handles.DrawLine(p0, p1);
			Handles.DrawLine(p2, p3);

			Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
			p0 = p3;
		}
		ShowDirections();
	}

	private void ShowDirections()
	{
		Handles.color = Color.green;
		Vector3 point = spline.GetPoint(0f);
		Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);
		int steps = stepsPerCurve * spline.CurveCount;
		for (int i = 1; i <= steps; i++)
		{
			point = spline.GetPoint(i / (float)steps);
			Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
		}
	}

	private Vector3 ShowPoint(int index)
	{
		Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0)
		{
			size *= 2f;
		}
		Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
		{
			selectedIndex = index;
			Repaint();
		}
		if (selectedIndex == index)
		{
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(spline, "Move Point");
				EditorUtility.SetDirty(spline);
				spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}
}