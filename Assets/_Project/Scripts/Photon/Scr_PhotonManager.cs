//using Photon.Pun;
//using Photon.Realtime;
//using UnityEngine;
//using System.Collections;

//public class PhotonManager : MonoBehaviourPunCallbacks
//{
//    [Header("씬 설정")]
//    [SerializeField] private string gameSceneName = "Sce_GameScene";

//    [Header("UI Manager")]
//    public UIManager_test2 uiManager;

//    private bool isMatching = false;
//    private float matchingStartTime;
//    private bool isConnectedToMaster = false;

//    void Start()
//    {
//        PhotonNetwork.AutomaticallySyncScene = false;
//        PhotonNetwork.ConnectUsingSettings();
//    }

//    void Update()
//    {
//        if (isMatching && Time.time - matchingStartTime > 30f)
//        {
//            Debug.Log("매칭 시간 초과!");
//            OnCancelButtonClicked();
//        }
//    }

//    public void OnStartButtonClicked()
//    {
//        if (isMatching)
//        {
//            Debug.Log("이미 매칭 중입니다");
//            return;
//        }

//        if (!isConnectedToMaster)
//        {
//            Debug.Log("아직 포톤 연결 중입니다...");
//            return;
//        }

//        Debug.Log("매칭 시작!");
//        isMatching = true;
//        matchingStartTime = Time.time;

//        if (uiManager != null)
//            uiManager.ShowMatchingUI();

//        PhotonNetwork.JoinRandomRoom();
//    }

//    public void OnCancelButtonClicked()
//    {
//        if (!isMatching) return;

//        Debug.Log("매칭 취소!");
//        isMatching = false;

//        if (PhotonNetwork.InRoom)
//            PhotonNetwork.LeaveRoom();

//        if (uiManager != null)
//            uiManager.HideMatchingUI();
//    }

//    public override void OnConnectedToMaster()
//    {
//        Debug.Log("포톤 마스터 서버 연결 성공!");
//        isConnectedToMaster = true;
//    }

//    public override void OnJoinedLobby()
//    {
//        Debug.Log("로비 입장 성공!");

//        if (isMatching)
//        {
//            PhotonNetwork.JoinRandomRoom();
//        }
//    }

//    public override void OnJoinedRoom()
//    {
//        Debug.Log("방 입장! 현재 인원: " + PhotonNetwork.CurrentRoom.PlayerCount + "/2");

//        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
//        {
//            StartGameScene();
//        }
//        else
//        {
//            Debug.Log("상대방 기다리는 중...");
//        }
//    }

//    public override void OnJoinRandomFailed(short returnCode, string message)
//    {
//        Debug.Log("참여할 방 없음, 새 방 생성...");

//        if (isMatching)
//        {
//            RoomOptions options = new RoomOptions();
//            options.MaxPlayers = 2;
//            options.IsVisible = true;
//            options.IsOpen = true;

//            PhotonNetwork.CreateRoom(null, options);
//        }
//    }

//    public override void OnCreatedRoom()
//    {
//        Debug.Log("새 방 생성 성공! 상대방 기다리는 중...");
//    }

//    public override void OnPlayerEnteredRoom(Player newPlayer)
//    {
//        Debug.Log("상대방 입장! 현재 인원: " + PhotonNetwork.CurrentRoom.PlayerCount + "/2");

//        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
//        {
//            StartGameScene();
//        }
//    }

//    public override void OnLeftRoom()
//    {
//        Debug.Log("방 나감");
//    }

//    public override void OnJoinRoomFailed(short returnCode, string message)
//    {
//        Debug.Log("방 입장 실패: " + message);
//        OnCancelButtonClicked();
//    }

//    public override void OnCreateRoomFailed(short returnCode, string message)
//    {
//        Debug.Log("방 생성 실패: " + message);
//        OnCancelButtonClicked();
//    }

//    public override void OnDisconnected(DisconnectCause cause)
//    {
//        Debug.Log("포톤 연결 끊김: " + cause);
//        isMatching = false;
//        isConnectedToMaster = false;

//        if (uiManager != null)
//            uiManager.HideMatchingUI();
//    }

//    private void StartGameScene()
//    {
//        Debug.Log("2명 모임! 게임씬으로 전환");
//        isMatching = false;

//        if (uiManager != null)
//            uiManager.HideMatchingUI();

//        PhotonNetwork.LoadLevel(gameSceneName);
//    }
//}