using Game.PhysicsRelated;

namespace Game.Interfaces
{
    public interface IDamageable
    {
        public bool ReceiveFlatDamage(float damageAmount);

        public bool ReceiveCollisionDamage(ForceData forceData);

        public bool ReceiveProjectileDamage(float damageAmount, ForceData forceData);

        public bool ReceiveExplosionDamage(float damagePercentage, ForceData forceData);

        public bool ReceiveMeleeDamage(float damageAmount, ForceData forceData);
    }
}