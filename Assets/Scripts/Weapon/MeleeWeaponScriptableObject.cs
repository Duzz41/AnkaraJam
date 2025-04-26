using System.Collections;
using UnityEngine;
using Cinemachine; // Cinemachine kütüphanesini ekleyin
namespace DotGalacticos.Guns
{
    [CreateAssetMenu(fileName = "Melee Weapon", menuName = "Guns/Melee Weapon", order = 0)]
    public class MeleeWeaponScriptableObject : ScriptableObject
    {
        public GunType Type;
        public GunUseType usageType;
        public string Name;
        public int Damage;
        public float AttackCooldown;

        [Header("Attack Settings")]
        public float AttackRange;
        public bool isAttacking;
        public float throwForce;
        public float rotationSpeed;
        public float lifetime;
        public Vector3 SpawnPosition;
        public Vector3 SpawnRotation;


        [Header("Audio Settings")]
        public AudioClip[] AttackSounds;
        public AudioClip[] HitSounds;


        [Header("Model Prefab")]
        public GameObject ModelPrefab; // Model prefabı eklendi
        private Animator weaponAnimator;
        public GameObject projectilePrefab;
        [SerializeField]
        private AudioSource modelAudioSource;
        private float lastAttackTime; // Son saldırı zamanı
        public float projectileSpeed = 20f; // Merminin hızı

        [Header("Camera Shake Settings")]
        public float shakeDuration = 0.2f; // Shake süresi
        public float shakeAmplitude = 1f; // Shake genliği
        public float shakeFrequency = 2f; // Shake frekansı

        private CinemachineBasicMultiChannelPerlin perlin; // Cinemachine bileşeni

        private MonoBehaviour ActiveMonoBehaviour;

        public void Spawn(
            Transform Parent,
            MonoBehaviour ActiveMonoBehaviour,
            Camera ActiveCamera = null
        )
        {
            this.ActiveMonoBehaviour = ActiveMonoBehaviour;

            ModelPrefab = Instantiate(ModelPrefab);
            ModelPrefab.transform.SetParent(Parent, false);
            ModelPrefab.transform.localPosition = SpawnPosition;
            ModelPrefab.transform.localRotation = Quaternion.Euler(SpawnRotation);
            lastAttackTime = -AttackCooldown;
            modelAudioSource = ModelPrefab.GetComponent<AudioSource>();
            weaponAnimator = ModelPrefab.GetComponent<Animator>();
            SoundManager.instance.RegisterAudioSource(modelAudioSource);
        }
        public void Despawn()
        {

            Debug.Log($"Destroying {ModelPrefab.name}");
            Destroy(ModelPrefab);

            modelAudioSource = null;
            weaponAnimator = null;
        }
        public void Attack()
        {
            if (Time.time >= lastAttackTime + AttackCooldown)
            {
                isAttacking = true;
                AudioClip AttackSound = AttackSounds[Random.Range(0, AttackSounds.Length)];
                modelAudioSource.PlayOneShot(AttackSound);

                // Melee attack logic
                if (weaponAnimator != null)
                {
                    weaponAnimator.SetTrigger("Attack");
                }
                else
                {
                    Debug.LogWarning("Weapon animator is not assigned.");
                }

                if (usageType == GunUseType.Melee)
                {
                    lastAttackTime = Time.time; // Saldırı zamanını güncelle
                    ActiveMonoBehaviour.StartCoroutine(ResetAttackState());
                    ActiveMonoBehaviour.StartCoroutine(DealDamage());
                }
                else
                {
                    lastAttackTime = Time.time;
                    ActiveMonoBehaviour.StartCoroutine(ResetAttackState());
                }
            }
            else
            {
                Debug.Log($"{name} sadece {AttackCooldown} saniye aralıkla saldırabilir.");
            }
        }
        private IEnumerator ResetAttackState()
        {
            // AttackCooldown süresi boyunca bekle
            yield return new WaitForSeconds(AttackCooldown);
            isAttacking = false; // Saldırı bitti
        }

        private IEnumerator DealDamage()
        {
            // Saldırı sırasında düşmanları kontrol et
            RaycastHit hit;
            Vector3 rayOrigin = Camera.main.transform.position; // Ray başlangıç noktası
            Vector3 rayDirection = Camera.main.transform.forward; // Ray yönü

            // Raycast ile düşmanı kontrol et
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, AttackRange))
            {
                // Ray çizimini yap
                Debug.DrawLine(rayOrigin, hit.point, Color.red, 1f); // Kırmızı çizgi, 1 saniye boyunca görünür

                if (hit.collider.CompareTag("Enemy"))
                {
                    Debug.Log($"{Name} düşmana vurdu!"); // Düşmana vurulduğunda log mesajı
                                                         // Burada düşmana hasar verme kodu yazılacak
                    hit.collider.GetComponent<EnemyHealth>().TakeDamage(Damage);
                }
            }
            else
            {
                // Ray çizimini yap
                Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * AttackRange, Color.green, 1f); // Yeşil çizgi, 1 saniye boyunca görünür
            }

            yield return null;
        }

        public void Hit(Vector3 position)
        {
            AudioClip HitSound = HitSounds[Random.Range(0, HitSounds.Length)];
            AudioSource.PlayClipAtPoint(HitSound, position);
        }

        public object Clone()
        {
            MeleeWeaponScriptableObject config = CreateInstance<MeleeWeaponScriptableObject>();
            config.Type = Type;
            config.usageType = usageType;

            config.lifetime = lifetime;
            config.rotationSpeed = rotationSpeed;
            config.Name = Name;
            config.name = name;
            config.throwForce = throwForce;
            config.AttackRange = AttackRange;
            config.isAttacking = isAttacking;
            config.AttackCooldown = AttackCooldown;
            config.Damage = Damage;
            config.AttackSounds = AttackSounds;
            config.HitSounds = HitSounds;
            config.ModelPrefab = ModelPrefab;

            return config;
        }
    }
}