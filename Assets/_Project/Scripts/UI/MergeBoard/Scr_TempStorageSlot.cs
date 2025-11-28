using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TempStorageSlot : MonoBehaviour
{
    [SerializeField] private const int MAX_STACK = 7;

    private Queue<GiftBoxData> storage = new Queue<GiftBoxData>();

    [SerializeField] private Button tempButton;
    public Image tempIcon; // 보관칸에 있을떄 출력할 이미지

    public Image tempTextBox; // 보관칸 텍스트박스
    public TMP_Text countText;

    void Start()
    {
        tempButton = GetComponent<Button>();
        if (tempButton != null) tempButton.onClick.AddListener(OnClick);
        //tempButton.gameObject.SetActive(false);

        DataManager.Instance.OnBoardDataLoaded += HandleBoardLoaded;
    }

    // 데이터 대기
    private void HandleBoardLoaded()
    {
        LoadFromBoardData();
        RefreshUI();
    }

    // 기프트박스 임시보관칸에 넣기
    public bool Add(GiftBoxData data)
    {
        if (storage.Count >= MAX_STACK)
        {
            Debug.Log("임시보관칸이 가득 찼습니다.");
            return false;
        }

        storage.Enqueue(data);
        SaveToBoardData();
        RefreshUI();
        Debug.Log($"임시보관칸 : {storage.Count}");
        return true;
    }

    // 도넛 임시보관칸에 넣기 
    public bool Add(DonutData data)
    {
        if (storage.Count >= MAX_STACK)
            return false;

        // DonutData → GiftBoxData 변환 후 저장
        GiftBoxData fakeGift = new GiftBoxData()
        {
            id = data.id,
            sprite = data.sprite,
            level = data.level,
            donutType = data.donutType
        };

        storage.Enqueue(fakeGift);
        SaveToBoardData();
        RefreshUI();
        return true;
    }

    // 꺼내기
    public GiftBoxData Pop()
    {
        if (storage.Count == 0)
            return null;

        var emptyCell = BoardManager.Instance.FindEmptyActiveCell();

        if (emptyCell == null) return null;

        var item = storage.Dequeue();
        SaveToBoardData();
        RefreshUI();
        return item;
    }

    private void SaveToBoardData()
    {
        var board = DataManager.Instance.MergeBoardData;

        board.tempGiftIds.Clear();
        foreach (var item in storage)
            board.tempGiftIds.Add(item.id);   // "Gift_1"
    }

    private void LoadFromBoardData()
    {
        var board = DataManager.Instance.MergeBoardData;

        storage.Clear();
        foreach (var id in board.tempGiftIds)
        {
            var gift = DataManager.Instance.GetGiftBoxDataByID(id);
            if (gift != null)
            {
                storage.Enqueue(gift);
            }
            else
            {
                // 저 id가 실제 도넛일 수도 있다면?
                DonutData donut = DataManager.Instance.GetDonutByID(id);
                if (donut != null)
                {
                    GiftBoxData fake = new GiftBoxData()
                    {
                        id = donut.id,
                        sprite = donut.sprite,
                        level = donut.level,
                        donutType = donut.donutType
                    };
                    storage.Enqueue(fake);
                }
            }
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (storage.Count == 0)
        {
            tempButton.gameObject.SetActive(false);
            tempTextBox.gameObject.SetActive(false);
            tempIcon.sprite = null;
            countText.text = "";
            return;
        }

        tempButton.gameObject.SetActive(true);
        var first = storage.Peek();
        tempIcon.sprite = first.sprite;

        if (storage.Count == 1)
        {
            tempButton.gameObject.SetActive(true);
            tempTextBox.gameObject.SetActive(false);
        }
        if (storage.Count > 1)
        {
            tempButton.gameObject.SetActive(true);
            tempTextBox.gameObject.SetActive(true);
            countText.text = storage.Count.ToString();
        }
    }

    // 임시칸 클릭 → 하나 생성
    public void OnClick()
    {
        var data = Pop();
        if (data == null) return;

        BoardManager.Instance.SpawnFromTempStorage(data);
    }

    private void OnDisable()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.OnBoardDataLoaded -= HandleBoardLoaded;
    }

}
