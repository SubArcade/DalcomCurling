using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_DonutStateEditor : MonoBehaviour
{
    [SerializeField] private Image topDonutImage;
    [SerializeField] private TMP_Text donutLevelText;
    [SerializeField] private TMP_Text donutTypeText;
    [SerializeField] private TMP_Text donutPowerupValueText;
    [SerializeField] private TMP_Text donutMaxValueText;
    [SerializeField] private Slider donutSlider;
    [SerializeField] private List<Scr_DonutSlot> slotUIs;
    [SerializeField] private GameObject blur;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button backButton;
    [SerializeField] private Button marketButton;

    private int currentSelectedSlot = -1;
    private int dragStartSlot = -1; // 드래그 시작 슬롯 (1~5)

    private void OnEnable()
    {
        // UIManager.Instance.OnReadyPanelOpen += () => Open(true);
        Open(true);
        for (int i = 0; i < slotUIs.Count; i++)
        {
            int slotIndex = i + 1; // 1~5
            slotUIs[i].Init(this, slotIndex);
            RefreshSlot(slotIndex);
            slotUIs[i].SetSelected(false);
        }

        // 기본 선택: 1번
        SelectSlot(1);
    }

    private void OnDisable()
    {
        Open(false);
        // UIManager.Instance.OnReadyPanelOpen -= () => Open(true); // 이벤트 쓰면 이쪽에서 해제
    }
    
    private void Open(bool isActive)
    {
        if (blur != null) blur.SetActive(isActive);
        if (mainMenu != null) mainMenu.SetActive(!isActive);
    }
    
    private void Awake()
    {
        donutSlider.onValueChanged.AddListener(OnSliderChanged);
        backButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.MainPanel));
        // marketButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.MainPanel));
    }

    private void Start()
    {
        // 최소 5칸 보장 + 기본값 세팅 (이미 함수 있음)
        //DataManager.Instance.EnsureDonutSlots();

        // 슬롯 초기 이미지 로드
        /*for (int i = 0; i < slotUIs.Count; i++)
        {
            int slotIndex = i + 1; // 1~5
            slotUIs[i].Init(this, slotIndex);
            RefreshSlot(slotIndex);
            slotUIs[i].SetSelected(false);
        }

        // 기본 선택: 1번
        SelectSlot(1);*/
    }

    /// <summary>
    /// 아래 슬롯 "클릭" 시 호출 (선택만)
    /// </summary>
    public void OnClickSlot(int slotIndex)
    {
        SelectSlot(slotIndex);
    }

    /// <summary>
    /// 드래그 시작 슬롯 기록
    /// </summary>
    public void BeginDrag(int slotIndex)
    {
        dragStartSlot = slotIndex;
    }

    /// <summary>
    /// 드래그 취소
    /// </summary>
    public void CancelDrag()
    {
        dragStartSlot = -1;
    }

    /// <summary>
    /// 드래그 종료 시, 드롭된 슬롯과 스왑 처리
    /// </summary>
    public void EndDrag(int dropSlotIndex)
    {
        if (dragStartSlot < 1 || dropSlotIndex < 1)
        {
            dragStartSlot = -1;
            return;
        }

        if (dragStartSlot != dropSlotIndex)
        {
            SwapSlot(dragStartSlot, dropSlotIndex);
        }

        dragStartSlot = -1;
    }

    /// <summary>
    /// 아래 슬롯에서 도넛 데이터를 읽어와 UI에 반영
    /// </summary>
    private void RefreshSlot(int slotIndex)
    {
        int listIndex = slotIndex - 1;

        var entryList = DataManager.Instance.InventoryData.donutEntries;
        DonutEntry entry = (entryList != null && listIndex < entryList.Count)
            ? entryList[listIndex]
            : null;
    
        //Debug.Log(entry.id);    
        // 도넛 amount가 0일경우 처음 앤트리에 넣은값
        if (entry.donutAmount == 0)
        {
            DataManager.Instance.InventoryData.donutEntries[listIndex].donutAmount = 1;
        }
        
        if (entry.level == 0)
        {
            string[] parts = entry.id.Split('_');
            string last = parts[parts.Length - 1];
            if (int.TryParse(last, out int parsedLevel)) 
                entry.level = parsedLevel;
            DataManager.Instance.InventoryData.donutEntries[listIndex].level = parsedLevel;
        }

        Sprite sprite = GetSpriteForEntry(entry);

        slotUIs[listIndex].SetSprite(sprite);
    }

    /// <summary>
    /// 슬롯 선택 시 위쪽 이미지/텍스트 갱신 + Outline 처리
    /// 선택된 슬롯 활성화
    /// </summary>
    private void SelectSlot(int slotIndex)
    {
        currentSelectedSlot = slotIndex;

        int listIndex = slotIndex - 1;
        var entries = DataManager.Instance.InventoryData.donutEntries;
        DonutEntry entry = (entries != null && listIndex < entries.Count)
            ? entries[listIndex]
            : null;
        
        Sprite sprite = GetSpriteForEntry(entry);
        topDonutImage.sprite  = sprite;
        topDonutImage.enabled = (sprite != null);

        // 슬라이더/ +숫자 초기화
        donutSlider.SetValueWithoutNotify(1);
        donutPowerupValueText.text = "+ 1";
        donutSlider.minValue = 1;
        
        if (entry != null)
        {
            donutTypeText.text  = entry.type.ToString();
            donutLevelText.text = $"LV.{entry.level.ToString()}";
            donutSlider.maxValue = entry.level;
            donutMaxValueText.text = entry.level.ToString();
        }
        else
        {
            donutTypeText.text  = "-";
            donutLevelText.text = "LV.0";
            donutSlider.maxValue = 1f;
            donutMaxValueText.text = "1";
        }
        
        // 아래 선택 Outline 토글
        for (int i = 0; i < slotUIs.Count; i++)
        {
            int sIndex = i + 1;
            slotUIs[i].SetSelected(sIndex == slotIndex);
        }
    }

    /// <summary>
    /// slotA 와 slotB의 도넛을 서로 교체 (데이터 + 이미지 동시)
    /// </summary>
    private void SwapSlot(int slotA, int slotB)
    {
        var list = DataManager.Instance.InventoryData.donutEntries;
        if (list == null) return;

        int idxA = slotA - 1;
        int idxB = slotB - 1;

        if (idxA < 0 || idxB < 0 || idxA >= list.Count || idxB >= list.Count)
        {
            Debug.LogError($"[Scr_DonutStateEditor] 잘못된 스왑 인덱스: {slotA}, {slotB}");
            return;
        }

        // 데이터 스왑
        DonutEntry temp = list[idxA];
        list[idxA] = list[idxB];
        list[idxB] = temp;

        // UI 스왑(각 슬롯 새로고침)
        RefreshSlot(slotA);
        RefreshSlot(slotB);

        // 현재 선택은 드롭된 쪽으로
        SelectSlot(slotB);
    }
    
    // 슬라이더 값 변경
    private void OnSliderChanged(float value)
    {
        int delta = Mathf.RoundToInt(value);
        donutPowerupValueText.text = $"+ {delta}";

        // 선택된 도넛 레벨 미리보기
        if (currentSelectedSlot < 1) return;

        int listIndex = currentSelectedSlot - 1;
        var entries = DataManager.Instance.InventoryData.donutEntries;
        if (entries == null || listIndex >= entries.Count) return;

        DonutEntry entry = entries[listIndex];
        if (entry == null) return;
        
        int previewLevel = entry.weight + delta;
        
        DataManager.Instance.InventoryData.donutEntries[listIndex].donutAmount = delta;
    }

    /// <summary>
    /// 실제로 수치를 적용(저장)하고 싶을 때 버튼에 연결해서 사용
    /// </summary>
    public void ApplySliderValue()
    {
        if (currentSelectedSlot < 1) return;

        int delta = Mathf.RoundToInt(donutSlider.value);
        if (delta == 0) return;

        int listIndex = currentSelectedSlot - 1;
        var entries = DataManager.Instance.InventoryData.donutEntries;
        if (entries == null || listIndex >= entries.Count) return;

        DonutEntry entry = entries[listIndex];
        if (entry == null) return;

        entry.weight += delta;

        // UI 갱신
        donutSlider.SetValueWithoutNotify(0);
        donutPowerupValueText.text = "+ 0";
        donutLevelText.text = entry.level.ToString();

        RefreshSlot(currentSelectedSlot);
    }

    /// <summary>
    /// DonutEntry에서 SO를 통해 Sprite 가져오기
    /// </summary>
    private Sprite GetSpriteForEntry(DonutEntry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.id))
            return null;

        // id 예: "Hard_3" → level = 3
        int level = int.Parse(entry.id.Split('_')[1]);

        DonutData data = DataManager.Instance.GetDonutData(entry.type, level);
        return data != null ? data.sprite : null;
    }
}
