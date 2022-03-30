using Game.Character.Movement;
using Game.Interfaces;

namespace Game.Character.Controller.Interfaces
{
    public interface IGameCharacter : IPossessable, IDamageable
    {
        public GameCharacterStats GameCharacterStats { get; }
        public CharacterBodyBase CharacterBodyBase { get; }
        public GameCharacterSaveData GenerateSaveData();
        public void InitializeGameCharacter(GameCharacterSaveData characterSaveData);
    }
}