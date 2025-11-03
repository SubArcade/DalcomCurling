using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
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

    [SerializeField] private GameObject loginPanel;

    void Start()
    {
        FirebaseAuthManager.Instance.LoginState += OnChangedState;

        // email = GameObject.Find("Input_Email")?.GetComponent<TMP_InputField>();
        // password = GameObject.Find("Input_PW")?.GetComponent<TMP_InputField>();
        // outputText = GameObject.Find("Info_Text")?.GetComponent<TMP_Text>();

        // createButton = GameObject.Find("SignUp_Button")?.GetComponent<Button>();
        // loginButton = GameObject.Find("Login_Button")?.GetComponent<Button>();
        // logoutButton = GameObject.Find("Logout_Button")?.GetComponent<Button>();
        // guestButton = GameObject.Find("Guest_Button")?.GetComponent<Button>();

        createButton.onClick.AddListener(Create);
        loginButton.onClick.AddListener(Login);
        logoutButton.onClick.AddListener(LogOut);
        guestButton.onClick.AddListener(AnonymousLogin);

        loginPanel = GameObject.Find("Login_Panel");

        FirebaseAuthManager.Instance.Init();
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

    void OnDestroy()
    {
        if (FirebaseAuthManager.Instance != null)
        {
            FirebaseAuthManager.Instance.LoginState -= OnChangedState;
        }
    }

}