using Game.Character.Movement;
using UnityEngine.InputSystem;

namespace Game.Character.Controller.Interfaces
{
    public interface IGameCharacter : IPossessable
    {
        public PlayerInput InputComponent { get; }
        public CharacterBodyBase CharacterBodyBase { get; }
        public GameCharacterSaveData GenerateSaveData();
        public void InitializeGameCharacter(GameCharacterSaveData characterSaveData);
    }
}