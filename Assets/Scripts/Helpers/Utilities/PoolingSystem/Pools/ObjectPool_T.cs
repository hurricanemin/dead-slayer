using System;
using Helpers.Utilities.PoolingSystem.Bases;
using Object = UnityEngine.Object;

namespace Helpers.Utilities.PoolingSystem.Pools
{
    public class ObjectPool<T0> : PoolBase<T0> where T0 : Object
    {
        public ObjectPool(Func<T0> createMethod, Func<T0> disposeMethod, Action<T0> onGet = null,
            Action<T0> onRelease = null, Action<T0> onDestroy = null, int capacity = 0, int maxSize = 0) : base(
            createMethod, disposeMethod, onGet, onRelease, onDestroy, capacity, maxSize)
        {
        }

        public void InsertElements(T0[] objects)
        {
        }
    }
}