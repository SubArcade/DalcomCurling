using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_PrivacyNotice : MonoBehaviour
{
    [SerializeField] private GameObject privacyNoticePopup;
    [SerializeField] private GameObject privacyNoticeKo;
    [SerializeField] private GameObject privacyNoticeEn;
    [SerializeField] private Button privacyNoticeCloseButton;

    private void OnEnable()
    {
        LocalizationManager.Instance.OnLanguageChanged += () =>
        {
            if (LocalizationManager.Instance.CurrentLanguage == "ko")
            {
                privacyNoticeKo.SetActive(true);
                privacyNoticeEn.SetActive(false);
            }
            else
            {
                privacyNoticeKo.SetActive(false);
                privacyNoticeEn.SetActive(true);
            }
        };
    }

    void Awake()
    {
        privacyNoticeCloseButton.onClick.AddListener(() => privacyNoticePopup.SetActive(false));
    }
}
