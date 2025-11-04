using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scr_Main_Panel : MonoBehaviour
{
    [Header("휴지통 관련")]
    [SerializeField] private GameObject TrashCan;
    [SerializeField] private Transform TrashCanTransform;
    [SerializeField] private Image TrashCanImage;

    [Header("각각 해당하는 팝업창 열기")]
    [SerializeField] private Button EntryButton; //미완성  11.03
    [SerializeField] private Button CodexButton;
    [SerializeField] private Button UpgradeButton;
    [SerializeField] private Button LevelButton;


    //메인 패널창에 아직 연결 및 생성되지않은 것
    // 1. 휴지통
    // 2. 엔트리
    // 3. 환경설정
    // 4. 컬링시작
    // 5. 주문서
    // 6. 머지(합성)칸
    // 그외에는 더 있을 수 있으니 확인 부탁드립니다.

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f); // 혹시라도 로딩 지연 대비

        Transform canvas = GameObject.FindGameObjectWithTag("MainCanvas")?.transform;

        EntryButton = transform.Find("Bottom/ButtonGroup/Entry_Button")?.GetComponent<Button>();
        //엔트리버튼 연결만 해놓고 팝업창은 따로 만들어야함
        CodexButton = transform.Find("Bottom/ButtonGroup/Codex_Button")?.GetComponent<Button>();       
        //휴지통버튼 연결만 해놓고 팝업차은 따로 만들어야함
        UpgradeButton = transform.Find("Bottom/ButtonGroup/Upgrade_Button")?.GetComponent<Button>();
        LevelButton = transform.Find("Top/Level")?.GetComponent<Button>();
        
        TrashCan = transform.Find("Bottom/ButtonGroup/basket_Button")?.gameObject;
        TrashCanTransform = TrashCan.transform;
        TrashCanImage = TrashCan.GetComponent<Image>();
        OnMouseTrashCan();
    }

    private Vector3 trashOriginalScale;

    private void OnMouseTrashCan() 
    {
        trashOriginalScale = TrashCanTransform.localScale;

        EventTrigger trigger = TrashCan.GetComponent<EventTrigger>();
        if (trigger != null) 
        {
            trigger = TrashCan.AddComponent<EventTrigger>();
        }
        AddTrigger(trigger, EventTriggerType.PointerEnter, () =>
        {
            TrashCanTransform.DOScale(trashOriginalScale * 1.2f, 0.2f).SetEase(Ease.OutBack);
        });

        AddTrigger(trigger, EventTriggerType.PointerExit, () =>
        {
            TrashCanTransform.DOScale(trashOriginalScale, 0.2f).SetEase(Ease.OutBack);
        });

    }
    private void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((_) => action());
        trigger.triggers.Add(entry);
    }
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped != null)
        {
            Destroy(dropped);
        }
    }

}
