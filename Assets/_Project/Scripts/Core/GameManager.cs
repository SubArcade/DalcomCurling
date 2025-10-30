using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public enum GameState
{
    Lobby,    // 로비
    Playing,    // 플레이
    Result      // 결과
}

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Lobby;
    public string gameSceneName;
    public string menuSceneName;
    private void Awake() => Instance = this;

    private void Start()
    {
        // 씬 들어오면 로딩 닫기(혹시 열려 있으면)
        UIManager.Instance?.HideLoading();

        // 여기서 카운트다운/스폰 트리거 등 시작
        SetState(GameState.Lobby);
        
        if (SceneManager.GetActiveScene().name != menuSceneName)
            SceneManager.LoadScene(menuSceneName, LoadSceneMode.Additive);
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
    
    
}