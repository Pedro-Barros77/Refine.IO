public class Enums
{
    public enum MeleeTargetType
    {
        /// <summary>
        /// This is not a target for melee weapons.
        /// </summary>
        None,
        /// <summary>
        /// An enemy without armor/on foot.
        /// </summary>
        Enemy,
        /// <summary>
        /// An armoured enemy/tank.
        /// </summary>
        EnemyTank,
        /// <summary>
        /// A rock/stone.
        /// </summary>
        Stone,
        /// <summary>
        /// A tree/wood.
        /// </summary>
        Wood
    }

    public enum MeleeWeaponType
    {
        Fists,
        Axe,
        Pickaxe
    }
}
