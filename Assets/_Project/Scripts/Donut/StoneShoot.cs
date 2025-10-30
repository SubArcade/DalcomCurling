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
        Launched
    }
    public enum SweepState
    {
        FrontSweep,
        LeftSweep,
        RightSweep,
        None
    }
    //public float powerAmount = 10f;

    //public float spinAmount = 0f;

    public Rigidbody rigid;
    [Header("스크립트 참조")] 
    public UI_LaunchIndicator uiLaunch;


    // --- 설정 변수 ---
    [Header("Launch Settings")] public float launchForceMultiplier = 5.5f; // 당긴 거리에 곱해질 힘의 계수, 초기값 5.5
    public float maxDragDistance = 0.5f; // 최대 드래그 허용 거리 , 초기값 0.5
    public float maxRotationDragDistance = 1f; // 회전 입력 최대 드래그 거리 (화면 높이 비율), 초기값 1
    public float maxRotationValue = 5f; // 회전 입력 최대값 (5), 초기값 5
    public float autoMoveToHogLineSpeed = 5f; // 호그라인까지 자동으로 이동할때 사용될 속도값, 초기값 5
    
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
    public LaunchState currentState { get; private set; } = LaunchState.WaitingForInitialDrag;

    private Vector3 finalLaunchDirectionWithForce;
    private float finalLaunchForce;

    private bool needToTap = false;

    // 최종 회전값 (외부에 노출할 float 값, 새로 추가)
    // 이 값을 사용자의 회전 로직에 입력하시면 됩니다.
    public float finalRotationValue { get; private set; } = 0f;

    // 임시 회전 입력 시작점 (새로 추가)
    private Vector3 rotationDragStartScreenPos;

    [Header("UI 오브젝트")] public GameObject makeDonutText;
    public GameObject powerAndDirectionText;
    public GameObject rotationText;
    public Button releaseButton;
    public Button frontSweepButton;
    public Button leftSweepButton;
    public Button rightSweepButton;

    [Header("게임 오브젝트")] public GameObject stone;
    public Transform spawnPos;

    public Transform releasePoint;
    public Transform startHogLine;
    public Transform earlyZoneLine;
    public Transform perfectZoneLine;

    //--- UI 연결 변수 ---
    public float currentDragRatio { get; private set; }
    public float currentLaunchAngle { get; private set; }

    // --- 내부 변수 ---
    private float releaseRandomValue = -99f;
    private Vector3 actualDragStartScreenPos;
    private Rigidbody rb;
    public bool isDragging { get; private set; } = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        currentState = LaunchState.WaitingForInitialDrag;
        makeDonutText.SetActive(true);
        releaseButton.onClick.AddListener(ReleaseButtonClicked);
        frontSweepButton.onClick.AddListener(FrontSweeping);
        leftSweepButton.onClick.AddListener(LeftSweeping);
        rightSweepButton.onClick.AddListener(RightSweeping);
    }

    // 스톤 생성부분을 매니저에 일임 (턴 시작시 자동 생성)
    public void PrepareForShot(Rigidbody stoneRb)
    {
        rb = stoneRb;
        currentState = LaunchState.WaitingForInitialDrag;
        powerAndDirectionText.SetActive(true);
        makeDonutText.SetActive(false);
    }

    void Update()
    {
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

        if (currentState == LaunchState.Launched && needToTap == true)
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
        currentState = LaunchState.Launched;
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
        rb.DOMove(releasePoint.position, autoMoveToHogLineSpeed).SetSpeedBased(true).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                rb.isKinematic = false; //이제 물리작용을 적용할것이기에 다시 해제
                rb.velocity = Vector3.zero; // 올바른 힘을 전달하기 위해 기존의 속도를 제거
                float calculatedRandomValue = (100 + releaseRandomValue) * 0.01f;   // 릴리즈 탭 판정을 토대로 가져온 수치를 퍼센트만큼 추가함
                
                // DOMOVE로 움직이던 도넛을 이젠 직접 물리적 힘을 주어서 이동시키기위해 도넛의 물리 스크립트 함수를 호출 
                rb.gameObject.GetComponent<StoneForceController>().AddForceToStone(finalLaunchDirectionWithForce * calculatedRandomValue,
                    finalLaunchForce * calculatedRandomValue, finalRotationValue * calculatedRandomValue);
                
                currentLaunchAngle = 0f; //설정했던 각도 초기화
                currentDragRatio = 0f;  //설정했던 드래그 값 초기화

                //rb = null;
                makeDonutText.SetActive(true);
                currentState = LaunchState.WaitingForInitialDrag;
                releaseButton.interactable = false;
            });
    }

    private void TapBeforeHogLine()
    {
        if (needToTap == true)
        {
            needToTap = false;
            if (rb.transform.position.z > perfectZoneLine.position.z &&
                rb.transform.position.z <= startHogLine.position.z) // 퍼펙트존 안에 있을때 탭했으면
            {
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
            else if (rb.transform.position.z > earlyZoneLine.position.z &&
                     rb.transform.position.z <= perfectZoneLine.position.z) //얼리존 안에 있을때 탭했으면
            {
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
            else // 얼리존과 퍼펙트존 모두를 벗어났다면
            {
                Debug.Log("랜덤 릴리즈 벗어남");
            }
        }
    }

    private void FrontSweeping()
    {
        
    }

    private void LeftSweeping()
    {
        
    }

    private void RightSweeping()
    {
        
    }
}