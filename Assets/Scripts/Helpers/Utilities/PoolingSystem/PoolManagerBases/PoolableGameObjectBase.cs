using System;
using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using Helpers.Utilities.PoolingSystem.Interfaces;
using Newtonsoft.Json;
using UnityEngine;

namespace Helpers.Utilities.PoolingSystem.PoolManagerBases
{
    public abstract class PoolableGameObjectBase : MonoBehaviour, IPoolableGameObject
    {
        public virtual GameObjectPoolType GameObjectPoolType => GameObjectPoolType.Base;

        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        private Transform objectTransform;

        public int InstanceID() => GetInstanceID();
        public Type Type() => GetType();
        public Transform Transform() => objectTransform;
        public TransformData TransformData() => new TransformData(objectTransform);
        public GameObjectPoolType PoolType() => GameObjectPoolType;
        public virtual Enum CategorizationType() => null;
        public PoolableGameObjectData GetData() => null;

        public virtual void SetData(PoolableGameObjectData data)
        {
        }
    }

    [Serializable]
    public abstract class PoolableGameObjectData
    {
        [JsonIgnore] public virtual GameObjectPoolType PoolType => GameObjectPoolType.Base;
        [JsonIgnore] public virtual Type CategorizationEnumType => typeof(Enum);
        [JsonProperty("T")] public SerializedTransformData serializedTransformData;
        [JsonProperty("i")] public int attributeEnumIndex;
    }
}