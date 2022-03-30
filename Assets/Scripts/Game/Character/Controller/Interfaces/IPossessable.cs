using UnityEngine.InputSystem;

namespace Game.Character.Controller.Interfaces
{
    public interface IPossessable
    {
        public int InstanceID { get; }
        public PlayerInput InputComponent { get; }
        public IPossessor CurrentPossessor { get; }
        public bool OnPossessionAttempt(IPossessor possessor);
        public void HandleAction(InputAction.CallbackContext context);
    }

    public enum PossessorType : byte
    {
        Player,
        Ai,
    }
}