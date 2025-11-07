using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scr_PanelControl : MonoBehaviour
{
    [Header("화면 전환 캔버스(판넬)")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject detailedSettigsPanel;
    [SerializeField] private GameObject giftboxPopup;
    [SerializeField] private GameObject playerLevelInfoPopup;
    [SerializeField] private GameObject askUpgradePopup;
    [SerializeField] private GameObject failUpgradePopup;
    [SerializeField] private GameObject donutCodexPopup;
    [SerializeField] private GameObject donutUpgradePopup;
    [SerializeField] private GameObject donutCodexClickPopup;
    [SerializeField] private GameObject rewardCheckPopup;
    [SerializeField] private GameObject nickNameChangePopup;
    [SerializeField] private GameObject EntryPopUp;
    [SerializeField] private GameObject MatchingPopUp;
    
    [Header("화면 전환 버튼")]
    [SerializeField] private Button playerLevelInfoButton;
    [SerializeField] private Button donutCodexButton;
    [SerializeField] private Button donutUpgradeButton;
    [SerializeField] private Button EntryPopUpButton;
    [SerializeField] private Button detailedSettingsButton;
    [SerializeField] private Button testBattle;
    
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
    
    private Vector3 trashOriginalScale;
    private Vector3 startOriginalScale;
    private Vector3 UpgradeOriginalScale;
    private Vector3 CodexOriginalScale;
    private Vector3 EntryOriginalScale;
    
    void Awake()
    {
        UIManager.Instance.RegisterPanel(PanelId.StartPanel,startPanel);    
        UIManager.Instance.RegisterPanel(PanelId.LoginPanel,loginPanel);    
        UIManager.Instance.RegisterPanel(PanelId.MainPanel,mainPanel);    
        UIManager.Instance.RegisterPanel(PanelId.DetailedSettingsPanel,detailedSettigsPanel);    
        UIManager.Instance.RegisterPanel(PanelId.GiftboxPopup,giftboxPopup);
        UIManager.Instance.RegisterPanel(PanelId.PlayerLevelInfoPopup,playerLevelInfoPopup);
        UIManager.Instance.RegisterPanel(PanelId.DonutCodexPopup,donutCodexPopup);
        UIManager.Instance.RegisterPanel(PanelId.DonutUpgradePopup,donutUpgradePopup);
        UIManager.Instance.RegisterPanel(PanelId.EntryPopUp, EntryPopUp);
        UIManager.Instance.RegisterPanel(PanelId.MatchingPopUp, MatchingPopUp);
        
        playerLevelInfoButton.onClick.AddListener(()=>UIManager.Instance.Open(PanelId.PlayerLevelInfoPopup));
        donutCodexButton.onClick.AddListener(()=>UIManager.Instance.Open(PanelId.DonutCodexPopup));
        donutUpgradeButton.onClick.AddListener(()=>UIManager.Instance.Open(PanelId.DonutUpgradePopup));
        EntryPopUpButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.EntryPopUp));
        detailedSettingsButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.DetailedSettingsPanel));
        testBattle.onClick.AddListener(() => UIManager.Instance.Open(PanelId.MatchingPopUp));
    }

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

    //쓰레기통크기
    private void OnMouseTrashCan()
    {
        trashOriginalScale = TrashCanTransform.localScale;
        AddHoverEffect(TrashCan, TrashCanTransform, trashOriginalScale);
    } 
    
    //시작버튼크기
    private void OnMouseStartButton()
    {
        startOriginalScale = StartButtonTransform.localScale;
        AddHoverEffect(StartPopUp, StartButtonTransform, startOriginalScale);
    }
    
    //도감버튼크기
    private void OnMouseCodexButton()
    {
        CodexOriginalScale = CodexButtonTransform.localScale;
        AddHoverEffect(Codex, CodexButtonTransform, CodexOriginalScale);
    }
    
    //엔트리버튼크기
    private void OnMouseEntryButton()
    {
        EntryOriginalScale = EntryTransform.localScale;
        AddHoverEffect(Entry, EntryTransform, EntryOriginalScale);
    }
    
    //업그레이드버튼크기
    private void OnMouseUpgradeButton()
    {
        UpgradeOriginalScale = UpgradeTransform.localScale;
        AddHoverEffect(Upgrade, UpgradeTransform, UpgradeOriginalScale);
    }
    
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

    //드래그앤 드랍 삭제!
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped != null)
        {
            Destroy(dropped);
        }
    }
}
