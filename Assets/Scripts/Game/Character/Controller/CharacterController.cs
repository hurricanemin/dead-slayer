using UnityEngine.InputSystem;

namespace Game.Character.Controller
{
    public sealed class CharacterController
    {
        private GameCharacter possessedGameCharacter;

        public void PossessCharacter(GameCharacter gameCharacter)
        {
            // if (gameCharacter == null) return; // TODO Free roam?
            if (possessedGameCharacter != null)
                if (possessedGameCharacter.GetInstanceID() != gameCharacter.GetInstanceID())
                    possessedGameCharacter.InputComponent.onActionTriggered -= HandleAction;
            possessedGameCharacter = gameCharacter;
            gameCharacter.InputComponent.onActionTriggered += HandleAction;
        }

        private void HandleAction(InputAction.CallbackContext context)
        {
            possessedGameCharacter.HandleAction(context);
        }
    }
}