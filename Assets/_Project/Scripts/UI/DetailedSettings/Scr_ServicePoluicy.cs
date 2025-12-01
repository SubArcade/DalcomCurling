using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_ServicePoluicy : MonoBehaviour
{
    [Header("팝업")]
    [SerializeField] private GameObject servicePolicyPopup;
    [SerializeField] private GameObject servicePolicyKo;
    [SerializeField] private GameObject servicePolicyEn;
    [SerializeField] private Button servicePolicyCloseButton;
    
    private void OnEnable()
    {
        LocalizationManager.Instance.OnLanguageChanged += () =>
        {
            if (LocalizationManager.Instance.CurrentLanguage == "ko")
            {
                servicePolicyKo.SetActive(true);
                servicePolicyEn.SetActive(false);
            }
            else
            {
                servicePolicyKo.SetActive(false);
                servicePolicyEn.SetActive(true);
            }
        };
    }

    void Awake()
    {
        servicePolicyCloseButton.onClick.AddListener(() => servicePolicyPopup.SetActive(false));
    }
}
