using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DotGalacticos.Guns.Demo
{
    [DisallowMultipleComponent]
    public class PlayerWeaponSelector : MonoBehaviour
    {
        private Animator animator;

        [SerializeField]
        private Transform GunParent; // Silahın yerleştirileceği nokta
        [SerializeField]
        private GunType gunType; // Seçilecek silah türü
        [SerializeField]
        private List<MeleeWeaponScriptableObject> Weapons; // Melee silahları listesi
        private int currentWeaponIndex = 0; // Aktif silahın indeksi

        [Space]
        [Header("Runtime Filled")]
        public MeleeWeaponScriptableObject ActiveWeapon;

        public MeleeWeaponScriptableObject ActiveBaseWeapon;

        private void Awake()
        {
            MeleeWeaponScriptableObject meleeWeaponScriptableObject = Weapons.Find(weapon => weapon.Type == gunType);
            if (Weapons == null || Weapons.Count == 0)
            {
                Debug.LogError("No MeleeWeaponScriptableObject found.");
                return;
            }
            ActiveBaseWeapon = meleeWeaponScriptableObject;
            animator = GetComponent<Animator>();
            SetupWeapon(ActiveBaseWeapon); // Başlangıçta belirlenen silah türünü kur
        }

        private void SetupWeapon(MeleeWeaponScriptableObject type)
        {
            ActiveBaseWeapon = type;
            ActiveWeapon = type.Clone() as MeleeWeaponScriptableObject; // Aktif silahı klonla
            ActiveWeapon.Spawn(GunParent, this); // Silahı sahneye yerleştir
        }
        public void DespawnActiveGun()
        {
            if (ActiveWeapon != null)
            {
                ActiveWeapon.Despawn();
                Destroy(ActiveWeapon);
            }
        }

        public void SelectWeapon(int diceResult)
        {
            EquipWeapon(diceResult); // Zar sonucuna göre silahı seç
        }
        public void EquipWeapon(int index)
        {
            if (index >= 0 && index < Weapons.Count)
            {
                DespawnActiveGun();
                currentWeaponIndex = index;
                Debug.Log($"Silah değiştirildi: {Weapons[currentWeaponIndex].Name}");
                SetupWeapon(Weapons[currentWeaponIndex]);
                // Burada silahı sahneye yerleştirme veya diğer gerekli işlemleri yapabilirsiniz
            }
            else
            {
                Debug.LogWarning("Geçersiz silah indeksi.");
            }
        }

    }
}