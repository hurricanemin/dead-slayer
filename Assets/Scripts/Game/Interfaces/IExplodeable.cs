namespace Game.Interfaces
{
    public interface IExplodeable
    {
        void OnExploded();
        void SetColliders(bool isActive);
    }
}