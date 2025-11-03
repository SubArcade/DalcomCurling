using Firebase;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginSystem : MonoBehaviour
{
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField password;
    
    [SerializeField] private TMP_Text outputText;

    [SerializeField] private Button createButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button guestButton;
    [SerializeField] private Button googleButton;
    [SerializeField] private Button testLoginButton;

    [SerializeField] private GameObject loginPanel;

    private async void Awake()
    {
        Debug.Log("[FirebaseInit] Firebase 초기화 시작");

        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dep == DependencyStatus.Available)
        {
            Debug.Log("[FirebaseInit] Firebase 초기화 완료 ✅");

            // Firebase가 완전히 준비된 시점에 AuthManager 초기화
            InitializeAuthManager();
        }
        else
        {
            Debug.LogError($"[FirebaseInit] Firebase 초기화 실패: {dep}");
        }
    }
    private void InitializeAuthManager()
    {
        FirebaseAuthManager.Instance.Init();

        // 확인용 로그
        Debug.Log($"[FirebaseInit] AuthManager 초기화 완료 상태: {FirebaseAuthManager.Instance != null}");
    }

    void Start()
    {
        FirebaseAuthManager.Instance.LoginState += OnChangedState;

        // email = GameObject.Find("Input_Email")?.GetComponent<TMP_InputField>();
        // password = GameObject.Find("Input_PW")?.GetComponent<TMP_InputField>();
        // outputText = GameObject.Find("Info_Text")?.GetComponent<TMP_Text>();

        createButton = GameObject.Find("SignUp_Button")?.GetComponent<Button>();
        loginButton = GameObject.Find("Login_Button")?.GetComponent<Button>();
        logoutButton = GameObject.Find("Logout_Button")?.GetComponent<Button>();
        guestButton = GameObject.Find("Guest_Button")?.GetComponent<Button>();
        googleButton = GameObject.Find("GoogleLogin_Button")?.GetComponent<Button>();
        testLoginButton = GameObject.Find("TestLogin_Button")?.GetComponent<Button>(); //TODO: 테스트용 나중에 삭제

        createButton.onClick.AddListener(Create);
        loginButton.onClick.AddListener(Login);
        logoutButton.onClick.AddListener(LogOut);
        guestButton.onClick.AddListener(AnonymousLogin);
        googleButton.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Google 로그인 버튼 클릭됨");
            FirebaseAuthManager.Instance.LoginWithGoogle();
        });
        testLoginButton.onClick.AddListener(TestLogin);

        loginPanel = GameObject.Find("Login_Panel");

        
    }

    private void OnChangedState(bool sign)
    {
        outputText.text = sign ? "Login : " : "Logout : ";
        outputText.text += FirebaseAuthManager.Instance.UserId;
    }

    public void Create()
    {
        string e = email.text;
        string p = password.text;

        FirebaseAuthManager.Instance.Create(e, p);
    }

    public void Login()
    {
        FirebaseAuthManager.Instance.Login(email.text, password.text);
    }

    public void LogOut()
    {
        FirebaseAuthManager.Instance.Logout();
    }

    public void AnonymousLogin() //게스트 로그인
    {
        FirebaseAuthManager.Instance.AnonymousLogin();
    }

    public void GoogleLogin()
    {
        FirebaseAuthManager.Instance.LoginWithGoogle();
    }

    public void TestLogin() //테스트 로그인 나중에 삭제
    {
        FirebaseAuthManager.Instance.LoginTestAccount();
    }

    void OnDestroy()
    {
        if (FirebaseAuthManager.Instance != null)
        {
            FirebaseAuthManager.Instance.LoginState -= OnChangedState;
        }
    }

}