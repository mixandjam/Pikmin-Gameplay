using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TMP_Text;

public class InteractiveObject : MonoBehaviour
{

    [Header("Connections")]
    [SerializeField] Text interactText = default;
    [Header("Variables")]
    [SerializeField] private int pikminNeeded = 1;
    public float radius = 1;
    private int currentPikmin = 0;

    private void Awake()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        interactText.enabled = false;
    }

    public bool AssignPikmin()
    {
        currentPikmin++;
        interactText.enabled = true;
        interactText.text = $"{currentPikmin}/{pikminNeeded}";

        if (currentPikmin == pikminNeeded)
            Interact();

        return true;
    }
    public void ReleasePikmin()
    {
        if (currentPikmin == 0)
            return;

        currentPikmin--;

        if (currentPikmin == 0)
            interactText.enabled = false;
        else
            interactText.text = $"{currentPikmin}/{pikminNeeded}";

        if (currentPikmin < pikminNeeded)
            StopInteract();
    }

    public Vector3 GetPositon()
    {
        float angle = currentPikmin * Mathf.PI * 2f / pikminNeeded;
        return transform.position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
    }
    public virtual void Interact()
    {

    }
    public virtual void StopInteract()
    {

    }
}
