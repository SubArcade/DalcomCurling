using Firebase.Auth;
using UnityEngine;
using System;
using Google;
using System.Threading.Tasks;

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

    // TODO: GoogleSign SDK설치 후 확인

    //public async void LoginWithGoogle()
    //{
    //    try
    //    {
    //        // Google 로그인 구성
    //        GoogleSignInConfiguration config = new GoogleSignInConfiguration
    //        {
    //            WebClientId = "YOUR_WEB_CLIENT_ID_HERE", // Firebase 콘솔에서 복사한 Web client ID
    //            RequestIdToken = true
    //        };

    //        // Google Sign-In 초기화
    //        GoogleSignIn.Configuration = config;
    //        GoogleSignIn.Configuration.UseGameSignIn = false;
    //        GoogleSignIn.DefaultInstance.SignOut(); // 이전 세션 초기화

    //        Debug.Log("[Google] 로그인 시도 중...");
    //        GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();

    //        if (googleUser == null)
    //        {
    //            Debug.LogWarning("[Google] 로그인 취소됨");
    //            return;
    //        }

    //        Debug.Log($"[Google] 로그인 성공: {googleUser.Email}");

    //        // Firebase 인증과 연동
    //        Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
    //        var authResult = await auth.SignInWithCredentialAsync(credential);
    //        user = authResult.User;

    //        Debug.Log($"[Firebase] 구글 로그인 완료: {user.Email} (UID={user.UserId})");

    //        //  Firestore 연동이 필요하면 주석 해제
    //        // var dataManager = GameObject.FindAnyObjectByType<DataManager>();
    //        // if (dataManager != null)
    //        //     await dataManager.SyncUserData(user, "(Google)");

    //        LoginState?.Invoke(true);
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError($"[Google/Firebase] 로그인 실패: {e.Message}");
    //        Debug.LogException(e);
    //    }
    //}


    public void Logout()
    {
        auth.SignOut();
        Debug.Log("로그아웃");
        LoginState?.Invoke(false);
    }
}
