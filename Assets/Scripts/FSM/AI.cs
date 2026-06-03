using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    [Header("Agent Settings")]
    [SerializeField] AgentSettings agentSettings;

    [Header("Components")]
    NavMeshAgent agent;
    [SerializeField] Transform player;
    Animator anim;
    State currentState;
    

#region Unity Functions
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        currentState = new Idle(gameObject,agent,agentSettings,anim,player);
    }

    void Update()
    {
        currentState = currentState.Process();
    }
#endregion

    
   


}
