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
    private WaveSpawner _waveSpawner;

    private void Start()
    {
        // Find the wave spawner in the scene
        _waveSpawner = FindObjectOfType<WaveSpawner>();
        
        if (_waveSpawner == null)
        {
            Debug.LogError("WaveUI couldn't find a WaveSpawner in the scene!");
            return;
        }

        // Subscribe to wave events
        _waveSpawner.onWaveStart.AddListener(OnWaveStart);
        _waveSpawner.onWaveCompleted.AddListener(OnWaveCompleted);
        _waveSpawner.onAllWavesCompleted.AddListener(OnAllWavesCompleted);

        // Hide the wave banner initially
        if (waveBanner != null)
        {
            waveBanner.SetActive(false);
        }
    }

    private void Update()
    {
        if (_waveSpawner != null && enemiesText != null)
        {
            // Update enemies remaining text
            enemiesText.text = $"Düşmanlar: {_waveSpawner.RemainingEnemies}";
        }
    }

    private void OnWaveStart(int waveIndex)
    {
        // Update wave text
        if (waveText != null)
        {
            waveText.text = $"WAVE {_waveSpawner.CurrentWave}/{_waveSpawner.TotalWaves}";
        }

        // Show wave banner
        StartCoroutine(ShowWaveBanner());
    }

    private void OnWaveCompleted(int waveIndex)
    {
        // If we have a countdown text, start the countdown to next wave
        if (countdownText != null && waveIndex < _waveSpawner.TotalWaves - 1)
        {
            StartCoroutine(ShowCountdown(_waveSpawner.waves[waveIndex].waveInterval));
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
            waveText.text = $"WAVE {_waveSpawner.CurrentWave} TAMAMLANDI!";
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