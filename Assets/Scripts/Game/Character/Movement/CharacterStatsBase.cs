using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Character.Movement
{
    [Serializable]
    public class CharacterStatsBase
    {
        [SerializeField] [JsonProperty("max_hp")]
        private float maxHp;

        [SerializeField] [JsonProperty("movement_speed")]
        private float movementSpeed;

        [SerializeField] [JsonProperty("jump_force")]
        private float jumpForce;

        public float MaxHp => maxHp;
        public float MovementSpeed => movementSpeed;
        public float JumpForce => jumpForce;
    }
}