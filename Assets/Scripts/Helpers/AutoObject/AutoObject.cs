using Helpers.AutoObject.Interfaces;
using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using UnityEngine;

namespace Helpers.AutoObject
{
    public abstract class AutoObject : MonoBehaviour, IAutoObject
    {
        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        public Transform objectTransform;

        [SerializeField] [HideInInspector] protected bool areReferencesSet;

        public int InstanceID => this.GetInstanceID();
        public Transform ObjectTransform => objectTransform;
    }
}