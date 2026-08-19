using System;
using System.Collections;
using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCombatFSM : MonoBehaviour
    {
        [Header("Required Dependencies")]
        [SerializeField] private PlayerInputManager inputManager;
        [SerializeField] private SoftTargetLock targetLock;
        [SerializeField] private CombatLunge combatLunge;
        [SerializeField] private Transform cameraTransform;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float rotationSmoothTime = 0.1f;

        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed = 10f;
        [SerializeField] private float dashDuration = 0.35f;
        [SerializeField] private float iFrameDuration = 0.2f;

        [Header("Melee & Combo Timings")]
        [SerializeField] private float attack1Duration = 0.45f;
        [SerializeField] private float attack2Duration = 0.45f;
        [SerializeField] private float attack3Duration = 0.65f;
        [SerializeField] private float comboBufferWindow = 0.25f;

        [Header("Damage Settings")]
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

        // Decoupled Observer Events for Animation & Audio
        public event Action<int> OnAttackExecuted;
        public event Action OnDashExecuted;
        public event Action OnPowderExecuted;
        public event Action OnExecutionTriggered;
        public event Action<float> OnSpeedUpdated;

        // Public State Properties
        public PlayerCombatState CurrentState { get; private set; } = PlayerCombatState.FreeMovement;
        public bool IsInvulnerable { get; private set; } = false;
        public int GunpowderCount { get; private set; } = 3;
        public int FlintlockAmmo { get; private set; } = 4;

        private CharacterController controller;
        private Coroutine activeActionRoutine;
        private bool attackBuffered = false;
        private bool dashBuffered = false;
        private float rotationVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (inputManager == null) inputManager = GetComponent<PlayerInputManager>();
            if (targetLock == null) targetLock = GetComponent<SoftTargetLock>();
            if (combatLunge == null) combatLunge = GetComponent<CombatLunge>();

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (inputManager == null) return;

            Vector3 inputDir = GetCameraRelativeDirection(inputManager.move);

            // 1. Process Dash Cancel Input
            if (inputManager.dash)
            {
                inputManager.dash = false; // Consume flag

                if (CurrentState != PlayerCombatState.ExecutionWindup && CurrentState != PlayerCombatState.Downed)
                {
                    if (CurrentState == PlayerCombatState.FreeMovement)
                    {
                        StartDash(inputDir);
                        return;
                    }
                    else if (IsAttackingState())
                    {
                        dashBuffered = true;
                    }
                }
            }

            // 2. FSM Execution
            switch (CurrentState)
            {
                case PlayerCombatState.FreeMovement:
                    HandleFreeMovement(inputDir);

                    if (inputManager.slash)
                    {
                        inputManager.slash = false;
                        StartAttack(1, inputDir);
                    }
                    else if (inputManager.powder)
                    {
                        inputManager.powder = false;
                        if (GunpowderCount > 0) StartPowderThrow(inputDir);
                    }
                    else if (inputManager.shoot)
                    {
                        inputManager.shoot = false;
                        TryExecutionOrQuickShot(inputDir);
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
                    break;
            }
        }

        #region Movement & Direction

        private void HandleFreeMovement(Vector3 moveDir)
        {
            if (moveDir.sqrMagnitude > 0.01f)
            {
                float targetYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);

                controller.Move(moveDir * (moveSpeed * Time.deltaTime));
            }

            OnSpeedUpdated?.Invoke(moveDir.magnitude * moveSpeed);
        }

        private Vector3 GetCameraRelativeDirection(Vector2 rawInput)
        {
            if (rawInput.sqrMagnitude < 0.01f) return Vector3.zero;

            Vector3 camFwd = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 camRight = cameraTransform != null ? cameraTransform.right : Vector3.right;
            camFwd.y = 0f;
            camRight.y = 0f;

            return (camFwd.normalized * rawInput.y + camRight.normalized * rawInput.x).normalized;
        }

        private bool IsAttackingState() =>
            CurrentState == PlayerCombatState.Attack1 ||
            CurrentState == PlayerCombatState.Attack2 ||
            CurrentState == PlayerCombatState.Attack3;

        #endregion

        #region Attack & Combo Pipeline

        private void StartAttack(int comboIndex, Vector3 inputDir)
        {
            if (activeActionRoutine != null) StopCoroutine(activeActionRoutine);
            activeActionRoutine = StartCoroutine(AttackRoutine(comboIndex, inputDir));
        }

        private IEnumerator AttackRoutine(int comboIndex, Vector3 inputDir)
        {
            attackBuffered = false;
            dashBuffered = false;

            // 1. Soft-Lock & Lunge
            Transform target = targetLock != null ? targetLock.GetTarget(inputDir, cameraTransform) : null;
            Vector3 fallback = inputDir.sqrMagnitude > 0.01f ? inputDir : transform.forward;
            if (combatLunge != null) combatLunge.ExecuteLunge(target, fallback);

            // 2. Fire Event
            OnAttackExecuted?.Invoke(comboIndex);

            // 3. State & Timings
            float duration = attack1Duration;
            float dmg = slashDamage;
            bool isFinisher = false;

            if (comboIndex == 1)
            {
                CurrentState = PlayerCombatState.Attack1;
                duration = attack1Duration;
            }
            else if (comboIndex == 2)
            {
                CurrentState = PlayerCombatState.Attack2;
                duration = attack2Duration;
            }
            else if (comboIndex == 3)
            {
                CurrentState = PlayerCombatState.Attack3;
                duration = attack3Duration;
                dmg = finisherDamage;
                isFinisher = true;
            }

            // Deal Damage midway through swing
            yield return new WaitForSeconds(duration * 0.4f);
            ExecuteMeleeHitbox(dmg, isFinisher);

            // Input Buffer Window
            float remainingTime = duration * 0.6f;
            float elapsed = 0f;

            while (elapsed < remainingTime)
            {
                elapsed += Time.deltaTime;

                if (dashBuffered)
                {
                    StartDash(inputDir);
                    yield break;
                }

                if (attackBuffered && (remainingTime - elapsed) <= comboBufferWindow)
                {
                    if (comboIndex < 3)
                    {
                        StartAttack(comboIndex + 1, inputDir);
                        yield break;
                    }
                }
                yield return null;
            }

            CurrentState = PlayerCombatState.FreeMovement;
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

        #region Black Powder & Execution

        private void StartPowderThrow(Vector3 inputDir)
        {
            if (activeActionRoutine != null) StopCoroutine(activeActionRoutine);
            activeActionRoutine = StartCoroutine(PowderThrowRoutine());
        }

        private IEnumerator PowderThrowRoutine()
        {
            CurrentState = PlayerCombatState.BlackPowderThrow;
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
            activeActionRoutine = null;
        }

        private void TryExecutionOrQuickShot(Vector3 inputDir)
        {
            if (FlintlockAmmo <= 0) return;

            Transform target = targetLock != null ? targetLock.GetTarget(inputDir, cameraTransform) : null;

            if (target != null && target.TryGetComponent(out IDamageable damageable) && damageable.IsStunned)
            {
                float dist = Vector3.Distance(transform.position, target.position);
                if (dist <= executionRange)
                {
                    if (activeActionRoutine != null) StopCoroutine(activeActionRoutine);
                    activeActionRoutine = StartCoroutine(ExecutionRoutine(target, damageable));
                    return;
                }
            }

            // Quick Shot Fallback
            FlintlockAmmo--;
            if (target != null && target.TryGetComponent(out IDamageable normalTarget))
            {
                normalTarget.TakeDamage(30f, target.position, transform.forward);
            }
        }

        private IEnumerator ExecutionRoutine(Transform target, IDamageable damageable)
        {
            CurrentState = PlayerCombatState.ExecutionWindup;
            FlintlockAmmo--;

            Vector3 lookDir = (target.position - transform.position);
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(lookDir.normalized);

            OnExecutionTriggered?.Invoke();

            yield return new WaitForSeconds(executionWindupDuration);

            if (damageable != null && damageable.IsAlive)
            {
                damageable.Execute(transform);
            }

            CurrentState = PlayerCombatState.FreeMovement;
            activeActionRoutine = null;
        }

        #endregion

        #region Dash

        private void StartDash(Vector3 inputDir)
        {
            if (activeActionRoutine != null) StopCoroutine(activeActionRoutine);
            if (combatLunge != null) combatLunge.CancelLunge();
            activeActionRoutine = StartCoroutine(DashRoutine(inputDir));
        }

        private IEnumerator DashRoutine(Vector3 inputDir)
        {
            CurrentState = PlayerCombatState.DashRoll;
            IsInvulnerable = true;

            Vector3 dashDir = inputDir.sqrMagnitude > 0.01f ? inputDir : transform.forward;
            transform.rotation = Quaternion.LookRotation(dashDir);

            OnDashExecuted?.Invoke();

            float elapsed = 0f;
            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= iFrameDuration) IsInvulnerable = false;

                controller.Move(dashDir * (dashSpeed * Time.deltaTime));
                yield return null;
            }

            IsInvulnerable = false;
            CurrentState = PlayerCombatState.FreeMovement;
            activeActionRoutine = null;
        }

        #endregion

        public void AddGunpowder(int amount) => GunpowderCount = Mathf.Clamp(GunpowderCount + amount, 0, maxGunpowderPouches);
        public void AddAmmo(int amount) => FlintlockAmmo = Mathf.Clamp(FlintlockAmmo + amount, 0, maxFlintlockAmmo);
    }
}