using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerPikminInteraction : MonoBehaviour
{

    private PikminManager pikminManager;
    private Animator anim;

    void Start()
    {
        pikminManager = FindObjectOfType<PikminManager>();
        pikminManager.pikminThrow.AddListener((x) => Throw(x));

        anim = GetComponent<Animator>();
    }

    void Update()
    {
        
    }

    public void Throw(Vector3 hitPoint)
    {
        transform.DOLookAt(new Vector3(hitPoint.x, transform.position.y, hitPoint.z), .1f);
        //transform.LookAt(new Vector3(hitPoint.x, transform.position.y, hitPoint.z));
        anim.SetTrigger("throw");
    }
}
