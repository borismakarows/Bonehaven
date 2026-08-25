using UnityEngine;
using UnityEngine.AI;

namespace BoneHaven
{
    public class State
    {
        public enum STATE
        {
            IDLE, PATROL, PURSUE, ATTACK, UNBALANCED, STUNNED, HURT, DEAD
        }

        public enum EVENT
        {
            ENTER, UPDATE, EXIT
        }

        public STATE name;
        protected EVENT stage;
        protected GameObject npc;
        protected Animator anim;
        protected Transform player;
        protected State nextState;
        protected NavMeshAgent agent;
        protected EnemyConfigSO config;

        [Header("Animation Hashes")]
        protected readonly int isIdleHash = Animator.StringToHash("isIdle");
        protected readonly int isWalkingHash = Animator.StringToHash("isWalking");
        protected readonly int isRunningHash = Animator.StringToHash("isRunning");
        protected readonly int isAttackingHash = Animator.StringToHash("isAttacking");
        protected readonly int isUnbalancedHash = Animator.StringToHash("isUnbalanced");
        protected readonly int isStunnedHash = Animator.StringToHash("isStunned");
        protected readonly int isHurtHash = Animator.StringToHash("isHurt");
        protected readonly int isDeadHash = Animator.StringToHash("isDead");

        public State(GameObject _npc, NavMeshAgent _agent, EnemyConfigSO _config, Animator _anim, Transform _player)
        {
            npc = _npc;
            agent = _agent;
            anim = _anim;
            stage = EVENT.ENTER;
            player = _player;
            config = _config;
        }

        public virtual void Enter() { stage = EVENT.UPDATE; }
        public virtual void Update() { stage = EVENT.UPDATE; }
        public virtual void Exit() { stage = EVENT.EXIT; }

        #region AI Behaviours

        protected bool CanSeePlayer()
        {
            if (player == null || config == null || agent == null) return false;
            Vector3 direction = player.position - agent.transform.position;
            float angle = Vector3.Angle(direction, agent.transform.forward);

            return direction.magnitude <= config.visionDistance && angle <= config.visionAngle;
        }

        protected bool CanAttackPlayer()
        {
            if (player == null || config == null || agent == null) return false;
            float dist = Vector3.Distance(player.position, agent.transform.position);
            return dist <= config.attackRange;
        }

        protected void LookAtPlayer()
        {
            if (player == null || config == null || npc == null) return;
            Vector3 direction = player.position - npc.transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f)
            {
                npc.transform.rotation = Quaternion.Slerp(
                    npc.transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * config.rotationSpeed
                );
            }
        }

        #endregion

        #region Process

        public State Process()
        {
            if (stage == EVENT.ENTER) Enter();
            if (stage == EVENT.UPDATE) Update();
            if (stage == EVENT.EXIT)
            {
                Exit();
                return nextState;
            }
            return this;
        }

        #endregion
    }

    #region Basic States

    public class Idle : State
    {
        public Idle(GameObject _npc, NavMeshAgent _agent, EnemyConfigSO _config, Animator _anim, Transform _player)
            : base(_npc, _agent, _config, _anim, _player)
        {
            name = STATE.IDLE;
        }

        public override void Enter()
        {
            anim.SetTrigger(isIdleHash);
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            base.Enter();
        }

        public override void Update()
        {
            if (CanSeePlayer())
            {
                nextState = new Pursue(npc, agent, config, anim, player);
                stage = EVENT.EXIT;
            }
            else if (config != null && Random.Range(0, 100) <= config.patrolStartChanceRatio)
            {
                nextState = new Patrol(npc, agent, config, anim, player);
                stage = EVENT.EXIT;
            }
        }

        public override void Exit()
        {
            anim.ResetTrigger(isIdleHash);
            base.Exit();
        }
    }

    public class Patrol : State
    {
        private int currentIndex = -1;

        public Patrol(GameObject _npc, NavMeshAgent _agent, EnemyConfigSO _config, Animator _anim, Transform _player)
            : base(_npc, _agent, _config, _anim, _player)
        {
            name = STATE.PATROL;
        }

        public override void Enter()
        {
            currentIndex = 0;
            anim.SetTrigger(isWalkingHash);
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.speed = config != null ? config.moveSpeed * 0.6f : 2f;
            }
            base.Enter();
        }

        public override void Update()
        {
            if (CanSeePlayer())
            {
                nextState = new Pursue(npc, agent, config, anim, player);
                stage = EVENT.EXIT;
                return;
            }

            if (GameEnvironment.Singleton != null && GameEnvironment.Singleton.Checkpoints.Count > 0)
            {
                if (agent != null && agent.isOnNavMesh && agent.remainingDistance < 1f)
                {
                    currentIndex = (currentIndex + 1) % GameEnvironment.Singleton.Checkpoints.Count;
                    agent.SetDestination(GameEnvironment.Singleton.Checkpoints[currentIndex].position);
                }
            }
        }

        public override void Exit()
        {
            anim.ResetTrigger(isWalkingHash);
            base.Exit();
        }
    }

    public class Pursue : State
    {
        public Pursue(GameObject _npc, NavMeshAgent _agent, EnemyConfigSO _config, Animator _anim, Transform _player)
            : base(_npc, _agent, _config, _anim, _player)
        {
            name = STATE.PURSUE;
        }

        public override void Enter()
        {
            anim.ResetTrigger(isIdleHash);
            anim.ResetTrigger(isWalkingHash);
            anim.ResetTrigger(isUnbalancedHash);
            anim.ResetTrigger(isStunnedHash);
            anim.ResetTrigger(isHurtHash);

            anim.SetTrigger(isRunningHash);

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.speed = config != null ? config.moveSpeed : 4f;
            }
            base.Enter();
        }

        public override void Update()
        {
            if (player == null) return;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
            }

            if (CanAttackPlayer())
            {
                nextState = new Attack(npc, agent, config, anim, player);
                stage = EVENT.EXIT;
            }
            else if (!CanSeePlayer() && agent != null && agent.isOnNavMesh && agent.remainingDistance <= agent.stoppingDistance)
            {
                nextState = new Idle(npc, agent, config, anim, player);
                stage = EVENT.EXIT;
            }
        }

        public override void Exit()
        {
            anim.ResetTrigger(isRunningHash);
            base.Exit();
        }
    }

    public class Attack : State
    {
        private float attackTimer;

        public Attack(GameObject _npc, NavMeshAgent _agent, EnemyConfigSO _config, Animator _anim, Transform _player)
            : base(_npc, _agent, _config, _anim, _player)
        {
            name = STATE.ATTACK;
        }

        public override void Enter()
        {
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            attackTimer = 0f;

            anim.SetTrigger(isAttackingHash);
            base.Enter();
        }

        public override void Update()
        {
            LookAtPlayer();
            attackTimer += Time.deltaTime;

            float cooldown = config != null ? config.attackCooldown : 2f;
            if (attackTimer >= cooldown)
            {
                if (!CanAttackPlayer())
                {
                    nextState = new Pursue(npc, agent, config, anim, player);
                    stage = EVENT.EXIT;
                }
                else
                {
                    attackTimer = 0f;
                    anim.SetTrigger(isAttackingHash);
                }
            }
        }

        public override void Exit()
        {
            anim.ResetTrigger(isAttackingHash);
            if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
            base.Exit();
        }
    }

    #endregion

    #region Combat Reaction States

    public class Hurt : State
    {
        private float elapsed;
        private const float hurtDuration = 0.35f;
        private readonly Vector3 knockbackDir;

        public Hurt(GameObject _npc, NavMeshAgent _agent, EnemyConfigSO _config, Animator _anim, Transform _player, Vector3 _hitDir)
            : base(_npc, _agent, _config, _anim, _player)
        {
            name = STATE.HURT;
            knockbackDir = _hitDir;
        }

        public override void Enter()
        {
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            anim.SetTrigger(isHurtHash);
            elapsed = 0f;
            base.Enter();
        }

        public override void Update()
        {
            elapsed += Time.deltaTime;
            npc.transform.position += knockbackDir * (2f * Time.deltaTime);

            if (elapsed >= hurtDuration)
            {
                nextState = new Pursue(npc, agent, config, anim, player);
                stage = EVENT.EXIT;
            }
        }

        public override void Exit()
        {
            anim.ResetTrigger(isHurtHash);
            if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
            base.Exit();
        }
    }

    public class Unbalanced : State
    {
        public Unbalanced(GameObject _npc, NavMeshAgent _agent, EnemyConfigSO _config, Animator _anim, Transform _player)
            : base(_npc, _agent, _config, _anim, _player)
        {
            name = STATE.UNBALANCED;
        }

        public override void Enter()
        {
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            anim.SetTrigger(isUnbalancedHash);
            base.Enter();
        }

        public override void Exit()
        {
            anim.ResetTrigger(isUnbalancedHash);
            if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
            base.Exit();
        }
    }

    public class Stunned : State
    {
        public Stunned(GameObject _npc, NavMeshAgent _agent, EnemyConfigSO _config, Animator _anim, Transform _player)
            : base(_npc, _agent, _config, _anim, _player)
        {
            name = STATE.STUNNED;
        }

        public override void Enter()
        {
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            anim.SetTrigger(isStunnedHash);
            base.Enter();
        }

        public override void Exit()
        {
            anim.ResetTrigger(isStunnedHash);
            if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
            base.Exit();
        }
    }

    public class Dead : State
    {
        public Dead(GameObject _npc, NavMeshAgent _agent, EnemyConfigSO _config, Animator _anim, Transform _player)
            : base(_npc, _agent, _config, _anim, _player)
        {
            name = STATE.DEAD;
        }

        public override void Enter()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            anim.ResetTrigger(isAttackingHash);
            anim.ResetTrigger(isRunningHash);
            anim.ResetTrigger(isWalkingHash);
            anim.ResetTrigger(isIdleHash);
            anim.ResetTrigger(isHurtHash);
            anim.ResetTrigger(isUnbalancedHash);
            anim.ResetTrigger(isStunnedHash);

            anim.SetTrigger(isDeadHash);
            base.Enter();
        }

        public override void Update()
        {
            // Ölüm durumunda döngü işletilmez
        }
    }

    #endregion
}