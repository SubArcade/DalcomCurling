using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    [SerializeField] private Toggle toggle1; // effectToggle
    [SerializeField] private Toggle toggle2; // characterToggle

    private void Start()
    {
        //toggle1.onValueChanged.AddListener((isOn) =>
        //{

        //}

        //toggle2.onValueChanged.AddListener((isOn) =>
        //{

        //} 
    }

    private void OnToggle1Changed(bool isOn)
    {
        //if (isOn)
        //{
        //    Debug.Log("effect 켜짐");
        //}
    }

    private void OnToggle2Changed(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("character 켜짐");
        }
    }
}
