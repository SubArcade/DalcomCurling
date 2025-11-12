using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DonutInfoPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text donutInfo;

    public TMP_Text DonutInfo => donutInfo;

    void Awake()
    {
        // Inspector 연결이 안 되어 있으면 자동 탐색 (비활성화 포함)
        if (donutInfo == null)
        {
            donutInfo = GetComponentInChildren<TMP_Text>(true);
            if (donutInfo == null)
                Debug.LogError("❌ DonutInfoPopup에서 TMP_Text를 찾지 못했습니다!");
        }
    }

    public void Hide() => gameObject.SetActive(false);
}
