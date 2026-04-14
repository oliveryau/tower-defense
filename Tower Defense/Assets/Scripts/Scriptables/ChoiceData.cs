using UnityEngine;

public enum ChoiceType
{
    TowerCapacity,
    TowerUpgrade,
    HeroUpgrade, // future
    HeroAbility  // future
}

public enum ChoiceRarity
{
    Normal,
    Rare,
    Epic,
    Legend
}

[CreateAssetMenu(fileName = "ChoiceData", menuName = "Scriptable Objects/ChoiceData")]
public class ChoiceData : ScriptableObject
{
    [Header("UI")]
    public string id;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public ChoiceRarity ChoiceRarity;

    [Header("Core")]
    public ChoiceType ChoiceType;
    [Range(1, 100)] public int weight = 10;
    public int maxStacks = 1;

    [Header("Tower Upgrade")]
    public TowerData towerData;
    public int towerCapacity;
    public float bonusDamage;
    public float bonusRange;
    [Tooltip("10 means 10% faster attack rate")]
    public float bonusAttackSpeedPercent;
}