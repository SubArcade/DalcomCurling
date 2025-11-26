using UnityEngine;
using UnityEngine.UI;

public class TutorialHighlighter : MonoBehaviour
{
    [SerializeField] private Image overlay;
    [SerializeField] private Material highlightMaterial;

    private void Awake()
    {
        overlay.material = new Material(highlightMaterial);
        overlay.gameObject.SetActive(false);
    }

    // 특정 UI 강조
    public void Highlight(RectTransform target, float radius = 0.15f)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);

        Vector2 normalized = new Vector2(
            screenPos.x / Screen.width,
            screenPos.y / Screen.height
        );

        overlay.material.SetVector("_HoleCenter", normalized);
        overlay.material.SetFloat("_HoleRadius", radius);

        overlay.gameObject.SetActive(true);
    }

    // 하이라이트 종료
    public void Hide()
    {
        overlay.gameObject.SetActive(false);
    }
}

