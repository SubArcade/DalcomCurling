using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class SwipeToStart : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("도넛 오브젝트")]
    [SerializeField] private RectTransform  donut;   // 움직일 도넛
    [SerializeField] private Image donutImage; 
    [SerializeField] private float swipeThreshold = 50f; // 스와이프 감지 거리
    [SerializeField] private float moveDistance = 100f; // 위로 이동할 월드 거리
    [SerializeField] private float duration = 1.0f;   // 애니메이션 시간
    [SerializeField] private float dealyTime = 0.5f;   // 애니메이션 시간
    

    private Vector2 startTouchPos;
    private bool isSwiped = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        startTouchPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isSwiped) return;

        Vector2 endPos = eventData.position;
        float swipeDist = endPos.y - startTouchPos.y;

        if (swipeDist > swipeThreshold)
        {
            isSwiped = true;
            StartSwipeUp();
        }
    }

    private void StartSwipeUp()
    {
        if (donut == null) return;

        Vector3 targetPos = donut.anchoredPosition + Vector2.up * moveDistance;
        
        donut.DOAnchorPos(targetPos, duration).SetEase(Ease.InOutQuad);
        donut.DOScale(0.2f, duration).SetEase(Ease.InOutQuad);
        
        float fadeDelay = duration * dealyTime;
        donutImage.DOFade(0f, duration * dealyTime).SetEase(Ease.InOutQuad).SetDelay(fadeDelay);
        
        // 애니메이션 끝난 후 도넛 비활성화 + 다음 화면
        DOVirtual.DelayedCall(duration, () =>
        {
            donut.gameObject.SetActive(false);
            UIManager.Instance.Open(PanelId.MainPanel);
        });
    }
}