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

    [Header("시작버튼 관련")]
    [SerializeField] private GameObject StartPopUp;
    [SerializeField] private Transform StartButtonTransform;
    [SerializeField] private Image startImage;

    [Header("도감버튼 관련")]
    [SerializeField] private GameObject Codex;
    [SerializeField] private Transform CodexButtonTransform;
    [SerializeField] private Image CodexImage;

    [Header("엔트리버튼 관련")]
    [SerializeField] private GameObject Entry;
    [SerializeField] private Transform EntryTransform;
    [SerializeField] private Image EntryImage;

    [Header("업그레이드버튼 관련")]
    [SerializeField] private GameObject Upgrade;
    [SerializeField] private Transform UpgradeTransform;
    [SerializeField] private Image UpgradeImage;


    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f); // 혹시라도 로딩 지연 대비

        TrashCan = transform.Find("Bottom/ButtonGroup/basket_Button")?.gameObject;
        TrashCanTransform = TrashCan.transform;
        TrashCanImage = TrashCan.GetComponent<Image>();

        StartPopUp = transform.Find("Bottom/Battle_Button")?.gameObject;
        StartButtonTransform = StartPopUp.transform;
        startImage = StartPopUp.GetComponent<Image>();
        
        Codex = transform.Find("Bottom/ButtonGroup/Codex_Button")?.gameObject;
        CodexButtonTransform = Codex.transform;
        CodexImage = Codex.GetComponent<Image>();

        Entry = transform.Find("Bottom/ButtonGroup/Entry_Button")?.gameObject;
        EntryTransform = Entry.transform;
        EntryImage = Entry.GetComponent<Image>();

        Upgrade = transform.Find("Bottom/ButtonGroup/Upgrade_Button")?.gameObject;
        UpgradeTransform = Upgrade.transform;
        UpgradeImage = Upgrade.GetComponent<Image>();

        OnMouseTrashCan();
        OnMouseStartButton();
        OnMouseCodexButton();
        OnMouseEntryButton();
        OnMouseUpgradeButton();
    }

    private Vector3 trashOriginalScale;
    private Vector3 startOriginalScale;
    private Vector3 UpgradeOriginalScale;
    private Vector3 CodexOriginalScale;
    private Vector3 EntryOriginalScale;

    private void OnMouseTrashCan()
    {
        trashOriginalScale = TrashCanTransform.localScale;
        AddHoverEffect(TrashCan, TrashCanTransform, trashOriginalScale);
    } //쓰레기통크기
    private void OnMouseStartButton()
    {
        startOriginalScale = StartButtonTransform.localScale;
        AddHoverEffect(StartPopUp, StartButtonTransform, startOriginalScale);
    }//시작버튼크기
    private void OnMouseCodexButton()
    {
        CodexOriginalScale = CodexButtonTransform.localScale;
        AddHoverEffect(Codex, CodexButtonTransform, CodexOriginalScale);
    }//도감버튼크기
    private void OnMouseEntryButton()
    {
        EntryOriginalScale = EntryTransform.localScale;
        AddHoverEffect(Entry, EntryTransform, EntryOriginalScale);
    }//엔트리버튼크기
    private void OnMouseUpgradeButton()
    {
        UpgradeOriginalScale = UpgradeTransform.localScale;
        AddHoverEffect(Upgrade, UpgradeTransform, UpgradeOriginalScale);
    }//업그레이드버튼크기
    //마우스 올리면 크기가 커져요!
    private void AddHoverEffect(GameObject target, Transform targetTransform, Vector3 originalScale)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = target.AddComponent<EventTrigger>();
        }

        AddTrigger(trigger, EventTriggerType.PointerEnter, () =>
        {
            targetTransform.DOScale(originalScale * 1.2f, 0.2f).SetEase(Ease.OutBack);
        });

        AddTrigger(trigger, EventTriggerType.PointerExit, () =>
        {
            targetTransform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack);
        });
    }

    //마우스 이벤트 추가
    private void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((_) => action());
        trigger.triggers.Add(entry);
    }
}
