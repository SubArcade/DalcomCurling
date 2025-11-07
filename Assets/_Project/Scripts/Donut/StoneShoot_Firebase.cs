using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// 로컬 플레이어의 샷 입력 처리와 샷 데이터 계산을 담당합니다.
public class StoneShoot_Firebase : MonoBehaviour
{
    // 입력이 완료되면, 계산된 샷 데이터를 담아 이 이벤트를 발생시킵니다.
    public event Action<LastShot> OnShotConfirmed;

    public enum LaunchState
    {
        WaitingForInitialDrag,
        WaitingForRotationInput,
        WaitingForPressRelease,
        MovingToHogLine,
        Launched
    }

    [Header("스크립트 및 UI 참조")]
    public UI_LaunchIndicator_Firebase uiLaunch;

    public StoneManager stoneManager;
    public GameObject powerAndDirectionText;
    public GameObject rotationText;
    public Button releaseButton;

    [Header("설정 변수")]
    public float launchForceMultiplier = 4f;
    public float maxDragDistance = 0.5f;
    public float maxRotationDragDistance = 1f;
    public float maxRotationValue = 5f;
    public float autoMoveToHogLineSpeed = 6f;
    public float maxUIDirectionAngle = 60f;

    [Header("퍼펙트존 확률 조정 가능 변수")]
    //public List<StoneShoot.WeightedItem> perfectZoneRandomWeights; // StoneShoot의 구조체 재사용
    //public List<StoneShoot.WeightedItem> earlyZoneRandomWeights;
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
        // 총 합계는 반드시 100이어야 합니다.
    };
    
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
        // 총 합계는 반드시 100이어야 합니다.
    };

    [Header("씬 참조 오브젝트")]
    public Transform startHogLine;
    public Transform perfectZoneLine;
    public Transform earlyZoneLine;

    // --- 상태 및 출력 변수 ---
    public LaunchState CurrentState { get; private set; } = LaunchState.WaitingForInitialDrag;
    public float FinalRotationValue { get; private set; } = 0f;
    public float CurrentDragRatio { get; private set; }
    public float CurrentLaunchAngle { get; private set; }
    public bool IsDragging { get; private set; } = false;

    // --- 내부 변수 ---
    private bool _inputEnabled = false;
    private Rigidbody _currentStoneRb;
    [SerializeField]private Camera _mainCamera;
    private Vector3 _actualDragStartScreenPos;
    private Vector3 _rotationDragStartScreenPos;
    private Vector3 _finalLaunchDirection;
    private float _finalLaunchForce;
    private float _releaseRandomValue = -99f;
    private bool _needToTap = false;
    private LastShot myShot;

    void Awake()
    {
        //_mainCamera = Camera.main;
        releaseButton.onClick.AddListener(ReleaseButtonClicked);
        DisableInput(); // 시작 시 비활성화
    }

    public void EnableInput(Rigidbody stoneRb)
    {
        _inputEnabled = true;
        _currentStoneRb = stoneRb;
        CurrentState = LaunchState.WaitingForInitialDrag;
        powerAndDirectionText.SetActive(true);
        Debug.Log("InputController: 입력 활성화됨.");
    }

    public void DisableInput()
    {
        _inputEnabled = false;
        powerAndDirectionText.SetActive(false);
        rotationText.SetActive(false);
        releaseButton.interactable = false;
        if (uiLaunch != null) uiLaunch.ActivateTapStartPointImage(false);
        Debug.Log("InputController: 입력 비활성화됨.");
    }

    void Update()
    {
        if (!_inputEnabled) return;

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

        if (IsDragging)
        {
            Vector3 currentPos = Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
            UpdateDragVisual(currentPos);
        }
    }

    #region 입력 처리 (Input Handling)
    private void HandleLaunchInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag_Launch(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && IsDragging)
        {
            EndDrag_Launch(Input.mousePosition);
        }
    }

    private void HandleRotationInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag_Rotation(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && IsDragging)
        {
            UpdateRotationValue(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && IsDragging)
        {
            EndDrag_Rotation();
        }
    }

    private void HandleTapInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TapBeforeHogLine();
        }
    }
    #endregion

    #region 드래그 로직 (Drag Logic)
    private void StartDrag_Launch(Vector3 screenPosition)
    {
        IsDragging = true;
        _actualDragStartScreenPos = screenPosition;
        if (_currentStoneRb != null) _currentStoneRb.isKinematic = true;
        uiLaunch.ActivateTapStartPointImage(true, _actualDragStartScreenPos);
        powerAndDirectionText.SetActive(true);
       
    }

    private void EndDrag_Launch(Vector3 screenPosition)
    {
        if (!IsDragging) return;
        uiLaunch.ActivateTapStartPointImage(false);

        Vector3 dragVector2D = screenPosition - _actualDragStartScreenPos;
        float dragDistance = dragVector2D.magnitude / _mainCamera.pixelHeight;
        float clampedDistance = Mathf.Min(dragDistance, maxDragDistance);
        Vector2 launchDirection2D = -dragVector2D.normalized;

        _finalLaunchDirection = (launchDirection2D.y < 0)
            ? new Vector3(launchDirection2D.x, 0f, 0f)
            : new Vector3(launchDirection2D.x, 0f, launchDirection2D.y);
        _finalLaunchDirection.Normalize();

        _finalLaunchForce = clampedDistance * launchForceMultiplier;

        CurrentState = LaunchState.WaitingForRotationInput;
        IsDragging = false;
        powerAndDirectionText.SetActive(false);
        rotationText.SetActive(true);
    }

    private void StartDrag_Rotation(Vector3 screenPosition)
    {
        IsDragging = true;
        _rotationDragStartScreenPos = screenPosition;
    }

    private void EndDrag_Rotation()
    {
        if (!IsDragging) return;
        IsDragging = false;
        rotationText.SetActive(false);
        releaseButton.interactable = true;
        CurrentState = LaunchState.WaitingForPressRelease;
    }
    #endregion

    #region 값 계산 및 시각화 (Value Calculation & Visualization)
    private void UpdateDragVisual(Vector3 currentScreenPos)
    {
        Vector3 dragVector2D = currentScreenPos - _actualDragStartScreenPos;
        if (dragVector2D.magnitude > 0.01f)
        {
            Vector2 launchDirection2D = -dragVector2D.normalized;
            float angle = Vector2.SignedAngle(Vector2.up, launchDirection2D);
            angle *= maxUIDirectionAngle / 90f;
            if (launchDirection2D.y < 0) angle = Mathf.Clamp(angle, -maxUIDirectionAngle, maxUIDirectionAngle);
            CurrentLaunchAngle = angle;
        }
        float dragDistance = dragVector2D.magnitude / _mainCamera.pixelHeight;
        CurrentDragRatio = Mathf.Clamp01(dragDistance / maxDragDistance);
    }

    private void UpdateRotationValue(Vector3 currentScreenPos)
    {
        Vector3 dragVector = currentScreenPos - _rotationDragStartScreenPos;
        float dragXDistance = dragVector.x;
        float normalizedDrag = dragXDistance / _mainCamera.pixelWidth;
        float dragRatio = Mathf.Clamp(normalizedDrag / maxRotationDragDistance, -1f, 1f);
        FinalRotationValue = dragRatio * maxRotationValue;
    }
    #endregion

    #region 발사 및 탭 (Launch & Tap)
    private void ReleaseButtonClicked()
    {
        CurrentState = LaunchState.MovingToHogLine;
        MoveDonutToHogLine();
        FirebaseGameManager.Instance.ChangeCameraRelease(); // 버튼누르면 시점 변화
        _needToTap = true;
    }

    private void MoveDonutToHogLine()
    {
        if (_currentStoneRb == null) return;
        _currentStoneRb.isKinematic = true;
        _currentStoneRb.DOMove(startHogLine.position, autoMoveToHogLineSpeed).SetSpeedBased(true).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (_needToTap)
                {  
                    // 탭을 못했으면 랜덤값 0으로 처리 또는 다른 패널티
                    _releaseRandomValue = 0;
                }
                // FinalizeShot()에서 샷 데이터를 FirebaseGameManager에 전달하고, 실제 발사는 StoneManager에서 담당
                FinalizeShot();
                FirebaseGameManager.Instance.ChangeLocalStateToSimulatingMyShot();                
                FirebaseGameManager.Instance.ChangeFixedDeltaTime();
                stoneManager.LaunchStone(myShot, stoneManager.myTeam == StoneForceController_Firebase.Team.A ? stoneManager.aShotCount : stoneManager.bShotCount);
            });
    }

    public void SimulateStone(Rigidbody currentDonut, LastShot shotData, int stoneId)
    {
        currentDonut.isKinematic = true;
        currentDonut.DOMove(startHogLine.position, autoMoveToHogLineSpeed).SetSpeedBased(true).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                stoneManager.LaunchStone(shotData, stoneId);
            });
    }

    private void TapBeforeHogLine()
    {
        if (!_needToTap || _currentStoneRb == null) return;

        float zPos = _currentStoneRb.transform.position.z;
        List<StoneShoot.WeightedItem> weights = null;

        if (zPos >= perfectZoneLine.position.z && zPos <= startHogLine.position.z)
        {
            weights = perfectZoneRandomWeights;
            Debug.Log("퍼펙트존");
        }
        else if (zPos >= earlyZoneLine.position.z && zPos < perfectZoneLine.position.z)
        {
            weights = earlyZoneRandomWeights;
            Debug.Log("얼리존");
        }

        if (weights != null)
        {
            _needToTap = false;
            int randomPoint = Random.Range(1, 101);
            Debug.Log($"randomPoint : {randomPoint}");
            int cumulativeWeight = 0;
            foreach (var item in weights)
            {
                cumulativeWeight += (int)(item.weight * 10);
                Debug.Log($"itemweight : {item.weight}");
                if (randomPoint * 10 <= cumulativeWeight)
                { 
                    _releaseRandomValue = item.value;
                    break;
                }
            }
            Debug.Log($"releaseRandomValue: {_releaseRandomValue}");
        }
    }
    #endregion

    // 최종 샷 데이터를 FirebaseGameManager에 전달
    private void FinalizeShot()
    {
        if (!_inputEnabled) return;

        // DOTween 애니메이션과 물리 시뮬레이션의 충돌을 막기 위해, 모든 트윈을 즉시 중단합니다.
        if (_currentStoneRb != null) _currentStoneRb.DOKill();

        float calculatedRandomValue = (100 + _releaseRandomValue) * 0.01f;
        Vector3 finalDirection = _finalLaunchDirection * _finalLaunchForce + Vector3.forward * autoMoveToHogLineSpeed;
        float finalForce = _finalLaunchForce + autoMoveToHogLineSpeed;

        var directionDict = new Dictionary<string, float>
        {
            { "x", finalDirection.x * calculatedRandomValue },
            { "y", finalDirection.y * calculatedRandomValue },
            { "z", finalDirection.z * calculatedRandomValue }
        };

        var releasePosDict = new Dictionary<string, float>
        {
            { "x", _currentStoneRb.transform.position.x },
            { "y", _currentStoneRb.transform.position.y },
            { "z", _currentStoneRb.transform.position.z }
        };

        LastShot shotData = new LastShot
        {
            Force = finalForce * calculatedRandomValue,
            Team = stoneManager.myTeam,
            Spin = FinalRotationValue * calculatedRandomValue,
            Direction = directionDict,
            ReleasePosition = releasePosDict
        };

        myShot = shotData;
        OnShotConfirmed?.Invoke(shotData);
        
        Debug.Log("샷 정보 전송 완료.");

        DisableInput();
    }
}
