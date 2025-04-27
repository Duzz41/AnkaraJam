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
                perlin.m_AmplitudeGain = meleeWeapon.shakeAmplitude;
                perlin.m_FrequencyGain = meleeWeapon.shakeFrequency;

                yield return new WaitForSeconds(meleeWeapon.shakeDuration);

                perlin.m_AmplitudeGain = 0; // Shake'i durdur
                perlin.m_FrequencyGain = 0; // Shake'i durdur
            }
        }
    }
}