using System;

namespace Helpers.Utilities.PoolingSystem.Interfaces
{
    public interface IPoolable
    {
        int InstanceID();
        Type Type();
    }
}