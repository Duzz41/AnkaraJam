using System.Collections;
using DotGalacticos.Guns.Demo;
using UnityEngine;

namespace DotGalacticos.Guns
{
    public class DiceRoller : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponSelector weaponSelector; // Silah seçici
        [SerializeField] private float rollDuration = 3f; // Zar atma süresi
        [SerializeField] private float rollInterval = 1f; // Her zar atma aralığı
        private bool isRolling = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                isRolling = false;
                StartRolling(); // F tuşuna basıldığında zar at
            }
        }

        public void StartRolling()
        {
            if (!isRolling)
            {
                StartCoroutine(RollDiceCoroutine());
            }
        }

        private IEnumerator RollDiceCoroutine()
        {
            isRolling = true;
            float elapsedTime = 0f;
            int finalDiceResult = 0;
            int diceResult = Random.Range(1, 7); // 1 ile 6 arasında rastgele bir sayı
            Debug.Log($"Zar sonucu: {diceResult}");
            finalDiceResult = diceResult; // Son zar sonucunu güncelle
            elapsedTime += rollInterval; // Geçen süreyi güncelle
            weaponSelector.SelectWeapon(finalDiceResult - 1); // 0 tabanlı indeks
            yield return new WaitForSeconds(rollInterval); // Belirtilen aralıkta bekle
            // Süre sonunda son zar sonucunu göster
            Debug.Log($"Son zar sonucu: {finalDiceResult}");

            isRolling = false;
        }
    }
}