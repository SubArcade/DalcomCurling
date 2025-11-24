using UnityEngine;
using UnityEngine.UI;

public class TestLevelUpButton : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClickLevelUpTest);
    }

    private void OnClickLevelUpTest()
    {
        var player = DataManager.Instance.PlayerData;

        player.level++;
        BoardManager.Instance.SpawnGiftBox(); // 레벨업 시 기프트박스
        Debug.Log($"🟢 [TEST] 플레이어 레벨이 {player.level}로 상승!");

        // 보드 해금 반영
        BoardManager.Instance.RefreshBoardUnlock();
    }
}
