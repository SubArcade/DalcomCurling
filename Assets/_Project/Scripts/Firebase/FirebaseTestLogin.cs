using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class FirebaseTestLogin : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button anonymousLoginButton;
    [SerializeField] private Button logoutButton;

    void Start()
    {
        // FirebaseAuthManager 초기화 및 상태 변경 이벤트 구독
        FirebaseAuthManager.Instance.LoginState += OnLoginStateChanged;
        FirebaseAuthManager.Instance.Init();

        // UI 버튼에 리스너 추가
        createAccountButton?.onClick.AddListener(CreateAccount);
        loginButton?.onClick.AddListener(Login);
        anonymousLoginButton?.onClick.AddListener(AnonymousLogin);
        logoutButton?.onClick.AddListener(Logout);

        // 초기 로그인 상태 표시
        UpdateStatusText();
    }

    void OnDestroy()
    {
        // 스크립트가 파괴될 때 이벤트 구독 해제
        if (FirebaseAuthManager.Instance != null)
        {
            FirebaseAuthManager.Instance.LoginState -= OnLoginStateChanged;
        }
    }

    private void OnLoginStateChanged(bool isLoggedIn)
    {
        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        string userId = FirebaseAuthManager.Instance.UserId;
        if (!string.IsNullOrEmpty(userId))
        {
            statusText.text = $"로그인됨: {userId}";
        }
        else
        {
            statusText.text = "로그아웃됨";
        }
    }

    public void CreateAccount()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "이메일과 비밀번호를 입력하세요.";
            return;
        }
        FirebaseAuthManager.Instance.Create(email, password);
    }

    public void Login()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "이메일과 비밀번호를 입력하세요.";
            return;
        }
        FirebaseAuthManager.Instance.Login(email, password);
    }

    public void AnonymousLogin()
    {
        FirebaseAuthManager.Instance.AnonymousLogin();
    }

    public void Logout()
    {
        FirebaseAuthManager.Instance.Logout();
    }
}
