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

    // 외부에서 FirebaseUser를 넘겨받아 Firestore에 등록/갱신하는 함수
    public async void RunFirestoreSamples()
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
    }
}
