using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private int damage;
    private float lifetime;
    private float timer;

    public GameObject hitEffect; // Optional effect to spawn on hit
    public GameObject trailEffect; // Optional trail effect
    
    [Header("Visual Effects")]
    [Tooltip("Should the projectile pulsate?")]
    public bool doPulsate = true;
    
    [Tooltip("Pulsation speed")]
    public float pulsateSpeed = 5f;
    
    [Tooltip("Pulsation size range")]
    public Vector2 pulsateRange = new Vector2(0.8f, 1.2f);
    
    [Tooltip("Should the projectile rotate?")]
    public bool doRotate = true;
    
    [Tooltip("Rotation speed")]
    public Vector3 rotationSpeed = new Vector3(0, 180f, 90f);

    // Initialize the projectile
    public void Initialize(Vector3 direction, float speed, int damage, float lifetime)
    {
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;
        this.lifetime = lifetime;
        timer = 0f;
        
        // Create trail effect if specified
        if (trailEffect != null)
        {
            Instantiate(trailEffect, transform);
        }
        
        // Start with a small scale and grow to full size
        transform.localScale = Vector3.one * 0.1f;
        StartCoroutine(GrowProjectile());
    }
    
    private System.Collections.IEnumerator GrowProjectile()
    {
        Vector3 targetScale = Vector3.one;
        float duration = 0.2f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(Vector3.one * 0.1f, targetScale, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = targetScale;
    }

    private void Update()
    {
        // Move the projectile
        transform.position += direction * speed * Time.deltaTime;

        // Apply visual effects
        if (doRotate)
        {
            // Rotate projectile continuously for a more dynamic look
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            // Standard rotation to face direction of travel
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Apply pulsating scale effect
        if (doPulsate)
        {
            float pulseFactor = Mathf.Lerp(pulsateRange.x, pulsateRange.y, 
                                          (Mathf.Sin(Time.time * pulsateSpeed) + 1f) * 0.5f);
            transform.localScale = Vector3.one * pulseFactor;
        }

        // Track lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            // Fade out and destroy
            StartCoroutine(FadeAndDestroy());
        }
    }
    
    private System.Collections.IEnumerator FadeAndDestroy()
    {
        // Get all renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        // Fade out over 0.2 seconds
        float duration = 0.2f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            
            // Fade all materials
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if we hit player
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Projectile hit player for {damage} damage!");
            }
            
            // Trigger hit effect and destroy
            HandleHitEffect(transform.position, transform.forward);
        }
        // Check if we hit environment
        else if (!other.CompareTag("Enemy") && !other.isTrigger)
        {
            // Trigger hit effect and destroy
            HandleHitEffect(transform.position, transform.forward);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Alternative collision detection
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Projectile hit player for {damage} damage!");
            }
            
            // Get contact info and trigger effect
            ContactPoint contact = collision.contacts[0];
            HandleHitEffect(contact.point, contact.normal);
        }
        // Check if we hit environment
        else if (!collision.gameObject.CompareTag("Enemy"))
        {
            ContactPoint contact = collision.contacts[0];
            HandleHitEffect(contact.point, contact.normal);
        }
    }
    
    private void HandleHitEffect(Vector3 position, Vector3 normal)
    {
        // Spawn hit effect if available
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, position, Quaternion.LookRotation(normal));
            
            // Make the effect larger and more dramatic
            effect.transform.localScale *= 1.5f;
            
            // Destroy effect after a few seconds
            Destroy(effect, 2f);
        }
        
        // Play impact sound
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            // Detach audio source so it continues playing after projectile destruction
            audioSource.transform.parent = null;
            audioSource.Play();
            Destroy(audioSource.gameObject, audioSource.clip.length);
        }
        
        // Stop all coroutines and destroy immediately
        StopAllCoroutines();
        Destroy(gameObject);
    }
}