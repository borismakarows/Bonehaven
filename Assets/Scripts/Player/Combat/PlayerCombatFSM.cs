using System;
using System.Collections;
using UnityEngine;

namespace BoneHaven
{
    public class PlayerCombatFSM : MonoBehaviour
    {
        [Header("Dash Settings")]
        [SerializeField] private float evadeSpeed = 19f;
        [SerializeField] private float evadeDuration = 1f;
        [SerializeField] private float iFrameDuration = 0.4f;

        [Header("Melee & Combo Settings")]
        [SerializeField] private float attack1Duration = 0.45f;
        [SerializeField] private float attack2Duration = 0.45f;
        [SerializeField] private float attack3Duration = 0.65f;
        [SerializeField] private float comboBufferWindow = 0.25f;
        [SerializeField] private float slashDamage = 20f;
        [SerializeField] private float finisherDamage = 40f;
        [SerializeField] private float meleeHitboxRadius = 1.6f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Powder & Execution Settings")]
        [SerializeField] private float powderThrowDuration = 0.4f;
        [SerializeField] private float executionWindupDuration = 0.35f;
        [SerializeField] private float executionRange = 1.8f;
        [SerializeField] private int maxGunpowderPouches = 3;
        [SerializeField] private int maxFlintlockAmmo = 4;
        [Header("Execution Buff Settings")]
        [SerializeField] private float executionSpeedBuffMultiplier = 1.25f; 
        [SerializeField] private float executionBuffDuration = 5.0f;
        private float attackSpeedMultiplier = 1.0f;
        private Coroutine buffRoutine;

        [Header("Dependencies")]
        private PlayerLocomotion locomotion;
        private SoftTargetLock targetLock;
        private CombatLunge combatLunge;

                public event Action<int> OnAttackExecuted;
        public event Action OnEvadeExecuted;
                public event Action OnPowderExecuted;
        public event Action OnExecutionTriggered;

        public PlayerCombatState CurrentState { get; private set; } = PlayerCombatState.FreeMovement;


        public bool IsInvulnerable { get; private set; } = false;
        public int GunpowderCount { get; private set; } = 3;
        public int FlintlockAmmo { get; private set; } = 4;

                private PlayerInputManager inputManager;
        private Coroutine activeActionRoutine;
        private bool attackBuffered = false;
        private bool evadeBuffered = false;
        private bool shootBuffered = false;

        private void Awake()

        {
            inputManager = GetComponent<PlayerInputManager>();
            if (locomotion == null) locomotion = GetComponent<PlayerLocomotion>();
            if (targetLock == null) targetLock = GetComponent<SoftTargetLock>();
            if (combatLunge == null) combatLunge = GetComponent<CombatLunge>();
        }

        private void Update()
        {
            if (CurrentState == PlayerCombatState.DashRoll || CurrentState == PlayerCombatState.ExecutionWindup) return;

            if (inputManager == null) return;

            Vector3 moveDir = locomotion.GetCameraRelativeDirection(inputManager.move);

            // Dash Check
            if (inputManager.evade)
            {
                inputManager.evade = false;
                if (CurrentState != PlayerCombatState.ExecutionWindup && CurrentState != PlayerCombatState.Downed)
                {
                    if (CurrentState == PlayerCombatState.FreeMovement)
                    {
                        StartEvade(moveDir);
                        return;
                    }
                    else if (IsAttackingState())
                    {
                        evadeBuffered = true;
                    }
                }
            }

            // FSM Actions
            switch (CurrentState)
            {
                case PlayerCombatState.FreeMovement:
                    if (inputManager.slash)
                    {
                        inputManager.slash = false;
                        StartAttack(1, moveDir);
                    }
                    else if (inputManager.powder)
                    {
                        inputManager.powder = false;
                        if (GunpowderCount > 0) StartPowderThrow();
                    }
                    else if (inputManager.shoot)
                    {
                        inputManager.shoot = false;
                        TryExecutionOrQuickShot(moveDir);
                    }
                    break;

                                case PlayerCombatState.Attack1:
                case PlayerCombatState.Attack2:
                case PlayerCombatState.Attack3:
                    if (inputManager.slash)
                    {
                        inputManager.slash = false;
                        attackBuffered = true;
                    }
                    if (inputManager.shoot)
                    {
                        inputManager.shoot = false;
                        shootBuffered = true;
                    }
                    break;

            }
        }

        private bool IsAttackingState() =>
            CurrentState == PlayerCombatState.Attack1 ||
            CurrentState == PlayerCombatState.Attack2 ||
            CurrentState == PlayerCombatState.Attack3;

        #region Attack & Combos

        private void StartAttack(int comboIndex, Vector3 moveDir)
        {
            if (activeActionRoutine != null) StopCoroutine(activeActionRoutine);
            activeActionRoutine = StartCoroutine(AttackRoutine(comboIndex, moveDir));
        }

                private IEnumerator AttackRoutine(int comboIndex, Vector3 moveDir)
        {
            locomotion.LockMovement(true);
            attackBuffered = false;
            evadeBuffered = false;
            shootBuffered = false;


            Transform target = targetLock != null ? targetLock.GetTarget(new Vector3(inputManager.move.x, 0f, inputManager.move.y), Camera.main.transform) : null;
            if (combatLunge != null) combatLunge.ExecuteLunge(target, moveDir);

            OnAttackExecuted?.Invoke(comboIndex);

            float baseDuration = comboIndex switch
            {
                1 => attack1Duration,
                2 => attack2Duration,
                _ => attack3Duration
            };
            float duration = baseDuration / attackSpeedMultiplier;

            float dmg = (comboIndex == 3) ? finisherDamage : slashDamage;
            bool isFinisher = (comboIndex == 3);

            CurrentState = comboIndex switch
            {
                1 => PlayerCombatState.Attack1,
                2 => PlayerCombatState.Attack2,
                _ => PlayerCombatState.Attack3
            };

            yield return new WaitForSeconds(duration * 0.4f);
            ExecuteMeleeHitbox(dmg, isFinisher);

            float remainingTime = duration * 0.6f;
            float elapsed = 0f;

            while (elapsed < remainingTime)
            {
                elapsed += Time.deltaTime;

                                if (evadeBuffered)
                {
                    StartEvade(moveDir);
                    yield break;
                }

                if (shootBuffered && (remainingTime - elapsed) <= (comboBufferWindow / attackSpeedMultiplier))
                {
                    TryExecutionOrQuickShot(moveDir);
                    yield break;
                }

                if (attackBuffered && (remainingTime - elapsed) <= (comboBufferWindow / attackSpeedMultiplier))

                {
                    if (comboIndex < 3)
                    {
                        StartAttack(comboIndex + 1, moveDir);
                        yield break;
                    }
                }
                yield return null;
            }

            CurrentState = PlayerCombatState.FreeMovement;
            locomotion.LockMovement(false);
            activeActionRoutine = null;
        }

        private void ExecuteMeleeHitbox(float damage, bool isFinisher)
        {
            Vector3 hitCenter = transform.position + transform.forward * 1.2f + Vector3.up * 1f;
            Collider[] hits = Physics.OverlapSphere(hitCenter, meleeHitboxRadius, enemyLayer);

            foreach (var col in hits)
            {
                if (col.TryGetComponent(out IDamageable damageable) && damageable.IsAlive)
                {
                    Vector3 hitDir = (col.transform.position - transform.position).normalized;
                    damageable.TakeDamage(damage, col.bounds.center, hitDir);

                    if (isFinisher && damageable.IsAlive)
                    {
                        damageable.ApplyBlackPowder();
                    }
                }
            }
        }

        #endregion

        #region Evade

        private void StartEvade(Vector3 moveDir)
        {
            if (activeActionRoutine != null) StopCoroutine(activeActionRoutine);
            if (combatLunge != null) combatLunge.CancelLunge();
            activeActionRoutine = StartCoroutine(EvadeRoutine(moveDir));
        }

        private IEnumerator EvadeRoutine(Vector3 moveDir)
        {
            CurrentState = PlayerCombatState.DashRoll;
            IsInvulnerable = true;
            locomotion.LockMovement(true);

            Vector3 rawDir = moveDir.sqrMagnitude > 0.01f ? -moveDir : -transform.forward;
            rawDir.y = 0f;
            Vector3 evadeDir = rawDir.normalized;

            OnEvadeExecuted?.Invoke();

            float elapsed = 0f;
            while (elapsed < evadeDuration)
            {
                elapsed += Time.deltaTime;
                
                if (elapsed >= iFrameDuration) 
                {
                    IsInvulnerable = false;
                }
               
                locomotion.ManualMove(evadeDir * (evadeSpeed * Time.deltaTime));
                yield return null;
            }

            IsInvulnerable = false;
            CurrentState = PlayerCombatState.FreeMovement;
            locomotion.LockMovement(false);
            activeActionRoutine = null;
        }

        #endregion

        #region Powder & Execution

        private void StartPowderThrow()
        {
            if (activeActionRoutine != null) StopCoroutine(activeActionRoutine);
            activeActionRoutine = StartCoroutine(PowderThrowRoutine());
        }

        private IEnumerator PowderThrowRoutine()
        {
            CurrentState = PlayerCombatState.BlackPowderThrow;
            locomotion.LockMovement(true);
            GunpowderCount--;
            OnPowderExecuted?.Invoke();

            yield return new WaitForSeconds(powderThrowDuration * 0.35f);

            Vector3 origin = transform.position + Vector3.up * 1f;
            Collider[] hits = Physics.OverlapSphere(origin, 3.5f, enemyLayer);

            foreach (var col in hits)
            {
                Vector3 toTarget = (col.bounds.center - origin).normalized;
                if (Vector3.Angle(transform.forward, toTarget) <= 60f * 0.5f)
                {
                    if (col.TryGetComponent(out IDamageable damageable) && damageable.IsAlive)
                    {
                        damageable.ApplyBlackPowder();
                    }
                }
            }

            yield return new WaitForSeconds(powderThrowDuration * 0.65f);
            CurrentState = PlayerCombatState.FreeMovement;
            locomotion.LockMovement(false);
            activeActionRoutine = null;
        }

                private void TryExecutionOrQuickShot(Vector3 moveDir)
                {
                    if (FlintlockAmmo <= 0) return;
                    shootBuffered = false;

                    Transform target = targetLock != null ? targetLock.GetTarget(new Vector3(inputManager.move.x, 0f, inputManager.move.y), Camera.main.transform) : null;
                    IDamageable damageable = null;
                    bool isStunnedOrUnbalanced = false;

                    if (target != null && target.TryGetComponent(out damageable))
                    {
                        float dist = Vector3.Distance(transform.position, target.position);
                        isStunnedOrUnbalanced = (damageable.IsStunned || damageable.IsUnbalanced) && dist <= (executionRange + 0.5f);
                    }

                    // Always start the execution routine/animation regardless of target state
                    if (activeActionRoutine != null) StopCoroutine(activeActionRoutine);
                    activeActionRoutine = StartCoroutine(ExecutionRoutine(target, damageable, isStunnedOrUnbalanced));
                }

                private IEnumerator ExecutionRoutine(Transform target, IDamageable damageable, bool isFinisher)
                {
                    CurrentState = PlayerCombatState.ExecutionWindup;
                    locomotion.LockMovement(true);
                    FlintlockAmmo--;

                    if (target != null)
                    {
                        Vector3 lookDir = (target.position - transform.position);
                        lookDir.y = 0f;
                        if (lookDir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(lookDir.normalized);
                    }

                    OnExecutionTriggered?.Invoke();

                    yield return new WaitForSeconds(executionWindupDuration);

                    if (damageable != null && damageable.IsAlive)
                    {
                        if (isFinisher)
                        {
                            damageable.Execute(transform);
                            ApplyExecutionSpeedBuff();
                        }
                        else
                        {
                            // Regular damage if not stunned/unbalanced
                            damageable.TakeDamage(30f, target.position, transform.forward);
                        }
                    }

                    CurrentState = PlayerCombatState.FreeMovement;
                    locomotion.LockMovement(false);
                    activeActionRoutine = null;
                }


        private void ApplyExecutionSpeedBuff()
        {
            if (buffRoutine != null) StopCoroutine(buffRoutine);
            buffRoutine = StartCoroutine(ExecutionSpeedBuffTimerRoutine());
        }

        private IEnumerator ExecutionSpeedBuffTimerRoutine()
        {
            attackSpeedMultiplier = executionSpeedBuffMultiplier; 
            yield return new WaitForSeconds(executionBuffDuration); 
            attackSpeedMultiplier = 1.0f; 
            buffRoutine = null;
        }

        #endregion

        public void AddGunpowder(int amount) => GunpowderCount = Mathf.Clamp(GunpowderCount + amount, 0, maxGunpowderPouches);
        public void AddAmmo(int amount) => FlintlockAmmo = Mathf.Clamp(FlintlockAmmo + amount, 0, maxFlintlockAmmo);
    }
}