using System;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum GameMode
{
    SOLO,
    DUO,
    TRIO
}

public enum GameTier
{
    Bronze,
    Silver,
    Gold,
    Platinum,
    Diamond,
    Challenger_I,
    Challenger_II,
    Challenger_III,
    Challenger_IV,
}

[System.Serializable, FirestoreData]
public class UserDataRoot
{
    [field: SerializeField] [FirestoreProperty] public PlayerData player { get; set; } = new PlayerData(){
        email = "test@test.com",
        nickname = "test",
        gold = 250,
        gem = 7,
        energy = 10,
        level = 1,
        exp = 0,
        lastAt = 0,
        maxEnergy = 20,
        perSecEnergy = 10,
        soloScore = 0,
        soloTier = GameTier.Bronze,
    };
    [field: SerializeField] [FirestoreProperty] public InventoryData inventory { get; set; } = new InventoryData();
    [field: SerializeField] [FirestoreProperty] public MergeBoardData mergeBoard { get; set; } = new MergeBoardData();
    [field: SerializeField] [FirestoreProperty] public DonutData donut { get; set; } = new DonutData();
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    private FirebaseFirestore db;
    
    private string userCollection = "user";   // 컬랙션(테이블) 이름
    [Header("플레이어 데이터 (신규 생성 시 초기 저장, 기존 계정은 불러와 갱신)")]
    [SerializeField, Tooltip("프리미어키(PK)")] private string docId = "ZflvQAZZmYj1SKj8mZKZ";
    
    [SerializeField]private UserDataRoot userData = new UserDataRoot();

    public PlayerData PlayerData => userData.player;
    public InventoryData InventoryData => userData.inventory;
    public MergeBoardData MergeBoardData => userData.mergeBoard;
    public DonutData DonutData => userData.donut;
    
    // 랭크
    private string rankCollection = "rank";
    [Header("랭크")]
    [SerializeField, Tooltip("시즌")] private string Season = "2025";
    private GameMode gameMode = GameMode.SOLO;
    
    // 데이터 값 바뀐거 호출
    public event Action<PlayerData> OnUserDataChanged;
    
    // 백그라운드 이벤트
    public event Action PauseChanged;
    
    void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(this);
    }

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
        PlayerData.email = userEmail;
        int maxEnergy = PlayerData.maxEnergy;
        int secEnergy = PlayerData.perSecEnergy;
        
        var docRef = db.Collection("user").Document(uId);
        var snap = await docRef.GetSnapshotAsync();
        
        if (!snap.Exists)
        {
            // 처음 로그인 시
            PlayerData.createAt = Timestamp.GetCurrentTimestamp();
            BasePlayerData(maxEnergy,secEnergy);
            await docRef.SetAsync(PlayerData, SetOptions.MergeAll);
            Debug.Log($"[FS] 신규 유저 생성: /{userCollection}/{uId}");
        }
        else
        {
            // 기존 유저 로드
            userData.player = snap.ConvertTo<PlayerData>();  
            BasePlayerData(maxEnergy,secEnergy);
            Debug.Log($"[FS] 기존 유저 로드 완료: /{userCollection}/{uId}");
        }
        OnUserDataChanged?.Invoke(PlayerData);
    }

    // 기본 데이터 적용
    private void BasePlayerData(int maxEnergy, int secEnergy)
    {
        PlayerData.maxEnergy = maxEnergy;
        PlayerData.perSecEnergy = secEnergy;
    }
    
    // 업데이트 BM이나 필수적인것들 중요한것들
    // 사용법 : await UpdateUserData(gold: 500, exp: 1200); 필요한 값만 넣어주세요
    public async Task UpdateUserDataAsync(
        string email = null,
        string nickname = null,
        int? gold = null,
        int? gem = null,
        int? energy = null,
        int? level = null,
        int? exp = null,
        int? soloScore = null,
        GameTier? soloTier = null
    )
    
    {
        try
        {
            var patch = new Dictionary<string, object>();

            // 플레이어 데이터
            PatchUtil.SetIfNotNullOrEmpty(patch, "email", email, v => PlayerData.email = v);
            PatchUtil.SetIfNotNullOrEmpty(patch, "nickname", nickname, v => PlayerData.nickname = v);
            PatchUtil.SetIfHasValue(patch, "gold",   gold,   v => PlayerData.gold = v);
            PatchUtil.SetIfHasValue(patch, "gem",    gem,    v => PlayerData.gem = v);
            PatchUtil.SetIfHasValue(
                patch, "energy", energy,
                v => PlayerData.energy = v//,
                //onChanged: () => Scr_EnergyRegenNotifier.Instance?.RescheduleNotification()
            );
            PatchUtil.SetIfHasValue(patch, "level",  level,  v => PlayerData.level = v);
            PatchUtil.SetIfHasValue(patch, "exp",    exp,    v => PlayerData.exp = v);
            
            // 랭크 데이터
            PatchUtil.SetIfHasValue(patch, "soloScore", soloScore, v => PlayerData.soloScore = v);
            PatchUtil.SetIfHasValue(patch, "soloTier", soloTier, v => PlayerData.soloTier = v);
            
            // 로컬 데이터 변경
            
            if (patch.Count == 0)
            {
                Debug.LogWarning("변경할 필드가 없습니다.");
                return;
            }
            if(patch.ContainsKey("energy"))
                patch["lastAt"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            // var docRef = db.Collection(userCollection).Document(docId);
            // await docRef.UpdateAsync(patch);
            await db.Collection(userCollection).Document(docId).UpdateAsync(patch);
            Debug.Log($"부분 업데이트 완료: /{userCollection}/{docId}");
            
            bool rankChanged = patch.ContainsKey("soloScore") || patch.ContainsKey("soloTier");
            if (rankChanged)
            {
                await UpsertLeader(PlayerData.soloScore, PlayerData.soloTier);
                Debug.Log($"[FS] Rank 동기화 완료: /{Season}_{gameMode.ToString().ToLower()}/{docId}");
            }

            OnUserDataChanged?.Invoke(PlayerData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS][WRITE][ERR] {e}");
        }
    }
    
    // 종료 시 전체 저장
    public async Task SaveAllUserDataAsync()
    {
        try
        {
            var patch = new Dictionary<string, object>
            {
                ["email"]   = PlayerData.email,
                ["nickname"]   = PlayerData.nickname,
                ["gold"]    = PlayerData.gold,
                ["gem"]     = PlayerData.gem,
                ["energy"]  = PlayerData.energy,
                ["level"]   = PlayerData.level,
                ["exp"]     = PlayerData.exp,
                ["lastAt"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ["soloScore"]   = PlayerData.soloScore,
                ["soloTier"] = PlayerData.soloTier
            };

            var docRef = db.Collection(userCollection).Document(docId);
            await docRef.SetAsync(patch, SetOptions.MergeAll);

            Debug.Log($"[FS] 전체 저장 완료: /{userCollection}/{docId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS][SAVE-ALL][ERR] {e}");
        }
    }

    
    // Rank 디비 관련 함수들
    
    // 디비에 들어가는 컬랙션과 문서 설정
    DocumentReference ModeDoc(GameMode? mode = null)
    {
        var targetMode = mode ?? gameMode;
        return db.Collection("rank").Document($"{Season}_{targetMode.ToString().ToLower()}");
    }

    CollectionReference LeadersCol(GameMode? mode = null)
    {
        var targetMode = mode ?? gameMode;
        return ModeDoc(targetMode).Collection("leaders");
    }

    
    // 랭킹 초기값 설정
    async Task SeedRankAsync(GameMode mode)
    {
        await ModeDoc(mode).SetAsync(new Dictionary<string, object> {
            { "season", Season }, { "mode", mode }, { "updatedAt", FieldValue.ServerTimestamp }
        }, SetOptions.MergeAll);
    }

    // 유저 데이터 넣기 (Rank)
    // 기본 데이터 값 설정 않넣어도 상관없다 ( GameMode mode = GameMode.SOLO, docid = docId)
    public async Task UpsertLeader(int score, GameTier tier, GameMode? mode = null, string docid = null, string nickname = null)
    {
        mode = mode ?? gameMode;
        docid = null ?? docId;
        nickname = null ?? PlayerData.nickname;
        var doc = LeadersCol(mode).Document(docid);
        await doc.SetAsync(new Dictionary<string, object> {
            { "nickname", nickname }, 
            { "score", score }, 
            { "tier", tier.ToString().ToLower() }, 
            { "updatedAt", FieldValue.ServerTimestamp }
        }, SetOptions.MergeAll);
    }

    // 예시 (테이블 기본 셋팅)
    async Task SeedSummaryAsync()
    {
        var sumDoc = db.Collection("rank_summaries").Document(Season);
        await sumDoc.SetAsync(new Dictionary<string, object> {
            { "soloTop100", new List<object>() },
            { "duoTop100",  new List<object>() },
            { "trioTop100", new List<object>() },
            { "updatedAt", FieldValue.ServerTimestamp }
        }, SetOptions.MergeAll);
    }


    // 랭크 구분 함수 ( 랭크를 디비에서 계산해서 넣어줘야 함 )
    private GameTier CalculateTier(int score, int? rank = null)
    {
        // 챌린저 구간 랭크 값 등수 받아와야함
        // 1) 챌린저 구간 ( 디비의 랭크 기준 )
        if (rank.HasValue)
        {
            if (rank.Value <= 5) return GameTier.Challenger_I;
            if (rank.Value <= 50) return GameTier.Challenger_II;
            if (rank.Value <= 200) return GameTier.Challenger_III;
            if (rank.Value <= 1000) return GameTier.Challenger_IV;
        }
        
        // 기본 구간
        if (score < 1000) return GameTier.Bronze;
        if (score < 2000) return GameTier.Silver;
        if (score < 3000) return GameTier.Gold;
        if (score < 4000) return GameTier.Platinum;
        if (score < 5000) return GameTier.Diamond;
        
        return GameTier.Challenger_IV;
    }
    
    // 시스템 백그라운드
    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            _ = SaveAllUserDataAsync();
            PauseChanged?.Invoke();
        }
    }

    // 포커스 잃을 떄
    // void OnApplicationFocus(bool hasFocus)
    // {
    //     if (!hasFocus)
    //     {
    //         _ = SaveAllUserDataAsync();
    //     }
    // }
    
    // Util
    
    // 값 타입(int, float, bool, etc.)용
    static class PatchUtil
    {
        public static void SetIfHasValue<T>(
            Dictionary<string, object> patch,
            string key,
            T? newValue,
            Action<T> assign,
            Action onChanged = null
        ) where T : struct
        {
            if (!newValue.HasValue) return;
            var v = newValue.Value;
            patch[key] = v;
            assign(v);
            onChanged?.Invoke();
        }

        // 문자열용
        public static void SetIfNotNullOrEmpty(
            Dictionary<string, object> patch,
            string key,
            string newValue,
            Action<string> assign
        )
        {
            if (string.IsNullOrEmpty(newValue)) return;
            patch[key] = newValue;
            assign(newValue);
        }
    }


}
