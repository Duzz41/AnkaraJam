using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace EnemyAssets
{
    [RequireComponent(typeof(NavMeshAgent))]
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
        public AudioClip[] FootstepAudioClips;
        public AudioClip AttackAudioClip;
        public AudioClip ProjectileAudioClip;
        [Range(0, 1)] public float EnemyAudioVolume = 0.5f;

        // State machine
        private enum EnemyState { Idle, Chasing, Attacking, Repositioning }
        private EnemyState currentState = EnemyState.Idle;

        // Components
        private NavMeshAgent _agent;
        private Animator _animator;
        private Transform _playerTransform;
        private float _nextAttackTime = 0f;
        private bool _hasAnimator;

        // Animation variables
        private float _speed;
        private float _animationBlend;

        // Animation IDs
        private int _animIDSpeed;
        private int _animIDMotionSpeed;
        private int _animIDAttack;

        private void Awake()
        {
            // Get components
            _agent = GetComponent<NavMeshAgent>();
            _hasAnimator = TryGetComponent(out _animator);

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
            // Set NavMeshAgent properties
            _agent.speed = MoveSpeed;
            _agent.stoppingDistance = OptimalRange * 0.8f;

            // Assign animation IDs
            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            if (_hasAnimator)
            {
                _animIDSpeed = Animator.StringToHash("Speed");
                _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
                _animIDAttack = Animator.StringToHash("Attack");
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
        }

        private void UpdateState(float distanceToPlayer)
        {
            // Change state based on distance to player
            if (distanceToPlayer <= AttackRange && distanceToPlayer >= OptimalRange * 0.7f)
            {
                // In good shooting range
                currentState = EnemyState.Attacking;
            }
            else if (distanceToPlayer < OptimalRange * 0.7f)
            {
                // Too close to player, back up
                currentState = EnemyState.Repositioning;
            }
            else if (distanceToPlayer <= ChaseRange)
            {
                // Player in sight but out of range, chase
                currentState = EnemyState.Chasing;
            }
            else
            {
                // Player out of sight
                currentState = EnemyState.Idle;
            }
        }

        private void ExecuteState(float distanceToPlayer)
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    // In idle state, enemy doesn't move
                    _agent.isStopped = true;
                    // Set the idle animation
                    SetMovementAnimation(0);
                    break;

                case EnemyState.Chasing:
                    // In chasing state, enemy moves toward player to get in attack range
                    _agent.isStopped = false;
                    _agent.SetDestination(_playerTransform.position);

                    // Set the run animation
                    bool isSprinting = true; // Always sprint when chasing
                    SetMovementAnimation(isSprinting ? SprintSpeed : MoveSpeed);

                    // Rotate toward movement direction
                    if (_agent.velocity.magnitude > 0.1f)
                    {
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            Quaternion.LookRotation(_agent.velocity.normalized),
                            Time.deltaTime * 5f);
                    }
                    break;

                case EnemyState.Repositioning:
                    // Enemy is too close, back away from player
                    _agent.isStopped = false;
                    
                    // Calculate a position at optimal range
                    Vector3 directionFromPlayer = (transform.position - _playerTransform.position).normalized;
                    Vector3 targetPosition = _playerTransform.position + directionFromPlayer * OptimalRange;
                    
                    // Move to that position
                    _agent.SetDestination(targetPosition);
                    
                    // Set movement animation
                    SetMovementAnimation(MoveSpeed);

                    // Still look at player while backing up
                    LookAtPlayer();
                    break;

                case EnemyState.Attacking:
                    // In attacking state, enemy stops and shoots
                    _agent.isStopped = true;

                    // Set the idle animation (minimal movement when attacking)
                    SetMovementAnimation(0);

                    // Look at player
                    LookAtPlayer();

                    // Attack if cooldown allows
                    if (Time.time >= _nextAttackTime)
                    {
                        ShootProjectile();
                        _nextAttackTime = Time.time + AttackCooldown;
                    }
                    break;
            }
        }

        private void LookAtPlayer()
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        private void SetMovementAnimation(float targetSpeed)
        {
            // Get current speed for smooth transition
            float currentSpeed = _animationBlend;

            // Smooth animation blending to target speed
            _animationBlend = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);

            // Round to avoid small floating-point errors
            if (_animationBlend < 0.01f) _animationBlend = 0f;
        }

        private void ShootProjectile()
        {
            // Trigger attack animation
            if (_hasAnimator)
            {
                _animator.SetTrigger(_animIDAttack);
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

        private IEnumerator DelayedProjectileSpawn()
        {
            // Wait a bit for animation
            yield return new WaitForSeconds(0.3f);
            
            // Calculate direction to player, taking into account potential movement
            Vector3 targetPosition = _playerTransform.position;
            
            // Aim slightly higher for better arc
            targetPosition.y += 1.0f;
            
            // Calculate direction
            Vector3 shootDirection = (targetPosition - ProjectileSpawnPoint.position).normalized;
            
            // Spawn projectile
            GameObject projectile = Instantiate(ProjectilePrefab, ProjectileSpawnPoint.position, Quaternion.LookRotation(shootDirection));
            
            // Add projectile component
            EnemyProjectile enemyProjectile = projectile.AddComponent<EnemyProjectile>();
            enemyProjectile.Initialize(shootDirection, ProjectileSpeed, AttackDamage, ProjectileLifetime);
            
            // Play projectile sound
            if (ProjectileAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(ProjectileAudioClip, ProjectileSpawnPoint.position, EnemyAudioVolume);
            }
        }

        private void UpdateAnimator()
        {
            if (_hasAnimator)
            {
                // Update speed parameter (for walking/running animation)
                _animator.SetFloat(_animIDSpeed, _animationBlend);

                // Set motion speed to 1 when moving, 0 when idle
                float motionSpeed = (_animationBlend > 0.1f) ? 1f : 0f;
                _animator.SetFloat(_animIDMotionSpeed, motionSpeed);
            }
        }

        // For footstep sounds - called by animation events
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips != null && FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position, EnemyAudioVolume);
                }
            }
        }

        // Draw gizmos for visualization in editor
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
        }
    }
}