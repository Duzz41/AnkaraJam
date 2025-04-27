using UnityEngine;

public class RangedWeapons : MonoBehaviour
{
    public GameObject projectilePrefab; // Mermi prefab'ı
    public Transform spawnPoint; // Merminin oluşturulacağı nokta
    public float projectileSpeed = 20f; // Merminin hızı

    void ShootProjectile()
    {
        // Rayın başlangıç noktası ve yönü
        Vector3 rayOrigin = Camera.main.transform.position;
        Vector3 rayDirection = Camera.main.transform.forward; // Karakterin baktığı yön

        RaycastHit hit;
        // Raycast ile çarpma noktasını kontrol et
        if (Physics.Raycast(rayOrigin, rayDirection, out hit))
        {
            // Mermiyi oluştur
            GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);

            // Merminin yönünü çarpma noktasına ayarla
            Vector3 direction = (hit.point - spawnPoint.position).normalized;

            // Merminin Rigidbody bileşenini al ve kuvvet uygula
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed; // Yön ve hız ile mermiyi hareket ettir
            }
        
            Debug.Log($"Mermi {hit.collider.name} ile çarpıştı!"); // Çarpma mesajı
        }
        else
        {
            Debug.Log("Ray bir nesneye çarpamadı.");
        }
    }
}
