using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EPOOutline;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// 이 스크립트는 씬에 있는 모든 돌의 생성, 관리, 발사를 담당합니다.
public class StoneManager : MonoBehaviour
{
    //[SerializeField] private GameObject stonePrefabA; // 플레이어 A(호스트)의 돌 프리팹
    //[SerializeField] private GameObject stonePrefabB; // 플레이어 B의 돌 프리팹
    [SerializeField] private Transform spawnPosition; // 돌이 생성될 위치
    [SerializeField] private GameObject donutSamplePrefab; // 도넛 반지름을 미리 계산해놓기 위한 프리팹.

    private Game gameReference;

    private Dictionary<int, StoneForceController_Firebase> _stoneControllers_A =
        new Dictionary<int, StoneForceController_Firebase>();

    private Dictionary<int, StoneForceController_Firebase> _stoneControllers_B =
        new Dictionary<int, StoneForceController_Firebase>();

    private StoneForceController_Firebase _currentTurnStone;
    public Scr_Collider_House _scr_Collider_House;
    private UI_LaunchIndicator_Firebase _uilaunchIndicator;
    public StoneForceController_Firebase.Team _currentTurnStoneTeam { get; private set; }
    public StoneForceController_Firebase.Team myTeam { get; private set; } = StoneForceController_Firebase.Team.None;
    private List<StonePosition> lastStonePosition = new List<StonePosition>();

    [Header("아웃 판정을 위한 라인 오브젝트 참조")]
    public Transform endHogLine;
    public Transform backLine;
    public string myUserId { get; private set; } // 현재 플레이어의 ID
    public int aShotIndex { get; private set; } = -1; // 인덱스를 정하기 위함이라 0부터 시작, 진짜 발사 개수는 +1
    public int bShotIndex { get; private set; } = -1;
    public int aScore { get; private set; }
    public int bScore { get; private set; }
    public int roundCount { get; private set; }

    public float donutColliderRadius { get; private set; }



    void Awake()
    {
        myUserId = FirebaseAuthManager.Instance.UserId; // Firebase 인증에서 ID 가져오기
        _uilaunchIndicator = transform.GetComponent<UI_LaunchIndicator_Firebase>();
    }

    private void Start()
    {
        // 아웃판정에 사용될 값(도넛의 반지름)을 미리 계산해둠
        donutColliderRadius = CalculateDonutColliderRadius(donutSamplePrefab); 
    }

    private float CalculateDonutColliderRadius(GameObject donut)
    {
        CapsuleCollider capsuleCollider = donut.GetComponent<CapsuleCollider>();
        // 1. Capsule Collider의 로컬 반지름 값
        float localRadius = capsuleCollider.radius;

        // 2. 오브젝트의 Transform 컴포넌트 월드 스케일(Scale) 가져오기
        Vector3 lossyScale = capsuleCollider.transform.lossyScale;

        // 3. 캡슐 콜라이더는 기본적으로 X와 Z 스케일에 영향을 받음 (수직 방향 제외)
        // 두 축 중 더 큰 스케일 값을 찾습니다.
        // 이는 오브젝트가 비균일하게 스케일 되었을 때, 가장 크게 늘어난 쪽의 반지름을 구하기 위함입니다.
        float maxScale = Mathf.Max(lossyScale.x, lossyScale.z); 

        // 4. 로컬 반지름에 최대 스케일 값을 곱하여 최종 월드 반지름을 계산합니다.
        float worldRadius = localRadius * maxScale;

        return worldRadius;
        // MeshCollider meshCollider = donut.GetComponent<MeshCollider>();
        //
        // if (meshCollider == null)
        // {
        //     Debug.LogError("MeshCollider를 찾을 수 없습니다.");
        //     return 0f;
        // }
        //
        // // 1. 콜라이더의 바운딩 박스 가져오기 (월드 좌표 기준)
        // Bounds bounds = meshCollider.bounds;
        //
        // // 2. 바운딩 박스의 절반 크기 (Extent) 가져오기
        // Vector3 extents = bounds.extents;
        //
        // // 3. 좌우 반지름은 보통 X축이나 Z축의 절반 크기(Extent)와 같습니다.
        // // 실린더가 Y축을 따라 세워져 있다고 가정하고, X와 Z 중 큰 값을 반지름으로 사용합니다.
        // // (스케일이 균일하지 않을 수 있으므로 Max를 사용해 안전하게 처리)
        // float radiusApproximation = Mathf.Max(extents.x, extents.z);
        //
        // // extents는 이미 바운딩 박스의 절반 크기(중앙에서 끝까지의 거리)이므로, 
        // // 이것이 곧 월드 좌표 기준의 반지름 근사치입니다.
        //
        // return radiusApproximation;

    }

    // 새 라운드가 시작될 때 돌을 생성하고 초기화합니다.
    // playerId가 인자로 넘어오면, 현재 턴과 무관하게 해당 플레이어의 프리팹을 생성
    // playerId가 인자로 넘어오지 않으면 현재 턴에 해당하는 프리팹을 생성
    public Rigidbody SpawnStone(Game game, DonutEntry selectedDonut, string playerId = null)
    {
        Vector3 startPos = spawnPosition.position;
        gameReference = game;

        // 내 턴이 왔지만, 이미 내가 지정된 횟수만큼 발사를 완료한 상태일때. 즉, 라운드가 끝나고 다시 자신의 턴이 돌아왔을때
        if (myTeam == StoneForceController_Firebase.Team.A && aShotIndex >= FirebaseGameManager.Instance.shotsPerRound)
        {
            return null;
        }
        else if (myTeam == StoneForceController_Firebase.Team.B && bShotIndex >= FirebaseGameManager.Instance.shotsPerRound)
        {
            return null;
        }

        if (myTeam == StoneForceController_Firebase.Team.None)
        {
            myTeam = (game.PlayerIds[0] == myUserId) ? StoneForceController_Firebase.Team.A : StoneForceController_Firebase.Team.B;
        }

        GameObject selectedPrefab;
        int currentDonutId = 0;
        string currentTurnPlayerId;

        // 선택된 도넛 엔트리에서 실제 도넛 데이터를 가져와 프리팹을 결정합니다.
        DonutData donutDataToSpawn = DataManager.Instance.GetDonutByID(selectedDonut.id);
        if (donutDataToSpawn == null || donutDataToSpawn.prefab == null)
        {
            Debug.LogError($"선택된 도넛({selectedDonut.id})에 해당하는 프리팹을 찾을 수 없습니다!");
            return null;
        }
        selectedPrefab = donutDataToSpawn.prefab;

        if (playerId == null)
        {
            currentTurnPlayerId = game.CurrentTurnPlayerId;
            if (game.PlayerIds[0] == currentTurnPlayerId)
            {
                _currentTurnStoneTeam = StoneForceController_Firebase.Team.A; // 현재 턴은 어느 팀의 차례인지 변수에 넣어줌, A팀 차례
                aShotIndex++;
                currentDonutId = aShotIndex;
            }
            else if (game.PlayerIds[1] == currentTurnPlayerId)
            {
                _currentTurnStoneTeam = StoneForceController_Firebase.Team.B; // 현재 턴은 어느 팀의 차례인지 변수에 넣어줌, B팀 차례
                bShotIndex++;
                currentDonutId = bShotIndex;
            }
            else
            {
                Debug.Log("currentTurnPlayerId 비교 관련 오류");
                return null;

            }
        }
        else
        {
            currentTurnPlayerId = myUserId;
            // 파라미터로 받은 playerId를 기준으로 돌을 생성합니다.
            if (game.PlayerIds[0] == playerId)
            {
                _currentTurnStoneTeam = StoneForceController_Firebase.Team.A;
                aShotIndex++;
                currentDonutId = aShotIndex;
            }
            else
            {
                _currentTurnStoneTeam = StoneForceController_Firebase.Team.B;
                bShotIndex++;
                currentDonutId = bShotIndex;
            }

            if (selectedPrefab == null)
            {
                Debug.LogError("돌 프리팹이 할당되지 않았습니다! 인스펙터에서 할당해주세요.");
                return null;
            }
        }


        GameObject newStone = Instantiate(selectedPrefab, startPos, spawnPosition.rotation);
        _currentTurnStone = newStone.GetComponent<StoneForceController_Firebase>();
        newStone.transform.GetComponent<Scr_DonutParticleSystem>().InitializeDonutParticles(_currentTurnStoneTeam);

        if (_currentTurnStone == null)
        {
            Debug.LogError("생성된 돌 프리팹에 StoneForceController_Firebase 컴포넌트가 없습니다!");
            Destroy(newStone);
            return null;
        }

        // 선택된 도넛의 물리 속성을 StoneForceController_Firebase에 전달합니다.
        _currentTurnStone.InitializeDonut(_currentTurnStoneTeam, currentDonutId, selectedDonut.id, selectedDonut.weight, selectedDonut.resilience, selectedDonut.friction);

        if (currentTurnPlayerId == game.PlayerIds[0]) // 현재 턴의 주인이 A팀(방장) 이면
        {
            _stoneControllers_A[currentDonutId] = _currentTurnStone;

            //Debug.Log("A팀");
            //Debug.Log(_stoneControllers_A.Count);
            //Debug.Log($"my user id = {myUserId}");
            //Debug.Log($"currentTurnPlayerId = {currentTurnPlayerId}, RoundStarting = {game.RoundStartingPlayerId}");
        }

        else
        {
            _stoneControllers_B[currentDonutId] = _currentTurnStone;

            //Debug.Log("B팀");
            //Debug.Log(_stoneControllers_B.Count);
            //Debug.Log($"my user id = {myUserId}");
            //Debug.Log($"currentTurnPlayerId = {currentTurnPlayerId}, RoundStarting = {game.RoundStartingPlayerId}");
        }

        return newStone.GetComponent<Rigidbody>();
    }



    // FirebaseGameManager가 샷 데이터를 받았을 때 호출할 함수
    // 서버/클라이언트로부터 받은 샷 데이터로 돌을 발사.
    public void LaunchStone(LastShot shotData, int stoneId)
    {
        StoneForceController_Firebase donutToLaunch;

        if (shotData.PlayerId == gameReference.PlayerIds[0])
        {
            if (!_stoneControllers_A.TryGetValue(stoneId, out var donutForceController))
            {
                Debug.LogError($"발사할 돌(Team : {shotData.Team},  (ID: {stoneId})을 찾을 수 없습니다!");
                return;
            }

            donutToLaunch = donutForceController;
        }
        else
        {
            if (!_stoneControllers_B.TryGetValue(stoneId, out var donutForceController))
            {
                Debug.LogError($"발사할 돌(Team : {shotData.Team},  (ID: {stoneId})을 찾을 수 없습니다!");
                Debug.Log($"{_stoneControllers_B.Count}");
                return;
            }

            donutToLaunch = donutForceController;
        }



        //Debug.Log($"StoneManager: 샷 데이터로 돌(ID: {stoneId})을 발사합니다.");
        //Debug.Log(
        //    $"stoneused , {gameReference.DonutsIndex[gameReference.PlayerIds[0]]}, {gameReference.DonutsIndex[gameReference.PlayerIds[1]]}  ");

        // 발사 전, Rigidbody의 isKinematic을 false로 설정하여 물리 효과를 받도록 합니다.

        if (!donutToLaunch.TryGetComponent<Rigidbody>(out Rigidbody rb)) // 못찾으면 그냥 턴 종료
        {
            Debug.Log("Rigidbody를 찾을 수 없음");
            StartCoroutine(MonitorSimulation(shotData.Team));
            return; // 여기 이후의 코드 진행을 멈춤
        }
        
        rb.isKinematic = false;
        rb.velocity = Vector3.zero; // 기존 속도 초기화
        rb.angularVelocity = Vector3.zero;
        

        // Dictionary를 Vector3로 변환
        Vector3 direction = new Vector3(shotData.Direction["x"], shotData.Direction["y"], shotData.Direction["z"]);

        donutToLaunch.AddForceToStone(direction, shotData.Force, shotData.Spin);

        // 발사 후 모든 돌의 움직임 감지 시작 (시뮬레이션 완료 모니터링)
        StartCoroutine(MonitorSimulation(shotData.Team));
    }

    // 모든 돌의 속도를 체크하여 시뮬레이션 완료 시점 감지
    private IEnumerator MonitorSimulation(StoneForceController_Firebase.Team team)
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
                    //var rb = controller.GetComponent<Rigidbody>();
                    if (!controller.TryGetComponent<Rigidbody>(out Rigidbody rb)) // 못찾으면
                    {
                        continue;
                    }
                    float velocity = rb.velocity.sqrMagnitude;
                    // Debug.Log(
                    //     $"[Monitor] Checking stone: {controller.gameObject.name}, Velocity: {velocity}"); // 진단용 로그 추가
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
                    //var rb = controller.GetComponent<Rigidbody>();
                    if (!controller.TryGetComponent<Rigidbody>(out Rigidbody rb)) // 못찾으면
                    {
                        continue;
                    }
                    float velocity = rb.velocity.sqrMagnitude;
                    // Debug.Log(
                    //     $"[Monitor] Checking stone: {controller.gameObject.name}, Velocity: {velocity}"); // 진단용 로그 추가
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

        foreach (var controller in _stoneControllers_A.Values)
        {
            if (controller != null)
            {
                controller.MoveFinishedInTurn();
            }
        }

        foreach (var controller in _stoneControllers_B.Values)
        {
            if (controller != null)
            {
                controller.MoveFinishedInTurn();
            }
        }

        if (FirebaseGameManager.Instance._isMyTurn && FirebaseGameManager.Instance.CurrentLocalState == "SimulatingMyShot")
        {
            FirebaseGameManager.Instance.ChangeState_To_WaitingForPrediction();
        }
        else
        {
            FirebaseGameManager.Instance.OnSimulationComplete(GetAllStonePositions());
        }

    }

    // 서버에서 받은 최종 위치로 돌들을 동기화. (누락된 돌 자동 생성 기능 포함)
    public void SyncPositions(List<StonePosition> serverPositions)
    {
        //Debug.Log($"서버의 최종 위치로 모든 돌을 동기화합니다. 서버 돌 개수: {serverPositions.Count}");
        lastStonePosition = serverPositions;

        foreach (var stonePos in serverPositions)
        {
            StoneForceController_Firebase fc = null;
            bool stoneExists = false;

            // 1. 로컬에서 돌 찾기
            if (stonePos.Team == "A")
            {
                stoneExists = _stoneControllers_A.TryGetValue(stonePos.StoneId, out fc);
            }
            else
            {
                stoneExists = _stoneControllers_B.TryGetValue(stonePos.StoneId, out fc);
            }

            // 2. 돌이 없으면 새로 생성
            if (!stoneExists)
            {
                Debug.LogWarning($"로컬에 없는 돌(Team: {stonePos.Team}, ID: {stonePos.StoneId})을 서버 데이터로 새로 생성합니다.");
                fc = SpawnMissingStone(stonePos);
                if (fc == null)
                {
                    Debug.LogError($"누락된 돌(ID: {stonePos.StoneId}) 생성에 실패했습니다. 동기화를 건너뜁니다.");
                    continue; // 다음 돌로 넘어감
                }
            }

            // 3. 돌 위치 및 상태 동기화
            Vector3 newPosition = new Vector3(stonePos.Position["x"], stonePos.Position["y"], stonePos.Position["z"]);

            if (CheckDonutPassedOutLine(newPosition.z))
            {
                //Debug.Log($"ID: {fc.donutId} 돌이 아웃 라인을 넘었으므로 제거합니다.");
                DonutOut(fc);
                continue;
            }

            //Rigidbody rb = fc.GetComponent<Rigidbody>();
            if (!fc.TryGetComponent<Rigidbody>(out Rigidbody rb)) // rigidbody 못찾으면
            {
                Debug.Log("Rigidbody를 찾을 수 없음");
                continue;
            }
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            
            // isTrigger를 true로 설정하여 순간이동 중 다른 오브젝트와 충돌 방지
            //var capCollider = fc.transform.GetComponent<CapsuleCollider>();
            if (!fc.transform.TryGetComponent<CapsuleCollider>(out CapsuleCollider capCollider))
            {
                Debug.Log("캡슐콜라이더를 찾을 수 없습니다");
                continue;
            }
            //if(capCollider != null) capCollider.isTrigger = true;
            capCollider.isTrigger = true;

            fc.transform.DOMove(newPosition, 0.2f).OnComplete(() => {
                // 이동 완료 후 물리 다시 활성화
                rb.isKinematic = false;
                capCollider.isTrigger = false;
            });
        }
    }

    // 동기화 과정에서 로컬에 존재하지 않는 돌을 생성하는 도우미 메서드
    private StoneForceController_Firebase SpawnMissingStone(StonePosition stoneInfo)
    {
        if (string.IsNullOrEmpty(stoneInfo.DonutId))
        {
            Debug.LogError($"SpawnMissingStone: DonutId가 없어 누락된 돌(ID: {stoneInfo.StoneId})을 생성할 수 없습니다.");
            return null;
        }

        DonutData donutDataToSpawn = DataManager.Instance.GetDonutByID(stoneInfo.DonutId);
        if (donutDataToSpawn == null || donutDataToSpawn.prefab == null)
        {
            Debug.LogError($"SpawnMissingStone: DonutId '{stoneInfo.DonutId}'에 해당하는 프리팹을 찾을 수 없습니다.");
            return null;
        }

        // 씬에 실제로 돌을 생성
        GameObject newStone = Instantiate(donutDataToSpawn.prefab, spawnPosition.position, spawnPosition.rotation);
        StoneForceController_Firebase sfc = newStone.GetComponent<StoneForceController_Firebase>();

        if (sfc == null)
        {
            Debug.LogError("SpawnMissingStone: 생성된 돌 프리팹에 StoneForceController_Firebase 컴포넌트가 없습니다.");
            Destroy(newStone);
            return null;
        }

        // 서버에서 받은 정보로 돌 초기화
        StoneForceController_Firebase.Team team = (stoneInfo.Team == "A") ? StoneForceController_Firebase.Team.A : StoneForceController_Firebase.Team.B;
        sfc.InitializeDonut(team, stoneInfo.StoneId, stoneInfo.DonutId, stoneInfo.Weight, stoneInfo.Resilience, stoneInfo.Friction);

        // 올바른 딕셔너리에 추가
        if (team == StoneForceController_Firebase.Team.A)
        {
            _stoneControllers_A[stoneInfo.StoneId] = sfc;
        }
        else
        {
            _stoneControllers_B[stoneInfo.StoneId] = sfc;
        }

        return sfc;
    }

    // 시뮬레이션 완료 시 모든 돌의 최종 위치를 수집하는 함수
    public List<StonePosition> GetAllStonePositions()
    {
        //if (_currentTurnStoneTeam == myTeam) return null;
        var positions = new List<StonePosition>();
        var toDestroyDonuts = new List<StoneForceController_Firebase>();
        //int maxLength = Math.Max(_stoneControllers_A.Count, _stoneControllers_B.Count);
        int maxLength = Math.Max(aShotIndex + 1, bShotIndex + 1);
        StoneForceController_Firebase sfc;
        for (int i = 0; i < maxLength; i++)
        {
            //if (i < _stoneControllers_A.Count && _stoneControllers_A.ContainsKey(i))
            if (_stoneControllers_A.ContainsKey(i))
            {
                sfc = _stoneControllers_A[i];
                if (CheckDonutPassedOutLine(sfc.transform.position.z)) // 아웃라인 확인해보고
                {
                    toDestroyDonuts.Add(sfc); // 아웃라인을 넘겼으면 삭제를 위한 리스트에 담아둔다
                }
                positions.Add(new StonePosition
                {
                    StoneId = sfc.donutId,
                    Team = sfc.team.ToString(),
                    DonutId = sfc.DonutId,
                    Weight = sfc.DonutWeight,
                    Resilience = sfc.DonutResilience,
                    Friction = sfc.DonutFriction,
                    Position = new Dictionary<string, float>
                    {
                        { "x", sfc.transform.position.x },
                        { "y", sfc.transform.position.y },
                        { "z", sfc.transform.position.z }
                    }
                });
                //Debug.Log($"donutId = {sfc.donutId}, Team = {sfc.team}, Position = ({sfc.transform.position.x}, {sfc.transform.position.y}, {sfc.transform.position.z})");
            }

            //if (i < _stoneControllers_B.Count && _stoneControllers_B.ContainsKey(i))
            if (_stoneControllers_B.ContainsKey(i))
            {
                sfc = _stoneControllers_B[i];
                if (CheckDonutPassedOutLine(sfc.transform.position.z)) // 아웃라인 확인해보고
                {
                    toDestroyDonuts.Add(sfc); // 아웃라인을 넘겼으면 삭제를 위한 리스트에 담아둔다
                }
                positions.Add(new StonePosition
                {
                    StoneId = sfc.donutId,
                    Team = sfc.team.ToString(),
                    DonutId = sfc.DonutId,
                    Weight = sfc.DonutWeight,
                    Resilience = sfc.DonutResilience,
                    Friction = sfc.DonutFriction,
                    Position = new Dictionary<string, float>
                    {
                        { "x", sfc.transform.position.x },
                        { "y", sfc.transform.position.y },
                        { "z", sfc.transform.position.z }
                    }
                });
                //Debug.Log($"donutId = {sfc.donutId}, Team = {sfc.team}, Position = ({sfc.transform.position.x}, {sfc.transform.position.y}, {sfc.transform.position.z})");

            }
        }

        for (int i = 0; i < toDestroyDonuts.Count; i++)
        {
            // 아웃처리할 도넛 리스트를 순회하며 해당 도넛을 아웃처리한다
            DonutOut(toDestroyDonuts[i]);
        }
        lastStonePosition = positions; // 지금 만들어둔 포지션정보를 로컬에도 저장 
        return positions;
    }

    public StoneForceController_Firebase GetCurrentTurnStone() // FirebaseGameManager가 현재 턴의 돌 정보를 가져갈 수 있도록 쓰이는 함수
    {
        return _currentTurnStone;
    }

    public StoneForceController_Firebase GetDonutToLaunch(int donutId) // 상대 입장에서 시뮬레이션 돌릴때 호출되는 함수
    {
        StoneForceController_Firebase donutToLaunch;
        if (myTeam == StoneForceController_Firebase.Team.A) // 내가 A팀이면
        {
            if (!_stoneControllers_B.TryGetValue(donutId, out var forceController)) //상대는 B팀이니까 B팀 리스트에서 찾음
            {
                Debug.LogError($"발사할 돌(ID: {donutId})을 찾을 수 없습니다!");
                return null;
            }

            donutToLaunch = forceController;
        }
        else if (myTeam == StoneForceController_Firebase.Team.B) //내가 B팀이면
        {
            if (!_stoneControllers_A.TryGetValue(donutId, out var forceController)) //상대틑 A팀이니까 A팀 리스트에서 찾음
            {
                Debug.LogError($"발사할 돌(ID: {donutId})을 찾을 수 없습니다!");
                return null;
            }

            donutToLaunch = forceController;
        }
        else
        {
            Debug.Log(" 팀 설정 값이 없음");
            return null;
        }
        return donutToLaunch;
    }

    public void HighlightNearestDonut()
    {
        Vector3 housePosition = _scr_Collider_House.transform.position; // 하우스(점수중심점) 의 포지션

        //하우스 콜라이더로부터 닿아있는 도넛들을 가져옴
        List<StoneForceController_Firebase> inHouseDonutList = _scr_Collider_House.GetInHouseDonutList();
        if (inHouseDonutList.Count == 0) // 만약 아무도 하우스에 도넛을 못올렸으면 무승부
        {
            return;
        }
    }


    public void CalculateScore(out StoneForceController_Firebase.Team team, out int score, out List<int> idList, bool needNearest = false)
    {

        Vector3 housePosition = _scr_Collider_House.transform.position; // 하우스(점수중심점) 의 포지션

        List<int> scoredIds = new List<int>();
        idList = null;
        //하우스 콜라이더로부터 닿아있는 도넛들을 가져옴
        List<StoneForceController_Firebase> inHouseDonutList = _scr_Collider_House.GetInHouseDonutList();

        if (inHouseDonutList.Count == 0) // 만약 아무도 하우스에 도넛을 못올렸으면 무승부
        {
            team = StoneForceController_Firebase.Team.None;
            score = 0;
            return;
        }

        List<StoneForceController_Firebase> sortedDonutList = inHouseDonutList
            .OrderBy(donut => Vector3.Distance(housePosition, donut.transform.position)) // 거리 오름차순 정렬
            .ToList();
        //LinQ를 이용해서 하우스에 올라간 도넛들의 거리를 측정하여 그 거리별로 오름차순으로 정렬

        if (needNearest)
        {
            StartCoroutine(HighlightScoredDonuts(sortedDonutList, 0, true));
            team = StoneForceController_Firebase.Team.None;
            score = -99;
            return;
        }
        team = sortedDonutList[0].team; // out으로 보낼 승리팀
        scoredIds.Add(sortedDonutList[0].donutId);
        //sortedDonutList[0].transform.GetComponent<Outlinable>().enabled = true;
        score = 1;
        

        int indexToHighlight = 0;
        
        for (int i = 1; i < sortedDonutList.Count; i++)
        {
            if (sortedDonutList[i].team == team) // 승리팀의 연속된 득점 계산.
            {
                indexToHighlight = i;
                //sortedDonutList[i].transform.GetComponent<Outlinable>().enabled = true;
                score++; // out으로 보낼 점수
                scoredIds.Add(sortedDonutList[i].donutId);
            }
            else
            {
                idList = null;
                break;
            }
        }

        idList = scoredIds;
        StartCoroutine(HighlightScoredDonuts(sortedDonutList, indexToHighlight));
        
        
    }

    public void VisualizeScoreDonuts(string team, List<int> idList)
    {
        List<StoneForceController_Firebase> sfc =  new List<StoneForceController_Firebase>();
        if (team == StoneForceController_Firebase.Team.A.ToString())
        {
            foreach (int id in idList)
            {
                for (int i = 0; i < _stoneControllers_A.Count; i++)
                {
                    if (_stoneControllers_A[i].donutId == id)
                    {
                        sfc.Add(_stoneControllers_A[i]);
                    }
                }
            }            
        }
        else if (team == StoneForceController_Firebase.Team.B.ToString())
        {
            foreach (int id in idList)
            {
                for (int i = 0; i < _stoneControllers_B.Count; i++)
                {
                    if (_stoneControllers_B[i].donutId == id)
                    {
                        sfc.Add(_stoneControllers_B[i]);
                    }
                }
            }       
        }
        StartCoroutine(HighlightScoredDonuts(sfc, sfc.Count - 1));
    }

    IEnumerator HighlightScoredDonuts(List<StoneForceController_Firebase> sortedDonutList, int index, bool needToDisable = false)
    {
        if (needToDisable) // 만약에 하나만 보여주고 아웃라인을 해제해야하는경우
        {
            if (sortedDonutList[0].transform.TryGetComponent<Outlinable>(out var outline))
            {
                outline.enabled = true; // 아웃라인 활성화
                DOVirtual.DelayedCall( 1.8f, () =>
                {
                    outline.enabled = false; // 아웃라인 비활성화
                }).SetLink(gameObject); //객체 오브젝트에 생명주기를 맞춰서 코루틴이 이미 종료되더라도 이 DelayedCall이 실행되도록 보장
            }
            yield break;
        }
        
        WaitForSeconds wait = new WaitForSeconds(0.5f);

        for (int i = 0; i <= index; i++)
        {
            yield return wait;
            if (sortedDonutList[i].transform.TryGetComponent<Outlinable>(out var outline))
            {
                outline.enabled = true;
            }
            //sortedDonutList[i].transform.GetComponent<Outlinable>().enabled = true;
        }
      
    }

    public void ClearOldDonutsInNewRound(Game newGame, int round = -99) // 새 라운드가 시작하기 전에, 기존에 생성되었던 모든 도넛들을 지움
    {
        foreach (KeyValuePair<int, StoneForceController_Firebase> force in _stoneControllers_A)
        {
            Destroy(force.Value.gameObject);
        }

        foreach (KeyValuePair<int, StoneForceController_Firebase> force in _stoneControllers_B)
        {
            Destroy(force.Value.gameObject);
        }
        _stoneControllers_A.Clear();
        _stoneControllers_B.Clear();
        lastStonePosition.Clear();

        aShotIndex = -1;
        bShotIndex = -1;
        roundCount = round == -99 ? newGame.RoundNumber : round;
        //roundCount = newGame.RoundNumber;
        aScore = FirebaseGameManager.Instance.aTeamScore;
        bScore = FirebaseGameManager.Instance.bTeamScore;
        _scr_Collider_House.ClearDonutList();
        _uilaunchIndicator.RoundChanged(roundCount, aScore, bScore);
    }

    /// <summary>
    /// 새 게임 시작 시 StoneManager의 모든 상태를 초기화합니다.
    /// </summary>
    public void ResetForNewGame()
    {
        Debug.Log("새 게임 시작을 위해 StoneManager 상태를 초기화합니다.");
        foreach (var controller in _stoneControllers_A.Values)
        {
            if (controller != null) Destroy(controller.gameObject);
        }
        foreach (var controller in _stoneControllers_B.Values)
        {
            if (controller != null) Destroy(controller.gameObject);
        }

        _stoneControllers_A.Clear();
        _stoneControllers_B.Clear();
        lastStonePosition.Clear();

        aShotIndex = -1;
        bShotIndex = -1;
        roundCount = 1;
        aScore = 0;
        bScore = 0;
        myTeam = StoneForceController_Firebase.Team.None;
        _currentTurnStone = null;
    }

    #region 변수 교체 함수

    public void A_ShotIndexUp()
    {
        aShotIndex++;
    }

    public void B_ShotIndexUp()
    {
        bShotIndex++;
    }

    public void A_ShotIndexDown()
    {
        aShotIndex--;
    }

    public void B_ShotIndexDown()
    {
        bShotIndex--;
    }

    public void RoundCountUp()
    {
        roundCount++;
    }

    #endregion


    #region 아웃 판정 관련 함수

    public void DonutOut(StoneForceController_Firebase donut) //도넛의 아웃 판정
    {
        if (donut.team == StoneForceController_Firebase.Team.A)
        {
            if (_stoneControllers_A.ContainsKey(donut.donutId))
            {
                _stoneControllers_A.Remove(donut.donutId);
            }
        }
        else
        {
            if (_stoneControllers_B.ContainsKey(donut.donutId))
            {
                _stoneControllers_B.Remove(donut.donutId);
            }
        }


        Destroy(donut.gameObject);
        
    }

    public bool CheckDonutPassedOutLine(float positionZ) // 도넛이 엔드 호그라인보다 더 앞에 있거나, 백라인을 넘었을 경우 아웃처리를 위함
    {
        if ((positionZ > backLine.position.z + donutColliderRadius) || (positionZ < endHogLine.position.z - donutColliderRadius))
        {
            // 백라인과 엔드호그라인의 z좌표를 가져오고, 그 좌표에 도넛 콜라이더의 반지름을 더해서
            // 도넛의 중심좌표와 위 계산의 결과값과 비교하여 아웃판정을 진행
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}
