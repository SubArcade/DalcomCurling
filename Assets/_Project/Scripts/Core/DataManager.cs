using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.Serialization;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    private FirebaseFirestore db;

    [Header("플레이어 데이터 (신규 생성 시 초기 저장, 기존 계정은 불러와 갱신)")]
    private string collection = "user";   // 컬랙션(테이블) 이름
    [SerializeField, Tooltip("프리미어키(PK)")] private string docId = "ZflvQAZZmYj1SKj8mZKZ";
    [SerializeField, Tooltip("이메일")] private string email = "";
    [SerializeField, Tooltip("골드")] private int gold = 250;
    [SerializeField, Tooltip("잼")] private int gem = 7;
    [SerializeField, Tooltip("에너지")] private int energy = 10;
    [SerializeField, Tooltip("레벨")] private int level = 1;
    [SerializeField, Tooltip("경험치")] private int exp = 0;
    
    void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(this);
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
        //await UpdateUserPartial(docId, gold: 500, exp: 1200);
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
                { "gold", gold },
                { "gem", gem },
                { "energy", energy },
                { "level", level },
                { "exp", exp },
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
    // 사용법 : await UpdateUserPartial(docId, gold: 500, exp: 1200); 필요한 값만 넣어주세요
    public async Task UpdateUserPartial(
        string docId,
        string email = null,
        string password = null,
        int? gold = null,
        int? gem = null,
        int? energy = null,
        int? level = null,
        int? exp = null)
    {
        try
        {
            var patch = new Dictionary<string, object>();

            if (email != null)    patch["email"] = email;
            if (password != null) patch["password"] = password;

            if (gold.HasValue)   patch["gold"]   = gold.Value;
            if (gem.HasValue)    patch["gem"]    = gem.Value;
            if (energy.HasValue) patch["energy"] = energy.Value;
            if (level.HasValue)  patch["level"]  = level.Value;
            if (exp.HasValue)    patch["exp"]    = exp.Value;

            if (patch.Count == 0)
            {
                Debug.LogWarning("[FS][WRITE] 변경할 필드가 없습니다.");
                return;
            }

            var docRef = db.Collection(collection).Document(docId);
            await docRef.UpdateAsync(patch); // 제공한 필드만 변경, 나머지는 그대로
            Debug.Log($"[FS][WRITE] 부분 업데이트 완료: /{collection}/{docId}");
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
            var snap = await db.Collection(collection).Document(docId).GetSnapshotAsync();
            if (!snap.Exists)
            {
                Debug.LogWarning($"[FS][READ] 문서 없음: /{collection}/{docId}");
                return;
            }

            var dict = snap.ToDictionary();
            string email = dict.TryGetValue("email", out var eVal) ? eVal.ToString() : "(null)";
            string password = dict.TryGetValue("password", out var pVal) ? pVal.ToString() : "(null)";

            Debug.Log($"[FS][READ] 문서 찾음: /{collection}/{docId}");
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

    // 외부에서 FirebaseUser를 넘겨받아 Firestore에 등록/갱신하는 함수
    /*public async void RunFirestoreSamples()
    {
        if (db == null)
        {
            Debug.LogError("[FS][DEMO] Firestore 초기화가 아직 안 되었습니다.");
            return;
        }

        try
        { 
            // 1) 새로운 데이터 추가 (자동 문서 ID 생성)
            var added = await AddUserAutoAsync(TEST_EMAIL, TEST_PASSWORD);
            if (added != null)
            {
                Debug.Log($"[FS][DEMO] 새 문서 추가 완료! docId={added.Id}");

                // 1-1) 방금 추가한 문서 바로 읽어보기
                var snap = await added.GetSnapshotAsync();
                var dict = snap.ToDictionary();
                var addedEmail = dict.ContainsKey("email") ? dict["email"] : "(null)";
                var addedPw = dict.ContainsKey("password") ? dict["password"] : "(null)";
                Debug.Log($"[FS][DEMO] 저장 확인: email={addedEmail}, password={addedPw}");
            }
            else
            {
                Debug.LogWarning("[FS][DEMO] 추가 실패(added==null)");
            }

            // 2) 업데이트 (샘플: DOC_ID 상수로 지정된 문서를 업데이트)
            //    실제로 존재하는 문서 ID여야 합니다. 없으면 새로 생성되며 MergeAll로 병합 저장됩니다.
            await UpdateUser(DOC_ID, TEST_EMAIL, TEST_PASSWORD);
            Debug.Log("[FS][DEMO] UpdateUser(DOC_ID, TEST_EMAIL, TEST_PASSWORD) 완료");

            // 3) 읽기 (DOC_ID 문서 읽기)
            await ReadUser(DOC_ID);

            // 4) 검색 (이메일로 문서 검색)
            await FindUserByEmail(TEST_EMAIL);

            Debug.Log("[FS][DEMO] === 샘플 완료 ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS][DEMO][ERR] {e.Message}");
            Debug.LogException(e);
        }
    }*/
}
