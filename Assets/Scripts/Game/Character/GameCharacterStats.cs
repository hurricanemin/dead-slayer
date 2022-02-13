using System;
using Helpers.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Character
{
    [Serializable]
    public class GameCharacterStats
    {
        [SerializeField] [JsonProperty("max_hp")]
        private float maxHp;

        [SerializeField] [JsonProperty("hp_regeneration_rate")]
        private float hpRegenerationRate;

        [SerializeField] [JsonProperty("movement_speed")]
        private float movementSpeed;

        [SerializeField] [JsonProperty("jump_force")]
        private float jumpForce;

        public float MaxHp => maxHp;
        public float MovementSpeed => movementSpeed;
        public float JumpForce => jumpForce;
    }

    public struct CharacterRuntimeStats // TODO
    {
        public float currentHp;
    }

    public class GameCharacterSaveData
    {
        // TODO Some kind of id?
        [JsonProperty("runtime_stats")] public CharacterRuntimeStats characterRuntimeStats;
        [JsonProperty("transform_data")] public SerializedTransformData transformData;
    }
    
    public enum GameCharacterType : byte
    {
        Custom,
        Player, // TODO 
    }
}