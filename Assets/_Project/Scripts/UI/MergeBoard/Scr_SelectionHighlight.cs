using UnityEngine;
using DG.Tweening;

public class SelectionHighlight : MonoBehaviour
{
    private RectTransform rt;
    private Tween pulseTween;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        StartPulse();
    }

    void OnDisable()
    {
        if (pulseTween != null)
            pulseTween.Kill();
    }

    private void StartPulse()
    {
        rt.localScale = Vector3.one;

        pulseTween = rt.DOScale(1.1f, 0.45f)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
