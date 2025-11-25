using Firebase.Auth;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Firebase;
using Google;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

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
        
        await FirebaseApp.CheckAndFixDependenciesAsync();
        
        // GPGS 초기화
        // PlayGamesPlatform.DebugLogEnabled = true;
        // PlayGamesPlatform.Activate();
        
        if (auth.CurrentUser != null)
        {
            Debug.Log($"자동 로그인 유지됨: UID = {auth.CurrentUser.UserId}");
            UIManager.Instance.Open(PanelId.StartPanel);
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
        if (_gpgsActivated) return;
    
        // 필요하면 나중에 config 도 넣을 수 있음
        Debug.Log("[GPGS] 초기화");
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    
        _gpgsActivated = true;
    }
    
    public void ConnectGpgsAccount()
    {
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
    }

}
