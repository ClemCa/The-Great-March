//credits to http://answers.unity.com/answers/1383657/view.html

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class LabelOverride : PropertyAttribute
{
    public string label;
    public float? rangeMin;
    public float? rangeMax;
    public LabelOverride(string label)
    {
        this.label = label;
        rangeMin = null;
        rangeMax = null;
    }
    public LabelOverride(string label, float rangeMin, float rangeMax)
    {
        this.label = label;
        this.rangeMin = rangeMin;
        this.rangeMax = rangeMax;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LabelOverride))]
    public class ThisPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                var propertyAttribute = this.attribute as LabelOverride;
                if (IsItBloodyArrayTho(property) == false)
                {
                    label.text = propertyAttribute.label;

                }
                else
                {
                    Debug.LogWarningFormat(
                        "{0}(\"{1}\") doesn't support arrays ",
                        typeof(LabelOverride).Name,
                        propertyAttribute.label
                    );
                }
                if (propertyAttribute.rangeMin.HasValue)
                {
                    if (property.propertyType == SerializedPropertyType.Float)
                        EditorGUI.Slider(position, property, propertyAttribute.rangeMin.Value, propertyAttribute.rangeMax.Value, label);

                    else if(property.propertyType == SerializedPropertyType.Integer)
                        EditorGUI.IntSlider(position, property, (int)propertyAttribute.rangeMin.Value, (int)propertyAttribute.rangeMax.Value, label);
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label);
                }
                
            }
            catch (System.Exception ex) { Debug.LogException(ex); }
        }
        bool IsItBloodyArrayTho(SerializedProperty property)
        {
            string path = property.propertyPath;
            int idot = path.IndexOf('.');
            if (idot == -1) return false;
            string propName = path.Substring(0, idot);
            SerializedProperty p = property.serializedObject.FindProperty(propName);
            return p.isArray;
            //CREDITS: https://answers.unity.com/questions/603882/serializedproperty-isnt-being-detected-as-an-array.html
        }
    }
#endif
}