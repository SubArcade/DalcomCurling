using UnityEngine;
using DG.Tweening;

public class Scr_TweenHandDragGuide : MonoBehaviour
{
    [Header("드래그 설정")]
    [SerializeField] private float dragDistance = 200f; // 얼마나 이동할지(px or world)
    [SerializeField] private float dragTime = 1f;       // 1회 드래그 시간
    [SerializeField] private int loopCount = 2;         // 반복 횟수(PingPong 포함)

    private RectTransform handRect;
    private Sequence currentSeq;
    private Vector3 originalScale;
    private void Awake()
    {
        handRect = GetComponent<RectTransform>();
        originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        currentSeq?.Kill();
    }

    /// <summary>
    /// 위에서 아래로 드래그
    /// </summary>
    public void PlayVerticalDrag()
    {
        PlayDragAnimation(direction: Vector2.down);
    }

    /// <summary>
    /// 왼쪽에서 오른쪽으로 드래그
    /// </summary>
    public void PlayHorizontalDrag()
    {
        PlayDragAnimation(direction: Vector2.right);
    }

    public void PlayTouchMove()
    {
        transform.localScale = originalScale;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(transform.DOPunchScale( Vector3.one * 1.2f - originalScale, dragTime, 7, 1f));
        sequence.SetLoops(loopCount, LoopType.Restart);
        
    }

    /// <summary>
    /// 공통 드래그 애니메이션
    /// </summary>
    private void PlayDragAnimation(Vector2 direction)
    {
        currentSeq?.Kill();
        currentSeq = DOTween.Sequence();

        Vector2 originalPos = handRect.anchoredPosition;
        Vector2 targetPos = originalPos + direction.normalized * dragDistance;

        currentSeq.Append(handRect.DOAnchorPos(targetPos, dragTime).SetEase(Ease.InOutQuad))
                  .SetLoops(loopCount * 2, LoopType.Yoyo) // 왕복을 포함하여 자연스럽게 반복
                  .OnComplete(() =>
                  {
                      // 끝나면 원래 자리로 리셋
                      handRect.anchoredPosition = originalPos;
                  });
    }
}
