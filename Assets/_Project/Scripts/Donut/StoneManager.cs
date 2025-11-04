using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// 이 스크립트는 씬에 있는 모든 돌의 생성, 관리, 발사를 담당합니다.
public class StoneManager : MonoBehaviour
{
    [SerializeField] private GameObject stonePrefabA; // 플레이어 A(호스트)의 돌 프리팹
    [SerializeField] private GameObject stonePrefabB; // 플레이어 B의 돌 프리팹
    [SerializeField] private Transform spawnPosition; // 돌이 생성될 위치

    private Game gameReference;

    private Dictionary<int, StoneForceController_Firebase> _stoneControllers_A = new Dictionary<int, StoneForceController_Firebase>();
    private Dictionary<int, StoneForceController_Firebase> _stoneControllers_B = new Dictionary<int, StoneForceController_Firebase>();
    private StoneForceController_Firebase _currentTurnStone;
    public StoneForceController_Firebase.Team _currentTurnStoneTeam { get; private set; }
    public StoneForceController_Firebase.Team myTeam { get; private set; } = StoneForceController_Firebase.Team.None;
    private string myUserId; // 현재 플레이어의 ID
    private int myShotCount = -1;

    void Awake()
    {
        myUserId = FirebaseAuthManager.Instance.UserId; // Firebase 인증에서 ID 가져오기
    }

    // 새 턴이 시작될 때 돌을 생성하고 초기화합니다.
    public Rigidbody SpawnStoneForTurn(Game game)
    {
        
        Vector3 startPos = spawnPosition.position;
        gameReference = game;
        // 상대방 샷 시 시뮬레이션 위치 적용
        if (game.CurrentTurnPlayerId != myUserId && game.LastShot != null && game.LastShot.ReleasePosition != null)
        {
            
            // 이성준 수정
            // Debug.Log($"상대방의 릴리즈 위치({game.LastShot.ReleasePosition["z"]})에서 돌을 생성합니다.");
            // startPos = new Vector3(
            //     game.LastShot.ReleasePosition["x"],
            //     game.LastShot.ReleasePosition["y"],
            //     game.LastShot.ReleasePosition["z"]
            // );
        }
        else
        {
            Debug.Log($"{game.CurrentTurnPlayerId}의 턴을 위한 돌을 기본 위치에 생성합니다.");
        }

        // 현재 턴의 플레이어가 누구인지 확인하여 프리팹과 팀 결정
        string currentTurnPlayerId = game.CurrentTurnPlayerId;
        GameObject selectedPrefab = null;
        _currentTurnStoneTeam = StoneForceController_Firebase.Team.A;
        
        int currentDonutId = myShotCount + 1; // 던질 도넛의 ID를 설정하는 부분. 

        if (game.PlayerIds[0] == currentTurnPlayerId)
        {
            selectedPrefab = stonePrefabA;
            _currentTurnStoneTeam = StoneForceController_Firebase.Team.A;
            currentDonutId = game.StonesUsed[currentTurnPlayerId];
        }
        else if (game.PlayerIds[1] == currentTurnPlayerId)
        {
            selectedPrefab = stonePrefabB;
            _currentTurnStoneTeam = StoneForceController_Firebase.Team.B;
            currentDonutId = game.StonesUsed[currentTurnPlayerId];
        }

        if (myTeam == StoneForceController_Firebase.Team.None) // 아직 자기 팀이 뭔지 제대로 설정이 안되어있으면, 위에서 판별한대로 자기팀을 설정 
        {
            myTeam = _currentTurnStoneTeam;
        }

        if (selectedPrefab == null)
        {
            Debug.LogError("돌 프리팹이 할당되지 않았습니다! 인스펙터에서 할당해주세요.");
            return null;
        }

        // 돌 생성 및 컨트롤러 초기화
        GameObject newStone = Instantiate(selectedPrefab, startPos, spawnPosition.rotation);
        _currentTurnStone = newStone.GetComponent<StoneForceController_Firebase>();
        
        
        if (_currentTurnStone == null)
        {
            Debug.LogError("생성된 돌 프리팹에 StoneForceController_Firebase 컴포넌트가 없습니다!");
            Destroy(newStone);
            return null;
        }

        _currentTurnStone.InitializeDonut(_currentTurnStoneTeam, currentDonutId);
        if (_currentTurnStoneTeam == StoneForceController_Firebase.Team.A)
        {
            _stoneControllers_A[currentDonutId] = _currentTurnStone;
        }
        else if(_currentTurnStoneTeam == StoneForceController_Firebase.Team.B)
        {
            _stoneControllers_B[currentDonutId] = _currentTurnStone;
        }
        else
        {
            Debug.Log("팀 설정 오류");
        }

        return newStone.GetComponent<Rigidbody>();
    }

    // FirebaseGameManager가 샷 데이터를 받았을 때 호출할 함수
    // 서버/클라이언트로부터 받은 샷 데이터로 돌을 발사.
    public void LaunchStone(LastShot shotData, int stoneId)
    {
        StoneForceController_Firebase donutToLaunch;
        if (shotData.Team == StoneForceController_Firebase.Team.A)
        {
            if (!_stoneControllers_A.TryGetValue(stoneId, out var donutForceController))
            {
                Debug.LogError($"발사할 돌(Team : {shotData.Team},  (ID: {stoneId})을 찾을 수 없습니다!");
                return;
            }
            donutToLaunch = donutForceController;
        }
        else if (shotData.Team == StoneForceController_Firebase.Team.B)
        {
            if (!_stoneControllers_B.TryGetValue(stoneId, out var donutForceController))
            {
                Debug.LogError($"발사할 돌(Team : {shotData.Team},  (ID: {stoneId})을 찾을 수 없습니다!");
                return;
            }
            donutToLaunch = donutForceController;
        }
        else
        {
            Debug.Log("팀 값이 LaunchStone 함수에 안넘어옴");
            return;
        }
        

        Debug.Log($"StoneManager: 샷 데이터로 돌(ID: {stoneId})을 발사합니다.");
        Debug.Log($"stoneused , {gameReference.StonesUsed[gameReference.PlayerIds[0]]}, {gameReference.StonesUsed[gameReference.PlayerIds[1]]}  ");

        // 발사 전, Rigidbody의 isKinematic을 false로 설정하여 물리 효과를 받도록 합니다.
        Rigidbody rb = donutToLaunch.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero; // 기존 속도 초기화
            rb.angularVelocity = Vector3.zero;
        }

        // Dictionary를 Vector3로 변환
        Vector3 direction = new Vector3(shotData.Direction["x"], shotData.Direction["y"], shotData.Direction["z"]);
        
        donutToLaunch.AddForceToStone(direction, shotData.Force, shotData.Spin);

        // 발사 후 모든 돌의 움직임 감지 시작 (시뮬레이션 완료 모니터링)
        StartCoroutine(MonitorSimulation());
    }

    // 모든 돌의 속도를 체크하여 시뮬레이션 완료 시점 감지
    private System.Collections.IEnumerator MonitorSimulation()
    {
        yield return new WaitForSeconds(0.5f); // 물리 시뮬레이션이 안정적으로 시작될 때까지 잠시 대기
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            bool allStonesStopped = true;
            foreach (var controller in _stoneControllers_A.Values)
            {
                if (controller != null)
                {
                    var rb = controller.GetComponent<Rigidbody>();
                    float velocity = rb.velocity.magnitude;
                    Debug.Log($"[Monitor] Checking stone: {controller.gameObject.name}, Velocity: {velocity}"); // 진단용 로그 추가
                    if (velocity > 0.01f)
                    {
                        allStonesStopped = false;
                        break;
                    }
                }
            }
            
            foreach (var controller in _stoneControllers_B.Values)
            {
                if (controller != null)
                {
                    var rb = controller.GetComponent<Rigidbody>();
                    float velocity = rb.velocity.magnitude;
                    Debug.Log($"[Monitor] Checking stone: {controller.gameObject.name}, Velocity: {velocity}"); // 진단용 로그 추가
                    if (velocity > 0.01f)
                    {
                        allStonesStopped = false;
                        break;
                    }
                }
            }

            if (allStonesStopped)
            {
                break; // 모든 돌이 멈췄으면 루프 탈출
            }

            yield return wait; // 0.2초 간격으로 다시 확인
        }

        // 시뮬레이션 완료 후 서버에 위치 전송
        FirebaseGameManager.Instance.OnSimulationComplete(GetAllStonePositions());
    }

    // 서버에서 받은 최종 위치로 돌들을 동기화.
    public void SyncPositions(List<StonePosition> serverPositions)
    {
        Debug.Log("서버의 최종 위치로 모든 돌을 동기화합니다.");
        StoneForceController_Firebase fc;
        Rigidbody rb;
        for (int i = 0; i < serverPositions.Count; i++)
        {
            if (i == 0 || (i % 2 == 0))
            {
                fc = _stoneControllers_A[i / 2];
                rb = fc.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                fc.transform.position = new Vector3(serverPositions[i].Position["x"], serverPositions[i].Position["y"], serverPositions[i].Position["z"]);
            }
            else
            {
                fc = _stoneControllers_B[(i / 2)];
                rb = fc.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                fc.transform.position = new Vector3(serverPositions[i].Position["x"], serverPositions[i].Position["y"], serverPositions[i].Position["z"]);
            }
        }
        
    }

    // 시뮬레이션 완료 시 모든 돌의 최종 위치를 수집하는 함수
    public List<StonePosition> GetAllStonePositions()
    {
        var positions = new List<StonePosition>();
        int maxLength = Math.Max(_stoneControllers_A.Count, _stoneControllers_B.Count);
        StoneForceController_Firebase sfc;
        for (int i = 0; i < maxLength; i++)
        {
            if (i < _stoneControllers_A.Count)
            {
                sfc = _stoneControllers_A[i];
                positions.Add(new StonePosition
                {
                    StoneId = sfc.donutId,
                    Team = sfc.team.ToString(), // Team enum을 문자열로 저장
                    Position = new Dictionary<string, float>
                    {
                        { "x", sfc.transform.position.x },
                        { "y", sfc.transform.position.y },
                        { "z", sfc.transform.position.z }
                    }
                });
            }

            if (i < _stoneControllers_B.Count)
            {
                sfc = _stoneControllers_B[i];
                positions.Add(new StonePosition
                {
                    StoneId = sfc.donutId,
                    Team = sfc.team.ToString(), // Team enum을 문자열로 저장
                    Position = new Dictionary<string, float>
                    {
                        { "x", sfc.transform.position.x },
                        { "y", sfc.transform.position.y },
                        { "z", sfc.transform.position.z }
                    }
                });
            }
        }
        
        return positions;
    }

    public StoneForceController_Firebase GetDonutToLaunch(int donutId) // 상대 입장에서 시뮬레이션 돌릴때 호출되는 함수
    {
        if (!_stoneControllers_B.TryGetValue(donutId, out var donutToLaunch))
        {
            Debug.LogError($"발사할 돌(ID: {donutId})을 찾을 수 없습니다!");
            return null;
        }
        
        return donutToLaunch;
        
    }
}
