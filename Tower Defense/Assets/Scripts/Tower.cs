using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;
    public TowerData GetTowerData() => data;

    private CircleCollider2D _circleCollider;
    private List<Enemy> _enemiesInRange;
    private Enemy targetEnemy;
    private ObjectPooler _projectilePool;

    private float _shootCountdown;

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _circleCollider.radius = data.range;
        _enemiesInRange = new List<Enemy>();
        _projectilePool = GetComponent<ObjectPooler>();
        _shootCountdown = data.shootInterval;
    }

    private void Update()
    {
        _shootCountdown -= Time.deltaTime;
        if (_shootCountdown <= 0)
        {
            _shootCountdown = data.shootInterval;
            Shoot();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, data.range);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            _enemiesInRange.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Remove(enemy);
            }
        }
    }

    private void Shoot()
    {
        _enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy); // ensure tower doesn't keep shooting at deactivated enemy
        targetEnemy = GetTargetEnemy();

        if (targetEnemy != null)
        {
            GameObject projectile = _projectilePool.GetPooledObject();
            projectile.transform.position = transform.position;
            projectile.SetActive(true);
            Vector2 _shootDirection = (targetEnemy.transform.position - transform.position).normalized;
            projectile.GetComponent<Projectile>().Shoot(data, _shootDirection);
        }
    }

    private Enemy GetTargetEnemy()
    {
        foreach (Enemy enemy in _enemiesInRange)
        {
            if (CanSeeEnemy(enemy)) // checking if tower can see stealth enemies
            {
                return enemy;
            }
        }
        return null; // no valid target if can't hit enemy
    }

    private bool CanSeeEnemy(Enemy enemy)
    {
        EnemyData enemyData = enemy.GetComponent<Enemy>().GetEnemyData();

        if (enemyData.HasPerk(EnemyPerks.Stealth) && !data.CanHitPerk(EnemyPerks.Stealth))
            return false; // cannot hit stealth enemies

        return true; // Tower can see this enemy
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesInRange.Remove(enemy);
    }
}
