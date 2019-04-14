using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ResourcesLinks
{
    public abstract class LinkPropertyDrawer<T> : PropertyDrawer where T : Object
    {
        private readonly List<string> files = new List<string>();
        private readonly List<string> paths = new List<string>();

        private static readonly List<string> stringBuffer = new List<string>(100);

        public abstract string ResourcesFolderName { get; }

        private void GetResourcesDirectories(List<string> directories)
        {
            var root = Application.dataPath;

            var dirs = Directory.GetDirectories(root);

            foreach (string dir in dirs)
            {
                LoadSubDirs(dir, directories);
            }
        }

        private void LoadSubDirs(string dir, List<string> directories)
        {
            directories.Add(Path.GetFullPath(dir));
            var subDirs = Directory.GetDirectories(dir);
            foreach (string subDir in subDirs)
            {
                LoadSubDirs(subDir, directories);
            }
        }

        private void GetAssetsPaths(string folder, List<string> files, List<string> paths, string resourcesFolder)
        {

      
            var rootFolder = resourcesFolder + "/" + folder;

          

            if (!Directory.Exists(rootFolder))
                return;
            var assets = Directory.GetFiles(rootFolder);

            foreach (var a in assets)
            {
                if (a.EndsWith(".meta"))
                    continue;
                var p = Path.GetFullPath(a);
                var assetPath = "Assets" + p.Remove(0, Application.dataPath.Length);
                var ext = Path.GetExtension(assetPath);
                assetPath = assetPath.Remove(assetPath.Length - ext.Length, ext.Length);

                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath + ext);

                if (asset == null)
                    continue;

                assetPath = Path.GetFileName(assetPath);
                files.Add(assetPath);
                paths.Add(folder + "/" + assetPath);
            }
            var f = "Assets" + resourcesFolder.Remove(0, Application.dataPath.Length) + "/" + folder;
            Debug.Log(f);
            foreach (var sub in AssetDatabase.GetSubFolders(f))
            {
                var subName = Path.GetFileName(sub);
                GetAssetsPaths(folder + "/" + subName, files, paths, resourcesFolder);
            }
            

           
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, new GUIContent(), property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;

            EditorGUI.indentLevel = 0;

            var rect = new Rect(position.x, position.y, position.width - 70, position.height);

            paths.Clear();
            files.Clear();

            stringBuffer.Clear();

            GetResourcesDirectories(stringBuffer);

            for (int i = stringBuffer.Count - 1; i >= 0; i--)
            {
                if (stringBuffer[i].EndsWith("Resources"))
                {
                    GetAssetsPaths(ResourcesFolderName, files, paths, stringBuffer[i]);
                }
            }

            for (int i = 0; i < paths.Count; i++)
            {
                paths[i] = paths[i].Remove(0, ResourcesFolderName.Length + 1);
            }

            var currentPath = property.FindPropertyRelative("Path");
          
            var currentIndex = paths.FindIndex(f => ResourcesFolderName + "/" + f == currentPath.stringValue);
            var buttonRect = new Rect(position.x + position.width - 70, position.y, 70, position.height);
            if (currentIndex < 0)
            {
                if (ResourceLinkSettings.Instance.SaveLinksToDeletedFiles)
                {
                    if (paths.Count > 0)
                    {
                        EditorGUI.HelpBox(rect, "object deleted", MessageType.Error);
                        if (GUI.Button(buttonRect, "reset"))
                        {
                            currentIndex = 0;
                            if (paths.Count == 0)
                                return;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        EditorGUI.HelpBox(rect, "folder is empty", MessageType.Error);
                        return;
                    }
                }
                else
                {
                    currentIndex = 0;
                }
            }

            var selectedIndex = EditorGUI.Popup(rect, currentIndex, paths.ToArray());
            currentPath.stringValue = ResourcesFolderName + "/" + paths[selectedIndex];
             
            var obj = Resources.Load(currentPath.stringValue);
            if (obj != null)
            {
                if (GUI.Button(buttonRect, "select"))
                    Selection.activeObject = obj;
            }
            else
            {
                EditorGUI.HelpBox(rect, "object not found", MessageType.Error);
            }
            

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}



