using UnityEngine;
using System.Collections;

namespace EnemyAssets
{
    // NavMesh yerine manuel hareket sistemi kullanan uçan düşman
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class RangedEnemyController : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [Tooltip("Move speed of the enemy in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the enemy in m/s")]
        public float SprintSpeed = 4.0f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Tooltip("Attack range - how far enemy can shoot")]
        public float AttackRange = 10.0f;

        [Tooltip("Optimal range - enemy tries to keep this distance from player")]
        public float OptimalRange = 8.0f;

        [Tooltip("Chase range - how far enemy can see the player")]
        public float ChaseRange = 15.0f;

        [Tooltip("How much damage enemy deals per attack")]
        public int AttackDamage = 5;

        [Tooltip("Time between attacks")]
        public float AttackCooldown = 2.0f;
        
        [Header("Floating Settings")]
        [Tooltip("Base height above ground")]
        public float baseHeight = 3.0f;
        
        [Tooltip("Hover oscillation speed")]
        public float hoverSpeed = 1.0f;
        
        [Tooltip("Maximum hover oscillation amplitude")]
        public float hoverAmplitude = 0.2f;
        
        [Tooltip("Height variation based on distance")]
        public float heightVariation = 2.0f;
        
        [Header("Movement Settings")]
        [Tooltip("Rotation speed")]
        public float rotationSpeed = 5.0f;
        
        [Tooltip("Smoothness of movement")]
        public float movementSmoothness = 8.0f;
        
        [Tooltip("Height check frequency")]
        public float heightCheckFrequency = 0.1f;

        [Header("Projectile Settings")]
        [Tooltip("Projectile prefab to spawn")]
        public GameObject ProjectilePrefab;

        [Tooltip("Speed of the projectile")]
        public float ProjectileSpeed = 15.0f;

        [Tooltip("Lifetime of the projectile in seconds")]
        public float ProjectileLifetime = 5.0f;

        [Tooltip("Spawn point for projectiles")]
        public Transform ProjectileSpawnPoint;

        [Header("Audio")]
        public AudioClip AttackAudioClip;
        public AudioClip ProjectileAudioClip;
        [Range(0, 1)] public float EnemyAudioVolume = 0.5f;

        // State machine
        private enum EnemyState { Idle, Chasing, Attacking, Repositioning }
        private EnemyState currentState = EnemyState.Idle;

        // Components
        private Rigidbody _rigidbody;
        private Animator _animator;
        private Transform _playerTransform;
        private float _nextAttackTime = 0f;
        private bool _hasAnimator;

        // Movement variables
        private Vector3 _targetPosition;
        private Vector3 _currentVelocity;
        private float _lastHeightCheck;
        private float _groundHeight;

        // Animation variables
        private float _animationBlend;

        // Animation IDs
        private int _animIDSpeed;
        private int _animIDMotionSpeed;
        private int _animIDAttack;
        private int _animIDHit;

        private void Awake()
        {
            // Get components
            _rigidbody = GetComponent<Rigidbody>();
            _hasAnimator = TryGetComponent(out _animator);

            // Rigidbody ayarları - yerçekimi ve rotasyon kısıtlamaları
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            // Find player (assuming it has "Player" tag)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("Player not found! Make sure the player has a 'Player' tag.");
            }

            // If ProjectileSpawnPoint is not set, use this transform
            if (ProjectileSpawnPoint == null)
            {
                ProjectileSpawnPoint = transform;
            }
        }

        private void Start()
        {
            // Assign animation IDs
            AssignAnimationIDs();
            
            // Initial position setup
            _targetPosition = transform.position;
            
            // Find initial ground height
            UpdateGroundHeight();
        }

        private void AssignAnimationIDs()
        {
            if (_hasAnimator)
            {
                _animIDSpeed = Animator.StringToHash("Speed");
                _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
                _animIDAttack = Animator.StringToHash("Attack");
                _animIDHit = Animator.StringToHash("Hit");
            }
        }

        private void Update()
        {
            if (_playerTransform == null) return;

            // Calculate distance to player
            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

            // Update state machine
            UpdateState(distanceToPlayer);

            // Execute current state behavior
            ExecuteState(distanceToPlayer);

            // Update animator
            UpdateAnimator();
            
            // Periodically update ground height
            if (Time.time - _lastHeightCheck > heightCheckFrequency)
            {
                UpdateGroundHeight();
                _lastHeightCheck = Time.time;
            }
        }

        private void FixedUpdate()
        {
            // Physics-based movement in FixedUpdate
            UpdateMovement();
        }

        private void UpdateGroundHeight()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 50f, ~0, QueryTriggerInteraction.Ignore))
            {
                _groundHeight = hit.point.y;
            }
        }

        private void UpdateState(float distanceToPlayer)
        {
            // Change state based on distance to player
            if (distanceToPlayer <= AttackRange && distanceToPlayer >= OptimalRange * 0.7f)
            {
                currentState = EnemyState.Attacking;
            }
            else if (distanceToPlayer < OptimalRange * 0.7f)
            {
                currentState = EnemyState.Repositioning;
            }
            else if (distanceToPlayer <= ChaseRange)
            {
                currentState = EnemyState.Chasing;
            }
            else
            {
                currentState = EnemyState.Idle;
            }
        }

        private void ExecuteState(float distanceToPlayer)
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    // Hover in place
                    _targetPosition = new Vector3(
                        transform.position.x,
                        CalculateHoverHeight(transform.position),
                        transform.position.z
                    );
                    SetMovementAnimation(0);
                    break;

                case EnemyState.Chasing:
                    // Chase player
                    Vector3 targetChasePosition = _playerTransform.position;
                    targetChasePosition.y = CalculateHoverHeight(_playerTransform.position);
                    
                    // Move towards player
                    _targetPosition = targetChasePosition;
                    SetMovementAnimation(SprintSpeed);
                    
                    // Look at player
                    LookAtTarget(_playerTransform.position);
                    break;

                case EnemyState.Repositioning:
                    // Back away from player
                    Vector3 directionFromPlayer = (transform.position - _playerTransform.position).normalized;
                    Vector3 repoPosition = _playerTransform.position + directionFromPlayer * OptimalRange;
                    repoPosition.y = CalculateHoverHeight(repoPosition);
                    
                    _targetPosition = repoPosition;
                    SetMovementAnimation(MoveSpeed);
                    
                    // Still look at player while backing up
                    LookAtTarget(_playerTransform.position);
                    break;

                case EnemyState.Attacking:
                    // Hover in place and attack
                    _targetPosition = new Vector3(
                        transform.position.x,
                        CalculateHoverHeight(transform.position),
                        transform.position.z
                    );
                    
                    SetMovementAnimation(0);
                    LookAtTarget(_playerTransform.position);

                    // Attack if cooldown allows
                    if (Time.time >= _nextAttackTime)
                    {
                        ShootProjectile();
                        _nextAttackTime = Time.time + AttackCooldown;
                    }
                    break;
            }
        }

        private float CalculateHoverHeight(Vector3 position)
        {
            // Calculate hover height with oscillation
            float oscillation = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            
            // Add some height variation based on distance to player
            float distanceToPlayer = Vector3.Distance(position, _playerTransform.position);
            float heightAdjustment = Mathf.Clamp(distanceToPlayer / ChaseRange, 0, 1) * heightVariation;
            
            return _groundHeight + baseHeight + oscillation + heightAdjustment;
        }

        private void UpdateMovement()
        {
            // Smooth movement towards target position
            Vector3 targetVelocity = (_targetPosition - transform.position) * movementSmoothness;
            
            // Clamp velocity to max speed
            if (targetVelocity.magnitude > SprintSpeed)
            {
                targetVelocity = targetVelocity.normalized * SprintSpeed;
            }
            
            // Apply velocity
            _rigidbody.linearVelocity = Vector3.SmoothDamp(
                _rigidbody.linearVelocity,
                targetVelocity,
                ref _currentVelocity,
                1f / movementSmoothness
            );
        }

        private void LookAtTarget(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0; // Only rotate on the horizontal plane
            
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotationSpeed
                );
            }
        }

        private void SetMovementAnimation(float targetSpeed)
        {
            // Smooth animation blending
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            
            if (_animationBlend < 0.01f) _animationBlend = 0f;
        }

        private void ShootProjectile()
        {
            // Trigger attack animation
            if (_hasAnimator)
            {
               // _animator.SetTrigger(_animIDAttack);
                
                // Play a more dramatic attack animation sequence
//                StartCoroutine(PlayAttackAnimationSequence());
            }

            // Play attack sound
            if (AttackAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(AttackAudioClip, transform.position, EnemyAudioVolume);
            }

            // Instantiate projectile at spawn point
            if (ProjectilePrefab != null)
            {
                StartCoroutine(DelayedProjectileSpawn());
            }
        }

        private IEnumerator PlayAttackAnimationSequence()
        {
            // Dramatic attack animation
            if (_hasAnimator)
            {
                _animator.SetBool("IsCharging", true);
            }
            
            // Rise up during attack
            float chargeTime = 0.5f;
            Vector3 startPos = transform.position;
            Vector3 chargePos = transform.position + new Vector3(0, 0.5f, 0);
            
            float elapsed = 0;
            while (elapsed < chargeTime)
            {
                Vector3 newPosition = Vector3.Lerp(startPos, chargePos, elapsed / chargeTime);
                _rigidbody.MovePosition(newPosition);
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            
            yield return new WaitForSeconds(0.2f);
            
            if (_hasAnimator)
            {
                _animator.SetBool("IsCharging", false);
            }
            
            // Lower back down
            elapsed = 0;
            while (elapsed < chargeTime * 0.7f)
            {
                Vector3 newPosition = Vector3.Lerp(chargePos, startPos, elapsed / (chargeTime * 0.7f));
                _rigidbody.MovePosition(newPosition);
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }

        // RangedEnemyController.cs içindeki DelayedProjectileSpawn metodunu güncelle

        private IEnumerator DelayedProjectileSpawn()
        {
            yield return new WaitForSeconds(0.7f);
    
            Vector3 targetPosition = _playerTransform.position;
            targetPosition.y += 2.5f; // Player'ın göğüs hizasına hedefle
    
            Vector3 shootDirection = (targetPosition - ProjectileSpawnPoint.position).normalized;
    
            GameObject projectile = Instantiate(ProjectilePrefab, ProjectileSpawnPoint.position, Quaternion.LookRotation(shootDirection));
    
            // EnemyProjectile component'i al veya ekle
            EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();
            if (enemyProjectile == null)
            {
                enemyProjectile = projectile.AddComponent<EnemyProjectile>();
            }
    
            // Projectile'ı initialize et
            enemyProjectile.Initialize(shootDirection, ProjectileSpeed, AttackDamage, ProjectileLifetime);
    
            // Ek güvenlik: Collider kontrolü
            Collider projectileCollider = projectile.GetComponent<Collider>();
            if (projectileCollider == null)
            {
                // Eğer collider yoksa, ekle
                SphereCollider sphere = projectile.AddComponent<SphereCollider>();
                sphere.isTrigger = true;
                sphere.radius = 0.5f; // Boyutu ayarla
            }
    
            // Ses çal
            if (ProjectileAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(ProjectileAudioClip, ProjectileSpawnPoint.position, EnemyAudioVolume);
            }
        }

        private void UpdateAnimator()
        {
            if (_hasAnimator)
            {
                // Update speed parameter
                _animator.SetFloat(_animIDSpeed, _animationBlend);

                // Set motion speed
                float motionSpeed = (_animationBlend > 0.1f) ? 1f : 0f;
                _animator.SetFloat(_animIDMotionSpeed, motionSpeed);
            }
        }

        public void TakeDamage()
        {
            if (_hasAnimator)
            {
                _animator.SetTrigger(_animIDHit);
                StartCoroutine(HitVisualFeedback());
            }
        }

        private IEnumerator HitVisualFeedback()
        {
            Vector3 originalPosition = transform.position;
            
            float shakeDuration = 0.2f;
            float shakeIntensity = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
                transform.position = originalPosition + shakeOffset;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.position = originalPosition;
        }

        // Draw gizmos for visualization
        private void OnDrawGizmosSelected()
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);

            // Draw optimal range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, OptimalRange);

            // Draw chase range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, ChaseRange);
            
            // Draw height line
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 10);
        }
    }
}