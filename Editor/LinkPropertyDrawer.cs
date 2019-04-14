using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ResourcesLinks
{
    public  class ResourcesProvider : AssetPostprocessor
    {
        public static event System.Action OnResourcesReloaded = () => { };

        static readonly List<AssetPathPair> allAssets = new List<AssetPathPair>();
        private static readonly List<string> stringBuffer = new List<string>(100);

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            Reload();
        }

        internal class AssetPathPair
        {
            public string Path;
            public Object Asset;

            public AssetPathPair(string path, Object asset)
            {
                Path = path;
                Asset = asset;
            }
        }

        private static void GetResourcesDirectories(List<string> directories)
        {
            var root = Application.dataPath;

            var dirs = Directory.GetDirectories(root);

            foreach (string dir in dirs)
            {
                LoadSubDirs(dir, directories);
            }
        }

        private static void LoadSubDirs(string dir, List<string> directories)
        {
            directories.Add(Path.GetFullPath(dir));
            var subDirs = Directory.GetDirectories(dir);
            foreach (string subDir in subDirs)
            {
                LoadSubDirs(subDir, directories);
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        [MenuItem("Resources Links/Reload Assets")]
        public static void Reload()
        {
            stringBuffer.Clear();

            allAssets.Clear();

            GetResourcesDirectories(stringBuffer);

            for (int i = stringBuffer.Count - 1; i >= 0; i--)
            {
                var folder = stringBuffer[i];
                if (folder.EndsWith("Resources"))
                {
                    var subFolders = AssetDatabase.GetSubFolders(ToLocalPath(folder));
                    foreach (var subFolder in subFolders)
                    {
                        GetAssetsPaths(Path.GetFileName(subFolder), folder);
                    }
                }
            }

            OnResourcesReloaded();
        }

        public static IEnumerable<string> GetResources<T>(string rootFolder) where T : Object
        {
            foreach (var asset in allAssets)
            {
                if (asset.Asset is T && asset.Path.StartsWith(rootFolder + "/"))
                    yield return asset.Path;
            }
        }

        static string ToLocalPath(string absolutePath)
        {
            return absolutePath.Remove(0, Application.dataPath.Length - 6);
        }

        static string ToAbsolutePath(string localPath)
        {
            return Application.dataPath + localPath;
        }

        public static void GetAssetsPaths(string folder, string resourcesFolder)
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

                var asset = AssetDatabase.LoadMainAssetAtPath(assetPath + ext);

                assetPath = Path.GetFileName(assetPath);

                allAssets.Add(new AssetPathPair(folder + "/" + assetPath, asset));
            }

            var fullFolder = "Assets" + resourcesFolder.Remove(0, Application.dataPath.Length) + "/" + folder;

            foreach (var sub in AssetDatabase.GetSubFolders(fullFolder))
            {
                var subName = Path.GetFileName(sub);
                GetAssetsPaths(folder + "/" + subName, resourcesFolder);
            }
        }
    }

    public abstract class LinkPropertyDrawer<T> : PropertyDrawer where T : Object
    {
        public abstract string ResourcesFolderName { get; }

        static readonly List<string> paths = new List<string>();
        static string[] pathsArray = new string[0];

        void UpdatePaths()
        {
            paths.Clear();
            foreach (var path in ResourcesProvider.GetResources<T>(ResourcesFolderName))
            {
                paths.Add(path);
            }
            pathsArray = new string[paths.Count];
            for (int i = 0; i < paths.Count; i++)
            {
                pathsArray[i] = paths[i].Remove(0, ResourcesFolderName.Length + 1);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, new GUIContent(), property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;

            EditorGUI.indentLevel = 0;

            var rect = new Rect(position.x, position.y, position.width - 70, position.height);

            var currentPath = property.FindPropertyRelative("Path");
          
            if(paths.Count == 0)
            {
                ResourcesProvider.OnResourcesReloaded -= UpdatePaths;
                ResourcesProvider.OnResourcesReloaded += UpdatePaths;
                UpdatePaths();
            }

            var currentIndex = paths.FindIndex(f => f == currentPath.stringValue);

            var buttonRect = new Rect(position.x + position.width - 70, position.y, 70, position.height);

            if (currentIndex < 0)
            {
                if (ResourceLinkSettings.Instance.SaveLinksToDeletedFiles)
                {
                    if (paths.Count > 0)
                    {
                        EditorGUI.HelpBox(rect, "object with link not found", MessageType.Error);
                        if (GUI.Button(buttonRect, "reset"))
                        {
                            ResourcesProvider.Reload();
                            currentIndex = 0;
                            currentPath.stringValue = paths[0];
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        EditorGUI.HelpBox(rect, "resources not found", MessageType.Error);
                        return;
                    }
                }
                else
                {
                    if (paths.Count == 0)
                    {
                        EditorGUI.indentLevel = indent;
                        EditorGUI.EndProperty();
                        return;
                    }

                    currentIndex = 0;
                    currentPath.stringValue = paths[0];
                }
            }

            if(paths.Count == 0)
            {
                EditorGUI.HelpBox(rect, "resources not found", MessageType.Error);
                return;
            }

            var obj = Resources.Load(currentPath.stringValue);

            if (obj != null)
            {
                if (GUI.Button(buttonRect, "select"))
                    Selection.activeObject = obj;
            }
            else
            {
                EditorGUI.HelpBox(rect, "object with link not found", MessageType.Error);

                if (paths.Count > 0 && GUI.Button(buttonRect, "reset"))
                {
                    currentIndex = 0;
                    foreach (var p in paths)
                    {
                        if (Resources.Load<T>(p))
                        {
                            currentPath.stringValue = p;
                            break;
                        }
                        currentIndex++;
                    }
                }
                else
                {
                    EditorGUI.indentLevel = indent;
                    EditorGUI.EndProperty();
                    return;
                }
            }

            var selectedIndex = EditorGUI.Popup(rect, currentIndex, pathsArray);

            currentPath.stringValue = paths[selectedIndex];

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}



