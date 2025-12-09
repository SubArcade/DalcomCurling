using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SwipeToDown : MonoBehaviour
{
    [Header("도넛 오브젝트")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform donut;   // 움직일 도넛
    [SerializeField] private Image donutImage; 
    [SerializeField] private Image effectImage; 
    [SerializeField] private float moveDistance = 300f; // 아래로 이동할 거리
    [SerializeField] private float duration = 1.0f;   // 애니메이션 시간
    [SerializeField] private float delayTime = 0.5f;  // 페이드 딜레이
    [SerializeField] private float introStartDelayTime = 1f;

    private Vector2 originalPos;
    private float originalAlpha;

    private void Awake()
    {
        originalPos = donut.anchoredPosition;
        originalAlpha = donutImage.color.a;

        ResetDonut();
        
        //StartSwipeDown();
        PlayBlinkEffect();
        StartCoroutine(DelaySwipeDown(introStartDelayTime));
    }

    private IEnumerator DelaySwipeDown(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartSwipeDown();
    }
    
    private void StartSwipeDown()
    {
        if (donut == null) return;

        Vector3 startPos = donut.anchoredPosition + Vector2.up * moveDistance;
        donut.anchoredPosition = startPos; 
        
        // 아래 방향으로 이동
        Vector3 targetPos = donut.anchoredPosition + Vector2.down * moveDistance;

        // 이동
        donut.DOAnchorPos(targetPos, duration).SetEase(Ease.InOutQuad);

        // 페이드 아웃
        float fadeDelay = duration * delayTime;
        donutImage.DOFade(1f, duration * delayTime).SetEase(Ease.InOutQuad).SetDelay(fadeDelay);
    }

    private void ResetDonut()
    {
        DOTween.Kill(donut);
        DOTween.Kill(donutImage);

        panel.SetActive(true);
        donut.gameObject.SetActive(true);

        donut.anchoredPosition = originalPos;

        var c = donutImage.color;
        c.a = originalAlpha;
        donutImage.color = c;
    }
    
    private void PlayBlinkEffect()
    {
        if (effectImage == null) return;

        // 먼저 기존 트윈 모두 제거 (중복 실행 방지)
        DOTween.Kill(effectImage);

        // Alpha 깜빡임
        effectImage
            .DOFade(0.3f, 0.6f)          // 살짝 투명해짐
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo); // 무한 반복 Yoyo
    }
}