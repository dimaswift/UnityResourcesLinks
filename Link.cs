using System;
using UnityEngine;

namespace ResourcesLinks
{
    [Serializable]
    public abstract class Link
    {
        public string Path;

        public bool IsValid => Resources.Load(Path) != null;
    }

    [Serializable]
    public abstract class GenericLink<T> : Link where T : UnityEngine.Object
    {
        public T Load() 
        {
            return Resources.Load<T>(Path);
        }
    }
}
