using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TMP_Text;

public class InteractiveObject : MonoBehaviour
{
    [SerializeField] private int pikminNeeded = 1;
    private int currentPikmin = 0;
    [SerializeField] Text interactText = default;
    public float radius = 1;

    private void Start()
    {
        interactText.enabled = false;
    }

    public bool AssignPikmin()
    {
        if (currentPikmin >= pikminNeeded)
            return false;

        currentPikmin++;
        interactText.enabled = true;
        interactText.text = $"{currentPikmin}/{pikminNeeded}";

        if (currentPikmin == pikminNeeded)
            Interact();

        return true;
    }

    public Vector3 GetPositon()
    {
        float angle = currentPikmin * Mathf.PI * 2f / pikminNeeded;
        return transform.position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
    }
    public virtual void Interact()
    {

    }
}
