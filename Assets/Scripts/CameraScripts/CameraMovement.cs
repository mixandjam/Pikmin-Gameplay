using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CameraMovement : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCam;
    private CinemachineOrbitalTransposer transposer;
    public float cameraStep = 90;
    public float time = .5f;

    private void Start()
    {
        transposer = virtualCam.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            DOVirtual.Float(transposer.m_XAxis.Value, transposer.m_XAxis.Value + cameraStep, time,SetCameraAxis).SetEase(Ease.OutSine);
        if(Input.GetKeyDown(KeyCode.E))
            DOVirtual.Float(transposer.m_XAxis.Value, transposer.m_XAxis.Value - cameraStep, time, SetCameraAxis).SetEase(Ease.OutSine);
    }

    void SetCameraAxis(float x)
    {
        transposer.m_XAxis.Value = x;
    }
}
