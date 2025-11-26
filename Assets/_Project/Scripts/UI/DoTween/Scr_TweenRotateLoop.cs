using DG.Tweening;
using UnityEngine;

/// <summary>
/// UI 요소를 계속해서 회전시키는 컴포넌트입니다.
/// </summary>
public class RotateLoop : MonoBehaviour
{
    [Tooltip("회전할 각도 (Z축 기준)")]
    public Vector3 rotationAngle = new Vector3(0, 0, 360);
    [Tooltip("한 바퀴 회전하는 데 걸리는 시간")]
    public float duration = 2f;
    [Tooltip("회전 방향")]
    public RotateMode rotateMode = RotateMode.FastBeyond360;
    [Tooltip("적용할 Ease 타입")]
    public Ease easeType = Ease.Linear;
    [Tooltip("시작 시 자동 재생 여부")]
    public bool playOnAwake = true;

    private Tween rotateTween;

    void OnEnable()
    {
        if (playOnAwake)
        {
            Play();
        }
    }

    void OnDisable()
    {
        rotateTween?.Kill();
    }

    /// <summary>
    /// 회전 애니메이션을 재생합니다.
    /// </summary>
    public void Play()
    {
        rotateTween?.Kill();
        // Z축을 기준으로 360도 회전하는 트윈을 생성하고 무한 반복합니다.
        rotateTween = transform.DORotate(rotationAngle, duration, rotateMode)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Restart);
    }
}
