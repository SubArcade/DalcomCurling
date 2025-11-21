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
        Aiming, // 조준 중 (힘, 방향, 스핀 입력 가능)
        MovingToHogLine, // 돌이 호그 라인으로 자동으로 이동하는 중
        Launched // 돌이 발사되어 움직이는 중
    }

    /// <summary>
    /// 현재 드래그 중인 입력의 종류를 나타내는 열거형
    /// </summary>
    public enum DragType
    {
        None,
        PowerDirection,
        Rotation
    }

    /// <summary>
    /// 조준 과정의 현재 단계를 나타내는 열거형
    /// </summary>
    private enum AimingPhase
    {
        PowerDirection, // 힘과 방향을 설정하는 단계
        Rotation        // 회전을 설정하는 단계
    }
    
    [System.Serializable]
     public struct WeightedItem
     {
         public int value;      // 결과로 얻을 정수 값
         public float weight;     // 가중치 (0~100 사이의 퍼센트)
     }

    [Header("스크립트 및 UI 참조")] // Unity Inspector에서 UI 분류를 위한 헤더
    public UI_LaunchIndicator_Firebase uiLaunch; // UI 인디케이터 스크립트 참조
    public StoneManager stoneManager; // 돌 관리를 담당하는 스크립트 참조

    [Header("조작 영역")]
    public RectTransform inputArea; // 힘/방향 및 회전 입력을 위한 통합 UI 영역

    [Header("궤적 미리보기")] // 궤적 미리보기 관련 변수 헤더
    public LineRenderer trajectoryLine; // 궤적을 그릴 라인 렌더러
    public int trajectoryPoints = 100; // 궤적을 계산할 포인트 수 (정밀도)
    public float trajectoryTimeStep = 0.1f; // 각 포인트 사이의 시간 간격 (시뮬레이션 스텝)
    [SerializeField, Range(0.1f, 2.0f)] private float trajectoryForceMultiplier = 0.5f; // 궤적 예측의 힘 계수 (1.0이 기본)
    [SerializeField, Range(0.001f, 0.1f)] private float trajectoryCurlFactor = 0.05f; // 궤적 예측의 휨 계수
    [SerializeField, Range(0.1f, 2.0f)] private float horizontalDragSensitivity = 0.5f; // 좌우 드래그 방향 민감도 (낮을수록 덜 꺾임)
    public Color min_Color = Color.white;
    public Color max_Color = Color.red;

    [Header("설정 변수")] // 게임 플레이 조작 관련 변수 헤더
    public float launchForceMultiplier = 9f; // 드래그 거리를 발사 힘으로 변환하는 계수, 4에서 8로 수정(11/16), 4에서 6로 수정 11/17, 9로수정 (11/20)
    public float maxDragDistance = 0.25f; // 초기 드래그의 최대 거리 (정규화된 화면 높이 기준) , 0.5에서 0.25로 수정(11/16)
    public float maxRotationDragDistance = 0.25f; // 회전 입력 드래그의 최대 거리 (정규화된 화면 폭 기준) , 1에서 0.5로 수정(11/16), 0.25수정 (11/20)
    public float maxRotationValue = 5f; // 스핀의 최대 값
    public float autoMoveToHogLineSpeed = 4f; // 도넛이 호그 라인까지 자동 이동하는 속도, 6에서 4로 수정 11/17, 5로수정 (11/20)
    public float maxUIDirectionAngle = 60f; // UI 화살표가 표시할 수 있는 최대 각도
    public float minLaunchDragDistance = 50f; // 발사로 인정할 최소 드래그 거리 (픽셀)

    [Header("퍼펙트존 확률 조정 가능 변수")] // 확률 구역별 가중치 변수 헤더
    
    private List<WeightedItem> perfectZoneRandomWeights = new List<WeightedItem>
    {
        new WeightedItem { value = -5, weight = 2f },
        new WeightedItem { value = -4, weight = 4f },
        new WeightedItem { value = -3, weight = 6f },
        new WeightedItem { value = -2, weight = 11f },
        new WeightedItem { value = -1, weight = 17f },
        new WeightedItem { value = 0, weight = 20f },
        new WeightedItem { value = 1, weight = 17f },
        new WeightedItem { value = 2, weight = 11f },
        new WeightedItem { value = 3, weight = 6f },
        new WeightedItem { value = 4, weight = 4f },
        new WeightedItem { value = 5, weight = 2f }
    };

    
    private List<WeightedItem> earlyZoneRandomWeights = new List<WeightedItem>
    {
        new WeightedItem { value = -10, weight = 0.5f },
        new WeightedItem { value = -9, weight = 1f },
        new WeightedItem { value = -8, weight = 1.5f },
        new WeightedItem { value = -7, weight = 2f },
        new WeightedItem { value = -6, weight = 3f },
        new WeightedItem { value = -5, weight = 4f },
        new WeightedItem { value = -4, weight = 6f },
        new WeightedItem { value = -3, weight = 7f },
        new WeightedItem { value = -2, weight = 9f },
        new WeightedItem { value = -1, weight = 10f },
        new WeightedItem { value = 0, weight = 12f },
        new WeightedItem { value = 1, weight = 10f },
        new WeightedItem { value = 2, weight = 9f },
        new WeightedItem { value = 3, weight = 7f },
        new WeightedItem { value = 4, weight = 6f },
        new WeightedItem { value = 5, weight = 4f },
        new WeightedItem { value = 6, weight = 3f },
        new WeightedItem { value = 7, weight = 2f },
        new WeightedItem { value = 8, weight = 1.5f },
        new WeightedItem { value = 9, weight = 1f },
        new WeightedItem { value = 10, weight = 0.5f }
    };

    [Header("씬 참조 오브젝트")] // 씬 내 특정 오브젝트 참조 헤더
    public Transform startHogLine; // 돌 발사 시작 위치 (호그 라인)
    public Transform perfectZoneLine; // 퍼펙트 존의 Z-위치
    public Transform earlyZoneLine; // 얼리 존의 Z-위치

    // --- 상태 및 출력 변수 ---
    public LaunchState CurrentState { get; private set; } = LaunchState.Aiming; // 현재 발사 상태
    public float FinalRotationValue { get; private set; } = 0f; // 최종 스핀 값
    public float CurrentDragRatio { get; private set; } // 힘 조작을 위한 드래그 비율 (0~1)
    public float CurrentLaunchAngle { get; private set; } // 방향 조작을 위한 발사 각도
    public bool IsDragging { get; private set; } = false; // 현재 드래그 중인지 여부
    public DragType CurrentDragType { get; private set; } = DragType.None; // 현재 드래그 타입

    // --- 내부 변수 ---
    private AimingPhase _currentAimingPhase; // 현재 조준 단계 (힘/방향 또는 회전)
    private bool _inputEnabled = false; // 현재 입력을 받을 수 있는지 여부
    private Rigidbody _currentStoneRb; // 현재 조작 중인 돌의 Rigidbody 참조
    [SerializeField] private Camera _mainCamera; // 메인 카메라 참조 (시리얼라이즈 필드로 Inspector에서 설정 가능)
    private Vector3 _actualDragStartScreenPos; // 초기 드래그 시작 화면 좌표
    private Vector3 _rotationDragStartScreenPos; // 회전 드래그 시작 화면 좌표
    private Vector3 _finalLaunchDirection = Vector3.forward; // 최종 발사 방향
    private Vector3 _finalLaunchDirectionForTrajectory = Vector3.forward; // 궤적 계산을 위한 최종 발사 방향
    private float _finalLaunchForce; // 최종 발사 힘
    private float _finalLaunchForceForTrajectory; // 궤적 계산을 위한 최종 발사 힘
    private float _releaseRandomValue = -99f; // 릴리즈 타이밍에 따른 랜덤 값
    private bool _needToTap = false; // 호그 라인까지 이동 중 탭이 필요한지 여부
    private bool _isTrajectoryPreviewActive = false; // 궤적 미리보기 활성화 여부
    private float draggedDistanceForTrajectory = 0; // 발사를 위해 드래그했던 정도를 기록할 변수, 궤적을 위해 저장
    private float draggedAmountBetween_0_Or_1 = 0; // 회전값을 주었을때, 회전힘에 따라 전진방향 힘을 살짝 약하게 주기 위한 값

    // --- 미리 준비한 샷 데이터 저장용 ---
    private LastShot _preparedShotData = null; // 'PreparingShot' 상태에서 미리 입력된 샷 데이터

    /// <summary>
    /// Unity 컴포넌트가 처음 로드될 때 호출됩니다.
    /// 이벤트 리스너를 추가하고 초기 상태를 설정합니다.
    /// </summary>
    void Awake()
    {
        // releaseButton.onClick.AddListener(ReleaseButtonClicked); // 버튼 리스너는 이제 사용하지 않습니다.
        _mainCamera = Camera.main; // 메인 카메라 자동 할당
        if (_mainCamera == null)
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다. 카메라에 'MainCamera' 태그가 설정되어 있는지 확인해주세요.");
        }
        // if (releaseButton != null) releaseButton.onClick.AddListener(ReleaseShot);
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
        CurrentState = LaunchState.Aiming; // 조준 상태로 변경
        _currentAimingPhase = AimingPhase.PowerDirection; // 조준 단계를 힘/방향 설정으로 초기화
        _releaseRandomValue = -99f; // 릴리즈 랜덤 값 초기화

        // 조준 값들 초기화
        FinalRotationValue = 0f;
        _finalLaunchForce = 0f;
        _finalLaunchForceForTrajectory = 0f;
        _finalLaunchDirection = Vector3.forward;
        _finalLaunchDirectionForTrajectory = Vector3.forward;
        CurrentDragRatio = 0f;
        CurrentLaunchAngle = 0f;

        _isTrajectoryPreviewActive = true; // 궤적 미리보기 활성화
        if (trajectoryLine != null) trajectoryLine.enabled = true;

        //Debug.Log("InputController: 입력 활성화됨 (Aiming state). 힘/방향을 설정하세요.");
    }

    /// <summary>
    /// 돌 조작 입력을 비활성화합니다.
    /// </summary>
    public void DisableInput()
    {
        DOTween.Kill("GuideTimer1");
        DOTween.Kill("GuideTimer2");
        DOTween.Kill("GuideTimer3");

        _inputEnabled = false; // 입력 비활성화 플래그

        _isTrajectoryPreviewActive = false; // 궤적 미리보기 비활성화
        if (trajectoryLine != null) trajectoryLine.enabled = false; // 궤적 라인 렌더러 비활성화
        //Debug.Log("InputController: 입력 비활성화됨.");
    }

    /// <summary>
    /// 매 프레임마다 호출됩니다.
    /// 게임 상태에 따라 적절한 입력 처리 및 UI 업데이트를 수행합니다.
    /// </summary>
    void Update()
    {
        if (!_inputEnabled) return; // 입력이 비활성화 상태면 아무것도 하지 않음

        // 상태에 따라 입력 처리 분기
        if (CurrentState == LaunchState.Aiming)
        {
            HandleAimingInput();
        }
        else if (CurrentState == LaunchState.MovingToHogLine && _needToTap)
        {
            HandleTapInput();
        }

        // 궤적 미리보기 업데이트
        if (CurrentState == LaunchState.Aiming && _isTrajectoryPreviewActive)
        {
            UpdateTrajectoryPreview();
        }
        else
        {
            if (trajectoryLine != null && trajectoryLine.enabled)
            {
                trajectoryLine.enabled = false;
            }
        }
    }

    #region 입력 처리 (Input Handling)
    /// <summary>
    /// 조준 상태(Aiming)에서 모든 입력을 처리합니다.
    /// 터치/마우스 위치에 따라 힘, 방향, 회전, 발사 입력을 구분하여 받습니다.
    /// </summary>
    private void HandleAimingInput()
    {
        // 카메라가 파괴되었는지 확인하고 다시 찾아봅니다.
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogWarning("조준 입력 처리 중 메인 카메라를 찾을 수 없습니다.");
                return;
            }
        }

        Vector2 touchPosition = Vector2.zero;
        bool isTouchBegan = false;
        bool isTouchMoved = false;
        bool isTouchEnded = false;

        // 터치 및 마우스 입력 통합 처리
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;
            isTouchBegan = touch.phase == TouchPhase.Began;
            isTouchMoved = touch.phase == TouchPhase.Moved;
            isTouchEnded = touch.phase == TouchPhase.Ended;
        }
        else
        {
            touchPosition = Input.mousePosition;
            isTouchBegan = Input.GetMouseButtonDown(0);
            isTouchMoved = Input.GetMouseButton(0);
            isTouchEnded = Input.GetMouseButtonUp(0);
        }

        // 현재 조준 단계에 따라 입력 처리 분기
        if (_currentAimingPhase == AimingPhase.PowerDirection)
        {
            // --- 힘/방향 설정 단계 ---
            if (isTouchBegan)
            {
                // 터치 시작 위치가 조작 영역 안인지 확인
                if (inputArea != null && RectTransformUtility.RectangleContainsScreenPoint(inputArea, touchPosition, null))
                {
                    StartDrag(touchPosition, DragType.PowerDirection);
                }
            }
            else if (isTouchMoved && IsDragging && CurrentDragType == DragType.PowerDirection)
            {
                // 힘/방향 실시간 계산
                // 실제 발사를 위한 계산과 궤적을 위한 계산이 분리되어있는 상태 ( 사용해야할 변수가 다름 )
                Vector3 dragVector2D = (Vector3)touchPosition - _actualDragStartScreenPos;
                Vector3 dragVector2DForTrajectory = new Vector3(dragVector2D.x * horizontalDragSensitivity, dragVector2D.y);

                float dragDistance = dragVector2D.magnitude / _mainCamera.pixelHeight;
                float dragDistanceForTrajectory = dragVector2DForTrajectory.magnitude / _mainCamera.pixelWidth;
                
                float clampedDistance = Mathf.Min(dragDistance, maxDragDistance);
                float clampedDistanceForTrajectory = Mathf.Min(dragDistanceForTrajectory, maxDragDistance);
                
                draggedDistanceForTrajectory = clampedDistance;
                
                _finalLaunchForce = clampedDistance * launchForceMultiplier;
                _finalLaunchForceForTrajectory = clampedDistanceForTrajectory * launchForceMultiplier;

                Vector2 launchDirection2D = -dragVector2D.normalized;
                _finalLaunchDirection = (launchDirection2D.y < 0)
                    ? new Vector3(launchDirection2D.x, 0f, 0f)
                    : new Vector3(launchDirection2D.x, 0f, launchDirection2D.y);
                _finalLaunchDirection.Normalize();
                
                Vector2 launchDirection2DForTrajectory = -dragVector2DForTrajectory.normalized;
                _finalLaunchDirectionForTrajectory = (launchDirection2DForTrajectory.y < 0)
                    ? new Vector3(launchDirection2DForTrajectory.x, 0f, 0f)
                    : new Vector3(launchDirection2DForTrajectory.x, 0f, launchDirection2DForTrajectory.y);
                _finalLaunchDirectionForTrajectory.Normalize();

                UpdateDragVisual(touchPosition);
            }
            else if (isTouchEnded && IsDragging && CurrentDragType == DragType.PowerDirection)
            {
                EndDrag(); // 드래그 종료

                // 드래그 거리가 충분한지 확인
                Vector3 dragVector = (Vector3)touchPosition - _actualDragStartScreenPos;
                if (dragVector.magnitude > minLaunchDragDistance)
                {
                    //Debug.Log("힘/방향 설정 완료. 이제 회전을 설정하세요.");
                    FirebaseGameManager.Instance.OnShotStepUI(); // 도넛 엔트리창만 off

                    _currentAimingPhase = AimingPhase.Rotation; // 다음 단계로 전환

                    // 2초 동안 추가 입력이 없으면 회전 가이드 표시
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        if (_inputEnabled && _currentAimingPhase == AimingPhase.Rotation && !IsDragging)
                        {
                            uiLaunch?.SHowGuideUI(2);
                        }
                    }).SetId("GuideTimer2");
                }
                else
                {
                    //Debug.Log($"드래그 거리가 짧아 힘/방향이 설정되지 않았습니다. 다시 시도하세요. (최소 드래그 거리: {minLaunchDragDistance}px)");
                    // 사용자가 다시 시도할 수 있도록 조준 값 초기화
                    _finalLaunchForce = 0f;
                    _finalLaunchForceForTrajectory = 0f;
                    _finalLaunchDirection = Vector3.forward;
                    _finalLaunchDirectionForTrajectory = Vector3.forward;
                    CurrentDragRatio = 0f;
                    CurrentLaunchAngle = 0f;
                }
            }
        }
        else // _currentAimingPhase == AimingPhase.Rotation
        {
            // --- 회전 설정 단계 ---
            if (isTouchBegan)
            {
                // 터치 시작 위치가 조작 영역 안인지 확인
                if (inputArea != null && RectTransformUtility.RectangleContainsScreenPoint(inputArea, touchPosition, null))
                {
                    StartDrag(touchPosition, DragType.Rotation);
                }
            }
            else if (isTouchMoved && IsDragging && CurrentDragType == DragType.Rotation)
            {
                // 회전값 실시간 계산
                UpdateRotationValue(touchPosition);
            }
            else if (isTouchEnded && IsDragging && CurrentDragType == DragType.Rotation)
            {
                EndDrag(); // 드래그 종료
                //Debug.Log("회전 설정 완료. 발사합니다!");
                FirebaseGameManager.Instance.OnIdleUI();
                ReleaseShot(); // 발사!
            }
        }
    }

    /// <summary>
    /// 호그 라인 이동 중 탭 입력을 처리합니다.
    /// </summary>
    private void HandleTapInput()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            TapBeforeHogLine();
        }
    }
    #endregion

    #region 드래그 로직 (Drag Logic)
    /// <summary>
    /// 드래그 시작 시 호출됩니다.
    /// </summary>
    /// <param name="screenPosition">드래그 시작 화면 좌표.</param>
    /// <param name="dragType">드래그 종류 (힘/방향 또는 회전).</param>
    private void StartDrag(Vector3 screenPosition, DragType dragType)
    {
        DOTween.Kill("GuideTimer1");
        DOTween.Kill("GuideTimer2");
        DOTween.Kill("GuideTimer3");

        IsDragging = true;
        CurrentDragType = dragType;

        if (dragType == DragType.PowerDirection)
        {
            _actualDragStartScreenPos = screenPosition;
            if (_currentStoneRb != null) _currentStoneRb.isKinematic = true;
        }
        else if (dragType == DragType.Rotation)
        {
            _rotationDragStartScreenPos = screenPosition;
        }
    }

    /// <summary>
    /// 드래그 종료 시 호출됩니다.
    /// </summary>
    private void EndDrag()
    {
        IsDragging = false;
        CurrentDragType = DragType.None;
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
        float dragRatio = Mathf.Clamp(normalizedDrag / (maxRotationDragDistance * 2), -1f, 1f); // 드래그 비율 클램프 (-1~1)
        draggedAmountBetween_0_Or_1 = Mathf.Abs(dragRatio); // 드래그한 비율의 절대값을 가져옴(나중에 발사 힘 계산에 포함하기 위함)
        FinalRotationValue = dragRatio * maxRotationValue; // 최종 스핀 값 계산
    }

    /// <summary>
    /// 현재 입력값을 기반으로 단순화된 물리 모델을 사용하여 예상 궤적을 계산하고 LineRenderer를 업데이트합니다.
    /// </summary>
    private void UpdateTrajectoryPreview()
    {
        if (trajectoryLine == null || _currentStoneRb == null) return; // LineRenderer 또는 돌 Rigidbody 없으면 리턴
                
        // 현재 설정된 힘, 방향, 스핀 값을 사용
        float currentForce = _finalLaunchForceForTrajectory;
        Vector3 currentDirection = _finalLaunchDirectionForTrajectory;
        float currentSpin = FinalRotationValue;

        Vector3 finalDirectionForSim = currentDirection;
        float finalForceForSim = currentForce;
        Vector3 launchVelocity = finalDirectionForSim.normalized * finalForceForSim * trajectoryForceMultiplier;

        List<Vector3> points = new List<Vector3>();
        points.Add(_currentStoneRb.transform.position);

        Vector3 currentPos = _currentStoneRb.transform.position;
        Vector3 currentVel = launchVelocity;
        float friction = 0.98f;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            // 컬 힘 계산 시 항상 현재 스핀 값을 사용
            Vector3 curlForce = Vector3.right * currentSpin * trajectoryCurlFactor * currentVel.magnitude * trajectoryTimeStep;
            currentVel += curlForce;
            currentVel *= Mathf.Pow(friction, trajectoryTimeStep); // 마찰력 적용
            currentPos += currentVel * trajectoryTimeStep; // 위치 업데이트
            points.Add(currentPos);

            if (currentVel.magnitude < 0.1f)
            {
                break;
            }
        }

        float t = Mathf.InverseLerp(0, maxDragDistance, draggedDistanceForTrajectory); // 드래그한 거리를 비율화
        Color resultColor = Color.Lerp(min_Color, max_Color, t); // 그 비율에 따라 시작 색상과 종료 색상 믹싱
        
        // 시작 컬러와 끝 컬러를 일단 통일하여 표시
        trajectoryLine.startColor = resultColor;
        //trajectoryLine.endColor = resultColor;

        trajectoryLine.positionCount = points.Count;
        trajectoryLine.SetPositions(points.ToArray());
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
            // 좌우 회전값에 따라 발사 힘을 아주 살짝 약하게 줌으로써 최종 전진거리 감소 효과
            Force = finalForce * calculatedRandomValue * (1 - (draggedAmountBetween_0_Or_1 * 0.1f)), // 최종 힘
            
            PlayerId = stoneManager.myUserId,
            Team = stoneManager.myTeam, // 발사하는 팀
            Spin = FinalRotationValue * calculatedRandomValue, // 최종 스핀 값
            Direction = directionDict, // 발사 방향
            ReleasePosition = releasePosDict // 릴리즈 위치
        };
    }

    /// <summary>
    /// 샷을 최종 확정하고 발사 절차를 시작합니다.
    /// </summary>
    private void ReleaseShot() 
    {
        DOTween.Kill("GuideTimer1");
        DOTween.Kill("GuideTimer2");
        DOTween.Kill("GuideTimer3");

        //Debug.Log($"RElesase clicked, myTurn = {FirebaseGameManager.Instance._isMyTurn}");
        //미리 보기 궤적 비활성화
        _isTrajectoryPreviewActive = false;
        if (trajectoryLine != null) trajectoryLine.enabled = false;

        FirebaseGameManager.Instance.ChangeCameraRelease(); // 스톤에 카메라 부착
        LastShot shotData = CalculateShotData();
        //Debug.Log(FirebaseGameManager.Instance.CurrentLocalState);
        FirebaseGameManager.Instance.Change_SuccessfullyShotInTime_To_True();

        if (FirebaseGameManager.Instance.CurrentLocalState == "WaitingForInput" && FirebaseGameManager.Instance._isMyTurn == false)
        {
            _preparedShotData = shotData;
            //Debug.Log("샷 준비 완료. 턴 시작을 기다립니다.");
            DisableInput();
            //카운트다운 제거
            //FirebaseGameManager.Instance.ControlCountdown(false);
            FirebaseGameManager.Instance.CountDownStop();
        }
        else if (FirebaseGameManager.Instance.CurrentLocalState == "WaitingForInput" && FirebaseGameManager.Instance._isMyTurn == true)
        {
            //Debug.Log("여기까지는 옴");
            CurrentState = LaunchState.MovingToHogLine;
            MoveDonutToHogLine(shotData);
            _needToTap = true;

            //카운트다운 제거
            //FirebaseGameManager.Instance.ControlCountdown(false);
            FirebaseGameManager.Instance.CountDownStop();

        }
    }

    /// <summary>
    /// 돌을 호그 라인까지 자동으로 이동시킵니다.
    /// </summary>
    /// <param name="shotData">발사할 샷 데이터.</param>
    private void MoveDonutToHogLine(LastShot shotData)
    {
        uiLaunch?.SHowGuideUI(3);

        if (_currentStoneRb == null)
        {
            Debug.LogError("오류: _currentStoneRb가 null입니다!"); // 오류 로그 추가
            return;
        }
        _currentStoneRb.isKinematic = true; // 물리 엔진 영향 비활성화

        // DOTween을 사용하여 호그 라인까지 돌을 이동시킵니다.
        _currentStoneRb.DOMove(startHogLine.position, autoMoveToHogLineSpeed).SetSpeedBased(true).SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(() => // 이동 완료 시 호출되는 콜백 함수
            {
                if (_needToTap)
                {
                    // 탭을 못했으면 FirebaseGameManager의 실패 처리 메서드를 호출합니다. 아웃처리 후 다음 상태로 넘어가도록 HandleTapFailed()호출.
                    Debug.Log("호그라인 전까지 탭하지 않았기에 턴을 넘깁니다.");
                    FirebaseGameManager.Instance.HandleTapFailed(_currentStoneRb, shotData.DonutId);
                    DisableInput();
                    return;
                }

                if (_currentStoneRb != null) _currentStoneRb.DOKill(); // DOTween 애니메이션 중지

                OnShotConfirmed?.Invoke(shotData); // 샷 데이터 확정 이벤트 발생
                //Debug.Log("샷 정보 전송 완료.");

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
            //Debug.LogWarning("미리 준비된 샷이 없어 일반 입력 모드로 전환됩니다.");
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
        List<WeightedItem> weights = null; // 가중치 리스트 초기화

        // 돌의 위치에 따라 퍼펙트존 또는 얼리존 판정
        if (zPos >= perfectZoneLine.position.z && zPos <= startHogLine.position.z)
        {
            weights = perfectZoneRandomWeights; // 퍼펙트존 가중치 사용
            //Debug.Log("퍼펙트존");
        }
        else if (zPos >= earlyZoneLine.position.z && zPos < perfectZoneLine.position.z)
        {
            weights = earlyZoneRandomWeights; // 얼리존 가중치 사용
            //Debug.Log("얼리존");
        }

        if (weights != null) // 가중치 리스트가 유효하면
        {
            _needToTap = false; // 탭 이벤트 비활성화
            int randomPoint = Random.Range(1, 101); // 1~100 사이의 랜덤 값
            //Debug.Log($"randomPoint : {randomPoint}");
            int cumulativeWeight = 0; // 누적 가중치
            foreach (var item in weights) // 가중치 리스트 순회
            {
                cumulativeWeight += (int)(item.weight * 10); // 가중치 적용
                //Debug.Log($"itemweight : {item.weight}");
                if (randomPoint * 10 <= cumulativeWeight) // 랜덤 값이 누적 가중치 범위 안에 들면
                {
                    _releaseRandomValue = item.value; // 해당 랜덤 값 적용
                    break; // 루프 종료
                }
            }
            //Debug.Log($"releaseRandomValue: {_releaseRandomValue}");
        }
    }
    #endregion
    
    
}
