using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace EnemyAssets
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [Tooltip("Move speed of the enemy in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the enemy in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Tooltip("Attack range - how close enemy needs to be to attack")]
        public float AttackRange = 1.5f;

        [Tooltip("Chase range - how far enemy can see the player")]
        public float ChaseRange = 10.0f;

        [Tooltip("How much damage enemy deals per attack")]
        public int AttackDamage = 10;

        [Tooltip("Time between attacks")]
        public float AttackCooldown = 2.0f;

        [Header("Audio")]
        public AudioClip[] FootstepAudioClips;
        public AudioClip AttackAudioClip;
        [Range(0, 1)] public float EnemyAudioVolume = 0.5f;

        // State machine
        private enum EnemyState { Idle, Chasing, Attacking }
        private EnemyState currentState = EnemyState.Idle;

        // Components
        private NavMeshAgent _agent;
        private Animator _animator;
        private Transform _playerTransform;
        private float _nextAttackTime = 0f;
        private bool _hasAnimator;

        // Animation variables (simplified from ThirdPersonController)
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
        }

        private void Start()
        {
            // Set NavMeshAgent properties
            _agent.speed = MoveSpeed;
            _agent.stoppingDistance = AttackRange;

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
            ExecuteState();

            // Update animator
            UpdateAnimator();
        }

        private void UpdateState(float distanceToPlayer)
        {
            // Change state based on distance to player
            if (distanceToPlayer <= AttackRange)
            {
                currentState = EnemyState.Attacking;
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

        private void ExecuteState()
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
                    // In chasing state, enemy moves toward player
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

                case EnemyState.Attacking:
                    // In attacking state, enemy stops and attacks
                    _agent.isStopped = true;

                    // Set the idle animation (no movement when attacking)
                    SetMovementAnimation(0);

                    // Look at player
                    Vector3 direction = (_playerTransform.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

                    // Attack if cooldown allows
                    if (Time.time >= _nextAttackTime)
                    {
                        Attack();
                        _nextAttackTime = Time.time + AttackCooldown;
                    }
                    break;
            }
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

        private void Attack()
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

        // This function can be called from animation events
        private void OnAttackHit()
        {
            // Deal damage to player if in range
            if (_playerTransform != null &&
                Vector3.Distance(transform.position, _playerTransform.position) <= AttackRange)
            {
                // Get player health component and damage it
                PlayerHealth playerHealth = _playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(AttackDamage);
                }
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

            // Draw chase range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, ChaseRange);
        }
    }
}