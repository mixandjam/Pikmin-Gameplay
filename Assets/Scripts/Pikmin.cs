using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
public class Pikmin : MonoBehaviour
{
    public enum State { Idle, Follow, Interact }
    private NavMeshAgent agent = default;
    private Coroutine updateTarget = default;
    public State state = default;
    InteractiveObject objective;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    public void SetTarget(Transform target, float updateTime = 1f)
    {
        if (state == State.Interact)
            objective.ReleasePikmin();

        state = State.Follow;
        agent.stoppingDistance = 0.5f;

        if (updateTarget != null)
            StopCoroutine(updateTarget);

        WaitForSeconds wait = new WaitForSeconds(updateTime);
        updateTarget = StartCoroutine(UpdateTarget());

        IEnumerator UpdateTarget()
        {
            while (true)
            {
                agent.SetDestination(target.position);
                yield return wait;
            }
        }
    }
    public void Throw(Vector3 target, float time)
    {
        state = State.Idle;
        if (updateTarget != null)
            StopCoroutine(updateTarget);

        agent.stoppingDistance = 0f;
        agent.enabled = false;
        transform.DOJump(target, 2, 1, time).SetEase(Ease.Linear).OnComplete(() =>
        {
            agent.enabled = true;
            CheckInteraction();
        });
    }

    void CheckInteraction()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);

        if (colliders.Length == 0)
            return;

        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<InteractiveObject>())
            {
                objective = collider.GetComponent<InteractiveObject>();
                if (objective.AssignPikmin())
                {
                    state = State.Interact;
                    agent.SetDestination(objective.GetPositon());
                    break;
                }
            }
        }
    }
}
