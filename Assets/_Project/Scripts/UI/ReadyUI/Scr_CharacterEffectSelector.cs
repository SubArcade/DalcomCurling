using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_CharacterEffectSelector : MonoBehaviour
{
    [SerializeField] private List<Scr_EffectSlot> effectSlots;
    [SerializeField] private List<Scr_CharacterSlot> characterSlots;
    
    private Scr_EffectSlot curEffectSlot;
    private Scr_CharacterSlot characterSlot;

    private void OnEnable()
    {
        InitEffectSlotsFromData();
        InitCharacterSlotsFromData();
    }
    
    private void InitEffectSlotsFromData()
    {
        var inv = DataManager.Instance.InventoryData;
        if (inv == null) return;

        var ownedEffects = inv.effectList;          // 소유한 EffectType 리스트
        var curEffect    = inv.curEffectType;       // 현재 선택된 EffectType

        Scr_EffectSlot toSelect = null;

        foreach (var slot in effectSlots)
        {
            if (slot == null) continue;

            bool isOwned = ownedEffects != null && ownedEffects.Contains(slot.EffectType);

            // 소유한 이펙트만 슬롯 자체 활성화
            slot.gameObject.SetActive(isOwned);

            // 처음엔 모두 선택 해제
            slot.IsClickEffect(false);

            // 현재 선택 타입과 같으면 나중에 SelectEffect로 처리
            if (isOwned && slot.EffectType.Equals(curEffect))
            {
                toSelect = slot;
            }
        }

        // 저장된 curEffectType 있으면 그 슬롯을 선택 상태로
        if (toSelect != null)
        {
            SelectEffect(toSelect);
        }
    }
    
    // 캐릭터 쪽 초기화
    // (필드 이름은 프로젝트에서 쓰는 걸로 바꿔도 됨)
    private void InitCharacterSlotsFromData()
    {
        var inv = DataManager.Instance.InventoryData;
        if (inv == null) return;

        var ownedChars   = inv.characterList;
        var curCharType  = inv.curCharacterType;

        Scr_CharacterSlot toSelect = null;

        foreach (var slot in characterSlots)
        {
            if (slot == null) continue;

            bool isOwned = ownedChars != null && ownedChars.Contains(slot.CharacterType);

            slot.gameObject.SetActive(isOwned);
            slot.IsClickEffect(false);

            if (isOwned && slot.CharacterType.Equals(curCharType))
            {
                toSelect = slot;
            }
        }

        if (toSelect != null)
        {
            SelectCharacter(toSelect);
        }
    }
    
    // 선택된 이펙트 활성화
    public void SelectEffect(Scr_EffectSlot clickedSlot)
    {
        foreach (var slot in effectSlots)
        {
            slot.IsClickEffect(false);
        }
        
        clickedSlot.IsClickEffect(true);
        curEffectSlot = clickedSlot;
    }
    
    // 선택된 캐릭터 활성하
    public void SelectCharacter(Scr_CharacterSlot clickedSlot)
    {
        foreach (var slot in characterSlots)
        {
            slot.IsClickEffect(false);
        }
        
        clickedSlot.IsClickEffect(true);
        characterSlot = clickedSlot;
    }
}
