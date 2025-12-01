using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
#endif

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


    public async void Init(bool isfirst = false)
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

        await FirebaseApp.CheckAndFixDependenciesAsync();

        if (auth.CurrentUser != null)
        {
            Debug.Log($"자동 로그인 유지됨: UID = {auth.CurrentUser.UserId}");
            if (isfirst) UIManager.Instance.Open(PanelId.StartPanel);
            else UIManager.Instance.Open(PanelId.MainPanel);

            await DataManager.Instance.EnsureUserDocAsync(auth.CurrentUser.UserId, isAutoLogin: true);
        }
        else
        {
            Debug.Log("로그인 필요 (게스트 또는 계정 로그인)");
            UIManager.Instance.Open(PanelId.LoginPanel);
        }

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
            UIManager.Instance.Open(PanelId.StartPanel);
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
        UIManager.Instance.Open(PanelId.LoginPanel);
    }

    private bool _gpgsActivated = false;

    private void EnsureGpgsActivated()
    {
#if UNITY_ANDROID
        if (_gpgsActivated) return;

        // 필요하면 나중에 config 도 넣을 수 있음
        Debug.Log("[GPGS] 초기화");
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        _gpgsActivated = true;
#endif
    }

    public void GooglePlayLoginFirst()
    {
#if UNITY_ANDROID
        EnsureGpgsActivated();

        PlayGamesPlatform.Instance.ManuallyAuthenticate(async status =>
        {
            if (status != SignInStatus.Success)
            {
                Debug.LogError($"[GPGS] 로그인 실패 또는 취소: {status}");
                return;
            }

            PlayGamesPlatform.Instance.RequestServerSideAccess(true, async authCode =>
            {
                if (string.IsNullOrEmpty(authCode))
                {
                    Debug.LogError("[GPGS] authCode 비어있음");
                    return;
                }

                var cred = PlayGamesAuthProvider.GetCredential(authCode);

                try
                {
                    var gUser = await auth.SignInWithCredentialAsync(cred);

                    await DataManager.Instance.EnsureUserDocAsync(
                        gUser.UserId,
                        gUser.Email ?? "gpgs",
                        authProviderType: AuthProviderType.GooglePlay
                    );

                    LoginState?.Invoke(true);
                    UIManager.Instance.Open(PanelId.StartPanel);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[GPGS] Firebase 로그인 중 오류: " + e);
                }
            });
        });
#endif
    }

    public void ConnectGpgsAccount()
    {
#if UNITY_ANDROID
        // 1. 현재 Firebase 유저 확인 (게스트인지)
        if (auth == null || auth.CurrentUser == null)
        {
            Debug.LogError("[GPGS] Firebase 유저가 없음. 먼저 게스트 로그인부터 해야 함");
            return;
        }

        if (!auth.CurrentUser.IsAnonymous)
        {
            Debug.LogWarning("[GPGS] 이미 비게스트 계정임. 연동 대상이 아님");
            // UI로 "이미 계정 연동됨" 같은 토스트 띄워도 됨
            return;
        }

        // 2. GPGS 준비
        EnsureGpgsActivated();

        // (선택) 혹시 이전에 이상하게 캐싱된 로그인 상태 있으면 한번 SignOut
        //PlayGamesPlatform.Instance.SignOut();

        // 3. ManuallyAuthenticate 호출 → 이 타이밍에 팝업이 떠야 함
        PlayGamesPlatform.Instance.ManuallyAuthenticate(async status =>
        {
            Debug.Log($"[GPGS] ManuallyAuthenticate status: {status}");

            if (status != SignInStatus.Success)
            {
                Debug.LogError($"[GPGS] 로그인 실패 또는 취소: {status}");
                // Canceled 같은 경우: 유저가 창 닫았거나, 예전에 '다시 묻지 않기' 했을 가능성
                return;
            }

            Debug.Log("[GPGS] 로그인 성공");

            PlayGamesPlatform.Instance.RequestServerSideAccess(true, async authCode =>
            {
                if (string.IsNullOrEmpty(authCode))
                {
                    Debug.LogError("[GPGS] authCode 비어있음");
                    return;
                }

                Debug.Log("[GPGS] authCode: " + authCode);
                var cred = PlayGamesAuthProvider.GetCredential(authCode);

                try
                {
                    // 🔹 지금 유저는 게스트(익명)이므로, 이 계정에 GPGS를 '연동'하는 것이 포인트
                    await auth.CurrentUser.LinkWithCredentialAsync(cred);
                    Debug.Log("[GPGS] 게스트 계정에 GPGS 연동 완료");

                    // 이후 유저 데이터 보장
                    await DataManager.Instance.EnsureUserDocAsync(auth.CurrentUser.UserId, isAutoLogin: true);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[GPGS] Firebase 연동 중 오류: " + e);
                }
            });
        });
#else
        Debug.LogWarning("Google Play Games 연동은 안드로이드에서만 지원됩니다.");
#endif
    }
    
    public async Task DeleteAccountAsync()
    {
        var user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("[AccountDelete] 현재 로그인된 유저가 없습니다.");
            return;
        }

        string uid = user.UserId;
        Debug.Log($"[AccountDelete] 삭제 대상 UID: {uid}");

        // Firebase Auth 계정 삭제
        try
        {
            await user.DeleteAsync();
            Debug.Log("[AccountDelete] Firebase Auth 계정 삭제 완료.");
        }
        catch (FirebaseException fe)
        {
            var authError = (AuthError)fe.ErrorCode;
            Debug.LogError($"[AccountDelete] 계정 삭제 실패: {authError} / {fe.Message}");

            if (authError == AuthError.RequiresRecentLogin)
            {
                // ★ 중요: 최근 로그인 필요
                // - 게스트: 다시 게스트 로그인 후 삭제 시도
                // - GPGS 연동 계정: GPGS 다시 로그인 → credential로 재인증 → Delete 다시 수행
                Debug.LogError("[AccountDelete] 최근 로그인 필요. 다시 로그인 후 계정 삭제를 재시도하세요.");
            }

            return;
        }

        // Firestore / 기타 유저 데이터 삭제
        try
        {
            // 예시: user 컬렉션 사용 중일 때
            await DataManager.Instance.DeleteUserDataAsync();
            await user.DeleteAsync();
            Debug.Log("[AccountDelete] Firestore user doc 삭제 완료.");
            // 랭크 데이터 삭제 필요
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AccountDelete] Firestore 삭제 중 오류 (무시 가능): {e}");
        }
        
        // 로컬 데이터 정리
        try
        {
            auth.SignOut();
            Debug.Log("[AccountDelete] 로컬 데이터 초기화 및 로그아웃 완료.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AccountDelete] 로컬 정리 중 오류: {e}");
        }
    }

}
