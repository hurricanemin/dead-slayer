using System;
using Object = UnityEngine.Object;

namespace Helpers.Utilities.PoolingSystem.PoolManagerBases
{
    public class PoolManagerBase<T0> where T0 : Object
    {
    }

    public abstract class PoolManagerRequestBase
    {
        public virtual Enum Category => null;
    }

// TODO EXAMPLE
// public class SsiRequestBase : PoolableGameObjectRequestBase
// {
//     public virtual ScreenSpaceIndicatorType ScreenSpaceIndicatorType => ScreenSpaceIndicatorType.Base;
//     public override Enum Category => null;
// }
//
// public class DeviceSsiRequest : SsiRequestBase
// {
//     public override ScreenSpaceIndicatorType ScreenSpaceIndicatorType => ScreenSpaceIndicatorType.Phone;
//     public override Enum Category => DeviceType;
//     public DeviceType DeviceType;
// }
//
// public enum DeviceType
// {
//     test,
//     mest
}