using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

[SelectionBase]
public class Pikmin : MonoBehaviour
{
    public enum State { Idle, Follow, Interact }
    [HideInInspector]
    public NavMeshAgent agent = default;
    private Coroutine updateTarget = default;
    public State state = default;
    public InteractiveObject objective;
    public bool isFlying;
    public bool isGettingIntoPosition;

    private PikminVisualHandler visualHandler;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        visualHandler = GetComponent<PikminVisualHandler>();
    }
    public void SetTarget(Transform target, float updateTime = 1f)
    {
        agent.acceleration = 8;

        if (state == State.Interact)
        {
            transform.parent = null;
            agent.enabled = true;
            objective.ReleasePikmin();
            objective = null;
        }

        state = State.Follow;
        agent.stoppingDistance = 1f;

        visualHandler.activationParticle.Play();
        visualHandler.leafParticle.Clear();
        visualHandler.leafParticle.Stop();

        if (updateTarget != null)
            StopCoroutine(updateTarget);

        WaitForSeconds wait = new WaitForSeconds(updateTime);
        updateTarget = StartCoroutine(UpdateTarget());

        IEnumerator UpdateTarget()
        {
            while (true)
            {
                if(agent.enabled)
                    agent.SetDestination(target.position);
                yield return wait;
            }
        }
    }
    public void Throw(Vector3 target, float time, float delay)
    {
        isFlying = true;

        agent.angularSpeed = 0;

        state = State.Idle;
        if (updateTarget != null)
            StopCoroutine(updateTarget);

        agent.stoppingDistance = 0f;
        agent.enabled = false;
        SetPikminTrail(true);

        //Vector3 finalTarget = (Vector3.Distance(transform.position, target) > 3) ? (transform.position

        transform.DOJump(target, 2, 1, time).SetDelay(delay).SetEase(Ease.Linear).OnComplete(() =>
        {
            agent.angularSpeed = 2000;
            agent.acceleration = 25;
            agent.enabled = true;
            isFlying = false;
            CheckInteraction();

            SetPikminTrail(false);
        });

        transform.LookAt(new Vector3(target.x, transform.position.y, target.z));
        transform.GetChild(0).DORotate(new Vector3(360 * 3, 0, 0), time, RotateMode.LocalAxisAdd).SetDelay(delay);
    }

    public void SetPikminTrail(bool on)
    {
        visualHandler.trail.emitting = on;
        if (on)
        {
            visualHandler.trail.Clear();
            visualHandler.particleTrail.Play();
        }
        else
        {
            visualHandler.particleTrail.Stop();
        }
    }

    internal void Reaction()
    {
        transform.DOJump(transform.position, .4f, 1, .3f);
        transform.DOPunchScale(-Vector3.up/2, .3f, 10, 1).SetDelay(Random.Range(0,.1f));
    }

    public void SetIdle()
    {
        agent.enabled = true;
        transform.parent = null;
        state = State.Idle;
        visualHandler.leafParticle.Play();
    }

    void CheckInteraction()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);

        if (colliders.Length == 0)
            return;

        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<InteractiveObject>() && collider.GetComponent<NavMeshAgent>().enabled)
            {
                objective = collider.GetComponent<InteractiveObject>();
                objective.AssignPikmin();
                StartCoroutine(GetInPosition());

                break;
            }
            else
            {
                visualHandler.leafParticle.Play();
            }
        }

        IEnumerator GetInPosition()
        {
            isGettingIntoPosition = true;

            agent.SetDestination(objective.GetPositon());
            yield return new WaitUntil(() => agent.IsDone());
            agent.enabled = false;
            state = State.Interact;

            if (objective != null)
            {
                transform.parent = objective.transform;
                transform.DOLookAt(new Vector3(objective.transform.position.x, transform.position.y, objective.transform.position.z), .2f);
            }

            isGettingIntoPosition = false;
        }
    }
}
