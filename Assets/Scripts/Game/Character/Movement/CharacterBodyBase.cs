using System;
using System.Linq;
using Helpers.AutoObject;
using Helpers.Utilities;
using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using UnityEngine;

namespace Game.Character.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public abstract class CharacterBodyBase : AutoObject
    {
        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        private Rigidbody bodyRb;

        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        private CapsuleCollider bodyCollider;

        [SerializeField] [HideInInspector] private BodyInfo bodyInfo;

        public CharacterMovementType MovementType => bodyInfo.characterMovementType;

        private void FixedUpdate()
        {
            switch (bodyInfo.characterMovementType)
            {
                case CharacterMovementType.Water:
                    return;
                default:
                case CharacterMovementType.Air:
                case CharacterMovementType.Ground:
                    Vector3 upDirection = objectTransform.up;
                    bodyInfo.characterMovementType = Physics.Raycast(
                        bodyInfo.relativeRaycastPoints.First() + upDirection * 0.05f, -upDirection, 0.25f,
                        ~bodyInfo.mask, QueryTriggerInteraction.Ignore)
                        ? CharacterMovementType.Ground
                        : CharacterMovementType.Air;
                    break;
            }
        }

        public virtual void ApplyMovementInput(Vector3 movementInput)
        {
            bodyRb.AddForce(movementInput * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        public virtual void Jump(float jumpForce)
        {
            bodyRb.AddForce(objectTransform.up * jumpForce, ForceMode.VelocityChange);
        }

        [FieldInitializer]
        protected virtual void InitializeVariables()
        {
            try
            {
                Vector3 height = Vector3.up * bodyCollider.height;
                bodyInfo.relativeRaycastPoints = new[] { height / 2, Vector3.zero, -height / 2 };
                bodyInfo.radius = bodyCollider.radius;
                bodyInfo.mask = this.gameObject.layer;
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
            public CharacterMovementType characterMovementType;
            public Vector3[] relativeRaycastPoints;
            public float radius;
            public int mask;
        }

        public enum CharacterMovementType : byte
        {
            Air,
            Ground,
            Water,
        }
    }
}