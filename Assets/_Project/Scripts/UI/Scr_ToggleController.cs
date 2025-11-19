using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    [SerializeField] private Toggle toggle1;
    [SerializeField] private Toggle toggle2;

    private void Start()
    {
        toggle1.onValueChanged.AddListener(OnToggle1Changed);
        toggle1.onValueChanged.AddListener(OnToggle2Changed);
    }

    private void OnToggle1Changed(bool isOn)
    {
        if (isOn)
        {
            toggle2.isOn = false;
        }
    }

    private void OnToggle2Changed(bool isOn)
    {
        if ( isOn)
        {
            toggle1.isOn = false;
        }
    }
}
