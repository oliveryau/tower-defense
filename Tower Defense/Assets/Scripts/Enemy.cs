using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    public EnemyData GetEnemyData() => data;

    public static event Action<EnemyData> OnEnemyReachedEnd;
    public static event Action<Enemy> OnEnemyDestroyed;
    private bool _isRemoved = false;

    private Path _currentPath;
    private Vector3 _targetPosition;
    private int _currentWayPoint;

    private float _health;

    private void Awake()
    {
        _currentPath = FindFirstObjectByType<Path>();
    }

    private void OnEnable()
    {
        _currentWayPoint = 0;
        _targetPosition = _currentPath.GetPosition(_currentWayPoint);
        _health = data.health;
    }

    private void Update()
    {
        EnemyMovement();
    }

    private void EnemyMovement()
    {
        if (_isRemoved) return;

        // move towards target position
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, data.speed * Time.deltaTime);

        // set new target position when current target reached
        float relativeDistance = (transform.position - _targetPosition).magnitude;
        if (relativeDistance < 0.01f)
        {
            if (_currentWayPoint < _currentPath.Waypoints.Length - 1)
            {
                _currentWayPoint++;
                _targetPosition = _currentPath.GetPosition(_currentWayPoint);
            }
            else
            {
                _isRemoved = true;
                OnEnemyReachedEnd?.Invoke(data);
                gameObject.SetActive(false);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isRemoved) return;

        _health -= damage;
        _health = Math.Max(0, _health);

        if (_health <= 0)
        {
            _isRemoved = true;
            OnEnemyDestroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
