using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For UI elements
using UnityEngine.Events;
using UnityEngine.SceneManagement; // For events
using Cinemachine; // Cinemachine kütüphanesini ekleyin

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum player health")]
    public int maxHealth = 100;

    [Tooltip("Current player health")]
    public int currentHealth;

    [Tooltip("Invincibility time after being hit (in seconds)")]
    public float invincibilityTime = 1.0f;

    [Header("UI References")]
    [Tooltip("Optional: UI Slider for health bar")]
    public Slider healthSlider;

    [Header("Damage Effect")]
    [Tooltip("UI Image that appears when damaged")]
    public Image damageImage;
    
    [Tooltip("Duration of damage effect fade out")]
    public float damageEffectDuration = 0.5f;

    [Header("Events")]
    [Tooltip("Event triggered when player takes damage")]
    public UnityEvent onDamage;

    [Tooltip("Event triggered when player dies")]
    public UnityEvent onDeath;
    
    // Private variables
    public CameraShake cameraShake;
    private bool _isInvincible = false;
    private Coroutine damageEffectCoroutine;
    
    
    

    private void Start()
    {
        cameraShake = GetComponent<CameraShake>();
        // Set initial health
        currentHealth = maxHealth;

        // Set up UI if available
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Initialize damage image
        if (damageImage != null)
        {
            damageImage.gameObject.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        // Check if player is invincible
        if (_isInvincible)
            return;

        // Apply damage
        currentHealth -= damage;
        // Trigger damage event
        onDamage?.Invoke();

        // Update UI
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // Show damage effect
        ShowDamageEffect();

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Start invincibility
        StartCoroutine(InvincibilityTimer());
    }

    private void ShowDamageEffect()
    {
        if (damageImage == null)
            return;

        // If there's already a coroutine running, stop it
        if (damageEffectCoroutine != null)
        {
            StopCoroutine(damageEffectCoroutine);
        }

        damageEffectCoroutine = StartCoroutine(DamageEffectCoroutine());
    }

    private IEnumerator DamageEffectCoroutine()
    {
        // Immediately show the damage image
        damageImage.gameObject.SetActive(true);
        Color imageColor = damageImage.color;
        imageColor.a = 1f; // Full opacity
        damageImage.color = imageColor;

        float elapsedTime = 0f;

        // Fade out the damage image
        while (elapsedTime < damageEffectDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / damageEffectDuration);
            imageColor.a = alpha;
            damageImage.color = imageColor;
            yield return null;
        }

        // Hide the image completely
        damageImage.gameObject.SetActive(false);
    }

    private IEnumerator InvincibilityTimer()
    {
        _isInvincible = true;

        // Optional: visual feedback for invincibility
        // e.g., make the player model flash

        yield return new WaitForSeconds(invincibilityTime);

        _isInvincible = false;
    }

    private void Die()
    {
        // Set health to 0 to be safe
        currentHealth = 0;

        // Trigger death event
        onDeath?.Invoke();

        // Implement death behavior
        Debug.Log("Player died!");
        SceneManager.LoadScene("MainMenu");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        // You can add more death behavior here:
        // - Play death animation
        // - Disable player controls
        // - Show game over screen
        // - etc.
    }

    public void Heal(int amount)
    {
        // Apply healing
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // Update UI
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
}