using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Right-hand properties panel. Draws the selected node through a SerializedObject so the nested
/// condition / content lists get proper list editing for free.
/// </summary>
public class StoryGraphInspector : VisualElement
{
    public event Action Changed;
    public event Action<StoryNode> RequestDelete;

    private StoryGraph _graph;
    private SerializedObject _serialized;
    private IMGUIContainer _container;
    private StoryNode _node;
    private int _nodeIndex = -1;
    private Vector2 _scroll;

    public StoryGraphInspector()
    {
        style.width = 330;
        style.minWidth = 280;
        style.borderLeftWidth = 1;
        style.borderLeftColor = new Color(0f, 0f, 0f, 0.4f);
        style.paddingLeft = 4;
        style.paddingRight = 4;
        _container = new IMGUIContainer(OnGUI);
        _container.style.flexGrow = 1;
        Add(_container);
    }

    public void SetGraph(StoryGraph graph)
    {
        _graph = graph;
        _serialized = graph != null ? new SerializedObject(graph) : null;
        _node = null;
        _nodeIndex = -1;
    }

    public void SetNode(StoryNode node)
    {
        _node = node;
        _nodeIndex = (node == null || _graph == null) ? -1 : _graph.Nodes.IndexOf(node);
    }

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        if (_serialized == null || _graph == null)
        {
            EditorGUILayout.HelpBox("No story graph selected.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        _serialized.Update();

        var nodesProperty = _serialized.FindProperty("Nodes");
        if (_node == null || _nodeIndex < 0 || _nodeIndex >= nodesProperty.arraySize)
        {
            EditorGUILayout.HelpBox("Select a node on the graph to edit it.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        var node = nodesProperty.GetArrayElementAtIndex(_nodeIndex);
        var typeProperty = node.FindPropertyRelative("Type");
        var hasDateProperty = node.FindPropertyRelative("HasDate");

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField(_node.Type == StoryNodeType.Decision ? "Decision" : "Beat", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(node.FindPropertyRelative("Name"));
        EditorGUILayout.PropertyField(typeProperty);
        EditorGUILayout.PropertyField(node.FindPropertyRelative("Weight"), new GUIContent("Weight", "Relative chance among eligible siblings. 0 disables this node."));

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Date", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(hasDateProperty, new GUIContent("Delay Until Date"));
        if (hasDateProperty.boolValue)
        {
            EditorGUILayout.PropertyField(node.FindPropertyRelative("Day"));
            EditorGUILayout.PropertyField(node.FindPropertyRelative("MinuteOfDay"));
            EditorGUILayout.HelpBox("Lower bound only: the node waits until this moment, then is treated as retroactively satisfied. It never fires on its own.", MessageType.None);
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Conditions (all must pass)", EditorStyles.boldLabel);
        var conditions = node.FindPropertyRelative("Conditions");
        for (int i = 0; i < conditions.arraySize; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Clause " + (i + 1), EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                conditions.DeleteArrayElementAtIndex(i);
                i--;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(conditions.GetArrayElementAtIndex(i), GUIContent.none, true);
            EditorGUILayout.EndVertical();
        }
        if (GUILayout.Button("Add Condition"))
        {
            conditions.InsertArrayElementAtIndex(conditions.arraySize);
            ResetCondition(conditions.GetArrayElementAtIndex(conditions.arraySize - 1));
        }

        if (_node.Type == StoryNodeType.Beat)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Beat Content", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(node.FindPropertyRelative("YarnNodes"), new GUIContent("Yarn Nodes"), true);
            EditorGUILayout.PropertyField(node.FindPropertyRelative("Appearances"), true);
            EditorGUILayout.PropertyField(node.FindPropertyRelative("Triggers"), true);
        }

        _serialized.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
            Changed?.Invoke();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Delete Node"))
            RequestDelete?.Invoke(_node);

        EditorGUILayout.EndScrollView();
    }

    private static void ResetCondition(SerializedProperty condition)
    {
        condition.FindPropertyRelative("Source").enumValueIndex = 0;
        condition.FindPropertyRelative("Comparison").enumValueIndex = (int)StoryComparison.GreaterOrEqual;
        condition.FindPropertyRelative("Advanced").boolValue = false;
        condition.FindPropertyRelative("ResourceIndex").intValue = 0;
        condition.FindPropertyRelative("Amount").intValue = 0;
        condition.FindPropertyRelative("Scope").enumValueIndex = 0;
        condition.FindPropertyRelative("PlanetName").stringValue = "";
        condition.FindPropertyRelative("Variable").stringValue = "";
        condition.FindPropertyRelative("ValueKind").enumValueIndex = 0;
        condition.FindPropertyRelative("NumberValue").floatValue = 0f;
        condition.FindPropertyRelative("BoolValue").boolValue = false;
        condition.FindPropertyRelative("StringValue").stringValue = "";
        condition.FindPropertyRelative("TimeUseDay").boolValue = true;
        condition.FindPropertyRelative("TimeValue").longValue = 0L;
    }
}
