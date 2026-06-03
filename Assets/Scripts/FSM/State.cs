using Unity.Mathematics;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public class State
{
    public enum STATE
    {
        IDLE, PATROL, PURSUE, CALLFORHELP, ATTACK, DEAD
    };

    public enum EVENT
    {
        ENTER, UPDATE, EXIT
    };

    public STATE name;
    protected EVENT stage;
    protected GameObject npc;
    protected Animator anim;
    protected Transform player;
    protected State nextState;
    protected NavMeshAgent agent;
    protected AgentSettings agentSettings;

    [Header("Animation")]
    protected readonly int isIdleHash = Animator.StringToHash("isIdle");
    protected readonly int isWalkingHash = Animator.StringToHash("isWalking");
    protected readonly int isRunning = Animator.StringToHash("isRunning");
    protected readonly int isShootingHash = Animator.StringToHash("isShooting");
    protected readonly int isSlashingHash = Animator.StringToHash("isSlashing");


    //Constructor
    public State(GameObject _npc, NavMeshAgent _agent,AgentSettings _agentSettings, Animator _anim, Transform _player)
    {
        npc = _npc;
        agent = _agent;
        anim = _anim;
        stage = EVENT.ENTER;
        player = _player;
        agentSettings = _agentSettings;
    }

    public virtual void Enter() { stage = EVENT.UPDATE;}
    public virtual void Update() { stage = EVENT.UPDATE; }
    public virtual void Exit() { stage = EVENT.EXIT; }

    #region AI Behaviours
    protected bool CanSeePlayer()
    {
        Vector3 direction = player.position - agent.transform.position;
        float angle = Vector3.Angle(direction, agent.transform.forward);
        float visDist = (agentSettings.enemyType == EnemyType.Melee) ? agentSettings.meleeVisDist : agentSettings.rangeVisDist;
        if (direction.magnitude < visDist && angle <= agentSettings.visAng) return true;
        else return false;
    }

    protected bool CanAttackPlayer()
    {
        float attackDistance = (agentSettings.enemyType == EnemyType.Melee) ? agentSettings.swordDist : agentSettings.shootDist;
        Vector3 direction = player.position - agent.transform.position;

        if (direction.magnitude <= attackDistance) return true;
        else return false;
    }

    protected void AttackToPlayer()
    {
        Vector3 direction = player.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation,
                                              Quaternion.LookRotation(direction),
                                              Time.deltaTime * agentSettings.rotationSpeed);
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
} 
#endregion

#region Idle
public class Idle : State
{
    public Idle(GameObject _npc, NavMeshAgent _agent, AgentSettings _agentSettings, Animator _anim, Transform _player)
            : base(_npc, _agent,_agentSettings , _anim, _player)
    {
        name = STATE.IDLE;
    }

    public override void Enter()
    {
        anim.SetTrigger(isIdleHash);
        agent.isStopped = false;
        base.Enter();
    }

    public override void Update()
    {
        if (CanSeePlayer())
        {
            nextState = new Pursue(npc, agent, agentSettings, anim, player);
            stage = EVENT.EXIT;
        }
        else if (UnityEngine.Random.Range(0,100)<= agentSettings.patrolStartRatio)
        {
            nextState = new Patrol(npc, agent, agentSettings, anim, player);
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger(isIdleHash);
        base.Exit();
    }
}
#endregion

#region Patrol
public class Patrol : State
{
    int currentIndex = -1;
    public Patrol(GameObject _npc, NavMeshAgent _agent, AgentSettings _agentSettings, Animator _anim, Transform _player)
            : base(_npc, _agent,_agentSettings, _anim, _player)
    {
        name = STATE.PATROL;
        agent.speed = 2;
        agent.isStopped = false;
    }
    
    public override void Enter()
    {
        currentIndex = 0;
        anim.SetTrigger(isWalkingHash);
        agent.isStopped = false;
        base.Enter();
    }

    public override void Update()
    {
        if (CanSeePlayer())
        { 
            nextState = new Pursue(npc,agent,agentSettings,anim,player);
            stage = EVENT.EXIT;
        }
        else if (agent.remainingDistance < 1)
        {
            if(currentIndex >= GameEnvironment.Singleton.Checkpoints.Count-1)
            {
                currentIndex = 0;
            }
            else 
                currentIndex++;
            agent.SetDestination(GameEnvironment.Singleton.Checkpoints[currentIndex].position);
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger(isWalkingHash);
        base.Exit();
    }


}
#endregion

#region Pursue
public class Pursue : State
{
    public Pursue(GameObject _npc, NavMeshAgent _agent, AgentSettings _agentSettings, Animator _anim, Transform _player)
            : base(_npc, _agent,_agentSettings, _anim, _player)
    {
        name = STATE.PURSUE;
        agent.speed = 5;
        agent.isStopped = false;
    }

    public override void Enter()
    {
        anim.SetTrigger(isRunning);
        base.Enter();
    }

    public override void Update()
    {
        agent.SetDestination(player.position);
        if (agent.hasPath)
        {
            if (CanAttackPlayer())
            {
                nextState = new Attack(npc, agent, agentSettings, anim,player);
                stage = EVENT.EXIT;
            }
            else if (!CanSeePlayer())
            {
                nextState = new Patrol(npc, agent, agentSettings, anim,player);
                stage = EVENT.EXIT;
            }
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger(isRunning);
        base.Exit();
    }
}
#endregion

#region Attack
public class Attack : State
{   
    AudioSource attackSFX;
    public Attack(GameObject _npc, NavMeshAgent _agent, AgentSettings _agentSettings, Animator _anim, Transform _player)
            : base(_npc, _agent,_agentSettings, _anim, _player)
    {
        name = STATE.ATTACK;
        agent.speed = 5;
        agent.isStopped = false;
        attackSFX = agent.GetComponent<AudioSource>();
    }
    
    
    public override void Enter()
    {
        
        agent.isStopped = true;
        attackSFX.Play();
        if (agentSettings.enemyType == EnemyType.Melee)
        {
            anim.SetTrigger(isSlashingHash);
        }
        else
        {
            anim.SetTrigger(isShootingHash);
        }   
        base.Enter();
    }

    public override void Update()
    {

        if (!CanAttackPlayer())
        {
            nextState = new Idle(npc,agent,agentSettings,anim,player);
            stage = EVENT.EXIT;
            return;
        }
        AttackToPlayer();
    }

    public override void Exit()
    {
        if (agentSettings.enemyType == EnemyType.Melee)
        {
            anim.ResetTrigger(isSlashingHash);
        }
        else
        {
            anim.ResetTrigger(isShootingHash);
        }   
        anim.SetTrigger(isIdleHash);
        agent.isStopped = false;
        attackSFX.Stop();
        base.Exit();
    }
}
#endregion


