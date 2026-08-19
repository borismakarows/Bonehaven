using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private PlayerCombatFSM combatFSM;
        private Animator animator;

        // Animator Parameters
        private static readonly int HashSpeed = Animator.StringToHash("Speed");
        private static readonly int HashAttack1 = Animator.StringToHash("Attack1");
        private static readonly int HashAttack2 = Animator.StringToHash("Attack2");
        private static readonly int HashAttack3 = Animator.StringToHash("Attack3");
        private static readonly int HashDash = Animator.StringToHash("Dash");
        private static readonly int HashPowder = Animator.StringToHash("ThrowPowder");
        private static readonly int HashExecute = Animator.StringToHash("Execute");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (combatFSM == null) combatFSM = GetComponent<PlayerCombatFSM>();
        }

        private void OnEnable()
        {
            if (combatFSM == null) return;
            combatFSM.OnAttackExecuted += PlayAttackAnimation;
            combatFSM.OnDashExecuted += PlayDashAnimation;
            combatFSM.OnPowderExecuted += PlayPowderAnimation;
            combatFSM.OnExecutionTriggered += PlayExecutionAnimation;
            combatFSM.OnSpeedUpdated += UpdateSpeed;
        }

        private void OnDisable()
        {
            if (combatFSM == null) return;
            combatFSM.OnAttackExecuted -= PlayAttackAnimation;
            combatFSM.OnDashExecuted -= PlayDashAnimation;
            combatFSM.OnPowderExecuted -= PlayPowderAnimation;
            combatFSM.OnExecutionTriggered -= PlayExecutionAnimation;
            combatFSM.OnSpeedUpdated -= UpdateSpeed;
        }

        private void UpdateSpeed(float speed)
        {
            animator.SetFloat(HashSpeed, speed);
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

        private void PlayDashAnimation() => animator.SetTrigger(HashDash);
        private void PlayPowderAnimation() => animator.SetTrigger(HashPowder);
        private void PlayExecutionAnimation() => animator.SetTrigger(HashExecute);
    }
}