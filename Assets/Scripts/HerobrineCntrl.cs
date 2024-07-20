using Plugins.Audio.Core;
using UnityEngine;
using UnityEngine.AI;

public class HerobrineCntrl : MonoBehaviour
{
    public enum AIState { Disabled, Patrolling, Alerted, Chase, DisablingLever };

    public Animator anim;
    public Transform[] pathPoints;
    public GameObject runTxt;

    public float visionDistance = 15f;
    public float hearingDistance = 30f;

    public float normalSpeed = 3f;
    public float alertedSpeed = 3.5f;
    public float chaseSpeed = 4.5f;

    public float normalAnimSpeed = 1.5f;
    public float alertedAnimSpeed = 2f;
    public float chaseAnimSpeed = 2.5f;

    public float chaseStartSoundCD = 20f;

    private NavMeshAgent agent;
    private SourceAudio sfx;
    private Transform player;
    private Transform trans;
    private AIState state = AIState.Disabled;
    private int pathPointId = -1;
    private LeverCntrl leverToDisable = null;
    private bool stateLocked = false;
    private float prevChaseSoundTime = -100f;

    private void Start()
    {
        trans = transform;
        agent = GetComponent<NavMeshAgent>();
        sfx = GetComponent<SourceAudio>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        state = AIState.Patrolling;
        agent.speed = normalSpeed;
        anim.speed = normalAnimSpeed;
        anim.speed = 1.5f;
    }

    private void FixedUpdate()
    {
        switch (state) {
            case AIState.Disabled:
                break;
            case AIState.Patrolling:
                if (HaveReachedDestination())
                {
                    int newPathPointId = Random.Range(0, pathPoints.Length);
                    if (newPathPointId == pathPointId) pathPointId++;
                    if (pathPointId >= pathPoints.Length) newPathPointId = 0;
                    agent.SetDestination(pathPoints[newPathPointId].position);
                    pathPointId = newPathPointId;
                }
                if (CheckVisibility(player)) ChangeState(AIState.Chase);
                break;
            case AIState.Alerted:
                if (CheckVisibility(player)) ChangeState(AIState.Chase);
                else if (HaveReachedDestination()) ChangeState(AIState.Patrolling);
                break;
            case AIState.Chase:
                agent.SetDestination(player.position);
                if (!CheckVisibility(player)) ChangeState(AIState.Alerted, true);
                break;
            case AIState.DisablingLever:
                if (HaveReachedDestination())
                {
                    leverToDisable.Cancel();
                    leverToDisable = null;
                    ChangeState(AIState.Patrolling);
                }
                break;
        }
    }

    private void ChangeState(AIState newState, bool ignoreSettings=false)
    {
        if (stateLocked) return;
        state = newState;

        if (ignoreSettings) return;
        switch (newState)
        {
            case AIState.Disabled:
                pathPointId = -1;
                agent.isStopped = true;
                agent.enabled = false;
                break;
            case AIState.Patrolling:
                agent.speed = normalSpeed;
                anim.speed = normalAnimSpeed;
                break;
            case AIState.Alerted:
                agent.speed = alertedSpeed;
                anim.speed = alertedAnimSpeed;
                pathPointId = -1;
                break;
            case AIState.Chase:
                agent.speed = chaseSpeed;
                anim.speed = chaseAnimSpeed;
                pathPointId = -1;
                if (prevChaseSoundTime < Time.time + chaseStartSoundCD)
                    sfx.PlayOneShot("startChase");
                break;
            case AIState.DisablingLever:
                agent.speed = alertedSpeed;
                anim.speed = alertedAnimSpeed;
                pathPointId = -1;
                break;
        }
        runTxt.SetActive(state == AIState.Chase || state == AIState.DisablingLever);
    }

    private bool HaveReachedDestination()
    {
        return (agent.remainingDistance < 0.5f && !agent.pathPending);
    }

    public void SoundReaction(Transform soundTransform)
    {
        if (state == AIState.Patrolling && CheckHearing(soundTransform))
        {
            ChangeState(AIState.Alerted);
            agent.SetDestination(soundTransform.position);
        }
    }

    public void CheckLeverVisibility(LeverCntrl lever)
    {
        if (state != AIState.Disabled && CheckVisibility(lever.transform, false))
        {
            ChangeState(AIState.DisablingLever);
            agent.SetDestination(lever.transform.position);
            leverToDisable = lever;
        }
    }

    private bool CheckHearing(Transform targetTransform)
    {
        if (Vector3.Distance(trans.position, targetTransform.position) > hearingDistance) return false;
        return true;
    }

    private bool CheckVisibility(Transform targetTransform, bool checkRaycast=true)
    {
        if (!checkRaycast)
        {
            if (Vector3.Distance(trans.position, targetTransform.position) < visionDistance) return true;
            else return false;
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(trans.position, targetTransform.position - trans.position, out hit, visionDistance))
            {
                if (hit.collider.transform != targetTransform) return false;
                return true;
            }
            else return false;
        }
    }

    public void ForceChase()
    {
        ChangeState(AIState.Chase);
        stateLocked = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            trans.LookAt(player);
            player.GetComponent<PlayerCntrl>().GameOver(trans);
            anim.Play("killAnim");
            sfx.PlayOneShot("jumpscare");
            ChangeState(AIState.Disabled);
        }
    }

    public void KillAnimEnd()
    {
        GameManager.Instance.GameOver();
    }
}
