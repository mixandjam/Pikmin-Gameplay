using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PikminVisualHandler : MonoBehaviour
{
    public TrailRenderer trail;
    public ParticleSystem particleTrail;
    public Transform model;

    // Start is called before the first frame update
    void Start()
    {
        trail = GetComponentInChildren<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
