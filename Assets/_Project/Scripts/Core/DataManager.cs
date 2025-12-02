using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Firestore;
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
    [field: SerializeField] [FirestoreProperty] public PlayerData player { get; set; } = new PlayerData();
    [field: SerializeField] [FirestoreProperty] public InventoryData inventory { get; set; } = new InventoryData();
    [field: SerializeField] [FirestoreProperty] public MergeBoardData mergeBoard { get; set; } = new MergeBoardData();
    [field: SerializeField] [FirestoreProperty] public QuestData quest { get; set; } = new QuestData();
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    private FirebaseFirestore db;
    
    private string userCollection = "user";   // 컬랙션(테이블) 이름
    [Header("플레이어 데이터 (신규 생성 시 초기 저장, 기존 계정은 불러와 갱신)")]
    [SerializeField, Tooltip("프리미어키(PK)")] private string docId = "ZflvQAZZmYj1SKj8mZKZ";

    public bool isLogin = false;
    [SerializeField]private UserDataRoot userData = new UserDataRoot();

    public PlayerData PlayerData => userData.player;
    public InventoryData InventoryData => userData.inventory;
    public MergeBoardData MergeBoardData => userData.mergeBoard;
    public QuestData QuestData => userData.quest;
    
    // 도넛 기본 데이터 SO
    private readonly Dictionary<DonutType, DonutTypeSO> _donutTypeDB = new();
    private readonly List<GiftBoxSO> _giftBoxList = new();
    
    // 랭크
    private string rankCollection = "rank";
    [Header("랭크")]
    [SerializeField, Tooltip("시즌")] private string Season = "2025";
    private GameMode gameMode = GameMode.SOLO;
    
    [SerializeField] private RankData rankData = new RankData();
    
    // 데이터 값 바뀐거 호출
    public event Action<PlayerData> OnUserDataChanged;
    public event Action<UserDataRoot> OnUserDataRootChanged;
    public event Action OnBoardDataLoaded;

    // 백그라운드 이벤트
    public event Action<bool> PauseChanged;
    
    // 처음 셋팅 데이터
    [SerializeField] private PlayerData firstPlayerData = new PlayerData()
    {
        email = "",
        nickname = "Dalcom",
        gold = 5000,
        gem = 1000,
        energy = 2000,
        level = 1,
        exp = 0,
        lastAt = 0,
        maxEnergy = 50,
        perSecEnergy = 10,
        soloScore = 0,
        soloTier = GameTier.Bronze,
        levelMax = 20,
        gainNamePlateType =
        {
            NamePlateType.NONE
        }
    };
    [SerializeField] private InventoryData firstInventoryData = new InventoryData()
    {
        donutEntries = new List<DonutEntry>()
        {
            null,
            null,
            null,
            null,
            null
        }
    };
    [SerializeField] private MergeBoardData firstMergeBoardData = new MergeBoardData()
    {
        generatorLevelHard = 1,
        generatorLevelMoist = 1,
        generatorLevelSoft = 1,
        cellMax = 49,
        cellWidth = 7,
        cellLength = 7,
    };
    [SerializeField] private QuestData firstQuestData = new QuestData()
    {
        maxCount = 5,
        refreshCount =5,
        baseGold = 0,
    };

    // 바뀐 데이터 이벤트 함수 실행용 함수
    // 텍스트를 바꿔줄꺼다
    public void GemChange(int gem)
    {
        PlayerData.gem = gem;
        OnUserDataChanged?.Invoke(PlayerData);
    }
    
    public void GoldChange(int gold)
    {
        PlayerData.gold = gold;
        OnUserDataChanged?.Invoke(PlayerData);
    }
    
    public void EnergyChange(int energy)
    {
        PlayerData.energy = energy;
        OnUserDataChanged?.Invoke(PlayerData);
    }

    public void LevelChange(int level) 
    {
        PlayerData.level = level;
        OnUserDataChanged?.Invoke(PlayerData);
    }
    public void NicknameChange(string Name)
    {
        PlayerData.nickname = Name;
        OnUserDataChanged?.Invoke(PlayerData);
    }

    void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(this);
        LoadAllDonutData();
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
        //Debug.Log("[FS] Firestore instance OK");
    }
    
    // 신규 생성 시 초기 저장, 기존 계정은 불러와 갱신
    public async Task EnsureUserDocAsync(string uId, string userEmail= null, bool isAutoLogin = false, AuthProviderType authProviderType = AuthProviderType.Guest)
    {
        docId = uId;
        
        int maxEnergy = firstPlayerData.maxEnergy;
        int secEnergy = firstPlayerData.perSecEnergy;
        int maxLevel = firstPlayerData.levelMax;
        
        int cellMax = firstMergeBoardData.cellMax;
        int cellWidth = firstMergeBoardData.cellWidth;
        int cellLength = firstMergeBoardData.cellLength;
        
        int baseGold = firstQuestData.baseGold;
        int RefreshCount = firstQuestData.refreshCount;
        int maxCount = firstQuestData.maxCount;
        
        var docRef = db.Collection(userCollection).Document(uId);
        var snap = await docRef.GetSnapshotAsync();

        if (isAutoLogin)
        {
            userData = snap.ConvertTo<UserDataRoot>();
            isLogin = true;
            
            BasePlayerData(maxEnergy, secEnergy, maxLevel);
            //BaseInventoryData();
            //BaseMergeBoardData(cellMax, cellWidth, cellLength);
            BaseQuestData(baseGold, RefreshCount, maxCount);

            await docRef.SetAsync(userData, SetOptions.MergeAll);
            Debug.Log($"자동 로그인: /{userCollection}/{uId}");
        }
        
        if (!snap.Exists)
        {
            // 처음 로그인 시
            PlayerData.email = userEmail;
            PlayerData.createAt = Timestamp.GetCurrentTimestamp();
            isLogin = true;
            
            BasePlayerData(maxEnergy, secEnergy, maxLevel);
            FirstBasePlayerData();
            FirstBaseInventoryData();
            BaseInventoryData();
            BaseMergeBoardData(cellMax, cellWidth, cellLength);
            FirstBaseMergeBoardData();
            BaseQuestData(baseGold, RefreshCount, maxCount);

            await docRef.SetAsync(userData, SetOptions.MergeAll);
            Debug.Log($"[FS] 신규 유저 생성: /{userCollection}/{uId}");
        }
        else
        {
            // 기존 유저 로드
            PlayerData.email = userEmail;
            userData = snap.ConvertTo<UserDataRoot>();
            isLogin = true;
            
            BasePlayerData(maxEnergy, secEnergy, maxLevel);
            //BaseInventoryData();
            //BaseMergeBoardData(cellMax, cellWidth, cellLength);
            BaseQuestData(baseGold, RefreshCount, maxCount);

            await docRef.SetAsync(userData, SetOptions.MergeAll);
            Debug.Log($"[FS] 기존 유저 로드/갱신 완료: /{userCollection}/{uId}");
        }
        OnUserDataChanged?.Invoke(PlayerData);
        OnUserDataRootChanged?.Invoke(userData);
        OnBoardDataLoaded?.Invoke();
    }
    
    // 기본 데이터 적용
    private void BasePlayerData(int maxEnergy, int secEnergy, int maxLevel)
    {
        PlayerData.maxEnergy = maxEnergy;
        PlayerData.perSecEnergy = secEnergy;
        PlayerData.levelMax = maxLevel;
    }

    // 값이 바뀌는 처음 셋팅 데이터
    private void FirstBasePlayerData()
    {
        userData.player = firstPlayerData;
    }
    
    // 기본 인벤토리 데이터
    private void BaseInventoryData()
    {
        InventoryData.hardDonutCodexDataList = new List<DonutCodexData>();
        InventoryData.softDonutCodexDataList = new List<DonutCodexData>();
        InventoryData.moistDnutCodexDataList = new List<DonutCodexData>();

        foreach (DonutType type in Enum.GetValues(typeof(DonutType)))
        {
            for (int level = 1; level <= 30; level++)
            {
                var codex = new DonutCodexData
                {
                    id = $"{type}_{level}",
                    donutDexViewState = DonutDexViewState.Question
                };

                switch (type)
                {
                    case DonutType.Hard:
                        InventoryData.hardDonutCodexDataList.Add(codex);
                        break;

                    case DonutType.Soft:
                        InventoryData.softDonutCodexDataList.Add(codex);
                        break;

                    case DonutType.Moist:
                        InventoryData.moistDnutCodexDataList.Add(codex);
                        break;
                }
            }
        }
        //Debug.Log("실행완료");
    }

    private void FirstBaseInventoryData()
    {
        //EnsureDonutSlots();
        userData.inventory = firstInventoryData;
    }
    
    
    // 기본 머지보드 데이터
    private void BaseMergeBoardData(int cellMax, int cellWidth, int cellLength)
    {
        MergeBoardData.cellMax = cellMax;
        MergeBoardData.cellWidth = cellWidth;
        MergeBoardData.cellLength = cellLength;
    }
    
    // 처음 머지보드 데이터 셋
    private void FirstBaseMergeBoardData()
    {
        MergeBoardData.cells = new List<CellData>();

        for(int x = 0; x < MergeBoardData.cellLength; x++) {

            for (int y = 0; y < MergeBoardData.cellWidth; y++)
            {
                CellData cellData = new CellData()
                {
                    x = x,
                    y = y,
                    isCellActive = false,
                    donutId = null,
                    isQuestActive = false,
                };

                MergeBoardData.cells.Add(cellData);
            }
        }
    }

    // 기본 퀘스트 데이터
    private void BaseQuestData(int baseGold, int RefreshCount, int MaxCount)
    {
        QuestData.baseGold = baseGold;
        QuestData.refreshCount = RefreshCount;
        QuestData.maxCount = MaxCount;
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
                //Debug.LogWarning("변경할 필드가 없습니다.");
                return;
            }
            if(patch.ContainsKey("energy"))
                patch["lastAt"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            // var docRef = db.Collection(userCollection).Document(docId);
            // await docRef.UpdateAsync(patch);
            await db.Collection(userCollection).Document(docId).UpdateAsync(patch);
            //Debug.Log($"부분 업데이트 완료: /{userCollection}/{docId}");
            
            bool rankChanged = patch.ContainsKey("soloScore") || patch.ContainsKey("soloTier");
            if (rankChanged)
            {
                await UpsertLeader(PlayerData.soloScore, PlayerData.soloTier);
                //Debug.Log($"[FS] Rank 동기화 완료: /{Season}_{gameMode.ToString().ToLower()}/{docId}");
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
            PlayerData.lastAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            var docRef = db.Collection(userCollection).Document(docId);
            await docRef.SetAsync(userData, SetOptions.MergeAll);
            await UpsertLeader(PlayerData.soloScore, PlayerData.soloTier);
            //Debug.Log($"[FS] 전체 저장 완료: /{userCollection}/{docId}");
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

    // 계정 삭제
    public async Task DeleteUserDataAsync()
    {
        var docRef = db.Collection("user").Document(docId);
        await docRef.DeleteAsync();
        Debug.Log("[DataManager] Firestore 유저 데이터 삭제 완료");
    }
    
    // 도넛 생성기 OR 머지 후 도감 등록
    public void AddCodexEntry(DonutType donutType, string donutId, int level)
    {
        level = level - 1;
        switch (donutType)
        {
            case DonutType.Hard:
                if (InventoryData.hardDonutCodexDataList[level].donutDexViewState == DonutDexViewState.Question)
                {
                    InventoryData.hardDonutCodexDataList[level] = new DonutCodexData()
                    {
                        id = donutId,
                        donutDexViewState = DonutDexViewState.Reward
                    };   
                }
                break;
            case DonutType.Soft:
                if (InventoryData.softDonutCodexDataList[level].donutDexViewState == DonutDexViewState.Question)
                {
                    InventoryData.softDonutCodexDataList[level] = new DonutCodexData()
                    {
                        id = donutId,
                        donutDexViewState = DonutDexViewState.Reward
                    };   
                }
                break;
            case DonutType.Moist:
                if (InventoryData.moistDnutCodexDataList[level].donutDexViewState == DonutDexViewState.Question)
                {
                    InventoryData.moistDnutCodexDataList[level] = new DonutCodexData()
                    {
                        id = donutId,
                        donutDexViewState = DonutDexViewState.Reward
                    };   
                }
                break;
        }
    }
    
    // 도넛 SO 데이터 불러오기
    
    // SO를 불러와 캐싱
    private void LoadAllDonutData()
    {
        var allSO = Resources.LoadAll<DonutTypeSO>("DonutData");

        foreach (var so in allSO)
        {
            //if (so.type == DonutType.Gift)
            //    continue; // Gift 타입 완전 제외

            if (!_donutTypeDB.ContainsKey(so.type))
            {
                _donutTypeDB.Add(so.type, so);
                //Debug.Log($"[DonutDB] Loaded {so.type} ({so.levels.Count} levels)");
            }
        }
        
        var allSo2 = Resources.LoadAll<GiftBoxSO>("DonutData");

        foreach (var so in allSo2)
        {
            if (!_giftBoxList.Contains(so))
            {
                _giftBoxList.Add(so);
            }
        }
    }
   
    // DonutData 가져오기
    public DonutData GetDonutData(DonutType type, int level)
    {
        if (_donutTypeDB.TryGetValue(type, out var so))
        {
            return so.GetLevelData(level);
        }

        //Debug.LogWarning($"[DonutDB] No data found for {type} level {level}");
        return null;
    }

    // GiftBoxSo 데이터 가져오기
    public GiftBoxData GetGiftBoxData(int level)
    {
        foreach (var so in _giftBoxList)
        {
            if (so.type == DonutType.Gift)
            {
                return so.GetLevelData(level); // GiftBoxData 반환
            }
        }

        Debug.LogWarning($"[DonutDB] GiftBoxData not found for level {level}");
        return null;
    }

    public GiftBoxData GetGiftBoxDataByID(string id) //기프트박스 데이터 변환
    {
        // id 형식이 "Gift_1", "Gift_2" 이런 형태라고 가정
        if (string.IsNullOrEmpty(id))
            return null;

        if (id.StartsWith("Gift_"))
        {
            string levelStr = id.Replace("Gift_", "");

            if (int.TryParse(levelStr, out int level))
            {
                // ⭐ 기존 함수 그대로 호출
                return GetGiftBoxData(level);
            }
        }

        return null;
    }

    // ID 기반으로 DonutData 가져오기 (ex: "Hard_3")
    public DonutData GetDonutByID(string id)
    {
        string[] parts = id.Split('_');
        if (parts.Length != 2) return null;

        if (System.Enum.TryParse(parts[0], out DonutType type) &&
            int.TryParse(parts[1], out int level))
        {
            return GetDonutData(type, level);
        }
        return null;
    }

    // 다음 단계 도넛 자동 가져오기 (머지 시 사용)
    public DonutData GetNextDonut(DonutData current)
    {
        if (current == null) return null;

        var next = GetDonutData(current.donutType, current.level + 1);
        return next;
    }

    // 생성기 레벨따라 도넛생성
    public List<DonutData> GetDonutsByTypeAndLevel(DonutType type, int level)
    {
        List<DonutData> result = new();

        // 타입 DB가 있는지 확인
        if (_donutTypeDB.TryGetValue(type, out var typeDB))
        {
            // 해당 타입에서 레벨 데이터 가져오기
            var data = typeDB.GetLevelData(level);

            if (data != null)
                result.Add(data);
        }

        return result;
    }

    // 인벤토리 데이터 관련 함수들
    /// <summary>
    /// 도넛 리스트가 비어있으면 5칸 초기화
    /// </summary>
    public void EnsureDonutSlots()
    {
        if (InventoryData.donutEntries == null)
            InventoryData.donutEntries = new List<DonutEntry>();

        if (InventoryData.donutEntries.Count == 0)
        {
            for (int i = 0; i < 5; i++)
            {
                InventoryData.donutEntries.Add(new DonutEntry()
                {
                    id = "Hard_1",
                    type = DonutType.Hard,   // 너가 정의한 기본값
                    weight = 0,
                    resilience = 0,
                    friction = 0
                });
            }
        }
    }

    /// <summary>
    /// 특정 슬롯(index)에 도넛 넣기
    /// </summary>
    public void SetDonutAt(int index, bool isDonutEntry = true, DonutEntry entry = null, DonutData donutData = null)
    {
        EnsureDonutSlots();

        //index = index - 1;
        if (index < 0 || index >= InventoryData.donutEntries.Count)
        {
            Debug.LogError($"[InventoryData] 잘못된 인덱스: {index}");
            return;
        }

        if (isDonutEntry)
        {
            //Debug.Log("앤트리 들어옴");
            InventoryData.donutEntries[index] = entry;
        }
        else
        {
            //Debug.Log("도넛데이터 들어옴");
            // Debug.Log(donutData.id);
            // Debug.Log(donutData.donutType);
            // Debug.Log(donutData.weight);
            // Debug.Log(donutData.friction);
            InventoryData.donutEntries[index].id = donutData.id;
            InventoryData.donutEntries[index].type = donutData.donutType;
            InventoryData.donutEntries[index].weight = donutData.weight;
            InventoryData.donutEntries[index].friction = donutData.friction;
            InventoryData.donutEntries[index].resilience =donutData.resilience;
        }
        
    }

    /// <summary>
    /// 게임 매니저로부터 받은 변경된 인벤토리와 점수 데이터를 적용하고 DB에 저장합니다.
    /// 게임진입시 선반영하는 패널티요소를 위한 동작으로 한번만 수행됨
    /// </summary>
    public async Task ApplyPenaltyData(List<DonutEntry> updatedDonutEntries, int newScore)
    {
        // 1. 로컬 데이터 업데이트
        InventoryData.donutEntries = updatedDonutEntries;
        PlayerData.soloScore = newScore;

        // 2. Firestore에 모든 변경사항 저장
        await SaveAllUserDataAsync();
        
        // 3. 로컬 리스너에게 변경사항 알림
        OnUserDataChanged?.Invoke(PlayerData);
        OnUserDataRootChanged?.Invoke(userData);
        Debug.Log($"페널티 데이터 적용 완료. 현재 점수: {newScore}");
    }

    
    //생성기 레벨 받아오기
    public int GetGeneratorLevel(DonutType type)
    {
        var board = userData.mergeBoard;

        return type switch
        {
            DonutType.Hard => board.generatorLevelHard,
            DonutType.Soft => board.generatorLevelSoft,
            DonutType.Moist => board.generatorLevelMoist,
            _ => 1
        };
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
    // public async Task UpsertLeader(int score, GameTier tier, GameMode? mode = null, string docid = null, string nickname = null)
    // {
    //     mode = mode ?? gameMode;
    //     docid = null ?? docId;
    //     nickname = null ?? PlayerData.nickname;
    //     var doc = LeadersCol(mode).Document(docid);
    //     await doc.SetAsync(new Dictionary<string, object> {
    //         { "nickname", nickname }, 
    //         { "score", score }, 
    //         { "tier", tier.ToString().ToLower() }, 
    //         { "updatedAt", FieldValue.ServerTimestamp }
    //     }, SetOptions.MergeAll);
    // }

    public async Task UpsertLeader(
        int? score = null, 
        GameTier? tier = null, 
        GameMode? mode = null, 
        string docid = null, 
        string nickname = null)
    {
        mode     ??= gameMode;            // 현재 게임 모드 (SOLO/DUO 등)
        docid    ??= docId;               // 현재 유저 UID
        nickname ??= PlayerData.nickname; // 현재 닉네임

        score ??= PlayerData.soloScore;   // 기본: 솔로 점수
        tier  ??= PlayerData.soloTier;    // 기본: 솔로 티어

        var data = new RankData
        {
            nickname  = nickname,
            score     = score.Value,
            tier      = tier.Value,
            uid       = docid
        };

        rankData = data;
        var docRef = LeadersCol(mode.Value).Document(docid);
        await docRef.SetAsync(data, SetOptions.MergeAll);

        Debug.Log($"[Rank] UpsertLeader 완료: mode={mode}, uid={docid}, score={data.score}, tier={data.tier}");
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

    private DocumentReference RankSummaryDoc =>
    db.Collection("rank_summaries").Document(Season);

    // GameMode → 필드 이름 매핑
    private string GetSummaryFieldName(GameMode mode)
    {
        return mode switch
        {
            GameMode.SOLO => "soloTop100",
            GameMode.DUO  => "duoTop100",
            GameMode.TRIO => "trioTop100"
        };
    }

    /// <summary>
    /// leaders 컬렉션 기준으로 Top100 다시 계산해서
    /// rank_summaries/{Season} 의 해당 모드 필드만 갱신
    /// </summary>
    public async Task RebuildRankSummaryAsync(GameMode mode, int limit = 100)
    {
        // 1) leaders에서 점수 내림차순 Top100 가져오기
        var col = LeadersCol(mode); // 이미 있는 함수라 했지?
        var query = col.OrderByDescending("score").Limit(limit);
        var snap = await query.GetSnapshotAsync();

        var topList = new List<Dictionary<string, object>>();

        foreach (var doc in snap.Documents)
        {
            var data = doc.ConvertTo<RankData>();

            topList.Add(new Dictionary<string, object>
            {
                { "nickname", data.nickname },
                { "score",    data.score },
                { "tier",     data.tier },
                { "uid",      data.uid }
            });
        }

        // 2) rank_summaries/{Season} 문서에 해당 모드 배열만 갈아끼우기
        string fieldName = GetSummaryFieldName(mode);

        var patch = new Dictionary<string, object>
        {
            { fieldName,  topList },
            { "updatedAt", FieldValue.ServerTimestamp }
        };

        await RankSummaryDoc.SetAsync(patch, SetOptions.MergeAll);

        Debug.Log($"[Rank] RebuildRankSummaryAsync: mode={mode}, count={topList.Count}");
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
        }
        PauseChanged?.Invoke(pause);
    }

    // 포커스 잃을 떄
    // void OnApplicationFocus(bool hasFocus)
    // {
    //     if (!hasFocus)
    //     {
    //         _ = SaveAllUserDataAsync();
    //     }
    //     PauseChanged?.Invoke(hasFocus);
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

    // 주어진 uId로 UserDataRoot를 가져오는 메서드 추가
    public async Task<UserDataRoot> GetUserDataRootAsync(string uId)
    {
        try
        {
            DocumentSnapshot userDoc = await db.Collection(userCollection).Document(uId).GetSnapshotAsync();
            if (userDoc.Exists)
            {
                return userDoc.ConvertTo<UserDataRoot>();
            }
            else
            {
                Debug.LogWarning($"[FS] User data not found for uId: {uId}");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS][READ][ERR] Failed to get user data for {uId}: {e}");
            return null;
        }
    }
        
}
