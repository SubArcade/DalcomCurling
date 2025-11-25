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
        tempButton.gameObject.SetActive(false);

        DataManager.Instance.OnBoardDataLoaded += HandleBoardLoaded;
    }

    // 데이터 대기
    private void HandleBoardLoaded()
    {
        LoadFromBoardData();
        RefreshUI();
    }

    // 임시보관칸에 넣기
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
        Debug.Log($"storage.Count : {storage.Count}");
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
            if (gift == null) continue;

            // 여기서는 GiftBoxData 그대로 큐에 넣으면 됨
            storage.Enqueue(gift);
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (storage.Count == 0)
        {
            tempButton.gameObject.SetActive(false);
            tempTextBox.gameObject.SetActive(false);
        }
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
}
