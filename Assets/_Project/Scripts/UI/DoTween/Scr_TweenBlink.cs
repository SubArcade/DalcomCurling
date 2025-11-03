using UnityEngine;
using DG.Tweening;

/// <summary>
/// CanvasGroup의 알파 값을 조절하여 깜빡이는 효과를 주는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class Blink : MonoBehaviour
{
    [Tooltip("깜빡이는 속도 (한 번 깜빡이는 데 걸리는 시간)")]
    public float duration = 1f;
    [Tooltip("최소 알파 값")]
    [Range(0f, 1f)]
    public float minAlpha = 0f;
    [Tooltip("최대 알파 값")]
    [Range(0f, 1f)]
    public float maxAlpha = 1f;
    [Tooltip("애니메이션 시작 전 딜레이")]
    public float delay = 0f;
    [Tooltip("적용할 Ease 타입")]
    public Ease easeType = Ease.Linear;
    [Tooltip("시작 시 자동 재생 여부")]
    public bool playOnAwake = true;

    private CanvasGroup canvasGroup;
    private Tween blinkTween;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if (playOnAwake)
        {
            Play();
        }
    }

    void OnDisable()
    {
        // 비활성화될 때 트윈을 정지시켜 리소스를 정리합니다.
        blinkTween?.Kill();
    }

    /// <summary>
    /// 깜빡임 애니메이션을 재생합니다.
    /// </summary>
    public void Play()
    {
        // 기존 트윈이 있다면 정지시킵니다.
        blinkTween?.Kill();
        
        canvasGroup.alpha = maxAlpha;
        blinkTween = canvasGroup.DOFade(minAlpha, duration)
            .SetEase(easeType)
            .SetDelay(delay)
            .SetLoops(-1, LoopType.Yoyo); // Yoyo 루프를 사용하여 왔다갔다 하도록 설정
    }
}
