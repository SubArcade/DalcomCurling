using Firebase.Auth;
using UnityEngine;
using System;
using Firebase;
using Google; 

public class FirebaseAuthManager
{
    // 유저 인증(로그인/로그아웃) 담당, 누가 이 앱을 사용 중인지
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
        
        // await FirebaseApp.CheckAndFixDependenciesAsync();
        //
        // if (auth.CurrentUser != null)
        // {
        //     Debug.Log($"자동 로그인 유지됨: UID = {auth.CurrentUser.UserId}");
        //     UIManager.Instance.Open(PanelId.StartPanel);
        // }
        // else
        // {
        //     Debug.Log("로그인 필요 (게스트 또는 계정 로그인)");
        //     UIManager.Instance.Open(PanelId.LoginPanel);
        // }
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

            await DataManager.Instance.EnsureUserDocAsync(user.UserId, user.Email ?? "guest");
            
            LoginState?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"익명 로그인 실패: {e.Message}");
            Debug.LogException(e);
        }
    }

    public async void LoginWithGoogle()
    {
        try
        {
            GoogleSignInConfiguration config = new GoogleSignInConfiguration
            {
                WebClientId = "941793478423-sq6ikguem1f8q62vduokq6gi0jidu1uo.apps.googleusercontent.com",
                RequestIdToken = true
            };

            GoogleSignIn.Configuration = config;
            GoogleSignIn.DefaultInstance.SignOut();

            Debug.Log("[Google] 로그인 시도");
            var googleUser = await GoogleSignIn.DefaultInstance.SignIn();

            if (googleUser == null)
            {
                Debug.LogWarning("[Google] 로그인 취소됨");
                return;
            }

            Debug.Log($"[Google] 로그인 성공: {googleUser.Email}");

            var credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

            user = await auth.SignInWithCredentialAsync(credential);
            Debug.Log($"[Firebase] 구글 로그인 완료: {user.Email} / {user.UserId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Google/Firebase] 로그인 실패: {e.Message}");
        }
    }

    public void Logout()
    {
        auth.SignOut();
        Debug.Log("로그아웃");
        LoginState?.Invoke(false);
    }
}
