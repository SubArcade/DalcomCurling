using UnityEngine;
using TMPro;

/// <summary>
/// 플로팅 텍스트의 동작을 제어하는 스크립트. 애니메이션 및 자동 파괴를 포함합니다.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class FloatingText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;

    // 애니메이션 지속 시간
    public float duration = 1.5f;
    // 텍스트가 위로 움직일 거리
    public float moveDistance = 1.0f;

    private float timer;
    private Vector3 startPosition;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        startPosition = transform.position;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > duration)
        {
            // 지속 시간이 지나면 오브젝트 파괴
            Destroy(gameObject);
            return;
        }

        // 위로 이동하는 애니메이션
        float progress = timer / duration;
        transform.position = startPosition + new Vector3(0, progress * moveDistance, 0);

        // 페이드 아웃 애니메이션
        Color color = textMesh.color;
        color.a = 1f - progress;
        textMesh.color = color;
    }
    
    /// <summary>
    /// 표시할 텍스트를 설정합니다.
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
