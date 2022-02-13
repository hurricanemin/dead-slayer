using Game.Character.Controller.Interfaces;
using Game.Character.Movement;
using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Character.Controller
{
    [RequireComponent(typeof(CharacterBodyBase))]
    public class CharacterController : MonoBehaviour, IGameCharacter
    {
        public GameCharacterStats GameCharacterStats => gameCharacterStats;
        public CharacterRuntimeStats CharacterRuntimeStats { get; private set; }

        public GameCharacterSaveData GenerateSaveData()
        {
            return new GameCharacterSaveData
            {
            };
        }

        public void InitializeGameCharacter(GameCharacterSaveData characterSaveData)
        {
            CharacterRuntimeStats = characterSaveData.characterRuntimeStats;
        }

        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        private CharacterBodyBase characterBody;

        [SerializeField] [HideInInspector] private GameCharacterStats gameCharacterStats;

        public void OnMovement(InputValue input)
        {
            Vector2 inputVec = input.Get<Vector2>() * gameCharacterStats.MovementSpeed;

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

        public void OnJump()
        {
            switch (characterBody.MovementType)
            {
                case CharacterBodyBase.CharacterMovementType.Air:
                    //TODO Double jump etc.
                    return;
                case CharacterBodyBase.CharacterMovementType.Ground:
                    characterBody.Jump(gameCharacterStats.JumpForce);
                    break;
                case CharacterBodyBase.CharacterMovementType.Water:
                    characterBody.ApplyMovementInput(Vector3.up * gameCharacterStats.MovementSpeed / 4);
                    // TODO
                    break;
            }
        }
    }
}