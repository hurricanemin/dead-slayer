using UnityEngine;

namespace Helpers.AutoObject.Interfaces
{
    public interface IAutoObject
    {
        public int InstanceID { get; }
        public Transform ObjectTransform { get; }
    }
}