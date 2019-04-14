using System;
using UnityEngine;

namespace ResourcesLinks
{
    [Serializable]
    public abstract class Link
    {
        [SerializeField]
        protected string Path;

        public virtual bool IsValid => Resources.Load(Path) != null;
    }

    [Serializable]
    public abstract class GenericLink<T> : Link where T : UnityEngine.Object
    {
        [NonSerialized]
        private T _cachedObject;

        public override bool IsValid => Resources.Load<T>(Path) != null;

        public T Value 
        {
            get
            {
                if (_cachedObject != null)
                    return _cachedObject;
                _cachedObject = Resources.Load<T>(Path);
                return _cachedObject;
            }
        }
    }
}
