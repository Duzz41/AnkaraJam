using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Enemy prefab to spawn")]
    public GameObject enemyPrefab;

    [Tooltip("Spawn points for enemies")]
    public Transform[] spawnPoints;

    [Tooltip("Time between enemy spawns")]
    public float spawnInterval = 5f;

    [Tooltip("Maximum number of enemies to spawn")]
    public int maxEnemies = 10;

    [Tooltip("Should start spawning immediately?")]
    public bool autoStart = true;

    [Header("Debug")]
    [Tooltip("Should draw gizmos for spawn points?")]
    public bool showGizmos = true;

    // Internal variables
    private List<GameObject> _spawnedEnemies = new List<GameObject>();
    private bool _isSpawning = false;
    private Coroutine _spawnCoroutine;

    private void Start()
    {
        if (autoStart)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (!_isSpawning)
        {
            _isSpawning = true;
            _spawnCoroutine = StartCoroutine(SpawnEnemies());
        }
    }

    public void StopSpawning()
    {
        if (_isSpawning && _spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _isSpawning = false;
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (_isSpawning)
        {
            // Check if we can spawn more enemies
            if (_spawnedEnemies.Count < maxEnemies)
            {
                // Remove null entries (destroyed enemies)
                _spawnedEnemies.RemoveAll(enemy => enemy == null);

                // Spawn a new enemy at a random spawn point
                if (spawnPoints.Length > 0 && enemyPrefab != null)
                {
                    // Get random spawn point
                    Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    
                    // Spawn enemy
                    GameObject enemy = Instantiate(enemyPrefab, 
                                                  spawnPoint.position, 
                                                  spawnPoint.rotation);
                    
                    // Add to list
                    _spawnedEnemies.Add(enemy);
                    
                    Debug.Log($"Spawned enemy. Total: {_spawnedEnemies.Count}/{maxEnemies}");
                }
                else
                {
                    Debug.LogError("Missing spawn points or enemy prefab!");
                }
            }
            
            // Wait before next spawn
            yield return new WaitForSeconds(spawnInterval);
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