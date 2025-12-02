using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 특정 이벤트가 발생했을 때 UnityEvent를 호출하여 다른 DoTween 애니메이션을 실행시키는 트리거입니다.
/// </summary>
public class TweenTrigger : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum TriggerType
    {
        OnClick,
        OnPointerEnter,
        OnPointerExit,
        OnEnable,
        OnCollisionEnter,
        OnTriggerEnter
    }

    [Tooltip("이벤트를 발생시킬 트리거 타입")]
    public TriggerType triggerType = TriggerType.OnClick;
    

    [Tooltip("호출할 UnityEvent")]
    public UnityEvent onTrigger;

    void OnEnable()
    {
        if (triggerType == TriggerType.OnEnable)
        {
            onTrigger.Invoke();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (triggerType == TriggerType.OnClick)
        {
            onTrigger.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (triggerType == TriggerType.OnPointerEnter)
        {
            onTrigger.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (triggerType == TriggerType.OnPointerExit)
        {
            onTrigger.Invoke();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (triggerType == TriggerType.OnCollisionEnter)
        {
            onTrigger.Invoke();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerType == TriggerType.OnTriggerEnter)
        {
            onTrigger.Invoke();
        }
    }
    
    // 2D 충돌 이벤트도 추가할 수 있습니다.
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (triggerType == TriggerType.OnCollisionEnter)
        {
            onTrigger.Invoke();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerType == TriggerType.OnTriggerEnter)
        {
            onTrigger.Invoke();
        }
    }
}
