using System.Collections;
using DotGalacticos.Guns.Demo;
using UnityEngine;

namespace DotGalacticos.Guns
{
    public class DiceRoller : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponSelector weaponSelector; // Silah seçici
        [SerializeField] private float rollDuration = 3f; // Zar atma süresi
        [SerializeField] private GameObject diceObject; // Zar modeli
        [SerializeField] private Transform handTransform; // Elin transform'u
        [SerializeField] private float rotationSpeed = 720f; // Zar dönüş hızı (derece/saniye)
        [SerializeField] private float handShowDuration = 5f; // Elin ekranda kalma süresi
        
        [Header("Shield Dice Reference")]
        [SerializeField] private ShieldDiceRoller shieldDiceRoller; // Kalkan zarı referansı
        
        [Header("Randomize Roll Intervals")]
        [SerializeField] private float minRollInterval = 15f; // Minimum zar atma aralığı (saniye)
        [SerializeField] private float maxRollInterval = 25f; // Maximum zar atma aralığı (saniye)

        private bool isRolling = false;
        private int finalDiceResult = 0;
        private float nextRollTimer = 0f;
        private float currentRollInterval = 0f;
        
        // Referans noktası olarak kullanmak için dice'ın başlangıç rotasyonu
        private Quaternion diceInitialRotation;

        void Start()
        {
            // Başlangıçta zarı ve eli gizle
            if (diceObject != null)
            {
                diceObject.SetActive(false);
                // Zar'ın başlangıç rotasyonunu kaydet
                diceInitialRotation = diceObject.transform.localRotation;
            }
            
            if (handTransform != null)
                handTransform.gameObject.SetActive(false);
                
            // İlk zar atma zamanını belirle
            SetNextRollInterval();
        }

        void Update()
        {
            // Otomatik zar atma zamanlayıcısı
            nextRollTimer += Time.deltaTime;
            if (nextRollTimer >= currentRollInterval)
            {
                nextRollTimer = 0f;
                if (!isRolling)
                {
                    StartRolling(); // Belirli aralıklarla otomatik zar at
                    SetNextRollInterval(); // Bir sonraki zar atımı için süreyi yeniden belirle
                }
            }
        }
        
        // Bir sonraki zar atımı için rastgele süre belirle
        private void SetNextRollInterval()
        {
            currentRollInterval = Random.Range(minRollInterval, maxRollInterval);
            Debug.Log($"Bir sonraki zar atımı {currentRollInterval} saniye sonra gerçekleşecek.");
        }

        public void StartRolling()
        {
            if (!isRolling)
            {
                StartCoroutine(RollDiceCoroutine());
                
                // Aynı anda kalkan zarını da başlat
                if (shieldDiceRoller != null)
                {
                    shieldDiceRoller.StartRollingWithWeaponDice();
                }
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
                    // Her frame'de zarı farklı eksenlerde döndür (local space'de döndür)
                    float xRotation = rotationSpeed * Time.deltaTime;
                    float yRotation = rotationSpeed * 0.8f * Time.deltaTime;
                    float zRotation = rotationSpeed * 0.6f * Time.deltaTime;
                    
                    diceObject.transform.Rotate(xRotation, yRotation, zRotation, Space.Self);
                }
                
                elapsedTime += Time.deltaTime;
                yield return null; // Bir sonraki frame'e geç
            }
            
            // Zar sonucunu belirle (1-8 arası)
            finalDiceResult = Random.Range(1, 9);
            Debug.Log($"Silah zarı sonucu: {finalDiceResult}");
            
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
            // Local rotasyonları kullan (dünya koordinatları yerine)
            Quaternion targetRotation = Quaternion.identity;
            
            switch (faceValue)
            {
                case 1:
                    targetRotation = Quaternion.Euler(-45, -22.5f, 0); // 1 üstte
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
            
            // Zarın LOCAL rotasyonunu belirle (parent'ın rotasyonundan bağımsız)
            diceObject.transform.localRotation = targetRotation;
        }
    }
}