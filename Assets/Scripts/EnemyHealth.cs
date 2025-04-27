using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum enemy health")]
    public int maxHealth = 100;

    [Tooltip("Current enemy health")]
    public int currentHealth;

    [Header("Effects")]
    [Tooltip("Particle effect to play when damaged")]
    public GameObject hitEffect;

    [Tooltip("Particle effect to play when enemy dies")]
    public GameObject deathEffect;

    [Header("Audio")]
    [Tooltip("Sound to play when taking damage")]
    public AudioClip hitSound;

    [Tooltip("Sound to play when enemy dies")]
    public AudioClip deathSound;

    [Range(0, 1)] public float audioVolume = 0.5f;

    [Header("Events")]
    [Tooltip("Event triggered when enemy takes damage")]
    public UnityEvent onDamage;

    [Tooltip("Event triggered when enemy dies")]
    public UnityEvent onDeath;

    // References
    private Animator _animator;

    private void Start()
    {
        // Set initial health
        currentHealth = maxHealth;
        
        // Get animator if available
        _animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        // Apply damage
        currentHealth -= damage;
        
        // Trigger damage event
        onDamage?.Invoke();

        // Play hit effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Play hit sound
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, audioVolume);
        }

        // Play hit animation if animator exists
        if (_animator != null)
        {
            _animator.SetTrigger("Hit");
        }

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
            return;
        }
    }

   private void Die()
{
    // Set health to 0 to be safe
    currentHealth = 0;
    
    // Trigger death event
    onDeath?.Invoke();
    
    // Death işlemleri başlamadan önce bazı bileşenleri devre dışı bırak
    
    // NavMeshAgent'ı devre dışı bırak (varsa)
    var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    if (navAgent != null)
    {
        navAgent.enabled = false;
    }
    
    // EnemyController'ı devre dışı bırak (varsa)
    var enemyController = GetComponent<EnemyAssets.EnemyController>();
    if (enemyController != null)
    {
        enemyController.enabled = false;
    }
    
    // Rigidbody'yi kinematik yap (varsa)
    var rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = true;
    }
    
    // Düşmanı yere sabitle
    // Y pozisyonunu yere göre ayarla
    Vector3 fixedPosition = transform.position;
    fixedPosition.y = transform.position.y; // Mevcut Y pozisyonunu koru
    transform.position = fixedPosition;
    
    // Play death effect
    if (deathEffect != null)
    {
        Instantiate(deathEffect, transform.position, Quaternion.identity);
    }
    
    // Play death sound
    if (deathSound != null)
    {
        AudioSource.PlayClipAtPoint(deathSound, transform.position, audioVolume);
    }

    // Play death animation if animator exists
    if (_animator != null)
    {
        // Root Motion'ı devre dışı bırak
        _animator.applyRootMotion = false;
        
        _animator.SetTrigger("Death");
        
        // Get animation length to delay destruction
        AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
        float destroyDelay = 0f;
        
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Contains("death") || clip.name.Contains("Death"))
            {
                destroyDelay = clip.length;
                break;
            }
        }
        
        // Destroy enemy after death animation
        Destroy(gameObject, destroyDelay > 0 ? destroyDelay : 2f);
    }
    else
    {
        // No animator, destroy immediately
        Destroy(gameObject);
    }
}
}