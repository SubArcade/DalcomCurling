using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    [SerializeField] private Toggle toggle1; // effectToggle
    [SerializeField] private Toggle toggle2; // characterToggle
    //[SerializeField] private Image toggle1Img;
    //[SerializeField] private Image toggle2Img;
    [SerializeField] private GameObject panel1; // effectPanel
    [SerializeField] private GameObject panel2; // characterPanel
    [SerializeField] private ToggleGroup toggleGroup; // CharacterEffectSelector에 붙은 ToggleGroup
    private void Start()
    {
        toggle1.onValueChanged.AddListener(OnToggle1Changed);
        toggle2.onValueChanged.AddListener(OnToggle2Changed);

        // 초기 상태 반영
      UpdateUI();
    }

    private void OnToggle1Changed(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("effect 켜짐");
            UpdateUI();
        }
    }

    private void OnToggle2Changed(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("character 켜짐");
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        bool isEffectOn = toggle1.isOn;
        bool isCharacterOn = toggle2.isOn;

        panel1.SetActive(isEffectOn);
        panel2.SetActive(isCharacterOn);

      //  toggle1Img.enabled = isEffectOn;
      //  toggle2Img.enabled = isCharacterOn;
    }

}
