using System.Collections.Generic;
using DG.Tweening;
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
    [SerializeField] private TextMeshProUGUI aScoreText;
    [SerializeField] private TextMeshProUGUI bScoreText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI roundChangeText;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private GameObject CountDownText;

    [Header("디버그 텍스트")]
    public TextMeshProUGUI debugStateText;
    [Header("UI 제어 객체 (on/off)")]
    [SerializeField] private GameObject roundPanel;
    [SerializeField] private GameObject donutEntry;
    [SerializeField] private GameObject minimap;
    [SerializeField] private GameObject result;
    [SerializeField] private Scr_TweenHandDragGuide guide;
    [SerializeField] private GameObject settingsPanel;
    [Header("도넛 엔트리 항목")]
    public DonutSelectionUI donutSelectionUI; // (선택 가능) 내 도넛 선택 UI
    [SerializeField] private List<DonutEntryUI> myDisplayDonutSlots; // (표시 전용) 내 도넛 슬롯들
    [SerializeField] private List<DonutEntryUI> opponentDisplayDonutSlots; // (표시 전용) 상대방 도넛 슬롯들

    [Header("결과창 보상 갱신을 위한 변수")]
    [SerializeField] private TextMeshProUGUI expText; 
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI pointText;
    [SerializeField] private TextMeshProUGUI resultText; // 승리,패배 결과 텍스트
    [SerializeField] private TextMeshProUGUI getDonutText; // 승리,패배에 따라 획득/잃은 도넛 텍스트

   
    [Header("플로팅 텍스트")]
    [SerializeField] private FloatingText floatingText; // 씬에 미리 배치된 FloatingText 컴포넌트

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

        if (floatingText != null)
        {
            // 이 플로팅 텍스트는 파괴하지 않고 계속 재사용할 것이므로, destroyOnComplete 값을 false로 설정합니다.
            floatingText.destroyOnComplete = false;
            floatingText.gameObject.SetActive(false);
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
            // 내 인벤토리 UI 채우기
            if (donutSelectionUI != null)
            {
                donutSelectionUI.Populate(MyProfile.Inventory.donutEntries);
            }
            else
            {
                Debug.LogWarning("UI_LaunchIndicator_Firebase: donutSelectionUI가 할당되지 않았습니다.");
            }

            // 내 인벤토리 UI 채우기 (오프닝 타임라인)
            PopulateDisplayDonuts(myDisplayDonutSlots, MyProfile.Inventory.donutEntries);

            // 상대방 인벤토리 UI 채우기 (오프닝 타임라인) 
            PopulateDisplayDonuts(opponentDisplayDonutSlots, OpponentProfile.Inventory.donutEntries);
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

        if (Input.GetKeyDown(KeyCode.Escape)) //세팅창열고닫기 [안드로이드는 뒤로가기 버튼]
        {
            if (settingsPanel.activeSelf) settingsPanel.SetActive(false);   // 닫기
            else settingsPanel.SetActive(true); // 열기
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
    /// 지정된 위치에 플로팅 텍스트를 표시합니다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="screenPosition">텍스트가 나타날 스크린 좌표</param>
    public void ShowFloatingText(string message, Vector3 screenPosition)
    {
        if (floatingText == null)
        {
            Debug.LogError("FloatingText 컴포넌트가 할당되지 않았습니다!");
            return;
        }

        // 1. 위치 설정
        floatingText.transform.position = screenPosition;
        
        // 2. 텍스트 설정
        floatingText.SetText(message);

        // 3. 활성화 (활성화 시 FloatingText.cs의 OnEnable에서 애니메이션이 자동 시작됨)
        floatingText.gameObject.SetActive(true);
    }


    /// <summary>
    /// UI 제어 메서드입니다.
    /// AllcloseUI() 먼저 호출하고 필요한UI만 켜지는 메서드 생성하여 호출 하면됩니다.
    /// </summary>


    public void FinishedUI(FirebaseGameManager.GameOutcome outcome) // 게임종료 - 결과창
    {
        AllcloseUI();
        result.SetActive(true);
        ResultRewardView(outcome);
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
        settingsPanel.SetActive(false);
    }
    public void ShowGuideUI(int select)
    {// 조작가이드용 손가락 실행 해주는 부분 (1 == 위아래 , 2 == 좌우)
     // 반복 횟수 , 속도 거리등은 해당 객체 인스펙터에서 조절
        guide.gameObject.SetActive(true);

        if (select == 1) { guide.PlayVerticalDrag(); }
        else if (select == 2) { guide.PlayHorizontalDrag(); }
        else if (select == 3) { guide.PlayTouchMove(); }
        else { Debug.Log("올바른 가이드 출력 번호가 아닙니다."); }
    }
    public void HideGuideUI()
    {
        guide.gameObject.SetActive(false);
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

    public void ResultRewardView(FirebaseGameManager.GameOutcome outcome)  //게임결과에따라 각종 데이터를 넣어줌 TODO:캐릭터이미지랑 레벨,랭킹도 넣어야함
    {
        int exp = 0;
        int rewardGold = 0;
        int rewardPoint = 0;
        string result = outcome.ToString();
        string getDonut = "";

        switch (outcome)
        {
            case FirebaseGameManager.GameOutcome.Win:
                exp = 15;
                rewardGold = 150;
                rewardPoint = 20; // 페널티 복구 10점 + 승리 보너스 10점
                result = "VICTORY!";
                getDonut = "획득 도넛";
                
                GameManager.Instance.ProcessWinOutcome(); // 페널티로 제거되었던 도넛 복구
                GameManager.Instance.ProcessDonutCapture(); // 상대 도넛 획득 (획득할 도넛 정보는 이미 게임 시작 시점에 결정됨)
                break;
            case FirebaseGameManager.GameOutcome.Lose:
                exp = 8;
                rewardGold = 50;
                rewardPoint = 0; // 솔로스코어는 미리 반영되었으므로 0
                result = "DEFEAT!";
                getDonut = "잃은 도넛";
                break;
            case FirebaseGameManager.GameOutcome.Draw:
                exp = 10;
                rewardGold = 100;
                rewardPoint = 0; // 페널티로 잃었던 10점 복구
                result = "DRAW!";
                getDonut = "획득 도넛";
                GameManager.Instance.ProcessDrawOutcome(); // 페널티로 제거되었던 도넛 복구
                break;
        }

        // UI 텍스트 갱신
        //int previewLevel = DataManager.Instance.PlayerData.exp + exp; 

        if (expText != null) expText.text = $"+{exp}";
        if (goldText != null) goldText.text = $"+{rewardGold}";
        if (pointText != null) pointText.text = $"+{rewardPoint}";
        if (resultText != null) resultText.text = $"{result}";
        if (getDonutText != null) getDonutText.text = $"{getDonut}";

        GameManager.Instance.SetResultRewards(exp, rewardGold, rewardPoint);

    }
}