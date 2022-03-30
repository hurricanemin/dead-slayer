using System;
using System.Linq;
using Game.Interfaces;
using Helpers.AutoObject;
using Helpers.Utilities;
using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using UnityEngine;

namespace Game.Character.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class CharacterBodyBase : AutoObject, IPhysicsObject
    {
        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        private Rigidbody bodyRb;

        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        private CapsuleCollider bodyCollider;

        [SerializeField] [HideInInspector] private BodyInfo bodyInfo;

        public BodyMovementState MovementState => bodyInfo.bodyMovementState;
        public Rigidbody Rigidbody => bodyRb;

        private Vector3 groundUp;

        private void FixedUpdate()
        {
            switch (bodyInfo.bodyMovementState)
            {
                case BodyMovementState.Water:
                    groundUp = objectTransform.up;
                    return;
                default:
                case BodyMovementState.Air:
                case BodyMovementState.Ground:
                    Vector3 upDirection = objectTransform.up;
                    Vector3 footPosition =
                        objectTransform.TransformPoint(bodyCollider.center + bodyInfo.relativeRaycastPoints.First() +
                                                       upDirection * 0.05f);
                    bool isBodyGrounded = Physics.Raycast(
                        footPosition, -upDirection, out RaycastHit hit, 0.1f,
                        ~bodyInfo.mask, QueryTriggerInteraction.Ignore);
                    bodyInfo.bodyMovementState = isBodyGrounded ? BodyMovementState.Ground : BodyMovementState.Air;
                    groundUp = isBodyGrounded ? hit.normal : objectTransform.up;
                    break;
            }
        }

        public void ApplyMovementInput(Vector3 movementInput)
        {
            float angleOffset =
                Mathf.Clamp(Vector2.SignedAngle(objectTransform.up.GetYZVector(), groundUp.GetYZVector()), -30, 30);
            Quaternion rotator = Quaternion.Euler(angleOffset, 0, 0);
            ApplyForce(rotator * movementInput, ForceMode.VelocityChange);
        }

        public void ApplyForce(Vector3 forceAmount, ForceMode forceMode)
        {
            bodyRb.AddForce(forceAmount, forceMode);
        }

        [FieldInitializer]
        protected virtual void InitializeVariables()
        {
            try
            {
                Vector3 height = Vector3.up * bodyCollider.height;
                bodyInfo.relativeRaycastPoints = new[] { -height / 2, Vector3.zero, height / 2 };
                bodyInfo.radius = bodyCollider.radius;
                bodyInfo.mask = LayerMask.GetMask(LayerMask.LayerToName(this.gameObject.layer));
            }
            catch (Exception e)
            {
                Debug.LogError("Error initializing {0}!\n{1}".Format(this.name, e));
                areReferencesSet = false;
            }
        }

        [Serializable]
        private struct BodyInfo
        {
            public BodyMovementState bodyMovementState;
            public Vector3[] relativeRaycastPoints;
            public float radius;
            public int mask;
        }

        public enum BodyMovementState : byte
        {
            Air,
            Ground,
            Water,
        }
    }
}