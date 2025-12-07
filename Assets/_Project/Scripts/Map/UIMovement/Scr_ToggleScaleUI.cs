using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ToggleScaleUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private float enlargedScale = 1.2f; // 커질 때 스케일
    [SerializeField] private float duration = 0.25f; // 시간

    private bool isEnlarged = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        isEnlarged = !isEnlarged;

        float targetScale = isEnlarged ? enlargedScale : 1f;

        targetRect.DOScale(targetScale, duration)
                  .SetEase(Ease.OutBack);
    }
    private void OnDisable()
    {
        // DOTween 중지 + 스케일 초기화
        targetRect.DOKill();
        targetRect.localScale = Vector3.one;

        // 상태값도 초기화
        isEnlarged = false;
    }
}
