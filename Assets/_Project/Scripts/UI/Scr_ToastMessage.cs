using UnityEngine;
using TMPro;
using DG.Tweening;

public class ToastMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI toastText;
    [SerializeField] private float showDuration = 1.5f;   // 유지 시간
    [SerializeField] private float fadeTime = 0.35f;      // 부드럽게 나타나는 시간

    private Tween currentTween;

    private bool isInitialized = false;

    private void Init()
    {
        if (isInitialized) return;
        isInitialized = true;
        toastText.alpha = 0f;
        toastText.gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        Init();
        // 이전 애니메이션 중지
        currentTween?.Kill();

        toastText.text = message;
        toastText.gameObject.SetActive(true);
        toastText.alpha = 0f;  // 초기 상태

        // DOTween 시퀀스 생성
        Sequence seq = DOTween.Sequence();

        seq.Append(toastText.DOFade(1f, fadeTime))      // 나타나기
           .AppendInterval(showDuration)                // 유지
           .Append(toastText.DOFade(0f, fadeTime))      // 사라지기
           .OnComplete(() => toastText.gameObject.SetActive(false));

        currentTween = seq;
    }
}
