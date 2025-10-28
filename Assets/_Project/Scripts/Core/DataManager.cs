using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    private FirebaseFirestore db;

    [Header("디비에 저장되는 데이터 초기 값")]
    private const string COLLECTION = "user";   // 컬랙션(테이블) 이름
    [SerializeField, Tooltip("골드")] private const int GOLD = 250;
    [SerializeField, Tooltip("잼")] private const int GEM = 7;
    [SerializeField, Tooltip("에너지")] private const int ENERGY = 10;
    [SerializeField, Tooltip("레벨")] private const int LEVEL = 1;
    [SerializeField, Tooltip("경험치")] private const int EXP = 0;
    
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }
    
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

        // 새로운 데이터 추가
        // var added = await AddUserAutoAsync("test@example.com", "홍길동");
        // if (added != null)
        // {
        //     Debug.Log($"[FS] 새 문서 추가 완료! docId={added.Id}");
        //
        //     // 확인용으로 곧바로 읽어보기
        //     var snap = await added.GetSnapshotAsync();
        //     Debug.Log($"[FS] 저장 확인: {snap.ToDictionary()["email"]}, {snap.ToDictionary()["password"]}");
        // }
        
        // 업데이트
        //await WriteUser(DOC_ID, TEST_EMAIL, TEST_PASSWORD);

        // 읽기
        //await ReadUser(DOC_ID);

        // 검색
       // await FindUserByEmail(TEST_EMAIL);
    }
    
    // 새 문서 추가 (자동 PK 생성)
    public async Task<DocumentReference> AddUserAutoAsync(string email, string password)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                { "email", email },
                { "password", password },
                { "gold", GOLD },
                { "gem", GEM },
                { "energy", ENERGY },
                { "level", LEVEL },
                { "exp", EXP },
                { "createAt", Timestamp.GetCurrentTimestamp() },
            };

            // AddAsync → 자동으로 고유한 문서 ID 생성
            var docRef = await db.Collection("user").AddAsync(data);
            return docRef;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS][ADD][ERR] {e}");
            return null;
        }
    }
    
    // 업데이트
    public async Task UpdateUser(string docId, string email, string password)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                { "email", email },
                { "password", password }, // 데모용: 운영에선 해시/토큰 등으로 대체
            };

            await db.Collection(COLLECTION).Document(docId).SetAsync(data, SetOptions.MergeAll);
            Debug.Log($"[FS][WRITE] /{COLLECTION}/{docId} 저장 완료 (email={email})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS][WRITE][ERR] {e}");
        }
    }

    // 전체 조회
    public async Task ReadUser(string docId)
    {
        try
        {
            var snap = await db.Collection(COLLECTION).Document(docId).GetSnapshotAsync();
            if (!snap.Exists)
            {
                Debug.LogWarning($"[FS][READ] 문서 없음: /{COLLECTION}/{docId}");
                return;
            }

            var dict = snap.ToDictionary();
            string email = dict.TryGetValue("email", out var eVal) ? eVal.ToString() : "(null)";
            string password = dict.TryGetValue("password", out var pVal) ? pVal.ToString() : "(null)";

            Debug.Log($"[FS][READ] 문서 찾음: /{COLLECTION}/{docId}");
            Debug.Log($"[FS][READ] email={email}, password={password}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS][READ][ERR] {e}");
        }
    }
    
    // 이메일 조회
    public async Task FindUserByEmail(string email)
    {
        // user 컬렉션에서 email 조건으로 문서 조회
        var query = db.Collection("user").WhereEqualTo("email", email);
        var result = await query.GetSnapshotAsync();

        if (result.Count == 0)
        {
            Debug.Log($"[FS][QUERY] 해당 이메일({email})을 가진 문서를 찾지 못했습니다.");
            return;
        }

        foreach (var doc in result.Documents)
        {
            var data = doc.ToDictionary();

            string docId = doc.Id;
            string emailValue = data.ContainsKey("email") ? data["email"].ToString() : "(null)";
            string passwordValue = data.ContainsKey("password") ? data["nickname"].ToString() : "(null)";

            Debug.Log($"[FS][QUERY] docId={docId}");
            Debug.Log($"[FS][QUERY] email={emailValue}");
            Debug.Log($"[FS][QUERY] password={passwordValue}");
        }
    }
}
