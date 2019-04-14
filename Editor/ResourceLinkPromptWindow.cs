using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace ResourcesLinks
{
    public class ResourceLinkPromptWindow : EditorWindow
    {

        public static void ShowWindow()
        {
            var win = GetWindow<ResourceLinkPromptWindow>();
            win.ShowPopup();

        }

        public void OnGUI()
        {
            ResourceLinkSettings.Instance.Namespace = EditorGUILayout.TextField("Namespace", ResourceLinkSettings.Instance.Namespace);
            ResourceLinkSettings.Instance.LinkClass = EditorGUILayout.TextField("Link Class", ResourceLinkSettings.Instance.LinkClass);
            if (GUILayout.Button("Create"))
            {
                ResourceLinkSettings.CreateWithSettings();
                Close();
                return;
            }
            EditorUtility.SetDirty(this);
        }
    }

}
