using DG.Tweening; // DOTween 애니메이션 라이브러리를 사용하기 위해 필요합니다.
using Firebase.Firestore; // Firebase Firestore 기능을 사용하기 위해 필요합니다.
using System;
using System.Collections.Generic; // 리스트나 딕셔너리 같은 자료구조를 사용하기 위해 필요합니다.
using System.Linq; // 리스트에서 데이터를 쉽게 찾거나 걸러낼 때 사용합니다.
using UnityEngine; // Unity 엔진의 기능을 사용하기 위해 필요합니다.
using UnityEngine.PlayerLoop;

/// <summary>
/// 이 스크립트는 컬링 게임의 전체적인 흐름(상태)을 관리하는 중요한 역할을 합니다.
/// Firebase Firestore와 연동하여 게임의 상태를 실시간으로 업데이트하고,
/// 플레이어의 행동(샷 발사, 예측 결과 전송 등)에 따라 게임을 진행합니다.
/// </summary>
public class FirebaseGameManager : MonoBehaviour
{
    public static FirebaseGameManager Instance { get; private set; }

    // 플레이어 프로필 로딩이 완료되었을 때 발생하는 이벤트
    public event Action OnProfilesLoaded;

    // --- 게임의 현재 상태를 나타내는 변수 ---
    // 게임이 현재 어떤 단계에 있는지를 알려주는 역할을 합니다.
    private enum LocalGameState
    {
        Idle, // 아무것도 하지 않고 대기 중인 상태
        WaitingForInput, // 내 턴이 되어 돌 조작을 기다리는 상태

        //PreparingShot, // 시뮬레이션 종료 후 미리 입력을 하는 상태
        SimulatingMyShot, // 내가 쏜 돌이 움직이는 중인 상태
        WaitingForPrediction, // 시뮬레이션이 끝나고 상대방의 예측 결과를 기다리는 상태
        SimulatingOpponentShot, // 상대방이 쏜 돌을 시뮬레이션 중인 상태 (예측자 역할)
        InTimeline // 연출 재생 중임을 나타내는 상태
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
    public bool _isMyTurn { get; private set; } = false; // 현재 내 턴 여부
    private PredictedResult _cachedPrediction = null; // 너무 일찍 도착한 예측 결과를 임시로 보관
    //private LastShot _cachedLastShot = null; // 상대의 발사 정보가 너무 일찍 도착했을경우 임시로 보관

    // --- 다른 스크립트들과의 연결 ---
    [SerializeField] private StoneShoot_Firebase inputController; // 돌 조작(입력)을 담당하는 스크립트
    [SerializeField] private StoneManager stoneManager; // 돌 생성 및 움직임 관리 스크립트
    [SerializeField] private Src_GameCamControl gameCamControl; // 카메라 연출을 제어하는 스크립트
    [SerializeField] private UI_LaunchIndicator_Firebase UI_LaunchIndicator_Firebase; // UI제어 스크립트
    [SerializeField] private DonutSelectionUI donutSelectionUI; // 도넛 선택 UI 스크립트

    // --- 플레이어 프로필 정보 ---
    private Dictionary<string, PlayerProfile> _playerProfiles;

    // --- 카메라 인덱스 상수 --- (카메라 추가하고 명칭도 다시 명명해야함)
    private const int START_VIEW_CAM = 0; // 기본 뷰 카메라
    private const int FOLLOW_STONE_CAM1 = 1; // 돌 따라가는 카메라 (비스듬한 탑뷰)
    private const int FOLLOW_STONE_CAM2 = 2; // 돌 따라가는 카메라 (발사 후)
    private const int FREE_LOOK_CAM = 3; // 점수라인 카메라
    private const int SIMULATING_VIEW_CAM = 4; // 시뮬레이션 카메라

    // --- 게임 플레이 옵션 ---
    [Header("게임 플레이 옵션")] public bool usePreparedShot = true; // 미리 조작한 샷 즉시 발사 기능 사용 여부

    // --- 게임 시스템 변수 ---
    public float timeMultiplier { get; private set; } = 5f; //게임 빨리감기 속도를 결정할 변수, 읽기전용 (기본값 5)

    // --- 게임 연결 상태 변수 ---
    [Header("연결 상태 관리")]
    [SerializeField] private float heartbeatInterval = 10f; // 생존 신호를 보내는 주기 (초)
    [SerializeField] private float disconnectionThreshold = 25f; // 연결 끊김으로 판단하는 임계 시간 (초)

    // --- 게임 내부 변수 ---
    private float initialFixedDeltaTime;
    private bool isFirstTurn = true;
    private int currentRound = 1;
    private Tweener countDownTween = null;
    private bool SuccessfullyShotInTime = false;
    private bool lostTimeToShot = false;
    private Coroutine _heartbeatCoroutine; // 생존 신호 코루틴 참조
    private bool roundDataUpdated = false;

    // --- 공유 가능한 게임 변수 ---
    public int aTeamScore { get; private set; } = 0;
    public int bTeamScore { get; private set; } = 0;
    public int shotsPerRound { get; private set; } = 4; // 이 변수값 조절하여 1라운드당 각각 몇번씩 던지게 할 것인지 조절 가능

    //public bool canShotDonutNow { get; private set; } = false;


    // --- 디버그용 ---
    // 현재 로컬 게임 상태, Firebase 게임 상태 를 외부에 노출, _currentGame이 null일 경우 "N/A"를 반환
    public string CurrentLocalState => _localState.ToString();

    public string CurrentGameState => _currentGame?.GameState ?? "N/A";


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
    async void Start() // Changed to async void
    {
        db = FirebaseFirestore.DefaultInstance;
        gameId = FirebaseMatchmakingManager.CurrentGameId;
        roomId = FirebaseMatchmakingManager.CurrentRoomId; // RoomId 가져오기
        myUserId = FirebaseAuthManager.Instance.UserId;

        initialFixedDeltaTime = Time.fixedDeltaTime;

        // 게임 ID, 사용자 ID, 룸 ID 중 하나라도 없으면 게임을 진행할 수 없습니다.
        if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(myUserId) || string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("Game ID, User ID, or Room ID is missing.");
            return;
        }

        // 룸 문서에서 플레이어 프로필 정보를 가져옵니다.
        DocumentSnapshot roomSnapshot = await db.Collection("rooms").Document(roomId).GetSnapshotAsync();
        if (roomSnapshot.Exists)
        {
            Room roomData = roomSnapshot.ConvertTo<Room>();
            _playerProfiles = roomData.PlayerProfiles;
            Debug.Log($"룸({roomId})에서 플레이어 프로필 정보를 성공적으로 로드했습니다.");
            OnProfilesLoaded?.Invoke(); // 프로필 로딩 완료 이벤트 호출
        }
        else
        {
            Debug.LogError($"룸({roomId}) 문서를 찾을 수 없습니다. 플레이어 프로필 로드 실패.");
            return;
        }

        // 돌 조작 스크립트가 존재한다면 샷 확정 이벤트를 등록합니다.
        if (inputController != null) inputController.OnShotConfirmed += SubmitShot;
        if (donutSelectionUI != null) donutSelectionUI.OnDonutSelectionChanged += OnDonutChanged; //도넛 변경 이벤트

        // Firestore에서 게임 데이터 변화를 감시합니다.
        DocumentReference gameRef = db.Collection("games").Document(gameId);
        gameListener = gameRef.Listen(OnGameSnapshot);

        // Firestore에 나의 준비 상태를 업데이트합니다.
        gameRef.UpdateAsync("ReadyPlayers", FieldValue.ArrayUnion(myUserId));

        // 하트비트 코루틴 시작
        _heartbeatCoroutine = StartCoroutine(UpdateHeartbeat());
    }

    /// <summary>
    /// 게임 오브젝트가 파괴될 때 호출됩니다.
    /// Firestore 감시자와 이벤트 등록을 해제합니다.
    /// </summary>
    void OnDestroy()
    {
        gameListener?.Stop();
        if (inputController != null) inputController.OnShotConfirmed -= SubmitShot;
        if (donutSelectionUI != null) donutSelectionUI.OnDonutSelectionChanged -= OnDonutChanged;

        // 하트비트 코루틴 중지
        if (_heartbeatCoroutine != null)
        {
            StopCoroutine(_heartbeatCoroutine);
        }
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
        //bool turnChanged = !isFirstSnapshot && _currentGame.CurrentTurnPlayerId != newGameData.CurrentTurnPlayerId;
        bool turnChanged = !isFirstSnapshot && _currentGame.TurnNumber != newGameData.TurnNumber;
        bool newShotFired = _currentGame?.LastShot?.Timestamp != newGameData.LastShot?.Timestamp;
        //bool newPredictionReceived = _currentGame?.PredictedResult?.TurnNumber != newGameData.PredictedResult?.TurnNumber;
        bool newPredictionReceived = false;
        if (_currentGame?.PredictedResult?.TurnNumber != newGameData.PredictedResult?.TurnNumber
            && _currentGame?.PredictedResult?.PredictingPlayerId != newGameData.PredictedResult?.PredictingPlayerId)
        {
            newPredictionReceived = true;
        }
        else if (newGameData.PredictedResult?.TurnNumber == 0 &&
                 newGameData.RoundNumber != 1 && newGameData.PredictedResult.FinalStonePositions.Count != 0)
        {
            newPredictionReceived = true;
        }

        bool roundFinished = _currentGame?.RoundNumber != newGameData.RoundNumber;

        _currentGame = newGameData;
        _isMyTurn = _currentGame.CurrentTurnPlayerId == myUserId;

        // 상대방의 연결 끊김을 감지하고 처리합니다.
        if (_currentGame.GameState == "InProgress")
        {
            CheckForDisconnectedPlayer();
        }

        Debug.Log($"[Snapshot] GameState: {_currentGame.GameState}, MyTurn: {_isMyTurn}, LocalState: {_localState}, newShot: {newShotFired}, newPrediction: {newPredictionReceived}");
        Debug.Log($"[Snapshot] MyId: {myUserId}, LastUploaderId: {_currentGame.LastUploaderId}");

        switch (_currentGame.GameState)
        {
            case "Initializing":
                if (_currentGame.ReadyPlayers.Count == 2 && IsHost())
                {
                    var updates = new Dictionary<string, object>
                    {
                        { "GameState", "Timeline" },
                        { "LastUploaderId", myUserId }
                    };
                    db.Collection("games").Document(gameId).UpdateAsync(updates);
                }

                break;
            case "Timeline":
                if (_localState == LocalGameState.Idle)
                {
                    _localState = LocalGameState.InTimeline; // 중복 실행 방지를 위해 InTimeline 상태로 변경
                    
                    UI_LaunchIndicator_Firebase.AllcloseUI(); //게임 UI를 모두 닫아둠

                    // [짧은 타임라인 실행 > InProgress로 상태 변경] 로직을 Action으로 묶어 재사용.
                    Action playShortTimelineAndStartGame = () =>
                    {
                        Debug.Log($"[{_currentGame.RoundNumber} 라운드 시작!] 연출 (2.5초)");
                        gameCamControl?.PlayRoundTimeline(); // 라운드시작 연출

                        DOVirtual.DelayedCall(2.5f, () =>
                        {
                            if (IsHost())
                            {
                                var updates = new Dictionary<string, object>
                                {
                                    { "GameState", "InProgress" },
                                    { "LastShot", null },
                                    { "PredictedResult", null },
                                    { "LastUploaderId", myUserId }
                                };
                                db.Collection("games").Document(gameId).UpdateAsync(updates);
                            }
                        });
                    };

                    if (isFirstTurn)
                    {
                        Debug.Log("[플레이어1 VS 플레이어2] 연출 시작 (8.5초)");
                        isFirstTurn = false;
                        roundDataUpdated = false;
                        gameCamControl?.PlayStartTimeline(); // 긴 타임라인 재생

                        // 8.5초의 연출 대기시간을 기다림
                        DOVirtual.DelayedCall(8.5f, () => {
                            Debug.Log("<<<<<< 첫턴 긴 타임라인 이후 짧은 타임라인 실행 >>>>>>");
                            playShortTimelineAndStartGame(); 
                        });
                    }
                    else // 첫 라운드가 아닐 경우
                    {
                        // 짧은 연출 로직만 실행합니다.

                        Debug.Log("<<<<<< 라운드 변경의 짧은 타임라인 실행 >>>>>>");
                        stoneManager.ClearOldDonutsInNewRound(_currentGame);
                        roundDataUpdated = false;
                        playShortTimelineAndStartGame();
                        
                    }
                }

                break;
            case "InProgress":
                
                // 첫 턴 시작 조건을 _localState == InTimeline일 때도 포함
                if (turnChanged || (_currentGame.GameState == "InProgress" && _isMyTurn &&
                                    (_localState == LocalGameState.Idle || _localState == LocalGameState.InTimeline)))
                {
                    HandleTurnChange();
                }
                //타임라인 상태에서 Idle로 진입 (플레이어2일때)
                else if (!_isMyTurn && _localState == LocalGameState.InTimeline)
                {
                    _localState = LocalGameState.Idle;
                }

                if (newShotFired) HandleNewShot();
                if (newPredictionReceived) HandleNewPrediction();
                break;

            case "RoundChanging":
                

                //if (_localState != LocalGameState.Idle && _localState != LocalGameState.PreparingShot) break; //중복 방지
                if (_localState != LocalGameState.Idle && _localState != LocalGameState.SimulatingOpponentShot)
                    break; //중복 방지
                //if (updatedRoundByMe == true) break;
                Debug.Log($"라운드 {_currentGame.RoundNumber - 1} 종료. 다음 라운드를 준비합니다.");
                _localState = LocalGameState.Idle; // 상태 초기화
                

                
                // if (stoneManager.roundCount != _currentGame.RoundNumber)
                // {
                //     Debug.Log($"stoneManager.roundCount: {stoneManager.roundCount}, _currentGame.RoundNumber: {_currentGame.RoundNumber}");
                //     Debug.Log("OnRoundEnd 호출되었음");
                //     OnRoundEnd();
                // }
                if (roundDataUpdated == false)
                {
                    Debug.Log("OnRoundEnd 호출되었음");
                    OnRoundEnd();
                }


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
        // 턴이 변경될 때마다 UI에 현재 턴 번호를 업데이트합니다.
        UI_LaunchIndicator_Firebase?.UpdateTurnDisplay(_currentGame.TurnNumber);

        // 일반적인 턴 시작일 때만 기본 카메라로 전환합니다.
        // bool isExecutingPreparedShot = usePreparedShot && _isMyTurn && _localState == LocalGameState.PreparingShot;
        // if (!isExecutingPreparedShot)

        if (_isMyTurn && _localState != LocalGameState.WaitingForInput || !_isMyTurn)
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                // 턴시작시 카메라전환에 약간 딜레이
                gameCamControl?.SwitchCamera(START_VIEW_CAM); // 내 턴 시작 시 카메라를 기본 뷰로 전환
            });
        }

        if (_isMyTurn)
        {
            if (_localState == LocalGameState.WaitingForInput)
            {
                bool wasShotExecuted = inputController.ExecutePreparedShot();
                if (wasShotExecuted)
                {
                    // 샷이 발사되었으므로, 중복 실행을 막기 위해 Idle로 전환
                    // (곧 SimulatingMyShot으로 변경될 것임)
                    //_localState = LocalGameState.Idle;
                }
                else
                {
                    //canShotDonutNow = true;
                }
            }
            else if (_localState == LocalGameState.Idle || _localState == LocalGameState.InTimeline)
            {
                Debug.Log("내 턴 시작. 입력을 준비합니다.");
                _localState = LocalGameState.WaitingForInput;

                UI_LaunchIndicator_Firebase.FireShotReadyUI(); //입력준비 UI

                //카운트다운 활성화
                //ControlCountdown(true);

                // UI에서 선택된 도넛 엔트리를 가져옵니다.
                DonutEntry selectedDonut = donutSelectionUI?.GetSelectedDonut();
                if (selectedDonut == null)
                {
                    Debug.LogError("발사할 도넛이 선택되지 않았거나 DonutSelectionUI가 할당되지 않았습니다.");
                    return;
                }

                Rigidbody donutRigid = stoneManager?.SpawnStone(_currentGame, selectedDonut);
                if (donutRigid != null)
                {
                    CountDownStart(10.0f, donutRigid);
                    //inputController?.EnableInput(donutRigid);
                }
                else
                {
                    // 뭔가 작동을 막아주거나 다른 기능을 해야함.           
                    Debug.Log("발사 기회를 모두 소진했을 가능성이 높음");
                }
            }
        }
        else if (!_isMyTurn)
        {
            //canShotDonutNow = false;
            UI_LaunchIndicator_Firebase.IdleUI(); //기본 UI

            SuccessfullyShotInTime = false;
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
        

        if (_currentGame.LastShot.PlayerId != myUserId && _localState == LocalGameState.Idle)
        {
            _localState = LocalGameState.SimulatingOpponentShot;

            // 만약 샷 정보에 발사 시간을 놓쳤음을 나타내는 정보가 있을경우
            if ((_currentGame.LastShot.Force == -999f)
                && (_currentGame.LastShot.Spin == -999f) && (_currentGame.LastShot.Direction["x"] == 0)
                && (_currentGame.LastShot.Direction["y"] == 0) && (_currentGame.LastShot.Direction["z"] == 0))
            {
                Debug.Log($"{_currentGame.LastShot.PlayerId}의 샷은 발사시간을 놓쳤습니다!");
                if (stoneManager.myTeam == StoneForceController_Firebase.Team.A)
                {
                    stoneManager.B_ShotIndexUp(); // 발사 실패로 인해 시뮬은 안하지만, 발사 횟수 자체는 올려줘야 함.
                }
                else
                {
                    stoneManager.A_ShotIndexUp(); // 발사 실패로 인해 시뮬은 안하지만, 발사 횟수 자체는 올려줘야 함.
                }

                //OnSimulationComplete(_currentGame.PredictedResult.FinalStonePositions);
                OnSimulationComplete(stoneManager.GetAllStonePositions());
                return;
            }

            gameCamControl?.SwitchCamera(SIMULATING_VIEW_CAM); // 시뮬레이션 시작 카메라로 전환

            // 상대방의 프로필에서 발사된 도넛 정보를 가져옵니다.
            string opponentId = GetOpponentId();
            PlayerProfile opponentProfile = GetPlayerProfile(opponentId);
            DonutEntry opponentDonut = null;

            if (opponentProfile != null && _currentGame.LastShot != null && !string.IsNullOrEmpty(_currentGame.LastShot.DonutId))
            {
                opponentDonut = opponentProfile.Inventory.donutEntries.FirstOrDefault(d => d.id == _currentGame.LastShot.DonutId);
            }

            if (opponentDonut == null)
            {
                Debug.LogError($"상대방({opponentId})의 발사된 도넛({_currentGame.LastShot?.DonutId}) 정보를 찾을 수 없습니다. 기본 도넛으로 대체합니다.");
                // TODO: 기본 도넛으로 대체하는 로직 추가 (예: 첫 번째 인벤토리 도넛 또는 기본값)
                // 현재는 임시로 첫 번째 도넛을 사용하거나, 에러를 발생시킬 수 있습니다.
                // 여기서는 임시로 상대방의 첫 번째 도넛을 사용하도록 합니다.
                opponentDonut = opponentProfile?.Inventory.donutEntries.FirstOrDefault();
                if (opponentDonut == null)
                {
                    // 정말 아무 도넛도 없으면 에러 처리
                    Debug.LogError("상대방 인벤토리에 도넛이 없습니다. 시뮬레이션을 진행할 수 없습니다.");
                    return;
                }
            }

            stoneManager?.SpawnStone(_currentGame, opponentDonut);
            int stoneIdToLaunch =
                stoneManager.myTeam == StoneForceController_Firebase.Team.A
                    ? stoneManager.bShotIndex
                    : stoneManager.aShotIndex;
            Debug.Log($"상대 샷(ID: {stoneIdToLaunch}) 시뮬레이션 시작.");
            //float simulationSpeed = (_currentGame.TurnNumber > 1) ? 2.0f : timeMultiplier;

            Time.timeScale = timeMultiplier;
            Time.fixedDeltaTime = initialFixedDeltaTime / 2f;
            Rigidbody rb = stoneManager.GetDonutToLaunch(stoneIdToLaunch).GetComponent<Rigidbody>();
            inputController.SimulateStone(rb, _currentGame.LastShot, stoneIdToLaunch);
            //stoneManager?.LaunchStone(_currentGame.LastShot, stoneIdToLaunch);
        }
        // else if (_currentGame.LastShot.PlayerId != myUserId && 
        //          (_localState == LocalGameState.SimulatingMyShot ||  _localState == LocalGameState.WaitingForPrediction))
        // {
        //     Debug.Log("발사 정보가 일찍 도착하여 캐시합니다.");
        //     _cachedLastShot =  _currentGame.LastShot;
        // }
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
        else if (_localState != LocalGameState.InTimeline)
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
            // 8턴(0~7)이 끝나면 라운드 전환 상태로 변경
            // 현재턴이 마지막 턴이고 선공플레이어가 아닐때 (후공 플레이이가 라운드 종료로직을 시작해야할때) true
            if (_currentGame.TurnNumber >= (shotsPerRound * 2) - 1 && !IsStartingPlayer())
            {
                Debug.Log("게임 종료를 위한 계산 시작");
                gameCamControl?.SwitchCamera(FREE_LOOK_CAM);
                // 호스트가 점수를 기반으로 다음 라운드 시작 플레이어를 결정하고 DB를 업데이트합니다.
                stoneManager.CalculateScore(out StoneForceController_Firebase.Team winnerTeam, out int score);
                UpdateScoreInLocal(winnerTeam, score); // 계산된 점수를 로컬상에서 변경
                Debug.Log($"{winnerTeam}: {score}");
                string winnerId = null;
                if (winnerTeam != StoneForceController_Firebase.Team.None)
                {
                    winnerId = (winnerTeam == stoneManager.myTeam) ? myUserId : GetNextPlayerId();
                }

                string nextRoundStarterId;
                if (score == 0 || winnerId == null) // 무승부이거나 승자가 없는 경우
                {
                    // 간단히 플레이어1을 다음 라운드 시작 플레이어로 지정합니다. (기획에 따라 변경 가능)
                    nextRoundStarterId = _currentGame.PlayerIds[0];
                }
                else
                {
                    // 패자(점수를 못 낸 팀)가 다음 라운드를 시작합니다.
                    nextRoundStarterId = _currentGame.PlayerIds.FirstOrDefault(id => id != winnerId);
                }

                //ResetGameDatas(nextRoundStarterId, false);
                
                // 3라운드가 끝났으면 게임 종료, 아니면 지속
                // 2라운드가 끝났지만, 점수차가 5점이상이 나면 3라운드에서 4점을 따라잡더라도 이길수 없으므로 콜드게임 처리
                 if (_currentGame.RoundNumber >= 3 || 
                     (_currentGame.RoundNumber == 2 && Math.Abs(aTeamScore - bTeamScore) >= 5))
                 {
                     ResetGameDatas(nextRoundStarterId, true); // 게임 끝내기 위한 정보들도 전송해야함
                 }
                 else
                 {
                     ResetGameDatas(nextRoundStarterId, false); // 다음 라운드를 위한 정보들 전송
                 }
                
            }
            else // 일반적인 턴에서 다음턴으로 넘겨줌
            {
                string nextPlayerId = GetNextPlayerId();
                var updates = new Dictionary<string, object>
                {
                    { "CurrentTurnPlayerId", nextPlayerId },
                    { "TurnNumber", FieldValue.Increment(1) }, //턴을 1씩 더해줌
                    { "LastUploaderId", myUserId }
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
        if (aTeamScore > bTeamScore)
        {
            if (stoneManager.myTeam == StoneForceController_Firebase.Team.A)
            {
                Debug.Log("승리");
                UI_LaunchIndicator_Firebase.FinishedUI();
            }
            else
            {
                Debug.Log("패배");
                UI_LaunchIndicator_Firebase.FinishedUI();
            }
        }
        else if (bTeamScore > aTeamScore)
        {
            if (stoneManager.myTeam == StoneForceController_Firebase.Team.A)
            {
                Debug.Log("패배");
                UI_LaunchIndicator_Firebase.FinishedUI();
            }
            else
            {
                Debug.Log("승리");
                UI_LaunchIndicator_Firebase.FinishedUI();
            }
        }
        else // 비겼을때 ( 연장전을 이때 시작하거나, 이미 연장전을 해서 이게 없어질 수도 있음 )
        {
            Debug.Log("비김");
            UI_LaunchIndicator_Firebase.FinishedUI();
        }
        
        // 리스너를 즉시 중지하여 추가 데이터 변경 감지를 막습니다.
        gameListener?.Stop();
        gameListener = null;

        Debug.Log("게임 종료! 10초 후 메뉴 씬으로 돌아갑니다.");

        // 호스트인 경우에만 DB 문서를 정리합니다.
        if (IsHost())
        {
            CleanupGameDocuments();
        }

        // 3초 후에 모든 플레이어를 메뉴 씬으로 보냅니다.
        DOVirtual.DelayedCall(10f, () =>
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
        //상태 변경을 바로 해주어 다음 인덱스 도넛이 생성되는 오류 방지
        _localState = LocalGameState.SimulatingMyShot;

        shotData.PlayerId = myUserId;
        shotData.Timestamp = Timestamp.GetCurrentTimestamp();

        // 현재 선택된 도넛을 가져옵니다.
        DonutEntry selectedDonut = donutSelectionUI?.GetSelectedDonut();
        
        // LastShot 데이터에 도넛 ID를 저장합니다.
        shotData.DonutId = selectedDonut?.id;

        // 사용한 도넛을 UI에서 '사용됨'으로 표시하고 다음 도넛을 선택합니다.
        if (donutSelectionUI != null && selectedDonut != null)
        {
            donutSelectionUI.MarkDonutAsUsed(selectedDonut);
        }
        
        int count = stoneManager.myTeam == StoneForceController_Firebase.Team.A
            ? stoneManager.aShotIndex
            : stoneManager.bShotIndex;


        var updates = new Dictionary<string, object>
        {
            { "LastShot", shotData },
            { $"DonutsIndex.{myUserId}", count }, // 발사 횟수 올림
            { "LastUploaderId", myUserId }
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
            var updates = new Dictionary<string, object>
            {
                { "GameState", "Finished" },
                { "LastUploaderId", myUserId }
            };
            db.Collection("games").Document(gameId).UpdateAsync(updates);
        }
    }

    public void ChangeFixedDeltaTime()
    {
        Time.fixedDeltaTime = initialFixedDeltaTime / 2f;
    }

    /// <summary>
    /// 돌 시뮬레이션이 완료되면 호출됩니다.
    /// 예측 결과를 전송하거나, 상대의 예측 결과를 기다립니다.   상대 시뮬 끝나면 내 도넛을 미리 생성하고 발사대기 가능하도록
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
            UI_LaunchIndicator_Firebase.FireShotReadyUI(); // UI켜주기

            if (_localState == LocalGameState.SimulatingOpponentShot)
            {
                Debug.Log("예측 결과를 서버에 전송합니다.");
                PredictedResult result = new PredictedResult
                {
                    PredictingPlayerId = myUserId,
                    TurnNumber = _currentGame.TurnNumber,
                    FinalStonePositions = finalPositions
                };
                var updates = new Dictionary<string, object>
                {
                    { "PredictedResult", result},
                    { "LastUploaderId", myUserId }
                };
                //db.Collection("games").Document(gameId).UpdateAsync("PredictedResult", result);
                db.Collection("games").Document(gameId).UpdateAsync(updates);

                // Idle 상태 대신, 다음 샷을 미리 준비하는 상태로 전환합니다.
                //_localState = LocalGameState.PreparingShot;
                //_localState = LocalGameState.Idle;
                Debug.Log("상대 턴 시뮬레이션 완료. 내 샷을 미리 준비합니다.");
                // 'myUserId'를 명시하여 '나'의 돌을 생성하도록 새 메서드 호출

                // 라운드 끝나면 안만들어지게
                if ((stoneManager.myTeam == StoneForceController_Firebase.Team.A
                    && stoneManager.aShotIndex >= shotsPerRound - 1)
                    || (stoneManager.myTeam == StoneForceController_Firebase.Team.B
                    && stoneManager.bShotIndex >= shotsPerRound - 1))
                {
                    //이미 발사횟수를 모두 소진함
                    Debug.Log("라운드에 발사가능한 횟수가 끝나서 내 턴으로 돌아오지 않습니다");

                }
                else
                {
                    // UI에서 선택된 도넛 엔트리를 가져옵니다.
                    DonutEntry selectedDonut = donutSelectionUI?.GetSelectedDonut();
                    if (selectedDonut == null)
                    {
                        Debug.LogError("발사할 도넛이 선택되지 않았거나 DonutSelectionUI가 할당되지 않았습니다.");
                        return;
                    }

                    Rigidbody donutRigid = stoneManager?.SpawnStone(_currentGame, selectedDonut, myUserId);
                    if (donutRigid != null)
                    {
                        //inputController?.EnableInput(donutRigid);
                        CountDownStart(10f, donutRigid);
                    }
                    else
                    {
                        Debug.Log("아마 발사횟수가 끝났을 가능성이 높음");
                    }
                }

                //inputController?.EnableInput(stoneManager?.SpawnStone(_currentGame, myUserId));
            }
            else if (_localState == LocalGameState.SimulatingMyShot)
            {
                ChangeState_To_WaitingForPrediction();
            }
        });
    }

    public void ChangeState_To_WaitingForPrediction()
    {
        _localState = LocalGameState.WaitingForPrediction;
        Debug.Log("내 샷 시뮬레이션 완료. 상대방의 예측 결과를 기다립니다.");

        if (_cachedPrediction != null && _cachedPrediction.TurnNumber == _currentGame.TurnNumber)
        {
            Debug.Log("캐시된 예측 결과를 즉시 처리합니다.");
            ProcessPrediction(_cachedPrediction);
        }
    }

    public void OnRoundEnd() //이번 라운드가 끝났을때.
    {
        // 라운드 변경 시 턴 UI를 초기화합니다.
        UI_LaunchIndicator_Firebase?.UpdateTurnDisplay(_currentGame.TurnNumber);
        gameCamControl?.SwitchCamera(FREE_LOOK_CAM);
        roundDataUpdated = true;

        stoneManager.CalculateScore(out StoneForceController_Firebase.Team team, out int score);
        aTeamScore = _currentGame.ATeamScore;
        bTeamScore = _currentGame.BTeamScore;
        string nextState;

        DOVirtual.DelayedCall(4f, () =>
        {
            // 라운드 종료 시 플레이어의 도넛 사용 상태를 초기화합니다.
            donutSelectionUI?.ResetDonutUsage();

            if (_currentGame.CurrentTurnPlayerId == "Finished" && _currentGame.RoundStartingPlayerId == "Finished")
            {
                nextState = "Finished";
            }
            else
            {
                nextState = "Timeline";
            }
            var updates = new Dictionary<string, object>
            {
                { "LastUploaderId", myUserId },
                { "GameState", nextState } // 다음 라운드 시작 전, 연출을 위해 Timeline 상태로 전환
            };
            //stoneManager?.ClearOldDonutsInNewRound(_currentGame);
            db.Collection("games").Document(gameId).UpdateAsync(updates);
        });
            
        
        
    }

    public void UpdateScoreInLocal(StoneForceController_Firebase.Team winner, int score)
    {
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

        Debug.Log($"승리팀 : {winner}, 점수 : {score}");
    }

    private void ResetGameDatas(string nextPlayerId, bool isFinished = false) // 다음 라운드 시작 플레이어를 파라미터로 받음
    {
        
        if (isFinished) // 만약 게임이 끝났다면
        {
            nextPlayerId = "Finished"; // 다음 플레이어 이름에 Finished를 적어서 상대방에게 게임 끝남을 알림
        }
        
        
        PredictedResult result = new PredictedResult
        {
            PredictingPlayerId = myUserId,
            TurnNumber = 0,
            FinalStonePositions = new List<StonePosition>()
        };

        currentRound = _currentGame.RoundNumber;
        var updates = new Dictionary<string, object>
        {
            { "CurrentTurnPlayerId", nextPlayerId }, // 다음 라운드 시작 플레이어 업데이트
            { "RoundStartingPlayerId", nextPlayerId },
            { "TurnNumber", 0 },
            { $"DonutsIndex.{_currentGame.PlayerIds[0]}", 0 },
            { $"DonutsIndex.{_currentGame.PlayerIds[1]}", 0 },
            { "RoundNumber", FieldValue.Increment(1) },
            { "ATeamScore", aTeamScore },
            { "BTeamScore", bTeamScore },
            { "PredictedResult", result },
            { "LastUploaderId", myUserId },
            { "GameState", "RoundChanging" } // 다음 라운드 시작 전, 연출을 위해 Timeline 상태로 전환
        };
        currentRound++;
        stoneManager?.RoundCountUp();
        roundDataUpdated = true;
        db.Collection("games").Document(gameId).UpdateAsync(updates);
        UI_LaunchIndicator_Firebase?.UpdateTurnDisplay(_currentGame.TurnNumber);

        //db.Collection("games").Document(gameId).UpdateAsync("PredictedResult", result);

        // DOVirtual.DelayedCall(4f, () =>
        // {
        //     stoneManager?.ClearOldDonutsInNewRound(_currentGame, currentRound);
        //     UI_LaunchIndicator_Firebase?.UpdateTurnDisplay(_currentGame.TurnNumber);
        // });
        
    }

    private void PlayerLostTimeToShotInTime(Rigidbody donutRigid)
    {
        StoneForceController_Firebase sfc = donutRigid.transform.GetComponent<StoneForceController_Firebase>();
        stoneManager.DonutOut(sfc);
        var zeroDict = new Dictionary<string, float>
        {
            { "x", 0 },
            { "y", 0 },
            { "z", 0 }
        };
        LastShot shotData = new LastShot()
        {
            Force = -999f, // 최종 힘
            PlayerId = stoneManager.myUserId,
            Team = stoneManager.myTeam, // 발사하는 팀
            Spin = -999f, // 최종 스핀 값
            Direction = zeroDict, // 발사 방향
            ReleasePosition = zeroDict // 릴리즈 위치
        };
        SubmitShot(shotData);
        _localState = LocalGameState.WaitingForPrediction;
        // DOVirtual.DelayedCall(1.0f, () =>
        // {
        //     ChangeTurn();
        // });
    }

    private void ChangeTurn()
    {
        string nextPlayerId = GetNextPlayerId();
        var updates = new Dictionary<string, object>
        {
            { "CurrentTurnPlayerId", nextPlayerId },
            { "TurnNumber", FieldValue.Increment(1) },
            { "LastUploaderId", myUserId }
        };
        db.Collection("games").Document(gameId).UpdateAsync(updates);
    }

    public void OnShotStepUI() //도넛 엔트리만 꺼주는 UI호출
    {
        UI_LaunchIndicator_Firebase.FireShotReadyTwoUI();
    }

    public void OnIdleUI()
    {
        UI_LaunchIndicator_Firebase.IdleUI();
    }

    #endregion

    #region 턴 관리

    /// <summary>
    /// 현재 사용자가 호스트인지 확인합니다.
    /// </summary>
    private bool IsHost() => _currentGame?.PlayerIds[0] == myUserId;

    private bool IsStartingPlayer()
    {
        return _currentGame.RoundStartingPlayerId == myUserId;
    }

    /// <summary>
    /// 다음 턴을 진행할 플레이어의 ID를 반환합니다.
    /// </summary>
    private string GetNextPlayerId() // 다음 플레이어 버그나서 수정 ( 11.09 )
    {
        // 현재 턴의 플레이어 ID를 가져옴
        string currentPlayerId = _currentGame.CurrentTurnPlayerId;

        // 지금 턴의 플레이어가 아닌 플레이어를 찾아 다음턴 플레이어로 반환
        return _currentGame.PlayerIds.FirstOrDefault(id => id != currentPlayerId);
    }

    #endregion

    #region Return Variables

    public int GetCurrentStoneId()
    {
        return _currentGame.DonutsIndex[myUserId];
    }

    #endregion

    #region 도넛 교체 관련

    /// <summary>
    /// DonutSelectionUI에서 다른 도넛을 선택했을 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
        private void OnDonutChanged(DonutEntry newDonut)
        {
            // 입력대기 상태면 도넛을 교체 할 수 있게
            if (_localState == LocalGameState.WaitingForInput)
            {
                Debug.Log($"선택한 도넛이 {newDonut.id}(으)로 변경되어 교체합니다.");
                ReplaceCurrentStone(newDonut);
            }
        }

    /// <summary>
    /// 현재 턴에 생성된 돌을 파괴하고 새로운 돌로 교체합니다.
    /// </summary>
    private void ReplaceCurrentStone(DonutEntry newDonut)
    {
        if (stoneManager == null || inputController == null) return;

        // 1. 현재 돌 가져오기 및 파괴
        StoneForceController_Firebase currentStone = stoneManager.GetCurrentTurnStone();
        if (currentStone != null)
        {
            // StoneManager의 관리 리스트에서 제거하지 않고 순수하게 게임 오브젝트만 파괴합니다.
            // SpawnStone에서 shotIndex를 기준으로 다시 리스트에 할당할 것이기 때문입니다.
            Destroy(currentStone.gameObject);
        }
    
        // 2. shotIndex를 1 감소시켜 SpawnStone에서 올바른 인덱스를 다시 사용하도록 함
        // (SpawnStone 내부에서 shotIndex가 1 증가하기 때문)
        if (stoneManager.myTeam == StoneForceController_Firebase.Team.A)
        {
            stoneManager.A_ShotIndexDown();
        }
        else
        {
            stoneManager.B_ShotIndexDown();
        }

        // 3. 새로운 돌 생성 및 제어권 부여
        Rigidbody newDonutRigid = stoneManager.SpawnStone(_currentGame, newDonut, myUserId);
        if (newDonutRigid != null)
        {
            inputController.EnableInput(newDonutRigid);
        }
    }

    #endregion


    #region 상태전환용 메서드

    public void ChangeLocalStateToSimulatingMyShot()
    {
        _localState = LocalGameState.SimulatingMyShot;

        var stoneToFollow = stoneManager?.GetCurrentTurnStone();
        if (stoneToFollow != null)
        {
            Debug.Log("카메라 전환을 시도합니다."); // 로그 추가
            gameCamControl?.SwitchCamera(FOLLOW_STONE_CAM2, stoneToFollow.transform, stoneToFollow.transform);
        }
        else
        {
            Debug.LogWarning("카메라가 따라갈 돌을 찾지 못했습니다."); // 경고 로그 추가
        }
    }

    public void ChangeCameraRelease()
    {
        var stoneToFollow = stoneManager?.GetCurrentTurnStone();

        gameCamControl?.SwitchCamera(FOLLOW_STONE_CAM1, stoneToFollow.transform, stoneToFollow.transform);
    }

    public void ControlCountdown(bool con) //카운트다운 컨트롤
    {
        UI_LaunchIndicator_Firebase.SetCountDown(con);
    }

    public void CountDownStart(float time, Rigidbody donutRigid)
    {
        UI_LaunchIndicator_Firebase?.UpdateTurnDisplay(_currentGame.TurnNumber + 1); // 턴 UI업데이트 (+1을 해주어 선반영)

        _localState = LocalGameState.WaitingForInput;
        inputController?.EnableInput(donutRigid);
        int _remainingTime = (int)time;
        ControlCountdown(true);
        countDownTween = DOTween.To(
                // 1. Getter: 시작 값 (10.0f)
                () => time,
                // 2. Setter: 변화하는 값을 처리하는 람다 함수
                (float x) =>
                {
                    // 변화하는 값 x를 정수로 변환하여 변수에 저장
                    // Mathf.CeilToInt를 사용하여 9.9초가 되어도 '10'으로 표시되게 함 (올림 처리)
                    _remainingTime = Mathf.CeilToInt(x);

                    // 디버깅 용 (매 프레임 호출됨)
                    //Debug.Log($"남은 시간: {_remainingTime}초");
                },
                // 3. 목표 값 (0.0f)
                0.0f,
                // 4. 지속 시간 (10초)
                time
            )
            // 카운트다운이 0이 된 후 실행될 콜백 (선택 사항)
            .OnComplete(() =>
            {
                Debug.Log("카운트다운 종료!");
                //canShotDonutNow = false;
                inputController?.DisableInput();
                if (SuccessfullyShotInTime == false)
                {
                    //lostTimeToShot = true;
                    PlayerLostTimeToShotInTime(donutRigid);
                }

                //_localState = LocalGameState.Idle;
                ControlCountdown(false);
                countDownTween = null;
            });
    }

    public void CountDownStop()
    {
        countDownTween?.Kill();
        countDownTween = null;
        ControlCountdown(false);
    }

    public void Change_SuccessfullyShotInTime_To_True()
    {
        SuccessfullyShotInTime = true;
    }

    #endregion


    /// <summary>
    /// 주기적으로 자신의 생존 신호(하트비트)를 Firestore에 업데이트합니다.
    /// </summary>
    private System.Collections.IEnumerator UpdateHeartbeat()
    {
        DocumentReference gameRef = db.Collection("games").Document(gameId);
        while (true)
        {
            // PlayerHeartbeats 필드 내의 현재 플레이어 ID에 현재 타임스탬프를 업데이트
            string heartbeatPath = $"PlayerHeartbeats.{myUserId}";
            var updates = new Dictionary<string, object> {
                { heartbeatPath, Timestamp.GetCurrentTimestamp()}
            };
            gameRef.UpdateAsync(updates);

            // 정해진 주기만큼 대기
            yield return new WaitForSeconds(heartbeatInterval);
        }
    }

    /// <summary>
    /// 상대 플레이어의 연결 끊김을 확인하고, 끊겼다면 게임을 종료시킵니다.
    /// </summary>
    private void CheckForDisconnectedPlayer()
    {
        if (_currentGame.PlayerHeartbeats == null) return;

        // 상대 플레이어 ID 찾기
        string opponentId = _currentGame.PlayerIds.FirstOrDefault(id => id != myUserId);
        if (string.IsNullOrEmpty(opponentId)) return;

        // 상대방의 하트비트가 있는지, 있다면 마지막 시간이 언제인지 확인
        if (_currentGame.PlayerHeartbeats.TryGetValue(opponentId, out Timestamp lastHeartbeat))
        {
            // 현재 시간과 마지막 하트비트 시간의 차이 계산
            TimeSpan timeSinceLastHeartbeat = Timestamp.GetCurrentTimestamp().ToDateTime() - lastHeartbeat.ToDateTime();

            // 시간 차이가 임계값을 넘었으면 연결이 끊긴 것으로 간주
            if (timeSinceLastHeartbeat.TotalSeconds > disconnectionThreshold)
            {
                Debug.LogWarning($"상대방({opponentId})의 연결이 끊긴 것으로 보입니다. 게임을 종료합니다.");
                ForfeitGame(opponentId, "Opponent disconnected");
            }
        }
        else if (_currentGame.TurnNumber > 0) // 게임이 시작되었는데도 상대 하트비트가 없으면 문제로 간주
        {
            // 첫 턴 이후에도 하트비트가 없다면 연결이 끊겼다고 판단
            Debug.LogWarning($"상대방({opponentId})의 하트비트가 감지되지 않습니다. 게임을 종료합니다.");
            ForfeitGame(opponentId, "Opponent never connected");
        }
    }



    /// <summary>
    /// 특정 플레이어의 패배로 게임을 종료시킵니다.
    /// </summary>
    /// <param name="loserId">패배한 플레이어의 ID</param>
    /// <param name="reason">게임 종료 사유</param>
    private void ForfeitGame(string loserId, string reason)
    {
        // 이미 게임이 끝났으면 중복 실행 방지
        if (_currentGame.GameState == "Finished") return;

        string winnerId = _currentGame.PlayerIds.FirstOrDefault(id => id != loserId);

        var updates = new Dictionary<string, object> {
                    { "GameState", "Finished" },
                    { "WinnerId", winnerId },
                    { "FinishReason", reason },
                    { "LastUploaderId", myUserId }
                };

        db.Collection("games").Document(gameId).UpdateAsync(updates);
    }

    /// <summary>
    /// 주어진 userId에 해당하는 플레이어의 프로필 정보를 반환합니다.
    /// </summary>
    public PlayerProfile GetPlayerProfile(string userId)
    {
        if (_playerProfiles == null || string.IsNullOrEmpty(userId) || !_playerProfiles.ContainsKey(userId))
        {
            Debug.LogWarning($"요청한 플레이어({userId})의 프로필을 찾을 수 없습니다.");
            return null;
        }
        return _playerProfiles[userId];
    }

    /// <summary>
    /// 상대방 플레이어의 ID를 반환합니다.
    /// </summary>
    public string GetOpponentId()
    {
        // _currentGame이 아직 로드되지 않았을 수 있으므로, _playerProfiles에서 먼저 찾아봅니다.
        if (_playerProfiles != null && _playerProfiles.Count >= 2)
        {
            return _playerProfiles.Keys.FirstOrDefault(id => id != myUserId);
        }
        // _currentGame이 로드된 후에는 여기서 찾습니다.
        if (_currentGame != null && _currentGame.PlayerIds != null)
        {
            return _currentGame.PlayerIds.FirstOrDefault(id => id != myUserId);
        }
        return null;
    }

    /// <summary>
    /// 플레이어 프로필 정보가 성공적으로 로드되었는지 확인합니다.
    /// </summary>
    public bool HasLoadedProfiles()
    {
        return _playerProfiles != null && _playerProfiles.Count >= 2;
    }
}
