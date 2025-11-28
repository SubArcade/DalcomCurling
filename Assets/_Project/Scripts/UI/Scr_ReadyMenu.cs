using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_ReadyMenu : MonoBehaviour
{
    [Header("버튼,토글")]
    public Toggle effectBtn;
    public Toggle characterBtn;
    public Image effectBtnImage;
    public Image characterBtnImage;
    public ToggleGroup characterEffectSelector;
    public Button activeBtn;

    [Header("이펙트, 캐릭터 아이템이 담길 판넬")]
    public GameObject EffectBox;
    public GameObject CharacterBox;

    // 비활성화 색상 (2B3F5D)
    private readonly Color32 activeColor = new Color32(0x2B, 0x3F, 0x5D, 0xFF);
    // 활성화 색상 
    private readonly Color32 inactiveColor = new Color32(0x53, 0x6E, 0x96 , 0xFF);

    void Start()
    {
        effectBtn.group = characterEffectSelector;
        characterBtn.group = characterEffectSelector;

        effectBtn.onValueChanged.AddListener(OnEffectToggle);
        characterBtn.onValueChanged.AddListener(OnCharacterToggle);

        UpdateBoxes();
    }

    //이펙트토글 ON
    private void OnEffectToggle(bool isOn)
    {
        if (isOn)
        {
            EffectBox.SetActive(true);
            CharacterBox.SetActive(false);

            effectBtnImage.color = isOn ? activeColor : inactiveColor;
            characterBtnImage.color = !isOn ? activeColor : inactiveColor;
        }
    }
    //캐릭터토글 ON
    private void OnCharacterToggle(bool isOn)
    {
        if (isOn)
        {
            CharacterBox.SetActive(true);
            EffectBox.SetActive(false);

            characterBtnImage.color = isOn ? activeColor : inactiveColor;
            effectBtnImage.color = !isOn ? activeColor : inactiveColor;
        }
    }

    //초기화용
    private void UpdateBoxes()
    {
        // 현재 토글 상태에 맞게 박스 초기화
        EffectBox.SetActive(effectBtn.isOn);
        CharacterBox.SetActive(characterBtn.isOn);
    }

}
