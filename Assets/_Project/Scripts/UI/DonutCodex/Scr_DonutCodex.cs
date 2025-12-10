using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Scr_DonutCodex : MonoBehaviour
{
    [Header("각 타입별 슬롯 리스트")]
    [SerializeField] private List<Scr_donutShell> hardShells;   // 단단
    [SerializeField] private List<Scr_donutShell> softShells;  // 말랑
    [SerializeField] private List<Scr_donutShell> moistShells;   // 촉촉

    [Header("버튼들")]
    [SerializeField, Tooltip("단단")] private Button hardButton; 
    [SerializeField, Tooltip("말랑")] private Button softButton; 
    [SerializeField, Tooltip("촉촉")] private Button moistButton; 
    [SerializeField, Tooltip("촉촉")] private Button infoButton; 
    
    [Header("판넬")]
    [SerializeField, Tooltip("말랑")] private GameObject hardPanel; 
    [SerializeField, Tooltip("단단")] private GameObject softPanel; 
    [SerializeField, Tooltip("촉촉")] private GameObject moistPanel; 
    [SerializeField, Tooltip("촉촉")] private GameObject infoPanel; 
    
    [Header("버튼 이미지")]
    [SerializeField, Tooltip("단단")] private Image hardImage;
    [SerializeField, Tooltip("촉촉")] private Image softImage;
    [SerializeField, Tooltip("말랑")] private Image moistImage;
    
    [Header("닫기 버튼")]
    [SerializeField, Tooltip("닫기")] private Button closeButton;
    [SerializeField, Tooltip("닫기")] private Button infoPopupCloseButton;
    
    [Header("도넛 정보창")]
    [SerializeField, Tooltip("이미지")] private Image image;
    [SerializeField, Tooltip("설명")] private TMP_Text infotext;
    [SerializeField, Tooltip("레벨")] private TMP_Text levelText;
    [SerializeField, Tooltip("도넛 종류")] private TMP_Text titleText;
    
    [Header("기본 이미지")]
    [SerializeField, Tooltip("단단")] private Sprite baseHardSprite;
    [SerializeField, Tooltip("촉촉")] private Sprite baseSoftSprite;
    [SerializeField, Tooltip("말랑")] private Sprite baseMoistSprite;
    
    public Scr_donutShell curDonutShell;
    
    

    private void OnEnable()
    {
        OnUserDataChangedHandler();
        SubscribeList(hardShells);
        SubscribeList(softShells);
        SubscribeList(moistShells);
        RefreshHard();
    }

    private void OnDisable()
    {
        UnsubscribeList(hardShells);
        UnsubscribeList(softShells);
        UnsubscribeList(moistShells);
    }

    // 도넛 클릭시 설명창 업데이트
    private void SubscribeList(List<Scr_donutShell> list)
    {
        if (list == null) return;

        foreach (var shell in list)
        {
            if (shell == null) continue;
            shell.OnDonutClicked += InfoSet;
        }
    }

    private void UnsubscribeList(List<Scr_donutShell> list)
    {
        if (list == null) return;

        foreach (var shell in list)
        {
            if (shell == null) continue;
            shell.OnDonutClicked -= InfoSet;
        }
    }
    
    private void OnUserDataChangedHandler()
    {
        // 데이터 들어온 뒤에만 호출됨
        //Debug.Log("OnUserDataChangedHandler 함수 실행");
        RefreshType(DonutType.Hard, hardShells);
        hardPanel.SetActive(true);
        softPanel.SetActive(false);
        moistPanel.SetActive(false);
    }
    
    void Awake()
    {
        hardPanel.SetActive(true);
        softPanel.SetActive(false);
        moistPanel.SetActive(false);
        
        hardButton.onClick.AddListener(RefreshHard);
        softButton.onClick.AddListener(RefreshSoft); 
        moistButton.onClick.AddListener(RefreshMoist);
        
        closeButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.MainPanel));
        
        
        // 도감 설명창
        infoButton.onClick.AddListener(() => infoPanel.SetActive(true));
        infoPopupCloseButton.onClick.AddListener(() => infoPanel.SetActive(false));
    }
    
    // 단단 버튼
    private void RefreshHard()
    {
        RefreshType(DonutType.Hard, hardShells);
        OnPanel(DonutType.Hard);
        
        image.sprite = baseHardSprite;
        if (LocalizationManager.Instance.CurrentLanguage == "ko")
        {
            titleText.text = "단단 도넛";
            levelText.text = "1단계";
            infotext.text = "단단 도넛에 대한 설명이에요 ( + 값 관련 설명)";
        }
        else
        {
            titleText.text = "Hard";
            levelText.text = "Level 1";
            infotext.text = "This is a description of the Hard Donut (+ value details)";
        }
        
        // 전에 클릭된 도넛 배경 비활성화
        if (curDonutShell != null)
        {
            curDonutShell.donutRoot.GetComponent<Image>().sprite = curDonutShell.baseSprite;
            curDonutShell = null;
        }

    }

    // 촉촉 버튼
    private void RefreshSoft()
    {
        RefreshType(DonutType.Soft, softShells);
        OnPanel(DonutType.Soft);
        image.sprite = baseSoftSprite;
        if (LocalizationManager.Instance.CurrentLanguage == "ko")
        {
            titleText.text = "말랑 도넛";
            levelText.text = "1단계";
            infotext.text = "말랑 도넛에 대한 설명이에요 ( + 값 관련 설명)";
        }
        else
        {
            titleText.text = "Soft";
            levelText.text = "Level 1";
            infotext.text = "This is a description of the Soft Donut (+ value details)";
        }
        
        // 전에 클릭된 도넛 배경 비활성화
        if (curDonutShell != null)
        {
            curDonutShell.donutRoot.GetComponent<Image>().sprite = curDonutShell.baseSprite;
            curDonutShell = null;
        }
    }
    
    // 말랑 버튼
    private void RefreshMoist()
    {
        RefreshType(DonutType.Moist, moistShells);
        OnPanel(DonutType.Moist);
        image.sprite = baseMoistSprite;
        if (LocalizationManager.Instance.CurrentLanguage == "ko")
        {
            titleText.text = "촉촉 도넛";
            levelText.text = "1단계";
            infotext.text = "촉촉 도넛에 대한 설명이에요 ( + 값 관련 설명)";
        }
        else
        {
            titleText.text = "Moist";
            levelText.text = "Level 1";
            infotext.text = "This is a description of the Moist Donut (+ value details)";
        }
        
        // 전에 클릭된 도넛 배경 비활성화
        if (curDonutShell != null)
        {
            curDonutShell.donutRoot.GetComponent<Image>().sprite = curDonutShell.baseSprite;
            curDonutShell = null;
        }
    }
    
    private void OnPanel(DonutType donutType)
    {
        hardPanel.SetActive(donutType == DonutType.Hard);
        softPanel.SetActive(donutType == DonutType.Soft);
        moistPanel.SetActive(donutType == DonutType.Moist);
        
        SetAlpha(hardImage, donutType == DonutType.Hard);
        SetAlpha(softImage, donutType == DonutType.Soft);
        SetAlpha(moistImage, donutType == DonutType.Moist);
    }

    private void SetAlpha(Image img, bool active)
    {
        Color c = img.color;
        c.a = active ? 1f : 0f;
        img.color = c;
    }
    
    /// <summary>
    /// codex + GetDonutByID를 기반으로 슬롯 세팅
    /// </summary>
    private void RefreshType(DonutType donuttype, List<Scr_donutShell> shellList)
    {
        for (int i = 0; i < shellList.Count; i++)
        {
            DonutDexViewState state = DonutDexViewState.Question;
            switch (donuttype)
            {
                case DonutType.Hard:
                    state = DataManager.Instance.InventoryData.hardDonutCodexDataList[i].donutDexViewState;
                    break;
                case DonutType.Soft:
                    state = DataManager.Instance.InventoryData.softDonutCodexDataList[i].donutDexViewState;
                    break;
                case DonutType.Moist:
                    state = DataManager.Instance.InventoryData.moistDnutCodexDataList[i].donutDexViewState;
                    break;
            }
            
            DonutData donutData = DataManager.Instance.GetDonutData(donuttype, i + 1);
            
            switch (state)
            {
                case DonutDexViewState.Question:
                    shellList[i].SetType(state);
                    shellList[i].SetDonut(donutData.donutType, donutData.level);
                    break;
                case DonutDexViewState.Donut:
                    shellList[i].SetType(state, donutData.sprite);
                    shellList[i].SetDonut(donutData.donutType, donutData.level);
                    break;
                case DonutDexViewState.Reward:
                    shellList[i].SetType(state, reward: donutData.rewardGem);
                    shellList[i].SetDonut(donutData.donutType, donutData.level);
                    break;
            }

           //Debug.Log($"DonutType: {donuttype}, DonutData: {donutData}");
        }
    }

    // 정보창 셋팅
    public void InfoSet(Scr_donutShell shell)
    {
        if (curDonutShell == shell)
            return;
        
        DonutData donutData = DataManager.Instance.GetDonutData(shell.donutType, shell.level);
        image.sprite = donutData.sprite;
        shell.donutRoot.GetComponent<Image>().sprite = shell.activeSprite;
        // 나중에 영어 버전 처리 필요
        if (LocalizationManager.Instance.CurrentLanguage == "ko")
        {
            infotext.text = donutData.description;
            levelText.text = $"{donutData.level} 단계";
        }
        else
        {
            infotext.text = donutData.descriptionEnglish;
            levelText.text = $"Level {donutData.level}";
        }
        
        // 전에 클릭된 도넛 배경 비활성화
        if (curDonutShell != null)
        {
            curDonutShell.donutRoot.GetComponent<Image>().sprite = curDonutShell.baseSprite;
        }
        curDonutShell = shell;
        
    }
    
#if UNITY_EDITOR

    [CustomEditor(typeof(Scr_DonutCodex))]
    public class Scr_DonutCodexEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Scr_DonutCodex codex = (Scr_DonutCodex)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("=== Codex Editor Tools ===", EditorStyles.boldLabel);

            if (GUILayout.Button("단단(Hard) Shell 자동 채우기"))
            {
                codex.SetHardShellsFromRoot();
            }

            if (GUILayout.Button("말랑(Soft) Shell 자동 채우기"))
            {
                codex.SetSoftShellsFromRoot();
            }

            if (GUILayout.Button("촉촉(Moist) Shell 자동 채우기"))
            {
                codex.SetMoistShellsFromRoot();
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
    
    public void SetHardShellsFromRoot()
    {
        if (hardPanel == null) return;

        hardShells = new List<Scr_donutShell>(
            hardPanel.GetComponentsInChildren<Scr_donutShell>(true)
        );
    }

    public void SetSoftShellsFromRoot()
    {
        if (softPanel == null) return;

        softShells = new List<Scr_donutShell>(
            softPanel.GetComponentsInChildren<Scr_donutShell>(true)
        );
    }

    public void SetMoistShellsFromRoot()
    {
        if (moistPanel == null) return;

        moistShells = new List<Scr_donutShell>(
            moistPanel.GetComponentsInChildren<Scr_donutShell>(true)
        );
    }
#endif
}