using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_MainMenu : MonoBehaviour
{
    public Button energyButton;
    public TMP_Text energyText;
    
    public Button loginButton;

    void Awake()
    {
        energyButton.onClick.AddListener(()=> testAdd());
        loginButton.onClick.AddListener(TestLogin);
    }
    
    private void OnEnable() =>  DataManager.Instance.OnUserDataChanged += see;
    private void Disable() =>  DataManager.Instance.OnUserDataChanged -= see;
    
    // 데이터 UI 노출
    public void see(PlayerData playerData)
    {
        energyText.text = $"{playerData.energy}/{playerData.maxEnergy}";
    }
    
    // 에너지 추가
    void testAdd()
    {
        int energy = DataManager.Instance.PlayerData.energy + 1;
        DataManager.Instance.UpdateUserDataAsync(energy: energy);
        energyText.text = $"{energy}/{DataManager.Instance.PlayerData.maxEnergy}";
    }
    
    // 테스트 계정 로그인
    void TestLogin()
    {
        FirebaseAuthManager.Instance.Login("asd@asd.asd", "asdasd");
        UIManager.Instance.Open(PanelId.Main);
    }
}
