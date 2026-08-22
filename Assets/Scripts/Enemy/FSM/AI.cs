using UnityEngine;
using UnityEngine.AI;

namespace BoneHaven
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public class AI : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private EnemyConfigSO enemyConfig;

        [Header("Targeting")]
        [SerializeField] private Transform player;

        private NavMeshAgent agent;
        private Animator anim;
        private State currentState;

        public EnemyConfigSO Config => enemyConfig;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();

            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.transform;
            }

            if (enemyConfig != null)
            {
                agent.speed = enemyConfig.moveSpeed;
            }

            currentState = new Idle(gameObject, agent, enemyConfig, anim, player);
        }

        private void Update()
        {
            if (currentState != null)
            {
                currentState = currentState.Process();
            }
        }

        #region State Transition Triggers

        public void TriggerHurt(Vector3 hitDirection)
        {
            currentState = new Hurt(gameObject, agent, enemyConfig, anim, player, hitDirection);
        }

        public void TriggerUnbalanced()
        {
            currentState = new Unbalanced(gameObject, agent, enemyConfig, anim, player);
        }

        public void TriggerStun()
        {
            currentState = new Stunned(gameObject, agent, enemyConfig, anim, player);
        }

        public void RecoverFromStun()
        {
            currentState = new Pursue(gameObject, agent, enemyConfig, anim, player);
        }

        public void TriggerDeath()
        {
            currentState = new Dead(gameObject, agent, enemyConfig, anim, player);
        }

        #endregion
    }
}