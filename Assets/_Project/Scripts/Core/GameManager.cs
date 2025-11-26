using System;
using System.Collections;
using System.Collections.Generic;
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
    
    public event Action<PlayerData> LevelUpdate;
    
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
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름이 menuSceneName일 때만 보상 반영
        if (scene.name == menuSceneName)
        {
            SetState(GameState.Lobby);
            ApplyResultRewards();
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
    private int pendingLevel;
    private int pendingGold;
    private int pendingPoint;

    public void SetResultRewards(int level, int gold, int point) 
    {
        pendingLevel = level;
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

            // 실제 데이터 반영
            DataManager.Instance.PlayerData.level += pendingLevel;
            DataManager.Instance.PlayerData.gold += pendingGold;
            DataManager.Instance.PlayerData.soloScore += pendingPoint;

            DataManager.Instance.GoldChange(DataManager.Instance.PlayerData.gold);
            DataManager.Instance.LevelChange(DataManager.Instance.PlayerData.level);

            // 레벨업 팝업 조건
            if (DataManager.Instance.PlayerData.level > oldLevel)
            {
                UIManager.Instance.Open(PanelId.LevelUpRewardPopUp);
            }

            // pending 값 초기화
            pendingLevel = 0;
            pendingGold = 0;
            pendingPoint = 0;
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
    
}