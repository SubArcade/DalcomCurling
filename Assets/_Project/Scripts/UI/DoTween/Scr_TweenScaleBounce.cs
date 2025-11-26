using DG.Tweening;
using UnityEngine;

/// <summary>
/// UI 요소의 크기를 조절하여 바운스 효과를 주는 컴포넌트입니다.
/// </summary>
public class ScaleBounce : MonoBehaviour
{
    [Tooltip("목표 크기")]
    public Vector3 targetScale = Vector3.one * 1.2f;
    [Tooltip("애니메이션에 걸리는 시간")]
    public float duration = 0.5f;
    [Tooltip("애니메이션 시작 전 딜레이")]
    public float delay = 0f;
    [Tooltip("바운스 효과의 강도 (Vibrato)")]
    public int vibrato = 10;
    [Tooltip("바운스 효과의 탄성 (Elasticity)")]
    public float elasticity = 1f;
    [Tooltip("시작 시 자동 재생 여부")]
    public bool playOnAwake = true;
    [Tooltip("애니메이션 반복 여부")]
    public bool loop = false;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        if (playOnAwake)
        {
            Play();
        }
    }

    /// <summary>
    /// 스케일 바운스 애니메이션을 재생합니다.
    /// </summary>
    public void Play()
    {
        transform.localScale = originalScale;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(transform.DOPunchScale(targetScale - originalScale, duration, vibrato, elasticity));

        if (loop)
        {
            sequence.SetLoops(-1, LoopType.Restart);
        }
    }
}
