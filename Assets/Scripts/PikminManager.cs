using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PikminEvent : UnityEvent<int> { }

[RequireComponent(typeof(PikminController))]
public class PikminManager : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform target = default;
    [SerializeField] private PikminController controller = default;
    [SerializeField] private float selectionRadius = 1;

    [Header("Spawning")]
    [SerializeField] private Pikmin pikminPrefab = default;

    [Header("Events")]
    public PikminEvent pikminFollow;


    private List<Pikmin> allPikmin = new List<Pikmin>();
    private int controlledPikmin = 0;

    // Start is called before the first frame update
    void Start()
    {
        PikminSpawner[] spawners = FindObjectsOfType(typeof(PikminSpawner)) as PikminSpawner[];
        foreach (PikminSpawner spawner in spawners)
        {
            spawner.SpawnPikmin(pikminPrefab, ref allPikmin);
        }
    }
    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            foreach (Pikmin pikmin in allPikmin)
            {
                if (Vector3.Distance(pikmin.transform.position, controller.hitPoint) < selectionRadius)
                {
                    if (pikmin.state != Pikmin.State.Follow)
                    {
                        pikmin.SetTarget(target, 0.25f);
                        controlledPikmin++;
                        pikminFollow.Invoke(controlledPikmin);
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            foreach (Pikmin pikmin in allPikmin)
            {
                if (pikmin.state == Pikmin.State.Follow)
                {
                    float time = Vector3.Distance(pikmin.transform.position, controller.hitPoint) / 5f;
                    pikmin.Throw(controller.hitPoint, 2);
                    controlledPikmin--;
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
                pikmin.SetIdle();
            }
        }
    }
}
