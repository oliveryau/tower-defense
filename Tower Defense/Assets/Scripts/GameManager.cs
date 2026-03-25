using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action<int> OnHealthChanged;

    private int _health = 50;

    private void OnEnable()
    {
        Enemy.onEnemyReachedEnd += HandleEnemyReachedEnd;
    }

    private void OnDisable()
    {
        Enemy.onEnemyReachedEnd -= HandleEnemyReachedEnd;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(_health);
    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _health = Mathf.Max(0, _health - data.damage);
        OnHealthChanged?.Invoke(_health);
    }
}
