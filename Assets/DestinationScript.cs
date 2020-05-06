using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DestinationScript : MonoBehaviour
{
    private Renderer renderer;
    [ColorUsage(true,true)]
    public Color originalColor,captureColor;
    public Vector3 capturePointOffset;
    [Space]
    [Header("Particle Systems")]
    public ParticleSystem captureParticle;
    public ParticleSystem storeParticle;
    public ParticleSystem smokeParticle;
    public ParticleSystem capsuleParticle;

    private void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    public Vector3 Point()
    {
        return transform.position + capturePointOffset;
    }

    public void StartCapture()
    {
        captureParticle.Play();
    }

    public void FinishCapture()
    {
        storeParticle.Play();
        smokeParticle.Play();
        capsuleParticle.Play();
        renderer.material.DOColor(captureColor, "_EmissionColor", .2f).OnComplete(() => renderer.material.DOColor(originalColor, "_EmissionColor", .5f));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Point(), .2f);
    }
}
