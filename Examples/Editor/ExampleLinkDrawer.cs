using UnityEngine;
using UnityEditor;
using ResourcesLinks.Examples;

namespace ResourcesLinks
{
    [CustomPropertyDrawer(typeof(ExampleLink))]
    public class ExampleLinkDrawer : LinkPropertyDrawer<GameObject>
    {
        public override string ResourcesFolderName => "Prefabs";
    }
}
