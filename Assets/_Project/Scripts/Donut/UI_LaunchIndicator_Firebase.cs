using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LaunchIndicator_Firebase : MonoBehaviour
{
    [Header("References")]
    // 발사 로직 스크립트를 드래그 앤 드롭으로 연결
    public StoneShoot_Firebase launchScript; 
    
    // UI Slider 컴포넌트를 드래그 앤 드롭으로 연결
    [Header("UI오브젝트들")]
    public Slider forceSlider;
    public Slider rotationSlider;
    public Slider sweepValueSlider;
    public RectTransform arrowRectTransform;
    public CanvasGroup arrowCanvasGroup;
    public CanvasGroup rotationAmountSliderCanvasGroup;
    public GameObject tapStartPointImage;
    void Update()
    {
        if (launchScript == null || arrowRectTransform == null || arrowCanvasGroup == null || rotationAmountSliderCanvasGroup == null) return;

        // 드래그 상태 확인
        bool isCurrentlyDragging = launchScript.IsDragging && launchScript.CurrentDragRatio > 0.01f;

        // 1. UI 표시/숨김 처리
        // 드래그 중이 아니거나 힘이 거의 0일 때는 UI를 숨깁니다.
        arrowCanvasGroup.alpha = isCurrentlyDragging && (launchScript.CurrentState == StoneShoot_Firebase.LaunchState.WaitingForInitialDrag)? 1f : 0f;
        arrowCanvasGroup.blocksRaycasts = isCurrentlyDragging;
        
        rotationAmountSliderCanvasGroup. alpha = isCurrentlyDragging && (launchScript.CurrentState == StoneShoot_Firebase.LaunchState.WaitingForRotationInput)? 1f : 0f;

        if (isCurrentlyDragging && launchScript.CurrentState == StoneShoot_Firebase.LaunchState.WaitingForInitialDrag)
        {
            // 2. 힘(거리) 슬라이더 업데이트
            if (forceSlider != null)
            {
                forceSlider.value = launchScript.CurrentDragRatio;
            }
            
            // 3. 화살표 회전 업데이트 (핵심 로직)
            // Z축 회전만 변경하여 화살표가 방향을 가리키도록 합니다.
            // RectTransform의 localEulerAngles 속성을 사용합니다.
            Vector3 rotation = arrowRectTransform.localEulerAngles;
            rotation.z = launchScript.CurrentLaunchAngle;
            arrowRectTransform.localEulerAngles = rotation;

            // 추가: 화살표의 크기를 드래그 비율에 따라 변경하여 피드백 강화
            float scale = Mathf.Lerp(0.5f, 1f, launchScript.CurrentDragRatio);
            arrowRectTransform.localScale = Vector3.one * scale;
        }
        else if (isCurrentlyDragging && launchScript.CurrentState == StoneShoot_Firebase.LaunchState.WaitingForRotationInput)
        {
            if (rotationSlider != null)
            {
                rotationSlider.value = launchScript.FinalRotationValue;
            }
            
            
        }
        else if (launchScript.CurrentState == StoneShoot_Firebase.LaunchState.Launched)
        {
            // 스위핑 로직이 제거되었으므로 sweepValueSlider는 더 이상 업데이트하지 않습니다.
            // if (sweepValueSlider != null)
            // {
            //     sweepValueSlider.value = launchScript.currentSweepValue;
            // }
        }
        else
        {
            forceSlider.value = 0;
        }
    }

    public void ActivateTapStartPointImage(bool activate, Vector3 pos = default(Vector3))
    {
        tapStartPointImage.transform.position = pos;
        tapStartPointImage.SetActive(activate);
        if (activate == true)
        {
            arrowRectTransform.position = pos;
        }
    }
    
}
