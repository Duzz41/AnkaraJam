using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public int numberOfEnemies;
        public float spawnInterval = 1f;
        public float waveInterval = 5f;  // Time before next wave starts
    }

    [Header("Wave Settings")]
    [Tooltip("List of waves with increasing difficulty")]
    public Wave[] waves;

    [Tooltip("Current wave index")]
    public int currentWaveIndex = 0;

    [Header("Spawn Settings")]
    [Tooltip("Enemy prefab to spawn")]
    public GameObject enemyPrefab;

    [Tooltip("Spawn points for enemies")]
    public Transform[] spawnPoints;

    [Header("Events")]
    [Tooltip("Event triggered when a wave starts")]
    public UnityEvent<int> onWaveStart;

    [Tooltip("Event triggered when a wave is completed")]
    public UnityEvent<int> onWaveCompleted;

    [Tooltip("Event triggered when all waves are completed")]
    public UnityEvent onAllWavesCompleted;

    [Header("Debug")]
    [Tooltip("Should draw gizmos for spawn points?")]
    public bool showGizmos = true;

    // Internal variables
    private List<GameObject> _activeEnemies = new List<GameObject>();
    private bool _isSpawning = false;
    private int _enemiesRemainingToSpawn = 0;
    private int _enemiesRemainingAlive = 0;
    private Coroutine _spawnCoroutine;

    // UI display properties
    public int CurrentWave => currentWaveIndex + 1;  // 1-based for display
    public int TotalWaves => waves.Length;
    public int RemainingEnemies => _enemiesRemainingAlive;

    private void Start()
    {
        // Start the first wave
        StartWave();
    }

    private void Update()
    {
        // Check if all enemies in the current wave are defeated AND all enemies are spawned already
        if (!_isSpawning && _enemiesRemainingToSpawn <= 0 && _enemiesRemainingAlive == 0 && currentWaveIndex < waves.Length)
        {
            // Wave completed - start the next wave with a delay
            StartCoroutine(StartNextWave());
        }
    }

    private IEnumerator StartNextWave()
    {
        // Trigger wave completed event
        onWaveCompleted?.Invoke(currentWaveIndex);

        // Check if there are more waves
        if (currentWaveIndex < waves.Length - 1)
        {
            // Wait for the interval between waves
            yield return new WaitForSeconds(waves[currentWaveIndex].waveInterval);
            
            // Move to next wave
            currentWaveIndex++;
            StartWave();
        }
        else
        {
            // All waves completed!
            Debug.Log("All waves completed!");
            onAllWavesCompleted?.Invoke();
        }
    }

    private void StartWave()
    {
        Debug.Log($"Starting Wave {CurrentWave}/{TotalWaves}");

        // Set up the wave
        Wave currentWave = waves[currentWaveIndex];
        _enemiesRemainingToSpawn = currentWave.numberOfEnemies;
        _enemiesRemainingAlive = currentWave.numberOfEnemies;

        // Trigger wave start event
        onWaveStart?.Invoke(currentWaveIndex);

        // Start spawning
        _isSpawning = true;
        _spawnCoroutine = StartCoroutine(SpawnEnemiesInWave(currentWave));
    }

    private IEnumerator SpawnEnemiesInWave(Wave wave)
    {
        // Wait a bit before first spawn
        yield return new WaitForSeconds(1f);

        while (_enemiesRemainingToSpawn > 0)
        {
            SpawnEnemy();
            _enemiesRemainingToSpawn--;

            // Wait for the next spawn
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        _isSpawning = false;
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Length > 0 && enemyPrefab != null)
        {
            // Get random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // Spawn enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Add component to track when enemy is destroyed
            EnemyWaveTracker tracker = enemy.AddComponent<EnemyWaveTracker>();
            tracker.waveSpawner = this;
            
            // Add to list
            _activeEnemies.Add(enemy);
            
            Debug.Log($"Spawned enemy in wave {CurrentWave}. Remaining: {_enemiesRemainingToSpawn-1}");
        }
        else
        {
            Debug.LogError("Missing spawn points or enemy prefab!");
        }
    }

    public void EnemyDefeated()
    {
        _enemiesRemainingAlive--;
        Debug.Log($"Enemy defeated! Remaining: {_enemiesRemainingAlive}");
    }

    // Simple component to track when an enemy is destroyed
    private class EnemyWaveTracker : MonoBehaviour
    {
        public WaveSpawner waveSpawner;

        private void OnDestroy()
        {
            if (waveSpawner != null)
            {
                waveSpawner.EnemyDefeated();
            }
        }
    }

    // Draw gizmos for spawn points
    private void OnDrawGizmos()
    {
        if (showGizmos && spawnPoints != null)
        {
            Gizmos.color = Color.blue;
            
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    // Draw a cube at spawn point
                    Gizmos.DrawWireCube(spawnPoint.position, new Vector3(1, 1, 1));
                    
                    // Draw a line from spawn point to this object
                    Gizmos.DrawLine(transform.position, spawnPoint.position);
                }
            }
        }
    }
}