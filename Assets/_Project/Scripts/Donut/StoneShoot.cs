using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class StoneShoot : MonoBehaviour
{
    public enum LaunchState
    {
        WaitingForInitialDrag,
        WaitingForRotationInput,
        WaitingForPressRelease,
        MovingToHogLine,
        Launched,
        AttackFinished
    }

    public enum SweepState
    {
        FrontSweep,
        LeftSweep,
        RightSweep,
        None
    }

    public enum Team
    {
        A,
        B
    }
    //public float powerAmount = 10f;

    //public float spinAmount = 0f;

    public Rigidbody rigid;
    [Header("스크립트 참조")] 
    public UI_LaunchIndicator uiLaunch;


    // --- 설정 변수 ---
    //[Header("게임 시스템 변수")] 
    //public float outJudgeStandard = 0.02f; //벽과 얼마만큼 가까워지면 아웃판정을 할건지 ( 높을수록 벽과 멀어도 아웃판정됨 ) , 초기값 0.02f
    public float launchForceMultiplier { get; private set; } = 4f; // 당긴 거리에 곱해질 힘의 계수, 초기값 4
    public float maxDragDistance { get; private set; } = 0.5f; // 최대 드래그 허용 거리 , 초기값 0.5
    public float maxRotationDragDistance { get; private set; } = 1f; // 회전 입력 최대 드래그 거리 (화면 높이 비율), 초기값 1
    public float maxRotationValue { get; private set; } = 5f; // 회전 입력 최대값 (5), 초기값 5
    public float autoMoveToHogLineSpeed { get; private set; } = 6f; // 호그라인까지 자동으로 이동할때 사용될 속도값, 초기값 5
    
    [System.Serializable]
    public struct WeightedItem
    {
        public int value;      // 결과로 얻을 정수 값
        public float weight;     // 가중치 (0~100 사이의 퍼센트)
    }

    [Header("퍼펙트존 확률 조정 가능 변수")]
    // 인스펙터에서 설정할 수 있는 가중치 리스트
    public List<WeightedItem> perfectZoneRandomWeights = new List<WeightedItem>
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
        // 총 합계는 반드시 100이어야 합니다.
    };
    
    public List<WeightedItem> earlyZoneRandomWeights = new List<WeightedItem>
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
        // 총 합계는 반드시 100이어야 합니다.
    };
    // --- UI 관련 설정 변수 ---
    [Header("UI 관련 설정 변수")] 
    public float maxUIDirectionAngle = 60f; // 드래그해서 힘조절 할 때, 좌우로 꺾이는 이미지의 최대 클램핑 각도, 기본값 60

    // --- 상태 및 출력 변수 ---
    // 현재 상태 (새로 추가)
    //public LaunchState currentState { get; private set; } = LaunchState.WaitingForInitialDrag;
    public LaunchState currentState  = LaunchState.WaitingForInitialDrag;

    

    // 최종 회전값 (외부에 노출할 float 값, 새로 추가)
    // 이 값을 사용자의 회전 로직에 입력하시면 됩니다.
    public float finalRotationValue { get; private set; } = 0f;

    // 임시 회전 입력 시작점 (새로 추가)
    private Vector3 rotationDragStartScreenPos;

    [Header("UI 오브젝트")] 
    public GameObject makeDonutText;
    public GameObject powerAndDirectionText;
    public GameObject rotationText;
    public GameObject sweepingPanel;
    public Button releaseButton;
    public Button frontSweepButton;
    public Button leftSweepButton;
    public Button rightSweepButton;
    public GameObject outText;

    [Header("게임 오브젝트")] 
    public GameObject stone;
    public Transform spawnPos; //생성될 위치

   // public Transform releasePoint;
    public Transform startHogLine; // 시작 호그라인, 자동으로 이동하는 구간의 끝점, 실제로 유저가 선택한 방향대로 힘을 주는 지점
    public Transform endHogLine; // 엔드 호그라인, 도넛이 최소한으로 여기까지는 이동해야 아웃처리가 안됨
    public Transform endHackLine; // 경기장 끝 아웃라인, 도넛이 이 지점을 넘어가버리면 아웃판정
    public Transform earlyZoneLine; // 얼리라인
    public Transform perfectZoneLine; // 퍼펙트존 라인
    public Transform leftWall;
    public Transform rightWall;
    
    //--- 게임 내 도넛들을 관리하는 리스트 ---
    public List<GameObject> inGameDonuts_A { get; private set; } = new List<GameObject>(); // A 팀의 도넛 전체를 관리할 리스트
    public List<GameObject> inGameDonuts_B { get; private set; } = new List<GameObject>(); // B 팀의 도넛 전체를 관리할 리스트
    public List<int> outDonutsId_A { get; private set; } = new List<int>();
    public List<int> outDonutsId_B { get; private set; } = new List<int>();
    
    
    // --- 스위핑 관련 ---
    private SweepState currentSweepState = SweepState.None;
    private SweepState previousSweepState = SweepState.None;
    public bool isSweeping { get; private set; } = false;
    //[Header("현재 스위프 미터 값")]
    public float currentSweepValue { get; private set; } = 0f;
    private float previousSweepValue = 0f;
    private const float MAX_VALUE = 1.0f;
    
    [Header("홀드시 증가할 값과 주기")]
    private const float LONG_HOLD_INCREASE_AMOUNT = 0.1f;
    private const float LONG_HOLD_INCREASE_INTERVAL = 0.1f;
    private Coroutine longHoldCoroutine;

    [Header("짧게 탭 할 경우의 설정")]
    private const float SHORT_PRESS_TARGET_INCREASE = 0.3f; // 목표 증가량 (0.3)
    private const float SHORT_PRESS_INTERVAL = 0.1f;
    // 짧게 누를 때의 로직도 0.1 단위로 0.1초마다 증가하도록 코루틴을 사용합니다.
    private const float SHORT_PRESS_STEP = 0.1f; // 0.1씩 증가
    private Coroutine shortPressCoroutine;
    
    [Header("안누르면 감소할 값과 주기")]
    private const float DECREASE_AMOUNT = 0.1f;
    private const float DECREASE_INTERVAL = 0.1f; // 0.1초마다
    private Coroutine autoDecreaseCoroutine; // 자동 감소 코루틴 참조

    //--- UI 연결 변수 ---
    public float currentDragRatio { get; private set; }
    public float currentLaunchAngle { get; private set; }

    // --- 내부 변수 ---
    private float releaseRandomValue = -99f;
    private Vector3 actualDragStartScreenPos;
    private Rigidbody rb;
    private StoneForceController forceController;
    public bool isDragging { get; private set; } = false;
    private Camera mainCamera;
    private SweepButtonScript frontSweep;
    private SweepButtonScript leftSweep;
    private SweepButtonScript rightSweep;
    
    private Vector3 finalLaunchDirectionWithForce;
    private float finalLaunchForce;

    private bool needToTap = false;
    private bool canSweep = false;

    private GameObject currentDonut;
    private int currentDonutId_A = -1;
    private int currentDonutId_B = -1;

    // private float leftWallOutValue;
    // private float rightWallOutValue;
    // private float endHogLineOutValue;
    // private float endHackLineOutValue;
    

    void Awake()
    {
        mainCamera = Camera.main;
        currentState = LaunchState.WaitingForInitialDrag;
        makeDonutText.SetActive(true);
        releaseButton.onClick.AddListener(ReleaseButtonClicked);
        
        frontSweep = frontSweepButton.GetComponent<SweepButtonScript>();
        leftSweep = leftSweepButton.GetComponent<SweepButtonScript>();
        rightSweep = rightSweepButton.GetComponent<SweepButtonScript>();

        frontSweep.OnPressStarted += FrontSweepingStart;
        frontSweep.OnShortTap += FrontSweepingTap;
        frontSweep.OnHoldThresholdReached += FrontSweepingHold;
        frontSweep.OnPressEnded += FrontSweepingEnd;

        leftSweep.OnPressStarted += LeftSweepingStart;
        leftSweep.OnShortTap += LeftSweepingTap;
        leftSweep.OnHoldThresholdReached += LeftSweepingHold;
        leftSweep.OnPressEnded += LeftSweepingEnd;
        
        rightSweep.OnPressStarted += RightSweepingStart;
        rightSweep.OnShortTap += RightSweepingTap;
        rightSweep.OnHoldThresholdReached += RightSweepingHold;
        rightSweep.OnPressEnded += RightSweepingEnd;

        
        /*
        // 아웃팟정을 위한 변수. 콜라이더를 사용하지 않도록 하여 물리계산을 줄여 최적화를 위해 미리 아웃 좌표값을 받아옴. (X축이 좌우, Z축이 앞뒤라고 가정. Z축이 값이 높을수록 전진방향으로 가정)
        // 좌측 아웃라인을 변수로 저장. 벽의 좌표 + 벽의 두께 + 스톤의 두께... 아래 방식들도 비슷함
        leftWallOutValue = leftWall.position.x + leftWall.GetComponent<Renderer>().bounds.extents.x + stone.GetComponent<Renderer>().bounds.extents.x + outJudgeStandard;
        rightWallOutValue = rightWall.position.x - rightWall.GetComponent<Renderer>().bounds.extents.x - stone.GetComponent<Renderer>().bounds.extents.x - outJudgeStandard;
        endHogLineOutValue = endHogLine.position.z + stone.GetComponent<Renderer>().bounds.extents.z;
        endHackLineOutValue = endHackLine.position.z - stone.GetComponent<Renderer>().bounds.extents.z;
        */
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (rb != null) return;
            currentState =  LaunchState.WaitingForInitialDrag;
            previousSweepValue = 0;
            currentSweepValue = 0;
            
            shortPressCoroutine = null;
            longHoldCoroutine = null;
            autoDecreaseCoroutine = null;
            
            currentDonutId_A++;
            
            GameObject st = Instantiate(stone, spawnPos.position, spawnPos.rotation);
            forceController = st.GetComponent<StoneForceController>();
            forceController.InitializeDonut(Team.A, currentDonutId_A);
            inGameDonuts_A.Add(st);
            currentDonut = st;
            rb = st.GetComponent<Rigidbody>();
            powerAndDirectionText.SetActive(true);
            makeDonutText.SetActive(false);
            //st.GetComponent<StoneForceController>().AddForceToStone(powerAmount, spinAmount);
        }

        if (rb == null) return;


        // 1단계 입력 (발사 방향)
        if (currentState == LaunchState.WaitingForInitialDrag)
        {
            if (Input.touchCount > 0)
            {
                HandleTouchInput_Launch();
            }
            else
            {
                HandleMouseInput_Launch();
            }
        }
        // 2단계 입력 (회전 값)
        else if (currentState == LaunchState.WaitingForRotationInput)
        {
            if (Input.touchCount > 0)
            {
                HandleTouchInput_Rotation();
            }
            else
            {
                HandleMouseInput_Rotation();
            }
        }

        if (currentState == LaunchState.MovingToHogLine && needToTap == true)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    TapBeforeHogLine();
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                TapBeforeHogLine();
            }
        }

        if (currentState == LaunchState.Launched)
        {
            //CheckDonutTransformToJudgeOut();
        }

        if (isDragging)
        {
            // 현재 입력 위치를 전달하여 시각화 업데이트
            Vector3 currentPos = Input.touchCount > 0 ? Input.GetTouch(0).position : Input.mousePosition;
            UpdateDragVisual(currentPos);
        }

    }

    private void HandleMouseInput_Launch()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag_Launch(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag_Launch(Input.mousePosition); // 함수 이름 변경
        }
    }

    // 이전에 논의했던 터치 통합을 위한 예시 함수
    private void HandleTouchInput_Launch()
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            StartDrag_Launch(touch.position);
        }
        else if (touch.phase == TouchPhase.Ended && isDragging)
        {
            EndDrag_Launch(touch.position);
        }
    }

    // --- 핵심 로직 함수 ---

    private void StartDrag_Launch(Vector3 screenPosition)
    {
        // **오브젝트 확인 없이** 드래그 시작
        isDragging = true;

        // **사용자가 실제로 클릭한 화면 좌표를 저장** (문제점 해결의 핵심)
        actualDragStartScreenPos = screenPosition;

        // 발사할 때 물체에 힘을 가하기 쉽게 키네마틱으로 전환 (선택 사항)
        rb.isKinematic = true;
        uiLaunch.ActivateTapStartPointImage(true, actualDragStartScreenPos);
    }

    private void UpdateDragVisual(Vector3 currentScreenPos)
    {
        // 1. 순수한 2D 화면 좌표 기준의 드래그 벡터 계산
        Vector3 dragVector2D = currentScreenPos - actualDragStartScreenPos;

        // 드래그 거리가 0이 아니어야 방향 계산이 의미가 있습니다.
        if (dragVector2D.magnitude > 0.01f)
        {
            // 2. 발사 방향 벡터 계산 (화면 좌표 기반)
            // 당긴 방향의 반대 방향을 사용 (발사 방향)
            Vector2 launchDirection2D = -dragVector2D.normalized;

            // 3. 발사 방향을 Z축 회전 각도로 변환 (Angle Calculation)
            // Vector2.up (0, 1)을 기준으로 launchDirection2D까지의 각도를 찾습니다.
            float angle = Vector2.SignedAngle(Vector2.up, launchDirection2D);
            angle *= maxUIDirectionAngle / 90f;
            // ******* 핵심 수정: 각도 제한 로직 추가 *******
            // 발사 방향 (launchDirection2D)의 Y 성분은 월드 Z축에 해당합니다.
            // Y 성분이 양수(0보다 큼)라는 것은 발사 방향이 화면 기준 '위쪽'을 향함 (뒤로 발사)을 의미합니다.
            if (launchDirection2D.y < 0)
            {
                // 뒤로 발사가 되는 경우, 각도를 -180도 ~ 180도 범위를 벗어나
                // 전방 범위 (-90도 ~ 90도) 내에서 가장 가까운 각도로 제한해야 합니다.
            
                // 각도가 양수(오른쪽)이면 90도로 제한하고, 음수(왼쪽)이면 -90도로 제한합니다.
                // Vector2.SignedAngle 결과는 -180 ~ 180입니다.

                if (angle > 0) // 오른쪽으로 꺾였다면 (0도 ~ 180도)
                {
                    // 최대 90도로 고정 (오른쪽 전방 제한)
                    angle = maxUIDirectionAngle;
                }
                else // 왼쪽으로 꺾였다면 (-180도 ~ 0도)
                {
                    // 최소 -90도로 고정 (왼쪽 전방 제한)
                    angle = -maxUIDirectionAngle;
                }
            
                // NOTE: 이 방법은 갑작스러운 각도 스냅을 유발할 수 있습니다. 
                // 또는, 그냥 angle = Mathf.Clamp(angle, -90f, 90f); 을 사용해 각도를 90도 범위로 제한합니다.
            
                // 만약 launchDirection2D.y를 0으로 강제한 벡터의 각도를 구한다면:
                // Vector2 clampedLaunchDirection = new Vector2(launchDirection2D.x, 0f);
                // angle = Vector2.SignedAngle(Vector2.up, clampedLaunchDirection);
                // 이 경우, Y=0 이므로 angle은 -90도(x=-1) 또는 90도(x=1)가 됩니다.
                // 좌우 드래그가 아닌, 위쪽으로 드래그 시에는 무조건 가장자리인 90 또는 -90으로 스냅됩니다.
            
                // 보다 부드러운 전환을 위해, Y가 양수일 경우의 각도는 단순히 -90과 90 사이에서 가장 가까운 값으로 스냅하는 것이 좋습니다.
                angle = Mathf.Clamp(angle, -maxUIDirectionAngle, maxUIDirectionAngle);
            }
            // ******* 수정된 핵심 로직 끝 *******

            // 4. 각도 저장
            // UI Rotation에 적용하기 위해 CurrentLaunchAngle에 할당
            currentLaunchAngle = angle;
        }

        // 5. 비율 계산 및 저장 (이전 로직과 동일)
        float dragDistance = dragVector2D.magnitude / mainCamera.pixelHeight;
        currentDragRatio = Mathf.Clamp01(dragDistance / maxDragDistance);
    }

    private void EndDrag_Launch(Vector3 screenPosition)
    {
        if (!isDragging) return;
        uiLaunch.ActivateTapStartPointImage(false);
        // 1. 순수한 2D 화면 좌표 기준의 드래그 벡터 계산 (시작점 수정)
        Vector3 dragVector2D = screenPosition - actualDragStartScreenPos;

        // 2. 당긴 거리 계산 (2D 벡터의 길이)
        float dragDistance = dragVector2D.magnitude / mainCamera.pixelHeight;
        float clampedDistance = Mathf.Min(dragDistance, maxDragDistance);

        // 3. 발사 방향 벡터 계산 (화면 좌표 기반)
        // 당긴 방향의 반대 방향을 사용
        Vector2 launchDirection2D = -dragVector2D.normalized;

        // 4. 드래그 방향에 따른 월드 방향 변환 처리
        Vector3 finalDirection = Vector3.zero;

        // 수직 드래그 방향의 절대값
        float absVerticalDrag = Mathf.Abs(launchDirection2D.y);

        // 수평 드래그 방향의 절대값
        float absHorizontalDrag = Mathf.Abs(launchDirection2D.x);

        

        if (launchDirection2D.y < 0) // 드래그 시작점 기준 '위쪽'으로 발사 방향 벡터가 향할 때
        {
            // Y축 방향(위로 드래그)은 무시하고 X축 방향(좌우)만 사용
            finalDirection = new Vector3(
                launchDirection2D.x, // X축(좌우) 값 유지
                0f, // Y축(수직) 움직임은 0으로 고정
                0f // Z축(앞뒤) 값 0으로 설정하여 앞뒤 움직임 무시
            );
        }
        else // 화면을 아래로 드래그 했을 때 (기존 로직 유지)
        {
            // 스크린 Y축 드래그 -> 월드 Z축, 스크린 X축 드래그 -> 월드 X축
            finalDirection = new Vector3(
                launchDirection2D.x,
                0f, // Y축(수직) 움직임은 0으로 고정
                launchDirection2D.y // 스크린 Y를 월드 Z로 매핑 (아래로 당기면 Z가 양수/앞을 향함)
            );
        }


        finalDirection.Normalize();

        // 5. 힘 계산 및 Rigidbody에 힘 가하기
        float finalForce = clampedDistance * launchForceMultiplier; //당긴거리 * 힘 계수 

        // rb.isKinematic = false; // 물리 활성화
        //rb.AddForce(finalDirection * finalForce, ForceMode.VelocityChange);
        finalLaunchDirectionWithForce = finalDirection * finalForce + Vector3.forward * autoMoveToHogLineSpeed; //실제로 드래그한 힘과 기본적으로 호그라인까지 오던 힘을 더함
        finalLaunchForce = finalForce + autoMoveToHogLineSpeed;
        currentState = LaunchState.WaitingForRotationInput; // 상태 변화
        // rb.gameObject.GetComponent<StoneForceController>().AddForceToStone(powerAmount, spinAmount);
        isDragging = false;
        powerAndDirectionText.SetActive(false);
        rotationText.SetActive(true);
        // CurrentDragRatio = 0f;
        // CurrentLaunchAngle = 0f;
    }

    private void HandleMouseInput_Rotation()
    {
        // 1. 마우스 클릭 시작 (회전 드래그 시작)
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag_Rotation(Input.mousePosition);
        }
        // 2. 마우스 클릭 유지 (실시간 회전값 업데이트)
        else if (Input.GetMouseButton(0) && isDragging)
        {
            // 드래그 중인 동안 실시간으로 회전값을 계산하고 FinalRotationValue에 할당
            UpdateRotationValue(Input.mousePosition);
        }
        // 3. 마우스 버튼 놓기 (회전 입력 완료 및 발사)
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag_Rotation();
        }
    }

    private void HandleTouchInput_Rotation()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        // 1. 터치 시작 (회전 드래그 시작)
        if (touch.phase == TouchPhase.Began)
        {
            StartDrag_Rotation(touch.position);
        }
        // 2. 터치 유지 (실시간 회전값 업데이트)
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            // 드래그 중인 동안 실시간으로 회전값을 계산하고 FinalRotationValue에 할당
            UpdateRotationValue(touch.position);
        }
        // 3. 터치 끝 (회전 입력 완료 및 발사)
        else if (touch.phase == TouchPhase.Ended && isDragging)
        {
            EndDrag_Rotation();
        }
    }

    private void StartDrag_Rotation(Vector3 screenPosition)
    {
        isDragging = true;
        // 실제 마우스 클릭 화면 좌표를 시작점으로 저장
        rotationDragStartScreenPos = screenPosition;
    }

    private void EndDrag_Rotation()
    {
        if (!isDragging) return;

        // 회전값을 최종 확정했으므로, 이제 발사(Launch) 상태로 전환
        //currentState = LaunchState.Launched;
        //rb.isKinematic = false;

        //rb.gameObject.GetComponent<StoneForceController>().AddForceToStone(finalLaunchDirectionWithForce, finalLaunchForce, finalRotationValue);
        isDragging = false;
        //currentLaunchAngle = 0f;
        //currentDragRatio = 0f;

        //rb = null;
        rotationText.SetActive(false);
        //makeDonutText.SetActive(true);
        releaseButton.interactable = true;
        currentState = LaunchState.WaitingForPressRelease;
    }

    // --- 회전값 계산 로직 (핵심) ---
    private void UpdateRotationValue(Vector3 currentScreenPos)
    {
        Vector3 dragVector = currentScreenPos - rotationDragStartScreenPos; //2D 화면 좌표 기준의 드래그 벡터 계산
        float dragXDistance = dragVector.x;     //Y축 이동은 무시하고 X축 이동만 사용
        float normalizedDrag = dragXDistance / mainCamera.pixelWidth;   //X축 거리를 화면 너비 비율로 정규화 (해상도 독립성 확보)
        float dragRatio = Mathf.Clamp(normalizedDrag / maxRotationDragDistance, -1f, 1f);   //최대 드래그 거리를 이용해 -1.0 ~ 1.0 비율로 제한
        finalRotationValue = dragRatio * maxRotationValue;  //드래그 비율에 알맞게 수치값 계산
    }

    private void ReleaseButtonClicked()
    {
        currentState = LaunchState.MovingToHogLine;
        MoveDonutToHogLine();
        needToTap = true;
        /*
        rb.isKinematic = false;

        rb.gameObject.GetComponent<StoneForceController>().AddForceToStone(finalLaunchDirectionWithForce, finalLaunchForce, finalRotationValue);
        currentLaunchAngle = 0f;
        currentDragRatio = 0f;

        rb = null;
        makeDonutText.SetActive(true);
        currentState = LaunchState.WaitingForInitialDrag;
        releaseButton.interactable = false;
        */
    }

    private void MoveDonutToHogLine()
    {
        rb.isKinematic = true; //domove로 움직일때, rigidbody때문에 방해받는것을 방지
        rb.DOMove(startHogLine.position, autoMoveToHogLineSpeed).SetSpeedBased(true).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (needToTap == true)
                {
                    DonutOut(currentDonut);
                }
                rb.isKinematic = false; //이제 물리작용을 적용할것이기에 다시 해제
                rb.velocity = Vector3.zero; // 올바른 힘을 전달하기 위해 기존의 속도를 제거
                float calculatedRandomValue = (100 + releaseRandomValue) * 0.01f;   // 릴리즈 탭 판정을 토대로 가져온 수치를 퍼센트만큼 추가함
                
                // DOMOVE로 움직이던 도넛을 이젠 직접 물리적 힘을 주어서 이동시키기위해 도넛의 물리 스크립트 함수를 호출 
                rb.gameObject.GetComponent<StoneForceController>().AddForceToStone(finalLaunchDirectionWithForce * calculatedRandomValue,
                    finalLaunchForce * calculatedRandomValue, finalRotationValue * calculatedRandomValue, this);
                currentState = LaunchState.Launched;
                currentLaunchAngle = 0f; //설정했던 각도 초기화
                currentDragRatio = 0f;  //설정했던 드래그 값 초기화
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    sweepingPanel.SetActive(true);
                    canSweep = true;
                });
                //rb = null;
                //makeDonutText.SetActive(true);
                //currentState = LaunchState.WaitingForInitialDrag;
                releaseButton.interactable = false;
            });
    }

    private void TapBeforeHogLine()
    {
        if (needToTap == true)
        {
            if (rb.transform.position.z >= perfectZoneLine.position.z &&
                rb.transform.position.z <= startHogLine.position.z) // 퍼펙트존 안에 있을때 탭했으면
            {
                needToTap = false;
                int randomPoint = Random.Range(1, 101); // 0 ~ 100 까지이지만, 소숫점들을 정수로 만들기 위해 10을 곱한 상태에서 계산

                int cumulativeWeight = 0;

                //누적 가중치를 계산하며 랜덤 값이 어느 구간에 속하는지 확인
                foreach (var item in perfectZoneRandomWeights) 
                {
                    cumulativeWeight += (int)(item.weight * 10); // 현재 항목의 가중치를 누적

                    // 무작위로 뽑은 숫자가 현재 누적 가중치보다 작거나 같으면 해당 항목을 선택
                    if (randomPoint * 10 <= cumulativeWeight)
                    {
                        releaseRandomValue = item.value; 
                        break;
                    }
                }
                Debug.Log("퍼펙트존");
                Debug.Log($"releaseRandomValue: {releaseRandomValue}");
            }
            else if (rb.transform.position.z >= earlyZoneLine.position.z &&
                     rb.transform.position.z < perfectZoneLine.position.z) //얼리존 안에 있을때 탭했으면
            {
                needToTap = false;
                int randomPoint = Random.Range(1, 101); // 0 ~ 100 까지이지만, 소숫점들을 정수로 만들기 위해 10을 곱한 상태에서 계산

                int cumulativeWeight = 0;

                // 누적 가중치를 계산하며 랜덤 값이 어느 구간에 속하는지 확인
                foreach (var item in earlyZoneRandomWeights)
                {
                    cumulativeWeight += (int)(item.weight * 10); // 현재 항목의 가중치를 누적

                    // 무작위로 뽑은 숫자가 현재 누적 가중치보다 작거나 같으면 해당 항목을 선택
                    if (randomPoint * 10 <= cumulativeWeight)
                    {
                        releaseRandomValue = item.value;
                        break;
                    }
                }
                Debug.Log("얼리존");
                Debug.Log($"releaseRandomValue: {releaseRandomValue}");
                
            }
            else if(rb.transform.position.z < earlyZoneLine.position.z) // 얼리존보다 일찍 눌렀다면 무시하기
            {
                Debug.Log("너무 일찍 눌러서 탭 무시");
            }
        }
    }

    private void DonutOut(GameObject donut) //도넛의 아웃 판정
    {
        outText.SetActive(true);
        DOVirtual.DelayedCall(3f, () =>
        {
            outText.SetActive(false);
        });
        donut.SetActive(false);

        StoneForceController sfc = donut.GetComponent<StoneForceController>();
        //sfc.team
        
        //rb.gameObject.SetActive(false);
        //forceController.enabled = false;
        //DonutAttackFinished();
        ResetCurrentTurnDonutValues();
    }

    // private void CheckDonutTransformToJudgeOut() // 도넛이 아웃 좌표를 넘어섰는지 계산
    // {
    //     //코드 가독성 및 주석 가독성을 위해 일부로 한줄에 조건을 다 안넣어두고 분리해두었습니다.
    //     if (rb.transform.position.x <= leftWallOutValue) // 왼쪽 벽(라인)의 사전 계산값과 비교하여 넘었는지 확인
    //     {
    //         DonutOut();
    //     }
    //     else if (rb.transform.position.x >= rightWallOutValue) // 오른쪽
    //     {
    //         DonutOut();
    //     }
    //     else if (rb.transform.position.z > endHackLineOutValue) // 엔드 핵 라인 ( 넘어가면 아웃 ) 
    //     {
    //         DonutOut();
    //     }
    //     
    //     // 엔드 호그라인은 도넛이 멈추었을때를 기준으로 판정하기에 이 함수에서는 다루지 않음.
    // }

    public void DonutContactedSideWall(Team team, int donutId)
    {
        if (team == Team.A)
        {
            DonutOut(inGameDonuts_A[donutId]);
        }
        else
        {
            DonutOut(inGameDonuts_B[donutId]);
        }
    }

    private void ResetCurrentTurnDonutValues()
    {
        sweepingPanel.SetActive(false);
        makeDonutText.SetActive(true);
        currentState = LaunchState.AttackFinished;
        currentSweepValue = 0;
        previousSweepValue = 0;
        if (shortPressCoroutine != null)
        {
            StopCoroutine(shortPressCoroutine);
            shortPressCoroutine = null;
        }

        if (longHoldCoroutine != null)
        {
            StopCoroutine(longHoldCoroutine);
            longHoldCoroutine = null;
        }

        if (autoDecreaseCoroutine != null)
        {
            StopCoroutine(autoDecreaseCoroutine);
            autoDecreaseCoroutine = null;
        }
        rb = null;
    }

    public void DonutAttackFinished(bool isPassedEndHogLine) //도넛이 공격을 마치고 모든 움직임을 멈추었을때 처리하는 함수
    {
        if (isPassedEndHogLine == true)
        {
            ResetCurrentTurnDonutValues();
        }
        else
        {
            DonutOut(currentDonut);
        }
    }

    #region Sweep
    private void FrontSweepingStart()
    {
        if (canSweep == false) return;
        // 다른 로직이 실행 중이면 중지합니다.
        if (longHoldCoroutine != null) StopCoroutine(longHoldCoroutine);
        if (shortPressCoroutine != null) StopCoroutine(shortPressCoroutine);
        // **새로 추가**: 버튼을 누르는 순간, 자동 감소 코루틴을 중지
        if (autoDecreaseCoroutine != null)
        {
            StopCoroutine(autoDecreaseCoroutine);
            autoDecreaseCoroutine = null;
        }
        isSweeping = true;
        currentSweepState = SweepState.FrontSweep;
        
        if (previousSweepState != currentSweepState)
        {
            currentSweepValue = 0;
        }
    }

    private void FrontSweepingTap()
    {
        if (canSweep == false) return;
        
        // 짧게 누르면 0.1초마다 0.1씩, 총 0.3까지 증가시키는 코루틴을 매니저에서 시작
        shortPressCoroutine = StartCoroutine(ShortTapStepIncrease());
        
    }

    private void FrontSweepingHold()
    {
        if (canSweep == false) return;
        
        // 0.1초마다 0.1씩 증가시키는 코루틴을 매니저에서 시작
        longHoldCoroutine = StartCoroutine(LongHoldContinuousIncrease());
    }

    private void FrontSweepingEnd()
    {
        // 모든 지속/예약된 증가 로직을 중지합니다.
        if (longHoldCoroutine != null)
        {
            StopCoroutine(longHoldCoroutine);
            longHoldCoroutine = null;
        }
        // 2. [핵심] 감소 시작 조건 확인: 증가 코루틴이 진행 중이 아니고 (ShortPress 코루틴은 스스로 종료 예정), 
        //    값이 0보다 크며, 자동 감소 코루틴이 실행 중이지 않을 때 시작합니다.
        if (currentSweepValue > 0f && autoDecreaseCoroutine == null)
        {
            // AutoDecreaseValue 코루틴을 시작
            autoDecreaseCoroutine = StartCoroutine(AutoDecreaseValue());
        }

        previousSweepState = currentSweepState;
    }
    
    

    private void LeftSweepingStart()
    {
        if (canSweep == false) return;
        // 다른 로직이 실행 중이면 중지합니다.
        if (longHoldCoroutine != null) StopCoroutine(longHoldCoroutine);
        if (shortPressCoroutine != null) StopCoroutine(shortPressCoroutine);
        // **새로 추가**: 버튼을 누르는 순간, 자동 감소 코루틴을 중지
        if (autoDecreaseCoroutine != null)
        {
            StopCoroutine(autoDecreaseCoroutine);
            autoDecreaseCoroutine = null;
        }
        isSweeping = true;
        currentSweepState = SweepState.LeftSweep;
        
        if (previousSweepState != currentSweepState)
        {
            currentSweepValue = 0;
        }
    }

    private void LeftSweepingTap()
    {
        if (canSweep == false) return;
        // 짧게 누르면 0.1초마다 0.1씩, 총 0.3까지 증가시키는 코루틴을 매니저에서 시작
        shortPressCoroutine = StartCoroutine(ShortTapStepIncrease());
    }

    private void LeftSweepingHold()
    {
        if (canSweep == false) return;
        // 0.1초마다 0.1씩 증가시키는 코루틴을 매니저에서 시작
        longHoldCoroutine = StartCoroutine(LongHoldContinuousIncrease());
    }

    private void LeftSweepingEnd()
    {
        // 모든 지속/예약된 증가 로직을 중지합니다.
        if (longHoldCoroutine != null)
        {
            StopCoroutine(longHoldCoroutine);
            longHoldCoroutine = null;
        }
        // 2. [핵심] 감소 시작 조건 확인: 증가 코루틴이 진행 중이 아니고 (ShortPress 코루틴은 스스로 종료 예정), 
        //    값이 0보다 크며, 자동 감소 코루틴이 실행 중이지 않을 때 시작합니다.
        if (currentSweepValue > 0f && autoDecreaseCoroutine == null)
        {
            // AutoDecreaseValue 코루틴을 시작
            autoDecreaseCoroutine = StartCoroutine(AutoDecreaseValue());
        }

        previousSweepState = currentSweepState;
    }

    
    
    private void RightSweepingStart()
    {
        if (canSweep == false) return;
        // 다른 로직이 실행 중이면 중지합니다.
        if (longHoldCoroutine != null) StopCoroutine(longHoldCoroutine);
        if (shortPressCoroutine != null) StopCoroutine(shortPressCoroutine);
        // **새로 추가**: 버튼을 누르는 순간, 자동 감소 코루틴을 중지
        if (autoDecreaseCoroutine != null)
        {
            StopCoroutine(autoDecreaseCoroutine);
            autoDecreaseCoroutine = null;
        }
        
        isSweeping = true;
        currentSweepState = SweepState.RightSweep;
        
        if (previousSweepState != currentSweepState)
        {
            currentSweepValue = 0;
        }
    }
    private void RightSweepingTap()
    {
        if (canSweep == false) return;
        // 짧게 누르면 0.1초마다 0.1씩, 총 0.3까지 증가시키는 코루틴을 매니저에서 시작
        shortPressCoroutine = StartCoroutine(ShortTapStepIncrease());
    }

    private void RightSweepingHold()
    {
        if (canSweep == false) return;
        // 0.1초마다 0.1씩 증가시키는 코루틴을 매니저에서 시작
        longHoldCoroutine = StartCoroutine(LongHoldContinuousIncrease());
    }

    private void RightSweepingEnd()
    {
        // 모든 지속/예약된 증가 로직을 중지합니다.
        if (longHoldCoroutine != null)
        {
            StopCoroutine(longHoldCoroutine);
            longHoldCoroutine = null;
        }
        // 2. [핵심] 감소 시작 조건 확인: 증가 코루틴이 진행 중이 아니고 (ShortPress 코루틴은 스스로 종료 예정), 
        //    값이 0보다 크며, 자동 감소 코루틴이 실행 중이지 않을 때 시작합니다.
        if (currentSweepValue > 0f && autoDecreaseCoroutine == null)
        {
            // AutoDecreaseValue 코루틴을 시작
            autoDecreaseCoroutine = StartCoroutine(AutoDecreaseValue());
        }

        previousSweepState = currentSweepState;
    }
    
    // --- [기능] 길게 누를 때 0.1초마다 0.1씩 증가 ---
    IEnumerator LongHoldContinuousIncrease()
    {
        // 처음 한 번은 즉시 증가시키고 다음 대기 시간을 설정합니다.
        IncreaseValue(LONG_HOLD_INCREASE_AMOUNT);
        
        while (true)
        {
            yield return new WaitForSeconds(LONG_HOLD_INCREASE_INTERVAL);
            IncreaseValue(LONG_HOLD_INCREASE_AMOUNT);
        }
    }
    
    // --- [기능] 짧게 누를 때 0.3초에 걸쳐 0.3까지 증가 ---
    IEnumerator ShortTapStepIncrease()
    {
        float targetValue = Mathf.Min(currentSweepValue + SHORT_PRESS_TARGET_INCREASE, MAX_VALUE);
        
        // 목표 값에 도달할 때까지 반복합니다.
        while (currentSweepValue < targetValue)
        {
            // 0.1초 대기
            yield return new WaitForSeconds(SHORT_PRESS_INTERVAL); // 기존 LONG_HOLD_INCREASE_INTERVAL 사용 가능
            
            // 0.1씩 증가 (Min을 사용하여 목표 값을 초과하지 않도록 방지)
            currentSweepValue = Mathf.Min(currentSweepValue + SHORT_PRESS_STEP, targetValue);
            forceController.SweepValueChanged(currentSweepState, currentSweepValue);
        }

        shortPressCoroutine = null; // 코루틴 완료

        // 자동 감소 로직 시작 (기존 로직 유지)
        if (currentSweepValue > 0f && autoDecreaseCoroutine == null)
        {
            autoDecreaseCoroutine = StartCoroutine(AutoDecreaseValue());
        }
    }
    
    // --- [기능] 버튼을 놓았을 때 0.1초마다 0.1씩 감소 ---
    IEnumerator AutoDecreaseValue()
    {
        while (currentSweepValue > 0f)
        {
            // 0.1초 대기
            yield return new WaitForSeconds(DECREASE_INTERVAL);

            // 0.1씩 감소 및 0 미만으로 내려가지 않도록 방지
            currentSweepValue = Mathf.Max(currentSweepValue - DECREASE_AMOUNT, 0f);
            forceController.SweepValueChanged(currentSweepState, currentSweepValue);
        }
        
        // 값이 0이 되면 코루틴을 종료합니다.
        autoDecreaseCoroutine = null;
    }
    
    // --- [기능] 값 증가 공통 함수 ---
    private void IncreaseValue(float amount)
    {
        currentSweepValue = Mathf.Min(currentSweepValue + amount, MAX_VALUE); 
        forceController.SweepValueChanged(currentSweepState, currentSweepValue);
    }
    
    #endregion
}