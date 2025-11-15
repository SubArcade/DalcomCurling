using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_LaunchIndicator_Firebase : MonoBehaviour
{
    //UI의 표시(활성화) 여부를 제어하는 스크립트
    
    [Header("References")]
    // 발사 로직 스크립트를 드래그 앤 드롭으로 연결
    public StoneShoot_Firebase launchScript;

    // UI Slider 컴포넌트를 드래그 앤 드롭으로 연결
    [Header("RoundPanel 노출변수")]
    public TextMeshProUGUI aScoreText;
    public TextMeshProUGUI bScoreText;
    public TextMeshProUGUI roundText;
    // 현재 턴 번호를 표시하는 TextMeshProUGUI 객체
    public TextMeshProUGUI turnText;
    public GameObject CountDownText;
    [Header("Debug")]
    public TextMeshProUGUI debugStateText;
    [Header("UI 제어 객체 (on/off)")]
    [SerializeField] private GameObject roundPanel;
    [SerializeField] private GameObject donutEntry;
    [SerializeField] private GameObject minimap;
    [SerializeField] private GameObject result;

    //내부변수
    private int displayTurn = 0;

    // 현재 플레이어와 상대방의 프로필 정보
    public PlayerProfile MyProfile { get; private set; }
    public PlayerProfile OpponentProfile { get; private set; }

    void Start()
    {
        // FirebaseGameManager가 프로필 로딩을 완료하면 HandleProfilesLoaded를 호출하도록 이벤트를 구독합니다.
        if (FirebaseGameManager.Instance != null)
        {
            FirebaseGameManager.Instance.OnProfilesLoaded += HandleProfilesLoaded;
        }
    }

    private void HandleProfilesLoaded()
    {
        // 이벤트 수신 후 즉시 구독을 해제하여 중복 호출을 방지합니다.
        if (FirebaseGameManager.Instance != null)
        {
            FirebaseGameManager.Instance.OnProfilesLoaded -= HandleProfilesLoaded;
        }

        var gameManager = FirebaseGameManager.Instance;
        if (gameManager == null) return;

        // GameManager에서 ID 정보를 가져옵니다.
        string myUserId = FirebaseAuthManager.Instance.UserId;
        string opponentId = gameManager.GetOpponentId(); 

        // 프로필 정보를 가져와서 저장합니다.
        MyProfile = gameManager.GetPlayerProfile(myUserId);
        OpponentProfile = gameManager.GetPlayerProfile(opponentId);

        if (MyProfile != null && OpponentProfile != null)
        {
            // TODO: 여기에 닉네임, 인벤토리 정보를 UI에 표시하는 로직 추가
            Debug.Log($"UI_LaunchIndicator_Firebase: 내 닉네임: {MyProfile.Nickname}, 상대 닉네임: {OpponentProfile.Nickname}");
        }
        else
        {
            Debug.LogWarning("UI_LaunchIndicator_Firebase: 프로필 정보를 모두 가져오지 못했습니다.");
        }
    }

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
            displayTurn = (turn / 2) + 1; // 내부 턴 번호를 컬링 규칙에 맞게 가공
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

    /// <summary>
    /// UI 제어 메서드입니다.
    /// AllcloseUI() 먼저 호출하고 필요한UI만 켜지는 메서드 생성하여 호출 하면됩니다.
    /// </summary>


    public void FinishedUI()
    {
        AllcloseUI();
        result.SetActive(true);
    }

    private void AllcloseUI()
    {
        roundPanel.SetActive(false);
        donutEntry.SetActive(false);
        minimap.SetActive(false);
        result.SetActive(false);
    }

}