using Game.Character.Controller.Interfaces;
using Game.Character.Movement;
using Game.PhysicsRelated;
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
        public int InstanceID => this.GetInstanceID();
        public PlayerInput InputComponent => inputComponent;
        public IPossessor CurrentPossessor { get; private set; }

        public CharacterRuntimeStats CharacterRuntimeStats => characterRuntimeStats;
        private CharacterRuntimeStats characterRuntimeStats;

        private Vector3 movementForce;
        private bool isMoving;

        private void FixedUpdate()
        {
            if (!isMoving) return;
            characterBody.ApplyMovementInput(movementForce);
        }

        public void InitializeGameCharacter(GameCharacterSaveData characterSaveData)
        {
            characterRuntimeStats = characterSaveData.characterRuntimeStats;
            characterSaveData.transformData.SetTransformData(characterBody.objectTransform);
        }

        public GameCharacterSaveData GenerateSaveData()
        {
            return new GameCharacterSaveData
            {
                transformData = new SerializedTransformData(characterBody.objectTransform),
                characterRuntimeStats = characterRuntimeStats
            };
        }

        private void OnMovementTest(Vector2 input)
        {
            Vector3 candidateInput = characterBody.objectTransform.right * input.x +
                                     characterBody.objectTransform.forward * input.y;
            Vector3 currentVelocity = characterBody.Rigidbody.velocity;

            if (currentVelocity.magnitude > gameCharacterStats.MovementSpeed)
            {
                // TODO
            }

            switch (characterBody.MovementState)
            {
                case CharacterBodyBase.BodyMovementState.Air:
                case CharacterBodyBase.BodyMovementState.Ground:
                    if (characterBody.MovementState == CharacterBodyBase.BodyMovementState.Air) candidateInput /= 2;
                    break;
                case CharacterBodyBase.BodyMovementState.Water:
                    // TODO
                    break;
            }

            movementForce = candidateInput;
        }

        private void OnJumpTest()
        {
            switch (characterBody.MovementState)
            {
                case CharacterBodyBase.BodyMovementState.Air:
                    //TODO Double jump etc.
                    return;
                case CharacterBodyBase.BodyMovementState.Ground:
                    characterBody.ApplyForce(characterBody.objectTransform.up * gameCharacterStats.JumpForce,
                        ForceMode.VelocityChange);
                    break;
                case CharacterBodyBase.BodyMovementState.Water:
                    characterBody.ApplyForce(Vector3.up * gameCharacterStats.MovementSpeed / 4,
                        ForceMode.VelocityChange);
                    break;
            }
        }

        public bool OnPossessionAttempt(IPossessor possessor)
        {
            CurrentPossessor = possessor;
            return true; // TODO Check if possessable.
        }

        public virtual void HandleAction(InputAction.CallbackContext context)
        {
            InputAction inputAction = context.action;
            InputActionPhase actionPhase = inputAction.phase;

            switch (inputAction.name)
            {
                case "Movement":
                    isMoving = actionPhase == InputActionPhase.Performed;
                    Vector2 input = inputAction.ReadValue<Vector2>();
                    OnMovementTest(input);
                    break;
                case "Jump":
                    if (actionPhase != InputActionPhase.Performed) return;
                    OnJumpTest();
                    break;
                case "Crouch":
                    switch (actionPhase)
                    {
                        case InputActionPhase.Performed:

                            break;
                        case InputActionPhase.Canceled:

                            break;
                    }

                    break;
                case "Sprint":

                    break;
                case "Interact":

                    break;
                case "Fire":

                    break;
                case "Reload":

                    break;
            }
        }

        public bool ReceiveFlatDamage(float damageAmount)
        {
            characterRuntimeStats.currentHp = Mathf.Clamp(characterRuntimeStats.currentHp - damageAmount, 0,
                gameCharacterStats.MaxHp);
            return IsAlive();
        }

        public bool ReceiveCollisionDamage(ForceData forceData)
        {
            characterRuntimeStats.currentHp = Mathf.Clamp(
                characterRuntimeStats.currentHp - forceData.force.sqrMagnitude, 0, gameCharacterStats.MaxHp); // TODO
            return IsAlive();
        }

        public bool ReceiveProjectileDamage(float damageAmount, ForceData forceData)
        {
            characterRuntimeStats.currentHp = Mathf.Clamp(characterRuntimeStats.currentHp - damageAmount, 0,
                gameCharacterStats.MaxHp);
            return IsAlive();
        }

        public bool ReceiveExplosionDamage(float damagePercentage, ForceData forceData)
        {
            characterRuntimeStats.currentHp = Mathf.Clamp(
                characterRuntimeStats.currentHp - gameCharacterStats.MaxHp * damagePercentage, 0,
                gameCharacterStats.MaxHp);
            return IsAlive();
        }

        public bool ReceiveMeleeDamage(float damageAmount, ForceData forceData)
        {
            characterRuntimeStats.currentHp = Mathf.Clamp(characterRuntimeStats.currentHp - damageAmount, 0,
                gameCharacterStats.MaxHp);
            return IsAlive();
        }

        public bool IsAlive()
        {
            return characterRuntimeStats.currentHp <= 0;
        }
    }
}