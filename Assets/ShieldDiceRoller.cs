using System.Collections;
using UnityEngine;

namespace DotGalacticos.Guns
{
    public class ShieldDiceRoller : MonoBehaviour
    {
        [SerializeField] private float rollDuration = 3f; // Zar atma süresi
        [SerializeField] private GameObject shieldDiceObject; // Kalkan zarı modeli
        [SerializeField] private Transform shieldHandTransform; // Kalkan zarı eli transform'u
        [SerializeField] private float rotationSpeed = 720f; // Zar dönüş hızı (derece/saniye)
        [SerializeField] private float handShowDuration = 5f; // Elin ekranda kalma süresi

        [Header("Shield Settings")]
        [SerializeField] private GameObject shieldObject; // Kalkan objesi
        [SerializeField] private float baseShieldDuration = 0.2f; // Temel kalkan süresi (en düşük)
        [SerializeField] private float maxShieldDuration = 2f; // En yüksek kalkan süresi

        [Header("Sync with Weapon Dice")]
        [SerializeField] private DiceRoller weaponDiceRoller; // Silah zarı referansı

        private bool isRolling = false;
        private int shieldDiceResult = 0;
        private float currentShieldDuration = 0f;
        private bool shieldActive = false;
        [SerializeField]
        private float shieldTimeRemaining = 0f;
        private Quaternion diceInitialRotation;

        void Start()
        {
            // Başlangıçta zarı, eli ve kalkanı gizle
            if (shieldDiceObject != null)
            {
                shieldDiceObject.SetActive(false);
                diceInitialRotation = shieldDiceObject.transform.localRotation;
            }

            if (shieldHandTransform != null)
                shieldHandTransform.gameObject.SetActive(false);

            if (shieldObject != null)
                shieldObject.SetActive(false);

            // Varsayılan bir kalkan süresi belirle
            shieldTimeRemaining = baseShieldDuration;
        }

        void Update()
        {
            // Fare sağ tuşu basılı ise ve kalkan aktif değilse kalkanı etkinleştir
            if (Input.GetMouseButton(1) && shieldActive == false && shieldTimeRemaining > 0)
            {
                ActivateShield();
            }

            // Fare sağ tuşu bırakıldığında kalkanı devre dışı bırak
            if (Input.GetMouseButtonUp(1) && shieldActive)
            {
                DeactivateShield();
            }

            // Kalkan aktifse ve süre dolmuşsa devre dışı bırak
            if (shieldActive)
            {
                shieldTimeRemaining -= Time.deltaTime;
                if (shieldTimeRemaining <= 0)
                {
                    DeactivateShield();
                }
            }
        }

        // Ana zar ile senkronize bir şekilde çalıştırmak için
        public void StartRollingWithWeaponDice()
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
            if (shieldHandTransform != null)
                shieldHandTransform.gameObject.SetActive(true);

            // Zarı göster
            if (shieldDiceObject != null)
                shieldDiceObject.SetActive(true);

            float elapsedTime = 0f;

            // Zarın rastgele dönmesi
            while (elapsedTime < rollDuration)
            {
                if (shieldDiceObject != null)
                {
                    // Her frame'de zarı farklı eksenlerde döndür (local space'de döndür)
                    float xRotation = rotationSpeed * Time.deltaTime;
                    float yRotation = rotationSpeed * 0.8f * Time.deltaTime;
                    float zRotation = rotationSpeed * 0.6f * Time.deltaTime;

                    shieldDiceObject.transform.Rotate(xRotation, yRotation, zRotation, Space.Self);
                }

                elapsedTime += Time.deltaTime;
                yield return null; // Bir sonraki frame'e geç
            }

            // Zar sonucunu belirle (1-8 arası)
            shieldDiceResult = Random.Range(1, 9);
            Debug.Log($"Kalkan zarı sonucu: {shieldDiceResult}");

            // Kalkan süresini hesapla (1-8 arası, lineer artan)
            shieldTimeRemaining = Mathf.Lerp(baseShieldDuration, maxShieldDuration, (shieldDiceResult - 1) / 7f);
            Debug.Log($"Kalkan süresi: {shieldTimeRemaining} saniye");

            // Zarın son pozisyonunu ayarla (sonuca göre)
            if (shieldDiceObject != null)
            {
                SetDiceFace(shieldDiceResult);
            }

            // Biraz bekle ve sonra eli ve zarı gizle
            yield return new WaitForSeconds(handShowDuration);

            // Eli ve zarı gizle
            if (shieldHandTransform != null)
                shieldHandTransform.gameObject.SetActive(false);

            if (shieldDiceObject != null)
                shieldDiceObject.SetActive(false);

            isRolling = false;
        }

        private void SetDiceFace(int faceValue)
        {
            Quaternion targetRotation = Quaternion.identity;

            switch (faceValue)
            {
                case 1:
                    targetRotation = Quaternion.Euler(-45, -22.5f, 0);
                    break;
                case 2:
                    targetRotation = Quaternion.Euler(-123.5f, -20, 90);
                    break;
                case 3:
                    targetRotation = Quaternion.Euler(-53.5f, -20, 90);
                    break;
                case 4:
                    targetRotation = Quaternion.Euler(320f, -20, -90);
                    break;
                case 5:
                    targetRotation = Quaternion.Euler(-45, -22.5f, 180);
                    break;
                case 6:
                    targetRotation = Quaternion.Euler(245f, -20, -90);
                    break;
                case 7:
                    targetRotation = Quaternion.Euler(-117.5f, -22.5f, 0);
                    break;
                case 8:
                    targetRotation = Quaternion.Euler(-117.5f, -22.5f, 180);
                    break;
            }

            shieldDiceObject.transform.localRotation = targetRotation;
        }

        private void ActivateShield()
        {

                shieldActive = true;

                if (shieldObject != null)
                    shieldObject.SetActive(true);

                Debug.Log($"Kalkan aktif edildi. Süre: {shieldTimeRemaining} saniye");
            
        }

        private void DeactivateShield()
        {
            shieldActive = false;

            if (shieldObject != null)
                shieldObject.SetActive(false);

            Debug.Log("Kalkan devre dışı bırakıldı.");
        }
    }
}