using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SweepButtonScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("길게 누름으로 판단할 최소 시간 (초)")]
    public float holdDurationThreshold = 0.3f; 
    
    private float pointerDownTime;
    private bool isPointerDown = false;
    private bool hasPassedThreshold = false; // 홀드 임계값을 통과했는지 여부

    // --- 매니저에게 알릴 이벤트 ---
    
    // 눌리기 시작했을 때 (매니저가 코루틴 등을 시작할 수 있도록)
    public Action OnPressStarted; 
    
    // 짧게 눌렀다 떼었을 때 (0.3초 미만)
    public Action OnShortTap; 
    
    // 홀드 임계값(0.3초)을 넘기는 '순간'
    public Action OnHoldThresholdReached; 
    
    // 버튼이 떼어졌을 때 (길게 눌렀든 짧게 눌렀든 관계없이 마지막 상태를 매니저에게 알림)
    // 매니저가 여기서 코루틴을 중지하거나 발사 로직을 실행합니다.
    public Action OnPressEnded; 

    // 포인터가 눌렸을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownTime = Time.time;
        isPointerDown = true;
        hasPassedThreshold = false;
        
        // 눌리기 시작했음을 매니저에게 알립니다.
        OnPressStarted?.Invoke();
    }

    // 포인터가 떼어졌을 때
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown) return;
        
        isPointerDown = false;
        
        float holdTime = Time.time - pointerDownTime;

        // 짧게 누름으로 끝났을 경우 (임계값 미만)
        if (holdTime < holdDurationThreshold)
        {
            OnShortTap?.Invoke();
        }
        
        // 떼어졌음을 매니저에게 알립니다. (길게 눌렀든 짧게 눌렀든 상관 없음)
        OnPressEnded?.Invoke();
    }

    // 매 프레임 호출되는 함수
    void Update()
    {
        // 길게 누르는 중이며, 아직 임계값을 통과하지 않은 상태일 때
        if (isPointerDown && !hasPassedThreshold)
        {
            if (Time.time - pointerDownTime >= holdDurationThreshold)
            {
                hasPassedThreshold = true;
                
                // 홀드 임계값을 넘겼음을 매니저에게 알립니다.
                // 매니저가 이 시점부터 지속적인 값 증가 로직을 시작합니다.
                OnHoldThresholdReached?.Invoke();
            }
        }
    }

}
