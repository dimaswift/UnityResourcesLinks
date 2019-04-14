using UnityEngine;
using UnityEditor;
using System.IO;

namespace ResourcesLinks
{

    [CustomEditor(typeof(ResourceLinkSettings))]
    public class ResourceLinkSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = target as ResourceLinkSettings;
            Undo.RecordObject(settings, "ResourceLinkSettings");
            var rect = EditorGUILayout.GetControlRect(false, 22);
            
            GUI.Label(new Rect(rect.x, rect.y, rect.width - 100, rect.height), "Link Scripts Folder: ", EditorStyles.boldLabel);
            GUI.Label(new Rect(rect.x + 150, rect.y, rect.width - 260, rect.height), settings.LinkScriptsFolder, EditorStyles.helpBox);
            if (GUI.Button(new Rect(rect.x + rect.width - 100, rect.y, 100, rect.height), "Change"))
            {
                var newPath = EditorUtility.OpenFolderPanel("Link Scripts Folder", Application.dataPath + settings.LinkScriptsFolder, "");
                if (string.IsNullOrEmpty(newPath))
                    return;
                newPath = newPath.Remove(0, Application.dataPath.Length + 1);
                settings.LinkScriptsFolder = newPath;
                EditorUtility.SetDirty(settings);
            }
            rect.y += 30;
            GUI.Label(new Rect(rect.x, rect.y, rect.width - 100, rect.height), "Link Editor Folder: ", EditorStyles.boldLabel);
            GUI.Label(new Rect(rect.x + 150, rect.y, rect.width - 260, rect.height), settings.LinkScriptsEditorFolder, EditorStyles.helpBox);
            if (GUI.Button(new Rect(rect.x + rect.width - 100, rect.y, 100, rect.height), "Change"))
            {
                var newPath = EditorUtility.OpenFolderPanel("Link Scripts Editor Folder", Application.dataPath + settings.LinkScriptsEditorFolder, "");
                if (string.IsNullOrEmpty(newPath))
                    return;
                newPath = newPath.Remove(0, Application.dataPath.Length + 1);
                settings.LinkScriptsEditorFolder = newPath;
                EditorUtility.SetDirty(settings);
            }
            rect.y += 30;

            settings.SaveLinksToDeletedFiles = GUI.Toggle(rect, settings.SaveLinksToDeletedFiles, "   Save Links To Deleted Files");

            EditorUtility.SetDirty(settings);
        }
    }

    public class ResourceLinkSettings : ScriptableObject
    {
        public string LinkScriptsFolder = "Scripts";
        public string LinkScriptsEditorFolder = "Scripts/Editor";
        public bool SaveLinksToDeletedFiles;
        public string Namespace;
        public string LinkClass;

        static ResourceLinkSettings _instance;
        public static ResourceLinkSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    var path = "Assets/ResourceLinkSettings.asset";
                    _instance = AssetDatabase.LoadAssetAtPath<ResourceLinkSettings>(path);

                    if (_instance == null)
                    {

                        _instance = CreateInstance<ResourceLinkSettings>();
                        AssetDatabase.CreateAsset(_instance, path);
                        AssetDatabase.Refresh();
                    }
                }
                return _instance;
            }
        }

        private static string linkTemplateWithNamespace = @"using UnityEngine;
using ResourcesLinks;

namespace {1}
{{
    [System.Serializable]
    public class {0}Link : GenericLink<{0}>
    {{

    }}
}}
";

        private static string linkTemplate = @"using UnityEngine;
using ResourcesLinks;

[System.Serializable]
public class {0}Link : GenericLink<{0}>
{{

}}

";

        private static string linkEditorTemplateWithNamespace = @"using UnityEngine;
using UnityEditor;
using ResourcesLinks;

namespace {1}
{{
    [CustomPropertyDrawer(typeof({0}Link))]
    public class {0}LinkDrawer : LinkPropertyDrawer<{0}>
    {{
        public override string ResourcesFolderName => ""{0}s"";
    }}
}}
";

        private static string linkEditorTemplate = @"using UnityEngine;
using UnityEditor;
using ResourcesLinks;

[CustomPropertyDrawer(typeof({0}Link))]
public class {0}LinkDrawer : LinkPropertyDrawer<{0}>
{{
    public override string ResourcesFolderName => ""{0}s"";
}}

";

        [MenuItem("Resources Links/Create New Link")]
        static void OpenSettingsWindow()
        {
            ResourceLinkPromptWindow.ShowWindow();
        }

        public static void CreateWithSettings()
        {
            var settings = Instance;
            var template = string.IsNullOrEmpty(settings.Namespace) ? linkTemplate : linkTemplateWithNamespace;
            var editorTemplate = string.IsNullOrEmpty(settings.Namespace) ? linkEditorTemplate : linkEditorTemplateWithNamespace;

            template = string.Format(template, settings.LinkClass, settings.Namespace);
            editorTemplate = string.Format(editorTemplate, settings.LinkClass, settings.Namespace);

            var path = Application.dataPath + "/";

            if(!Directory.Exists(path + settings.LinkScriptsFolder))
            {
              
                Directory.CreateDirectory(path + settings.LinkScriptsFolder);
            }

            if (!Directory.Exists(path + settings.LinkScriptsEditorFolder))
            {
                Directory.CreateDirectory(path + settings.LinkScriptsEditorFolder);
            }

            File.WriteAllText(path + settings.LinkScriptsFolder + "/" + settings.LinkClass + "Link" + ".cs", template);

            File.WriteAllText(path + settings.LinkScriptsEditorFolder + "/" + settings.LinkClass + "LinkDrawer" + ".cs", editorTemplate);

            AssetDatabase.Refresh();

            
        }

        [MenuItem("Resources Links/Settings")]
        static void SelectSettings()
        {
            Selection.activeObject = Instance;
        }
    }
}