using UnityEngine;
using TMPro;
using DG.Tweening;

public class InterfaceManager : MonoBehaviour
{

    public PikminManager pikminManager;
    public TextMeshProUGUI pikminCountText;

    void Start()
    {
        pikminManager.pikminFollow.AddListener((x) => UpdatePikminNumber(x));
    }

    void UpdatePikminNumber(int num)
    {
        pikminCountText.transform.DOComplete();
        pikminCountText.transform.DOPunchScale(Vector3.one/3, .3f, 10, 1);
        pikminCountText.text = num.ToString();
    }
}
