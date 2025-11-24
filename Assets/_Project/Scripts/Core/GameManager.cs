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
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ✅ 씬 이름이 menuSceneName일 때만 보상 반영
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
    public void ApplyResultRewards()
    {
        if (State == GameState.Lobby)
        {
            int oldLevel = DataManager.Instance.PlayerData.level;

            // 실제 데이터 반영
            DataManager.Instance.PlayerData.level += pendingLevel;
            DataManager.Instance.PlayerData.gold += pendingGold;
            DataManager.Instance.PlayerData.soloScore += pendingPoint;

            DataManager.Instance.GoldChange(DataManager.Instance.PlayerData.gold);
            DataManager.Instance.LevelChange(DataManager.Instance.PlayerData.level);

            // ✅ 레벨업 팝업 조건
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

}