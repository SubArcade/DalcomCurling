
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Scr_donutShell : MonoBehaviour
{
    [Header("타입 설정 (인스펙터 기본값)")] 
    [SerializeField] private DonutDexViewState type;

    [Header("공통")]
    [SerializeField] private Button button;

    [Header("상태별 루트 오브젝트")]
    [SerializeField] private GameObject questionRoot; // 물음표 화면
    [SerializeField] private GameObject donutRoot;    // 도넛 화면
    [SerializeField] private GameObject rewardRoot;   // 보상 화면

    [Header("Donut 상태용")]
    [SerializeField] private Image donutImage;

    [Header("Reward 상태용")]
    [SerializeField] private TMP_Text rewardText;
    private int gem;
    
    public event System.Action<Scr_donutShell> OnDonutClicked;
    
    private void Awake()
    {
        button.onClick.AddListener(OnClick);
    }
    
    /// <summary>
    /// 타입을 코드에 따라서 변경
    /// </summary>
    public void SetType(DonutDexViewState newType, Sprite donut = null, int reward = 0)
    {
        type = newType;
        if (questionRoot != null)
            questionRoot.SetActive(type == DonutDexViewState.Question);
        if (donutRoot != null)
            donutRoot.SetActive(type == DonutDexViewState.Donut);
        if (rewardRoot != null)
            rewardRoot.SetActive(type == DonutDexViewState.Reward);

        switch (type)
        {
            case DonutDexViewState.Question:
                break;
            case DonutDexViewState.Donut:
                donutImage.sprite = donut;
                break;
            case DonutDexViewState.Reward:
                gem = reward;
                rewardText.text = $"젬 {reward}개 획";
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 버튼 눌렀을 때 타입에 따라 다른 이벤트 호출
    /// </summary>
    private void OnClick()
    {
        switch (type)
        {
            case DonutDexViewState.Question:
                break;
            case DonutDexViewState.Donut:
                OnDonutClicked.Invoke(this);
                break;
            case DonutDexViewState.Reward:
                DataManager.Instance.PlayerData.gem += gem;
                // 메인호면 UI 갱신함수 필요 이벤트 함수
                DataManager.Instance.GemChange(DataManager.Instance.PlayerData.gem);
                break;
            default:
                break;
        }
    }
}
