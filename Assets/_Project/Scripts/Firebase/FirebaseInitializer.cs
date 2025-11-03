using Firebase;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance { get; private set; }
    public static bool IsInitialized { get; private set; }

    void Awake()
    {
        Debug.Log("FirebaseInitializer Awake() 호출됨");
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
        Debug.Log("FirebaseInitializer Start() 호출됨, 초기화 시작...");
        await InitializeFirebase(); // Firebase 초기화 비동기 실행
    }

    private async Task InitializeFirebase()
    {
        // Firebase 의존성 확인 및 초기화
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {            
            Debug.Log("Firebase가 성공적으로 초기화되었습니다.");
            IsInitialized = true;
        }
        else
        {
            Debug.LogError($"Firebase 초기화에 실패했습니다: {dependencyStatus}");
            IsInitialized = false;
        }
    }
}
