using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to display wave information")]
    public TextMeshProUGUI waveText;

    [Tooltip("Text component to display countdown")]
    public TextMeshProUGUI countdownText;

    [Tooltip("Text component to display enemies remaining")]
    public TextMeshProUGUI enemiesText;

    [Header("Animation")]
    [Tooltip("Animation duration for wave banner")]
    public float waveAnimDuration = 2f;

    [Tooltip("Panel containing the wave banner")]
    public GameObject waveBanner;

    // Reference to the wave spawner
    private EnhancedWaveSpawner _enhancedWaveSpawner;

    private void Start()
    {
        // Find the wave spawner in the scene
        _enhancedWaveSpawner = FindObjectOfType<EnhancedWaveSpawner>();
        
        if (_enhancedWaveSpawner == null)
        {
            Debug.LogError("WaveUI couldn't find a WaveSpawner in the scene!");
            return;
        }

        // Subscribe to wave events
        _enhancedWaveSpawner.onWaveStart.AddListener(OnWaveStart);
        _enhancedWaveSpawner.onWaveCompleted.AddListener(OnWaveCompleted);
        _enhancedWaveSpawner.onAllWavesCompleted.AddListener(OnAllWavesCompleted);

        // Hide the wave banner initially
        if (waveBanner != null)
        {
            waveBanner.SetActive(false);
        }
    }

    private void Update()
    {
        if (_enhancedWaveSpawner != null && enemiesText != null)
        {
            // Update enemies remaining text
            enemiesText.text = $"Düşmanlar: {_enhancedWaveSpawner.RemainingEnemies}";
        }
    }

    private void OnWaveStart(int waveIndex)
    {
        // Update wave text
        if (waveText != null)
        {
            waveText.text = $"WAVE {_enhancedWaveSpawner.CurrentWave}/{_enhancedWaveSpawner.TotalWaves}";
            
        }

        // Show wave banner
        StartCoroutine(ShowWaveBanner());
    }

    private void OnWaveCompleted(int waveIndex)
    {
        // If we have a countdown text, start the countdown to next wave
        if (countdownText != null && waveIndex < _enhancedWaveSpawner.TotalWaves - 1)
        {
            StartCoroutine(ShowCountdown(_enhancedWaveSpawner.waves[waveIndex].waveInterval));
        }
    }

    private void OnAllWavesCompleted()
    {
        // Show game completed message
        if (waveText != null)
        {
            waveText.text = "TÜM DALGALAR TAMAMLANDI!";
            StartCoroutine(ShowWaveBanner());
        }
    }

    private IEnumerator ShowWaveBanner()
    {
        if (waveBanner != null)
        {
            // Show the banner
            waveBanner.SetActive(true);
            
            // Wait for animation duration
            yield return new WaitForSeconds(waveAnimDuration);
            
            // Hide the banner
            waveBanner.SetActive(false);
        }
    }

    private IEnumerator ShowCountdown(float duration)
    {
        // Show wave completed message first
        if (waveText != null)
        {
            waveText.text = $"WAVE {_enhancedWaveSpawner.CurrentWave} TAMAMLANDI!";
            waveBanner.SetActive(true);
            yield return new WaitForSeconds(2f);
            waveBanner.SetActive(false);
        }
        
        // Now show countdown
        float remainingTime = duration;
        
        while (remainingTime > 0)
        {
            // Update countdown text
            countdownText.text = $"Sonraki Dalga: {Mathf.CeilToInt(remainingTime)}";
            countdownText.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(0.1f);
            remainingTime -= 0.1f;
        }
        
        // Hide countdown text
        countdownText.gameObject.SetActive(false);
    }
}