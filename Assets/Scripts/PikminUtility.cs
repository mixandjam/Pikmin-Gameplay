using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class PikminUtility
{
    public static bool IsDone(this NavMeshAgent agent)
    {
        return (!agent.pathPending &&
        agent.remainingDistance <= agent.stoppingDistance &&
        (!agent.hasPath || agent.velocity.sqrMagnitude == 0f));
    }
}
