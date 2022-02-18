using UnityEngine;

namespace Game.Interfaces
{
    public interface IPhysicsObject
    {
        public Rigidbody Rigidbody { get; }
        public void ApplyForce(Vector3 forceAmount, ForceMode forceMode);
    }
}