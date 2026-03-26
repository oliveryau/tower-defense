using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action<int> OnHealthChanged;
    public static event Action<int> OnManaChanged;

    private int _health = 50;
    private int _mana = 0;

    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(_health);
        OnManaChanged?.Invoke(_mana);
    }

    private void AddMana(int amount) 
    {
        _mana += amount;
        OnManaChanged?.Invoke(_mana);
    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _health = Mathf.Max(0, _health - data.damage);
        OnHealthChanged?.Invoke(_health);
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        AddMana(enemy.GetEnemyData().manaReward);
    }
}
