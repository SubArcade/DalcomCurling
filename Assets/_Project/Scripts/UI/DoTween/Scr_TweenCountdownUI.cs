using DG.Tweening;
using TMPro;
using UnityEngine;

public class Scr_TweenCountdownUI : MonoBehaviour
{
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private float interval = 1f; // 숫자 간격 시간

    private Sequence countdownSequence;

    private void OnEnable()
    {
        StartCountdown();
    }

    private void OnDisable()
    {
        // 객체가 비활성화될 때 진행 중인 트윈을 완전히 정리
        if (countdownSequence != null && countdownSequence.IsActive())
        {
            countdownSequence.Kill();
            countdownSequence = null;
        }

        // 혹시 남아있을 수 있는 텍스트 초기화
        countdownText.text = "";
    }

    private void StartCountdown()
    {
        // 이전 시퀀스가 남아있다면 먼저 제거
        if (countdownSequence != null && countdownSequence.IsActive())
            countdownSequence.Kill();

        countdownSequence = DOTween.Sequence();

        // 10부터 1까지 자동으로 반복 추가
        for (int i = 10; i >= 1; i--)
        {
            int num = i; // 캡처 변수
            countdownSequence.AppendCallback(() => ShowNumber(num));
            countdownSequence.AppendInterval(interval);
        }

        // 마지막 "END!" 표시
        countdownSequence
            .AppendCallback(() => ShowNumberText("END!"))
            .AppendInterval(interval)
            .OnComplete(() =>
            {
                countdownText.text = "";
                Debug.Log("카운트다운 완료!");
            });
    }

    private void ShowNumber(int number)
    {
        countdownText.text = number.ToString();
        AnimateText();
    }

    private void ShowNumberText(string text)
    {
        countdownText.text = text;
        AnimateText();
    }

    private void AnimateText()
    {
        // 텍스트가 톡 튀어나오는 애니메이션
        countdownText.transform.localScale = Vector3.zero;
        countdownText.transform
            .DOScale(1f, 0.5f)
            .SetEase(Ease.OutBack);
    }
}
