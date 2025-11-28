using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_ResultControl : MonoBehaviour
{
    [SerializeField] private GameObject secondPanel; // 결과창 두번째 판넬
    [SerializeField] private Button next_Button; // 두번째 파넬 여는 버튼
    [SerializeField] private Button confirm_Button; // 결과창 확인버튼

    [Header("상대 도넛 표시")]
    [SerializeField] private Image opponentDonutImage1; // 바꿔줄 이미지1
    [SerializeField] private Image opponentDonutImage2; // 바꿔줄 이미지2
    [SerializeField] private GameObject opponentDonutPanel1; // 도넛이 없을 경우 비활성화할 패널1
    [SerializeField] private GameObject opponentDonutPanel2; // 도넛이 없을 경우 비활성화할 패널2

    [Header("가져올 도넛 선택창")]
    [SerializeField] private List<Button> touchDonutList; // 도넛 선택 오브젝트
    private List<Button> selectedObjects = new List<Button>();
    private int count = 0;

    [Header("내 잃은 도넛")]
    [SerializeField] private Image loseDonut_a;
    [SerializeField] private Image loseDonut_b;

    [Header("획득한 도넛")]
    [SerializeField] private Image getDonut_a;
    [SerializeField] private Image getDonut_b;

    void Start() 
    {
        count = 0; // 초기화

        next_Button.onClick.AddListener(OpenNextPanel); // 다음판넬 열기
        confirm_Button.onClick.AddListener(ExitGame); // 0.5초 후 나가기

        foreach (var Button in touchDonutList)
        {
            Button btn = Button.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnObjectClicked());
            }
        }
        DisplayGameResultDonuts(); // 게임 결과 도넛 정보 표시

    }
    void Update()
    {
        
    }

    private void OnObjectClicked()
    {
        if (count > 2) return;
        count++;


    }



    private void OpenNextPanel() // 버튼동작
    {
        secondPanel.SetActive(true);
    }
    private void ExitGame() // 버튼동작
    {
        DOVirtual.DelayedCall(0.5f, () =>
        {
            SceneLoader.Instance.LoadLocal(GameManager.Instance.menuSceneName); 
        });
    }

    /// <summary>
    /// GameManager로부터 게임 결과 도넛 정보를 가져와 UI에 표시합니다.
    /// (내 잃은 도넛, 획득한 도넛, 상대방 선택 도넛)
    /// </summary>
    private void DisplayGameResultDonuts()
    {
        if (GameManager.Instance == null || DataManager.Instance == null)
        {
            Debug.LogError("GameManager 또는 DataManager 인스턴스를 찾을 수 없습니다.");

            opponentDonutPanel1?.SetActive(false);
            opponentDonutPanel2?.SetActive(false);
            return;
        }

        // --- 내 잃은 도넛 표시 ---
        DisplayDonut(GameManager.Instance.PlayerPenalizedDonut1, loseDonut_a);
        DisplayDonut(GameManager.Instance.PlayerPenalizedDonut2, loseDonut_b);

        // --- 획득한 도넛 표시 (승리 시) ---
        DisplayDonut(GameManager.Instance.CapturedDonut1, getDonut_a);
        DisplayDonut(GameManager.Instance.CapturedDonut2, getDonut_b);
        
    }

    /// <summary>
    /// 단일 도넛 정보를 UI에 표시하는 메서드
    /// </summary>
    /// <param name="donutEntry">표시할 DonutEntry 정보</param>
    /// <param name="targetImage">스프라이트를 설정할 Image 컴포넌트</param>
    /// <param name="targetPanel">도넛이 없을 경우 비활성화할 GameObject 패널</param>
    /// <param name="debugName">디버그 메시지용 이름</param>
    private void DisplayDonut(DonutEntry donutEntry, Image targetImage)
    {
        if (donutEntry != null)
        {
            DonutData donutData = DataManager.Instance.GetDonutByID(donutEntry.id);
            if (donutData != null && targetImage != null)
            {
                targetImage.sprite = donutData.sprite; // DonutData의 'sprite' 필드 사용
                
            }
            else
            {
               
            }
        }
        
    }
    
}
