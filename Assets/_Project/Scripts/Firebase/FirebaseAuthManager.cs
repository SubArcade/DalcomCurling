using Firebase.Auth;
using UnityEngine;
using System;
using Google; 

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

    public async void LoginTestAccount()
    {
        string testEmail = "test@example.com";
        string testPassword = "123456";

        try
        {
            // 로그인 시도
            var result = await auth.SignInWithEmailAndPasswordAsync(testEmail, testPassword);
            user = result.User;
            Debug.Log($"[Auth] 테스트 계정 로그인 성공: {user.Email}");
            LoginState?.Invoke(true);
        }
        catch (Exception e)
        {
            // 로그인 실패 시 (계정이 없으면 자동 생성)
            if (e.Message.Contains("no user record"))
            {
                Debug.Log("[Auth] 테스트 계정이 없어서 새로 생성합니다...");

                try
                {
                    var createResult = await auth.CreateUserWithEmailAndPasswordAsync(testEmail, testPassword);
                    user = createResult.User;
                    Debug.Log($"[Auth] 테스트 계정 생성 완료: {user.Email}");
                    LoginState?.Invoke(true);
                }
                catch (Exception createEx)
                {
                    Debug.LogError($"[Auth] 테스트 계정 생성 실패: {createEx.Message}");
                    Debug.LogException(createEx);
                }
            }
            else
            {
                Debug.LogError($"[Auth] 테스트 로그인 실패: {e.Message}");
                Debug.LogException(e);
            }
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
