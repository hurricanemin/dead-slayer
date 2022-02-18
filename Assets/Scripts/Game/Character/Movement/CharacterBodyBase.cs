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

        public CharacterMovementType MovementType => bodyInfo.characterMovementType;
        public Rigidbody Rigidbody => bodyRb;

        private Vector3 _movementForce;

        private void FixedUpdate()
        {
            ApplyMovementInput();

            switch (bodyInfo.characterMovementType)
            {
                case CharacterMovementType.Water:
                    return;
                default:
                case CharacterMovementType.Air:
                case CharacterMovementType.Ground:
                    Vector3 upDirection = objectTransform.up;
                    Vector3 footPosition =
                        objectTransform.TransformPoint(bodyCollider.center + bodyInfo.relativeRaycastPoints.First() +
                                                       upDirection * 0.05f);
                    bodyInfo.characterMovementType = Physics.Raycast(
                        footPosition, -upDirection, 0.1f,
                        ~bodyInfo.mask, QueryTriggerInteraction.Ignore)
                        ? CharacterMovementType.Ground
                        : CharacterMovementType.Air;
                    break;
            }
        }

        public void SetMovementInput(Vector3 movementInput)
        {
            _movementForce = movementInput;
        }

        private void ApplyMovementInput()
        {
            ApplyForce(_movementForce, ForceMode.VelocityChange);
        }

        public virtual void Jump(float jumpForce)
        {
            ApplyForce(objectTransform.up * jumpForce, ForceMode.VelocityChange);
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