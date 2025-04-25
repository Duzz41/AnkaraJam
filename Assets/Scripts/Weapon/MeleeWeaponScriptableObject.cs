using System.Collections;
using UnityEngine;

namespace DotGalacticos.Guns
{
    [CreateAssetMenu(fileName = "Melee Weapon", menuName = "Guns/Melee Weapon", order = 0)]
    public class MeleeWeaponScriptableObject : ScriptableObject
    {
        public GunType Type;
        public string Name;
        public int Damage;
        public float AttackCooldown;
        public float AttackRange;
        public bool isAttacking;

        public Vector3 SpawnPosition;
        public Vector3 SpawnRotation;


        [Header("Audio Settings")]
        public AudioClip[] AttackSounds;
        public AudioClip[] HitSounds;


        [Header("Model Prefab")]
        public GameObject ModelPrefab; // Model prefabı eklendi
        private Animator weaponAnimator;
        [SerializeField]
        private AudioSource modelAudioSource;
        private float lastAttackTime; // Son saldırı zamanı


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

            modelAudioSource = ModelPrefab.GetComponent<AudioSource>();
            weaponAnimator = ModelPrefab.GetComponent<Animator>();
        }

        public void Attack()
        {
            if (Time.time >= lastAttackTime + AttackCooldown)
            {
                isAttacking = true;
                AudioClip AttackSound = AttackSounds[Random.Range(0, AttackSounds.Length)];
                modelAudioSource.PlayOneShot(AttackSound);
                Debug.Log($"{Name} ile saldırıldı! Hasar: {Damage}");

                // Animatördeki Attack trigger'ını tetikle
                if (weaponAnimator != null)
                {
                    weaponAnimator.SetTrigger("Attack");
                }
                else
                {
                    Debug.LogWarning("Weapon animator is not assigned.");
                }

                lastAttackTime = Time.time;
                ActiveMonoBehaviour.StartCoroutine(ResetAttackState());
                ActiveMonoBehaviour.StartCoroutine(DealDamage()); // Hasar verme coroutine'u
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
                                                         // Örneğin: hit.collider.GetComponent<Enemy>().TakeDamage(Damage);
                }
            }
            else
            {
                // Ray çizimini yap
                Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * AttackRange, Color.green, 1f); // Yeşil çizgi, 1 saniye boyunca görünür
            }

            yield return null;
        }

        public void CheckForEnemyHit()
        {
            // Silahın collider'ını al
            Collider weaponCollider = ModelPrefab.GetComponent<Collider>();
            if (weaponCollider != null)
            {
                // Collider ile düşmanları kontrol et
                Collider[] hitColliders = Physics.OverlapBox(weaponCollider.bounds.center, weaponCollider.bounds.extents, ModelPrefab.transform.rotation);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.CompareTag("Enemy"))
                    {
                        Debug.Log($"{Name} düşmana vurdu!"); // Düşmana vurulduğunda log mesajı
                        // Burada düşmana hasar verme kodu yazılacak
                        // Örneğin: hitCollider.GetComponent<Enemy>().TakeDamage(Damage);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Weapon collider is not assigned.");
            }
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

            config.Name = Name;
            config.name = name;
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