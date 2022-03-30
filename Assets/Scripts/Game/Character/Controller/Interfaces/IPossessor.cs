namespace Game.Character.Controller.Interfaces
{
    public interface IPossessor
    {
        public PossessorType possessorType { get; }
        public IPossessable possessedObject { get; }
        public void PossessCharacter(IPossessable gameCharacter);
    }
}