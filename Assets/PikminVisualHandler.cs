using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PikminVisualHandler : MonoBehaviour
{
    public TrailRenderer trail;
    public ParticleSystem particleTrail;
    public ParticleSystem leafParticle;
    public ParticleSystem activationParticle;
    public Transform model;

    // Start is called before the first frame update
    void Start()
    {
        trail = GetComponentInChildren<TrailRenderer>();
        trail.emitting = false;
    }
}
