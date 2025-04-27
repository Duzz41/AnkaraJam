using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [SerializeField] private int maxDamage = 100; // Maksimum hasar
    [SerializeField] private int minDamage = 10; // Minimum hasar
    [SerializeField] private float explosionRadius = 5f; // Patlama yarıçapı
    [SerializeField] private AudioClip explosionSound; // Patlama sesi
    private AudioSource audioSource;

    void OnCollisionEnter(Collision collision)
    {
        Explode();
        Destroy(gameObject);
        PlayExplosionSound();
    }

    void Explode()
    {
        Vector3 explosionPosition = transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, explosionRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(explosionPosition, hitCollider.transform.position);
                int damage = CalculateDamage(distance);

                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    Debug.Log($"Düşmana {damage} hasar verildi!");
                }
            }
        }

    }
    public void PlayExplosionSound()
    {
        if (explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
    }
    int CalculateDamage(float distance)
    {
        // Hasar hesaplama
        float damageRatio = Mathf.Clamp01((explosionRadius - distance) / explosionRadius);
        int damage = Mathf.RoundToInt(maxDamage * damageRatio);

        // Minimum hasarı kontrol et
        return Mathf.Max(damage, minDamage);
    }
}