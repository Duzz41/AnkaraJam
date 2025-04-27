using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For UI elements
using UnityEngine.Events;
using UnityEngine.SceneManagement; // For events

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

    [Header("Events")]
    [Tooltip("Event triggered when player takes damage")]
    public UnityEvent onDamage;

    [Tooltip("Event triggered when player dies")]
    public UnityEvent onDeath;
    // Private variables
    private bool _isInvincible = false;

    private void Start()
    {
        // Set initial health
        currentHealth = maxHealth;

        // Set up UI if available
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
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

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Start invincibility
        StartCoroutine(InvincibilityTimer());
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