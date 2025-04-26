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
        [SerializeField] private GameObject diceObject; // Zar modeli
        [SerializeField] private Transform handTransform; // Elin transform'u
        [SerializeField] private float rotationSpeed = 720f; // Zar dönüş hızı (derece/saniye)
        [SerializeField] private float handShowDuration = 5f; // Elin ekranda kalma süresi
        [SerializeField] private float autoRollInterval = 30f; // Otomatik zar atma aralığı (saniye)

        private bool isRolling = false;
        private int finalDiceResult = 0;
        private float autoRollTimer = 0f;

        void Start()
        {
            // Başlangıçta zarı ve eli gizle
            if (diceObject != null)
                diceObject.SetActive(false);
            
            if (handTransform != null)
                handTransform.gameObject.SetActive(false);
        }

        void Update()
        {
            // Manuel zar atma kontrolü
            if (Input.GetKeyDown(KeyCode.F))
            {
                isRolling = false;
                StartRolling(); // F tuşuna basıldığında zar at
            }

            // Otomatik zar atma zamanlayıcısı
            autoRollTimer += Time.deltaTime;
            if (autoRollTimer >= autoRollInterval)
            {
                autoRollTimer = 0f;
                if (!isRolling)
                {
                    StartRolling(); // Belirli aralıklarla otomatik zar at
                }
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
            
            // Eli göster
            if (handTransform != null)
                handTransform.gameObject.SetActive(true);
            
            // Zarı göster
            if (diceObject != null)
                diceObject.SetActive(true);
            
            float elapsedTime = 0f;
            
            // Zarın rastgele dönmesi
            while (elapsedTime < rollDuration)
            {
                if (diceObject != null)
                {
                    // Her frame'de zarı farklı eksenlerde döndür
                    float xRotation = rotationSpeed * Time.deltaTime;
                    float yRotation = rotationSpeed * 0.8f * Time.deltaTime;
                    float zRotation = rotationSpeed * 0.6f * Time.deltaTime;
                    
                    diceObject.transform.Rotate(xRotation, yRotation, zRotation, Space.World);
                }
                
                elapsedTime += Time.deltaTime;
                yield return null; // Bir sonraki frame'e geç
            }
            
            // Zar sonucunu belirle (1-8 arası)
            finalDiceResult = Random.Range(1, 9);
            Debug.Log($"Zar sonucu: {finalDiceResult}");
            
            // Zarın son pozisyonunu ayarla (sonuca göre)
            if (diceObject != null)
            {
                // Zar sonucuna göre rotasyonu ayarla
                SetDiceFace(finalDiceResult);
            }
            
            // Silahı seç (0 tabanlı indeks olduğu için -1)
            weaponSelector.SelectWeapon(finalDiceResult - 1);
            
            // Biraz bekle ve sonra eli ve zarı gizle
            yield return new WaitForSeconds(handShowDuration);
            
            // Eli ve zarı gizle
            if (handTransform != null)
                handTransform.gameObject.SetActive(false);
            
            if (diceObject != null)
                diceObject.SetActive(false);
            
            isRolling = false;
        }
        
        private void SetDiceFace(int faceValue)
        {
            // Zarın yüzünü ayarlamak için rotasyonlar (8 yüzlü zar için)
            // Not: Bu rotasyonlar örnek olarak verilmiştir, gerçek 8 yüzlü zar modelinize göre ayarlamanız gerekir
            Quaternion targetRotation = Quaternion.identity;
            
            switch (faceValue)
            {
                case 1:
                    targetRotation = Quaternion.Euler(-45, -22.5f , 0); // 1 üstte
                    break;
                case 2:
                    targetRotation = Quaternion.Euler(-123.5f, -20, 90); // 2 üstte
                    break;
                case 3:
                    targetRotation = Quaternion.Euler(-53.5f, -20, 90); // 3 üstte
                    break;
                case 4:
                    targetRotation = Quaternion.Euler(320f, -20, -90); // 4 üstte
                    break;
                case 5:
                    targetRotation = Quaternion.Euler(-45, -22.5f, 180); // 5 üstte
                    break;
                case 6:
                    targetRotation = Quaternion.Euler(245f, -20, -90); // 6 üstte
                    break;
                case 7:
                    targetRotation = Quaternion.Euler(-117.5f, -22.5f, 0); // 7 üstte
                    break;
                case 8:
                    targetRotation = Quaternion.Euler(-117.5f, -22.5f, 180); // 8 üstte
                    break;
            }
            
            // Zarın rotasyonunu belirle
            diceObject.transform.rotation = targetRotation;
        }
    }
}