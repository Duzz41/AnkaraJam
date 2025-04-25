using UnityEngine;

namespace DotGalacticos.Guns
{
    [CreateAssetMenu(fileName = "Melee Weapon", menuName = "Guns/Melee Weapon", order = 0)]
    public class MeleeWeaponScriptableObject : ScriptableObject
    {
        public GunType Type;
        public string Name;
        public int Damage;
        public float AttackRange;
        public float AttackCooldown;

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
            }
            else
            {
                Debug.Log($"{name} sadece {AttackCooldown} saniye aralıkla saldırabilir.");
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
            config.AttackCooldown = AttackCooldown;
            config.Damage = Damage;
            config.AttackSounds = AttackSounds;
            config.HitSounds = HitSounds;
            config.ModelPrefab = ModelPrefab;

            return config;
        }
    }
}