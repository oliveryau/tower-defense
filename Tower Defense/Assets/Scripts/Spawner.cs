using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static event Action<int> OnWaveChanged;

    [SerializeField] private WaveData[] waves;
    private int _currentWaveIndex = 0;
    private WaveData CurrentWave => waves[_currentWaveIndex];

    [SerializeField] private ObjectPooler titanPool;
    [SerializeField] private ObjectPooler stealthPool;
    [SerializeField] private ObjectPooler armoredPool;

    private Dictionary<EnemyType, ObjectPooler> _poolDictionary;

    private float _spawnCountdown;
    private int _spawnCounter;
    private int _totalEnemiesToSpawn;
    private int _enemiesRemoved;
    private float _timeBetweenWaves = 0.5f;
    private float _waveCountdown;
    private bool _isBetweenWaves = false;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>()
        {
            { EnemyType.Titan, titanPool },
            { EnemyType.Stealth, stealthPool },
            { EnemyType.Armored, armoredPool },
        };
    }

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
        OnWaveChanged?.Invoke(_currentWaveIndex);
        CalculateEnemiesToSpawn();
    }

    private void Update()
    {
        WaveLogic();
    }

    private void WaveLogic()
    {
        if (_isBetweenWaves)
        {
            _waveCountdown -= Time.deltaTime;
            if (_waveCountdown <= 0f) // buffer time before next wave
            {
                _currentWaveIndex++;
                if (_currentWaveIndex < waves.Length)
                {
                    OnWaveChanged?.Invoke(_currentWaveIndex); // update wave ui
                    CalculateEnemiesToSpawn();
                }
                _spawnCountdown = 0f;
                _spawnCounter = 0;
                _enemiesRemoved = 0;
                _isBetweenWaves = false;
            }
        }
        else
        {
            // normal spawning
            _spawnCountdown -= Time.deltaTime;
            if (_currentWaveIndex >= waves.Length) // stop spawning if no more waves
            {
                return;
            }
            else if (_spawnCountdown <= 0 && _spawnCounter < _totalEnemiesToSpawn)
            {
                _spawnCountdown = CurrentWave.spawnInterval;
                SpawnEnemy();
                _spawnCounter++;
            }
            else if (_spawnCounter >= _totalEnemiesToSpawn && _enemiesRemoved >= _totalEnemiesToSpawn) // checking if full wave spawned and dead
            {
                _isBetweenWaves = true;
                _waveCountdown = _timeBetweenWaves;
            }
        }
    }

    private void CalculateEnemiesToSpawn()
    {
        _totalEnemiesToSpawn = 0;
        foreach (var enemySpawn in CurrentWave.enemies)
        {
            _totalEnemiesToSpawn += enemySpawn.count;
        }
    }

    private void SpawnEnemy()
    {
        EnemyType enemyType = GetNextEnemyType();

        if (_poolDictionary.TryGetValue(enemyType, out var pool))
        {
            GameObject spawnedEnemy = pool.GetPooledObject();
            spawnedEnemy.transform.position = transform.position;
            spawnedEnemy.SetActive(true);
        }
    }

    private EnemyType GetNextEnemyType()
    {
        int currentCount = 0;

        foreach (var enemySpawn in CurrentWave.enemies)
        {
            if (_spawnCounter < currentCount + enemySpawn.count)
            {
                return enemySpawn.EnemyType; // gets enemy type here
            }
            currentCount += enemySpawn.count;
        }

        return CurrentWave.enemies[0].EnemyType; // fallback, should not reach here
    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _enemiesRemoved++;
    }
}
