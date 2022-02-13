using System;

namespace Helpers.Utilities.AutomatedFieldSystem.CustomAttributes
{
    /// <summary>
    /// Automatically sets the field when object is selected.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class AutomatedField : Attribute
    {
        public readonly SearchType SearchType;
        public readonly SearchIn SearchIn;
        public readonly string NameOverride;

        /// <param name="type">Search type</param>
        /// <param name="searchIn">Where to search this field</param>
        public AutomatedField(SearchIn searchIn, SearchType type)
        {
            SearchType = type;
            SearchIn = searchIn;
        }

        /// <param name="type">Search type</param>
        /// <param name="searchIn">Where to search this field</param>
        /// <param name="nameOverride">Overrides field name. Empty means field will be searched by its name</param>
        public AutomatedField(SearchIn searchIn, SearchType type, string nameOverride)
        {
            SearchType = type;
            SearchIn = searchIn;
            NameOverride = nameOverride;
        }
    }

    public enum SearchType : byte
    {
        FirstEncounter,

        /// <summary>
        /// Non-case sensitive.
        /// </summary>
        ByName,
    }

    public enum SearchIn : byte
    {
        /// <summary>
        /// Will be searched on components root object.
        /// </summary>
        Root,

        /// <summary>
        /// Will be searched on components children.
        /// </summary>
        Children,

        /// <summary>
        /// Will be searched on components parents.
        /// </summary>
        Parent,

        /// <summary>
        /// Will be searched in base-parents children.
        /// </summary>
        BaseParentsChildren,

        /// <summary>
        /// Will be searched on components active scene.
        /// </summary>
        CurrentScene,
    }
}