using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_EffectSlot : MonoBehaviour
{
    [SerializeField] private Button effectButton;
    [SerializeField] private GameObject activeImage;
    [SerializeField] private EffectType effectType;
    [SerializeField] private Scr_CharacterEffectSelector selector;
    
    public EffectType EffectType => effectType;
    
    private void Awake()
    {
        effectButton.onClick.AddListener(() => selector.SelectEffect(this)); 
    }
    
    // 이펙트 선택
    public void IsClickEffect(bool isClick)
    {
        if (isClick)
        {
            activeImage.SetActive(true);
            DataManager.Instance.InventoryData.curEffectType = effectType;
            
            // 애널리틱스 이펙트 변경
            AnalyticsManager.Instance.ReadyEquip();
        }
        else
        {
            activeImage.SetActive(false);
        }
    }
}
