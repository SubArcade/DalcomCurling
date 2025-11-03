using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    private FirebaseFirestore db;

    [System.Serializable, FirestoreData]
    public class PlayerData
    {
        [field: SerializeField, Tooltip("이메일")] [FirestoreProperty] public string email { get; set; }
        [field: SerializeField, Tooltip("골드")] [FirestoreProperty] public int gold { get; set; }
        [field: SerializeField, Tooltip("잼")] [FirestoreProperty] public int gem { get; set; }
        [field: SerializeField, Tooltip("에너지")] [FirestoreProperty] public int energy { get; set; }
        [field: SerializeField, Tooltip("레벨")] [FirestoreProperty] public int level { get; set; }
        [field: SerializeField, Tooltip("경험치")] [FirestoreProperty] public int exp { get; set; }
        
        [FirestoreProperty] public Timestamp createAt { get; set; }
        [field: SerializeField, Tooltip("처음 접속 시간")] public long createdAtUnix { get; set; }
    }
    
    private string collection = "user";   // 컬랙션(테이블) 이름
    [Header("플레이어 데이터 (신규 생성 시 초기 저장, 기존 계정은 불러와 갱신)")]
    [SerializeField, Tooltip("프리미어키(PK)")] private string docId = "ZflvQAZZmYj1SKj8mZKZ";
    
    [SerializeField]
    private PlayerData playerData = new PlayerData
    {
        email = "test@test.com",
        gold = 250,
        gem = 7,
        energy = 10,
        level = 1,
        exp = 0
    };
    
    // 데이터 값 바뀐거 호출
    public event System.Action<PlayerData> OnUserDataChanged;
    
    void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(this);
    }
    
    /*
    async void Start()
    {
        // Firebase 준비
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep != DependencyStatus.Available)
        {
            Debug.LogError($"[FS] Firebase not ready: {dep}");
            return;
        }
        db = FirebaseFirestore.DefaultInstance;
        Debug.Log("[FS] Firestore instance OK");
        
        //await UpdateUserDataAsync(docId, gold: 500, exp: 1200);
    }
    */

    async void Start()
    {
        // FirebaseInitializer가 완료될 때까지 기다립니다.
        while (!FirebaseInitializer.IsInitialized)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        // 여러 클라이언트 테스트 시 로컬 DB 충돌을 막기 위해 지속성 비활성화
        FirebaseFirestore.DefaultInstance.Settings.PersistenceEnabled = false;
        db = FirebaseFirestore.DefaultInstance;
        Debug.Log("[FS] Firestore instance OK");
    }
    
    // 신규 생성 시 초기 저장, 기존 계정은 불러와 갱신
    public async Task EnsureUserDocAsync(string uId, string userEmail)
    {
        docId = uId;
        playerData.email = userEmail;
        var docRef = db.Collection("user").Document(uId);
        var snap = await docRef.GetSnapshotAsync();

        if (!snap.Exists)
        {
            // 처음 로그인 시
            playerData.createAt = Timestamp.GetCurrentTimestamp();

            await docRef.SetAsync(playerData, SetOptions.MergeAll);
            Debug.Log($"[FS] 신규 유저 생성: /{collection}/{uId}");
        }
        else
        {
            // 기존 유저 로드
            playerData = snap.ConvertTo<PlayerData>();
            Debug.Log($"[FS] 기존 유저 로드 완료: /{collection}/{uId}");
        }
    }
    
    // 업데이트
    // 사용법 : await UpdateUserData(docId, gold: 500, exp: 1200); 필요한 값만 넣어주세요
    public async Task UpdateUserDataAsync(
        string docId,
        string email = null,
        int? gold = null,
        int? gem = null,
        int? energy = null,
        int? level = null,
        int? exp = null
    )
    
    {
        try
        {
            var patch = new Dictionary<string, object>();

            if (email != null) patch["email"] = email; playerData.email = email;
            

            if (gold.HasValue) patch["gold"]   = gold.Value; playerData.gold = gold.Value;

            if (gem.HasValue) patch["gem"]    = gem.Value; playerData.gem = gem.Value;

            if (energy.HasValue) patch["energy"] = energy.Value; playerData.energy = energy.Value;

            if (level.HasValue) patch["level"]  = level.Value; playerData.level = level.Value;

            if (exp.HasValue) patch["exp"]    = exp.Value; playerData.exp = exp.Value;

            if (patch.Count == 0)
            {
                Debug.LogWarning("변경할 필드가 없습니다.");
                return;
            }

            var docRef = db.Collection(collection).Document(docId);
            await docRef.UpdateAsync(patch);
            Debug.Log($"부분 업데이트 완료: /{collection}/{docId}");
            
            OnUserDataChanged?.Invoke(playerData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS][WRITE][ERR] {e}");
        }
    }
}
