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
    public TextMeshProUGUI roundChangeText;
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
    [SerializeField] private DonutSelectionUI donutSelectionUI; // (선택 가능) 내 도넛 선택 UI
    [SerializeField] private List<DonutEntryUI> myDisplayDonutSlots; // (표시 전용) 내 도넛 슬롯들
    [SerializeField] private List<DonutEntryUI> opponentDisplayDonutSlots; // (표시 전용) 상대방 도넛 슬롯들

    //내부변수
    private int displayTurn = 0;

    // 현재 플레이어와 상대방의 프로필 정보
    public PlayerProfile MyProfile { get; private set; }
    public PlayerProfile OpponentProfile { get; private set; }

    void Start()
    {
        // Firebase에서 데이터를 로드합니다.
        var gameManager = FirebaseGameManager.Instance;
        if (gameManager != null)
        {
            // 데이터가 이미 로드되었는지 확인합니다.
            if (gameManager.HasLoadedProfiles())
            {
                // 이미 로드되었다면 즉시 UI를 업데이트합니다.
                HandleProfilesLoaded();
            }
            else
            {
                // 아직 로드되지 않았다면, 로드가 완료될 때를 기다리기 위해 이벤트를 구독합니다.
                gameManager.OnProfilesLoaded += HandleProfilesLoaded;
            }
        }
        else
        {
            Debug.LogError("UI_LaunchIndicator_Firebase: Start()에서 FirebaseGameManager.Instance를 찾을 수 없습니다.");
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
        Debug.Log($"GetPlayerProfile : {opponentId} 여기서 함수 호출");

        // 프로필 정보를 가져와서 저장합니다.
        MyProfile = gameManager.GetPlayerProfile(myUserId);
        Debug.Log("GetPlayerProfile : MyProfile 여기서 함수 호출");
        OpponentProfile = gameManager.GetPlayerProfile(opponentId);
        Debug.Log("GetPlayerProfile : OpponentProfile 여기서 함수 호출");

        if (MyProfile != null)
        {
            // TODO: 여기에 닉네임, 인벤토리 정보를 UI에 표시하는 로직 추가
            Debug.Log($"UI_LaunchIndicator_Firebase: 내 닉네임: {MyProfile.Nickname}, 상대 닉네임: {OpponentProfile.Nickname}");

            // 1. (선택 가능) 내 인벤토리 UI 채우기
            if (donutSelectionUI != null)
            {
                donutSelectionUI.Populate(MyProfile.Inventory.donutEntries);
            }
            else
            {
                Debug.LogWarning("UI_LaunchIndicator_Firebase: donutSelectionUI가 할당되지 않았습니다.");
            }

            // 2. (표시 전용) 내 인벤토리 UI 채우기
            PopulateDisplayDonuts(myDisplayDonutSlots, MyProfile.Inventory.donutEntries);

            // 3. (표시 전용) 상대방 인벤토리 UI 채우기
            PopulateDisplayDonuts(opponentDisplayDonutSlots, OpponentProfile.Inventory.donutEntries);
        }
        else if (OpponentProfile != null)
        {
            Debug.LogWarning("UI_LaunchIndicator_Firebase: OpponentProfile 프로필 정보를 모두 가져오지 못했습니다.");
        }
        else
        {
            Debug.LogWarning("UI_LaunchIndicator_Firebase: MyProfile 프로필 정보를 모두 가져오지 못했습니다.");
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
        roundChangeText.text = $"Round {round} Start!";
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


    public void FinishedUI() // 결과창
    {
        AllcloseUI();
        result.SetActive(true);
    }
    public void FireShotReadyUI() // 발사 준비상태 모든 UI가 다보임
    {
        AllcloseUI();
        roundPanel.SetActive(true);
        donutEntry.SetActive(true);
        minimap.SetActive(true);
    }
    public void FireShotReadyTwoUI()
    {
        donutEntry.SetActive(false);
    }
    public void IdleUI() //기본 상단 UI만 출력되는 상태
    {
        AllcloseUI();
        roundPanel.SetActive(true);
    }
    public void AllcloseUI() // 모든 UI를 닫음
    {
        roundPanel.SetActive(false);
        donutEntry.SetActive(false);
        minimap.SetActive(false);
        result.SetActive(false);
    }

    /// <summary>
    /// (표시 전용) 도넛 인벤토리 UI 슬롯들을 채웁니다.
    /// </summary>
    /// <param name="slots">채울 UI 슬롯 리스트</param>
    /// <param name="entries">표시할 도넛 엔트리 리스트</param>

    private void PopulateDisplayDonuts(List<DonutEntryUI> slots, List<DonutEntry> entries)
    {
        if (slots == null) return;
        if (entries == null) entries = new List<DonutEntry>();

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < entries.Count)
            {
                slots[i].gameObject.SetActive(true);
                // 표시 전용 슬롯은 클릭 기능이 필요 없으므로 onClickAction에 null 전달
                slots[i].Setup(entries[i], null);
            }
            else
            {
                slots[i].gameObject.SetActive(false); // 남는 슬롯은 비활성화
            }
        }
    }

}