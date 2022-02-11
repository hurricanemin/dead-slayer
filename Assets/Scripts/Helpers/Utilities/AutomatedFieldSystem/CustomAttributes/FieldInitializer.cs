using System;

namespace Helpers.Utilities.AutomatedFieldSystem.CustomAttributes
{
    /// <summary>
    /// Invokes the method when this object is selected.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class FieldInitializer : Attribute
    {
    }
}