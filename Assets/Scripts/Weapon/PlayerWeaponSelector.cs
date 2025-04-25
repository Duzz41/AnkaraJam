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

        private MeleeWeaponScriptableObject FindWeaponByType(GunType type)
        {
            // Silah türüne göre uygun silahı bul
            foreach (var weapon in Weapons)
            {
                if (weapon.Type == type) // gunType özelliği ile karşılaştır
                {
                    return weapon;
                }
            }
            return null; // Uygun silah bulunamazsa null döndür
        }

        private void SpawnWeapon()
        {
            // Silahı sahneye yerleştir
            GameObject weaponObject = Instantiate(ActiveWeapon.ModelPrefab, GunParent.position, Quaternion.identity);
            weaponObject.transform.SetParent(GunParent, false);
            weaponObject.transform.localPosition = Vector3.zero; // İstenilen pozisyona ayarlayın

            // Burada silahın modelini ve diğer bileşenlerini ekleyebilirsiniz
            Debug.Log($"{ActiveWeapon.Name} sahneye yerleştirildi!");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //SetupWeapon(GunType.Sword); // 1 tuşuna basıldığında kılıcı seç
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // SetupWeapon(GunType.Axe); // 2 tuşuna basıldığında baltayı seç
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                //SetupWeapon(GunType.Stick); // 3 tuşuna basıldığında sopayı seç
            }
        }
    }
}