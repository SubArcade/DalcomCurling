using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_MainMenu : MonoBehaviour
{
    public TextMeshProUGUI energyText;
    public GameObject energyTimer_Text;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemText;
    
    public Button loginButton;
    
    public TMP_Dropdown dropdown;
    public string id;
    public string pw;

    public Button testDBSaveButton;
    public Button testLogoutButton;

    private Dictionary<string, string> idDictionary = new Dictionary<string, string>
    {
        { "오민호", "omh@test.com" },
        { "김홍겸", "khg@test.com" },
        { "이성준", "lsj@test.com" },
        { "유병욱", "ybu@test.com" },
        { "최빛", "cb@test.com" },
        { "이강민", "lkm@test.com" },
        { "곽승지", "ksj@test.com" },
        { "윤성준", "ysj@test.com" },
        { "양창곤", "ycg@test.com" },
        { "이필립", "lpl@test.com" }
    };
    
    private Dictionary<string, string> pwDictionary = new Dictionary<string, string>
    {
        { "오민호", "omh123" },
        { "김홍겸", "khg123" },
        { "이성준", "lsj123" },
        { "유병욱", "ybu123" },
        { "최빛", "cb1234" },
        { "이강민", "lkm123" },
        { "곽승지", "ksj123" },
        { "윤성준", "ysj123" },
        { "양창곤", "ycg123" },
        { "이필립", "lpl123" }
    };
    
    void Awake()
    {
        //DataManager.Instance.PlayerData.energy = 0;
        loginButton.onClick.AddListener(TestLogin);
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(idDictionary.Keys));
        OnDropdownChanged(0);
        // 값 변경 시 호출될 이벤트 등록
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
        testDBSaveButton.onClick.AddListener(async () => await DataManager.Instance.SaveAllUserDataAsync());
        testLogoutButton.onClick.AddListener(() =>
        {
            DataManager.Instance.isLogin = false;
            FirebaseAuthManager.Instance.Logout();
        });
        //testLogoutButton.onClick.AddListener(addNameTitle);
    }

    // void OnEnable() 
    // {
    //     DataManager.Instance.OnUserDataChanged += see;
    //     see(DataManager.Instance.PlayerData);
    // }
    // void OnDisable() =>  DataManager.Instance.OnUserDataChanged -= see;
    

    // 데이터 UI 노출
    // public void see(PlayerData playerData)
    // {
    //     Debug.Log("에너지 셋업");
    //     energyText.text = $"{playerData.energy}/{playerData.maxEnergy}";
    //     if (playerData.energy >= playerData.maxEnergy)
    //     {
    //         energyTimer_Text.SetActive(true);
    //         TextMeshProUGUI t = energyTimer_Text.GetComponent<TextMeshProUGUI>();
    //         t.text = "가득참!";
    //     }
    //     else 
    //     {
    //         energyTimer_Text.SetActive(false);
    //     }
    //     goldText.text = $"{playerData.gold}";
    //     gemText.text = $"{playerData.gem}";
    // }
    
    // 에너지 추가
    void testAdd()
    {
        //DataManager.Instance.PlayerData.energy++;
        //DataManager.Instance.UpdateUserDataAsync(energy: energy);
        energyText.text = $"{ DataManager.Instance.PlayerData.energy}";
    }

    // 테스트 계정 로그인
    void TestLogin()
    {
        //FirebaseAuthManager.Instance.Login("asd@asd.asd", "asdasd");
        FirebaseAuthManager.Instance.Login(id, pw);
        UIManager.Instance.Open(PanelId.StartPanel);
        //UIManager.Instance.Open(PanelId.MainPanel);
    }
    
    void OnDropdownChanged(int index)
    {
        string display = dropdown.options[index].text;
        id = idDictionary[display];
        pw = pwDictionary[display];
    }

    void addNameTitle()
    {
        DataManager.Instance.PlayerData.gainNamePlateType.Add(NamePlateType.DoughNewbie);
        DataManager.Instance.PlayerData.gainNamePlateType.Add(NamePlateType.SoftTouch);
        DataManager.Instance.PlayerData.gainNamePlateType.Add(NamePlateType.DoughHandler);
        DataManager.Instance.PlayerData.gainNamePlateType.Add(NamePlateType.DonutPilot);
        DataManager.Instance.PlayerData.gainNamePlateType.Add(NamePlateType.DonutMaster);
    }
}
