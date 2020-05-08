using UnityEngine;

public class PikminAudioHandler : MonoBehaviour
{
    private Pikmin pikmin;

    public AudioSource generalSource;
    public AudioSource carrySource;
    public AudioSource throwSource;

    [Header("Sounds")]
    public AudioClip throwSound;
    public AudioClip noticeSound;
    public AudioClip grabSound;

    private void Awake()
    {
        pikmin = GetComponent<Pikmin>();
        pikmin.OnStartFollow.AddListener((x) => OnStartFollow(x));
        pikmin.OnStartThrow.AddListener((x) => OnStartThrow(x));
        pikmin.OnEndThrow.AddListener((x) => OnEndThrow(x));

        pikmin.OnStartCarry.AddListener((x) => OnStartCarry(x));
        pikmin.OnEndCarry.AddListener((x) => OnEndCarry(x));
    }

    public void OnStartFollow(int num)
    {
        carrySource.Stop();
        generalSource.PlayOneShot(noticeSound);
    }

    public void OnStartThrow(int num)
    {
        generalSource.PlayOneShot(throwSound);
        throwSource.Play();
    }

    public void OnEndThrow(int num)
    {
        if(pikmin.objective != null)
            generalSource.PlayOneShot(grabSound);
    }

    public void OnStartCarry(int num)
    {
        carrySource.Play();
    }

    public void OnEndCarry(int num)
    {
        carrySource.Stop();
    }
}
