using UnityEngine;
using UnityEngine.UI;

public class UIManager_test2 : MonoBehaviour
{
    [Header("매칭 UI")]
    public GameObject matchingPanel;
    public Button cancelButton;

    [Header("시작 UI")]
    public GameObject startPanel;
    public Button startButton;

    void Start()
    {
        // UI 초기화
        if (matchingPanel != null)
            matchingPanel.SetActive(false);

        if (startPanel != null)
            startPanel.SetActive(true);

        // 버튼 이벤트 연결
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelButtonClicked);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
    }

    public void OnStartButtonClicked()
    {
        Debug.Log("Start 버튼 클릭됨");

        // 시작 UI 숨기고 매칭 UI 표시
        if (startPanel != null)
            startPanel.SetActive(false);

        ShowMatchingUI();

        // PhotonManager에 전달
        /*PhotonManager photonManager = FindObjectOfType<PhotonManager>();
        if (photonManager != null)
        {
            photonManager.OnStartButtonClicked();
        }*/
    }

    public void ShowMatchingUI()
    {
        if (matchingPanel != null)
            matchingPanel.SetActive(true);
    }

    public void HideMatchingUI()
    {
        if (matchingPanel != null)
            matchingPanel.SetActive(false);

        // Start 버튼 다시 보이게
        if (startPanel != null)
            startPanel.SetActive(true);
    }

    public void OnCancelButtonClicked()
    {
        Debug.Log("취소 버튼 클릭됨");

        /*PhotonManager photonManager = FindObjectOfType<PhotonManager>();
        if (photonManager != null)
        {
            photonManager.OnCancelButtonClicked();
        }*/
    }
}