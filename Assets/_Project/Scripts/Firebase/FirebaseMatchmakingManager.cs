
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public string GameState { get; set; } // "Initializing", "Timeline", "InProgress", "Finished"

    [FirestoreProperty]
    public List<string> ReadyPlayers { get; set; } // 씬 로드를 완료한 플레이어 목록

    [FirestoreProperty]
    public int TurnNumber { get; set; } // 현재 턴 번호 (1~8)

    [FirestoreProperty]
    public Dictionary<string, int> StonesUsed { get; set; } // 플레이어별 사용한 돌 개수

    [FirestoreProperty]
    public LastShot LastShot { get; set; } // 마지막으로 쏜 샷의 정보

    [FirestoreProperty]
    public PredictedResult PredictedResult { get; set; } // 예측 시뮬레이션 결과
}

[FirestoreData]
public class LastShot
{
    [FirestoreProperty]
    public string PlayerId { get; set; }
    [FirestoreProperty]
    public float Force { get; set; }
    [FirestoreProperty] public float Spin { get; set; }
    [FirestoreProperty] public Dictionary<string, float> Direction { get; set; }
    [FirestoreProperty] public Dictionary<string, float> ReleasePosition { get; set; } // 릴리즈 시점의 위치
    [FirestoreProperty, ServerTimestamp] public Timestamp Timestamp { get; set; }
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

    private const string RoomsCollection = "rooms"; // Firestore의 룸 컬렉션 이름
    private const string GamesCollection = "games"; // Firestore의 게임 컬렉션 이름

    [SerializeField] private string gameSceneName = "Sce_GameScene"; // 게임 씬 이름

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
    }

    // 매치메이킹 시작 버튼을 누를 때 호출될 함수입니다.
    public async void StartMatchmaking()
    {
        Debug.Log("매치메이킹을 시작합니다...");
        string userId = FirebaseAuthManager.Instance.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("로그인하지 않은 상태에서는 매치메이킹을 시작할 수 없습니다.");
            return;
        }

        // 'waiting' 상태이고 플레이어가 2명 미만인 룸을 찾습니다.
        Query waitingRoomsQuery = db.Collection(RoomsCollection)
            .WhereEqualTo("Status", "waiting")
            .WhereLessThan("PlayerCount", 2) // PlayerCount 필드를 추가해야 합니다.
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
            await CreateRoomAsync();
        }
    }

    private async System.Threading.Tasks.Task JoinRoomAsync(DocumentReference roomRef)
    {
        string userId = FirebaseAuthManager.Instance.UserId;

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
                { "Status", "playing" }
            };

            transaction.Update(roomRef, updates);
        });

        Debug.Log($"룸에 성공적으로 참여했습니다: {roomRef.Id}");
        AttachListener(roomRef.Id); // 참여 후 리스너 부착
    }

    private async System.Threading.Tasks.Task CreateRoomAsync()
    {
        string userId = FirebaseAuthManager.Instance.UserId;
        DocumentReference newRoomRef = db.Collection(RoomsCollection).Document();

        var room = new Room
        {
            RoomId = newRoomRef.Id,
            Status = "waiting",
            PlayerIds = new List<string> { userId },
            PlayerCount = 1, // 플레이어 수 초기화
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            GameId = null // 초기에는 GameId가 없습니다.
        };

        await newRoomRef.SetAsync(room);
        Debug.Log($"새 룸을 생성했습니다: {newRoomRef.Id}");

        // 룸 생성 후, 다른 플레이어가 참여하는 것을 감지하기 위해 리스너를 부착합니다.
        AttachListener(newRoomRef.Id);
    }

    private void AttachListener(string roomId)
    {
        if (roomListener != null)
        {
            roomListener.Stop();
            roomListener = null;
        }

        DocumentReference roomRef = db.Collection(RoomsCollection).Document(roomId);
        roomListener = roomRef.Listen(snapshot =>
        {
            if (!snapshot.Exists) return;

            Debug.Log($"룸 데이터 변경 감지: {snapshot.Id}");
            Room room = snapshot.ConvertTo<Room>();

            // 룸이 채워졌는지 확인합니다.
            if (room.Status == "playing" && room.PlayerIds.Count == 2)
            {
                // GameId가 설정되었는지 확인합니다.
                if (!string.IsNullOrEmpty(room.GameId))
                {
                    // GameId가 있으면 게임 씬으로 이동합니다.
                    Debug.Log($"게임({room.GameId}) 준비 완료! 씬을 로드합니다.");
                    CurrentGameId = room.GameId;

                    if (roomListener != null)
                    {
                        roomListener.Stop();
                        roomListener = null;
                    }

                    // TODO: SceneLoader.Instance.LoadLocal(gameSceneName); 으로 변경
                    SceneManager.LoadScene(gameSceneName);
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
            GameState = "Initializing",
            ReadyPlayers = new List<string>(),
            TurnNumber = 1,
            StonesUsed = new Dictionary<string, int>
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

        // "rooms" 문서에 생성된 GameId를 업데이트합니다.
        await db.Collection(RoomsCollection).Document(room.RoomId).UpdateAsync("GameId", gameRef.Id);
        Debug.Log($"룸({room.RoomId})에 GameId({gameRef.Id}) 업데이트 완료.");
    }

    // 룸 리스너를 해제하는 함수입니다.
    void OnDestroy()
    {
        if (roomListener != null)
        {
            roomListener.Stop();
            roomListener = null;
        }
    }
}

