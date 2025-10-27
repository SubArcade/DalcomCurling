using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginSystem : MonoBehaviour
{
    public TMP_InputField email;
    public TMP_InputField password;

    public TMP_Text outputText;

    void Start()
    {
        FirebaseAuthManager.Instance.LoginState += OnChangedState;
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
        FirebaseAuthManager.Instance.Create(email.text, password.text);
    }

    public void LogOut()
    {
        FirebaseAuthManager.Instance.Logout();
    }

}
