using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneShoot : MonoBehaviour
{
    public enum LaunchState { WaitingForInitialDrag, WaitingForRotationInput, Launched }
    public float powerAmount = 10f;

    public float spinAmount = 0f;

    public Rigidbody rigid;

    public GameObject stone;

    public Transform spawnPos;
    
    // --- 설정 변수 ---
    [Header("Launch Settings")]
    public float launchForceMultiplier = 5f; // 당긴 거리에 곱해질 힘의 계수
    public float maxDragDistance = 2f;       // 최대 드래그 허용 거리
    public float maxRotationDragDistance = 1f; // 회전 입력 최대 드래그 거리 (화면 높이 비율)
    public float maxRotationValue = 5f;        // 회전 입력 최대값 (5)
    
    // --- 상태 및 출력 변수 ---
    // 현재 상태 (새로 추가)
    public LaunchState currentState { get; private set; } = LaunchState.WaitingForInitialDrag;
    
    private Vector3 finalLaunchDirectionWithForce;
    private float finalLaunchForce;
    // 최종 회전값 (외부에 노출할 float 값, 새로 추가)
    // 이 값을 사용자의 회전 로직에 입력하시면 됩니다.
    public float finalRotationValue { get; private set; } = 0f;
    
    // 임시 회전 입력 시작점 (새로 추가)
    private Vector3 rotationDragStartScreenPos;
    
    //--- UI 연결 변수 ---
    public float currentDragRatio { get; private set; }
    public float currentLaunchAngle { get; private set; }

    // --- 내부 변수 ---
    private Vector3 actualDragStartScreenPos;
    private Rigidbody rb;
    public bool isDragging { get; private set; } = false;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        currentState = LaunchState.WaitingForInitialDrag;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject st = Instantiate(stone, spawnPos.position, spawnPos.rotation);
            rb = st.GetComponent<Rigidbody>();
            //st.GetComponent<StoneForceController>().AddForceToStone(powerAmount, spinAmount);
        }

        if (rb == null) return;
        
        
        // 1단계 입력 (발사 방향)
        if (currentState == LaunchState.WaitingForInitialDrag)
        {
            if (Input.touchCount > 0) { HandleTouchInput_Launch(); }
            else { HandleMouseInput_Launch(); }
        }
        // 2단계 입력 (회전 값)
        else if (currentState == LaunchState.WaitingForRotationInput)
        {
            if (Input.touchCount > 0) { HandleTouchInput_Rotation(); }
            else { HandleMouseInput_Rotation(); }
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
            // Vector2.SignedAngle을 사용하여 Y축(위쪽)을 기준으로 각도를 계산합니다.
            // Vector2.up (0, 1)을 기준으로 launchDirection2D까지의 각도를 찾습니다.
            // 이 각도는 -180도에서 180도 사이의 값이 나옵니다.
            float angle = Vector2.SignedAngle(Vector2.up, launchDirection2D);

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

        // 1. 순수한 2D 화면 좌표 기준의 드래그 벡터 계산 (시작점 수정)
        // (마우스 끝점) - (실제 드래그 시작점 화면 좌표)
        Vector3 dragVector2D = screenPosition - actualDragStartScreenPos;
        
        // 2. 당긴 거리 계산 (2D 벡터의 길이)
        float dragDistance = dragVector2D.magnitude / mainCamera.pixelHeight;
        float clampedDistance = Mathf.Min(dragDistance, maxDragDistance);

        // 3. 발사 방향 벡터 계산 (화면 좌표 기반)
        // 당긴 방향의 반대 방향을 사용
        Vector2 launchDirection2D = -dragVector2D.normalized;

        // 4. 2D 화면 벡터를 3D 월드 X-Z 평면 방향으로 변환
        // (스크린 Y축 드래그 -> 월드 Z축, 스크린 X축 드래그 -> 월드 X축)
        Vector3 finalDirection = new Vector3(
            launchDirection2D.x,
            0f, // Y축(수직) 움직임은 0으로 고정하여 평면 발사
            launchDirection2D.y // 스크린 Y를 월드 Z로 매핑
        );
        
        finalDirection.Normalize();

        // 5. 힘 계산 및 Rigidbody에 힘 가하기
        float finalForce = clampedDistance * launchForceMultiplier; 
        
       // rb.isKinematic = false; // 물리 활성화
        //rb.AddForce(finalDirection * finalForce, ForceMode.VelocityChange);
        finalLaunchDirectionWithForce =  finalDirection * finalForce;
        finalLaunchForce = finalForce;
        currentState = LaunchState.WaitingForRotationInput;
       // rb.gameObject.GetComponent<StoneForceController>().AddForceToStone(powerAmount, spinAmount);
        isDragging = false;
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
        currentState = LaunchState.Launched;
        rb.isKinematic = false;
        
        rb.gameObject.GetComponent<StoneForceController>().AddForceToStone(finalLaunchDirectionWithForce, finalLaunchForce, finalRotationValue);
        isDragging = false;
        currentLaunchAngle = 0f;
        currentDragRatio = 0f;

        currentState = LaunchState.WaitingForInitialDrag;
    }
    
    // --- 회전값 계산 로직 (핵심) ---
    private void UpdateRotationValue(Vector3 currentScreenPos)
    {
        // 1. 순수한 2D 화면 좌표 기준의 드래그 벡터 계산
        Vector3 dragVector = currentScreenPos - rotationDragStartScreenPos;
        
        // 2. Y축 이동은 무시하고 X축 이동만 사용
        float dragXDistance = dragVector.x;
        
        // 3. X축 거리를 화면 너비 비율로 정규화 (해상도 독립성 확보)
        float normalizedDrag = dragXDistance / mainCamera.pixelWidth;

        // 4. 최대 드래그 거리를 이용해 -1.0 ~ 1.0 비율로 제한
        float dragRatio = Mathf.Clamp(normalizedDrag / maxRotationDragDistance, -1f, 1f);

        // 5. 비율을 최종 [-5, 5] 범위로 변환
        // [-1, 1] * 5f
        finalRotationValue = dragRatio * maxRotationValue;
        
        // Debug.Log($"Rotation Value: {FinalRotationValue:F2}");
    }
}
