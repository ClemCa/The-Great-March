using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineDecorator))]
public class SplineDecoratorInspector : Editor
{
	private SplineDecorator decorator;

	public override void OnInspectorGUI()
	{
		decorator = target as SplineDecorator;
		var sd = new SerializedObject(decorator);
		GUILayout.Label("Spawn", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(sd.FindProperty("StepSize"), new GUIContent("Global Frequency"));
		sd.ApplyModifiedProperties();
		EditorGUI.BeginChangeCheck();
		bool forward = EditorGUILayout.Toggle("Look Forward", decorator.lookForward);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(decorator, "Look Forward");
			EditorUtility.SetDirty(decorator);
			decorator.lookForward = forward;
		}
		EditorGUILayout.PropertyField(sd.FindProperty("items"), new GUIContent("Items"));
		sd.ApplyModifiedProperties();
		EditorGUI.BeginChangeCheck();
		bool spawn = EditorGUILayout.Toggle("On Play", decorator.spawnOnPlay);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(decorator, "On Play");
			EditorUtility.SetDirty(decorator);
			decorator.spawnOnPlay = spawn;
		}
		EditorGUI.BeginChangeCheck();
		bool spawnn = EditorGUILayout.Toggle("Only Selected Curves", decorator.spawnSelectedOnly);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(decorator, "Only Selected Curves");
			EditorUtility.SetDirty(decorator);
			decorator.spawnSelectedOnly = spawnn;
		}
		EditorGUI.BeginChangeCheck();
		bool autoapply = EditorGUILayout.Toggle("Auto Apply Changes", decorator.autoApplyChanges);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(decorator, "Auto Apply Changes");
			EditorUtility.SetDirty(decorator);
			decorator.autoApplyChanges = autoapply;
		}
		if (EditorUtility.IsDirty(decorator))
		{
			decorator.RegisterChange();
		}
		if (GUILayout.Button("Spawn Now"))
		{
			decorator.SpawnNow();
			EditorUtility.SetDirty(decorator);
		}
		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("Clear"))
		{
			decorator.Clear();
			EditorUtility.SetDirty(decorator);
		}
	}
}