using UnityEditor;
using UnityEngine;

/// <summary>
/// Shows only the fields a condition's source actually uses, so the inspector stays readable.
/// </summary>
[CustomPropertyDrawer(typeof(StoryCondition))]
public class StoryConditionDrawer : PropertyDrawer
{
    private const float Gap = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight + Gap;
        int rows = 2; // Source + Comparison
        var source = (StoryConditionSource)property.FindPropertyRelative("Source").enumValueIndex;
        switch (source)
        {
            case StoryConditionSource.Resource:
                rows += 4; // advanced, resource, amount, scope
                if ((StoryResourceScope)property.FindPropertyRelative("Scope").enumValueIndex == StoryResourceScope.NamedPlanet)
                    rows += 1;
                break;
            case StoryConditionSource.YarnVariable:
                rows += 3; // variable, kind, value
                break;
            case StoryConditionSource.Time:
                rows += 2; // useDay, value
                break;
        }
        return rows * line;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        float line = EditorGUIUtility.singleLineHeight;
        float step = line + Gap;
        var rect = new Rect(position.x, position.y, position.width, line);

        var sourceProperty = property.FindPropertyRelative("Source");
        var comparisonProperty = property.FindPropertyRelative("Comparison");
        EditorGUI.PropertyField(rect, sourceProperty); rect.y += step;
        EditorGUI.PropertyField(rect, comparisonProperty); rect.y += step;

        switch ((StoryConditionSource)sourceProperty.enumValueIndex)
        {
            case StoryConditionSource.Resource:
                DrawResource(ref rect, property, step);
                break;
            case StoryConditionSource.YarnVariable:
                DrawYarn(ref rect, property, step);
                break;
            case StoryConditionSource.Time:
                DrawTime(ref rect, property, step);
                break;
        }

        EditorGUI.EndProperty();
    }

    private static void DrawResource(ref Rect rect, SerializedProperty property, float step)
    {
        var advanced = property.FindPropertyRelative("Advanced");
        EditorGUI.PropertyField(rect, advanced); rect.y += step;

        var resourceIndex = property.FindPropertyRelative("ResourceIndex");
        string[] names = advanced.boolValue
            ? System.Enum.GetNames(typeof(Registry.AdvancedResources))
            : System.Enum.GetNames(typeof(Registry.Resources));
        int safeIndex = Mathf.Clamp(resourceIndex.intValue, 0, Mathf.Max(0, names.Length - 1));
        resourceIndex.intValue = EditorGUI.Popup(rect, "Resource", safeIndex, names); rect.y += step;

        EditorGUI.PropertyField(rect, property.FindPropertyRelative("Amount")); rect.y += step;

        var scope = property.FindPropertyRelative("Scope");
        EditorGUI.PropertyField(rect, scope); rect.y += step;
        if ((StoryResourceScope)scope.enumValueIndex == StoryResourceScope.NamedPlanet)
        {
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("PlanetName"));
            rect.y += step;
        }
    }

    private static void DrawYarn(ref Rect rect, SerializedProperty property, float step)
    {
        EditorGUI.PropertyField(rect, property.FindPropertyRelative("Variable")); rect.y += step;
        var kind = property.FindPropertyRelative("ValueKind");
        EditorGUI.PropertyField(rect, kind); rect.y += step;
        switch ((StoryValueKind)kind.enumValueIndex)
        {
            case StoryValueKind.Number:
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("NumberValue"));
                break;
            case StoryValueKind.Bool:
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("BoolValue"));
                break;
            case StoryValueKind.String:
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("StringValue"));
                break;
        }
    }

    private static void DrawTime(ref Rect rect, SerializedProperty property, float step)
    {
        EditorGUI.PropertyField(rect, property.FindPropertyRelative("TimeUseDay")); rect.y += step;
        EditorGUI.PropertyField(rect, property.FindPropertyRelative("TimeValue"));
    }
}
