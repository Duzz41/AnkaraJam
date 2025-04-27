using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; // Ana menüye dönmek için gerekli
using UnityEngine.UI; // Image için gerekli

public class EnhancedWaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public string enemyName;
        public GameObject enemyPrefab;
        [Tooltip("Weight for random selection, higher = more likely to spawn")]
        public int spawnWeight = 1;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        [Tooltip("Total number of enemies in this wave")]
        public int totalEnemies;
        
        [Header("Enemy Type Ratios")]
        [Tooltip("How many melee enemies to spawn (0 = none)")]
        public int meleeEnemyCount;
        [Tooltip("How many ranged enemies to spawn (0 = none)")]
        public int rangedEnemyCount;
        
        [Header("Timing")]
        [Tooltip("Time between enemy spawns")]
        public float spawnInterval = 1f;
        [Tooltip("Time before next wave starts")]
        public float waveInterval = 5f;
    }

    [Header("Wave Settings")]
    [Tooltip("List of waves with increasing difficulty")]
    public Wave[] waves;

    [Tooltip("Current wave index")]
    public int currentWaveIndex = 0;

    [Header("Enemy Types")]
    [Tooltip("Melee enemy prefab")]
    public GameObject meleeEnemyPrefab;
    
    [Tooltip("Ranged enemy prefab")]
    public GameObject rangedEnemyPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Spawn points for enemies")]
    public Transform[] spawnPoints;
    
    [Tooltip("Preferred spawn points for ranged enemies (if empty, will use regular spawn points)")]
    public Transform[] rangedSpawnPoints;

    [Header("Win Screen")]
    [Tooltip("Kazandınız görseli")]
    public GameObject winScreen;
    
    [Tooltip("Ana menü sahne adı")]
    public string mainMenuSceneName = "MainMenu";

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
    private int _meleeEnemiesToSpawn = 0;
    private int _rangedEnemiesToSpawn = 0;
    private int _enemiesRemainingAlive = 0;
    private Coroutine _spawnCoroutine;
    private bool _isTransitioning = false; // Yeni flag

    // UI display properties
    public int CurrentWave => currentWaveIndex + 1;  // 1-based for display
    public int TotalWaves => waves.Length;
    public int RemainingEnemies => _enemiesRemainingAlive;

    private void Start()
    {
        // Start the first wave
        StartWave();
        
        // Başlangıçta kazandınız ekranını gizle
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
    }

    private void Update()
    {
        // Wave tamamlanma kontrolü: spawn işlemi bitmiş mi, tüm düşmanlar spawn edilmiş mi ve tüm düşmanlar ölmüş mü?
        if (!_isSpawning && !_isTransitioning && _meleeEnemiesToSpawn == 0 && _rangedEnemiesToSpawn == 0 && _enemiesRemainingAlive == 0 && currentWaveIndex < waves.Length)
        {
            // Güvenlik kontrolü: Aktif düşmanları tekrar kontrol et
            _activeEnemies.RemoveAll(enemy => enemy == null);
            
            // Eğer hala aktif düşman yoksa, sonraki dalgaya geç
            if (_activeEnemies.Count == 0)
            {
                // Geçiş başladı, başka geçiş başlatma
                _isTransitioning = true;
                // Wave tamamlandı - bir sonraki dalgayı başlat
                StartCoroutine(StartNextWave());
            }
        }
    }

    private IEnumerator StartNextWave()
    {
        // Trigger wave completed event
        onWaveCompleted?.Invoke(currentWaveIndex);

        // Check if there are more waves
        if (currentWaveIndex < waves.Length - 1)
        {
            // Bir sonraki dalgaya geçmeden önce, tüm düşmanların gerçekten öldüğünden emin ol
            while (_activeEnemies.Count > 0)
            {
                _activeEnemies.RemoveAll(enemy => enemy == null);
                if (_activeEnemies.Count > 0)
                {
                    Debug.Log($"Waiting for {_activeEnemies.Count} enemies to be destroyed before next wave...");
                    yield return new WaitForSeconds(0.5f);
                }
            }
            
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
            
            // Kazandınız ekranını göster
            ShowWinScreen();
        }
        
        // Geçiş tamamlandı, yeni wave'e geçilebilir
        _isTransitioning = false;
    }

    private void ShowWinScreen()
    {
        // Kazandınız görselini göster
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
        
        // 5 saniye sonra ana menüye dön
        StartCoroutine(ReturnToMainMenu());
    }
    
    private IEnumerator ReturnToMainMenu()
    {
        // 5 saniye bekle
        yield return new WaitForSeconds(5f);
        
        // Ana menüye dön
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("Ana menü sahne adı belirtilmemiş!");
        }
    }

    private void StartWave()
    {
        Debug.Log($"Starting Wave {CurrentWave}/{TotalWaves}");

        // Set up the wave
        Wave currentWave = waves[currentWaveIndex];
        _meleeEnemiesToSpawn = currentWave.meleeEnemyCount;
        _rangedEnemiesToSpawn = currentWave.rangedEnemyCount;
        _enemiesRemainingAlive = currentWave.totalEnemies;

        // Verify total enemy count matches type counts
        if (currentWave.meleeEnemyCount + currentWave.rangedEnemyCount != currentWave.totalEnemies)
        {
            Debug.LogWarning($"Wave {CurrentWave}: Total enemy count ({currentWave.totalEnemies}) doesn't match " +
                             $"the sum of melee ({currentWave.meleeEnemyCount}) and ranged ({currentWave.rangedEnemyCount}) enemies!");
            // Adjust total to match sum
            _enemiesRemainingAlive = currentWave.meleeEnemyCount + currentWave.rangedEnemyCount;
        }

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

        while (_meleeEnemiesToSpawn > 0 || _rangedEnemiesToSpawn > 0)
        {
            // Decide what type of enemy to spawn
            bool spawnMelee = false;
            
            // If one type is depleted, spawn the other
            if (_meleeEnemiesToSpawn <= 0) spawnMelee = false;
            else if (_rangedEnemiesToSpawn <= 0) spawnMelee = true;
            else
            {
                // Otherwise, decide randomly based on remaining counts
                float meleeRatio = (float)_meleeEnemiesToSpawn / (float)(_meleeEnemiesToSpawn + _rangedEnemiesToSpawn);
                spawnMelee = Random.value < meleeRatio;
            }
            
            if (spawnMelee)
            {
                SpawnMeleeEnemy();
                _meleeEnemiesToSpawn--;
            }
            else
            {
                SpawnRangedEnemy();
                _rangedEnemiesToSpawn--;
            }

            // Wait for the next spawn
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        _isSpawning = false;
    }

    private void SpawnMeleeEnemy()
    {
        if (spawnPoints.Length > 0 && meleeEnemyPrefab != null)
        {
            // Get random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // Spawn enemy
            GameObject enemy = Instantiate(meleeEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Add component to track when enemy is destroyed
            EnemyWaveTracker tracker = enemy.AddComponent<EnemyWaveTracker>();
            tracker.waveSpawner = this;
            
            // Add to list
            _activeEnemies.Add(enemy);
            
            Debug.Log($"Spawned melee enemy in wave {CurrentWave}. Remaining melee: {_meleeEnemiesToSpawn}, ranged: {_rangedEnemiesToSpawn}");
        }
        else
        {
            Debug.LogError("Missing spawn points or melee enemy prefab!");
        }
    }

    private void SpawnRangedEnemy()
    {
        // Choose from ranged spawn points if available, otherwise use regular spawn points
        Transform[] availableSpawnPoints = (rangedSpawnPoints != null && rangedSpawnPoints.Length > 0) 
            ? rangedSpawnPoints 
            : spawnPoints;
            
        if (availableSpawnPoints.Length > 0 && rangedEnemyPrefab != null)
        {
            // Get random spawn point
            Transform spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Length)];
            
            // Spawn enemy
            GameObject enemy = Instantiate(rangedEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Add component to track when enemy is destroyed
            EnemyWaveTracker tracker = enemy.AddComponent<EnemyWaveTracker>();
            tracker.waveSpawner = this;
            
            // Add to list
            _activeEnemies.Add(enemy);
            
            Debug.Log($"Spawned ranged enemy in wave {CurrentWave}. Remaining melee: {_meleeEnemiesToSpawn}, ranged: {_rangedEnemiesToSpawn}");
        }
        else
        {
            Debug.LogError("Missing spawn points or ranged enemy prefab!");
        }
    }

    public void EnemyDefeated()
    {
        _enemiesRemainingAlive--;
        Debug.Log($"Enemy defeated! Remaining alive: {_enemiesRemainingAlive}, Active enemies: {_activeEnemies.Count}");
        
        // Aktif düşman listesini de güncelle
        _activeEnemies.RemoveAll(enemy => enemy == null);
    }

    // Simple component to track when an enemy is destroyed
    private class EnemyWaveTracker : MonoBehaviour
    {
        public EnhancedWaveSpawner waveSpawner;

        private void OnDestroy()
        {
            // Sahne kapanıyorsa veya uygulama sonlanıyorsa bu işlemi yapma
            if (waveSpawner != null && !waveSpawner.Equals(null) && gameObject.scene.isLoaded)
            {
                waveSpawner.EnemyDefeated();
            }
        }
    }

    // Draw gizmos for spawn points
    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            // Draw regular spawn points in blue
            if (spawnPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (Transform spawnPoint in spawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireCube(spawnPoint.position, new Vector3(1, 1, 1));
                        Gizmos.DrawLine(transform.position, spawnPoint.position);
                    }
                }
            }
            
            // Draw ranged spawn points in green
            if (rangedSpawnPoints != null)
            {
                Gizmos.color = Color.green;
                foreach (Transform spawnPoint in rangedSpawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 1f);
                        Gizmos.DrawLine(transform.position, spawnPoint.position);
                    }
                }
            }
        }
    }
}