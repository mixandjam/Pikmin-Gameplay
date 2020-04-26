using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CarryObject : InteractiveObject
{
    [SerializeField] private Transform destination = default;
    private NavMeshAgent agent = default;
    private Coroutine destinationRoutine = default;

    public override void Initialize()
    {
        base.Initialize();
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
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
    public override void StopInteract()
    {
        agent.enabled = false;
        StopCoroutine(destinationRoutine);
    }
}
