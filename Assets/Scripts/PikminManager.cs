using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Animations.Rigging;

[System.Serializable] public class PikminEvent : UnityEvent<int> { }
[System.Serializable] public class PlayerEvent : UnityEvent<Vector3> { }

[RequireComponent(typeof(PikminController))]
public class PikminManager : MonoBehaviour
{
    private MovementInput charMovement;

    [Header("Positioning")]
    public Transform pikminThrowPosition;

    [Header("Targeting")]
    [SerializeField] private Transform target = default;
    [SerializeField] private PikminController controller = default;
    [SerializeField] private float selectionRadius = 1;

    [Header("Spawning")]
    [SerializeField] private Pikmin pikminPrefab = default;

    [Header("Events")]
    public PikminEvent pikminFollow;
    //public PlayerEvent pikminHold;
    public PlayerEvent pikminThrow;

    private List<Pikmin> allPikmin = new List<Pikmin>();
    private int controlledPikmin = 0;

    public Rig whistleRig;
    public ParticleSystem whistlePlayerParticle;

    // Start is called before the first frame update
    void Start()
    {
        charMovement = FindObjectOfType<MovementInput>();

        PikminSpawner[] spawners = FindObjectsOfType(typeof(PikminSpawner)) as PikminSpawner[];
        foreach (PikminSpawner spawner in spawners)
        {
            spawner.SpawnPikmin(pikminPrefab, ref allPikmin);
        }
    }

    public void SetWhistleRadius(float radius)
    {
        selectionRadius = radius;
    }

    public void SetWhistleRigWeight(float weight)
    {
        whistleRig.weight = weight;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Time.timeScale = Time.timeScale == 1 ? .2f : 1;

        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);

        if (Input.GetMouseButtonDown(1))
        {
            whistlePlayerParticle.Play();
            controller.audio.Play();
            DOVirtual.Float(0, (5/2) + .5f, .5f, SetWhistleRadius).SetId(2);
            DOVirtual.Float(0, 1, .2f, SetWhistleRigWeight).SetId(1);

            charMovement.transform.GetChild(0).DOScaleY(27f, .05f).SetLoops(-1, LoopType.Yoyo).SetId(3);

            controller.visualCylinder.localScale = Vector3.zero;
            controller.visualCylinder.DOScaleX(5, .5f);
            controller.visualCylinder.DOScaleZ(5, .5f);
            controller.visualCylinder.DOScaleY(2, .4f).SetDelay(.4f);
        }

        if (Input.GetMouseButtonUp(1))
        {
            whistlePlayerParticle.Stop();
            controller.audio.Stop(); DOTween.Kill(2); DOTween.Kill(1); DOTween.Kill(3);
            charMovement.transform.GetChild(0).DOScaleY(28, .1f);
            DOVirtual.Float(whistleRig.weight, 0, .2f, SetWhistleRigWeight);
            selectionRadius = 0;
            controller.visualCylinder.DOKill();
            controller.visualCylinder.DOScaleX(0, .2f);
            controller.visualCylinder.DOScaleZ(0, .2f);
            controller.visualCylinder.DOScaleY(0f, .05f);
        }

        if (Input.GetMouseButton(1))
        {
            foreach (Pikmin pikmin in allPikmin)
            {
                if (Vector3.Distance(pikmin.transform.position, controller.hitPoint) < selectionRadius)
                {
                    if (pikmin.state != Pikmin.State.Follow)
                    {
                        if (pikmin.isFlying || pikmin.isGettingIntoPosition)
                            return;

                        pikmin.SetTarget(target, 0.25f);
                        controlledPikmin++;
                        pikminFollow.Invoke(controlledPikmin);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            foreach (Pikmin pikmin in allPikmin)
            {
                if (pikmin.state == Pikmin.State.Follow && Vector3.Distance(pikmin.transform.position, charMovement.transform.position) < 2)
                {
                    pikmin.agent.enabled = false;
                    float delay = .05f;
                    pikmin.transform.DOMove(pikminThrowPosition.position,delay);

                    pikmin.Throw(controller.hitPoint, .5f, delay);
                    controlledPikmin--;

                    pikminThrow.Invoke(controller.hitPoint);
                    pikminFollow.Invoke(controlledPikmin);
                    break;
                }
            }
        }

    }
    public void FinishInteraction(InteractiveObject objective)
    {
        foreach (Pikmin pikmin in allPikmin)
        {
            if (pikmin.objective == objective)
            {
                pikmin.SetCarrying(false);
                pikmin.SetIdle();
            }
        }
    }

    public void StartIntetaction(InteractiveObject objective)
    {
        foreach (Pikmin pikmin in allPikmin)
        {
            if (pikmin.objective == objective)
            {
                pikmin.SetCarrying(true);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(controller.target.position, selectionRadius);
    }

}
