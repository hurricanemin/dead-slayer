using UnityEngine.InputSystem;

namespace Game.Character.Controller.Interfaces
{
    public interface IPossessable
    {
        public void HandleAction(InputAction.CallbackContext context);
    }
}