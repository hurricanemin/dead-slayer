namespace Game.Character.Controller.Interfaces
{
    public interface IGameCharacter : IPossessable
    {
        public GameCharacterStats GameCharacterStats { get; }
        public CharacterRuntimeStats CharacterRuntimeStats { get; }
        public GameCharacterSaveData GenerateSaveData();
        public void InitializeGameCharacter(GameCharacterSaveData characterSaveData);
    }
}