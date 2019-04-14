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

        public virtual string ResourcesFolderName => typeof(T).Name + "s";

        private void GetAssetsPaths(string folder, List<string> files, List<string> paths)
        {
            var resourcesFolder = "/Resources";
            var rootFolder = Application.dataPath + resourcesFolder + "/" + folder;
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
            var f = "Assets" + resourcesFolder + "/" + folder;

            foreach (var sub in AssetDatabase.GetSubFolders(f))
            {
                var subName = Path.GetFileName(sub);
                GetAssetsPaths(folder + "/" + subName, files, paths);
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

            GetAssetsPaths(ResourcesFolderName, files, paths);

            for (int i = 0; i < paths.Count; i++)
            {
                paths[i] = paths[i].Remove(0, ResourcesFolderName.Length + 1);
            }

            var currentPath = property.FindPropertyRelative("Path");
            if (files.Count > 0)
            {
                var currentIndex = paths.FindIndex(f => ResourcesFolderName + "/" + f == currentPath.stringValue);

                if (currentIndex < 0)
                    currentIndex = 0;

                var selectedIndex = EditorGUI.Popup(rect, currentIndex, paths.ToArray());
                currentPath.stringValue = ResourcesFolderName + "/" + paths[selectedIndex];
                var buttonRect = new Rect(position.x + position.width - 70, position.y, 70, position.height);
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
            }
            else
            {
                EditorGUI.HelpBox(rect, "empty", MessageType.Warning);
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}



