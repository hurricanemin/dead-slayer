using System.Linq;
using Game.Interfaces;
using Helpers.Utilities;
using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.PhysicsRelated
{
    public class RagdollManager : MonoBehaviour
    {
        [SerializeField] [AutomatedField(SearchIn.Children, SearchType.FirstEncounter)]
        private Animator animator;

        [SerializeField] [AutomatedField(SearchIn.Children, SearchType.FirstEncounter)]
        private RagdollJointBase[] ragdollJoints;

        [SerializeField] private int jointCount;
        [SerializeField] [HideInInspector] private float currentBodyWeight = 75;

        public IDamageable OwningCharacter { get; private set; }

        private void Awake()
        {
            OwningCharacter = this.GetComponentInParent<IDamageable>();
        }
        
        public Animator Animator
        {
            get => animator;
            private set => animator = value;
        }
        
        public RagdollJointBase[] BodyJoints => ragdollJoints;

        public void ToggleRagdoll(bool isActive)
        {
            animator.enabled = !isActive;
            for (int i = 0; i < jointCount; i++) ragdollJoints[i].SetActive(isActive);
        }

        public void ApplyForce(ForceData forceData, BodyPart part, BodyDirection bodyDirection)
        {
            RagdollJointBase jointBase = ReturnBodyJoint(part, bodyDirection);
            if (jointBase != null) jointBase.AddForce(forceData);
        }

        public Transform ReturnBodyPart(BodyPart bodyPart, BodyDirection bodyDirection)
        {
            RagdollJointBase part = ReturnBodyJoint(bodyPart, bodyDirection);
            return part == null ? null : part.transform;
        }

        public Transform[] ReturnBodyParts()
        {
            Transform[] bodyParts = new Transform[jointCount];
            for (int i = 0; i < jointCount; i++) bodyParts[i] = ragdollJoints[i].transform;
            return bodyParts;
        }

        public RagdollJointBase ReturnBodyJoint(BodyPart bodyPart, BodyDirection bodyDirection)
        {
            RagdollJointBase candidateJointBase =
                ragdollJoints.FirstOrDefault(x => x.BodyPartType == bodyPart && x.bodyDirection == bodyDirection);
            if (candidateJointBase != null) return candidateJointBase;
            Debug.LogError("Couldn't found body part [{0} {1}]!".Format(bodyDirection, bodyPart));
            return null;
        }

        public void InitializeVariables()
        {
            jointCount = ragdollJoints.Length;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this.gameObject);
#endif
        }
    }
}