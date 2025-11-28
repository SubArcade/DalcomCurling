using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // List를 사용하기 위해 추가

/// <summary>
/// 인벤토리의 개별 도넛 슬롯 UI를 제어하는 스크립트입니다.
/// </summary>
[RequireComponent(typeof(CanvasGroup))] // CanvasGroup이 항상 있도록 보장
public class DonutEntryUI : MonoBehaviour
{
    [SerializeField] private Image donutImage; // 도넛 이미지를 표시할 Image 컴포넌트
    [SerializeField] private GameObject selectionHighlight; // 선택되었을 때 표시될 하이라이트 오브젝트

    private CanvasGroup _canvasGroup; // UI의 상호작용 및 투명도 제어를 위한 컴포넌트
    private DonutEntry _donutData; // 이 UI 슬롯이 담고 있는 도넛 데이터
    private Action<DonutEntry> _onClickAction; // 클릭 시 호출될 콜백
    private bool _isUsed = false; // 이 슬롯이 사용되었는지 여부

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// 도넛 데이터로 이 UI 슬롯을 초기화합니다.
    /// </summary>
    public void Setup(DonutEntry data, Action<DonutEntry> onClickAction)
    {
        _donutData = data;
        _onClickAction = onClickAction;
        _isUsed = false; // 새로 설정될 때는 항상 사용되지 않은 상태로 초기화

        // --- FIX: DataManager.Instance가 초기화되었는지 확인 ---
        if (DataManager.Instance == null)
        {
            Debug.LogError($"DonutEntryUI.Setup: DataManager.Instance가 null입니다. ID: {data?.id}. 스크립트 실행 순서(Script Execution Order) 문제일 수 있습니다.");
            if (donutImage != null) donutImage.sprite = null;
            return; // NullReferenceException을 방지하기 위해 여기서 중단
        }
        // --- End of FIX ---

        // DataManager를 통해 도넛의 원본 데이터를 가져옵니다.
        // DonutData는 DonutTypeSO.cs에 정의되어 있습니다.
        DonutData originalDonutData = DataManager.Instance.GetDonutByID(data.id);
        if (originalDonutData != null)
        {
            if (donutImage != null)
            {
                donutImage.sprite = originalDonutData.sprite;
            }
            else
            {
                Debug.LogWarning($"DonutEntryUI: donutImage가 할당되지 않았습니다. ID: {data.id}");
            }
        }
        else
        {
            Debug.LogWarning($"DonutEntryUI: ID({data.id})에 해당하는 도넛 데이터를 찾을 수 없습니다.");
            if (donutImage != null) donutImage.sprite = null; // 데이터 없으면 이미지 비움
        }

        SetUsed(false); // UI 상태 초기화
        SetSelected(false); // 기본적으로는 선택되지 않은 상태로 시작
    }

    /// <summary>
    /// 이 UI 슬롯이 클릭되었을 때 호출됩니다. (UI Button 컴포넌트와 연결)
    /// </summary>
    public void OnClick()
    {
        if (_isUsed) return; // 사용된 슬롯은 클릭 불가
        _onClickAction?.Invoke(_donutData);
    }

    /// <summary>
    /// 이 UI 슬롯의 선택 상태를 설정합니다.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(isSelected);
        }
    }

    /// <summary>
    /// 이 슬롯을 '사용됨' 상태로 설정하고 시각적으로 비활성화합니다.
    /// </summary>
    public void SetUsed(bool used)
    {
        _isUsed = used;
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = _isUsed ? 0.5f : 1.0f;
            _canvasGroup.interactable = !_isUsed;
        }
    }

    /// <summary>
    /// 이 슬롯에 할당된 도넛 엔트리 데이터를 반환합니다.
    /// </summary>
    public DonutEntry GetDonutEntry()
    {
        return _donutData;
    }

    /// <summary>
    /// 이 슬롯이 현재 사용된 상태인지 반환합니다.
    /// </summary>
    public bool IsUsed()
    {
        return _isUsed;
    }
}
