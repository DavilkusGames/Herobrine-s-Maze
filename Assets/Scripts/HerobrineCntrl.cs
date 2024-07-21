using Plugins.Audio.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HerobrineCntrl : MonoBehaviour
{
    public enum AIState { Disabled, Patrolling, Alerted, Chase, DisablingLever, FleeAway };

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

    public float chaseTime = 15f;
    public LayerMask defaultLayerMask;
    public LayerMask withoutPlayerLayerMask;

    private NavMeshAgent agent;
    private SourceAudio sfx;
    private Transform player;
    private Transform trans;
    private AIState state = AIState.Disabled;
    private int pathPointId = -1;
    private LeverCntrl leverToDisable = null;
    private bool stateLocked = false;
    private bool chaseCoroutineRunning = false;

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
                if (HaveReachedDestination()) MoveToRandomPoint();
                if (CheckVisibility(player, true, defaultLayerMask)) ChangeState(AIState.Chase);
                break;
            case AIState.Alerted:
                if (CheckVisibility(player, true, defaultLayerMask)) ChangeState(AIState.Chase);
                else if (HaveReachedDestination()) ChangeState(AIState.Patrolling);
                break;
            case AIState.Chase:
                agent.SetDestination(player.position);
                if (!CheckVisibility(player, true, defaultLayerMask)) ChangeState(AIState.Alerted, true);
                break;
            case AIState.DisablingLever:
                if (HaveReachedDestination())
                {
                    leverToDisable.Cancel();
                    leverToDisable = null;
                    agent.SetDestination(player.position);
                    ChangeState(AIState.Alerted);
                }
                break;
            case AIState.FleeAway:
                if (HaveReachedDestination())
                {
                    ChangeState(AIState.Patrolling);
                }
                break;
        }
    }

    private void ChangeState(AIState newState, bool ignoreSettings=false)
    {
        if (stateLocked && newState != AIState.Disabled) return;
        state = newState;

        if (ignoreSettings) return;
        switch (newState)
        {
            case AIState.Disabled:
                if (chaseCoroutineRunning)
                {
                    StopCoroutine(nameof(ChaseTimer));
                    chaseCoroutineRunning = false;
                }

                runTxt.SetActive(false);
                pathPointId = -1;
                agent.isStopped = true;
                agent.enabled = false;
                break;
            case AIState.Patrolling:
                StopCoroutine(nameof(ChaseTimer));
                chaseCoroutineRunning = false;

                runTxt.SetActive(false);
                agent.speed = normalSpeed;
                anim.speed = normalAnimSpeed;
                break;
            case AIState.Alerted:
                agent.speed = alertedSpeed;
                anim.speed = alertedAnimSpeed;
                pathPointId = -1;
                break;
            case AIState.Chase:
                if (!chaseCoroutineRunning)
                {
                    StartCoroutine(nameof(ChaseTimer));
                    chaseCoroutineRunning = true;
                    sfx.PlayOneShot("startChase");
                }

                runTxt.SetActive(true);
                agent.speed = chaseSpeed;
                anim.speed = chaseAnimSpeed;
                pathPointId = -1;
                break;
            case AIState.DisablingLever:
                runTxt.SetActive(true);
                agent.speed = alertedSpeed;
                anim.speed = alertedAnimSpeed;
                pathPointId = -1;
                break;
            case AIState.FleeAway:
                runTxt.SetActive(false);
                agent.speed = alertedSpeed;
                anim.speed = alertedAnimSpeed;
                MoveToFurthestPoint();
                break;
        }
    }

    private IEnumerator ChaseTimer()
    {
        yield return new WaitForSeconds(chaseTime);
        ChangeState(AIState.FleeAway);
    }

    private void MoveToRandomPoint()
    {
        int newPathPointId = Random.Range(0, pathPoints.Length);
        if (newPathPointId == pathPointId) pathPointId++;
        if (pathPointId >= pathPoints.Length) newPathPointId = 0;
        agent.SetDestination(pathPoints[newPathPointId].position);
        pathPointId = newPathPointId;
    }

    private void MoveToFurthestPoint()
    {
        int newPathPointId = 0;
        float maxDist = 0;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            float dist = GameManager.FastDistance(trans.position, pathPoints[i].position);
            if (dist > maxDist)
            {
                newPathPointId = i;
                maxDist = dist;
            }
        }
        agent.SetDestination(pathPoints[newPathPointId].position);
        pathPointId = newPathPointId;
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
        if (state != AIState.Disabled && CheckVisibility(lever.transform, true, withoutPlayerLayerMask))
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

    private bool CheckVisibility(Transform targetTransform, bool checkRaycast, LayerMask mask)
    {
        if (!checkRaycast)
        {
            if (Vector3.Distance(trans.position, targetTransform.position) < visionDistance) return true;
            else return false;
        }
        else
        {
            RaycastHit hit;

            if (Physics.Raycast(trans.position, targetTransform.position - trans.position, out hit, visionDistance, mask))
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

    public void ForceFlee()
    {
        if (chaseCoroutineRunning)
        {
            StopCoroutine(nameof(ChaseTimer));
            chaseCoroutineRunning = false;
        }
        ChangeState(AIState.FleeAway);
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
