using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_CharacterSlot : MonoBehaviour
{
    [SerializeField] private Button effectButton;
    [SerializeField] private GameObject activeImage;
    [SerializeField] private CharacterType characterType;
    [SerializeField] private Scr_CharacterEffectSelector selector;
    
    public CharacterType CharacterType => characterType;
    
    private void Awake()
    {
        effectButton.onClick.AddListener(() => selector.SelectCharacter(this)); 
    }
    
    
    // 이펙트 선택
    public void IsClickEffect(bool isClick)
    {
        if (isClick)
        {
            activeImage.SetActive(true);
            DataManager.Instance.InventoryData.curCharacterType = characterType;
        }
        else
        {
            activeImage.SetActive(false);
        }
    }
}
