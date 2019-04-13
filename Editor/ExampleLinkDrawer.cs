using UnityEngine;
using UnityEditor;
using ResourcesLinks.Examples;

namespace ResourcesLinks.Editor
{
    [CustomPropertyDrawer(typeof(LinkExample))]
    public class ExampleLinkDrawer : LinkPropertyDrawer<Sprite>
    {
        public override string ResourcesFolderName => "Sprites";
    }

}
