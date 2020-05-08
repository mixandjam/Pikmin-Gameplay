using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SetDestination : MonoBehaviour
{
    public Transform target;
    public NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TargetUpdate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator TargetUpdate()
    {
        yield return new WaitForSeconds(.3f);
        agent.SetDestination(target.position);
        StartCoroutine(TargetUpdate());
    }
}
