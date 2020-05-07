using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using DG.Tweening;

[System.Serializable] public class PikminEvent : UnityEvent<int> { }
[System.Serializable] public class PlayerEvent : UnityEvent<Vector3> { }

[RequireComponent(typeof(PikminController))]
public class PikminManager : MonoBehaviour
{
    private MovementInput charMovement;
    private Pikmin currentPikmin;

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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Time.timeScale = Time.timeScale == 1 ? .2f : 1;

        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);

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
}
