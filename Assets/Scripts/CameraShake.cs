using System.Collections;
using UnityEngine;
using Cinemachine; // Cinemachine kütüphanesini ekleyin
public class CameraShake : MonoBehaviour
{
    [SerializeField]
    private CinemachineBasicMultiChannelPerlin perlin; // Cinemachine bileşeni
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CinemachineVirtualCamera virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>().GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }
    public IEnumerator ShakeCamera()
    {
        Debug.Log($"psdk<gjn<ospdgndaps");
        if (perlin != null)
        {

            perlin.m_AmplitudeGain = Random.Range(-1f, 1f);
            perlin.m_FrequencyGain = 1;

            yield return new WaitForSeconds(1);

            perlin.m_AmplitudeGain = 0; // Shake'i durdur
            perlin.m_FrequencyGain = 0; // Shake'i durdur
        }
    }
}
