using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

[SelectionBase]
public class CarryObject : InteractiveObject
{
    [SerializeField] private DestinationScript destination;
    private NavMeshAgent agent = default;
    private Coroutine destinationRoutine = default;
    private float originalAgentSpeed;
    private Renderer objectRenderer;
    private Collider collider;
    [SerializeField] private Vector3 destinationOffset;
    [SerializeField] [ColorUsage(false,true)] private Color captureColor;

    public override void Initialize()
    {
        base.Initialize();
        destination = FindObjectOfType<DestinationScript>();
        objectRenderer = GetComponentInChildren<Renderer>();
        agent = GetComponent<NavMeshAgent>();
        collider = GetComponent<Collider>();
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
            (FindObjectOfType(typeof(PikminManager)) as PikminManager).StartIntetaction(this);

            agent.avoidancePriority = 50;
            agent.isStopped = false;
            agent.SetDestination(destination.Point());
            yield return new WaitUntil(() => agent.IsDone());
            agent.enabled = false;
            collider.enabled = false;

            (FindObjectOfType(typeof(PikminManager)) as PikminManager).FinishInteraction(this);

            //Delete UI
            if(fractionObject!=null)
                Destroy(fractionObject);

            //Capture Animation
            float time = 1.3f;
            Sequence s = DOTween.Sequence();
            s.AppendCallback(() => destination.StartCapture());
            s.Append(objectRenderer.material.DOColor(captureColor, "_EmissionColor", time));
            s.Join(transform.DOMove(destination.transform.position, time).SetEase(Ease.InQuint));
            s.Join(transform.DOScale(0, time).SetEase(Ease.InQuint));
            s.AppendCallback(() => destination.FinishCapture());
            s.Append(destination.transform.DOPunchScale(-Vector3.one * 35, .5f, 10, 1));
        }

    }

    public override void UpdateSpeed(int extra)
    {
        agent.speed = (extra > 0) ? originalAgentSpeed + (extra * .2f) : originalAgentSpeed;
    }

    public override void StopInteract()
    {
        agent.avoidancePriority = 30;
        agent.isStopped = true;
        if(destinationRoutine != null)
            StopCoroutine(destinationRoutine);
    }

    private void Update()
    {
        if(fractionObject != null)
            fractionObject.transform.position = Camera.main.WorldToScreenPoint(transform.position + uiOffset);
    }

}
