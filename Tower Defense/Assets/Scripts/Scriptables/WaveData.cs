using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public EnemyType EnemyType;
    public int count;
}

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    //public EnemyType EnemyType;
    public EnemySpawnData[] enemies;
    public float spawnInterval;
    //public int enemiesPerWave;
}
