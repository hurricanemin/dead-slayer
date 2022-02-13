using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Helpers.Utilities
{
    public class TransformData
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector3 Scale { get; private set; }
        public Vector3 LocalPosition { get; private set; }
        public Quaternion LocalRotation { get; private set; }
        public Vector3 LocalScale { get; private set; }

        [JsonConstructor]
        public TransformData()
        {
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
            Scale = Vector3.one;
        }

        public TransformData(Transform transform)
        {
            Position = transform.position;
            LocalPosition = transform.localPosition;
            Rotation = transform.rotation;
            LocalRotation = transform.localRotation;
            Scale = transform.lossyScale;
            LocalScale = transform.localScale;
        }

        public TransformData(Vector3 position)
        {
            Position = position;
            LocalPosition = Vector3.zero;
            Rotation = Quaternion.identity;
            LocalRotation = Quaternion.identity;
            Scale = Vector3.one;
            LocalScale = Vector3.one;
        }

        public TransformData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            LocalPosition = Vector3.zero;
            Rotation = rotation;
            LocalRotation = Quaternion.identity;
            Scale = Vector3.one;
            LocalScale = Vector3.one;
        }

        public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            LocalPosition = Vector3.zero;
            Rotation = rotation;
            LocalRotation = Quaternion.identity;
            Scale = scale;
            LocalScale = Vector3.one;
        }

        public void SyncGlobal(Transform transform)
        {
            transform.position = Position;
            transform.rotation = Rotation;
            transform.localScale = Scale;
        }

        public void SyncLocal(Transform transform)
        {
            transform.localPosition = LocalPosition;
            transform.localRotation = LocalRotation;
            transform.localScale = LocalScale;
        }

        public void UpdateData(Transform transform)
        {
            LocalPosition = transform.localPosition;
            LocalRotation = transform.localRotation;
            LocalScale = transform.localScale;
            Position = transform.position;
            Rotation = transform.rotation;
            Scale = transform.lossyScale;
        }
    }
    
    [Serializable]
    public class SerializedTransformData
    {
        [JsonProperty("p")] public readonly Vector3 Position;

        [JsonProperty("r")] public readonly Vector3 Rotation;

        [JsonProperty("s")] public readonly Vector3 Scale;

        [JsonConstructor]
        public SerializedTransformData()
        {
        }

        public SerializedTransformData(Transform transform)
        {
            Position = transform.position;
            Rotation = transform.eulerAngles;
            Scale = transform.lossyScale;
        }

        public SerializedTransformData(Vector3 position)
        {
            Position = position;
            Rotation = Vector3.zero;
            Scale = Vector3.one;
        }

        public SerializedTransformData(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
            Scale = Vector3.one;
        }

        public SerializedTransformData(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public void SetTransformData(Transform transform)
        {
            transform.position = Position;
            transform.eulerAngles = Rotation;
            transform.localScale = Scale;
        }

        public TransformData ReturnTransformData()
        {
            return new TransformData(Position, Quaternion.Euler(Rotation), Scale);
        }
    }

}