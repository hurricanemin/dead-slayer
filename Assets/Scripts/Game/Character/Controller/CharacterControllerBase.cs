using Game.Character.Movement;
using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Character.Controller
{
    [RequireComponent(typeof(CharacterBodyBase))]
    public abstract class CharacterControllerBase : MonoBehaviour
    {
        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        private CharacterBodyBase characterBody;

        [SerializeField] [HideInInspector] private CharacterStatsBase characterStats;

        private void OnMovement(InputValue input)
        {
            Vector2 inputVec = input.Get<Vector2>() * characterStats.MovementSpeed;

            switch (characterBody.MovementType)
            {
                case CharacterBodyBase.CharacterMovementType.Air:
                    inputVec /= 2;
                    characterBody.ApplyMovementInput(new Vector3(inputVec.x, 0, inputVec.y));
                    break;
                case CharacterBodyBase.CharacterMovementType.Ground:
                    characterBody.ApplyMovementInput(new Vector3(inputVec.x, 0, inputVec.y));
                    break;
                case CharacterBodyBase.CharacterMovementType.Water:
                    // TODO
                    break;
            }
        }

        private void OnJump()
        {
            switch (characterBody.MovementType)
            {
                case CharacterBodyBase.CharacterMovementType.Air:
                    //TODO Double jump etc.
                    return;
                case CharacterBodyBase.CharacterMovementType.Ground:
                    characterBody.Jump(characterStats.JumpForce);
                    break;
                case CharacterBodyBase.CharacterMovementType.Water:
                    characterBody.ApplyMovementInput(Vector3.up * characterStats.MovementSpeed / 4);
                    // TODO
                    break;
            }
        }
    }
}