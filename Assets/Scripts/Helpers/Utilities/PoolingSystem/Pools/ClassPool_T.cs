using System;
using Helpers.Utilities.PoolingSystem.Bases;

namespace Helpers.Utilities.PoolingSystem.Pools
{
    public class ClassPool<T0> : PoolBase<T0> where T0 : class
    {
        public ClassPool(Func<T0> createMethod, Func<T0> disposeMethod, Action<T0> onGet = null,
            Action<T0> onRelease = null, Action<T0> onDestroy = null, int capacity = 0, int maxSize = 0) : base(
            createMethod, disposeMethod, onGet, onRelease, onDestroy, capacity, maxSize)
        {
        }
    }
}