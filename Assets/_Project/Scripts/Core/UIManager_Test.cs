using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum PanelId_Test
{
    Start,
    Login,
    Main,
    DetailedSettings,
    Giftbox,
    Info,
    DonutInfo,
    Matching
}

public class UIManager_Test : MonoBehaviour
{
    public static UIManager_Test Instance { get; private set; }
    
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject settingsPanel;

    private readonly Dictionary<PanelId_Test, GameObject> currentPanel = new();
    [SerializeField] private PanelId_Test currentPanelId = PanelId_Test.Login;
    public PanelId_Test CurrentPanelId => currentPanelId;
    
    [SerializeField] private Button settingsBackBtn;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        HideLoading();
        if (settingsPanel) settingsPanel.SetActive(false);
        
        //settingsBackBtn.onClick.AddListener(OnClickCloseSettingUI);
    }

    // ===== 패널 레지스트리 =====
    public void RegisterPanel(PanelId_Test id, GameObject panel)
    {
        if (panel == null) return;
        currentPanel[id] = panel;
    }

    public void Open(PanelId_Test id, Action onOpen  = null)
    {
        StartCoroutine(OpenRoutine(id, onOpen ));
    }
    
    private IEnumerator OpenRoutine(PanelId_Test id, Action onOpen)
    {
        if (currentPanel == null || currentPanel.Count == 0)
        {
            Debug.LogError("[UIManager] currentPanel is null or empty");
            onOpen?.Invoke();
            yield break;
        }

        GameObject target = null;

        // 선택한 패널만 활성화, 나머지는 비활성화
        foreach (var kv in currentPanel)
        {
            var go = kv.Value;
            if (go == null) continue;

            bool active = kv.Key.Equals(id);
            if (active) target = go;

            if (go.activeSelf != active)
                go.SetActive(active);
        }

        currentPanelId = id;
        yield return new WaitForSeconds(1f);

        // 열림 완료 콜백
        onOpen?.Invoke();
    }
    
    public void HideAllPanels()
    {
        foreach (var kv in currentPanel)
            if (kv.Value) kv.Value.SetActive(false);
    }
    
    public void ShowLoading(string msg = null)
    {
        if (loadingPanel)
        {
            loadingPanel.SetActive(true);
            //Debug.Log("[UIManager] loading panel 실행");
        }
    }
    public void HideLoading()
    {
        if (loadingPanel)
        {
            loadingPanel.SetActive(false);
            //Debug.Log("[UIManager] Hide loading panel");
        }
    }

    public void ShowSettings(bool show)
    {
        if (settingsPanel) settingsPanel.SetActive(show);
        
        if (!PhotonNetwork.InRoom) return; //메뉴창(룸입장 전)닫기누르면 오류코드 안나게 방어코드넣음
    }

    public void ShowSettings() // 룸입장 안한 메뉴에서 세팅열때 쓸녀석
    {
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    // 버튼 바인딩용
    public void OnClickOpenSettingUI()  => ShowSettings(true);
    public void OnClickCloseSettingUI() => ShowSettings(false);
}
