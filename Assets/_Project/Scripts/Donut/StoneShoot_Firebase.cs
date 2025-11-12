using System; // 기본적인 시스템 기능을 사용하기 위해 필요합니다.
using System.Collections.Generic; // 리스트, 딕셔너리 같은 컬렉션 사용을 위해 필요합니다.
using DG.Tweening; // DOTween 애니메이션 라이브러리를 사용하기 위해 필요합니다.
using UnityEngine; // Unity 엔진의 기능을 사용하기 위해 필요합니다.
using UnityEngine.UI; // UI 요소를 사용하기 위해 필요합니다.
using Random = UnityEngine.Random; // UnityEngine.Random을 명시적으로 사용하여 System.Random과 충돌 방지.

/// <summary>
/// 로컬 플레이어의 샷 입력 처리와 샷 데이터 계산을 담당합니다.
/// 사용자 입력을 받아 돌의 발사 방향, 힘, 스핀을 결정하고,
/// 이를 바탕으로 샷 데이터를 생성하여 FirebaseGameManager에 전달합니다.
/// </summary>
public class StoneShoot_Firebase : MonoBehaviour
{
    /// <summary>
    /// 입력이 완료되면 계산된 샷 데이터를 담아 이 이벤트를 발생시킵니다.
    /// FirebaseGameManager가 이 이벤트를 구독하여 샷 데이터를 Firestore에 전송합니다.
    /// </summary>
    public event Action<LastShot> OnShotConfirmed;

    /// <summary>
    /// 돌 발사 과정의 현재 상태를 나타내는 열거형입니다.
    /// </summary>
    public enum LaunchState
    {
        WaitingForInitialDrag, // 초기 드래그 시작을 기다리는 상태 (방향 및 힘 결정)
        WaitingForRotationInput, // 스핀 입력 조작을 기다리는 상태
        WaitingForPressRelease, // 릴리즈 버튼 클릭 대기 (발사 최종 확정)
        MovingToHogLine, // 돌이 호그 라인으로 자동으로 이동하는 중
        Launched // 돌이 발사되어 움직이는 중
    }

    [Header("스크립트 및 UI 참조")] // Unity Inspector에서 UI 분류를 위한 헤더
    public UI_LaunchIndicator_Firebase uiLaunch; // UI 인디케이터 스크립트 참조
    public StoneManager stoneManager; // 돌 관리를 담당하는 스크립트 참조
    public GameObject powerAndDirectionText; // 힘과 방향을 표시하는 텍스트 UI
    public GameObject rotationText; // 회전 값을 표시하는 텍스트 UI
    public Button releaseButton; // 발사 확정 버튼 참조

    [Header("궤적 미리보기")] // 궤적 미리보기 관련 변수 헤더
    public LineRenderer trajectoryLine; // 궤적을 그릴 라인 렌더러
    public int trajectoryPoints = 100; // 궤적을 계산할 포인트 수 (정밀도)
    public float trajectoryTimeStep = 0.1f; // 각 포인트 사이의 시간 간격 (시뮬레이션 스텝)
    [SerializeField, Range(0.1f, 2.0f)] private float trajectoryForceMultiplier = 0.5f; // 궤적 예측의 힘 계수 (1.0이 기본)
    [SerializeField, Range(0.001f, 0.1f)] private float trajectoryCurlFactor = 0.05f; // 궤적 예측의 휨 계수
    [SerializeField, Range(0.1f, 2.0f)] private float horizontalDragSensitivity = 0.5f; // 좌우 드래그 방향 민감도 (낮을수록 덜 꺾임)

    [Header("설정 변수")] // 게임 플레이 조작 관련 변수 헤더
    public float launchForceMultiplier = 4f; // 드래그 거리를 발사 힘으로 변환하는 계수
    public float maxDragDistance = 0.5f; // 초기 드래그의 최대 거리 (정규화된 화면 높이 기준)
    public float maxRotationDragDistance = 1f; // 회전 입력 드래그의 최대 거리 (정규화된 화면 폭 기준)
    public float maxRotationValue = 5f; // 스핀의 최대 값
    public float autoMoveToHogLineSpeed = 6f; // 돌이 호그 라인까지 자동 이동하는 속도
    public float maxUIDirectionAngle = 60f; // UI 화살표가 표시할 수 있는 최대 각도

    [Header("퍼펙트존 확률 조정 가능 변수")] // 확률 구역별 가중치 변수 헤더
    // StoneShoot 스크립트의 WeightedItem 구조체를 재사용합니다.
    private List<StoneShoot.WeightedItem> perfectZoneRandomWeights = new List<StoneShoot.WeightedItem>
    {
        new StoneShoot.WeightedItem { value = -5, weight = 2f },
        new StoneShoot.WeightedItem { value = -4, weight = 4f },
        new StoneShoot.WeightedItem { value = -3, weight = 6f },
        new StoneShoot.WeightedItem { value = -2, weight = 11f },
        new StoneShoot.WeightedItem { value = -1, weight = 17f },
        new StoneShoot.WeightedItem { value = 0, weight = 20f },
        new StoneShoot.WeightedItem { value = 1, weight = 17f },
        new StoneShoot.WeightedItem { value = 2, weight = 11f },
        new StoneShoot.WeightedItem { value = 3, weight = 6f },
        new StoneShoot.WeightedItem { value = 4, weight = 4f },
        new StoneShoot.WeightedItem { value = 5, weight = 2f }
    };

    // StoneShoot 스크립트의 WeightedItem 구조체를 재사용합니다.
    private List<StoneShoot.WeightedItem> earlyZoneRandomWeights = new List<StoneShoot.WeightedItem>
    {
        new StoneShoot.WeightedItem { value = -10, weight = 0.5f },
        new StoneShoot.WeightedItem { value = -9, weight = 1f },
        new StoneShoot.WeightedItem { value = -8, weight = 1.5f },
        new StoneShoot.WeightedItem { value = -7, weight = 2f },
        new StoneShoot.WeightedItem { value = -6, weight = 3f },
        new StoneShoot.WeightedItem { value = -5, weight = 4f },
        new StoneShoot.WeightedItem { value = -4, weight = 6f },
        new StoneShoot.WeightedItem { value = -3, weight = 7f },
        new StoneShoot.WeightedItem { value = -2, weight = 9f },
        new StoneShoot.WeightedItem { value = -1, weight = 10f },
        new StoneShoot.WeightedItem { value = 0, weight = 12f },
        new StoneShoot.WeightedItem { value = 1, weight = 10f },
        new StoneShoot.WeightedItem { value = 2, weight = 9f },
        new StoneShoot.WeightedItem { value = 3, weight = 7f },
        new StoneShoot.WeightedItem { value = 4, weight = 6f },
        new StoneShoot.WeightedItem { value = 5, weight = 4f },
        new StoneShoot.WeightedItem { value = 6, weight = 3f },
        new StoneShoot.WeightedItem { value = 7, weight = 2f },
        new StoneShoot.WeightedItem { value = 8, weight = 1.5f },
        new StoneShoot.WeightedItem { value = 9, weight = 1f },
        new StoneShoot.WeightedItem { value = 10, weight = 0.5f }
    };

    [Header("씬 참조 오브젝트")] // 씬 내 특정 오브젝트 참조 헤더
    public Transform startHogLine; // 돌 발사 시작 위치 (호그 라인)
    public Transform perfectZoneLine; // 퍼펙트 존의 Z-위치
    public Transform earlyZoneLine; // 얼리 존의 Z-위치

    // --- 상태 및 출력 변수 ---
    public LaunchState CurrentState { get; private set; } = LaunchState.WaitingForInitialDrag; // 현재 발사 상태
    public float FinalRotationValue { get; private set; } = 0f; // 최종 스핀 값
    public float CurrentDragRatio { get; private set; } // 힘 조작을 위한 드래그 비율 (0~1)
    public float CurrentLaunchAngle { get; private set; } // 방향 조작을 위한 발사 각도
    public bool IsDragging { get; private set; } = false; // 현재 드래그 중인지 여부

    // --- 내부 변수 ---
    private bool _inputEnabled = false; // 현재 입력을 받을 수 있는지 여부
    private Rigidbody _currentStoneRb; // 현재 조작 중인 돌의 Rigidbody 참조
    [SerializeField] private Camera _mainCamera; // 메인 카메라 참조 (시리얼라이즈 필드로 Inspector에서 설정 가능)
    private Vector3 _actualDragStartScreenPos; // 초기 드래그 시작 화면 좌표
    private Vector3 _rotationDragStartScreenPos; // 회전 드래그 시작 화면 좌표
    private Vector3 _finalLaunchDirection; // 최종 발사 방향
    private float _finalLaunchForce; // 최종 발사 힘
    private float _releaseRandomValue = -99f; // 릴리즈 타이밍에 따른 랜덤 값
    private bool _needToTap = false; // 호그 라인까지 이동 중 탭이 필요한지 여부
    private bool _isTrajectoryPreviewActive = false; // 궤적 미리보기 활성화 여부

    // --- 미리 준비한 샷 데이터 저장용 ---
    private LastShot _preparedShotData = null; // 'PreparingShot' 상태에서 미리 입력된 샷 데이터

    /// <summary>
    /// Unity 컴포넌트가 처음 로드될 때 호출됩니다.
    /// 이벤트 리스너를 추가하고 초기 상태를 설정합니다.
    /// </summary>
    void Awake()
    {
        // _mainCamera = Camera.main; // 메인 카메라 자동 할당 (필요 시 주석 해제)
        releaseButton.onClick.AddListener(ReleaseButtonClicked);
        if (trajectoryLine != null) trajectoryLine.enabled = false; // 궤적 라인 렌더러 초기에는 비활성화
        DisableInput(); // 컴포넌트 시작 시 입력 비활성화
    }

    /// <summary>
    /// 돌 조작 입력을 활성화합니다.
    /// </summary>
    /// <param name="stoneRb">조작할 돌의 Rigidbody.</param>
    public void EnableInput(Rigidbody stoneRb)
    {
        _inputEnabled = true; // 입력 활성화 플래그
        _currentStoneRb = stoneRb; // 현재 조작할 돌 설정
        CurrentState = LaunchState.WaitingForInitialDrag; // 초기 드래그 대기 상태로 변경
        powerAndDirectionText.SetActive(true); // 힘/방향 UI 활성화
        _releaseRandomValue = -99f; // 릴리즈 랜덤 값 초기화

        FinalRotationValue = 0f; //스핀값 초기화

        Debug.Log("InputController: 입력 활성화됨.");
    }

    /// <summary>
    /// 돌 조작 입력을 비활성화합니다.
    /// </summary>
    public void DisableInput()
    {
        _inputEnabled = false; // 입력 비활성화 플래그
        powerAndDirectionText.SetActive(false); // 힘/방향 UI 비활성화
        rotationText.SetActive(false); // 회전 UI 비활성화
        releaseButton.interactable = false; // 발사 버튼 비활성화
        if (uiLaunch != null) uiLaunch.ActivateTapStartPointImage(false); // 탭 시작점 이미지 비활성화

        if (trajectoryLine != null) trajectoryLine.enabled = false; // 궤적 라인 렌더러 비활성화
        _isTrajectoryPreviewActive = false; // 상태 변수도 비활성화 
        Debug.Log("InputController: 입력 비활성화됨.");
    }

    /// <summary>
    /// 매 프레임마다 호출됩니다.
    /// 게임 상태에 따라 적절한 입력 처리 및 UI 업데이트를 수행합니다.
    /// </summary>
    void Update()
    {
        if (!_inputEnabled) return; // 입력이 비활성화 상태면 아무것도 하지 않음

        // 힘, 방향, 스핀 입력을 받습니다.
        if (CurrentState == LaunchState.WaitingForInitialDrag)
        {
            HandleLaunchInput();
        }
        else if (CurrentState == LaunchState.WaitingForRotationInput)
        {
            HandleRotationInput();
        }

        if (CurrentState == LaunchState.MovingToHogLine && _needToTap)
        {
            HandleTapInput();
        }

        // 시작적 업데이트 (UI 조작 화살표 용)
        //if (IsDragging && CurrentState == LaunchState.WaitingForInitialDrag)
        //{
        //    Vector3 currentPos = Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
        //    UpdateDragVisual(currentPos);
        //}

        if (IsDragging)
        {
            Vector3 currentScreenPos = Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;

            if (CurrentState == LaunchState.WaitingForInitialDrag)
            {
                // 힘/방향 실시간 계산 및 멤버 변수 업데이트
                Vector3 dragVector2D = currentScreenPos - _actualDragStartScreenPos;
                dragVector2D.x *= horizontalDragSensitivity;

                float dragDistance = dragVector2D.magnitude / _mainCamera.pixelHeight;
                float clampedDistance = Mathf.Min(dragDistance, maxDragDistance);
                _finalLaunchForce = clampedDistance * launchForceMultiplier; // 멤버 변수 업데이트

                Vector2 launchDirection2D = -dragVector2D.normalized;
                _finalLaunchDirection = (launchDirection2D.y < 0)
                    ? new Vector3(launchDirection2D.x, 0f, 0f)
                    : new Vector3(launchDirection2D.x, 0f, launchDirection2D.y);
                _finalLaunchDirection.Normalize(); // 멤버 변수 업데이트

                // UI 화살표 업데이트
                UpdateDragVisual(currentScreenPos);
            }
            else if (CurrentState == LaunchState.WaitingForRotationInput)
            {
                // 회전값 실시간 계산 및 업데이트
                UpdateRotationValue(currentScreenPos);
            }
        }

        // --- 궤적 미리보기 로직 ---
               
        // TODO : PreparingShot 에서 WaitingForInput로 넘어갈때 정보 초기화됨
        // PreparingShot에서 입력한 정보를 릴리즈 버튼을 눌러야만 넘기는 상태라 조정해야함
        
        if (_isTrajectoryPreviewActive) // 궤적 미리보기가 활성화 상태일 때만 업데이트
        {
            if (trajectoryLine != null && !trajectoryLine.enabled)
            {
                trajectoryLine.enabled = true; // 혹시라도 비활성화되어 있다면 다시 활성화
            }
            UpdateTrajectoryPreview();
        }
        else // 궤적  미리보기가 비활성화 상태일 때
        {
            if (trajectoryLine != null && trajectoryLine.enabled)
            {
                trajectoryLine.enabled = false; // 라인 렌더러 비활성화
            }
        }

    }

    #region 입력 처리 (Input Handling)
    /// <summary>
    /// 초기 드래그(방향 및 힘) 입력을 처리합니다.
    /// 마우스/터치 다운 시 드래그 시작, 업 시 드래그 종료.
    /// </summary>
    private void HandleLaunchInput()
    {
        if (Input.GetMouseButtonDown(0)) StartDrag_Launch(Input.mousePosition);
        //else if (Input.GetMouseButtonUp(0) && IsDragging) EndDrag_Launch(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0) && IsDragging) EndDrag_Launch();

        // 터치 입력 지원
        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) StartDrag_Launch(touch.position);
            //else if (touch.phase == TouchPhase.Ended && IsDragging) EndDrag_Launch(touch.position);
            else if (touch.phase == TouchPhase.Ended && IsDragging) EndDrag_Launch();
        }
    }

    /// <summary>
    /// 회전(스핀) 입력을 처리합니다.
    /// 마우스/터치 다운 시 드래그 시작, 드래그 중 값 업데이트, 업 시 드래그 종료.
    /// </summary>
    private void HandleRotationInput()
    {
        if (Input.GetMouseButtonDown(0)) StartDrag_Rotation(Input.mousePosition);
        else if (Input.GetMouseButton(0) && IsDragging) UpdateRotationValue(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0) && IsDragging) EndDrag_Rotation();
        // 터치 입력 지원
        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) StartDrag_Rotation(touch.position);
            else if (touch.phase == TouchPhase.Moved && IsDragging) UpdateRotationValue(touch.position);
            else if (touch.phase == TouchPhase.Ended && IsDragging) EndDrag_Rotation();
        }
    }

    /// <summary>
    /// 호그 라인 이동 중 탭 입력을 처리합니다.
    /// </summary>
    private void HandleTapInput()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) TapBeforeHogLine();
    }
    #endregion

    #region 드래그 로직 (Drag Logic)
    /// <summary>
    /// 초기 드래그 시작 시 호출됩니다.
    /// </summary>
    /// <param name="screenPosition">드래그 시작 화면 좌표.</param>
    private void StartDrag_Launch(Vector3 screenPosition)
    {
        IsDragging = true; // 드래그 중 플래그 활성화
        _actualDragStartScreenPos = screenPosition; // 드래그 시작 위치 기록
        if (_currentStoneRb != null) _currentStoneRb.isKinematic = true; // 돌의 물리 움직임 일시 정지
        uiLaunch.ActivateTapStartPointImage(true, _actualDragStartScreenPos); // 탭 시작점 UI 활성화
        powerAndDirectionText.SetActive(true); // 힘/방향 UI 활성화

        _isTrajectoryPreviewActive = true;
        if (trajectoryLine != null) trajectoryLine.enabled = true; // 라인 렌더러 활성화
    }

    /// <summary>
    /// 초기 드래그 종료 시 호출됩니다.
    /// 발사 방향과 힘을 계산하고 상태를 전환합니다.
    /// </summary>
    /// <param name="screenPosition">드래그 종료 화면 좌표.</param>
    private void EndDrag_Launch() //파라미터 Vector3 screenPosition <- 삭제 했음
    {
        if (!IsDragging) return; // 드래그 중이 아니면 아무것도 하지 않음
        
        // 궤적 계산에 영향을 끼쳐 일단 제거
        /* 
        Vector3 dragVector2D = screenPosition - _actualDragStartScreenPos; // 드래그 벡터 계산
        float dragDistance = dragVector2D.magnitude / _mainCamera.pixelHeight; // 화면 높이에 비례한 드래그 거리
        float clampedDistance = Mathf.Min(dragDistance, maxDragDistance); // 최대 드래그 거리 클램프
        Vector2 launchDirection2D = -dragVector2D.normalized; // 발사 방향 (역방향)

        // UI 표시 방향을 실제 월드 방향으로 변환 (z-축을 기준으로)
        _finalLaunchDirection = (launchDirection2D.y < 0)
            ? new Vector3(launchDirection2D.x, 0f, 0f) // 역방향 드래그는 x축 움직임에만 영향
            : new Vector3(launchDirection2D.x, 0f, launchDirection2D.y);
        _finalLaunchDirection.Normalize(); // 방향 벡터 정규화
        _finalLaunchForce = clampedDistance * launchForceMultiplier; // 최종 발사 힘 계산
        */
        
        IsDragging = false; // 드래그 종료
        CurrentState = LaunchState.WaitingForRotationInput; // 회전 입력 대기 상태로 전환

        // PreparingShot 상태에서는 화살표 UI를 사용하지 않으므로, 시각적 업데이트 로직은 건너뜁니다.
        if (FirebaseGameManager.Instance.CurrentLocalState != "PreparingShot")
        {
            uiLaunch.ActivateTapStartPointImage(false); // 탭 시작점 이미지 비활성화
        }

        powerAndDirectionText.SetActive(false); // 힘/방향 UI 비활성화
        rotationText.SetActive(true); // 회전 UI 활성화 (스핀 조작 시작)
    }

    /// <summary>
    /// 회전 드래그 시작 시 호출됩니다.
    /// </summary>
    /// <param name="screenPosition">드래그 시작 화면 좌표.</param>
    private void StartDrag_Rotation(Vector3 screenPosition)
    {
        IsDragging = true; // 드래그 중 플래그 활성화
        _rotationDragStartScreenPos = screenPosition; // 회전 드래그 시작 위치 기록
    }

    /// <summary>
    /// 회전 드래그 종료 시 호출됩니다.
    /// </summary>
    private void EndDrag_Rotation()
    {
        if (!IsDragging) return; // 드래그 중이 아니면 아무것도 하지 않음
        IsDragging = false; // 드래그 종료
        rotationText.SetActive(false); // 회전 UI 비활성화
        releaseButton.interactable = true; // 발사 버튼 활성화
        CurrentState = LaunchState.WaitingForPressRelease; // 릴리즈 대기 상태로 전환
    }
    #endregion

    #region 값 계산 및 시각화 (Value Calculation & Visualization)
    /// <summary>
    /// 드래그 기반의 UI 화살표 및 힘/방향 값을 업데이트합니다.
    /// </summary>
    /// <param name="currentScreenPos">현재 드래그 중인 화면 좌표.</param>
    private void UpdateDragVisual(Vector3 currentScreenPos) // 기존 화살표 UI 시각화
    {
        Vector3 dragVector2D = currentScreenPos - _actualDragStartScreenPos; // 드래그 벡터 계산
        if (dragVector2D.magnitude > 0.01f) // 드래그 길이가 충분할 때만
        {
            Vector2 launchDirection2D = -dragVector2D.normalized; // 발사 방향 정규화
            float angle = Vector2.SignedAngle(Vector2.up, launchDirection2D); // Y축 기준으로 각도 계산
            angle *= maxUIDirectionAngle / 90f; // UI 최대 각도에 맞춰 스케일
            if (launchDirection2D.y < 0) angle = Mathf.Clamp(angle, -maxUIDirectionAngle, maxUIDirectionAngle); // 발사 역방향 제한
            CurrentLaunchAngle = angle; // 현재 발사 각도 업데이트
        }
        float dragDistance = dragVector2D.magnitude / _mainCamera.pixelHeight; // 화면 높이 대비 드래그 거리
        CurrentDragRatio = Mathf.Clamp01(dragDistance / maxDragDistance); // 드래그 비율 클램프 (0~1)
    }

    /// <summary>
    /// 드래그 기반으로 회전(스핀) 값을 업데이트합니다.
    /// </summary>
    /// <param name="currentScreenPos">현재 드래그 중인 화면 좌표.</param>
    private void UpdateRotationValue(Vector3 currentScreenPos)
    {
        Vector3 dragVector = currentScreenPos - _rotationDragStartScreenPos; // 회전 드래그 벡터 계산
        float dragXDistance = dragVector.x; // X축 드래그 거리
        float normalizedDrag = dragXDistance / _mainCamera.pixelWidth; // 화면 폭 대비 정규화된 드래그 거리
        float dragRatio = Mathf.Clamp(normalizedDrag / maxRotationDragDistance, -1f, 1f); // 드래그 비율 클램프 (-1~1)
        FinalRotationValue = dragRatio * maxRotationValue; // 최종 스핀 값 계산
    }

    /// <summary>
    /// 현재 입력값을 기반으로 단순화된 물리 모델을 사용하여 예상 궤적을 계산하고 LineRenderer를 업데이트합니다.
    /// /// (실시간 드래그 반영을 위해 수정된 버전)
    /// </summary>
    private void UpdateTrajectoryPreview()
    {
        if (trajectoryLine == null || _currentStoneRb == null) return; // LineRenderer 또는 돌 Rigidbody 없으면 리턴
                
        float currentForce = _finalLaunchForce;
        Vector3 currentDirection = _finalLaunchDirection;
        float currentSpin = FinalRotationValue;

        Vector3 finalDirectionForSim = currentDirection;  // 발사 방향
        float finalForceForSim = currentForce;  // 발사 속도
        Vector3 launchVelocity = finalDirectionForSim.normalized * finalForceForSim * trajectoryForceMultiplier;

        List<Vector3> points = new List<Vector3>(); // 궤적을 그릴 포인트 저장 리스트
        points.Add(_currentStoneRb.transform.position); // A지점 (돌이 놓여있는 위치 )
        points.Add(startHogLine.position);  // B 지점 (호그 라인)    

        //B지점 (호그라인) 부터 시뮬레이션
        Vector3 currentPos = startHogLine.position; // 시작 위치
        Vector3 currentVel = launchVelocity; // 돌의 초기 속도
        float friction = 0.98f; // 각 스텝마다 속도가 줄어드는 비율 (마찰력) 물리 예측을 위한 단순화된 파라미터 (실제 물리와 맞추려면 튜닝 필요)

        for (int i = 0; i < trajectoryPoints; i++) // 지정된 포인트 수만큼 시뮬레이션
        {
            float spinForPreview = 0f;
            if (CurrentState == LaunchState.WaitingForRotationInput) //첫번째 드래그에서는 스핀값 안들어가게
            {
                spinForPreview = currentSpin;
            }

            // 결정된 스핀 값으로 컬 힘을 계산합니다. (힘/방향 단계에서는 spinForPreview가 0이므로 컬이 없음)
            Vector3 curlForce = Vector3.right * spinForPreview * trajectoryCurlFactor * currentVel.magnitude * trajectoryTimeStep;
            currentVel += curlForce;
            currentVel *= Mathf.Pow(friction, trajectoryTimeStep); // 마찰력 적용 (속도를 점차 줄임)
            currentPos += currentVel * trajectoryTimeStep; // 위치 업데이트
            points.Add(currentPos); // 계산된 위치 추가

            // 돌이 거의 멈추면 계산 중단
            if (currentVel.magnitude < 0.1f)
            {
                break;
            }
        }

        // LineRenderer 업데이트
        trajectoryLine.positionCount = points.Count; // 포인트 수 설정
        trajectoryLine.SetPositions(points.ToArray()); // 계산된 포인트들로 선 그리기
    }
    #endregion

    #region 발사 및 탭 (Launch & Tap)
    /// <summary>
    /// 현재 입력된 값들을 바탕으로 최종 샷 데이터를 계산하여 반환합니다.
    /// </summary>
    /// <returns>계산된 LastShot 데이터.</returns>
    private LastShot CalculateShotData()
    {
        float calculatedRandomValue = (_releaseRandomValue == -99f) ? 1f : (100 + _releaseRandomValue) * 0.01f;

        // 최종 발사 방향과 힘은 초기 드래그 값과 자동 이동 속도를 기반으로 계산됩니다.
        Vector3 finalDirection = _finalLaunchDirection * _finalLaunchForce + Vector3.forward * autoMoveToHogLineSpeed;
        float finalForce = _finalLaunchForce + autoMoveToHogLineSpeed;

        // 샷 방향 데이터를 Dictionary 형태로 저장 (Firestore 호환성)
        var directionDict = new Dictionary<string, float>
        {
            { "x", finalDirection.x * calculatedRandomValue },
            { "y", finalDirection.y * calculatedRandomValue },
            { "z", finalDirection.z * calculatedRandomValue }
        };

        // 돌의 릴리즈 위치 데이터를 Dictionary 형태로 저장 (Firestore 호환성)
        var releasePosDict = new Dictionary<string, float>
        {
            { "x", _currentStoneRb.transform.position.x },
            { "y", _currentStoneRb.transform.position.y },
            { "z", _currentStoneRb.transform.position.z }
        };

        // LastShot 객체를 생성하여 반환
        return new LastShot
        {
            Force = finalForce * calculatedRandomValue, // 최종 힘
            PlayerId = stoneManager.myUserId,
            Team = stoneManager.myTeam, // 발사하는 팀
            Spin = FinalRotationValue * calculatedRandomValue, // 최종 스핀 값
            Direction = directionDict, // 발사 방향
            ReleasePosition = releasePosDict // 릴리즈 위치
        };
    }

    /// <summary>
    /// 발사(릴리즈) 버튼 클릭 시 호출됩니다.
    /// 현재 게임 상태에 따라 미리 준비한 샷을 저장하거나 돌을 발사합니다.
    /// </summary>
    private void ReleaseButtonClicked()
    {
        //미리 보기 궤적 비활성화
        _isTrajectoryPreviewActive = false;
        if (trajectoryLine != null) trajectoryLine.enabled = false;

        FirebaseGameManager.Instance.ChangeCameraRelease(); // 스톤에 카메라 부착
        LastShot shotData = CalculateShotData();

        if (FirebaseGameManager.Instance.CurrentLocalState == "PreparingShot")
        {
            _preparedShotData = shotData;
            Debug.Log("샷 준비 완료. 턴 시작을 기다립니다.");
            DisableInput();
        }
        else
        {
            CurrentState = LaunchState.MovingToHogLine;
            MoveDonutToHogLine(shotData);
            _needToTap = true;

            //카운트다운 제거
            FirebaseGameManager.Instance.ControlCountdown(false);

        }
    }

    /// <summary>
    /// 돌을 호그 라인까지 자동으로 이동시킵니다.
    /// </summary>
    /// <param name="shotData">발사할 샷 데이터.</param>
    private void MoveDonutToHogLine(LastShot shotData)
    {
        if (_currentStoneRb == null)
        {
            Debug.LogError("오류: _currentStoneRb가 null입니다!"); // 오류 로그 추가
            return;
        }
        _currentStoneRb.isKinematic = true; // 물리 엔진 영향 비활성화

        // DOTween을 사용하여 호그 라인까지 돌을 이동시킵니다.
        _currentStoneRb.DOMove(startHogLine.position, autoMoveToHogLineSpeed).SetSpeedBased(true).SetEase(Ease.Linear)
            .OnComplete(() => // 이동 완료 시 호출되는 콜백 함수
            {
                if (_needToTap)
                {
                    // 탭을 못했으면 _releaseRandomValue가 -99이므로, CalculateShotData에서 100%로 처리됨
                    // 이 경우 (탭을 놓쳤을 경우) 원래의 로직에 따라 랜덤값이 적용되지 않음.
                }

                if (_currentStoneRb != null) _currentStoneRb.DOKill(); // DOTween 애니메이션 중지

                OnShotConfirmed?.Invoke(shotData); // 샷 데이터 확정 이벤트 발생
                Debug.Log("샷 정보 전송 완료.");

                FirebaseGameManager.Instance.ChangeLocalStateToSimulatingMyShot(); // 로컬 상태를 시뮬레이션 중으로 변경               
                FirebaseGameManager.Instance.ChangeFixedDeltaTime(); // FixedDeltaTime 변경 (시뮬레이션 속도 조절)
                stoneManager.LaunchStone(shotData, stoneManager.myTeam == StoneForceController_Firebase.Team.A ? stoneManager.aShotIndex : stoneManager.bShotIndex); // 돌 발사

                DisableInput(); // 입력 비활성화
            });
    }

    /// <summary>
    /// 'PreparingShot' 상태에서 미리 준비된 샷을 즉시 실행합니다.
    /// </summary>
    /// <returns>샷이 실행되었으면 true, 없으면 false.</returns>
    public bool ExecutePreparedShot()
    {
        if (_preparedShotData != null) // 미리 준비된 샷 데이터가 있다면
        {
            Debug.Log("미리 입력된 샷으로 발사를 시작합니다.");
            CurrentState = LaunchState.MovingToHogLine; // 호그 라인 이동 상태로 변경
            MoveDonutToHogLine(_preparedShotData); // 미리 준비된 샷 데이터로 돌 이동
            _needToTap = false; // 미리 준비된 샷은 탭 입력 필요 없음
            _preparedShotData = null; // 사용한 샷 데이터 초기화
            return true; // 샷 실행됨
        }
        else
        {
            Debug.LogWarning("미리 준비된 샷이 없어 일반 입력 모드로 전환됩니다.");
            //EnableInput(this._currentStoneRb); // 준비된 샷 없으면 일반 입력 활성화
            return false; // 샷 실행 안됨
        }
    }

    /// <summary>
    /// 상대방의 돌을 시뮬레이션하기 위해 호그 라인까지 이동시킵니다.
    /// </summary>
    /// <param name="currentDonut">시뮬레이션할 돌의 Rigidbody.</param>
    /// <param name="shotData">상대방의 샷 데이터.</param>
    /// <param name="stoneId">돌의 고유 ID.</param>
    public void SimulateStone(Rigidbody currentDonut, LastShot shotData, int stoneId)
    {
        currentDonut.isKinematic = true; // 물리 엔진 영향 비활성화
        // DOTween을 사용하여 호그 라인까지 돌을 이동시킵니다.
        currentDonut.DOMove(startHogLine.position, autoMoveToHogLineSpeed).SetSpeedBased(true).SetEase(Ease.Linear)
            .OnComplete(() => // 이동 완료 시 호출되는 콜백 함수
            {
                stoneManager.LaunchStone(shotData, stoneId); // 돌 발사
            });
    }

    /// <summary>
    /// 돌이 호그 라인을 통과하기 전 탭 입력 처리 로직입니다.
    /// 탭 위치에 따라 랜덤 값을 부여합니다.
    /// </summary>
    private void TapBeforeHogLine()
    {
        if (!_needToTap || _currentStoneRb == null) return; // 탭이 필요 없거나 돌 없으면 리턴

        float zPos = _currentStoneRb.transform.position.z; // 돌의 현재 Z-위치
        List<StoneShoot.WeightedItem> weights = null; // 가중치 리스트 초기화

        // 돌의 위치에 따라 퍼펙트존 또는 얼리존 판정
        if (zPos >= perfectZoneLine.position.z && zPos <= startHogLine.position.z)
        {
            weights = perfectZoneRandomWeights; // 퍼펙트존 가중치 사용
            Debug.Log("퍼펙트존");
        }
        else if (zPos >= earlyZoneLine.position.z && zPos < perfectZoneLine.position.z)
        {
            weights = earlyZoneRandomWeights; // 얼리존 가중치 사용
            Debug.Log("얼리존");
        }

        if (weights != null) // 가중치 리스트가 유효하면
        {
            _needToTap = false; // 탭 이벤트 비활성화
            int randomPoint = Random.Range(1, 101); // 1~100 사이의 랜덤 값
            Debug.Log($"randomPoint : {randomPoint}");
            int cumulativeWeight = 0; // 누적 가중치
            foreach (var item in weights) // 가중치 리스트 순회
            {
                cumulativeWeight += (int)(item.weight * 10); // 가중치 적용
                Debug.Log($"itemweight : {item.weight}");
                if (randomPoint * 10 <= cumulativeWeight) // 랜덤 값이 누적 가중치 범위 안에 들면
                {
                    _releaseRandomValue = item.value; // 해당 랜덤 값 적용
                    break; // 루프 종료
                }
            }
            Debug.Log($"releaseRandomValue: {_releaseRandomValue}");
        }
    }
    #endregion
}
