

using UnityEngine.InputSystem;

namespace Game.Character.Controller.Interfaces
{
    public interface IPossessable
    {
        public void OnMovement(InputValue input);
        public void OnJump();
    }
}
