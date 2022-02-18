using Newtonsoft.Json;

namespace Helpers.Utilities.PoolingSystem.PoolManagerBases
{
    public class ObjectSaveData
    {
        [JsonProperty("transform_data")] public SerializedTransformData transformData;
    }
}