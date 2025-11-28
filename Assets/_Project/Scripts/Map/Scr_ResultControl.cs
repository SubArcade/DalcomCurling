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
    private Image loseDonut_a;
    private Image loseDonut_b;
    private Image getDonut_a;
    private Image getDonut_b;
    private int count = 0;

    //상대 도넛 . 내 읽은 도넛 이미지 받아오기.


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
                //btn.onClick.AddListener(() => OnObjectClicked(img));
            }
        }
    }
    void Update()
    {
        
    }

    private void OnObjectClicked(Image img)
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
    /// GameManager로부터 상대방 도넛 정보를 가져와 UI에 표시합니다.
    /// </summary>
    private void DisplayOpponentDonuts()
    {
        if (GameManager.Instance == null || DataManager.Instance == null)
        {
            Debug.LogError("GameManager 또는 DataManager 인스턴스를 찾을 수 없습니다.");
            opponentDonutPanel1?.SetActive(false);
            opponentDonutPanel2?.SetActive(false);
            return;
        }

        // 첫 번째 도넛 표시
        DonutEntry donut1 = GameManager.Instance.OpponentSelectedDonut1;
        if (opponentDonutPanel1 != null)
        {
            if (donut1 != null)
            {
                DonutData donutData1 = DataManager.Instance.GetDonutByID(donut1.id);
                if (donutData1 != null && opponentDonutImage1 != null)
                {
                    opponentDonutImage1.sprite = donutData1.sprite; 
                    opponentDonutPanel1.SetActive(true);
                }
                else
                {
                    opponentDonutPanel1.SetActive(false);
                }
            }
            else
            {
                opponentDonutPanel1.SetActive(false);
            }
        }

        // 두 번째 도넛 표시
        DonutEntry donut2 = GameManager.Instance.OpponentSelectedDonut2;
        if (opponentDonutPanel2 != null)
        {
            if (donut2 != null)
            {
                DonutData donutData2 = DataManager.Instance.GetDonutByID(donut2.id);
                if (donutData2 != null && opponentDonutImage2 != null)
                {
                    opponentDonutImage2.sprite = donutData2.sprite;
                    opponentDonutPanel2.SetActive(true);
                }
                else
                {
                    opponentDonutPanel2.SetActive(false);
                }
            }
            else
            {
                opponentDonutPanel2.SetActive(false);
            }
        }
    }
}
