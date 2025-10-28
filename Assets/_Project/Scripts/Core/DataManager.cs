using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    
    private FirebaseFirestore db;

    // 테스트 값 (원하면 바꿔서 사용)
    private const string COLLECTION = "user";
    private const string DOC_ID = "XHcsQo4GTZZFj7aQ6wyB";
    private const string TEST_EMAIL = "test@example.com";
    private const string TEST_PASSWORD = "1234"; // 실제 서비스에서는 절대 평문 저장 금지!

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
    private async Task<DocumentReference> AddUserAutoAsync(string email, string password)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                { "email", email },
                { "password", password },
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
    private async Task UpdateUser(string docId, string email, string password)
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
    private async Task ReadUser(string docId)
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
    private async Task FindUserByEmail(string email)
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
            string passwordValue = data.ContainsKey("password") ? data["password"].ToString() : "(null)";

            Debug.Log($"[FS][QUERY] docId={docId}");
            Debug.Log($"[FS][QUERY] email={emailValue}");
            Debug.Log($"[FS][QUERY] password={passwordValue}");
        }
    }

    public async void OnLoginSuccess(FirebaseUser user)
    {
        string uid = user.UserId;
        string email = user.Email;

        Debug.Log($"[DataManager] 로그인된 유저: {email} (uid={uid})");

        // Firestore에 이 유저의 문서가 있는지 확인
        var docRef = db.Collection("user").Document(uid);
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            // 문서가 없으면 새로 만들어줌
            var data = new Dictionary<string, object>
            {
                { "email", email },
                { "createdAt", Timestamp.GetCurrentTimestamp() },
                { "nickname", "새 유저" }
            };

            await docRef.SetAsync(data);
            Debug.Log($"[FS] 새 유저 데이터 생성: {email}");
        }
        else
        {
            Debug.Log($"[FS] 기존 유저 데이터 존재: {email}");
        }
    }

    // 유저 정보 읽기
    public async Task<Dictionary<string, object>> GetUserData(string uid)
    {
        var doc = await db.Collection("user").Document(uid).GetSnapshotAsync();
        if (!doc.Exists)
        {
            Debug.LogWarning("유저 문서 없음");
            return null;
        }
        return doc.ToDictionary();
    }
}
