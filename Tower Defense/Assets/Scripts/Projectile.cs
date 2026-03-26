using UnityEngine;

public class Projectile : MonoBehaviour
{
    private TowerData _data;
    private Vector3 _shootDirection;
    private float _projectileDuration;

    private void Start()
    {
        transform.localScale = Vector3.one * _data.projectileSize;
    }

    private void Update()
    {
        ProjectileLogic();
    }

    public void Shoot(TowerData data, Vector3 shootDirection)
    {
        _data = data;
        _shootDirection = shootDirection;
        _projectileDuration = data.projectileDuration;
    }

    private void ProjectileLogic()
    {
        if (_projectileDuration <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            _projectileDuration -= Time.deltaTime;
            transform.position += new Vector3(_shootDirection.x, _shootDirection.y) * _data.projectileSpeed * Time.deltaTime;
        }
    }

    private bool CanDamageEnemy(EnemyData enemyData)
    {
        if (enemyData.HasPerk(EnemyPerks.Armored) && !_data.CanHitPerk(EnemyPerks.Armored))
            return false; // cannot damage armored enemies

        return true; // can damage enemies
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            EnemyData enemyData = enemy.GetEnemyData();
            float damage = CanDamageEnemy(enemyData) ? _data.damage : 0f;
            enemy.TakeDamage(damage);
            gameObject.SetActive(false);
        }
    }
}
