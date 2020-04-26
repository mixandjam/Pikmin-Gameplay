using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PikminController : MonoBehaviour
{
    [HideInInspector] public Vector3 hitPoint = Vector3.zero;
    [SerializeField] private Transform follow = default;
    [SerializeField] private Vector3 followOffset = Vector3.zero;
    [SerializeField] private Transform target = default;
    [SerializeField] private Vector3 targetOffset = Vector3.zero;
    private Camera cam = default;
    private LineRenderer line = default;
    const int linePoints = 5;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        line = GetComponentInChildren<LineRenderer>();
        line.positionCount = linePoints;
    }

    void Update()
    {
        UpdateMousePosition();
    }

    void UpdateMousePosition()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            hitPoint = hit.point;
            target.position = hit.point + targetOffset;
            target.up = Vector3.Lerp(target.up, hit.normal, .3f);
            for (int i = 0; i < linePoints; i++)
            {
                Vector3 linePos = Vector3.Lerp(follow.position + followOffset, target.position, (float)i / 5f);
                line.SetPosition(i, linePos);
            }
        }
    }

}
