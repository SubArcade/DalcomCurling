
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// Firestore의 'rooms' 문서 구조를 나타내는 클래스입니다.
// Firestore는 속성을 통해 자동으로 이 클래스를 직렬화/역직렬화합니다.
[FirestoreData]
public class Room
{
    [FirestoreProperty]
    public string RoomId { get; set; } // 룸의 고유 ID

    [FirestoreProperty]
    public string Status { get; set; } // "waiting", "playing"

    [FirestoreProperty]
    public List<string> PlayerIds { get; set; } // 룸에 있는 플레이어들의 ID 목록

    [FirestoreProperty]
    public int PlayerCount { get; set; } // 현재 플레이어 수

    [FirestoreProperty]
    public Timestamp CreatedAt { get; set; } // 룸 생성 시간

    [FirestoreProperty]
    public string GameId { get; set; } // 생성된 게임의 ID

    [FirestoreProperty]
    public string ScoreBracket { get; set; } // 룸 생성자의 점수 구간

    // 새로 추가할 필드: 각 플레이어의 프로필 정보를 저장합니다.
    [FirestoreProperty]
    public Dictionary<string, PlayerProfile> PlayerProfiles { get; set; }
}

// Firestore의 'games' 문서 구조를 나타내는 클래스입니다.
[FirestoreData]
public class Game
{
    [FirestoreProperty]
    public List<string> PlayerIds { get; set; }

    [FirestoreProperty]
    public string CurrentTurnPlayerId { get; set; }

    [FirestoreProperty]
    public string RoundStartingPlayerId { get; set; }

    [FirestoreProperty]
    public string GameState { get; set; } // "Initializing", "Timeline", "InProgress", "Finished"

    [FirestoreProperty]
    public List<string> ReadyPlayers { get; set; } // 씬 로드를 완료한 플레이어 목록

    [FirestoreProperty]
    public int TurnNumber { get; set; } // 현재 턴 번호 (1~8)

    [FirestoreProperty]
    public int RoundNumber { get; set; } // 현재 라운드 정보 ( 1 ~ 3 )

    [FirestoreProperty]
    public int ATeamScore { get; set; } //A 팀의 현재 점수

    [FirestoreProperty]
    public int BTeamScore { get; set; } //B 팀의 현재 점수


    [FirestoreProperty]
    public Dictionary<string, int> DonutsIndex { get; set; } // 플레이어별 사용한 돌 개수

    [FirestoreProperty]
    public LastShot LastShot { get; set; } // 마지막으로 쏜 샷의 정보

    [FirestoreProperty]
    public PredictedResult PredictedResult { get; set; } // 예측 시뮬레이션 결과

    [FirestoreProperty]
    public Dictionary<string, Timestamp> PlayerHeartbeats { get; set; } // 각 플레이어의 마지막 접속 시간을 저장하는 속성

    [FirestoreProperty]
    public string WinnerId { get; set; } // 승자 ID를 저장하는 속성

    [FirestoreProperty]
    public string FinishReason { get; set; } // 게임 종료 사유를 저장하는 속성
}

[FirestoreData]
public class LastShot
{
    [FirestoreProperty]
    public string PlayerId { get; set; }
    [FirestoreProperty]
    public StoneForceController_Firebase.Team Team { get; set; }
    [FirestoreProperty]
    public float Force { get; set; }
    [FirestoreProperty] public float Spin { get; set; }
    [FirestoreProperty] public Dictionary<string, float> Direction { get; set; }
    [FirestoreProperty] public Dictionary<string, float> ReleasePosition { get; set; } // 릴리즈 시점의 위치
    [FirestoreProperty, ServerTimestamp] public Timestamp Timestamp { get; set; }
    [FirestoreProperty] public string DonutId { get; set; } // 발사된 도넛의 ID
}

[FirestoreData]
public class PredictedResult
{
    [FirestoreProperty]
    public string PredictingPlayerId { get; set; }
    [FirestoreProperty]
    public int TurnNumber { get; set; } // 어떤 턴의 예측 결과인지 확인용
    [FirestoreProperty]
    public List<StonePosition> FinalStonePositions { get; set; }
    [FirestoreProperty]
    public Dictionary<string, int> Score { get; set; } // 점수 정보 (필요시 사용)
}

[FirestoreData]
public class StonePosition
{
    [FirestoreProperty]
    public int StoneId { get; set; }
    [FirestoreProperty]
    public string Team { get; set; }
    [FirestoreProperty]
    public Dictionary<string, float> Position { get; set; } // Vector3 저장용
}


public class FirebaseMatchmakingManager : MonoBehaviour
{
    public static FirebaseMatchmakingManager Instance { get; private set; }

    private FirebaseFirestore db;
    private ListenerRegistration roomListener; // 룸 문서의 변경사항을 감지하는 리스너

    public static string CurrentGameId { get; private set; }
    public static string CurrentRoomId { get; private set; } // 게임 씬에 RoomId를 전달하기 위한 변수

    private const string RoomsCollection = "rooms"; // Firestore의 룸 컬렉션 이름
    private const string GamesCollection = "games"; // Firestore의 게임 컬렉션 이름

    // [SerializeField] private UnityEngine.UI.Button matchmakingButton; // 스타트버튼 
    // [SerializeField] private TMPro.TextMeshProUGUI matchmakingStatusText; // 매칭 상태를 표시할 텍스트 
    //TODO : 해당 버튼과 매칭상태 표시는 UI쪽으로 옮겨서 필요한 코드를 호출하는 방식으로 변경해야함

    private Coroutine matchmakingTimeoutCoroutine; // 매치메이킹 타임아웃 코루틴
    private string waitingRoomId; // 현재 대기 중인 룸의 ID

    // 점수 구간 정의
    private const string ScoreBracket_0_199 = "0-199";
    private const string ScoreBracket_200_499 = "200-499";
    private const string ScoreBracket_500_999 = "500-999";
    private const string ScoreBracket_1000_1499 = "1000-1499";
    private const string ScoreBracket_1500_Plus = "1500+";

    void Awake()
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

    void Start()
    {
        // Firestore 인스턴스를 초기화합니다.
        db = FirebaseFirestore.DefaultInstance;
        //matchmakingButton.onClick.AddListener(()=>StartMatchmaking());
    }

    /// <summary>
    /// 솔로 점수(soloScore)에 따라 점수 구간을 반환합니다.
    /// </summary>
    /// <param name="soloScore">플레이어의 솔로 점수</param>
    /// <returns>점수 구간 문자열</returns>
    private string GetScoreBracket(int soloScore)
    {
        if (soloScore >= 0 && soloScore <= 199)
        {
            return ScoreBracket_0_199;
        }
        else if (soloScore >= 200 && soloScore <= 499)
        {
            return ScoreBracket_200_499;
        }
        else if (soloScore >= 500 && soloScore <= 999)
        {
            return ScoreBracket_500_999;
        }
        else if (soloScore >= 1000 && soloScore <= 1499)
        {
            return ScoreBracket_1000_1499;
        }
        else // 1500 이상
        {
            return ScoreBracket_1500_Plus;
        }
    }

    // 매치메이킹 시작 버튼을 누를 때 호출될 함수입니다.
    public async Task StartMatchmaking()
    {
        Debug.Log("매치메이킹을 시작합니다...");

        //매칭버튼을 누르면 버튼 비활성화
        //if (matchmakingButton != null) { matchmakingButton.interactable = false; }
        //if (matchmakingStatusText != null) { matchmakingStatusText.text = "매칭중"; matchmakingStatusText.gameObject.SetActive(true); }

        string userId = FirebaseAuthManager.Instance.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("로그인하지 않은 상태에서는 매치메이킹을 시작할 수 없습니다.");
            return;
        }

        // Firestore에서 현재 플레이어의 데이터를 직접 가져와 솔로 점수(soloScore)를 얻습니다.
        DocumentSnapshot userDoc = await db.Collection("user").Document(userId).GetSnapshotAsync();
        if (!userDoc.Exists)
        {
            Debug.LogError($"Firestore에 사용자 데이터가 없습니다: {userId}");
            return;
        }

        // PlayerData 클래스를 사용하여 데이터를 역직렬화합니다.
        PlayerData playerData = userDoc.ConvertTo<PlayerData>();
        int playerSoloScore = playerData.soloScore;
        string playerScoreBracket = GetScoreBracket(playerSoloScore);
        Debug.Log($"현재 플레이어의 솔로 점수: {playerSoloScore}, 점수 구간: {playerScoreBracket}");

        // 'waiting' 상태이고 플레이어가 2명 미만이며, 동일한 점수 구간의 룸을 찾습니다.
        Query waitingRoomsQuery = db.Collection(RoomsCollection)
            .WhereEqualTo("Status", "waiting")
            .WhereLessThan("PlayerCount", 2)
            .WhereEqualTo("ScoreBracket", playerScoreBracket)
            .Limit(1);

        QuerySnapshot snapshot = await waitingRoomsQuery.GetSnapshotAsync();

        if (snapshot.Count > 0)
        {
            // 참여할 룸을 찾았습니다.
            DocumentSnapshot roomDoc = snapshot.Documents.First();
            Debug.Log($"참여할 룸을 찾았습니다: {roomDoc.Id}");
            await JoinRoomAsync(roomDoc.Reference);
        }
        else
        {
            // 참여할 룸이 없으므로 새 룸을 생성합니다.
            Debug.Log("참여할 룸이 없어 새 룸을 생성합니다.");
            await CreateRoomAsync(playerScoreBracket); // 점수 구간을 전달
        }
    }

    private async System.Threading.Tasks.Task JoinRoomAsync(DocumentReference roomRef)
    {
        string userId = FirebaseAuthManager.Instance.UserId;

        // 참가 플레이어의 프로필 정보를 미리 가져옵니다. (트랜잭션 외부에서 await)
        UserDataRoot joiningUserData = await DataManager.Instance.GetUserDataRootAsync(userId);
        if (joiningUserData == null)
        {
            Debug.LogError($"참가 플레이어({userId})의 데이터를 불러오지 못했습니다.");
            return;
        }

        // [테스트용 임시 코드] 인벤토리가 비어있으면 더미 데이터를 주입합니다.
        if (joiningUserData.inventory == null || joiningUserData.inventory.donutEntries == null || joiningUserData.inventory.donutEntries.Count == 0)
        {
            Debug.LogWarning($"참가 플레이어({userId})의 인벤토리가 비어있어 테스트용 더미 데이터를 주입합니다.");
            joiningUserData.inventory = new InventoryData
            {
                donutEntries = new List<DonutEntry>
                {
                    new DonutEntry { id = "Soft_15", type = DonutType.Soft, weight = 10, resilience = 5, friction = 3 },
                    new DonutEntry { id = "Hard_22", type = DonutType.Hard, weight = 12, resilience = 6, friction = 4 },
                    new DonutEntry { id = "Moist_13", type = DonutType.Moist, weight = 11, resilience = 7, friction = 2 },
                    new DonutEntry { id = "Soft_14", type = DonutType.Soft, weight = 9, resilience = 8, friction = 5 },
                    new DonutEntry { id = "Hard_2", type = DonutType.Hard, weight = 13, resilience = 6, friction = 3 }
                }
            };
        }

        // [테스트용 임시 코드] 닉네임이 비어있으면 더미 닉네임을 주입합니다.
        string displayNickname = string.IsNullOrEmpty(joiningUserData.player.nickname) ? "플레이어2" : joiningUserData.player.nickname;

        PlayerProfile joiningProfile = new PlayerProfile
        {
            Nickname = displayNickname,
            Email = joiningUserData.player.email,
            Inventory = joiningUserData.inventory
        };
        await db.RunTransactionAsync(async transaction =>
        {
            DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(roomRef);
            if (!snapshot.Exists)
            {
                throw new Exception("참여하려는 룸이 더 이상 존재하지 않습니다.");
            }

            Room room = snapshot.ConvertTo<Room>();

            // 룸이 가득 찼거나 더 이상 대기 중이 아닌 경우, 트랜잭션을 중단합니다.
            if (room.PlayerCount >= 2 || room.Status != "waiting")
            {
                Debug.Log("룸에 참여할 수 없습니다. (가득 찼거나 게임 중)");
                return; // 다른 룸을 다시 찾도록 로직을 되돌릴 수 있습니다.
            }

            // 룸 정보를 업데이트합니다.
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "PlayerIds", FieldValue.ArrayUnion(userId) },
                { "PlayerCount", room.PlayerCount + 1 },
                { "Status", "playing" },
                { $"PlayerProfiles.{userId}", joiningProfile } // 참가 플레이어 프로필 추가
            };

            transaction.Update(roomRef, updates);
        });

        Debug.Log($"룸에 성공적으로 참여했습니다: {roomRef.Id}");
        AttachListener(roomRef.Id); // 참여 후 리스너 부착
    }

    private async System.Threading.Tasks.Task CreateRoomAsync(string playerScoreBracket)
    {
        string userId = FirebaseAuthManager.Instance.UserId;
        DocumentReference newRoomRef = db.Collection(RoomsCollection).Document();

        // 호스트 플레이어의 프로필 정보를 가져옵니다.
        UserDataRoot hostUserData = await DataManager.Instance.GetUserDataRootAsync(userId);
        if (hostUserData == null)
        {
            Debug.LogError($"호스트 플레이어({userId})의 데이터를 불러오지 못했습니다.");
            return;
        }

        // [테스트용 임시 코드] 인벤토리가 비어있으면 더미 데이터를 주입합니다.
        if (hostUserData.inventory == null || hostUserData.inventory.donutEntries == null || hostUserData.inventory.donutEntries.Count == 0)
        {
            Debug.LogWarning($"호스트 플레이어({userId})의 인벤토리가 비어있어 테스트용 더미 데이터를 주입합니다.");
            hostUserData.inventory = new InventoryData
            {
                donutEntries = new List<DonutEntry>
                {
                    new DonutEntry { id = "Soft_10", type = DonutType.Soft, weight = 10, resilience = 5, friction = 3 },
                    new DonutEntry { id = "Hard_5", type = DonutType.Hard, weight = 12, resilience = 6, friction = 4 },
                    new DonutEntry { id = "Moist_20", type = DonutType.Moist, weight = 11, resilience = 7, friction = 2 },
                    new DonutEntry { id = "Soft_22", type = DonutType.Soft, weight = 9, resilience = 8, friction = 5 },
                    new DonutEntry { id = "Hard_2", type = DonutType.Hard, weight = 13, resilience = 6, friction = 3 }
                }
            };
        }

        // [테스트용 임시 코드] 닉네임이 비어있으면 더미 닉네임을 주입합니다.
        string displayNickname = string.IsNullOrEmpty(hostUserData.player.nickname) ? "플레이어1" : hostUserData.player.nickname;

        PlayerProfile hostProfile = new PlayerProfile
        {
            Nickname = displayNickname,
            Email = hostUserData.player.email,
            Inventory = hostUserData.inventory
        };
        var room = new Room
        {
            RoomId = newRoomRef.Id,
            Status = "waiting",
            PlayerIds = new List<string> { userId },
            PlayerCount = 1, // 플레이어 수 초기화
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            GameId = null, // 초기에는 GameId가 없습니다.
            ScoreBracket = playerScoreBracket, // 룸 생성 시 점수 구간 설정
            PlayerProfiles = new Dictionary<string, PlayerProfile> { { userId, hostProfile } } // 호스트 프로필 추가
        };

        await newRoomRef.SetAsync(room);
        Debug.Log($"새 룸을 생성했습니다: {newRoomRef.Id}");

        // 타임아웃 처리를 위해 현재 룸 ID를 저장하고 코루틴을 시작합니다.
        waitingRoomId = newRoomRef.Id;
        matchmakingTimeoutCoroutine = StartCoroutine(MatchmakingTimeoutCoroutine());

        // 룸 생성 후, 다른 플레이어가 참여하는 것을 감지하기 위해 리스너를 부착합니다.
        AttachListener(newRoomRef.Id);
    }

    // 다른플레이어의 참가를 감지하는 리스너를 부착하고 룸에 모두 입장했다면 호스트가 방을 생성하고 입장까지 연결 시켜주는 함수
    // 이부분의 최적화가 읽기를 줄이는 방법중 하나 룸의 변경사항이 있을경우 콜백됨
    private void AttachListener(string roomId)
    {
        if (roomListener != null) // 리스너 중복 방지
        {
            roomListener.Stop();
            roomListener = null;
        }


        DocumentReference roomRef = db.Collection(RoomsCollection).Document(roomId);
        roomListener = roomRef.Listen(snapshot =>
        {
            if (!snapshot.Exists) return;

            Room room = snapshot.ConvertTo<Room>();
            Debug.Log($"룸 데이터 변경 감지: {snapshot.Id}");

            // 룸이 채워졌는지 확인합니다.
            if (room.Status == "playing" && room.PlayerIds.Count == 2)
            {
                // 매칭이 성공했으므로 타임아웃 코루틴을 중지합니다.
                if (matchmakingTimeoutCoroutine != null)
                {
                    StopCoroutine(matchmakingTimeoutCoroutine);
                    matchmakingTimeoutCoroutine = null;
                }
                waitingRoomId = null; // 대기 룸 ID 초기화

                // GameId가 설정되었는지 확인합니다.
                if (!string.IsNullOrEmpty(room.GameId))
                {
                    // GameId가 있으면 게임 씬으로 이동합니다.
                    Debug.Log($"게임({room.GameId}) 준비 완료! 씬을 로드합니다.");
                    CurrentGameId = room.GameId;
                    CurrentRoomId = room.RoomId; // RoomId 전달

                    if (roomListener != null)
                    {
                        roomListener.Stop();
                        roomListener = null;
                    }
                    SceneLoader.Instance.LoadLocal(GameManager.Instance.gameSceneName);
                }
                else
                {
                    // GameId가 없으면, 호스트 플레이어가 게임을 생성해야 합니다.
                    string myUserId = FirebaseAuthManager.Instance.UserId;
                    // 첫 번째 플레이어를 호스트로 지정합니다. 
                    if (room.PlayerIds[0] == myUserId)
                    {
                        Debug.Log("내가 호스트이므로 게임 생성을 시작합니다.");
                        CreateGameAsync(room);
                    }
                }
            }
        });
    }

    private async void CreateGameAsync(Room room)
    {
        // 새 게임 객체를 생성합니다.
        var newGame = new Game
        {
            PlayerIds = room.PlayerIds,
            CurrentTurnPlayerId = room.PlayerIds[0], // 첫 번째 플레이어부터 시작
            RoundStartingPlayerId = room.PlayerIds[0], // 첫 번째 플레이어가 이 라운드의 시작 플레이어임을 기록
            GameState = "Initializing",
            ReadyPlayers = new List<string>(),
            TurnNumber = 0,
            RoundNumber = 1,
            ATeamScore = 0,
            BTeamScore = 0,
            DonutsIndex = new Dictionary<string, int>
            {
                { room.PlayerIds[0], 0 },
                { room.PlayerIds[1], 0 }
            },
            LastShot = null,
            PredictedResult = null
        };

        // "games" 컬렉션에 새 게임 문서를 추가합니다.
        DocumentReference gameRef = await db.Collection(GamesCollection).AddAsync(newGame);
        Debug.Log($"새 게임 문서 생성 완료: {gameRef.Id}");

        // "rooms" 문서에 생성된 GameId를 업데이트합니다.  콜백으로 AttachListener이 다시 호출 됨
        await db.Collection(RoomsCollection).Document(room.RoomId).UpdateAsync("GameId", gameRef.Id);
        Debug.Log($"룸({room.RoomId})에 GameId({gameRef.Id}) 업데이트 완료.");
    }

    private System.Collections.IEnumerator MatchmakingTimeoutCoroutine()
    {
        yield return new WaitForSeconds(30f);

        // 타임아웃 시점에 waitingRoomId가 아직 설정되어 있다면 (즉, 매칭이 안됨) 매칭을 취소합니다.
        if (!string.IsNullOrEmpty(waitingRoomId))
        {
            CancelMatchmaking();
        }
    }

    private async void CancelMatchmaking()
    {
        Debug.Log("매치메이킹 시간 초과 또는 취소. 매칭을 중단합니다.");
        UIManager.Instance.Close(PanelId.MatchingPopUp); //매칭팝업창 닫기
        // 리스너 중지
        if (roomListener != null)
        {
            roomListener.Stop();
            roomListener = null;
        }

        // 타임아웃 코루틴 중지
        if (matchmakingTimeoutCoroutine != null)
        {
            StopCoroutine(matchmakingTimeoutCoroutine);
            matchmakingTimeoutCoroutine = null;
        }

        // Firestore에서 룸 삭제
        if (!string.IsNullOrEmpty(waitingRoomId))
        {
            await db.Collection(RoomsCollection).Document(waitingRoomId).DeleteAsync();
            Debug.Log($"Firestore에서 룸({waitingRoomId})을 삭제했습니다.");
            waitingRoomId = null; // 룸 ID 초기화
        }

        // UI 리셋
        //if (matchmakingButton != null) { matchmakingButton.interactable = true; }
        //if (matchmakingStatusText != null) { matchmakingStatusText.gameObject.SetActive(false); }
    }

    void OnDestroy() // 게임이 종료되거나 명시적으로 파괴할시 호출되므로 필요한코드, 삭제금지
    {
        if (roomListener != null)
        {
            roomListener.Stop();
            roomListener = null;
        }

        // 코루린이 돌고있다면 삭제
        if (matchmakingTimeoutCoroutine != null)
        {
            StopCoroutine(matchmakingTimeoutCoroutine);
            matchmakingTimeoutCoroutine = null;
        }
    }
}