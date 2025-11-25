using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class SwipeToStart : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("도넛 오브젝트")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform donut;   // 움직일 도넛
    [SerializeField] private Image donutImage; 
    [SerializeField] private float swipeThreshold = 50f; // 스와이프 감지 거리
    [SerializeField] private float moveDistance = 100f; // 위로 이동할 월드 거리
    [SerializeField] private float duration = 1.0f;   // 애니메이션 시간
    [SerializeField] private float dealyTime = 0.5f;   // 애니메이션 시간
    
    [Header("Analytics")]
    [SerializeField] private GameObject appLunch;
    

    private Vector2 startTouchPos;
    private bool isSwiped = false;

    private Vector2 originalPos;
    private Vector3 originalScale;
    private float originalAlpha;

    void OnEnable()
    {
        ResetDonut();
    } 
    
    private void Awake()
    {
        // 처음 상태 저장
        originalPos = donut.anchoredPosition;
        originalScale = donut.localScale;
        originalAlpha = donutImage.color.a;
    }
    
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
            //donut.gameObject.SetActive(false);
            UIManager.Instance.Open(PanelId.MainPanel);
            
            // 애널리틱 종료
            appLunch.gameObject.SetActive(false);
        });
    }
    
    // 초기화
    private void ResetDonut()
    {
        // 모든 DOTween 트윈 중지 (중간에 초기화할 때 꼬임 방지)
        DOTween.Kill(donut);
        DOTween.Kill(donutImage);

        panel.SetActive(true);
        donut.gameObject.SetActive(true);
        isSwiped = false;

        donut.anchoredPosition = originalPos;
        donut.localScale = originalScale;

        var c = donutImage.color;
        c.a = originalAlpha;
        donutImage.color = c;
    }
}