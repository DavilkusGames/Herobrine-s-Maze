using UnityEngine;
using UnityEngine.AI;

public class MenuHerobrineCntrl : MonoBehaviour
{
    public Animator anim;
    public Transform[] menuPathPoints;

    private NavMeshAgent agent;
    private int pathPointId = 0;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        pathPointId = 1;
        agent.SetDestination(menuPathPoints[pathPointId].position);
        anim.speed = 2f;
    }

    private void Update()
    {
        if (agent.remainingDistance > 0 && agent.remainingDistance < 0.5f)
        {
            pathPointId++;
            if (pathPointId >= menuPathPoints.Length) pathPointId = 0;
            agent.SetDestination(menuPathPoints[pathPointId].position);
        }
    }
}
