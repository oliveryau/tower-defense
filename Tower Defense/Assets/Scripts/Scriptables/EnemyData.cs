using UnityEngine;

[System.Flags]
public enum EnemyPerks
{
    Default = 0,
    Stealth = 1 << 0, // 0001
    Armored = 1 << 1  // 0010
}

[System.Flags]
public enum EnemyAbility
{
    None = 0,
    Split = 1 << 0,
    Healing = 1 << 1
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float health;
    public float damage;
    public float speed;
    public int manaReward;

    [SerializeField] private EnemyPerks perks; // stores perks using binary
    [SerializeField] private EnemyAbility abilities; // stores abilities using binary

    public bool HasPerk(EnemyPerks perk) => (perks & perk) == perk; // checks if enemy has ability and sets it
}
