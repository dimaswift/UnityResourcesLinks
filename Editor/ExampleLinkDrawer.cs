using UnityEngine;
using UnityEditor;
using ResourcesLinks.Examples;

namespace ResourcesLinks
{
    [CustomPropertyDrawer(typeof(LinkExample))]
    public class ExampleLinkDrawer : LinkPropertyDrawer<Sprite>
    {
        
    }
}
