using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CarryObject : InteractiveObject
{
    [SerializeField] private Transform destination = default;
    private NavMeshAgent agent = default;
    private Coroutine destinationRoutine = default;
    private float originalAgentSpeed;
    [SerializeField] private Vector3 destinationOffset;

    public override void Initialize()
    {
        base.Initialize();
        agent = GetComponent<NavMeshAgent>();
        //agent.enabled = false;
        originalAgentSpeed = agent.speed;
    }
    public override void Interact()
    {
        if (destinationRoutine != null)
            StopCoroutine(destinationRoutine);

        agent.enabled = true;
        destinationRoutine = StartCoroutine(GetInPosition());

        IEnumerator GetInPosition()
        {
            agent.SetDestination(destination.position);
            yield return new WaitUntil(() => agent.IsDone());
            agent.enabled = false;
            (FindObjectOfType(typeof(PikminManager)) as PikminManager).FinishInteraction(this);
            Destroy(this.gameObject);
        }

    }

    public override void UpdateSpeed(int extra)
    {
        agent.speed = (extra > 0) ? originalAgentSpeed + (extra * .2f) : originalAgentSpeed;
    }

    public override void StopInteract()
    {
        //agent.enabled = false;
        if(destinationRoutine != null)
            StopCoroutine(destinationRoutine);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination.position + destinationOffset, .2f);
    }
}
