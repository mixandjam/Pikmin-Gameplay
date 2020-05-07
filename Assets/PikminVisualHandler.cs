using UnityEngine;
using DG.Tweening;

public class PikminVisualHandler : MonoBehaviour
{
    private Pikmin pikmin;

    public TrailRenderer trail;
    public ParticleSystem particleTrail;
    public ParticleSystem leafParticle;
    public ParticleSystem activationParticle;
    public Transform model;

    private void Awake()
    {
        pikmin = GetComponent<Pikmin>();
        pikmin.OnStartFollow.AddListener((x) => OnStartFollow(x));
        pikmin.OnStartThrow.AddListener((x) => OnStartThrow(x));
        pikmin.OnEndThrow.AddListener((x) => OnEndThrow(x));
    }
    void Start()
    {
        trail = GetComponentInChildren<TrailRenderer>();
        trail.emitting = false;
    }

    public void OnStartFollow(int num)
    {
        transform.DOJump(transform.position, .4f, 1, .3f);
        transform.DOPunchScale(-Vector3.up / 2, .3f, 10, 1).SetDelay(Random.Range(0, .1f));

        activationParticle.Play();
        leafParticle.Clear();
        leafParticle.Stop();
    }

    public void OnStartThrow(int num)
    {
        trail.Clear();
        trail.emitting = true;
        particleTrail.Play();
    }

    public void OnEndThrow(int num)
    {
        particleTrail.Stop();
        trail.emitting = false;

        if(pikmin.objective == null)
            leafParticle.Play();
    }
}
