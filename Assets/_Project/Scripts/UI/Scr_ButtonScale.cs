using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scr_ButtonScale : MonoBehaviour
{
    [Header("ReadyMenu UI")]
    [SerializeField] private GameObject ReadyMenuUI;

    [Header("상점버튼 연결")]
    [SerializeField] private Button marketBtn;

    [Header("휴지통 관련")]
    [SerializeField] private GameObject TrashCan;
    [SerializeField] private Transform TrashCanTransform;
    [SerializeField] private Image TrashCanImage;
    [SerializeField] private Sprite TrashCanSprite;
    [SerializeField] private Sprite openTrashCanImage;

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

    void Start()
    {
        marketBtn.onClick.AddListener(() => 
        {
            UIManager.Instance.Open(PanelId.ShopPopUp);
            SoundManager.Instance.buttonClick();
        });

        OnMouseTrashCan();
        OnMouseStartButton();
        OnMouseCodexButton();
        OnMouseEntryButton();
        OnMouseUpgradeButton();

        //레디메뉴가 꺼지면 이벤트 호출
        var readyMenuBehaviour = ReadyMenuUI.AddComponent<ReadyMenuWatcher>();
        readyMenuBehaviour.onDisabled += ResetButtonScales;
    }

    private Vector3 trashOriginalScale;
    private Vector3 startOriginalScale;
    private Vector3 UpgradeOriginalScale;
    private Vector3 CodexOriginalScale;
    private Vector3 EntryOriginalScale;

    private void OnMouseTrashCan()
    {
        trashOriginalScale = TrashCanTransform.localScale;

        EventTrigger trigger = TrashCan.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = TrashCan.AddComponent<EventTrigger>();
        }

        // 마우스 올릴 때
        AddTrigger(trigger, EventTriggerType.PointerEnter, () =>
        {
            // 이미지 교체 (열린 쓰레기통)
            TrashCanImage.sprite = openTrashCanImage;

            // 크기 확대
            TrashCanTransform.DOScale(trashOriginalScale * 1.2f, 0.2f).SetEase(Ease.OutBack);
        });

        // 마우스 내릴 때
        AddTrigger(trigger, EventTriggerType.PointerExit, () =>
        {
            // 크기를 원래대로 돌린 뒤 → 애니메이션 완료 후 이미지 교체
            TrashCanTransform.DOScale(trashOriginalScale, 0.2f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    // 이미지 교체 (닫힌 쓰레기통)
                    TrashCanImage.sprite = TrashCanSprite;
                });
        });

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

    //버튼 크기 초기화
    private void ResetButtonScales()
    {
        TrashCanTransform.localScale = trashOriginalScale;
        StartButtonTransform.localScale = startOriginalScale;
        CodexButtonTransform.localScale = CodexOriginalScale;
        EntryTransform.localScale = EntryOriginalScale;
        UpgradeTransform.localScale = UpgradeOriginalScale;
    }

}

//레디메뉴가 켜져있나 꺼져있나 판단하는 보조 클래스
public class ReadyMenuWatcher : MonoBehaviour
{
    public System.Action onDisabled; //외부에서 구독할수있는 콜백

    void OnDisable()
    {
        onDisabled?.Invoke(); //비활성화되면 호출
    }
}
