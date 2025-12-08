using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Lobby,    // 로비
    Playing,    // 플레이
    Result      // 결과
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Lobby;
    public string gameSceneName;
    public string menuSceneName;
    private bool isAppStarted = true;
    private bool isEndingGame = false;
    [SerializeField] private GameObject notifier;
    [SerializeField] private GameObject matchmakingObj;
    public long lastDailyReset;
    public event Action DailyReset;
    public event Action<PlayerData> LevelUpdate;

    // 플레이어가 페널티로 잃은 도넛 (결과 화면 표시용)
    public DonutEntry PlayerPenalizedDonut1 { get; private set; }
    public DonutEntry PlayerPenalizedDonut2 { get; private set; }
    // 상대방으로부터 획득한 도넛 (승리 시 결과 화면 표시용)
    public DonutEntry CapturedDonut1 { get; private set; }
    public DonutEntry CapturedDonut2 { get; private set; }
    public FirebaseGameManager.GameOutcome LastGameOutcome { get; private set; } //게임 승패 저장

    // 페널티로 제거된 도넛 정보를 임시 저장하기 위한 변수
    private DonutEntry penalizedDonut1;
    private DonutEntry penalizedDonut2;
    private int penaltyIndex1 = -1;
    private int penaltyIndex2 = -1;

    public bool isEasterEgg = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        // 씬 들어오면 로딩 닫기(혹시 열려 있으면)
        UIManager.Instance?.HideLoading();

        // 여기서 카운트다운/스폰 트리거 등 시작
        SetState(GameState.Lobby);


        if (SceneManager.GetActiveScene().name != menuSceneName)
            SceneManager.LoadScene(menuSceneName, LoadSceneMode.Additive);
        
        // 앱 최초 실행 시에만 isAppStarted가 true인 상태로 Init 호출
        FirebaseAuthManager.Instance.Init(isAppStarted);
        
        StartCoroutine(Delay(2f));
    }
    
    
    private IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        notifier.SetActive(true);
        matchmakingObj.SetActive(true);
        CheckDailyReset();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름이 menuSceneName일 때만 보상 반영 및 초기화
        if (scene.name == menuSceneName)
        {
            // isAppStarted가 true이면, 앱 최초 실행 후 메뉴 씬이 처음 로드된 것.
            if (isAppStarted)
            {
                // Start()에서 Init(true)가 이미 호출되었으므로, 플래그만 false로 변경.
                isAppStarted = false;
            }
            else
            {
                // isAppStarted가 false이면, 게임이 끝나고 메뉴 씬으로 돌아온 것.
                // Init(false)를 호출하여 MainPanel이 열리도록 함.
               // FirebaseAuthManager.Instance.Init(isAppStarted);
            }
            
            SetState(GameState.Lobby);
        }
    }
    
    // 안드리오드 기계 뒤로가기 버튼 처리
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameState.Lobby == State)
        {
            //Debug.Log("뒤로가기 눌림!");
            // 팝업 띄우기
            UIManager.Instance.Open(PanelId.ExitPopup);
        }
    }

    // 게임 시작
    public void StartGame()
    {
        // 이전 게임의 결과 표시용 데이터 초기화
        PlayerPenalizedDonut1 = null;
        PlayerPenalizedDonut2 = null;
        CapturedDonut1 = null;
        CapturedDonut2 = null;
        LastGameOutcome = FirebaseGameManager.GameOutcome.Draw; // 기본값으로 초기화

        SetState(GameState.Playing);
        UIManager.Instance.Open(PanelId.MatchingPopUp);
        FirebaseMatchmakingManager.Instance.StartMatchmaking();
        DataManager.Instance.SaveAllUserDataAsync();
    }
    
    // 게임 끝 -> 메인화면으로 다시 전환
    public async void EndGame()
    {
        if (isEndingGame) return;
        isEndingGame = true;
        
        Debug.Log("EndGame 실행");
        SceneLoader.Instance.LoadLocal(menuSceneName, true, () =>
        {
            UIManager.Instance.Open(PanelId.MainPanel);
            SetState(GameState.Lobby);
            ApplyResultRewards();
            isEndingGame = false;
        });
    }
    

    public void SetState(GameState state)
    {
        State = state;
        // HUD나 시스템에 이벤트로 알려주고, 상태별 로직 실행

    }

    /// <summary>
    /// 게임 결과를 GameManager에 저장합니다.
    /// </summary>
    public void SetGameOutcome(FirebaseGameManager.GameOutcome outcome)
    {
        LastGameOutcome = outcome;
    }

    //게임종료시의 UI로부터 받아올 보상 값들을 담을 변수
    public int pendingExp;
    public int pendingGold;
    public int pendingPoint;
    private List<DonutEntry> pendingRewardDonuts = new List<DonutEntry>();

    public void SetResultRewards(int level, int gold, int point) //이부분을 호출
    {
        pendingExp = level;
        pendingGold = gold;
        pendingPoint = point;
    }
    
    // 게임 종료 후 바로 실행되는 함수
    public void ApplyResultRewards()
    {
        Debug.Log("ApplyResultRewards 작동111111111111111111111111111");
        // 1. 페널티 복구 로직 (DB에서 데이터를 다시 로드한 후 실행)
        if (LastGameOutcome == FirebaseGameManager.GameOutcome.Draw)
        {
            if (penaltyIndex1 != -1 && penaltyIndex2 != -1)
            {
                DataManager.Instance.SetDonutAt(penaltyIndex1, true, penalizedDonut1);
                DataManager.Instance.SetDonutAt(penaltyIndex2, true, penalizedDonut2);
                Debug.Log("무승부: 페널티로 제거되었던 도넛과 점수가 복구됩니다.");
            }
        }
        else if (LastGameOutcome == FirebaseGameManager.GameOutcome.Win)
        {
            if (penaltyIndex1 != -1 && penaltyIndex2 != -1)
            {
                DataManager.Instance.SetDonutAt(penaltyIndex1, true, penalizedDonut1);
                DataManager.Instance.SetDonutAt(penaltyIndex2, true, penalizedDonut2);
                Debug.Log("승리: 페널티로 제거되었던 도넛이 복구됩니다.");
            }
        }
        else if (LastGameOutcome == FirebaseGameManager.GameOutcome.Lose)
        {
            Debug.Log("패배");
        }
        UIManager.Instance.Open(PanelId.MainPanel);
        
        int oldLevel = DataManager.Instance.PlayerData.level;
        //UIManager.Instance.Open(PanelId.MainPanel);
        
        Debug.Log(">>>>>> 게임보상을 반영합니다.");
        // 2. 실제 데이터 반영 (경험치, 골드, 포인트)
        
        //DataManager.Instance.PlayerData.exp += pendingExp;
        DataManager.Instance.PlayerData.gold += pendingGold;
        DataManager.Instance.PlayerData.soloScore += pendingPoint;
        Debug.Log($"pendingGold : {pendingGold}, pendingPoint : {pendingPoint}");

        DataManager.Instance.GoldChange(DataManager.Instance.PlayerData.gold);
        //DataManager.Instance.LevelChange(DataManager.Instance.PlayerData.level);            

        
        // 3. 승리 시 상대 도넛 보상 지급
        if (LastGameOutcome == FirebaseGameManager.GameOutcome.Win)
        {
            if (CapturedDonut1 != null) pendingRewardDonuts.Add(CapturedDonut1);
            if (CapturedDonut2 != null) pendingRewardDonuts.Add(CapturedDonut2);
        }
        
        // 보류 중인 보상 도넛 지급
        foreach (var donutEntry in pendingRewardDonuts)
        {
            DonutData rewardDonut = DataManager.Instance.GetDonutByID(donutEntry.id);
            if (rewardDonut != null)
            {
                BoardManager.Instance.AddRewardDonut(rewardDonut);
            }
        }

        //DataManager.Instance.PlayerData.exp -= pendingExp;
        LevelUp(pendingExp);
        // 레벨업 팝업 조건
        // if (DataManager.Instance.PlayerData.level > oldLevel)
        // {
        //     UIManager.Instance.Open(PanelId.LevelUpRewardPopUp);
        // }

        // 4. pending 값 초기화
        pendingExp = 0;
        pendingGold = 0;
        pendingPoint = 0;
        pendingRewardDonuts.Clear();
        
        // 5. 모든 결과 및 보상이 적용된 최종 데이터를 Firestore에 저장
        _ = DataManager.Instance.SaveAllUserDataAsync();
        
    }

    // 레벨업 로직
    public void LevelUp(int getExp)
    {
        if(DataManager.Instance.PlayerData.level >= 20)
            return;
        
        DataManager.Instance.PlayerData.exp += getExp;
        Debug.Log($"exp : {DataManager.Instance.PlayerData.exp}");
        
        // 100 이 넘으면 레벨업
        if (DataManager.Instance.PlayerData.exp >= DataManager.Instance.PlayerData.level * 100)
        {
            Debug.Log($"레벨업 {DataManager.Instance.PlayerData.level} / {DataManager.Instance.PlayerData.exp / (DataManager.Instance.PlayerData.level * 100)}");
            DataManager.Instance.PlayerData.level += 1;
            DataManager.Instance.LevelChange(DataManager.Instance.PlayerData.level);
            
            LevelUpdate.Invoke(DataManager.Instance.PlayerData);
            UIManager.Instance.Open(PanelId.LevelUpRewardPopUp);
            // 레벨업 보상상자
            //BoardManager.Instance.SpawnGiftBox();
            // 레벨업팝업UI 컴포넌트로 달린 스크립트에보상상자 주는 함수있슴니다
            BoardManager.Instance.RefreshBoardUnlock();
        }
        
    }
    private int index1;
    private int index2;
    
    public void ApplyStartGamePenalty(int _index1, int _index2, List<DonutEntry> opponentDonuts)
    {
        if (DataManager.Instance == null) return;
        index1 = _index1;
        index2 = _index2;

        // 1. DataManager로부터 현재 데이터 가져오기
        var currentDonuts = DataManager.Instance.InventoryData.donutEntries;
        var currentScore = DataManager.Instance.PlayerData.soloScore;

        if (currentDonuts == null || currentDonuts.Count < 5)
        {
            //Debug.LogWarning("페널티를 적용할 수 없습니다: 도넛 인벤토리가 초기화되지 않았습니다.");
            return;
        }

        // 1-1. 페널티 적용 전, 복구를 위해 현재 도넛 정보와 인덱스 저장
        this.penaltyIndex1 = index1;
        this.penalizedDonut1 = currentDonuts[index1];
        this.PlayerPenalizedDonut1 = currentDonuts[index1]; // 결과 화면 표시용으로 저장
        this.penaltyIndex2 = index2;
        this.penalizedDonut2 = currentDonuts[index2];
        this.PlayerPenalizedDonut2 = currentDonuts[index2]; // 결과 화면 표시용으로 저장

        // 1-2. (승리 시) 획득할 상대방 도넛을 미리 저장
        if (opponentDonuts != null)
        {
            List<DonutEntry> availableOpponentDonuts = opponentDonuts.Where(d => d != null).ToList();
            if (availableOpponentDonuts.Count >= 2 && index1 < availableOpponentDonuts.Count && index2 < availableOpponentDonuts.Count && index1 != index2)
            {
                CapturedDonut1 = availableOpponentDonuts[index1];
                CapturedDonut2 = availableOpponentDonuts[index2];
                Debug.Log($"결과 화면에 표시할 획득 예정 도넛으로 저장: {CapturedDonut1.id}, {CapturedDonut2.id}");
            }
            else
            {
                Debug.LogWarning("상대방 도넛 정보가 충분하지 않아 획득 예정 도넛을 저장할 수 없습니다.");
            }
        }

        // 2. 새로운 데이터 상태 계산
        currentDonuts[index1] = null;
        currentDonuts[index2] = null;

        int newScore = Mathf.Max(0, currentScore - 10);

        // 3. DataManager에 변경된 데이터 저장을 요청
        _ = DataManager.Instance.ApplyPenaltyData(currentDonuts, newScore);

        Debug.Log($"게임 시작 페널티 로직 실행. 인덱스 {index1}, {index2}의 도넛이 제거됩니다.");
    }

    /// <summary>
    /// 무승부 시, 페널티로 제거된 도넛을 복구하도록 DataManager에 요청합니다.
    /// </summary>
    public void ProcessDrawOutcome(int exp, int rewardGold, int rewardPoint)
    {
        if (DataManager.Instance == null) return;
        if (penaltyIndex1 == -1 || penaltyIndex2 == -1) // 페널티 정보가 유효하지 않으면 실행 안함
        {
            Debug.LogWarning("무승부 도넛 복구 실패: 페널티 정보가 유효하지 않습니다.");
            return;
        }
        
        // DataManager의 SetDonutAt을 직접 호출하여 로컬 인벤토리 복구
        DataManager.Instance.SetDonutAt(penaltyIndex1, true, penalizedDonut1);
        DataManager.Instance.SetDonutAt(penaltyIndex2, true, penalizedDonut2);

        // soloscore 10점 복구
        DataManager.Instance.PlayerData.soloScore += 10;
        Debug.Log($"무승부: soloscore 10점 복구. 현재 점수: {DataManager.Instance.PlayerData.soloScore}");
        DataManager.Instance.PlayerData.gold += rewardGold;
        

        // 로컬 리스너에게 변경 알림 
        //DataManager.Instance.OnUserDataChanged?.Invoke(DataManager.Instance.PlayerData);
        //TODO : OnUserDataChanged는 여기서 호출안됨 변경알림때문에 문제 생길경우 수정해줘야함 

        // 복구 요청 후 임시 데이터 초기화
        penalizedDonut1 = null;
        penalizedDonut2 = null;
        penaltyIndex1 = -1;
        penaltyIndex2 = -1;
        Debug.Log("무승부: 페널티로 제거되었던 도넛이 로컬에서 복구되었습니다.");
    }

    /// <summary>
    /// 승리 시, 페널티로 제거된 도넛을 복구하도록 DataManager에 요청합니다.
    /// </summary>
    public void ProcessWinOutcome()
    {
        if (DataManager.Instance == null) return;
        if (penaltyIndex1 == -1 || penaltyIndex2 == -1) // 페널티 정보가 유효하지 않으면 실행 안함
        {
            Debug.LogWarning("승리 도넛 복구 실패: 페널티 정보가 유효하지 않습니다.");
            return;
        }

        // DataManager의 SetDonutAt을 직접 호출하여 로컬 인벤토리 복구
        DataManager.Instance.SetDonutAt(penaltyIndex1, true, penalizedDonut1);
        DataManager.Instance.SetDonutAt(penaltyIndex2, true, penalizedDonut2);

        // 복구 요청 후 임시 데이터 초기화
        penalizedDonut1 = null;
        penalizedDonut2 = null;
        penaltyIndex1 = -1;
        penaltyIndex2 = -1;
        Debug.Log("승리: 페널티로 제거되었던 도넛이 로컬에서 복구되었습니다.");
    }

    /// <summary>
    /// 승리 시, 미리 저장해둔 상대방 도넛을 보상으로 지급 대기열에 추가합니다.
    /// </summary>
    public void ProcessDonutCapture()
    {
        if (CapturedDonut1 != null && CapturedDonut2 != null)
        {
            // 보상 도넛을 즉시 추가하지 않고, 나중에 안전하게 지급하기 위해 임시 리스트에 추가합니다.
            pendingRewardDonuts.Add(CapturedDonut1);
            pendingRewardDonuts.Add(CapturedDonut2);
            Debug.Log($"보상 도넛 2개({CapturedDonut1.id}, {CapturedDonut2.id})를 획득하여 보류 중입니다.");
        }
        else
        {
            Debug.LogWarning("도넛 획득 실패: 캡처할 도넛 정보가 미리 설정되지 않았습니다.");
        }
    }

    // 하루 리셋
    public void CheckDailyReset()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    
        // 오늘 00:00 UTC 기준 (혹은 KST 00:00)
        DateTimeOffset today = DateTimeOffset.UtcNow.Date; 
        long todayStart = today.ToUnixTimeSeconds();

        if (lastDailyReset < todayStart)
        {
            DailyReset?.Invoke();
            DataManager.Instance.QuestData.currentChargeCount = 0;
            DataManager.Instance.InventoryData.dailyFreeGemClaimed = true;
            DataManager.Instance.InventoryData.dailyFreeEnergy = 5;
            //Debug.Log("[Daily] 오늘자 리셋 완료");
        }
        else
        {
            //Debug.Log("[Daily] 이미 오늘 리셋 완료 상태");
        }
    }
}