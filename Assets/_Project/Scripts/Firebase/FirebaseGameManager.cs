using DG.Tweening; // DOTween 애니메이션 라이브러리를 사용하기 위해 필요합니다.
using Firebase.Firestore; // Firebase Firestore 기능을 사용하기 위해 필요합니다.
using System;
using System.Collections;
using System.Collections.Generic; // 리스트나 딕셔너리 같은 자료구조를 사용하기 위해 필요합니다.
using System.Linq; // 리스트에서 데이터를 쉽게 찾거나 걸러낼 때 사용합니다.
using UnityEngine; // Unity 엔진의 기능을 사용하기 위해 필요합니다.

/// <summary>
/// 이 스크립트는 컬링 게임의 전체적인 흐름(상태)을 관리하는 중요한 역할을 합니다.
/// Firebase Firestore와 연동하여 게임의 상태를 실시간으로 업데이트하고,
/// 플레이어의 행동(샷 발사, 예측 결과 전송 등)에 따라 게임을 진행합니다.
/// </summary>
public class FirebaseGameManager : MonoBehaviour
{
    // 게임 결과를 나타내는 열거형
    public enum GameOutcome
    {
        Win,
        Lose,
        Draw
    }

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
        InTimeline, // 연출 재생 중임을 나타내는 상태
        WaitingForRoundChange, //라운드 변경을 원활하게 하기 위해 대기하는 상태
        FinishedGame // 서버로부터 종료 요청을 수신받고 종료처리를 하는 상태
    }

    private LocalGameState _localState = LocalGameState.Idle; // 현재 게임의 로컬 상태

    // --- Firebase 관련 필드 ---
    private FirebaseFirestore db; // Firebase Firestore 데이터베이스 접근 객체
    private ListenerRegistration gameListener; // Firestore 데이터 변경 감시자
    private string gameId; // 현재 진행 중인 게임의 고유 ID
    private string roomId; // 현재 진행 중인 게임의 룸 ID
    private string myUserId; // 현재 플레이 중인 나의 고유 ID
    public StoneForceController_Firebase.Team MyTeam { get; private set; } = StoneForceController_Firebase.Team.None;

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

    public StoneManager StoneManagerInGM => stoneManager;
    
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
    public float timeMultiplier { get; private set; } = 4f; //게임 빨리감기 속도를 결정할 변수, 읽기전용 (기본값 5)
    public float fixedTimeMultiplier { get; private set; } = 0.005f; // 기존 0.0025(8배)

    // --- 게임 연결 상태 변수 ---
    [Header("연결 상태 관리")]
    [SerializeField] private float heartbeatInterval = 10f; // 생존 신호를 보내는 주기 (초)
    [SerializeField] private float disconnectionThreshold = 25f; // 연결 끊김으로 판단하는 임계 시간 (초)

    // --- SO 연결 ---\
    [SerializeField] private EffectSO effectSo;
    
    private Coroutine waitInitializingCoroutine;

    public EffectSO EffectSoObject
    {
        get { return effectSo; }
    }

    // --- 게임 내부 변수 ---
    private float initialFixedDeltaTime;
    private bool isFirstTurn = true;
    private int currentRound = 1;
    private Tweener countDownTween = null;
    private bool SuccessfullyShotInTime = false;
    private bool lostTimeToShot = false;
    private Rigidbody _currentTurnDonutRigid;
    private Coroutine _heartbeatCoroutine; // 생존 신호 코루틴 참조
    private bool roundDataUpdated = false;
    private bool _justTimedOut = false; // 마지막 턴이 타임아웃으로 실패했는지 여부
    private bool penaltyApplied = false; // 게임 시작 시의 페널티가 적용되었는지 확인하는 플래그

    // --- 공유 가능한 게임 변수 ---
    public int aTeamScore { get; private set; } = 0;
    public int bTeamScore { get; private set; } = 0;
    public int shotsPerRound { get; private set; } = 4; // 이 변수값 조절하여 1라운드당 각각 몇번씩 던지게 할 것인지 조절 가능

    //public bool canShotDonutNow { get; private set; } = false;


    // --- 디버그용 ---
    // 현재 로컬 게임 상태, Firebase 게임 상태 를 외부에 노출, _currentGame이 null일 경우 "N/A"를 반환
    public string CurrentLocalState => _localState.ToString();

    public string CurrentGameState => _currentGame?.GameState ?? "N/A";
    [SerializeField] private GameObject blackOutInObj;


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
        // 새 게임 시작 시 StoneManager의 상태를 깨끗하게 초기화합니다.
        if (stoneManager != null)
        {
            stoneManager.ResetForNewGame();
        }

        db = FirebaseFirestore.DefaultInstance;
        gameId = FirebaseMatchmakingManager.CurrentGameId;
        roomId = FirebaseMatchmakingManager.CurrentRoomId; // RoomId 가져오기
        myUserId = FirebaseAuthManager.Instance.UserId;

        initialFixedDeltaTime = 0.02f;

        //Time.fixedDeltaTime /= timeMultiplier;
        //Time.fixedDeltaTime = fixedTimeMultiplier;

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
        if (_localState == LocalGameState.FinishedGame)
        {
            //Debug.Log("로컬 상태가 FinishedGame이므로 스냅샷 처리를 무시합니다.");
            return;
        }

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
        // bool newPredictionReceived = false;
        // if (_currentGame?.PredictedResult?.TurnNumber != newGameData.PredictedResult?.TurnNumber
        //     && _currentGame?.PredictedResult?.PredictingPlayerId != newGameData.PredictedResult?.PredictingPlayerId)
        // {
        //     newPredictionReceived = true;
        // }
        // else if (newGameData.PredictedResult?.TurnNumber == 0 &&
        //          newGameData.RoundNumber != 1 && newGameData.PredictedResult.FinalStonePositions.Count != 0)
        // {
        //     newPredictionReceived = true;
        // }
        // else if (newGameData.PredictedResult == null && newGameData.RoundNumber != 1)
        // {
        //     newPredictionReceived = false;
        // }
        bool newPredictionReceived = false;
        if (newGameData.PredictedResult != null)
        {
            if (_currentGame.PredictedResult == null ||
                _currentGame.PredictedResult.TurnNumber != newGameData.PredictedResult.TurnNumber ||
                (_currentGame.PredictedResult.TurnNumber == newGameData.PredictedResult.TurnNumber &&
                 _currentGame.PredictedResult.FinalStonePositions.Count !=
                 newGameData.PredictedResult.FinalStonePositions.Count))
            {
                newPredictionReceived = true;
            }
        }

        bool roundFinished = _currentGame?.RoundNumber != newGameData.RoundNumber;

        _currentGame = newGameData;
        _isMyTurn = _currentGame.CurrentTurnPlayerId == myUserId;

        if (_currentGame.PlayerIds != null && _currentGame.PlayerIds.Count > 1)
        {
            MyTeam = (_currentGame.PlayerIds[0] == myUserId) ? StoneForceController_Firebase.Team.A : StoneForceController_Firebase.Team.B;
        }

        // 상대방의 연결 끊김을 감지하고 처리합니다.
        if (_currentGame.GameState == "InProgress")
        {
            CheckForDisconnectedPlayer();
        }

        //Debug.Log($"[Snapshot] GameState: {_currentGame.GameState}, MyTurn: {_isMyTurn}, LocalState: {_localState}, newShot: {newShotFired}, newPrediction: {newPredictionReceived}");
        //Debug.Log($"[Snapshot] MyId: {myUserId}, LastUploaderId: {_currentGame.LastUploaderId}");

        switch (_currentGame.GameState)
        {
            case "Initializing":
                Debug.Log("Initi1alizing 들어옴");
                var updates = new Dictionary<string, object>
                {
                    { "GameState", "Timeline" },
                    { "LastUploaderId", myUserId }
                };
                db.Collection("games").Document(gameId).UpdateAsync(updates);
                
                if (waitInitializingCoroutine == null)
                {
                    waitInitializingCoroutine = StartCoroutine(WaitInitializing());
                }
                
                break;
            case "Timeline":
                if (_localState == LocalGameState.Idle)
                {
                    _localState = LocalGameState.InTimeline; // 중복 실행 방지를 위해 InTimeline 상태로 변경
                    UI_LaunchIndicator_Firebase.AllcloseUI(); //게임 UI를 모두 닫아둠

                    // isFirstTurn은 게임 전체의 첫 턴일 때만 true입니다.
                    // 따라서, 새 라운드가 시작될 때(!isFirstTurn) UI를 초기화합니다.
                    if (!isFirstTurn)
                    {
                        UI_LaunchIndicator_Firebase?.UpdateTurnDisplay(0); // 턴 UI를 1로 리셋
                    }


                    // [짧은 타임라인 실행 > InProgress로 상태 변경] 로직을 Action으로 묶어 재사용.
                    Action playShortTimelineAndStartGame = () =>
                    {
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
                        ApplyInitialPenalty(); // 로컬 페널티 적용

                        Debug.Log("[플레이어1 VS 플레이어2] 연출 시작 (8.5초)");
                        isFirstTurn = false;
                        roundDataUpdated = false;
                        gameCamControl?.PlayStartTimeline(); // 긴 타임라인 재생
                        SoundManager.Instance.appearEntry();

                        // 사운드 출력 테스트용 +++
                        DOVirtual.DelayedCall(2f, () =>
                        {
                            //SoundManager.Instance.selectDonut();
                        });

                        DOVirtual.DelayedCall(7.5f, () =>
                        {
                            SoundManager.Instance.appearVS();
                        });
                        // 8.5초의 연출 대기시간을 기다림
                        DOVirtual.DelayedCall(8.5f, () =>
                        {

                            playShortTimelineAndStartGame();

                            if (_currentGame.RoundStartingPlayerId != myUserId)
                            {
                                // 후공일때 상대방을 기다리는중 UI
                                DOVirtual.DelayedCall(2.5f, () =>
                                {
                                    UI_LaunchIndicator_Firebase.WatingThrowUI();
                                }).SetId("WatingThrowUI");
                            }
                        });
                    }
                    else // 첫 라운드가 아닐 경우
                    {
                        // 짧은 연출 로직만 실행합니다.
                        stoneManager.ClearOldDonutsInNewRound(_currentGame);
                        gameCamControl.SwitchCamera(START_VIEW_CAM);
                        _cachedPrediction = null;
                        roundDataUpdated = false;
                        playShortTimelineAndStartGame();

                        if (_currentGame.RoundStartingPlayerId != myUserId)
                        {
                            // 후공일때 상대방을 기다리는중 UI
                            DOVirtual.DelayedCall(2.5f, () =>
                            {
                                UI_LaunchIndicator_Firebase.WatingThrowUI();
                            }).SetId("WatingThrowUI");
                        }
                        SoundManager.Instance.roundturnStart();
                    }
                }

                break;
            case "InProgress":
                if (_localState == LocalGameState.WaitingForRoundChange) break;

                // 첫 턴 시작 조건을 _localState == InTimeline일 때도 포함
                // if (turnChanged && (_currentGame.GameState == "InProgress" &&
                //                     (_localState == LocalGameState.Idle || _localState == LocalGameState.InTimeline)))
                if (turnChanged || (_currentGame.GameState == "InProgress" && _isMyTurn &&
                                    (_localState == LocalGameState.InTimeline))) 
                                    //(_localState == LocalGameState.Idle || _localState == LocalGameState.InTimeline))) 
                    
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
                
                // if (_localState != LocalGameState.Idle && _localState != LocalGameState.SimulatingOpponentShot)
                //     break; //중복 방지
                 
                //if (updatedRoundByMe == true) break;
                //Debug.Log($"라운드 {_currentGame.RoundNumber - 1} 종료. 다음 라운드를 준비합니다.");
                _localState = LocalGameState.Idle; // 상태 초기화

                //라운드 변경 시 사용한 도넛 목록 초기화
                donutSelectionUI?.ResetDonutUsage();
                //카메라도 시작 캠으로 변경
                //gameCamControl?.SwitchCamera(START_VIEW_CAM);
                gameCamControl?.SwitchCamera(FREE_LOOK_CAM);

                // if (stoneManager.roundCount != _currentGame.RoundNumber)



                // if (stoneManager.roundCount != _currentGame.RoundNumber)
                // {
                //     Debug.Log($"stoneManager.roundCount: {stoneManager.roundCount}, _currentGame.RoundNumber: {_currentGame.RoundNumber}");
                //     Debug.Log("OnRoundEnd 호출되었음");
                //     OnRoundEnd();
                // }
                if (roundDataUpdated == false)
                {
                    //Debug.Log("OnRoundEnd 호출되었음");
                    OnRoundEnd();
                }


                break;

            case "Finished":
                HandleGameFinished();
                break;
        }

    }

    // 상대방 연결이 끊기면 기다리고 일정 시간 지나면 결과 처리
    IEnumerator WaitInitializing()
    {
        float elapsed = 0f;
        float timeout = 10f;

        while (elapsed < timeout)
        {
            if (_currentGame.ReadyPlayers.Count == 2)
            {
                Debug.Log("Initializing 조건문 실행");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Initializing 타임아웃: 상대방이 연결되지 않음");
        ForfeitGame(_currentGame.PlayerIds.FirstOrDefault(id => id != myUserId), "Opponent disconnected");
        
        waitInitializingCoroutine = null;
    }
    
    /// <summary>
    /// 턴이 바뀌었을 때 호출됩니다.
    /// 내 턴이면 돌을 준비하고 입력을 활성화하며, 상대 턴이면 입력을 비활성화합니다.
    /// </summary>
    private void HandleTurnChange()
    {
        // 턴이 변경될 때, 이전 턴에서 캐시된 예측 결과가 있다면 지금 동기화합니다.
        // 이렇게 하면 카메라가 전환된 후, 새 턴이 시작되기 직전의 자연스러운 타이밍에 위치 보정이 이루어집니다.
        if (_cachedPrediction != null && _cachedPrediction.TurnNumber == _currentGame.TurnNumber - 1 && _cachedPrediction.PredictingPlayerId != myUserId)
        {
            stoneManager?.SyncPositions(_cachedPrediction.FinalStonePositions);
            _cachedPrediction = null; // 사용한 예측 결과는 비웁니다.
            ProceedWithTurnChange();
        }
        else
        {
            ProceedWithTurnChange();
        }
    }

    private void ProceedWithTurnChange()
    {
        // 턴이 변경될 때마다 UI에 현재 턴 번호를 업데이트합니다.
        UI_LaunchIndicator_Firebase?.UpdateTurnDisplay(_currentGame.TurnNumber);
        UI_LaunchIndicator_Firebase?.TurnColor(_isMyTurn);

        // 일반적인 턴 시작일 때만 기본 카메라로 전환합니다.
        // bool isExecutingPreparedShot = usePreparedShot && _isMyTurn && _localState == LocalGameState.PreparingShot;
        // if (!isExecutingPreparedShot)

        //if (_isMyTurn && _localState != LocalGameState.WaitingForInput || !_isMyTurn)
        if (_isMyTurn && _localState != LocalGameState.WaitingForInput)
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                // 턴시작시 카메라전환에 약간 딜레이
                gameCamControl?.SwitchCamera(START_VIEW_CAM); // 내 턴 시작 시 카메라를 기본 뷰로 전환
            });
        }

        if (_isMyTurn)
        {
            //UI_LaunchIndicator_Firebase.IdleUI(); // 내 턴이 시작되면 패널을 확실히 끔
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
                UI_LaunchIndicator_Firebase.ShowFloatingText("Your Turn", new Vector3(Screen.width / 2, Screen.height * 0.4f, 0));
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
                    _currentTurnDonutRigid = donutRigid;
                    CountDownStart(10.0f);
                    UI_LaunchIndicator_Firebase?.TurnColor(true);
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
            SuccessfullyShotInTime = false;
            inputController?.DisableInput();
            _localState = LocalGameState.Idle;

            DOVirtual.DelayedCall(2.5f, () => //점수 하이라이트가 2초이므로 기다림
            {
                UI_LaunchIndicator_Firebase.WatingThrowUI(); // 기다림 UI 출력
            }).SetId("WatingThrowUI");

            StoneForceController_Firebase.Team team = StoneForceController_Firebase.Team.None;
            int score = 0;
            float delayTime = 0;
            stoneManager?.CalculateScore(out team, out score, out List<int> donutIds, true); // 마지막에 true를 통해 딱 하나의 도넛만을 필요하다고 알림
            if (team == StoneForceController_Firebase.Team.None && score == -99) //하우스에 도넛이 없으면 score를 0반환, 있으면 -99반환 
            {
                delayTime = 2f; //도넛 하이라이트 지속해줄 시간
                gameCamControl?.SwitchCamera(FREE_LOOK_CAM); // 카메라를 변경
            }
            else
            {
                delayTime = 0.3f;
            }

            DOVirtual.DelayedCall(delayTime, () =>
            {
                // 게임이 종료된 상태라면 UI를 닫지 않고 그냥 리턴합니다.
                if (_localState == LocalGameState.FinishedGame || (_currentGame != null && _currentGame.GameState == "Finished"))
                {
                    return;
                }
                UI_LaunchIndicator_Firebase.IdleUI(); //기본 UI
                gameCamControl?.SwitchCamera(START_VIEW_CAM);

            });

        }
    }

    /// <summary>
    /// 새로운 샷 정보가 Firestore에 올라왔을 때 호출됩니다.
    /// 내 샷인지 상대 샷인지 구분하여 시뮬레이션을 수행합니다.
    /// </summary>
    private void HandleNewShot()
    {
        if (_currentGame.LastShot == null) return;


        //Debug.Log($"lastshotId : {_currentGame.LastShot.PlayerId}");
        if (_currentGame.LastShot.PlayerId != myUserId && _localState == LocalGameState.Idle)
        {
            _localState = LocalGameState.SimulatingOpponentShot;
            DOTween.Kill("WatingThrowUI"); //트윈킬 처리안해주면 시뮬레이션중에 튀어나옴
            UI_LaunchIndicator_Firebase.IdleUI(); // 상대방 샷 데이터가 도착하면 패널을 끔

            SoundManager.Instance.timerFast();
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
                else if (stoneManager.myTeam == StoneForceController_Firebase.Team.B)
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

            if (opponentProfile != null && _currentGame.LastShot != null && !string.IsNullOrEmpty(_currentGame.LastShot.DonutTypeAndNumber))
            {
                opponentDonut = opponentProfile.Inventory.donutEntries.FirstOrDefault(d => d.id == _currentGame.LastShot.DonutTypeAndNumber);
            }

            if (opponentDonut == null)
            {
                Debug.LogError($"상대방({opponentId})의 발사된 도넛({_currentGame.LastShot?.DonutTypeAndNumber}) 정보를 찾을 수 없습니다. 기본 도넛으로 대체합니다.");
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

            int stoneIdToLaunch = _currentGame.LastShot.DonutId;
            stoneManager?.SpawnStone(_currentGame, opponentDonut, null, stoneIdToLaunch);
            // int stoneIdToLaunch =
            //     stoneManager.myTeam == StoneForceController_Firebase.Team.A
            //         ? stoneManager.bShotIndex
            //         : stoneManager.aShotIndex;
            //Debug.Log($"상대 샷(ID: {stoneIdToLaunch}) 시뮬레이션 시작.");
            //float simulationSpeed = (_currentGame.TurnNumber > 1) ? 2.0f : timeMultiplier;

            Time.timeScale = timeMultiplier;
            Time.fixedDeltaTime = fixedTimeMultiplier;
            //Debug.Log($"FixedDeltaTime = {Time.fixedDeltaTime}");
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
            //Debug.Log("예측 결과가 일찍 도착하여 캐시합니다.");
            _cachedPrediction = _currentGame.PredictedResult;
        }
    }

    /// <summary>
    /// 예측 결과를 처리하는 메서드입니다.
    /// 시뮬레이션이 완료된 돌들의 최종 위치를 동기화하고,
    /// 라운드의 마지막 턴인 경우 점수를 계산하여 게임 상태를 업데이트하거나 다음 턴으로 넘깁니다.
    /// </summary>
    /// <param name="result">상대방 또는 자신의 클라이언트로부터 받은 예측 결과 데이터.</param>
    private void ProcessPrediction(PredictedResult result)
    {
        // Debug.Log($"{result.PredictingPlayerId}로부터 받은 예측 결과 처리. 결과를 캐시하고 턴 전환을 시작.");

        // 예측 결과 데이터를 캐시. (HandleTurnChange에서 사용될 수 있음)
        _cachedPrediction = result;

        // 1. 돌들의 최종 위치를 즉시 동기화.
        // 이 시점에서 stoneManager는 각 돌의 Transform.position과 해당 반사 효과 위치를 업데이트.
        stoneManager?.SyncPositions(result.FinalStonePositions);
        // 예측을 처리했으므로 캐시된 예측 결과초기화.
        _cachedPrediction = null;

        // 2. 턴 전환 또는 라운드 종료 로직을 결정.
        bool isLastTurn = _currentGame.PredictedResult.TurnNumber >= (shotsPerRound * 2) - 1;

        if (isLastTurn)
        {
            // 라운드의 마지막 턴.
            // 현재 턴이 라운드의 마지막 턴인지, 그리고 현재 플레이어가 해당 라운드의 후공 플레이어인지 확인.
            if (!IsStartingPlayer())
            {
                Debug.Log("끝나는 로직 실행해야함");
                // --- 라운드 종료 로직 (후공 플레이어만 실행해 동기화문제 방지) ---

                // 시간 지연: 점수 계산 전 최종 돌 배치 보여주기.
                DOVirtual.DelayedCall(1f, () =>
                {
                    //1. 카메라 전환
                    gameCamControl?.SwitchCamera(FREE_LOOK_CAM);
                    
                    // 2. 현재 필드 위의 돌들을 기반으로 점수를 계산.
                    // out 매개변수로 승리 팀, 점수, 점수를 획득한 돌들의 ID 리스트.
                    stoneManager.CalculateScore(out StoneForceController_Firebase.Team winnerTeam, out int score, out List<int> donutIds);
                    
                    // 3. 계산된 라운드 점수를 로컬 총 점수에 반영.
                    UpdateScoreInLocal(winnerTeam, score);
                    
                    // 4. 다음 라운드의 시작 플레이어를 결정.
                    // 기본적으로 패배 팀이 다음 라운드를 시작. 무승부이거나 승자가 없는 경우 PlayerIds[0] (호스트)가 시작.
                    string winnerId = (winnerTeam != StoneForceController_Firebase.Team.None) ? (winnerTeam == stoneManager.myTeam ? myUserId : GetNextPlayerId()) : null;
                    string nextRoundStarterId = (score == 0 || winnerId == null) ? _currentGame.PlayerIds[0] : _currentGame.PlayerIds.FirstOrDefault(id => id != winnerId);

                    // 5. 게임 종료 여부 (콜드 게임 포함)를 판단.
                    // - 3라운드까지 모두 완료되었거나 (정상 종료)
                    // - 2라운드 완료 시점에 점수 차이가 5점 이상 벌어져서 남은 1라운드에서 역전이 불가능한 경우 (콜드 게임)
                    if (_currentGame.RoundNumber >= 3 || (_currentGame.RoundNumber == 2 && Math.Abs(aTeamScore - bTeamScore) >= 5))
                    {
                        // 게임 종료: ResetGameDatas에 isFinished: true를 전달하여 게임을 끝냄.
                        ResetGameDatas(nextRoundStarterId, winnerTeam, donutIds, true);
                    }
                    else
                    {
                        // 다음 라운드 진행: ResetGameDatas에 isFinished: false를 전달하여 다음 라운드를 준비.
                        ResetGameDatas(nextRoundStarterId, winnerTeam, donutIds, false);
                    }
                });
            }
            // 선공 플레이어는 마지막 턴 예측 처리 후 아무것도 하지 않고 대기.
            // 선공 플레이어가 마지막 턴 예측을 올린 후, 추가적인 턴 진행을 방지.
        }
        else
        {
            // --- 다음 턴으로 전환 로직 (라운드 중간) ---
            
            // 다음 턴 플레이어의 ID를 획득.
            string nextPlayerId = GetNextPlayerId();
            
            // 데이터베이스에 다음 턴 플레이어 정보와 턴 번호 증가를 요청.
            var updates = new Dictionary<string, object>
            {
                { "CurrentTurnPlayerId", nextPlayerId },    // 다음 플레이어로 턴 변경
                { "TurnNumber", FieldValue.Increment(1) },  // 턴 번호 1 증가
                { "LastUploaderId", myUserId }              // 마지막 업로더 ID 기록
            };
            db.Collection("games").Document(gameId).UpdateAsync(updates);
        }

        // 예측 처리 완료 후 로컬 상태를 Idle로 변경하여 다음 게임 상태 변화를 대기.
        _localState = LocalGameState.Idle;
    }


    /// <summary>
    /// 게임 종료 상태가 감지되었을 때 호출됩니다.
    /// </summary>
    private void HandleGameFinished()
    {
        blackOutInObj.SetActive(false);
        _localState = LocalGameState.FinishedGame; // 서버로부터 게임 종료 명령을 받으면 자신의 로컬 상태도 종료 상태로 변경
        Time.fixedDeltaTime = initialFixedDeltaTime;
        inputController?.DisableInput(); // 입력 중지
        //Debug.Log($"FixedDeltaTime = {Time.fixedDeltaTime}");
        Time.timeScale = 1f;

        GameOutcome outcome;
        // win,draw,lose 사운드 추가 +++
        // 연결 끊김 또는 몰수패로 승자가 결정되었는지 먼저 확인
        if (!string.IsNullOrEmpty(_currentGame.WinnerId))
        {
            if (_currentGame.WinnerId == myUserId)
            {
                Debug.Log("상대방의 연결 끊김 또는 몰수패로 승리했습니다.");
                outcome = GameOutcome.Win;
                SoundManager.Instance.winDecide();
            }
            else
            {
                Debug.Log("연결 문제 또는 몰수패로 패배했습니다.");
                outcome = GameOutcome.Lose;
                SoundManager.Instance.loseDecide();
            }
        }
        else // WinnerId가 없는 경우, 정상적으로 점수를 비교하여 결과 결정
        {
            if (aTeamScore > bTeamScore)
            {
                if (stoneManager.myTeam == StoneForceController_Firebase.Team.A)
                {
                    Debug.Log("승리");
                    outcome = GameOutcome.Win;
                    SoundManager.Instance.winDecide();
                }
                else
                {
                    Debug.Log("패배");
                    outcome = GameOutcome.Lose;
                    SoundManager.Instance.loseDecide();
                }
            }
            else if (bTeamScore > aTeamScore)
            {
                if (stoneManager.myTeam == StoneForceController_Firebase.Team.A)
                {
                    Debug.Log("패배");
                    outcome = GameOutcome.Lose;
                    SoundManager.Instance.loseDecide();
                }
                else
                {
                    Debug.Log("승리");
                    outcome = GameOutcome.Win;
                    SoundManager.Instance.winDecide();
                }
            }
            else // 비겼을때
            {
                Debug.Log("비김");
                outcome = GameOutcome.Draw;
                SoundManager.Instance.drawDecide();
            }
        }

        UI_LaunchIndicator_Firebase.FinishedUI(outcome);

        // 리스너를 즉시 중지하여 추가 데이터 변경 감지를 막습니다.
        gameListener?.Stop();
        gameListener = null;

        //Debug.Log("게임 종료! 5초 후 메뉴 씬으로 돌아갑니다.");

        // 호스트인 경우에만 DB 문서를 정리합니다.
        if (IsHost())
        {
            CleanupGameDocuments();
        }

    }

    /// <summary>
    /// 호스트 플레이어가 게임 관련 DB 문서를 삭제합니다.
    /// </summary>
    private async void CleanupGameDocuments()
    {
        //Debug.Log("호스트로서 게임 및 룸 문서를 정리합니다.");`
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
    /// 샷 발사 시 탭 입력을 실패했을 때 호출됩니다.
    /// 턴이 멈추지 않도록 실패한 샷으로 처리하고 턴을 넘깁니다.
    /// </summary>
    // public void HandleTapFailed(Rigidbody donutRigid, string donutTypeAndNumber)
    // {
    //     Debug.Log("탭 입력 실패. 턴을 넘깁니다.");
    //     if (donutRigid != null)
    //     {
    //         stoneManager.DonutOut(donutRigid.transform.GetComponent<StoneForceController_Firebase>(), "Tap Failed");
    //     }
    //
    //     _justTimedOut = true; // 타임아웃으로 턴을 놓쳤음을 기록
    //
    //     var zeroDict = new Dictionary<string, float> { { "x", 0 }, { "y", 0 }, { "z", 0 } };
    //     LastShot failedShotData = new LastShot()
    //     {
    //         Force = -999f, // 실패를 나타내는 특수 값
    //         PlayerId = myUserId,
    //         Team = stoneManager.myTeam,
    //         Spin = -999f,
    //         Direction = zeroDict,
    //         //ReleasePosition = zeroDict,
    //         DonutTypeAndNumber = donutTypeAndNumber
    //     };
    //
    //     SubmitShot(failedShotData);
    //     _localState = LocalGameState.WaitingForPrediction;
    // }

    /// <summary>
    /// 돌 조작 스크립트에서 샷이 확정되었을 때 호출됩니다. (인덱스가 없는 경우의 오버로드)
    /// 샷 데이터를 Firebase에 전송하고 입력을 비활성화합니다.
    /// </summary>
    public void SubmitShot(LastShot shotData)
    {
        // 이 오버로드는 인덱스를 모르므로, ID 기반으로 UI 업데이트를 시도합니다. (중복 도넛 문제 가능성 있음)
        if (donutSelectionUI != null && !string.IsNullOrEmpty(shotData.DonutTypeAndNumber))
        {
            var myDonutEntries = _playerProfiles[myUserId]?.Inventory?.donutEntries;
            if (myDonutEntries != null)
            {
                var donutToMark = myDonutEntries.FirstOrDefault(e => e.id == shotData.DonutTypeAndNumber);
                if (donutToMark != null)
                {
                    // donutSelectionUI.MarkDonutAsUsed(donutToMark); // 이 메서드는 이제 int를 받음
                    donutSelectionUI.MarkDonutAsUsed(donutSelectionUI.GetSelectedDonutIndex()); // 이 메서드는 이제 int를 받음
                }
            }
        }

        // 공통 로직 호출
        ProcessShotSubmission(shotData);
    }

    /// <summary>
    /// 돌 조작 스크립트에서 샷이 확정되었을 때 호출됩니다.
    /// 샷 데이터를 Firebase에 전송하고 입력을 비활성화합니다.
    /// </summary>
    public void SubmitShot(LastShot shotData, int usedIndex)
    {
        bool isFailedShot = shotData.Force == -999f;
        //상태 변경을 바로 해주어 다음 인덱스 도넛이 생성되는 오류 방지
        if (!isFailedShot)
        {
            _localState = LocalGameState.SimulatingMyShot;
        }

        shotData.PlayerId = myUserId;
        shotData.Timestamp = Timestamp.GetCurrentTimestamp();

        // shotData에 DonutId가 아직 설정되지 않은 경우에만 UI에서 가져옵니다.
        if (string.IsNullOrEmpty(shotData.DonutTypeAndNumber))
        {
            // 현재 선택된 도넛을 가져옵니다.
            DonutEntry selectedDonut = donutSelectionUI?.GetSelectedDonut();

            // LastShot 데이터에 도넛 ID를 저장합니다.
            shotData.DonutTypeAndNumber = selectedDonut?.id;

            // UI에서 사용된 도넛을 비활성화 처리합니다.
            donutSelectionUI?.MarkDonutAsUsed(usedIndex);
        }
        else
        {
            Debug.LogError($"SubmitShot: 유효하지 않은 인덱스({usedIndex})를 받아 샷을 처리할 수 없습니다.");
        }

        // 공통 로직 호출
        ProcessShotSubmission(shotData);
    }

    /// <summary>
    /// 샷 데이터를 Firestore에 업데이트하는 공통 로직
    /// </summary>
    private void ProcessShotSubmission(LastShot shotData)
    {
        int count = stoneManager.myTeam == StoneForceController_Firebase.Team.A
            ? stoneManager.aShotIndex
            : stoneManager.bShotIndex;

        var updates = new Dictionary<string, object>
        {
            { "LastShot", shotData },
            { $"DonutsIndex.{myUserId}", count }, // 발사 횟수 올림
            { "LastUploaderId", myUserId }
        };

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

    /// <summary>
    /// 현재 플레이어가 게임을 포기하고 몰수패 처리됩니다. 상대방은 승리합니다.
    /// 이 메서드는 항복 버튼에 연결해서 사용합니다.
    /// </summary>
    public void SurrenderGame()
    {
        if (!string.IsNullOrEmpty(myUserId))
        {
            // ForfeitGame 메서드를 호출하여 현재 플레이어를 패배자로, 상대방을 승리자로 처리합니다.
            ForfeitGame(myUserId, "Player surrendered");
            Debug.Log("플레이어가 게임을 포기하여 몰수패 처리됩니다.");
        }
    }

    public void ChangeFixedDeltaTime()
    {
        Time.fixedDeltaTime = fixedTimeMultiplier;
    }

    /// <summary>
    /// 돌 시뮬레이션이 완료되면 호출됩니다.
    /// 예측 결과를 전송하거나, 상대의 예측 결과를 기다립니다. 상대 시뮬 끝나면 내 도넛을 미리 생성하고 발사대기 가능하도록
    /// </summary>
    public void OnSimulationComplete(List<StonePosition> finalPositions)
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = initialFixedDeltaTime;
        //Debug.Log($"FixedDeltaTime = {Time.fixedDeltaTime}");
        //Debug.Log("시뮬레이션 완료.");

        //시뮬레이션 완료 후 딜레이주기
        DOVirtual.DelayedCall(1.5f, () =>
        {
            StoneForceController_Firebase.Team team = StoneForceController_Firebase.Team.None;
            int score = 0;
            float delayTime = 0;
            stoneManager?.CalculateScore(out team, out score, out List<int> donutIds, true); // 마지막에 true를 통해 딱 하나의 도넛만을 필요하다고 알림
            if (team == StoneForceController_Firebase.Team.None && score == -99) //하우스에 도넛이 없으면 score를 0반환, 있으면 -99반환 
            {
                delayTime = 2f; //도넛 하이라이트 지속해줄 시간
                gameCamControl?.SwitchCamera(FREE_LOOK_CAM); // 카메라를 변경
            }
            else
            {
                delayTime = 0.3f;
            }

            DOVirtual.DelayedCall(delayTime, () =>
            {
                gameCamControl?.SwitchCamera(START_VIEW_CAM); // 시뮬레이션 완료 후 시점 전환
                UI_LaunchIndicator_Firebase.FireShotReadyUI(); // UI켜주기

                if (_localState == LocalGameState.SimulatingOpponentShot)
                {
                    //Debug.Log("예측 결과를 서버에 전송합니다.");
                    PredictedResult result = new PredictedResult
                    {
                        PredictingPlayerId = myUserId,
                        TurnNumber = _currentGame.TurnNumber,
                        FinalStonePositions = finalPositions
                    };
                    var updates = new Dictionary<string, object>
                    {
                        { "PredictedResult", result },
                        { "LastUploaderId", myUserId }
                    };
                    db.Collection("games").Document(gameId).UpdateAsync(updates);

                    // if (_justTimedOut)
                    // {
                    //     // 이전 턴이 타임아웃으로 실패했다면, 다음 돌을 미리 생성하지 않고 기다립니다.
                    //     _localState = LocalGameState.Idle; // 상태를 Idle로 변경하여 실제 턴 시작을 기다림
                    //     Debug.Log("Idle");
                    //     _justTimedOut = false; // 플래그 초기화
                    //     //Debug.Log("타임아웃으로 인한 턴 종료. 다음 돌 미리 생성 건너뛰기.");
                    // }
                    // else
                    // {
                    // 일반적인 상대 턴 종료 후, 내 샷을 미리 준비합니다.
                    //Debug.Log("상대 턴 시뮬레이션 완료. 내 샷을 미리 준비합니다.");
                    if ((stoneManager.myTeam == StoneForceController_Firebase.Team.A
                         && stoneManager.aShotIndex >= shotsPerRound - 1)
                        || (stoneManager.myTeam == StoneForceController_Firebase.Team.B
                            && stoneManager.bShotIndex >= shotsPerRound - 1))
                    {
                        //Debug.Log("라운드에 발사가능한 횟수가 끝나서 내 턴으로 돌아오지 않습니다");
                    }
                    else
                    {
                        DonutEntry selectedDonut = donutSelectionUI?.GetSelectedDonut();
                        if (selectedDonut == null)
                        {
                            //Debug.LogError("발사할 도넛이 선택되지 않았거나 DonutSelectionUI가 할당되지 않았습니다.");
                            return;
                        }

                        _currentTurnDonutRigid = stoneManager?.SpawnStone(_currentGame, selectedDonut, myUserId);
                        if (_currentTurnDonutRigid != null)
                        {
                            CountDownStart(10f); // Use the new signature
                            UI_LaunchIndicator_Firebase?.TurnColor(true);
                        }
                        else
                        {
                            //Debug.Log("아마 발사횟수가 끝났을 가능성이 높음");
                        }
                        //}
                    }
                }
                else if (_localState == LocalGameState.SimulatingMyShot)
                {
                    ChangeState_To_WaitingForPrediction();
                }
            });
        });
    }

    public void ChangeState_To_WaitingForPrediction()
    {
        _localState = LocalGameState.WaitingForPrediction;
        //Debug.Log("내 샷 시뮬레이션 완료. 상대방의 예측 결과를 기다립니다.");

        if (_cachedPrediction != null && _cachedPrediction.TurnNumber == _currentGame.TurnNumber)
        {
            //Debug.Log("캐시된 예측 결과를 즉시 처리합니다.");
            ProcessPrediction(_cachedPrediction);
        }
    }

    public void OnRoundEnd() //이번 라운드가 끝났을때.
    {
        SuccessfullyShotInTime = false;
        UI_LaunchIndicator_Firebase?.UpdateTurnDisplay(7);

        stoneManager?.SyncPositions(_currentGame.PredictedResult.FinalStonePositions);
        
        gameCamControl?.SwitchCamera(FREE_LOOK_CAM);
        roundDataUpdated = true;

        stoneManager?.VisualizeScoreDonuts(_currentGame.ScoredDonuts.Team, _currentGame.ScoredDonuts.StoneId);
        aTeamScore = _currentGame.ATeamScore;
        bTeamScore = _currentGame.BTeamScore;

        DOVirtual.DelayedCall(4f, () =>
        {
            donutSelectionUI?.ResetDonutUsage();
            string nextState = (_currentGame.CurrentTurnPlayerId == "Finished" && _currentGame.RoundStartingPlayerId == "Finished") ? "Finished" : "Timeline";

            PredictedResult result = new PredictedResult
            {
                PredictingPlayerId = myUserId,
                TurnNumber = 0,
                FinalStonePositions = new List<StonePosition>()
            };

            var updates = new Dictionary<string, object>
            {
                { "LastUploaderId", myUserId },
                { "PredictedResult", result },
                { "ScoredDonuts", null },
                { "GameState", nextState }
            };
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

        //Debug.Log($"승리팀 : {winner}, 점수 : {score}");
    }

    private void ResetGameDatas(string nextPlayerId, StoneForceController_Firebase.Team team,
        List<int> scoredDonutIds, bool isFinished = false) // 다음 라운드 시작 플레이어를 파라미터로 받음
    {
        Debug.Log("reset");
        SuccessfullyShotInTime = false;
        _localState = LocalGameState.WaitingForRoundChange;
        
        
        if (isFinished) // 만약 게임이 끝났다면
        {
            nextPlayerId = "Finished"; // 다음 플레이어 이름에 Finished를 적어서 상대방에게 게임 끝남을 알림
        }


        // PredictedResult result = new PredictedResult
        // {
        //     PredictingPlayerId = myUserId,
        //     TurnNumber = 0,
        //     FinalStonePositions = new List<StonePosition>()
        // };

        var scoredDonuts = new ScoredDonuts
        {
            StoneId = scoredDonutIds,
            Team = team.ToString(),
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
            //{ "PredictedResult", result },
            { "LastUploaderId", myUserId },
            { "ScoredDonuts", scoredDonuts },
            { "GameState", "RoundChanging" } // 다음 라운드 시작 전, 연출을 위해 Timeline 상태로 전환
        };
        currentRound++;
        stoneManager?.RoundCountUp();
        roundDataUpdated = true;
        db.Collection("games").Document(gameId).UpdateAsync(updates);

        //db.Collection("games").Document(gameId).UpdateAsync("PredictedResult", result);

        // DOVirtual.DelayedCall(4f, () =>
        // {
        //     stoneManager?.ClearOldDonutsInNewRound(_currentGame, currentRound);
        //     UI_LaunchIndicator_Firebase?.UpdateTurnDisplay(_currentGame.TurnNumber);
        // });

    }
    /// <summary>
    /// 샷 발사 시 탭 입력을 실패했을 때 호출됩니다.
    /// 턴이 멈추지 않도록 실패한 샷으로 처리하고 턴을 넘깁니다.
    /// </summary>
    public void PlayerLostTimeToShotInTime(Rigidbody donutRigid, string message)
    {
        string donutTypeAndNumber = null;
        StoneShoot_Firebase stoneShoot = transform.GetComponent<StoneShoot_Firebase>();
        if (donutRigid != null)
        {
            if (stoneShoot.IsFinalDirectionAvailable()) // 이미 방향이라도 설정한게 있으면
            {
                stoneShoot.ReleaseShot(); // 그냥 자동으로 발사한다
                return;
            }
            StoneForceController_Firebase sfc = donutRigid.transform.GetComponent<StoneForceController_Firebase>();
            donutTypeAndNumber = sfc.DonutTypeAndNumber;
            //stoneManager.DonutOut(sfc, "Timeout");
            stoneManager.DonutOut(sfc, message);
        }
        _justTimedOut = true; // 타임아웃으로 턴을 놓쳤음을 기록
        var zeroDict = new Dictionary<string, float>
        {
            { "x", 0 },
            { "y", 0 },
            { "z", 0 }
        };
        LastShot failedShotData = new LastShot()
        {
            Force = -999f, // 실패를 나타내는 특수 값
            PlayerId = myUserId,
            DonutId = stoneManager.CurrentTurnStone.donutId,
            Team = stoneManager.myTeam, // 발사하는 팀
            Spin = -999f, // 최종 스핀 값
            Direction = zeroDict, // 발사 방향
            DonutTypeAndNumber = donutTypeAndNumber// 시간 초과된 도넛의 ID를 명시적으로 전달
        };
        SubmitShot(failedShotData);
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

    public string GetMyUserId()
    {
        return myUserId;
    }

    #endregion

    #region 도넛 교체 관련

    /// <summary>
    /// DonutSelectionUI에서 다른 도넛을 선택했을 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void OnDonutChanged(DonutEntry newDonut)
    {
        if (_justTimedOut)
        {
            _justTimedOut = false;
            //Debug.Log("얘가 자꾸 도넛 발사 실패하면 호출되서 막아버림");
            return;
        }
        // 입력대기 상태면 도넛을 교체 할 수 있게
        if (_localState == LocalGameState.WaitingForInput)
        {

            //Debug.Log($"선택한 도넛이 {newDonut.id}(으)로 변경되어 교체합니다.");
            ReplaceCurrentStone(newDonut);
        }
    }

    /// <summary>
    /// 현재 턴에 생성된 돌을 파괴하고 새로운 돌로 교체합니다.
    /// </summary>
    private void ReplaceCurrentStone(DonutEntry newDonut)
    {
        //_currentTurnDonutEntry = newDonut; // 교체된 도넛 인스턴스로 업데이트

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
        _currentTurnDonutRigid = stoneManager.SpawnStone(_currentGame, newDonut, myUserId);
        if (_currentTurnDonutRigid != null)
        {
            inputController.EnableInput(_currentTurnDonutRigid);
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
            //Debug.Log("카메라 전환을 시도합니다."); // 로그 추가
            gameCamControl?.SwitchCamera(FOLLOW_STONE_CAM2, stoneToFollow.transform, stoneToFollow.transform);
        }
        else
        {
            //Debug.LogWarning("카메라가 따라갈 돌을 찾지 못했습니다."); // 경고 로그 추가
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

    public void CountDownStart(float time)
    {
        UI_LaunchIndicator_Firebase?.UpdateTurnDisplay(_currentGame.TurnNumber + 1); // 턴 UI업데이트 (+1을 해주어 선반영)

        _localState = LocalGameState.WaitingForInput;
        inputController?.EnableInput(_currentTurnDonutRigid);

        // 3초 동안 입력이 없으면 가이드 표시
        DOVirtual.DelayedCall(3f, () =>
        {
            if (_localState == LocalGameState.WaitingForInput && inputController != null && inputController.CurrentDragType == StoneShoot_Firebase.DragType.None)
            {
                UI_LaunchIndicator_Firebase?.ShowGuideUI(1);
            }
        }).SetId("GuideTimer1");

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
                ControlCountdown(false);
                countDownTween = null;

                // 시간 내에 샷을 성공적으로 완료했으면 아무것도 하지 않음
                if (SuccessfullyShotInTime)
                {
                    return;
                }

                // --- 시간 초과 처리 ---
                Debug.Log("입력 시간 초과. 턴을 넘깁니다.");

                inputController?.DisableInput();
                PlayerLostTimeToShotInTime(_currentTurnDonutRigid, "TimeOut");
                SoundManager.Instance.timeOver();
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

    /// <summary>
    /// 게임 시작 시 페널티를 로컬에서 생성 및 적용합니다.
    /// </summary>
    private void ApplyInitialPenalty()
    {
        if (penaltyApplied) return;

        // 0-4 사이에서 중복되지 않는 랜덤 인덱스 2개 생성
        int index1 = UnityEngine.Random.Range(0, 5);
        int index2;
        do
        {
            index2 = UnityEngine.Random.Range(0, 5);
        } while (index1 == index2);

        PlayerProfile opponentProfile = GetPlayerProfile(GetOpponentId());
        List<DonutEntry> opponentDonuts = opponentProfile?.Inventory?.donutEntries;
        GameManager.Instance.ApplyStartGamePenalty(index1, index2, opponentDonuts);
        penaltyApplied = true;
        Debug.Log($"게임 시작 페널티 로직 실행. 인덱스 {index1}, {index2}의 도넛이 제거됩니다.");
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
