using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어의 도넛 인벤토리 엔트리 UI를 관리하고 선택을 처리하는 스크립트입니다.
/// </summary>
public class DonutSelectionUI : MonoBehaviour
{
    // 도넛 선택이 변경될 때 발생하는 이벤트
    public event System.Action<DonutEntry> OnDonutSelectionChanged;

    [SerializeField] private List<DonutEntryUI> donutSlots; // 인스펙터에서 할당할 도넛 슬롯 UI 요소들

    private DonutEntry _selectedDonutEntry; // 현재 선택된 도넛 엔트리 데이터
    private int _selectedIndex = -1; // 현재 선택된 도넛 슬롯의 인덱스

    /// <summary>
    /// 플레이어의 인벤토리 엔트리 리스트로 UI 슬롯들을 채웁니다.
    /// </summary>
    /// <param name="entries">플레이어의 도넛 엔트리 리스트.</param>
    public void Populate(List<DonutEntry> entries)
    {
        if (entries == null) entries = new List<DonutEntry>();

        for (int i = 0; i < donutSlots.Count; i++)
        {
            if (i < entries.Count)
            {
                donutSlots[i].gameObject.SetActive(true);
                donutSlots[i].Setup(entries[i], OnDonutClicked);
            }
            else
            {
                donutSlots[i].gameObject.SetActive(false); // 남는 슬롯은 비활성화
            }
        }

        // 사용 가능한 첫 번째 도넛을 자동으로 선택합니다.
        SelectNextAvailableDonut(-1);
    }

    /// <summary>
    /// 개별 도넛 슬롯 UI가 클릭되었을 때 호출되는 콜백입니다.
    /// </summary>
    /// <param name="clickedDonut">클릭된 도넛의 데이터.</param>
    private void OnDonutClicked(DonutEntry clickedDonut)
    {
        for (int i = 0; i < donutSlots.Count; i++)
        {
            // 활성화되어 있고, 사용되지 않았으며, 데이터가 일치하는 슬롯을 찾습니다.
            if (donutSlots[i].gameObject.activeSelf && !donutSlots[i].IsUsed() && donutSlots[i].GetDonutEntry() == clickedDonut)
            {
                SetSelectedDonut(i);
                break;
            }
        }
    }

    /// <summary>
    /// 특정 인덱스의 도넛 슬롯을 선택 상태로 설정합니다.
    /// </summary>
    /// <param name="index">선택할 슬롯의 인덱스.</param>
    public void SetSelectedDonut(int index)
    {
        // 유효성 검사: 인덱스가 범위를 벗어나거나, 슬롯이 비활성화되었거나, 이미 사용된 경우 선택하지 않습니다.
        if (index < 0 || index >= donutSlots.Count || !donutSlots[index].gameObject.activeSelf || donutSlots[index].IsUsed())
        {
            // Debug.LogWarning($"DonutSelectionUI: 유효하지 않거나 사용할 수 없는 도넛 슬롯 인덱스 {index} 입니다.");
            return;
        }
        
        // 선택이 실제로 변경되었는지 확인
        if (_selectedIndex == index)
        {
            return; // 이미 선택된 슬롯이면 아무것도 하지 않음
        }

        // 이전에 선택된 슬롯이 있다면 선택 해제
        if (_selectedIndex != -1 && _selectedIndex < donutSlots.Count)
        {
            donutSlots[_selectedIndex].SetSelected(false);
        }

        // 새 슬롯 선택
        _selectedIndex = index;
        donutSlots[_selectedIndex].SetSelected(true);
        _selectedDonutEntry = donutSlots[_selectedIndex].GetDonutEntry();

        //Debug.Log($"DonutSelectionUI: 도넛 슬롯 {index} 선택됨. ID: {_selectedDonutEntry?.id}");
        
        // 선택 변경 이벤트를 호출합니다.
        OnDonutSelectionChanged?.Invoke(_selectedDonutEntry);
    }

    /// <summary>
    /// 현재 선택된 도넛 엔트리 데이터를 반환합니다.
    /// </summary>
    public DonutEntry GetSelectedDonut()
    {
        return _selectedDonutEntry;
    }
    
    /// <summary>
    /// 특정 도넛을 '사용됨'으로 표시하고, 다음 사용 가능한 도넛을 자동으로 선택합니다.
    /// </summary>
    /// <param name="usedEntry">사용된 도넛 엔트리 데이터.</param>
    public void MarkDonutAsUsed(DonutEntry usedEntry)
    {
        if (usedEntry == null) return;

        for (int i = 0; i < donutSlots.Count; i++)
        {
            if (donutSlots[i].gameObject.activeSelf && donutSlots[i].GetDonutEntry()?.id == usedEntry.id)
            {
                donutSlots[i].SetUsed(true);
                donutSlots[i].SetSelected(false); // 사용되었으므로 선택 해제
                //Debug.Log($"DonutSelectionUI: 도넛 {usedEntry.id} 사용됨으로 표시.");

                // 현재 선택된 도넛이 방금 사용한 도넛이었다면, 다음 도넛을 선택합니다.
                if (_selectedIndex == i)
                {
                    SelectNextAvailableDonut(i);
                }
                break;
            }
        }
    }

    /// <summary>
    /// 지정된 인덱스 다음부터 사용 가능한 첫 번째 도넛을 찾아 선택합니다.
    /// </summary>
    /// <param name="startIndex">검색을 시작할 인덱스.</param>
    private void SelectNextAvailableDonut(int startIndex)
    {
        // startIndex 다음부터 끝까지 검색
        for (int i = startIndex + 1; i < donutSlots.Count; i++)
        {
            if (donutSlots[i].gameObject.activeSelf && !donutSlots[i].IsUsed())
            {
                SetSelectedDonut(i);
                return;
            }
        }

        // 처음부터 startIndex까지 검색 (순환)
        for (int i = 0; i <= startIndex; i++)
        {
            if (donutSlots[i].gameObject.activeSelf && !donutSlots[i].IsUsed())
            {
                SetSelectedDonut(i);
                return;
            }
        }

        // 사용 가능한 도넛이 하나도 없는 경우
        Debug.Log("DonutSelectionUI: 더 이상 사용 가능한 도넛이 없습니다.");
        _selectedIndex = -1;
        _selectedDonutEntry = null;
    }

    /// <summary>
    /// 모든 도넛 슬롯의 '사용됨' 상태를 초기화하고, 첫 번째 사용 가능한 도넛을 다시 선택합니다.
    /// 라운드가 끝날 때 호출됩니다.
    /// </summary>
    public void ResetDonutUsage()
    {
        foreach (var slot in donutSlots)
        {
            if (slot.gameObject.activeSelf)
            {
                slot.SetUsed(false);
            }
        }
        // 모든 도넛이 사용 가능해졌으므로, 첫 번째 사용 가능한 도넛을 다시 선택합니다.
        SelectNextAvailableDonut(-1);
        //Debug.Log("DonutSelectionUI: 도넛 사용 상태가 초기화되었습니다.");
    }
}
