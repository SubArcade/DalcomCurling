using UnityEngine;

/// <summary>
/// 플로팅 텍스트 생성을 관리하는 싱글톤 매니저 클래스
/// </summary>
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [Tooltip("표시할 플로팅 텍스트의 프리팹")]
    public GameObject floatingTextPrefab;
    [Tooltip("텍스트가 생성될 부모 캔버스")]
    public Canvas worldSpaceCanvas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 지정된 위치에 플로팅 텍스트를 표시합니다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="position">텍스트가 나타날 월드 좌표</param>
    public void ShowText(string message, Vector3 position)
    {
        if (floatingTextPrefab == null || worldSpaceCanvas == null)
        {
            Debug.LogError("FloatingTextManager: Prefab 또는 Canvas가 설정되지 않았습니다.");
            return;
        }

        // 프리팹으로부터 플로팅 텍스트 오브젝트 생성
        GameObject textObject = Instantiate(floatingTextPrefab, position, Quaternion.identity, worldSpaceCanvas.transform);
        
        // FloatingText 컴포넌트를 가져와 텍스트 설정
        FloatingText floatingText = textObject.GetComponent<FloatingText>();
        if (floatingText != null)
        {
            floatingText.SetText(message);
        }
        else
        {
            Debug.LogError("FloatingTextManager: 프리팹에 FloatingText 컴포넌트가 없습니다.");
        }
    }
}
