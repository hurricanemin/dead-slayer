using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using UnityEngine;

namespace Helpers.AutoObject
{
    public abstract class AutoObject : MonoBehaviour
    {
        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        protected Transform objectTransform;
        
        protected bool areReferencesSet;
    }
}
