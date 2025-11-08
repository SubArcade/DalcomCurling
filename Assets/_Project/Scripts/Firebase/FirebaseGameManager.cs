using Firebase.Firestore; // Firebase Firestore 기능을 사용하기 위해 필요합니다.
using System.Collections.Generic; // 리스트나 딕셔너리 같은 자료구조를 사용하기 위해 필요합니다.
using UnityEngine; // Unity 엔진의 기능을 사용하기 위해 필요합니다.
using System.Linq; // 리스트에서 데이터를 쉽게 찾거나 걸러낼 때 사용합니다.
using DG.Tweening; // DOTween 애니메이션 라이브러리를 사용하기 위해 필요합니다.

/// <summary>
/// 이 스크립트는 컬링 게임의 전체적인 흐름(상태)을 관리하는 중요한 역할을 합니다.
/// Firebase Firestore와 연동하여 게임의 상태를 실시간으로 업데이트하고,
/// 플레이어의 행동(샷 발사, 예측 결과 전송 등)에 따라 게임을 진행합니다.
/// </summary>
public class FirebaseGameManager : MonoBehaviour
{
    // --- 싱글톤 패턴 ---
    // 이 클래스는 게임 내에 단 하나만 존재하도록 합니다.
    // 다른 스크립트에서 쉽게 접근할 수 있도록 'Instance'라는 이름으로 자신을 저장합니다.
    public static FirebaseGameManager Instance { get; private set; }

    // --- 게임의 현재 상태를 나타내는 변수 ---
    // 게임이 현재 어떤 단계에 있는지를 알려주는 역할을 합니다.
    private enum LocalGameState
    {
        Idle, // 아무것도 하지 않고 대기 중인 상태
        WaitingForInput, // 내 턴이 되어 돌 조작을 기다리는 상태
        SimulatingMyShot, // 내가 쏜 돌이 움직이는 중인 상태
        WaitingForPrediction, // 시뮬레이션이 끝나고 상대방의 예측 결과를 기다리는 상태
        SimulatingOpponentShot // 상대방이 쏜 돌을 시뮬레이션 중인 상태 (예측자 역할)
    }
    private LocalGameState _localState = LocalGameState.Idle; // 현재 게임의 로컬 상태

    // --- Firebase 관련 필드 ---
    private FirebaseFirestore db; // Firebase Firestore 데이터베이스 접근 객체
    private ListenerRegistration gameListener; // Firestore 데이터 변경 감시자
    private string gameId; // 현재 진행 중인 게임의 고유 ID
    private string roomId; // 현재 진행 중인 게임의 룸 ID
    private string myUserId; // 현재 플레이 중인 나의 고유 ID

    // --- 게임 상태 관련 필드 ---
    private Game _currentGame; // Firestore에서 받아온 최신 게임 데이터
    private bool _isMyTurn = false; // 현재 내 턴 여부
    private PredictedResult _cachedPrediction = null; // 너무 일찍 도착한 예측 결과를 임시로 보관

    // --- 다른 스크립트들과의 연결 ---
    [SerializeField] private StoneShoot_Firebase inputController; // 돌 조작(입력)을 담당하는 스크립트
    [SerializeField] private StoneManager stoneManager; // 돌 생성 및 움직임 관리 스크립트
    [SerializeField] private Src_GameCamControl gameCamControl; // 카메라 연출을 제어하는 스크립트

    // --- 카메라 인덱스 상수 --- (카메라 추가하고 명칭도 다시 명명해야함)
    private const int START_VIEW_CAM = 0; // 기본 뷰 카메라
    private const int FOLLOW_STONE_CAM1 = 1; // 돌 따라가는 카메라 (비스듬한 탑뷰)
    private const int FOLLOW_STONE_CAM2 = 2; // 돌 따라가는 카메라 (발사 후)
    private const int FREE_LOOK_CAM = 3; // 점수라인 카메라
    private const int SIMULATING_VIEW_CAM = 4; // 시뮬레이션 카메라

    // --- 게임 시스템 변수 ---
    public float timeMultiplier { get; private set; } = 5f; //게임 빨리감기 속도를 결정할 변수, 읽기전용 (기본값 5)

    // --- 게임 내부 변수 ---
    private float initialFixedDeltaTime;
    private bool roundUpdated = false;
    
    // --- 공유 가능한 게임 변수 ---
    public int aTeamScore { get; private set; }= 0;
    public int bTeamScore { get; private set; }= 0;

    #region Unity Lifecycle (유니티 생명주기)
    /// <summary>
    /// 유니티 게임 오브젝트가 처음 생성될 때 한 번 호출됩니다.
    /// 이 스크립트가 게임에 단 하나만 존재하도록 설정합니다.
    /// </summary>
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 게임 오브젝트가 활성화될 때 호출됩니다.
    /// Firebase 연결을 초기화하고 게임 데이터를 감시하기 시작합니다.
    /// </summary>
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        gameId = FirebaseMatchmakingManager.CurrentGameId;
        roomId = FirebaseMatchmakingManager.CurrentRoomId; // RoomId 가져오기
        myUserId = FirebaseAuthManager.Instance.UserId;

        initialFixedDeltaTime = Time.fixedDeltaTime;

        // 게임 ID나 사용자 ID가 없으면 게임을 진행할 수 없습니다.
        if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(myUserId))
        {
            Debug.LogError("Game ID or User ID is missing.");
            return;
        }

        // 돌 조작 스크립트가 존재한다면 샷 확정 이벤트를 등록합니다.
        if (inputController != null) inputController.OnShotConfirmed += SubmitShot;

        // Firestore에서 게임 데이터 변화를 감시합니다.
        DocumentReference gameRef = db.Collection("games").Document(gameId);
        gameListener = gameRef.Listen(OnGameSnapshot);

        // Firestore에 나의 준비 상태를 업데이트합니다.
        gameRef.UpdateAsync("ReadyPlayers", FieldValue.ArrayUnion(myUserId));
    }

    /// <summary>
    /// 게임 오브젝트가 파괴될 때 호출됩니다.
    /// Firestore 감시자와 이벤트 등록을 해제합니다.
    /// </summary>
    void OnDestroy()
    {
        gameListener?.Stop();
        if (inputController != null) inputController.OnShotConfirmed -= SubmitShot;
    }
    #endregion

    #region Firebase Event Handlers
    /// <summary>
    /// Firestore에서 게임 데이터가 변경될 때마다 호출됩니다.
    /// 게임의 최신 상태를 받아와 필요한 동작을 수행합니다.
    /// </summary>
    private void OnGameSnapshot(DocumentSnapshot snapshot)
    {
        if (!snapshot.Exists) return;

        Game newGameData;
        try
        {
            newGameData = snapshot.ConvertTo<Game>(ServerTimestampBehavior.Estimate);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Snapshot] Failed to convert data: {e}");
            return;
        }

        bool isFirstSnapshot = (_currentGame == null);
        bool turnChanged = !isFirstSnapshot && _currentGame.CurrentTurnPlayerId != newGameData.CurrentTurnPlayerId;
        bool newShotFired = _currentGame?.LastShot?.Timestamp != newGameData.LastShot?.Timestamp;
        bool newPredictionReceived = _currentGame?.PredictedResult?.TurnNumber != newGameData.PredictedResult?.TurnNumber;
        bool roundFinished = _currentGame?.RoundNumber != newGameData.RoundNumber;

        _currentGame = newGameData;
        _isMyTurn = _currentGame.CurrentTurnPlayerId == myUserId;

        Debug.Log($"[Snapshot] GameState: {_currentGame.GameState}, MyTurn: {_isMyTurn}, LocalState: {_localState}, newShot: {newShotFired}, newPrediction: {newPredictionReceived}");

        switch (_currentGame.GameState)
        {
            case "Initializing":
                if (_currentGame.ReadyPlayers.Count == 2 && IsHost())
                {
                    db.Collection("games").Document(gameId).UpdateAsync("GameState", "Timeline");
                }
                break;
            case "Timeline":
                if (_localState == LocalGameState.Idle)
                {
                    Debug.Log("타임라인 재생 및 5.83초 대기 시작");
                    gameCamControl?.PlayStartTimeline(); // 타임라인 재생만 실행

                    // 5.83초 후 게임 상태 변경
                    DOVirtual.DelayedCall(5.83f, () =>
                    {
                        if (IsHost())
                        {
                            var updates = new Dictionary<string, object>
                            {
                                { "GameState", "InProgress" },
                                { "LastShot", null },
                                { "PredictedResult", null }
                            };
                            db.Collection("games").Document(gameId).UpdateAsync(updates);
                        }
                    });
                }
                break;
            case "InProgress":
                
                if (turnChanged || (_currentGame.GameState == "InProgress" && _isMyTurn && _localState == LocalGameState.Idle))
                {
                    if (roundFinished)
                    {
                        OnRoundEnd();
                    }
                    HandleTurnChange();
                }
                if (newShotFired) HandleNewShot();
                if (newPredictionReceived) HandleNewPrediction();
                break;
            case "Finished":
                HandleGameFinished();
                break;
        }
    }

    /// <summary>
    /// 턴이 바뀌었을 때 호출됩니다.
    /// 내 턴이면 돌을 준비하고 입력을 활성화하며, 상대 턴이면 입력을 비활성화합니다.
    /// </summary>
    private void HandleTurnChange()
    {
        
        // if (_currentGame.TurnNumber >=  4 && _currentGame.RoundNumber < 3) // 게임 턴이 끝나고 다시 시작해야할때, 만약 턴이 8턴(4개씩 양쪽 쏜)이 지난 후라면, 다음라운드
        // {
        //     OnRoundEnd();
        //     if (IsHost()) // 중복실행을 방지하기 위해 방장만 실행하도록 함.
        //     {
        //         ResetGameDatas();
        //     }
        // }
        // else if (_currentGame.TurnNumber >= 4 && _currentGame.RoundNumber >= 3) // 라운드가 종료되고, 3라운드가 지났을때
        // {
        //     
        //     db.Collection("games").Document(gameId).UpdateAsync("GameState", "Finished"); // 여기서 돌개수(턴)으로 게임종료
        // }
        DOVirtual.DelayedCall(0.5f, () => { // 턴시작시 카메라전환에 약간 딜레이
            gameCamControl?.SwitchCamera(START_VIEW_CAM); // 내 턴 시작 시 카메라를 기본 뷰로 전환
        });


        if (_isMyTurn && _localState == LocalGameState.Idle)
        {
            Debug.Log("내 턴 시작. 입력을 준비합니다.");
            _localState = LocalGameState.WaitingForInput;
            inputController?.EnableInput(stoneManager?.SpawnStoneForTurn(_currentGame));
        }
        else if (!_isMyTurn)
        {
            inputController?.DisableInput();
            _localState = LocalGameState.Idle;
        }
    }

    /// <summary>
    /// 새로운 샷 정보가 Firestore에 올라왔을 때 호출됩니다.
    /// 내 샷인지 상대 샷인지 구분하여 시뮬레이션을 수행합니다.
    /// </summary>
    private void HandleNewShot()
    {
        if (_currentGame.LastShot == null) return;

        if (_currentGame.LastShot.PlayerId == myUserId && _localState == LocalGameState.WaitingForInput)
        {
            // 이성준 수정
            // _localState = LocalGameState.SimulatingMyShot;
            // Debug.Log($"내 샷(ID: {_currentGame.StonesUsed[myUserId] - 1}) 시뮬레이션 시작.");
            // Time.timeScale = 1.0f;
            // Time.fixedDeltaTime = initialFixedDeltaTime / timeMultiplier;
            //
            // int stoneIdToLaunch = _currentGame.StonesUsed[myUserId] - 1;
            // stoneManager?.LaunchStone(_currentGame.LastShot, stoneIdToLaunch);
        }
        else if (_currentGame.LastShot.PlayerId != myUserId && _localState == LocalGameState.Idle)
        {
            _localState = LocalGameState.SimulatingOpponentShot;

            gameCamControl?.SwitchCamera(SIMULATING_VIEW_CAM); // 시뮬레이션 시작 카메라로 전환

            stoneManager?.SpawnStoneForTurn(_currentGame);
            int stoneIdToLaunch =
                stoneManager.myTeam == StoneForceController_Firebase.Team.A ? stoneManager.bShotCount : stoneManager.aShotCount;
            Debug.Log($"상대 샷(ID: {stoneIdToLaunch}) 시뮬레이션 시작.");
            //float simulationSpeed = (_currentGame.TurnNumber > 1) ? 2.0f : timeMultiplier;

            Time.timeScale = timeMultiplier;
            Time.fixedDeltaTime = initialFixedDeltaTime / 2f;
            Rigidbody rb = stoneManager.GetDonutToLaunch(stoneIdToLaunch).GetComponent<Rigidbody>();
            inputController.SimulateStone(rb, _currentGame.LastShot, stoneIdToLaunch);
            //stoneManager?.LaunchStone(_currentGame.LastShot, stoneIdToLaunch);
        }
    }

    /// <summary>
    /// 새로운 예측 결과가 Firestore에 올라왔을 때 호출됩니다.
    /// 예측 결과를 바로 처리하거나 캐시합니다.
    /// </summary>
    private void HandleNewPrediction()
    {
        if (_localState == LocalGameState.WaitingForPrediction)
        {
            ProcessPrediction(_currentGame.PredictedResult);
        }
        else
        {
            Debug.Log("예측 결과가 일찍 도착하여 캐시합니다.");
            _cachedPrediction = _currentGame.PredictedResult;
        }
    }

    /// <summary>
    /// 예측 결과를 처리하는 메서드입니다.
    /// 돌 위치를 동기화하고 다음 턴으로 넘깁니다.
    /// </summary>
    private void ProcessPrediction(PredictedResult result)
    {
        Debug.Log($"{result.PredictingPlayerId}로부터 받은 예측 결과 처리.");
        stoneManager?.SyncPositions(result.FinalStonePositions);

        DOVirtual.DelayedCall(1f, () =>
        {
            // 9턴이 끝나면 게임 종료 상태로 전환 (5개씩 돌 던지면 끝) , 0턴부터 시작이라 ..
            if (_currentGame.TurnNumber >= 1 )
            {
                ResetGameDatas();
                //db.Collection("games").Document(gameId).UpdateAsync("GameState", "Finished"); // 여기서 돌개수(턴)으로 게임종료
            }
            else
            {
                string nextPlayerId = GetNextPlayerId();
                var updates = new Dictionary<string, object>
                {
                    { "CurrentTurnPlayerId", nextPlayerId },
                    { "TurnNumber", FieldValue.Increment(1) }
                };
                db.Collection("games").Document(gameId).UpdateAsync(updates);
            }
            
            
            
            
            _localState = LocalGameState.Idle;
            _cachedPrediction = null;
        });
    }

    /// <summary>
    /// 게임 종료 상태가 감지되었을 때 호출됩니다.
    /// </summary>
    private void HandleGameFinished()
    {
        // 리스너를 즉시 중지하여 추가 데이터 변경 감지를 막습니다.
        gameListener?.Stop();
        gameListener = null;

        Debug.Log("게임 종료! 3초 후 메뉴 씬으로 돌아갑니다.");

        // 호스트인 경우에만 DB 문서를 정리합니다.
        if (IsHost())
        {
            CleanupGameDocuments();
        }

        // 3초 후에 모든 플레이어를 메뉴 씬으로 보냅니다.
        DOVirtual.DelayedCall(3f, () =>
        {
            SceneLoader.Instance.LoadLocal(GameManager.Instance.menuSceneName); // 메뉴씬으로 이동
        });
    }

    /// <summary>
    /// 호스트 플레이어가 게임 관련 DB 문서를 삭제합니다.
    /// </summary>
    private async void CleanupGameDocuments()
    {
        Debug.Log("호스트로서 게임 및 룸 문서를 정리합니다.");
        if (!string.IsNullOrEmpty(gameId))
        {
            await db.Collection("games").Document(gameId).DeleteAsync();
        }
        if (!string.IsNullOrEmpty(roomId))
        {
            await db.Collection("rooms").Document(roomId).DeleteAsync();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 돌 조작 스크립트에서 샷이 확정되었을 때 호출됩니다.
    /// 샷 데이터를 Firebase에 전송하고 입력을 비활성화합니다.
    /// </summary>
    public void SubmitShot(LastShot shotData)
    {
        shotData.PlayerId = myUserId;
        shotData.Timestamp = Timestamp.GetCurrentTimestamp();
        int count = stoneManager.myTeam == StoneForceController_Firebase.Team.A
            ? stoneManager.aShotCount
            : stoneManager.bShotCount;


        var updates = new Dictionary<string, object>
        {
            { "LastShot", shotData },
            { $"DonutsIndex.{myUserId}", count } // 발사 횟수 올림
        };

        Debug.Log($"SubmitShot.count = {count}");

        db.Collection("games").Document(gameId).UpdateAsync(updates);
        inputController?.DisableInput();
    }

    /// <summary>
    /// 플레이어가 중간에 게임을 나갈 때 호출됩니다.
    /// </summary>
    public void LeaveGame()
    {
        // 게임 상태를 "Finished"로 설정하여 모든 플레이어가 게임을 종료하도록 합니다.
        if (!string.IsNullOrEmpty(gameId))
        {
            db.Collection("games").Document(gameId).UpdateAsync("GameState", "Finished");
        }
    }
    
    public void ChangeFixedDeltaTime()
    {
        Time.fixedDeltaTime = initialFixedDeltaTime / 2f;
    }

    /// <summary>
    /// 돌 시뮬레이션이 완료되면 호출됩니다.
    /// 예측 결과를 전송하거나, 상대의 예측 결과를 기다립니다.
    /// </summary>
    public void OnSimulationComplete(List<StonePosition> finalPositions)
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = initialFixedDeltaTime;
        Debug.Log("시뮬레이션 완료.");

        //시뮬레이션 완료 후 딜레이주기
        DOVirtual.DelayedCall(1.5f, () =>
        {
            gameCamControl?.SwitchCamera(START_VIEW_CAM); // 시뮬레이션 완료 후 시점 전환

            if (_localState == LocalGameState.SimulatingOpponentShot)
            {
                Debug.Log("예측 결과를 서버에 전송합니다.");
                PredictedResult result = new PredictedResult
                {
                    PredictingPlayerId = myUserId,
                    TurnNumber = _currentGame.TurnNumber,
                    FinalStonePositions = finalPositions
                };
                db.Collection("games").Document(gameId).UpdateAsync("PredictedResult", result);
                _localState = LocalGameState.Idle;
                if (_currentGame.TurnNumber >= 1)
                {
                    
                }
            }
            else if (_localState == LocalGameState.SimulatingMyShot)
            {
                _localState = LocalGameState.WaitingForPrediction;
                Debug.Log("내 샷 시뮬레이션 완료. 상대방의 예측 결과를 기다립니다.");

                if (_cachedPrediction != null && _cachedPrediction.TurnNumber == _currentGame.TurnNumber)
                {
                    Debug.Log("캐시된 예측 결과를 즉시 처리합니다.");
                    ProcessPrediction(_cachedPrediction);
                }
            }
        });
    }

    public void OnRoundEnd()//이번 라운드가 끝났을때.
    {
        StoneForceController_Firebase.Team winner;
        int score;
        stoneManager.CalculateScore(out winner, out score);
        stoneManager.ClearOldDonutsInNewRound();
        if (winner == StoneForceController_Firebase.Team.A)
        {
            aTeamScore += score;
        }
        else if (winner == StoneForceController_Firebase.Team.B)
        {
            bTeamScore += score;
        }
        else
        {
            Debug.Log("무승부");
        }

        roundUpdated = true;
        Debug.Log($"승리팀 : {winner}, 점수 : {score}");
        //받아온 리턴값으로 여기서 결과를 처리한다.
    }

    private void ResetGameDatas()
    {
        var updates = new Dictionary<string, object>
        {
            //{ "CurrentTurnPlayerId", nextPlayerId },
            { "TurnNumber", 0},
            { $"DonutsIndex.{_currentGame.PlayerIds[0]}", 0 },
            { $"DonutsIndex.{_currentGame.PlayerIds[1]}", 0 },
            { "RoundNumber", FieldValue.Increment(1) },
            
        };
        db.Collection("games").Document(gameId).UpdateAsync(updates);
        
        PredictedResult result = new PredictedResult
        {
            PredictingPlayerId = myUserId,
            TurnNumber = 0,
            FinalStonePositions =  new List<StonePosition>()
        };
        db.Collection("games").Document(gameId).UpdateAsync("PredictedResult", result);
    }
    
    #endregion

    #region Utilities
    /// <summary>
    /// 현재 사용자가 호스트인지 확인합니다.
    /// </summary>
    private bool IsHost() => _currentGame?.PlayerIds[0] == myUserId;

    /// <summary>
    /// 다음 턴을 진행할 플레이어의 ID를 반환합니다.
    /// </summary>
    private string GetNextPlayerId() => _currentGame.PlayerIds.FirstOrDefault(id => id != myUserId);
    #endregion

    #region Return Variables

    public int GetCurrentStoneId()
    {
        return _currentGame.DonutsIndex[myUserId];
    }
    #endregion

    #region Change Private Variables

    public void ChangeLocalStateToSimulatingMyShot()
    {
        _localState = LocalGameState.SimulatingMyShot;

        // 방금 쏜 돌을 추적하도록 카메라 설정
        var stoneToFollow = stoneManager?.GetCurrentTurnStone();
        if (stoneToFollow != null)
        {
            gameCamControl?.SwitchCamera(FOLLOW_STONE_CAM2, stoneToFollow.transform, stoneToFollow.transform);

        }
    }
    public void ChangeCameraRelease()
    {
        var stoneToFollow = stoneManager?.GetCurrentTurnStone();

        gameCamControl?.SwitchCamera(FOLLOW_STONE_CAM1, stoneToFollow.transform, stoneToFollow.transform);
    }

    #endregion

}
