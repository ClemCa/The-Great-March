using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ClemCAddons;
using System.Linq;

public class ChunkEntrance : MonoBehaviour
{
	public int selectedScene = -1;
	public ELevelType levelType;

    void OnDrawGizmosSelected()
    {
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.up + Vector3.right + Vector3.forward, transform.lossyScale), Vector3.one * 0.5f);
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.up+Vector3.left + Vector3.forward, transform.lossyScale), Vector3.one * 0.5f);
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.up + Vector3.right + Vector3.back, transform.lossyScale), Vector3.one * 0.5f);
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.up + Vector3.left + Vector3.back, transform.lossyScale), Vector3.one * 0.5f);
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.down + Vector3.right + Vector3.forward, transform.lossyScale), Vector3.one * 0.5f);
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.down + Vector3.left + Vector3.forward, transform.lossyScale), Vector3.one * 0.5f);
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.down + Vector3.right + Vector3.back, transform.lossyScale), Vector3.one * 0.5f);
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.down + Vector3.left + Vector3.back, transform.lossyScale), Vector3.one * 0.5f);

		Gizmos.DrawCube(transform.position.GetCorner(Vector3.up + Vector3.right, transform.lossyScale), (Vector3.one * 0.2f).SetZ(transform.lossyScale.z));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.up + Vector3.left, transform.lossyScale), (Vector3.one * 0.2f).SetZ(transform.lossyScale.z));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.down + Vector3.right, transform.lossyScale), (Vector3.one * 0.2f).SetZ(transform.lossyScale.z));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.down + Vector3.left, transform.lossyScale), (Vector3.one * 0.2f).SetZ(transform.lossyScale.z));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.up + Vector3.back, transform.lossyScale), (Vector3.one * 0.2f).SetX(transform.lossyScale.x));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.up + Vector3.forward, transform.lossyScale), (Vector3.one * 0.2f).SetX(transform.lossyScale.x));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.down + Vector3.back, transform.lossyScale), (Vector3.one * 0.2f).SetX(transform.lossyScale.x));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.down + Vector3.forward, transform.lossyScale), (Vector3.one * 0.2f).SetX(transform.lossyScale.x));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.left + Vector3.forward, transform.lossyScale), (Vector3.one * 0.2f).SetY(transform.lossyScale.y));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.left + Vector3.back, transform.lossyScale), (Vector3.one * 0.2f).SetY(transform.lossyScale.y));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.right + Vector3.forward, transform.lossyScale), (Vector3.one * 0.2f).SetY(transform.lossyScale.y));
		Gizmos.DrawCube(transform.position.GetCorner(Vector3.right + Vector3.back, transform.lossyScale), (Vector3.one * 0.2f).SetY(transform.lossyScale.y));


	}

	void OnDrawGizmos()
    {
		Gizmos.DrawWireCube(transform.position, transform.lossyScale);
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(ChunkEntrance))]
public class ChunkEntranceInspector : Editor
{
	public override void OnInspectorGUI()
	{
		var chunk = (target as ChunkEntrance);
		var sceneLoader = FindObjectOfType<SceneLoader>();
		if (sceneLoader == null)
		{
			Debug.LogError("No Scene Loader. ChunkEntrance and SceneLoader should be in the same Scene");
			return;
		}
		var scenes = EditorBuildSettings.scenes;
		var scenesNames = new string[] { };
		GUIContent[] content = new GUIContent[] { };
		foreach (var scene in scenes)
		{
			content = content.Add(new GUIContent(scene.path.Split('/').Last().Replace(".unity", "").Replace("_", " ")));
			scenesNames = scenesNames.Add(scene.path.Split('/').Last().Replace(".unity", ""));
		}
		var self = sceneLoader.ScenesAreas.Areas.FindIndex(chunk.transform);
		if (self == -1)
		{
			chunk.selectedScene = -1;
		}
		else
		{
			chunk.selectedScene = scenesNames.FindIndex(sceneLoader.ScenesAreas.Names[self]);
		}
		var r = GUILayout.SelectionGrid(chunk.selectedScene, content, 2);


		if (r != chunk.selectedScene)
		{
			Undo.RegisterCompleteObjectUndo(sceneLoader, "Updated chunks");
			if (self == -1)
			{
				sceneLoader.ScenesAreas.Names = sceneLoader.ScenesAreas.Names.Add(scenesNames[r]);
				sceneLoader.ScenesAreas.Areas = sceneLoader.ScenesAreas.Areas.Add(chunk.transform);
				sceneLoader.ScenesAreas.LevelTypes = sceneLoader.ScenesAreas.LevelTypes.Add(chunk.levelType);
			}
			else
			{
				sceneLoader.ScenesAreas.Names[self] = scenesNames[r];
				sceneLoader.ScenesAreas.Areas[self] = chunk.transform;
				sceneLoader.ScenesAreas.LevelTypes[self] = chunk.levelType;
			}
			EditorUtility.SetDirty(sceneLoader);
			chunk.selectedScene = r;
		}

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("levelType"));
		serializedObject.ApplyModifiedProperties();
	}
}
#endif