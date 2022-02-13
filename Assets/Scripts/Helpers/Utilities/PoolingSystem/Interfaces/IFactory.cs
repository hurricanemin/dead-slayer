namespace Helpers.Utilities.PoolingSystem.Interfaces
{
    public interface IFactory
    {
    }

    public interface IFactory<out TOutput> : IFactory
    {
        TOutput GetObject();
    }
}