using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private PlayerLocomotion locomotion;
        private PlayerCombatFSM combatFSM;
        private CombatLunge combatLunge;
        private Animator animator;

        private static readonly int HashSpeed = Animator.StringToHash("Speed");
        private static readonly int HashMotionSpeed = Animator.StringToHash("MotionSpeed");
        private static readonly int HashAttack1 = Animator.StringToHash("Attack1");
        private static readonly int HashAttack2 = Animator.StringToHash("Attack2");
        private static readonly int HashAttack3 = Animator.StringToHash("Attack3");
        private static readonly int HashEvade = Animator.StringToHash("Evade");
        private static readonly int HashPowder = Animator.StringToHash("ThrowPowder");
        private static readonly int HashExecute = Animator.StringToHash("Execute");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (locomotion == null) locomotion = GetComponentInParent<PlayerLocomotion>();
            if (combatFSM == null) combatFSM = GetComponentInParent<PlayerCombatFSM>();
            if (combatLunge == null) combatLunge = GetComponent<CombatLunge>();
        }

        private void OnEnable()
        {
            if (locomotion != null) locomotion.OnLocomotionUpdated += UpdateLocomotionAnimation;
            if (combatFSM != null)
            {
                combatFSM.OnAttackExecuted += PlayAttackAnimation;
                combatFSM.OnEvadeExecuted += PlayEvadeAnimation;
                combatFSM.OnPowderExecuted += PlayPowderAnimation;
                combatFSM.OnExecutionTriggered += PlayExecutionAnimation;
            }
        }

        private void OnDisable()
        {
            if (locomotion != null) locomotion.OnLocomotionUpdated -= UpdateLocomotionAnimation;
            if (combatFSM != null)
            {
                combatFSM.OnAttackExecuted -= PlayAttackAnimation;
                combatFSM.OnEvadeExecuted -= PlayEvadeAnimation;
                combatFSM.OnPowderExecuted -= PlayPowderAnimation;
                combatFSM.OnExecutionTriggered -= PlayExecutionAnimation;
            }
        }

        private void UpdateLocomotionAnimation(float speed, float inputMagnitude)
        {
            animator.SetFloat(HashSpeed, speed);
            animator.SetFloat(HashMotionSpeed, speed > 0.1f ? 1.0f : 0.0f);
        }

        private void PlayAttackAnimation(int comboIndex)
        {
            switch (comboIndex)
            {
                case 1: animator.SetTrigger(HashAttack1); break;
                case 2: animator.SetTrigger(HashAttack2); break;
                case 3: animator.SetTrigger(HashAttack3); break;
            }
        }

        private void PlayEvadeAnimation() => animator.SetTrigger(HashEvade);
        private void PlayPowderAnimation() => animator.SetTrigger(HashPowder);
        private void PlayExecutionAnimation() => animator.SetTrigger(HashExecute);
    }
}