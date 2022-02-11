using System;
using Helpers.Utilities.PoolingSystem.PoolManagerBases;
using UnityEngine;

namespace Helpers.Utilities.PoolingSystem.Interfaces
{
    public interface IPoolableGameObject : IPoolable
    {
        Transform Transform();
        TransformData TransformData();
        GameObjectPoolType PoolType();
        Enum CategorizationType();
        void SetData(PoolableGameObjectData data);
        PoolableGameObjectData GetData();
    }

    public enum GameObjectPoolType : sbyte
    {
        Base = -1,
    }
}