using Game.Character.Controller.Interfaces;
using UnityEngine.InputSystem;

namespace Game.Character.Controller
{
    public sealed class CharacterController : IPossessor
    {
        public PossessorType possessorType => PossessorType.Player;
        public IPossessable possessedObject { get; private set; }

        public void PossessCharacter(IPossessable gameCharacter)
        {
            // if (gameCharacter == null) return; // TODO Free roam?
            if (!gameCharacter.OnPossessionAttempt(this)) return;
            if (possessedObject != null)
                if (possessedObject.InstanceID != gameCharacter.InstanceID)
                    possessedObject.InputComponent.onActionTriggered -= HandleAction;
            possessedObject = gameCharacter;
            possessedObject.InputComponent.onActionTriggered += HandleAction;
        }

        private void HandleAction(InputAction.CallbackContext context)
        {
            possessedObject.HandleAction(context);
        }
    }
}