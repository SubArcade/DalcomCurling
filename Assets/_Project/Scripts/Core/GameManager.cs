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
    [SerializeField] private GameObject notifier;
    [SerializeField] private GameObject matchmakingObj;
    
    public event Action<PlayerData> LevelUpdate;

    // 페널티로 제거된 도넛 정보를 임시 저장하기 위한 변수
    private DonutEntry penalizedDonut1;
    private DonutEntry penalizedDonut2;
    private int penaltyIndex1 = -1;
    private int penaltyIndex2 = -1;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
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
        
        FirebaseAuthManager.Instance.Init();

        StartCoroutine(Delay(2f));
    }
    
    
    private IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        notifier.SetActive(true);
        matchmakingObj.SetActive(true);
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름이 menuSceneName일 때만 보상 반영
        if (scene.name == menuSceneName)
        {
            SetState(GameState.Lobby);
            StartCoroutine(ApplyRewardsNextFrame());
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    
    // 안드리오드 기계 뒤로가기 버튼 처리
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameState.Lobby == State)
        {
            Debug.Log("뒤로가기 눌림!");
            // 팝업 띄우기
        }
    }

    // 게임 시작
    public void StartGame()
    {
        SetState(GameState.Playing);
        UIManager.Instance.Open(PanelId.MatchingPopUp);
        FirebaseMatchmakingManager.Instance.StartMatchmaking();
        DataManager.Instance.SaveAllUserDataAsync();
    }
    
    public void EndGame()
    {
        SetState(GameState.Result);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
    }
    

    public void SetState(GameState state)
    {
        State = state;
        // HUD나 시스템에 이벤트로 알려주고, 상태별 로직 실행

    }

    //게임종료시의 UI로부터 받아올 보상 값들을 담을 변수
    private int pendingExp;
    private int pendingGold;
    private int pendingPoint;
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
        if (State == GameState.Lobby)
        {
            UIManager.Instance.Open(PanelId.MainPanel);
            
            int oldLevel = DataManager.Instance.PlayerData.level;
            UIManager.Instance.Open(PanelId.MainPanel);
            
            Debug.Log(">>>>>> 게임보상을 반영합니다.");
            // 실제 데이터 반영
            DataManager.Instance.PlayerData.exp += pendingExp;
            DataManager.Instance.PlayerData.gold += pendingGold;
            DataManager.Instance.PlayerData.soloScore += pendingPoint;

            DataManager.Instance.GoldChange(DataManager.Instance.PlayerData.gold);
            DataManager.Instance.LevelChange(DataManager.Instance.PlayerData.level);            

            // 레벨업 팝업 조건
            if (DataManager.Instance.PlayerData.level > oldLevel)
            {
                UIManager.Instance.Open(PanelId.LevelUpRewardPopUp);
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

            // pending 값 초기화
            pendingExp = 0;
            pendingGold = 0;
            pendingPoint = 0;
            pendingRewardDonuts.Clear();
        }
    }
    private IEnumerator ApplyRewardsNextFrame()
    {
        yield return null; // 한 프레임 대기
        ApplyResultRewards();
    }

    // 레벨업 로직
    public void LevelUp()
    {
        if(DataManager.Instance.PlayerData.level >= 20)
            return;
        
        DataManager.Instance.PlayerData.exp += 20;
    
        // 100 이 넘으면 레벨업
        if (DataManager.Instance.PlayerData.exp >= 100)
        {
            DataManager.Instance.PlayerData.level += 1;
            DataManager.Instance.PlayerData.exp -= 100;
            
            LevelUpdate.Invoke(DataManager.Instance.PlayerData);
            UIManager.Instance.Open(PanelId.LevelUpRewardPopUp);
            // 레벨업 보상상자
           // BoardManager.Instance.SpawnGiftBox();
           // 레벨업팝업UI 컴포넌트로 달린 스크립트에보상상자 주는 함수있슴니다
        }
        
    }
    private int index1;
    private int index2;
    public void ApplyStartGamePenalty(int _index1, int _index2)
    {
        if (DataManager.Instance == null) return;
        index1 = _index1;
        index2 = _index2;

        // 1. DataManager로부터 현재 데이터 가져오기
        var currentDonuts = DataManager.Instance.InventoryData.donutEntries;
        var currentScore = DataManager.Instance.PlayerData.soloScore;

        if (currentDonuts == null || currentDonuts.Count < 5)
        {
            Debug.LogWarning("페널티를 적용할 수 없습니다: 도넛 인벤토리가 초기화되지 않았습니다.");
            return;
        }

        // 1-1. 페널티 적용 전, 복구를 위해 현재 도넛 정보와 인덱스 저장
        this.penaltyIndex1 = index1;
        this.penalizedDonut1 = currentDonuts[index1];
        this.penaltyIndex2 = index2;
        this.penalizedDonut2 = currentDonuts[index2];

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
    public void ProcessDrawOutcome()
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
    /// 승리 시 상대방의 도넛 엔트리에서 랜덤으로 2개를 복사하여 자신의 인벤토리에 추가합니다.
    /// </summary>
    /// <param name="opponentDonuts">상대방의 도넛 엔트리 리스트</param>
    public void ProcessDonutCapture(List<DonutEntry> opponentDonuts)
    {
        if (DataManager.Instance == null || opponentDonuts == null || opponentDonuts.Count == 0)
        {
            Debug.LogWarning("도넛 획득 실패: 상대방 도넛 정보가 유효하지 않습니다.");
            return;
        }

        List<DonutEntry> availableOpponentDonuts = opponentDonuts.Where(d => d != null).ToList();

        if (availableOpponentDonuts.Count < 2)
        {
            Debug.LogWarning("도넛 획득 실패: 상대방의 유효한 도넛이 2개 미만입니다.");
            return;
        }

        DonutEntry capturedDonut1 = availableOpponentDonuts[index1];
        DonutEntry capturedDonut2 = availableOpponentDonuts[index2];

        if (capturedDonut1 != null && capturedDonut2 != null)
        {
            // 보상 도넛을 즉시 추가하지 않고, 나중에 안전하게 지급하기 위해 임시 리스트에 추가합니다.
            pendingRewardDonuts.Add(capturedDonut1);
            pendingRewardDonuts.Add(capturedDonut2);
            Debug.Log($"보상 도넛 2개({capturedDonut1.id}, {capturedDonut2.id})를 획득하여 보류 중입니다.");
        }
        else
        {
            Debug.LogError("도넛 획득 오류: 선택된 도넛 중 null이 있습니다. 로직을 확인하세요.");
        }
    }
}