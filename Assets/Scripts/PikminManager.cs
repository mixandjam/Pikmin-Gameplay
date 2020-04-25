using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PikminController))]
public class PikminManager : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform target = default;
    [SerializeField] private PikminController controller = default;
    private List<Pikmin> allPikmin = new List<Pikmin>();

    [Header("Spawning")]
    [SerializeField] private int spawnNum = 0;
    [SerializeField] private Pikmin pikminPrefab = default;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < spawnNum; i++)
        {
            Pikmin newPikmin = Instantiate(pikminPrefab);
            newPikmin.transform.position = spawnPosition + Random.insideUnitSphere;
            allPikmin.Add(newPikmin);
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (Pikmin pikmin in allPikmin)
            {
                if (pikmin.state == Pikmin.State.Idle && Vector3.Distance(pikmin.transform.position, controller.hitPoint) < 4)
                    pikmin.SetTarget(target, 0.25f);

            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (Pikmin pikmin in allPikmin)
            {
                if (pikmin.state == Pikmin.State.Follow)
                {
                    float time = 0.1f * Vector3.Distance(pikmin.transform.position, controller.hitPoint);
                    pikmin.Throw(controller.hitPoint, time);
                    break;
                }

            }
        }
    }
}
