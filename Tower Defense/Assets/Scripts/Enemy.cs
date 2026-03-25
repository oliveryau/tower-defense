using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    public static event Action<EnemyData> onEnemyReachedEnd;

    private Vector3 _targetPosition;
    private int _currentWayPoint;

    private Path _currentPath;

    private void Awake()
    {
        _currentPath = FindFirstObjectByType<Path>();
    }

    private void OnEnable()
    {
        _currentWayPoint = 0;
        _targetPosition = _currentPath.GetPosition(_currentWayPoint);
    }

    private void Update()
    {
        // Move towards target position
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition,
            data.speed * Time.deltaTime);

        // Set new target position when current target reached
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
                onEnemyReachedEnd?.Invoke(data);
                gameObject.SetActive(false);
            }
        }
    }
}
