using DG.Tweening;
using UnityEngine;

/// <summary>
/// RectTransform을 이용하여 UI 요소를 슬라이드 인 효과로 보여주는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SlideIn : MonoBehaviour
{
    public enum SlideDirection
    {
        FromLeft,
        FromRight,
        FromTop,
        FromBottom
    }

    [Tooltip("슬라이드 방향")]
    public SlideDirection direction = SlideDirection.FromLeft;
    [Tooltip("슬라이드 거리. 0이면 화면 크기만큼 이동합니다.")]
    public float slideDistance = 0;
    [Tooltip("애니메이션에 걸리는 시간")]
    public float duration = 1f;
    [Tooltip("애니메이션 시작 전 딜레이")]
    public float delay = 0f;
    [Tooltip("적용할 Ease 타입")]
    public Ease easeType = Ease.OutExpo;
    [Tooltip("시작 시 자동 재생 여부")]
    public bool playOnAwake = true;

    private RectTransform rectTransform;
    private Vector2 originalPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    void OnEnable()
    {
        if (playOnAwake)
        {
            Play();
        }
    }

    /// <summary>
    /// 슬라이드 인 애니메이션을 재생합니다.
    /// </summary>
    public void Play()
    {
        Vector2 startPosition = GetStartPosition();
        rectTransform.anchoredPosition = startPosition;
        rectTransform.DOAnchorPos(originalPosition, duration)
            .SetDelay(delay)
            .SetEase(easeType);
    }

    private Vector2 GetStartPosition()
    {
        Rect parentRect = GetParentRect();
        float distance = slideDistance > 0 ? slideDistance : GetDistance(parentRect);

        switch (direction)
        {
            case SlideDirection.FromLeft:
                return new Vector2(originalPosition.x - distance, originalPosition.y);
            case SlideDirection.FromRight:
                return new Vector2(originalPosition.x + distance, originalPosition.y);
            case SlideDirection.FromTop:
                return new Vector2(originalPosition.x, originalPosition.y + distance);
            case SlideDirection.FromBottom:
                return new Vector2(originalPosition.x, originalPosition.y - distance);
            default:
                return originalPosition;
        }
    }

    private Rect GetParentRect()
    {
        RectTransform parent = transform.parent as RectTransform;
        return parent != null ? parent.rect : new Rect(0, 0, Screen.width, Screen.height);
    }

    private float GetDistance(Rect parentRect)
    {
        switch (direction)
        {
            case SlideDirection.FromLeft:
                return parentRect.width + rectTransform.rect.width / 2;
            case SlideDirection.FromRight:
                return parentRect.width + rectTransform.rect.width / 2;
            case SlideDirection.FromTop:
                return parentRect.height + rectTransform.rect.height / 2;
            case SlideDirection.FromBottom:
                return parentRect.height + rectTransform.rect.height / 2;
            default:
                return 0;
        }
    }
}
