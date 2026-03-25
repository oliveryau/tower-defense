using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private WaveData[] waves;
    private int _currentWaveIndex = 0;
    private WaveData CurrentWave => waves[_currentWaveIndex];

    [SerializeField] private ObjectPooler titanPool;
    [SerializeField] private ObjectPooler stealthPool;
    [SerializeField] private ObjectPooler armoredPool;

    private Dictionary<EnemyType, ObjectPooler> _poolDictionary;

    private float _spawnTimer;
    private float _spawnCounter;
    private int _enemiesRemoved;
    private float _timeBetweenWaves = 0.5f;
    private float _waveCooldown;
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

    private void Update()
    {
        if (_isBetweenWaves)
        {
            _waveCooldown -= Time.deltaTime;
            if (_waveCooldown <= 0f) // buffer time before next wave
            {
                _currentWaveIndex++;
                _spawnTimer = 0f;
                _spawnCounter = 0;
                _enemiesRemoved = 0;
                _isBetweenWaves = false;
            }
        }
        else
        {
            // normal spawning
            _spawnTimer -= Time.deltaTime;
            if (_currentWaveIndex >= waves.Length) // stop spawning if no more waves
            {
                return;
            }
            else if (_spawnTimer <= 0 && _spawnCounter < CurrentWave.enemiesPerWave)
            {
                _spawnTimer = CurrentWave.spawnInterval;
                SpawnEnemy();
                _spawnCounter++;
            }
            else if (_spawnCounter >= CurrentWave.enemiesPerWave && _enemiesRemoved >=
                CurrentWave.enemiesPerWave) // checking if full wave spawned and dead
            {
                _isBetweenWaves = true;
                _waveCooldown = _timeBetweenWaves;
            }
        }
    }

    private void SpawnEnemy()
    {
        if (_poolDictionary.TryGetValue(CurrentWave.EnemyType, out var pool))
        {
            GameObject spawnedEnemy = pool.GetPooledObject();
            spawnedEnemy.transform.position = transform.position;
            spawnedEnemy.SetActive(true);
        }
    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _enemiesRemoved++;
    }
}
