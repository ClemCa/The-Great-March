using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class MindVisualizer : MonoBehaviour
{
    [SerializeField] private Menu menu;
    [SerializeField] private MindVisualizerOptions options;
    [SerializeField] private string currentPath;
    [SerializeField] private DialogueGenerator dialogueGenerator;

    private void Init()
    {
        currentPath = menu.Name;
        options.Navigation += UpdatePath;
        options.Options = menu.GetOptions(currentPath);
        options.BackText = currentPath.Split(".").Last();
    }


    // DON'T FORGET THE WHOLE GENERATION IS MADE FOR DEBUG PURPOSES
    // For the game, a menu like this would require a lot of other
    // conditions, and this wouldn't be made using reflection
    // This one is to make sure easily that every topic can be talked about using the dialogue generator
    public void GenerateFrom(string name, EvolutiveStory.Character character, int maxDepth)
    {
        menu = new Menu(new List<Menu>(), name, this);
        var fields = character.GetType().GetFields().Cast<MemberInfo>().Concat(character.GetType().GetProperties());
        fields = fields.Where(t => Attribute.IsDefined(t, typeof(Visualizable))).ToArray();
        foreach(var field in fields)
        {
            menu.Children.Add(GenerateFromField(field, 1, maxDepth));
        }
        Init();
    }

    private Menu GenerateFromField(MemberInfo fieldInfo, int depth, int maxDepth)
    {
        var menu = new Menu(new List<Menu>(), fieldInfo.Name, this);
        if (depth == maxDepth)
            return menu;
        MemberInfo[] fields;
        if(fieldInfo is FieldInfo)
            fields = ((FieldInfo)fieldInfo).FieldType.GetFields().Cast<MemberInfo>().Concat(((FieldInfo)fieldInfo).FieldType.GetProperties().Cast<MemberInfo>()).ToArray();
        else
            fields = ((PropertyInfo)fieldInfo).PropertyType.GetFields().Cast<MemberInfo>().Concat(((PropertyInfo)fieldInfo).PropertyType.GetProperties().Cast<MemberInfo>()).ToArray();
        fields = fields.Where(t => Attribute.IsDefined(t, typeof(Visualizable))).ToArray();
        foreach (var field in fields)
        {
            menu.Children.Add(GenerateFromField(field, depth + 1, maxDepth));
        }
        return menu;
    }

    private void GenerateDialogueFromPath(string path)
    {
        var p = path.Split(".").Skip(1).ToArray();
        var field = GetFieldFromPath(p, dialogueGenerator.TestCharacter);
        dialogueGenerator.GenerateFromField(dialogueGenerator.TestCharacter, field);
    }

    private object GetFieldFromPath(string[] path, object current)
    {
        var target = path[0];
        var field = current.GetType().GetField(target);
        if(field != null)
        {
            current = field.GetValue(current);
        }
        else
        {
            var property = current.GetType().GetProperty(target);
            current = property.GetValue(current);
        }
        if (path.Length == 1)
            return current;
        return GetFieldFromPath(path.Skip(1).ToArray(), current);
    }

    public void UpdatePath(string target)
    {
        currentPath = menu.Navigation(currentPath, target);
        options.Options = menu.GetOptions(currentPath);
        options.BackText = currentPath.Split(".").Last();
    }

    [Serializable]
    public struct Menu
    {
        public MindVisualizer MindVisualizer;
        public string Name;
        public List<Menu> Children;
        public Menu(List<Menu> children, string name, MindVisualizer mindVisualizer)
        {
            Children = children;
            Name = name;
            MindVisualizer = mindVisualizer;
        }

        public bool IsFinal
        {
            get
            {
                return Children == null || Children.Count == 0;
            }
        }
        public string Navigation(string path, string target)
        {
            if (path == target)
            {
                Debug.Log("supposed to close");
                return path;
            }
            var p = path.Split(".");
            var r = NavigationInternal(p.Skip(1).ToArray(), target);
            switch (r)
            {
                case -1:
                    return string.Join(".", p.SkipLast(1));
                case 0:
                    Debug.Log("supposed to do something");
                    MindVisualizer.GenerateDialogueFromPath(path + "." + target);
                    return path;
                case 1:
                    return path + "." + target;
                default:
                    Debug.LogError("BUG");
                    return path;
            }
        }

        private int NavigationInternal(string[] path, string target)
        {
            if (path.Length == 0)
            {
                var c = Children.Find(t => t.Name == target);
                return c.IsFinal ? 0 : 1;
            }
            else
            {
                if(path.Length == 1 && path[0] == target)
                    return -1;
                var c = Children.Find(t => t.Name == path[0]);
                return c.NavigationInternal(path.Skip(1).ToArray(), target);
            }
        }

        public string[] GetOptions(string path)
        {
            var p = path.Split(".");
            if(p.Length <= 1)
                return Children.Select(t => t.Name).ToArray();
            return GetOptionsInternal(p.TakeLast(p.Length - 1).ToArray());
        }
        
        private string[] GetOptionsInternal(string[] path)
        {
            if (path.Length == 0)
                return Children.Select(t => t.Name).ToArray();
            var next = path[0];
            var child = Children.Find(t => t.Name == next);
            return child.GetOptionsInternal(path.TakeLast(path.Length - 1).ToArray());
        }
    }
}

[AttributeUsage(AttributeTargets.All, Inherited = false)]
public class Visualizable : Attribute
{

}