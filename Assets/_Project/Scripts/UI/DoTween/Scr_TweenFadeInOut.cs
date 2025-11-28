using DG.Tweening;
using UnityEngine;

/// <summary>
/// CanvasGroup의 알파 값을 조절하여 페이드 인/아웃 효과를 주는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class FadeInOut : MonoBehaviour
{
    [Tooltip("페이드 인에 걸리는 시간")]
    public float fadeInDuration = 1f;
    [Tooltip("페이드 아웃에 걸리는 시간")]
    public float fadeOutDuration = 1f;
    [Tooltip("애니메이션 시작 전 딜레이")]
    public float delay = 0f;
    [Tooltip("적용할 Ease 타입")]
    public Ease easeType = Ease.Linear;
    [Tooltip("애니메이션 반복 여부")]
    public bool loop = false;
    [Tooltip("시작 시 자동 재생 여부")]
    public bool playOnAwake = true;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        // CanvasGroup 컴포넌트를 가져옵니다.
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if (playOnAwake)
        {
            Play();
        }
    }

    /// <summary>
    /// 페이드 애니메이션을 재생합니다.
    /// </summary>
    public void Play()
    {
        // 초기 알파 값을 0으로 설정합니다.
        canvasGroup.alpha = 0f;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(canvasGroup.DOFade(1f, fadeInDuration).SetEase(easeType));
        sequence.Append(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(easeType).SetDelay(1f)); // 1초 대기 후 페이드 아웃

        if (loop)
        {
            sequence.SetLoops(-1, LoopType.Restart);
        }
    }
}
