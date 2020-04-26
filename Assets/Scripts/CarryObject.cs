using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CarryObject : InteractiveObject
{
    private NavMeshAgent agent;
    public override void Initialize()
    {
        base.Initialize();
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
    }
    public override void Interact()
    {
        agent.enabled = true;
    }
}
