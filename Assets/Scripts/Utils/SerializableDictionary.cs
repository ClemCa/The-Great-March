using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

#nullable enable

[System.Serializable]
public class SerializableDictionarySubarray<TValue> where TValue : class
{
    [SerializeField] public TValue[] array;
    public SerializableDictionarySubarray() { array = System.Array.Empty<TValue>(); }
    public SerializableDictionarySubarray(TValue[] arr) { array = arr; }
}

public class SerializableDictionaryHelper
{
    public static bool IsSubArray(System.Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SerializableDictionarySubarray<>);
    }
}

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IDictionary<TKey, TValue>, IDictionary // implementing the generic version for the custom inspector
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();
    private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
    public event System.Action OnValueChanged = new System.Action(() => { });
    private Dictionary<TKey, TValue> Dictionary
    {
        get
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<TKey, TValue>();
            }
            return dictionary;
        }
    }
    public ICollection<TKey> Keys => Dictionary.Keys;

    public ICollection<TValue> Values => Dictionary.Values;

    public int Count => Dictionary.Count;

    public bool IsReadOnly => false;

    public bool IsFixedSize => false;

    ICollection IDictionary.Keys => Dictionary.Keys.Cast<object>().ToList();

    ICollection IDictionary.Values => Dictionary.Values.Cast<object>().ToList();

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    public object? this[object key] { get => Dictionary[(TKey)key]; set { Dictionary[(TKey)key] = (TValue)value!; OnValueChanged?.Invoke(); } }
    public TValue this[TKey key] { get => Dictionary[key]; set { Dictionary[key] = value; OnValueChanged?.Invoke(); } }
    public SerializableDictionary() { }
    public SerializableDictionary(Dictionary<TKey, TValue> dict)
    {
        foreach (var kvp in dict)
        {
            Dictionary[kvp.Key] = kvp.Value;
        }
    }
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var kvp in Dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
        if (typeof(TKey) == typeof(int))
        {
            List<TKey> newKeys = keys.OrderBy(k => k is null ? 0 : (int)(object)k).ToList();
            values = newKeys.Select((k) =>
            {
                int i = keys.IndexOf(k);
                return values[i];
            }).ToList();
            keys = newKeys;
        }
    }

    public void OnAfterDeserialize()
    {
        dictionary = new Dictionary<TKey, TValue>();
        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
        {
            dictionary[keys[i]] = values[i];
        }
    }

    public void Add(TKey key, TValue value)
    {
        Dictionary[key] = value;
    }

    public bool ContainsKey(TKey key)
    {
        return Dictionary.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        return Dictionary.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return Dictionary.TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Dictionary.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        Dictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return Dictionary.ContainsKey(item.Key) && EqualityComparer<TValue>.Default.Equals(Dictionary[item.Key], item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach (var kvp in Dictionary)
        {
            array[arrayIndex++] = kvp;
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Dictionary.Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var kvp in Dictionary)
        {
            yield return kvp;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(object key, object value)
    {
        Dictionary[(TKey)key] = (TValue)value;
    }

    public bool Contains(object key)
    {
        return Dictionary.ContainsKey((TKey)key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return Dictionary.GetEnumerator();
    }

    public void Remove(object key)
    {
        Dictionary.Remove((TKey)key);
    }

    public void CopyTo(System.Array array, int index)
    {
        foreach (var kvp in Dictionary)
        {
            array.SetValue(kvp, index++);
        }
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
public class SerializableDictionaryPropertyDrawer : PropertyDrawer
{
    bool isExpanded = true;
    object? newKey;
    object? newValue;
    object? renameKey;
    string[]? previousKeys;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight; // label
        if (isExpanded)
        {
            IDictionary? dict;
            if (property.serializedObject.targetObject.GetType().GetField(fieldInfo.Name) == null)
            {
                dict = property.boxedValue as IDictionary;
            }
            else
            {
                dict = fieldInfo.GetValue(property.serializedObject.targetObject) as IDictionary;
            }
            if (dict != null)
            {
                foreach (var key in dict.Keys)
                {
                    var value = dict[key];
                    if (value != null && SerializableDictionaryHelper.IsSubArray(value.GetType()))
                    {
                        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // size
                        if (value != null)
                        {
                            var arrayField = value.GetType().GetField("array");
                            var arr = arrayField?.GetValue(value) as System.Array;
                            if (arr != null)
                            {
                                foreach (var item in arr)
                                {
                                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                                }
                            }
                        }
                    }
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4; // buttons, fields
        }
        // for array
        if (newValue != null && SerializableDictionaryHelper.IsSubArray(newValue.GetType()))
        {
            var arrayField = newValue.GetType().GetField("array");
            var arr = arrayField?.GetValue(newValue) as System.Array;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // size field
            if (arr != null)
            {
                height += arr.Length * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }
        }
        return height;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.boxedValue == null)
        {
            property.boxedValue = System.Activator.CreateInstance(fieldInfo.FieldType);
            property.serializedObject.ApplyModifiedProperties();
        }
        IDictionary? dict = property.boxedValue as IDictionary;
        EditorGUI.BeginProperty(position, label, property);
        position.height = EditorGUIUtility.singleLineHeight;
        isExpanded = EditorGUI.Foldout(position, isExpanded, label);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        System.Type TKey = fieldInfo.FieldType.GetGenericArguments()[0];
        System.Type TValue = fieldInfo.FieldType.GetGenericArguments()[1];
        if (isExpanded)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            if (dict != null)
            {
                foreach (var key in dict.Keys)
                {
                    dict[key] = DrawField(position, key.ToString(), dict[key], TValue, out position);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.boxedValue = dict;
            }
            EditorGUI.indentLevel--;
            newKey ??= DefaultKey(TKey);
            newValue ??= DefaultKey(TValue);
            renameKey ??= DefaultKey(TKey);
            position.width /= 3;
            if (GUI.Button(position, "New", EditorStyles.miniButtonLeft) && dict != null)
            {
                if (dict.Contains(newKey))
                {
                    Debug.LogWarning("Key already exists in dictionary: " + newKey);
                    return;
                }
                dict.Add(newKey, newValue);
                property.boxedValue = dict;
                Debug.Log("Added new entry to dictionary: " + newKey + " -> " + newValue);
            }
            position.x += position.width;
            if (GUI.Button(position, "Rename", EditorStyles.miniButtonMid) && dict != null)
            {
                if (!dict.Contains(renameKey))
                {
                    Debug.LogWarning("Key does not exist in dictionary: " + renameKey);
                    return;
                }
                var value = dict[renameKey];
                dict.Remove(renameKey);
                dict.Add(newKey, value);
                property.boxedValue = dict;
                Debug.Log("Renamed entry in dictionary: " + renameKey + " to " + newKey);
            }
            position.x += position.width;
            if (GUI.Button(position, "Delete", EditorStyles.miniButtonRight) && dict != null)
            {
                if (dict.Contains(newKey))
                {
                    dict.Remove(newKey);
                    property.boxedValue = dict;
                }
            }
            position.x -= position.width * 2;
            position.width *= 2;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            newKey = DrawField(position, "New Key", newKey, TKey, out position);
            newValue = DrawField(position, "New Value", newValue, TValue, out position);
            renameKey = DrawField(position, "Old Key", renameKey, TKey, out _);
        }
        EditorGUI.EndProperty();
    }

    private object? DefaultKey(System.Type type)
    {
        return type switch
        {
            _ when type == typeof(string) => "New Key",
            _ when type == typeof(int) => 0,
            _ when type == typeof(float) => 0f,
            _ when type == typeof(double) => 0.0,
            _ when type == typeof(bool) => false,
            _ when type.IsEnum => System.Enum.GetValues(type).GetValue(0),
            _ when type == typeof(Vector2) => Vector2.zero,
            _ when type == typeof(Vector3) => Vector3.zero,
            _ when type == typeof(Vector4) => Vector4.zero,
            _ when type == typeof(Color) => Color.white,
            _ when type == typeof(Quaternion) => Quaternion.identity,
            _ when type == typeof(Object) => null,
            _ when typeof(Object).IsAssignableFrom(type) => null,
            _ when type.IsInstanceOfType(typeof(Object)) => null,
            _ when SerializableDictionaryHelper.IsSubArray(type) => System.Activator.CreateInstance(type),
            _ => throw new System.ArgumentException("Cannot create default value for type " + type)
        };
    }

    private object? DrawField(Rect position, string label, object? entry, System.Type type, out Rect newPosition)
    {
        newPosition = position;
        newPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        object? DrawUnsupported()
        {
            EditorGUI.LabelField(position, label, "Unsupported Type: " + (entry?.GetType() ?? typeof(object)));
            return null;
        }
        return type switch
        {
            _ when type == typeof(string) => EditorGUI.TextField(position, label, entry as string ?? string.Empty),
            _ when type == typeof(int) => EditorGUI.IntField(position, label, entry is int v ? v : 0),
            _ when type == typeof(float) => EditorGUI.FloatField(position, label, entry is float v ? v : 0f),
            _ when type == typeof(double) => EditorGUI.DoubleField(position, label, entry is double v ? v : 0.0),
            _ when type == typeof(bool) => EditorGUI.Toggle(position, label, entry is bool v && v),
            _ when type.IsEnum => EditorGUI.EnumPopup(position, label, entry as System.Enum ?? (System.Enum)System.Enum.GetValues(type).GetValue(0)!),
            _ when type == typeof(Vector2) => EditorGUI.Vector2Field(position, label, entry is Vector2 v ? v : Vector2.zero),
            _ when type == typeof(Vector3) => EditorGUI.Vector3Field(position, label, entry is Vector3 v ? v : Vector3.zero),
            _ when type == typeof(Vector4) => EditorGUI.Vector4Field(position, label, entry is Vector4 v ? v : Vector4.zero),
            _ when type == typeof(Color) => EditorGUI.ColorField(position, label, entry is Color v ? v : Color.white),
            _ when type == typeof(Quaternion) => EditorGUI.Vector4Field(position, label, (entry is Quaternion v ? v : Quaternion.identity).eulerAngles),
            _ when type == typeof(Transform) => EditorGUI.ObjectField(position, label, entry as Transform, typeof(Transform), true),
            _ when type == typeof(GameObject) => EditorGUI.ObjectField(position, label, entry as GameObject, typeof(GameObject), true),
            _ when type == typeof(Object) => EditorGUI.ObjectField(position, label, entry as Object, typeof(Object), true),
            _ when typeof(Object).IsAssignableFrom(type) => EditorGUI.ObjectField(position, label, entry as Object, type, true),
            _ when SerializableDictionaryHelper.IsSubArray(type) => DrawArrayField(position, label, entry, type, out newPosition),
            _ when type == typeof(object) => DrawUnsupported(),
            _ => throw new System.ArgumentException("Unknown type " + (entry?.GetType() ?? typeof(object)) + ": " + entry + " (null: " + (entry == null) + ")")
        };
    }

    private object? DrawArrayField(Rect position, string label, object? entry, System.Type type, out Rect newPosition)
    {
        System.Type elementType = type.GetGenericArguments()[0];
        var list = System.Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
        if (list == null)
        {
            newPosition = position;
            return entry;
        }
        if (entry != null)
        {
            var arrayField = type.GetField("array");
            System.Array? arr = arrayField?.GetValue(entry) as System.Array;
            if (arr != null)
            {
                foreach (var item in arr)
                {
                    list.Add(item);
                }
            }
        }
        int size = list.Count;
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(position, label);
        position.x += 15;
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        size = Mathf.Max(0, EditorGUI.IntField(position, "Size", size));
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        while (list.Count < size)
        {
            list.Add(DefaultKey(elementType));
        }
        while (list.Count > size)
        {
            list.RemoveAt(list.Count - 1);
        }
        for (int i = 0; i < list.Count; i++)
        {
            list[i] = DrawField(position, label + " Element " + i, list[i], elementType, out position);
        }
        position.x -= 15;
        System.Array array = System.Array.CreateInstance(elementType, list.Count);
        list.CopyTo(array, 0);
        newPosition = position;
        return System.Activator.CreateInstance(type, new object[] { array });
    }
}
#endif