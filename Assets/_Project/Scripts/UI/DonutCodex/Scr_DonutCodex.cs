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
    
    [Header("판넬")]
    [SerializeField, Tooltip("말랑")] private GameObject hardPanel; 
    [SerializeField, Tooltip("단단")] private GameObject softPanel; 
    [SerializeField, Tooltip("촉촉")] private GameObject moistPanel; 
    
    [Header("버튼 이미지")]
    [SerializeField, Tooltip("단단")] private Image hardImage;
    [SerializeField, Tooltip("단단")] private Image softImage;
    [SerializeField, Tooltip("단단")] private Image moistImage;
    
    [Header("닫기 버튼")]
    [SerializeField, Tooltip("닫기")] private Button closeButton;
    
    [Header("도넛 정보창")]
    [SerializeField, Tooltip("이미지")] private Image image;
    [SerializeField, Tooltip("설명")] private TMP_Text infotext;
    [SerializeField, Tooltip("레벨")] private TMP_Text levelText;


    private void OnEnable()
    {
        OnUserDataChangedHandler();
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
    }
    
    // 단단 버튼
    private void RefreshHard()
    {
        RefreshType(DonutType.Hard, hardShells);
        OnPanel(DonutType.Hard);
    }

    // 촉촉 버튼
    private void RefreshSoft()
    {
        RefreshType(DonutType.Soft, softShells);
        OnPanel(DonutType.Soft);
    }

    
    // 말랑 버튼
    private void RefreshMoist()
    {
        RefreshType(DonutType.Moist, moistShells);
        OnPanel(DonutType.Moist);
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
    public void InfoSet(Sprite sprite, string info, string level)
    {  
        image.sprite = sprite;
        infotext.text = info;
        // 나중에 영어 버전 처리 필요
        levelText.text = $"{level} 단계";
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