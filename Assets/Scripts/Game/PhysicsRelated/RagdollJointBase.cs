using System;
using Game.Interfaces;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.PhysicsRelated
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class RagdollJointBase : MonoBehaviour, IDamageable
    {
        [SerializeField] protected Rigidbody jointRigidbody;
        [SerializeField] protected CharacterJoint characterJoint;
        [SerializeField] protected Transform jointTransform;
        [HideInInspector] [SerializeField] protected RuntimeRagdollJointData runtimeJointData;
        [HideInInspector] [SerializeField] protected RagdollManager manager;

        public virtual BodyPart BodyPartType => BodyPart.None;
        public BodyDirection bodyDirection = BodyDirection.None;

        public Rigidbody Rigidbody => jointRigidbody;

        public Vector3 CalculatedVelocity { get; private set; }
        public Vector3 CalculatedAngularVelocity { get; private set; }

        protected bool isFirstFramePassed;
        protected bool isActive;

        private void LateUpdate()
        {
            if (isActive) return;
            Vector3 currentPosition = jointTransform.position;
            Vector3 currentRotation = jointTransform.eulerAngles;

            if (!isFirstFramePassed)
            {
                runtimeJointData.lastPosition = currentPosition;
                runtimeJointData.lastRotation = currentRotation;
                isFirstFramePassed = true;
            }

            float changeX = currentRotation.x - runtimeJointData.lastRotation.x;
            float changeY = currentRotation.y - runtimeJointData.lastRotation.y;
            float changeZ = currentRotation.z - runtimeJointData.lastRotation.z;
            bool angX = Mathf.Abs(changeX) > 180;
            bool angY = Mathf.Abs(changeY) > 180;
            bool angZ = Mathf.Abs(changeZ) > 180;

            if (angX || angY || angZ)
            {
                if (angX)
                {
                    if (changeX < 0) changeX += 360;
                    else changeX -= 360;
                }

                if (angY)
                {
                    if (changeY < 0) changeY += 360;
                    else changeY -= 360;
                }

                if (angZ)
                {
                    if (changeZ < 0) changeZ += 360;
                    else changeZ -= 360;
                }
            }

            CalculatedVelocity = (currentPosition - runtimeJointData.lastPosition) / Time.unscaledDeltaTime;
            CalculatedAngularVelocity = new Vector3(changeX, changeY, changeZ) / Time.unscaledDeltaTime;
            runtimeJointData.lastPosition = currentPosition;
            runtimeJointData.lastRotation = currentRotation;
        }

        public void SetActive(bool isActive)
        {
            if (isActive == this.isActive) return;
            this.isActive = isActive;
            CalculatedVelocity = this.isActive ? CalculatedVelocity : Vector3.zero;
            CalculatedAngularVelocity = isActive ? CalculatedAngularVelocity : Vector3.zero;
            jointRigidbody.velocity = Vector3.zero;
            jointRigidbody.angularVelocity = Vector3.zero;
            jointRigidbody.isKinematic = !this.isActive;
            if (!this.isActive) return;
            jointRigidbody.angularVelocity = -CalculatedAngularVelocity * (2 * Mathf.Deg2Rad);
            jointRigidbody.velocity = CalculatedVelocity;
            CalculatedVelocity = Vector3.zero;
            CalculatedAngularVelocity = Vector3.zero;
        }

        public void AddForce(ForceData forceData)
        {
            manager.ToggleRagdoll(true);
            if (!isActive) return;

            switch (forceData.forceType)
            {
                case ForceType.Flat:
                    jointRigidbody.AddForce(forceData.force, forceData.forceMode);
                    break;
                case ForceType.AtPosition:
                    jointRigidbody.AddForceAtPosition(forceData.force, forceData.position, forceData.forceMode);
                    break;
                case ForceType.Explosion:
                    jointRigidbody.AddExplosionForce(forceData.intensity, forceData.position, forceData.radius,
                        forceData.upwardsModifier,
                        forceData.forceMode);
                    break;
            }
        }

        // public void UpdateJoint()
        // {
        //     if (_shouldUpdateCollider)
        //     {
        //         float currentLength = _transform.InverseTransformPoint(nextBone.position).y;
        //         float difference = currentLength - _startLength;
        //         float ratio = currentLength / _startLength;
        //
        //         if (_hasCapsule)
        //         {
        //             _capsuleCollider.center = new Vector3(_startCenter.x, _startCenter.y * ratio, _startCenter.z);
        //             _capsuleCollider.height = _startSize.y * ratio;
        //         }
        //         else
        //         {
        //             _boxCollider.center = new Vector3(_startCenter.x, _startCenter.y + difference / 2);
        //             _boxCollider.size = new Vector3(_startSize.x, _startSize.y + difference, _startSize.z);
        //         }
        //     }
        //
        //     if (!_hasJoint) return;
        //     characterJoint.connectedAnchor = connectedBody.InverseTransformPoint(_transform.position);
        // }


        public void Initialize()
        {
#if UNITY_EDITOR
            manager = this.GetComponentInParent<RagdollManager>();
            jointTransform = this.transform;
            runtimeJointData = new RuntimeRagdollJointData
            {
                startPosition = jointTransform.localPosition,
                startRotation = jointTransform.localEulerAngles
            };
            jointRigidbody = this.GetComponent<Rigidbody>();
            jointRigidbody.useGravity = true;
            jointRigidbody.isKinematic = true;
            this.gameObject.layer = LayerMask.NameToLayer("GameCharacter");
            if (!this.TryGetComponent(out characterJoint)) return;
            characterJoint = this.GetComponent<CharacterJoint>();
            characterJoint.enableProjection = true;
            characterJoint.autoConfigureConnectedAnchor = false;
            bodyDirection =
                this.name.ToLower().Contains(BodyDirection.Left.ToString().ToLower())
                    ? BodyDirection.Left
                    : this.name.ToLower().Contains(BodyDirection.Right.ToString().ToLower())
                        ? BodyDirection.Right
                        : BodyDirection.None;
            // connectedBody = characterJoint.connectedBody.transform;
#endif
        }

        [Serializable]
        protected struct RuntimeRagdollJointData
        {
            // public Vector3 _startCenter;
            // public Vector3 _startSize;
            public Vector3 startPosition;
            public Vector3 startRotation;
            public Vector3 lastPosition;
            public Vector3 lastRotation;
        }

        public bool ReceiveFlatDamage(float damageAmount)
        {
            return manager.OwningCharacter.ReceiveFlatDamage(damageAmount);
        }

        public bool ReceiveCollisionDamage(ForceData forceData)
        {
            if (!manager.OwningCharacter.ReceiveCollisionDamage(forceData)) return false;
            AddForce(forceData);
            return true;
        }

        public bool ReceiveProjectileDamage(float damageAmount, ForceData forceData)
        {
            if (!manager.OwningCharacter.ReceiveProjectileDamage(damageAmount, forceData)) return false;
            AddForce(forceData);
            return true;
        }

        public bool ReceiveExplosionDamage(float damagePercentage, ForceData forceData)
        {
            if (!manager.OwningCharacter.ReceiveExplosionDamage(damagePercentage, forceData)) return false;
            AddForce(forceData);
            return true;
        }

        public bool ReceiveMeleeDamage(float damageAmount, ForceData forceData)
        {
            if (!manager.OwningCharacter.ReceiveMeleeDamage(damageAmount, forceData)) return false;
            AddForce(forceData);
            AddForce(forceData);
            return true;
        }
    }


    public enum BodyPart : byte
    {
        None,
        Head,
        Arm,
        ForeArm,
        Spine,
        Hips,
        Leg,
        UpLeg,
    }

    public enum BodyDirection : byte
    {
        None,
        Left,
        Right,
    }

    [Serializable]
    public struct ForceData
    {
        [JsonProperty("force_mode")] public ForceMode forceMode;
        [JsonProperty("force_type")] public ForceType forceType;
        [JsonProperty("force")] public Vector3 force;
        [JsonProperty("position")] public Vector3 position;
        [JsonProperty("intensity")] public float intensity;
        [JsonProperty("radius")] public float radius;
        [JsonProperty("upwards_modifier")] public float upwardsModifier;
    }

    public enum ForceType : byte
    {
        Flat = 0,
        AtPosition = 1,
        Explosion = 2,
    }
}