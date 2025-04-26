using DotGalacticos.Guns;
using Unity.Mathematics;
using UnityEngine;

public class ThrowWeapon : MonoBehaviour
{
    public float throwForce = 10f; // Fırlatma kuvveti
    public float rotationSpeed = 360f; // Dönme hızı
    public float lifetime = 5f; // Yok olma süresi
    public Vector3 offset;
    public Transform spawnPoint;
    public MeleeWeaponScriptableObject weaponData; // MeleeWeaponScriptableObject referansı
    private int damage; // Düşmana verilecek hasar
    void Start()
    {
        spawnPoint = GameObject.Find("GunSpawn").transform;
        if (weaponData != null)
        {
            damage = weaponData.Damage; // Hasar bilgisini al
            
        }
        else
        {
            Debug.LogWarning("Weapon data is not assigned.");
        }
    }
    public void ThrowObject()
    {
        Vector3 throwDirection = Camera.main.transform.forward;
        GameObject thrownWeapon = Instantiate(gameObject, spawnPoint.position + offset, quaternion.identity);
        thrownWeapon.GetComponent<Animator>().enabled = false; // Animasyonu devre dışı bırak
        Rigidbody rb = thrownWeapon.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            rb.angularVelocity = Vector3.right * rotationSpeed * Mathf.Deg2Rad; // Y ekseninde dönmes
        }
        else
        {
            Debug.LogWarning("Thrown object does not have a Rigidbody component.");
        }

        Object.Destroy(thrownWeapon, lifetime); // Belirli bir süre sonra yok olma
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) // Eğer çarpışılan nesne "Enemy" tag'ine sahipse
        {
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>(); // EnemyHealth scriptini al
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage); // Düşmana hasar uygula
                Debug.Log($"{weaponData.Name} düşmana vurdu! Hasar: {damage}"); // Log mesajı
            }
            Destroy(gameObject); // Fırlatılan nesneyi yok et
        }
    }
}