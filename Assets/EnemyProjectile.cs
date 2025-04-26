using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private int damage;
    private float lifetime;
    private float timer;

    public GameObject hitEffect; // Optional effect to spawn on hit

    // Initialize the projectile
    public void Initialize(Vector3 direction, float speed, int damage, float lifetime)
    {
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;
        this.lifetime = lifetime;
        timer = 0f;
    }

    private void Update()
    {
        // Move the projectile
        transform.position += direction * speed * Time.deltaTime;

        // Rotate projectile to face direction of travel
        transform.rotation = Quaternion.LookRotation(direction);

        // Track lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
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
        }

        // Spawn hit effect if available
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Destroy projectile on hit
        Destroy(gameObject);
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
        }

        // Spawn hit effect if available
        if (hitEffect != null)
        {
            ContactPoint contact = collision.contacts[0];
            Instantiate(hitEffect, contact.point, Quaternion.LookRotation(contact.normal));
        }

        // Destroy projectile on hit
        Destroy(gameObject);
    }
}