using Firebase.Auth;
using UnityEngine;
using System;

public class FirebaseAuthManager
{
    private static FirebaseAuthManager instance = null;
    public static FirebaseAuthManager Instance
    {
        get
        {
            if (instance == null)
                instance = new FirebaseAuthManager();
            return instance;
        }
    }

    private FirebaseAuth auth;
    private FirebaseUser user;

    public string UserId => user?.UserId;
    public Action<bool> LoginState;

    public void Init()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnChanged;
    }

    private void OnChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser != null)
        {
            bool signed = (auth.CurrentUser != user && auth.CurrentUser != null);
            if (!signed && user != null)
            {
                Debug.Log("로그아웃");
                LoginState?.Invoke(false);
            }

            user = auth.CurrentUser;
            if (signed)
            {
                Debug.Log("로그인");
                LoginState?.Invoke(true);
            }
        }
    }

    public async void Create(string email, string password)
    {
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;
            Debug.Log($"회원가입 완료: {user.Email}");

            var dataManager = GameObject.FindAnyObjectByType<DataManager>();
            dataManager?.RunFirestoreSamples();

            LoginState?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"회원가입 실패: {e.Message}");
            Debug.LogException(e);
        }
    }

    public async void Login(string email, string password)
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;
            Debug.Log($"로그인 완료: {user.Email}");

            var dataManager = GameObject.FindAnyObjectByType<DataManager>();
            dataManager?.RunFirestoreSamples();

            LoginState?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"로그인 실패: {e.Message}");
            Debug.LogException(e);
        }
    }

    public async void AnonymousLogin()
    {
        try
        {
            var result = await auth.SignInAnonymouslyAsync();
            user = result.User;
            Debug.Log($"익명 로그인 성공! User ID: {user.UserId}");

            var dataManager = GameObject.FindAnyObjectByType<DataManager>();
            dataManager?.RunFirestoreSamples();

            LoginState?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"익명 로그인 실패: {e.Message}");
            Debug.LogException(e);
        }
    }

    public void Logout()
    {
        auth.SignOut();
        Debug.Log("로그아웃");
        LoginState?.Invoke(false);
    }
}
