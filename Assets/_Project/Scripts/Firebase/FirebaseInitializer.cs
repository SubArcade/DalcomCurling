using System.Threading.Tasks;
using Firebase;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    // Firebase를 초기화를 진행한 후 해당 여부를 IsInitialized 변수에 저장해 이 객체를 통해 초기화 여부를 확인 ( 초기화 중복 실행 할 경우 Auth 오류발생할 가능성 있음 ) 
    // 인증된 유저의 ID를 저장해 유저의 ID가 필요한경우 FirebaseAuthManager.Instance.UserId 로 호출

    public static FirebaseInitializer Instance { get; private set; }
    public static bool IsInitialized { get; private set; }

    void Awake()
    {
        //Debug.Log("FirebaseInitializer Awake() 호출됨");
        if (Instance == null)
        {            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        //Debug.Log("FirebaseInitializer Start() 호출됨, 초기화 시작...");
        await InitializeFirebase(); // Firebase 초기화 비동기 실행
    }

    private async Task InitializeFirebase()
    {
        // Firebase 의존성 확인 및 초기화
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync(); // Firebase SDK가 내부적으로 필요한 모듈(Google Play 서비스 등)을 사용할 수 있는지 확인.
        if (dependencyStatus == DependencyStatus.Available)
        {            
            //Debug.Log("Firebase가 성공적으로 초기화되었습니다.");
            IsInitialized = true;
        }
        else
        {
            //Debug.LogError($"Firebase 초기화에 실패했습니다: {dependencyStatus}");
            IsInitialized = false;
        }
    }
}
