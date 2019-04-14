using System;
using UnityEngine;

namespace ResourcesLinks
{
    [Serializable]
    public abstract class Link
    {
        [SerializeField]
        protected string Path;

        public bool IsValid => Resources.Load(Path) != null;
    }

    [Serializable]
    public abstract class GenericLink<T> : Link where T : UnityEngine.Object
    {
        private T _cachedObject;
        private bool _loaded;

        public T Load() 
        {
            if (_loaded)
                return _cachedObject;
            return _cachedObject = Resources.Load<T>(Path);
        }
    }
}
