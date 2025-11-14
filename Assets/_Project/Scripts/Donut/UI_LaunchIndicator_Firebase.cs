using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_LaunchIndicator_Firebase : MonoBehaviour
{
    //UI의 표시(활성화) 여부를 제어하는 스크립트
    // TODO : 미리보기 궤적이 완성되면 기존에 있던 조작 UI는 삭제해야함
    
    [Header("References")]
    // 발사 로직 스크립트를 드래그 앤 드롭으로 연결
    public StoneShoot_Firebase launchScript;

    // UI Slider 컴포넌트를 드래그 앤 드롭으로 연결
    [Header("UI오브젝트들")]
    public TextMeshProUGUI aScoreText;
    public TextMeshProUGUI bScoreText;
    public TextMeshProUGUI roundText;
    // 현재 턴 번호를 표시하는 TextMeshProUGUI 객체
    public TextMeshProUGUI turnText;
    public GameObject CountDownText;
    [Header("Debug")]
    public TextMeshProUGUI debugStateText;
    void Update()
    {
        if (launchScript == null) return;

        var currentState = launchScript.CurrentState;
        var currentDragType = launchScript.CurrentDragType;

        // 조준 상태일 때만 조작 관련 UI를 업데이트합니다.
        if (currentState == StoneShoot_Firebase.LaunchState.Aiming)
        {

        }

        // 디버그용 상태 텍스트 업데이트
        if (debugStateText != null && FirebaseGameManager.Instance != null)
        {
            string localState = FirebaseGameManager.Instance.CurrentLocalState;
            string gameState = FirebaseGameManager.Instance.CurrentGameState;
            debugStateText.text = $"Local State: {localState}\nGame State: {gameState}";
        }
    }


    public void RoundChanged(int round, int aScore, int bScore)
    {
        aScoreText.text = $"{aScore}";
        bScoreText.text = $"{bScore}";
        roundText.text = $"Round : {round}";
    }

    /// <summary>
    /// 현재 턴 번호를 UI에 업데이트합니다.
    /// </summary>
    /// <param name="turn">현재 턴 번호.</param>
    public void UpdateTurnDisplay(int turn)
    {
        if (turnText != null)
        {
            int displayTurn = (turn / 2) + 1; // 내부 턴 번호를 컬링 규칙에 맞게 가공
            turnText.text = $"Turn : {displayTurn}";
        }
    }


    /// <summary>
    /// 카운트 다운 UI의 활성화 여부를 설정합니다.
    /// </summary>
    public void SetCountDown(bool IsturnWaitingForInput)
    {
        if (IsturnWaitingForInput)
        {
            CountDownText.SetActive(true);
        }
        else
        {
            CountDownText.SetActive(false);
        }
    }
}