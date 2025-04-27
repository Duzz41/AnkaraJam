using System.Collections;
using DotGalacticos.Guns.Demo;
using UnityEngine;
using Cinemachine; // Cinemachine kütüphanesini ekleyin

namespace DotGalacticos.Guns
{
    public class ShootController : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponSelector GunSelector; // Silah seçicisi
        [SerializeField]
        private CinemachineBasicMultiChannelPerlin perlin; // Cinemachine bileşeni
        void Start()
        {
            CinemachineVirtualCamera virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>().GetComponent<CinemachineVirtualCamera>();
            if (virtualCamera != null)
            {
                perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }

        private void Update()
        {
            // Sol fare tuşuna basıldığında saldır
            if (Input.GetMouseButtonDown(0)) // 0, sol fare tuşunu temsil eder
            {

                Attack();
            }


            if (GunSelector.ActiveWeapon.isAttacking == true)

            {
                StartCoroutine(ShakeCamera(GunSelector.ActiveWeapon));
            }
        }

        private void Attack()
        {
            if (GunSelector.ActiveWeapon != null)
            {
                GunSelector.ActiveWeapon.Attack(); // Silahın Attack metodunu çağır

            }
            else
            {
                Debug.LogWarning("No weapon assigned to the player.");
            }
        }
        private IEnumerator ShakeCamera(MeleeWeaponScriptableObject meleeWeapon)
        {
            if (perlin != null)
            {
                // Başlangıç sarsıntı değerlerini ayarla
                perlin.m_AmplitudeGain = meleeWeapon.shakeAmplitude;
                perlin.m_FrequencyGain = meleeWeapon.shakeFrequency;

                // Sarsıntı süresi boyunca bekle
                yield return new WaitForSeconds(meleeWeapon.shakeDuration);

                // Sarsıntıyı yavaşça azalt
                float elapsedTime = 0f;
                float shakeDuration = meleeWeapon.shakeDuration; // Sarsıntıyı azaltma süresi
                float startAmplitude = perlin.m_AmplitudeGain;
                float startFrequency = perlin.m_FrequencyGain;

                while (elapsedTime < shakeDuration)
                {
                    float t = elapsedTime / shakeDuration; // 0 ile 1 arasında bir oran
                    perlin.m_AmplitudeGain = Mathf.Lerp(startAmplitude, 0, t); // Amplitüdü yavaşça azalt
                    perlin.m_FrequencyGain = Mathf.Lerp(startFrequency, 0, t); // Frekansı yavaşça azalt

                    elapsedTime += Time.deltaTime; // Geçen süreyi güncelle
                    yield return null; // Bir sonraki frame'e geç
                }

                // Son durumda sıfır yap
                //perlin.m_AmplitudeGain = 0;
                //perlin.m_FrequencyGain = 0;
            }
        }
    }
}