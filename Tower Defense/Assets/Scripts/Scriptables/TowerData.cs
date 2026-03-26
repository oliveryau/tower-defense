using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    public float range;
    public float shootInterval;
    public float projectileSpeed;
    public float projectileSize;
    public float projectileDuration;
    public float damage;

    [SerializeField] private EnemyPerks canHitPerks;

    public bool CanHitPerk(EnemyPerks perk) => (canHitPerks & perk) == perk;
}
