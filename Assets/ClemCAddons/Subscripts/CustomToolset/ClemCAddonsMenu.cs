using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using ClemCAddons;
using System.Linq;
using System.Reflection;
using System;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

#if (UNITY_EDITOR)
public class ClemCAddonsMenu
{
    private static GameObject _toRename;
    private static double _renameTime;


    [MenuItem("GameObject/ClemCAddons/Go to GUID", false, 0)]
    public static void GoToGUIDScene()
    {
        GUIDScenePrompt.ShowWindow();
    }

    [MenuItem("GameObject/ClemCAddons/Create Container/Inherit All", false, 0)]
    public static void CreateContainerAll(MenuCommand menuCommand)
    {
        var target = Selection.activeGameObject;
        GameObject container = new GameObject(target.name + " Container");
        GameObjectUtility.SetParentAndAlign(container, target.transform.parent.gameObject);
        container.transform.SetSiblingIndex(target.transform.GetSiblingIndex());
        container.transform.localPosition = target.transform.localPosition;
        container.transform.localRotation = target.transform.localRotation;
        target.transform.parent = container.transform;
        target.transform.localPosition = Vector3.zero;
        target.transform.localRotation = Quaternion.identity;
        Selection.activeObject = container;
        RenameFile(container);
    }

    [MenuItem("GameObject/ClemCAddons/Create Container/Inherit Position", false, 0)]
    public static void CreateContainerPos(MenuCommand menuCommand)
    {
        var target = Selection.activeGameObject;
        GameObject container = new GameObject(target.name + " Container");
        GameObjectUtility.SetParentAndAlign(container, target.transform.parent.gameObject);
        container.transform.SetSiblingIndex(target.transform.GetSiblingIndex());
        container.transform.localPosition = target.transform.localPosition;
        target.transform.parent = container.transform;
        target.transform.localPosition = Vector3.zero;
        Selection.activeObject = container;
        RenameFile(container);
    }

    [MenuItem("GameObject/ClemCAddons/Create Container/Inherit Rotation", false, 0)]
    public static void CreateContainerRot(MenuCommand menuCommand)
    {
        var target = Selection.activeGameObject;
        GameObject container = new GameObject(target.name + " Container");
        GameObjectUtility.SetParentAndAlign(container, target.transform.parent.gameObject);
        container.transform.SetSiblingIndex(target.transform.GetSiblingIndex());
        container.transform.localRotation = target.transform.localRotation;
        target.transform.parent = container.transform;
        target.transform.localRotation = Quaternion.identity;
        Selection.activeObject = container;
        RenameFile(container);
    }


    [MenuItem("GameObject/ClemCAddons/Create Container/Inherit None", false, 0)]
    public static void CreateContainerNone(MenuCommand menuCommand)
    {
        var target = Selection.activeGameObject;
        GameObject container = new GameObject(target.name + " Container");
        GameObjectUtility.SetParentAndAlign(container, target.transform.parent.gameObject);
        container.transform.SetSiblingIndex(target.transform.GetSiblingIndex());
        container.transform.localPosition = Vector3.zero;
        target.transform.parent = container.transform;
        Selection.activeObject = container;
        RenameFile(container);
    }

    [MenuItem("GameObject/ClemCAddons/Give Container/All", false, 0)]
    public static void GiveContainerAll(MenuCommand menuCommand)
    {
        var target = Selection.activeTransform;
        target.parent.localPosition = target.localPosition;
        target.localPosition = Vector3.zero;
        target.parent.localRotation = target.localRotation;
        target.localRotation = Quaternion.identity;
        target.parent.localScale = target.localScale;
        target.localScale = Vector3.zero;
    }

    [MenuItem("GameObject/ClemCAddons/Give Container/Position", false, 0)]
    public static void GiveContainerPos(MenuCommand menuCommand)
    {
        var target = Selection.activeTransform;
        target.parent.localPosition = target.localPosition;
        target.localPosition = Vector3.zero;
    }

    [MenuItem("GameObject/ClemCAddons/Give Container/Rotation", false, 0)]
    public static void GiveContainerRot(MenuCommand menuCommand)
    {
        var target = Selection.activeTransform;
        target.parent.localRotation = target.localRotation;
        target.localRotation = Quaternion.identity;
    }

    [MenuItem("GameObject/ClemCAddons/Give Container/Scale", false, 0)]
    public static void GiveContainerScale(MenuCommand menuCommand)
    {
        var target = Selection.activeTransform;
        target.parent.localScale = target.localScale;
        target.localScale = Vector3.zero;
    }

    [MenuItem("GameObject/ClemCAddons/Give Container/Position & Rotation", false, 0)]
    public static void GiveContainerPosRot(MenuCommand menuCommand)
    {
        var target = Selection.activeTransform;
        target.parent.localPosition = target.localPosition;
        target.localPosition = Vector3.zero;
        target.parent.localRotation = target.localRotation;
        target.localRotation = Quaternion.identity;
    }
    [MenuItem("GameObject/ClemCAddons/Give Container/World/All", false, 0)]
    public static void GiveContainerWAll(MenuCommand menuCommand)
    {
        var target = Selection.activeTransform;
        target.parent.localPosition = target.localPosition;
        target.localPosition = Vector3.zero;
        target.parent.localRotation = target.localRotation;
        target.localRotation = Quaternion.identity;
        target.parent.localScale = target.localScale;
        target.localScale = Vector3.zero;
    }

    [MenuItem("GameObject/ClemCAddons/Give Container/World/Position", false, 0)]
    public static void GiveContainerWPos(MenuCommand menuCommand)
    {
        var target = Selection.activeTransform;
        target.parent.localPosition = target.localPosition;
        target.localPosition = Vector3.zero;
    }

    [MenuItem("GameObject/ClemCAddons/Give Container/World/Rotation", false, 0)]
    public static void GiveContainerWRot(MenuCommand menuCommand)
    {
        var target = Selection.activeTransform;
        target.parent.localRotation = target.localRotation;
        target.localRotation = Quaternion.identity;
    }

    [MenuItem("GameObject/ClemCAddons/Give Container/World/Scale", false, 0)]
    public static void GiveContainerWScale(MenuCommand menuCommand)
    {
        var target = Selection.activeTransform;
        target.parent.localScale = target.localScale;
        target.localScale = Vector3.zero;
    }

    [MenuItem("GameObject/ClemCAddons/Give Container/World/Position & Rotation", false, 0)]
    public static void GiveContainerWPosRot(MenuCommand menuCommand)
    {
        var target = Selection.activeTransform;
        target.parent.localPosition = target.localPosition;
        target.localPosition = Vector3.zero;
        target.parent.localRotation = target.localRotation;
        target.localRotation = Quaternion.identity;
    }

    [MenuItem("Assets/ClemCAddons/Show in Folder", false, 0)]
    public static void ShowInFolder(MenuCommand menuCommand)
    {
        var target = Selection.activeObject;
        var pb = Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
        var ins = pb.GetField("s_LastInteractedProjectBrowser", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        var method = pb.GetMethod("ClearSearch", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(ins, null);
        EditorGUIUtility.PingObject(target);
    }

    [MenuItem("Assets/ClemCAddons/Copy", false, 0)]
    public static void Copy()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        var t = path.Split('/');
        var newPath = path.Substring(0,path.Length-t[t.Length - 1].Length);
        var filename = t[t.Length - 1].Split('.')[0];
        var match = Regex.Match(filename, @"\d+$");
        if (match.Success)
        {
            filename = filename.Substring(0, filename.Length - match.Value.Length) + (int.Parse(match.Value) + 1);
        }
        else
            filename = filename + " new";
        NamePrompt.ShowWindow(path, newPath, filename, t[t.Length - 1].Split('.')[1]);
    }
    [MenuItem("Assets/ClemCAddons/Duplicate", false, 0)]
    public static void Duplicate()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        var t = path.Split('/');
        var newPath = path.Substring(0, path.Length - t[t.Length - 1].Length);
        var filename = t[t.Length - 1].Split('.')[0];
        var match = Regex.Match(filename, @"\d+$");
        if (match.Success)
        {
            filename = filename.Substring(0, filename.Length - match.Value.Length) + (int.Parse(match.Value) + 1);
        }
        else
            filename = filename + " new";
        AssetDatabase.CopyAsset(path, newPath + filename + "." + t[t.Length - 1].Split('.')[1]);
    }

    [MenuItem("Assets/ClemCAddons/Add to build settings", false, 0)]
    public static void AddBuildSettings()
    {
        foreach(var sel in Selection.objects)
        {
            if(System.Array.Find(EditorBuildSettings.scenes, t => t.path == AssetDatabase.GetAssetPath(sel)) == null)
                EditorBuildSettings.scenes = EditorBuildSettings.scenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sel), true));
        }
    }

    [MenuItem("Assets/ClemCAddons/Add to build settings", true, 0)]
    public static bool AddBuildSettingsCheck()
    {
        return Selection.activeObject.GetType() == typeof(SceneAsset);
    }

    [MenuItem("Assets/ClemCAddons/Go to GUID", false, 0)]
    public static void GoToGUID()
    {
        GUIDPrompt.ShowWindow();
    }


    private static void RenameFile(GameObject gameObject)
    {
        _toRename = gameObject;
        _renameTime = EditorApplication.timeSinceStartup + 0.2d;
        EditorApplication.update += EngageRenameMode;
    }

    private static void EngageRenameMode()
    {
        if (EditorApplication.timeSinceStartup >= _renameTime)
        {
            EditorApplication.update -= EngageRenameMode;
            var e = new Event { keyCode = KeyCode.F2, type = EventType.KeyDown }; // or Event.KeyboardEvent("f2");
            EditorWindow.focusedWindow.SendEvent(e);
        }
    }


    public class NamePrompt : EditorWindow
    {
        string path = "";
        string newPath = "";
        string extension = "";
        string fileName = "";

        public static void ShowWindow(string path, string newPath, string defaultName, string extension)
        {
            var r = (NamePrompt)GetWindow(typeof(NamePrompt));
            r.path = path;
            r.newPath = newPath;
            r.fileName = defaultName;
            r.extension = extension;
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Copy");
            newPath = EditorGUILayout.TextField("Path", newPath);
            fileName = EditorGUILayout.TextField("Name", fileName);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy"))
            {
                AssetDatabase.CopyAsset(path, newPath + fileName +"."+ extension);
            }
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }

    public class GUIDScenePrompt : EditorWindow
    {
        int guid = 0;

        public static void ShowWindow()
        {
            var r = (GUIDPrompt)GetWindow(typeof(GUIDPrompt));
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Find File by GUID");
            guid = EditorGUILayout.IntField("GUID", guid);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Find"))
            {
                GameObject target = null;
                var root = SceneManager.GetActiveScene().GetRootGameObjects();
                foreach(var obj in root)
                {
                    var t = obj.FindDeepUID(guid);
                    if (t != null)
                    {
                        target = t;
                        break;
                    }
                }
                if (target == null)
                {
                    var ms = new MessageBoxCC();
                    ms.Message_Box("Failed to find GUID", "No gameobject with \"" + guid + "\" GUID could be found", "OK");
                    return;
                }
                EditorGUIUtility.PingObject(target);
                Close();
            }
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }

    public class GUIDPrompt : EditorWindow
    {
        string guid = "";

        public static void ShowWindow()
        {
            var r = (GUIDPrompt)GetWindow(typeof(GUIDPrompt));
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Find File by GUID");
            guid = EditorGUILayout.TextField("GUID", guid);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Find"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if(path == "")
                {
                    var ms = new MessageBoxCC();
                    ms.Message_Box("Failed to find GUID", "No file with \""+guid+"\" GUID could be found", "OK");
                    return;
                }
                var target = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                EditorGUIUtility.PingObject(target);
                Close();
            }
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }
    public class MessageBoxCC
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern System.IntPtr GetActiveWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern int MessageBox(IntPtr hwnd, String lpText, String lpCaption, uint uType);

        public static System.IntPtr GetWindowHandle()
        {
            return GetActiveWindow();
        }

        /// <summary>
        /// Shows Message Box with button type.
        /// </summary>
        /// <param name="text">Main alert text / content.</param>
        /// <param name="caption">Message box title.</param>
        /// <param name="type">Type of message / icon to use - </param>
        /// <remarks>types: AbortRetryIgnore, CancelTryContinue, Help, OK, OkCancel, RetryCancel, YesNo, YesNoCancel</remarks>
        /// <example>Message_Box("My Text Message", "My Title", "OK");</example>
        /// <returns>OK,CANCEL,ABORT,RETRY, IGNORE, YES, NO, TRY AGAIN</returns>
        public string Message_Box(string text, string caption, string type)
        {
            try
            {
                string DialogResult = string.Empty;
                uint MB_ABORTRETRYIGNORE = (uint)(0x00000002L | 0x00000010L);
                uint MB_CANCELTRYCONTINUE = (uint)(0x00000006L | 0x00000030L);
                uint MB_HELP = (uint)(0x00004000L | 0x00000040L);
                uint MB_OK = (uint)(0x00000000L | 0x00000040L);
                uint MB_OKCANCEL = (uint)(0x00000001L | 0x00000040L);
                uint MB_RETRYCANCEL = (uint)0x00000005L;
                uint MB_YESNO = (uint)(0x00000004L | 0x00000040L);
                uint MB_YESNOCANCEL = (uint)(0x00000003L | 0x00000040L);
                int intresult = -1;
                string strResult = string.Empty;

                switch (type)
                {
                    case "AbortRetryIgnore":
                        intresult = MessageBox(GetWindowHandle(), text, caption, MB_ABORTRETRYIGNORE);
                        break;
                    case "CancelTryContinue":
                        intresult = MessageBox(GetWindowHandle(), text, caption, MB_CANCELTRYCONTINUE);
                        break;
                    case "Help":
                        intresult = MessageBox(GetWindowHandle(), text, caption, MB_HELP);
                        break;
                    case "OK":
                        intresult = MessageBox(GetWindowHandle(), text, caption, MB_OK);
                        break;
                    case "OkCancel":
                        intresult = MessageBox(GetWindowHandle(), text, caption, MB_OKCANCEL);
                        break;
                    case "RetryCancel":
                        intresult = MessageBox(GetWindowHandle(), text, caption, MB_RETRYCANCEL);
                        break;
                    case "YesNo":
                        intresult = MessageBox(GetWindowHandle(), text, caption, MB_YESNO);
                        break;
                    case "YesNoCancel":
                        intresult = MessageBox(GetWindowHandle(), text, caption, MB_YESNOCANCEL);
                        break;
                    default:
                        intresult = MessageBox(GetWindowHandle(), text, caption, (uint)(0x00000000L | 0x00000010L));
                        break;
                }

                switch (intresult)
                {
                    case 1:
                        strResult = "OK";
                        break;
                    case 2:
                        strResult = "CANCEL";
                        break;
                    case 3:
                        strResult = "ABORT";
                        break;
                    case 4:
                        strResult = "RETRY";
                        break;
                    case 5:
                        strResult = "IGNORE";
                        break;
                    case 6:
                        strResult = "YES";
                        break;
                    case 7:
                        strResult = "NO";
                        break;
                    case 10:
                        strResult = "TRY AGAIN";
                        break;
                    default:
                        strResult = "OK";
                        break;

                }

                return strResult;
            }
            catch
            {
                return string.Empty;
            }
        }
    }

}
#endif