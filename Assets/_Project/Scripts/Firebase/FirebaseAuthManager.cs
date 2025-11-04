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

    // 변경에 이렇게 기능이 Firebase Auth에 존재
    // 이메일 변경
    //await FirebaseAuth.DefaultInstance.CurrentUser.UpdateEmailAsync(newEmail);

    // 비밀번호 변경
    //await FirebaseAuth.DefaultInstance.CurrentUser.UpdatePasswordAsync(newPassword);
    
    void Awake()
    {
        Instance.Init();
    }
    
    public async void Init()
    {
        // FirebaseInitializer가 완료될 때까지 기다립니다.
        while (!FirebaseInitializer.IsInitialized)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnChanged;

        // 초기 상태 강제 확인
        OnChanged(this, null);
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

    // 회원가입
    public async void Create(string email, string password)
    {
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;
            Debug.Log($"회원가입 완료: {user.Email}");

            LoginState?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"회원가입 실패: {e.Message}");
            Debug.LogException(e);
        }
    }

    // 로그인
    public async void Login(string email, string password)
    {
        Debug.Log($"email: {email}, password: {password}");
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;
            Debug.Log($"로그인 완료: {user.Email}");
            
            await DataManager.Instance.EnsureUserDocAsync(user.UserId, user.Email);

            LoginState?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"로그인 실패: {e.Message}");
            Debug.LogException(e);
        }
    }

    // 게스트 로그인
    public async void AnonymousLogin()
    {
        try
        {
            var result = await auth.SignInAnonymouslyAsync();
            user = result.User;
            Debug.Log($"익명 로그인 성공! User ID: {user.UserId}");

            await DataManager.Instance.EnsureUserDocAsync(user.UserId, user.Email);

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
