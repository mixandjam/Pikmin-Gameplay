using System.Collections;
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

    public PikminEvent OnStartFollow;
    public PikminEvent OnStartThrow;
    public PikminEvent OnEndThrow;
    public PikminEvent OnStartCarry;
    public PikminEvent OnEndCarry;

    private PikminVisualHandler visualHandler;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        visualHandler = GetComponent<PikminVisualHandler>();
    }
    public void SetTarget(Transform target, float updateTime = 1f)
    {
        if (state == State.Interact)
        {
            transform.parent = null;
            agent.enabled = true;
            objective.ReleasePikmin();
            objective = null;
        }

        state = State.Follow;
        agent.stoppingDistance = 1f;

        OnStartFollow.Invoke(0);

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
        OnStartThrow.Invoke(0);

        isFlying = true;
        state = State.Idle;

        if (updateTarget != null)
            StopCoroutine(updateTarget);

        agent.stoppingDistance = 0f;
        agent.enabled = false;

        transform.DOJump(target, 2, 1, time).SetDelay(delay).SetEase(Ease.Linear).OnComplete(() =>
        {
            agent.enabled = true;
            isFlying = false;
            CheckInteraction();

            OnEndThrow.Invoke(0);
        });

        transform.LookAt(new Vector3(target.x, transform.position.y, target.z));
        visualHandler.model.DOLocalRotate(new Vector3(360 * 3, 0, 0), time, RotateMode.LocalAxisAdd).SetDelay(delay);

    }

    public void SetCarrying(bool on)
    {
        if (on)
            OnStartCarry.Invoke(0);
        else
            OnEndCarry.Invoke(0);
    }

    public void SetIdle()
    {
        objective = null;
        agent.enabled = true;
        transform.parent = null;
        state = State.Idle;
        OnEndThrow.Invoke(0);
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
        }

        OnEndThrow.Invoke(0);

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
