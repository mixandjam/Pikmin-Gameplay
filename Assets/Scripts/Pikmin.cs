using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
public class Pikmin : MonoBehaviour
{
    public enum State { Idle, Follow, Attack }
    private NavMeshAgent agent = default;
    private Coroutine updateTarget = default;
    public State state = default;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    public void SetTarget(Transform target, float updateTime = 1f)
    {
        state = State.Follow;
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

        agent.enabled = false;
        transform.DOJump(target, 2, 1, time).OnComplete(() => agent.enabled = true);
    }
}
