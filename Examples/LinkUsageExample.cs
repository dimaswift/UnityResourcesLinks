using UnityEngine;

namespace ResourcesLinks.Examples
{
    public class LinkUsageExample : MonoBehaviour
    {
        [SerializeField] private ExampleLink link;
        
        void Start()
        {
            print(string.Format("example link is valid: {0}, value: {1}", link.IsValid, link.Value));
        }
    }

}
