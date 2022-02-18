using Game.Character.Controller.Interfaces;
using Game.Character.Movement;
using Helpers.Utilities;
using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Character.Controller
{
    [RequireComponent(typeof(CharacterBodyBase), typeof(PlayerInput))]
    public abstract class GameCharacter : MonoBehaviour, IGameCharacter
    {
        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        private PlayerInput inputComponent;

        [SerializeField] [AutomatedField(SearchIn.Root, SearchType.FirstEncounter)]
        private CharacterBodyBase characterBody;

        [SerializeField] private GameCharacterStats gameCharacterStats;
        public CharacterBodyBase CharacterBodyBase => characterBody;
        public GameCharacterStats GameCharacterStats => gameCharacterStats;
        public PlayerInput InputComponent => inputComponent;
        public CharacterRuntimeStats CharacterRuntimeStats { get; private set; }

        public void InitializeGameCharacter(GameCharacterSaveData characterSaveData)
        {
            CharacterRuntimeStats = characterSaveData.characterRuntimeStats;
            characterSaveData.transformData.SetTransformData(characterBody.objectTransform);
        }

        public GameCharacterSaveData GenerateSaveData()
        {
            return new GameCharacterSaveData
            {
                transformData = new SerializedTransformData(characterBody.objectTransform),
                characterRuntimeStats = CharacterRuntimeStats
            };
        }

        private void OnMovement(Vector2 input)
        {
            Vector3 candidateInput = characterBody.objectTransform.right * input.x +
                                     characterBody.objectTransform.forward * input.y;
            Vector3 currentVelocity = characterBody.Rigidbody.velocity;

            if (currentVelocity.magnitude > gameCharacterStats.MovementSpeed)
            {
                // TODO
            }

            switch (characterBody.MovementType)
            {
                case CharacterBodyBase.CharacterMovementType.Air:
                case CharacterBodyBase.CharacterMovementType.Ground:
                    if (characterBody.MovementType == CharacterBodyBase.CharacterMovementType.Air) candidateInput /= 2;
                    break;
                case CharacterBodyBase.CharacterMovementType.Water:
                    // TODO
                    break;
            }

            characterBody.SetMovementInput(candidateInput);
        }

        private void OnJump()
        {
            switch (characterBody.MovementType)
            {
                case CharacterBodyBase.CharacterMovementType.Air:
                    //TODO Double jump etc.
                    return;
                case CharacterBodyBase.CharacterMovementType.Ground:
                    characterBody.Jump(gameCharacterStats.JumpForce);
                    // characterBody.BodyRb.AddForce(characterBody.objectTransform.up * gameCharacterStats.JumpForce, ForceMode.VelocityChange);
                    break;
                case CharacterBodyBase.CharacterMovementType.Water:
                    characterBody.ApplyForce(Vector3.up * gameCharacterStats.MovementSpeed / 4,
                        ForceMode.VelocityChange);
                    break;
            }
        }

        public virtual void HandleAction(InputAction.CallbackContext context)
        {
            switch (context.action.name)
            {
                case "Jump":
                    OnJump();
                    break;
                case "Movement":
                    Vector2 input = context.action.ReadValue<Vector2>();
                    OnMovement(input);
                    break;
            }
        }
    }
}