using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using ClemCAddons;
using System.Linq;

#if(UNITY_EDITOR)
public class SerializationChecker
{
    private const string _path = "Utilities/ClemCAddons/IgnoredFields.CAddons";
    private static List<string> _exceptions = null;

    public static List<string> Exceptions
    {
        get
        {
            if(_exceptions == null)
            {
                var r = ReadFile(_path);
                if (r != null)
                    _exceptions = r.ToType(typeof(List<string>));
                else
                    _exceptions = new List<string>();
            }
            return _exceptions;
        }
        set
        {
            _exceptions = value;
            WriteToFile(_path, _exceptions.ToBytes());
        }
    }

    // HUGELY inefficient (x^y^z...), but only runs once on intentional click, should be fine.

    [MenuItem("GameObject/ClemCAddons/\u2713 Validation/\u2713 Check Serialized Fields", false,0)]
    public static void CheckSerializedFields(MenuCommand menuCommand)
    {
        _exceptions = null;
        Scene scene;
        if (Selection.activeGameObject != null)
            scene = Selection.activeGameObject.scene;
        else
            scene = SceneManager.GetActiveScene();

        foreach (GameObject child in scene.GetRootGameObjects())
        {
            CheckGameObject(child);
            CheckChildren(child.transform);
        }
    }
    [MenuItem("GameObject/ClemCAddons/\u2713 Validation/Exclude from validation", false, 0)]
    public static void ExcludeValidation(MenuCommand menuCommand)
    {
        var selection = Selection.gameObjects;
        var t = Exceptions;
        foreach (GameObject go in selection)
        {
            ExcludeGameObject(go, ref t);
        }
        Exceptions = t;
    }

    [MenuItem("GameObject/ClemCAddons/\u2713 Validation/Include it back!", false, 0)]
    public static void IncludeValidation(MenuCommand menuCommand)
    {
        var selection = Selection.gameObjects;
        var t = Exceptions;
        foreach (GameObject go in selection)
        {
            IncludeGameObject(go, ref t);
        }
        Exceptions = t;
    }
    [MenuItem("GameObject/ClemCAddons/\u2713 Validation/Clear Exceptions", false, 0)]
    public static void ClearValidationData(MenuCommand menuCommand)
    {
        Exceptions = new List<string>();
    }


    [MenuItem("CONTEXT/Component/ClemCAddons/Exclude from validation", true)]
    public static bool ExcludeCheckValid(MenuCommand command)
    {
        var component = (Component)command.context;
        return Exceptions.FindIndex(
            t => t == component.gameObject.name+component.name || t == component.gameObject.name + component.gameObject.GetComponents<Component>().Where(t => t != null).Select(t => t.name).Sum(t => t.Length)) == -1;
    }

    [MenuItem("CONTEXT/Component/ClemCAddons/Include it back!", true)]
    public static bool IncludeCheckValid(MenuCommand command)
    {
        var component = (Component)command.context;
        return Exceptions.FindIndex(t => t == component.gameObject.name + component.name) != -1;
    }

    [MenuItem("CONTEXT/Component/ClemCAddons/Exclude from validation", false, 0)]
    public static void ExcludeCheck(MenuCommand command)
    {
        var component = (Component)command.context;
        var t = Exceptions;
        t.Add(component.gameObject.name + component.name);
        Exceptions = t; // because fuck, these do not count as SET
    }

    [MenuItem("CONTEXT/Component/ClemCAddons/Include it back!", false, 0)]
    public static void IncludeCheck(MenuCommand command)
    {
        var component = (Component)command.context;
        var t = Exceptions;
        t.RemoveAll(t => t == component.gameObject.name + component.name);
        Exceptions = t; // because fuck, these do not count as SET
    }

    private static void ExcludeGameObject(GameObject go, ref List<string> t)
    {
        t.Add(go.name + go.GetComponents<Component>().Where(t => t != null).Select(t => t.name).Sum(t => t.Length));
        for (int i = 0; i < go.transform.childCount; i++)
            ExcludeGameObject(go.transform.GetChild(i).gameObject, ref t);
    }

    private static void IncludeGameObject(GameObject go, ref List<string> t)
    {
        t.RemoveAll(v => v == go.name + go.GetComponents<Component>().Where(t => t != null).Select(t => t.name).Sum(t => t.Length));
        foreach (Component c in go.GetComponents<Component>())
        {
            if (c == null)
                continue;
            t.RemoveAll(t => t == go.name + c.name);
        }
        for (int i = 0; i < go.transform.childCount; i++)
            IncludeGameObject(go.transform.GetChild(i).gameObject, ref t);
    }

    private static void CheckChildren(Transform gameObject)
    {
        for(int i = 0; i < gameObject.childCount; i++)
        {
            CheckGameObject(gameObject.GetChild(i).gameObject);
            CheckChildren(gameObject.GetChild(i));
        }
    }

    private static void CheckGameObject(GameObject gameObject)
    {
        foreach (Component component in gameObject.GetComponents(typeof(Component)))
        {
            if (component == null)
            {
                Debug.LogError("A component of " + gameObject + " is null. The script might be missing.\nNull components are never ignored", gameObject);
                continue;
            }
            if (Exceptions.FindIndex(t => t == component.gameObject.name + component.name) != -1)
                continue;
            if(Exceptions.FindIndex(t => t == component.gameObject.name + component.gameObject.GetComponents<Component>().Where(t => t != null).Select(t => t.name).Sum(t => t.Length)) != -1)
                continue;
            if (component.GetType() == typeof(Transform) || component.GetType() == typeof(RectTransform))
                continue;
            CheckComponent(component);
        }
    }
    
    private static void CheckComponent(Component component)
    {
        if (component == null)
            return;
        SerializedObject t = new SerializedObject(component);
        var iterator = t.GetIterator();
        iterator.NextVisible(true);
        CheckProperty(iterator, component.gameObject.name);
        while (iterator.NextVisible(false))
        {
            CheckProperty(iterator, component.gameObject.name);
        }
    }

    private static void CheckProperty(SerializedProperty property, string objectName)
    {
        if(GetTargetObjectOfProperty(property) == null)
        {
            Debug.LogWarning("Missing " + property.displayName + " in " + objectName + " / " + property.serializedObject.targetObject.name, property.serializedObject.targetObject);
        } else
        {
        }
    }


    private static void WriteToFile(string path, byte[] content)
    {
        System.IO.FileInfo file = new System.IO.FileInfo(path);
        file.Directory.Create(); // If the directory already exists, this method does nothing.
        System.IO.File.WriteAllBytes(file.FullName, content);
    }

    private static byte[] ReadFile(string path)
    {
        System.IO.FileInfo file = new System.IO.FileInfo(path);
        file.Directory.Create(); // If the directory already exists, this method does nothing.
        if (file.Exists)
            return System.IO.File.ReadAllBytes(file.FullName);
        else
        {
            return null;
        }
    }


    // credits to https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs
    // for the whole GetTargetObjectOfProperty stuff below. Could've done something ugly like https://forum.unity.com/threads/get-a-general-object-value-from-serializedproperty.327098/#post-2309509
    // but this saved me the time and efforts I would've had to take debugging this shit

    public static object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        if (prop == null)
        {
            return null;
        }
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj;
    }

    private static object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }

    private static object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();
        //while (index-- >= 0)
        //    enm.MoveNext();
        //return enm.Current;

        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext()) return null;
        }
        return enm.Current;
    }

}
#endif