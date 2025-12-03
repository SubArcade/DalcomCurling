using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_ResultControl : MonoBehaviour
{
    [Header("제어 객체")]
    [SerializeField] private GameObject secondPanel; // 결과창 두번째 판넬
    [SerializeField] private Button next_Button; // 두번째 파넬 여는 버튼
    [SerializeField] private Button confirm_Button; // 결과창 확인버튼

    [Header("가져올 도넛 선택창")]
    [SerializeField] private List<Button> touchDonutList; // 도넛 선택 오브젝트 (이 버튼들에 스프라이트 표시)

    [Header("Second Panel 도넛 이미지")]
    [SerializeField] private Image donut_1;
    [SerializeField] private Image donut_2;

    [Header("플레이어의 캐릭터가 적용될 결과창 이미지")]
    [SerializeField] private Image characterImg;

    [Header("레벨판넬 witePanel1")]
    [SerializeField] private GameObject witePanel1;
    [SerializeField] private TextMeshProUGUI levelText; //플레이어 레벨 텍스트
    [SerializeField] private TextMeshProUGUI expUpText; //경험치가 오르는 양을 나타낼 텍스트
    [SerializeField] private TextMeshProUGUI levelUpText; //레벨업시 나타날 텍스트
    [SerializeField] private Image expGage; //경험치 게이지바
    [SerializeField] private TextMeshProUGUI expText; //경험치게이지바의 경험치 텍스트

    [Header("랭킹판넬 witePanel2")]
    [SerializeField] private GameObject witePanel2;
    [SerializeField] private Image tierImage; //플레이어의 티어가 들어갈 이미지
    [SerializeField] private TextMeshProUGUI rankUpText; //랭크업을 나타낼 텍스트
    [SerializeField] private TextMeshProUGUI pointUpText; //포인트 오르는양을 나타낼 텍스트
    [SerializeField] private Image pointGage; //포인트 게이지바
    [SerializeField] private TextMeshProUGUI pointText; //게지이바의 포인트 텍스트

    [SerializeField] private Scr_TierSpriteSO tierSpriteSO; //티어 이미지 연결용
    [SerializeField] private CharacterSO characterSO; //캐릭터 이미지 연결용

    private Sprite capturedDonutSprite1;
    private Sprite capturedDonutSprite2;
    private int selectionCount = 0;

    void OnEnable()
    {
        // 결과창이 활성화될 때마다 UI를 최신 게임 결과에 맞게 설정.
        SetupUIForGameOutcome();

        // 결과창이 뜨면 20초 후에 자동 종료
        //DOVirtual.DelayedCall(20f, () =>
        //{
        //    if (SceneLoader.Instance != null)
        //    {
        //        GameManager.Instance.EndGame();
        //    }
        //    else
        //    {
        //        Debug.LogError("SceneLoader.Instance를 찾을 수 없습니다.");
        //    }
        //}).SetId("endgame");
    }

   

    void Start()
    {
        next_Button.onClick.AddListener(OpenNextPanel);
        confirm_Button.onClick.AddListener(ExitGame);
    }

    private void SetupUIForGameOutcome()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        var gameOutcome = GameManager.Instance.LastGameOutcome;
        int exp = GameManager.Instance.pendingExp;
        int point = GameManager.Instance.pendingPoint;

        switch (gameOutcome)
        {
            case FirebaseGameManager.GameOutcome.Win:
                SetupForWin();
                SetwitePanel1(exp);
                SetwitePanel2(point);
                SetCharacterImage();
                break;
            case FirebaseGameManager.GameOutcome.Lose:
                SetupForLose();
                SetwitePanel1(exp);
                SetwitePanel2(point);
                SetCharacterImage();
                break;
            case FirebaseGameManager.GameOutcome.Draw:
            default:
                SetupForDraw();
                SetwitePanel1(exp);
                SetwitePanel2(point);
                SetCharacterImage();
                break;
        }
    }

    private void SetupForWin()
    {
        Debug.Log("승리! 획득할 도넛을 선택하세요.");
        // 획득한 도넛 스프라이트를 미리 가져옴
        capturedDonutSprite1 = GetDonutSprite(GameManager.Instance.CapturedDonut1);
        capturedDonutSprite2 = GetDonutSprite(GameManager.Instance.CapturedDonut2);

        selectionCount = 0;

        foreach (var button in touchDonutList)
        {
            button.gameObject.SetActive(true); // 버튼 활성화
            button.interactable = true; // 상호작용 가능하게 설정
            button.onClick.RemoveAllListeners(); // OnEnable에서 중복 등록 방지를 위해 기존 리스너 제거
            button.onClick.AddListener(() => OnDonutSelectionClicked(button));
        }

        donut_1.sprite = capturedDonutSprite1;
        donut_2.sprite = capturedDonutSprite2;
    }

    private void SetupForLose()
    {
        Debug.Log("패배. 잃은 도넛을 표시합니다.");

        // 모든 버튼의 리스너를 제거하고 상호작용 비활성화
        foreach (var button in touchDonutList)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = false;
        }

        capturedDonutSprite1 = GetDonutSprite(GameManager.Instance.PlayerPenalizedDonut1);
        capturedDonutSprite2 = GetDonutSprite(GameManager.Instance.PlayerPenalizedDonut2);

        // 잃은 도넛 두 개만 표시
        if (touchDonutList.Count >= 2)
        {
            touchDonutList[0].gameObject.SetActive(true);
            SetButtonSprite(touchDonutList[0], capturedDonutSprite1);

            touchDonutList[1].gameObject.SetActive(true);
            SetButtonSprite(touchDonutList[1], capturedDonutSprite2);
        }

        // 나머지 버튼들은 비활성화
        for (int i = 2; i < touchDonutList.Count; i++)
        {
            touchDonutList[i].gameObject.SetActive(false);
        }

        donut_1.sprite = capturedDonutSprite1;
        donut_2.sprite = capturedDonutSprite2;
    }

    private void SetupForDraw()
    {
        Debug.Log("무승부. 도넛 변동 없음.");
        // 무승부일 경우 모든 도넛 관련 UI를 비활성화합니다.
        foreach (var button in touchDonutList)
        {
            button.gameObject.SetActive(false);
        }

        donut_1.gameObject.SetActive(false);
        donut_2.gameObject.SetActive(false);
    }
    private void OnDonutSelectionClicked(Button clickedButton)
    {
        if (selectionCount >= 2)
        {
            Debug.Log("이미 2개의 도넛을 모두 획득했습니다.");
            return;
        }

        Image buttonImage = clickedButton.GetComponent<Image>();
        if (buttonImage == null) return;

        if (selectionCount == 0)
        {
            buttonImage.sprite = capturedDonutSprite1;
            Debug.Log("첫 번째 획득 도넛을 선택했습니다.");
        }
        else if (selectionCount == 1)
        {
            buttonImage.sprite = capturedDonutSprite2;
            Debug.Log("두 번째 획득 도넛을 선택했습니다.");
        }

        clickedButton.interactable = false; // 한 번 선택한 버튼은 다시 선택 불가
        selectionCount++;

        if (selectionCount >= 2)
        {
            // 모든 선택이 끝나면 나머지 버튼들도 비활성화
            foreach (var button in touchDonutList)
            {
                if (button.interactable)
                {
                    button.interactable = false;
                }
            }
        }
    }


    /// <summary>
    ﻿    /// 버튼의 Image 컴포넌트에 도넛 스프라이트를 설정
    ﻿    /// </summary>
    private void SetButtonSprite(Button button, Sprite sprite)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = sprite;
            buttonImage.enabled = (sprite != null); // 스프라이트가 있을 때만 보이도록 설정
        }
    }

    private void OpenNextPanel() // 버튼동작
    {
        secondPanel.SetActive(true);
    }
    private void ExitGame() // 버튼동작
    {
        DOVirtual.DelayedCall(0.5f, () =>
        {
            if (SceneLoader.Instance != null)
            {
                //SceneLoader.Instance.LoadLocal(GameManager.Instance.menuSceneName);
                GameManager.Instance.EndGame();
            }
            else
            {
                Debug.LogError("SceneLoader.Instance를 찾을 수 없습니다.");
            }
        }).SetId("endgame");
    }

    /// <summary>
    ﻿    /// DonutEntry로부터 스프라이트를 가져옴.
    ﻿    /// </summary>
    private Sprite GetDonutSprite(DonutEntry donutEntry)
    {
        DonutData donutData = DataManager.Instance.GetDonutByID(donutEntry.id);

        if (donutData == null)
        {
            Debug.LogWarning($"ID '{donutEntry.id}'에 해당하는 DonutData를 찾을 수 없습니다.");
            return null;
        }
        if (donutData.sprite == null)
        {
            Debug.LogWarning($"ID '{donutEntry.id}'의 DonutData에 스프라이트가 할당되지 않았습니다.");
            return null;
        }

        return donutData.sprite;
    }

    private void SetwitePanel1(int getExp) 
    {
        var player = DataManager.Instance.PlayerData;
        int maxExp = 100;

        //얻은 경험치와 레벨업여부판단변수
        int newExp = player.exp + getExp;
        bool isLevelUp = false;

        if (newExp > maxExp) 
        {
            player.level++;
            DataManager.Instance.LevelChange(player.level++);
            newExp -= maxExp;
            isLevelUp = true;
        }
        player.exp = newExp;
        DataManager.Instance.ExpChange(player.exp);

        //UI반영
        levelText.text = $"{player.level}";
        expUpText.text = $"+{getExp}EXP";
        expText.text = $"{newExp}/{maxExp} EXP";
        expGage.fillAmount = (float)newExp / maxExp;

        levelUpText.gameObject.SetActive(isLevelUp);
        if (isLevelUp)
        {
            levelUpText.text = "LEVEL UP!!";
        }
        else 
        {
            levelUpText.text = ""; 
        }
        //플레이어레벨 텍스트표시
        // "{결과로 받아온 경험치}+{기존경험치} / {maxEXP}EXP";와 게이지 연결
        // +{결과로 받아온 경험치}exp 텍스트 반영
        //레벨업을 했다면 레벨업 텍스트를 띄우고 아니라면 setactive(false)
    }

    private void SetwitePanel2(int getPoint)
    {
        var player = DataManager.Instance.PlayerData;
        GameTier oldTier = player.soloTier;
        int oldScore = player.soloScore;

        //점수갱신
        int newScore = oldScore + getPoint;

        //티어 계산해서 저장
        GameTier newTier = DataManager.Instance.CalculateTier(newScore);


        //티어별로 포인트 설정
        int maxPoint = 0;
        switch (newTier)
        {
            case GameTier.Bronze:
                {
                    maxPoint = 1000;
                    break;
                }
            case GameTier.Silver:
                {
                    maxPoint = 2000;
                    break;
                }
            case GameTier.Gold:
                {
                    maxPoint = 3000;
                    break;
                }
            case GameTier.Platinum:
                {
                    maxPoint = 4000;
                    break;
                }
            case GameTier.Diamond:
                {
                    maxPoint = 5000;
                    break;
                }
            default: maxPoint = 5000; break;
        }

        // UI 반영
        pointUpText.text = $"+{getPoint} P";
        pointText.text = $"{newScore}/{maxPoint} P";
        pointGage.fillAmount = (float)newScore / maxPoint;
        // 티어 이미지 반영
        tierImage.sprite = tierSpriteSO.GetSprite(newTier);

        // 랭크업 여부 체크
        if (newTier != oldTier)
        {
            rankUpText.gameObject.SetActive(true);
            rankUpText.text = "RANK UP!!";
        }
        else
        {
            rankUpText.gameObject.SetActive(false);
            rankUpText.text = "";
        }
        player.soloScore = newScore;
        player.soloTier = newTier;
        DataManager.Instance.ScoreChange(player.soloScore);
        DataManager.Instance.PlayerData.soloTier = player.soloTier;
    }

    private void SetCharacterImage()
    {
        var currentType = DataManager.Instance.InventoryData.curCharacterType;
        Sprite sprite = characterSO.GetCharacterSprite(currentType);

        if (sprite != null)
        {
            characterImg.sprite = sprite;
            characterImg.enabled = true;
        }
        else
        {
            characterImg.enabled = false;
        }
    }

    private void OnDestroy()
    {
        DOTween.Kill("endgame");
    }
}

