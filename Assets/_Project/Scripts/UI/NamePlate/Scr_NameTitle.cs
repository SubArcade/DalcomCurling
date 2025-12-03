using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_NameTitle : MonoBehaviour
{
    public  TMP_Text namePlateTitleText;
    public Toggle toggleButton;
    public NamePlateType namePlateType; 
    public NamePlaateSO namePlateSO;
    private NamePlate selectedPlate;
    private void OnEnable()
    {
        LocalizationManager.Instance.OnLanguageChanged += ApplyLocalizedText;
        LoadNamePlateData();
        ApplyLocalizedText();
        SoundManager.Instance.buttonClick();
    }

    private void OnDisable()
    {
        LocalizationManager.Instance.OnLanguageChanged -= ApplyLocalizedText;
        SoundManager.Instance.buttonClick();
    }
    
    /// <summary>
    /// SO 안에서 이 슬롯의 타입과 맞는 NamePlate 하나 찾아오기
    /// </summary>
    private void LoadNamePlateData()
    {
        if (namePlateSO == null)
        {
            Debug.LogError($"[Scr_NameTitle] namePlateSO가 인스펙터에 비어 있음: {gameObject.name}");
            return;
        }

        selectedPlate = null;

        foreach (var plate in namePlateSO.namePlateList)
        {
            if (plate.plateType == namePlateType)
            {
                selectedPlate = plate;
                break;
            }
        }

        if (selectedPlate == null)
        {
            Debug.LogError($"[Scr_NameTitle] SO에서 해당 NamePlateType 못 찾음: {namePlateType}, GameObject: {gameObject.name}");
        }
    }

    /// <summary>
    /// 현재 언어에 맞춰 텍스트 적용
    /// </summary>
    public void ApplyLocalizedText()
    {
        if (selectedPlate == null)
            return;

        // 나중에 LocalizationManager의 현재 언어로 변경하면 됨
        bool isKorean = Application.systemLanguage == SystemLanguage.Korean;

        namePlateTitleText.text = isKorean
            ? selectedPlate.koNamePlate
            : selectedPlate.enNamePlate;
    }

}
