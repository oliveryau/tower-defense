using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private float _spawnTimer;
    private float _spawnInterval = 1f;

    private ObjectPooler _objectPooler;

    private void Awake()
    {
        _objectPooler = FindFirstObjectByType<ObjectPooler>();
    }

    private void Update()
    {
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0)
        {
            _spawnTimer = _spawnInterval;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        GameObject spawnedEnemy = _objectPooler.GetPooledObject();
        spawnedEnemy.transform.position = transform.position;
        spawnedEnemy.SetActive(true);
    }
}
