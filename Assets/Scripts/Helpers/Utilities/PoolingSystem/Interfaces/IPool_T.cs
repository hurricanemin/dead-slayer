using System;

namespace Helpers.Utilities.PoolingSystem.Interfaces
{
    public interface IPool
    {
    }

    public interface IPool<in T0> : IPool, IDisposable
    {
        public int AvailableObjectCount { get; }
        public int BusyObjectCount { get; }
        public int PooledObjectCount { get; }
        public void ReleaseObject(T0 obj);
        public void DisposeObj(T0 obj);
    }
}