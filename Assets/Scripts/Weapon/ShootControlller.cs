using DotGalacticos.Guns.Demo;
using UnityEngine;

namespace DotGalacticos.Guns
{
    public class ShootController : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponSelector GunSelector; // Silah seçicisi

        private void Update()
        {
            // Sol fare tuşuna basıldığında saldır
            if (Input.GetMouseButtonDown(0)) // 0, sol fare tuşunu temsil eder
            {
                Attack();
            }
        }

        private void Attack()
        {
            if (GunSelector.ActiveWeapon != null)
            {
                // Oyuncunun konumunu al
                Vector3 attackPosition = transform.position;
                GunSelector.ActiveWeapon.Attack(); // Silahın Attack metodunu çağır
            }
            else
            {
                Debug.LogWarning("No weapon assigned to the player.");
            }
        }
    }
}