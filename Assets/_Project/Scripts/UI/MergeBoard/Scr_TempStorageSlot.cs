using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempStorageSlot : MonoBehaviour
{
    [SerializeField] private const int MAX_STACK = 7;

    private Queue<DonutData> storage = new Queue<DonutData>();

    [SerializeField] private Button tempButton;
    public Image tempIcon; // 보관칸에 있을떄 출력할 이미지

    public Image tempTextBox; // 보관칸 텍스트박스
    public Text countText;

    void Start()
    {
        tempButton = GetComponent<Button>();
        if (tempButton != null) tempButton.onClick.AddListener(OnClick);
        tempButton.gameObject.SetActive(false);
    }

    // 임시보관칸에 넣기
    public bool Add(DonutData data)
    {
        if (storage.Count >= MAX_STACK)
        {
            Debug.Log("임시보관칸이 가득 찼습니다.");
            return false;
        }

        storage.Enqueue(data);
        RefreshUI();
        Debug.Log($"storage.Count : {storage.Count}");
        return true;
    }

    // 꺼내기
    public DonutData Pop()
    {
        if (storage.Count == 0)
            return null;
        // TODO : Dequeue 하기전에 보드판 빈칸있으면 리턴 

        var item = storage.Dequeue();
        RefreshUI();
        return item;
    }

    private void RefreshUI()
    {
        if (storage.Count == 0)
        {
            tempButton.gameObject.SetActive(false);
            countText.text = "";
        }
        else
        {
            tempButton.gameObject.SetActive(true);
            tempIcon.sprite = storage.Peek().sprite;
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
