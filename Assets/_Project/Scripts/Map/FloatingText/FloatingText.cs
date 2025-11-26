using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// 플로팅 텍스트의 동작을 제어하는 스크립트. 활성화 시 애니메이션을 실행하고 자동으로 비활성화됩니다.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class FloatingText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;

    [Header("애니메이션 설정")]
    // 애니메이션 지속 시간
    public float duration = 1.5f;
    // 텍스트가 위로 움직일 거리 (픽셀 단위)
    public float moveDistance = 100f;

    [Header("동작 설정")]
    // true일 경우 애니메이션 완료 후 오브젝트를 파괴합니다. false일 경우 비활성화합니다.
    // 인스턴스 프리팹의 경우 true, 오브젝트 풀링의 경우 false로 설정합니다.
    public bool destroyOnComplete = true;
    public GameObject destroyTargetObject;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    // 오브젝트가 활성화될 때마다 호출됩니다.
    void OnEnable()
    {
        // 활성화 시 상태 초기화
        textMesh.alpha = 1.0f;
        
        // 이전 애니메이션이 남아있을 경우를 대비해 Kill
        transform.DOKill();
        textMesh.DOKill();

        // DOTween을 사용한 애니메이션 시퀀스 생성
        Sequence sequence = DOTween.Sequence(this);
        
        sequence.Append(transform.DOMoveY(transform.position.y + moveDistance, duration).SetEase(Ease.OutQuad));
        sequence.Join(textMesh.DOFade(0f, duration).SetEase(Ease.InQuad));
        sequence.OnComplete(() =>
        {
            if (destroyOnComplete)
            {
                if (destroyTargetObject != null)
                {
                    Destroy(destroyTargetObject);
                }
                else 
                { 
                    Destroy(gameObject);
                }
                    
            }
            else
            {
                gameObject.SetActive(false);
            }
        });
    }

    /// <summary>
    /// 표시할 텍스트를 설정합니다.
    /// 이 메서드는 gameObject.SetActive(true)를 호출하기 전에 사용해야 합니다.
    /// </summary>
    /// <param name="text">표시할 메시지</param>
    public void SetText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }
}
