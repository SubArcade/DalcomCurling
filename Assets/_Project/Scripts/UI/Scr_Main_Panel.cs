using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_Main_Panel : MonoBehaviour
{
    [Header("도감 팝업창 연결")]
    [SerializeField] private GameObject DonutCodex;

    [Header("도감 업그레이드 팝업창 연결")]
    [SerializeField] private GameObject DonutUpgrade;

    [Header("플레이어 레벨 팝업창 연결")]
    [SerializeField] private GameObject PlayerLevelInfo;

    [Header("기프트박스 팝업창 연결")]
    [SerializeField] private GameObject GiftBoxPopUp;
    // 11.03 작성날 기준 메인패널에 기프트박스 팝업을 띄울 버튼이나 이미지가 없음

    [Header("각각 해당하는 팝업창 열기")]
    [SerializeField] private Button EntryButton; //미완성  11.03
    [SerializeField] private Button CodexButton;
    [SerializeField] private Button basketButton; //미완성 11.03
    [SerializeField] private Button UpgradeButton;
    [SerializeField] private Button LevelButton;

    //메인 패널창에 아직 연결 및 생성되지않은 것
    // 1. 휴지통
    // 2. 엔트리
    // 3. 환경설정
    // 4. 컬링시작
    // 5. 주문서
    // 6. 머지(합성)칸
    // 그외에는 더 있을 수 있으니 확인 부탁드립니다.

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f); // 혹시라도 로딩 지연 대비

        Transform canvas = GameObject.FindGameObjectWithTag("MainCanvas")?.transform;
        DonutCodex = canvas.Find("DonutCodex")?.gameObject;       
        DonutUpgrade = canvas.Find("DonutUpgrade")?.gameObject;
        PlayerLevelInfo = canvas.Find("PlayerLevelInfo")?.gameObject;
        GiftBoxPopUp = canvas.Find("GiftBoxPopUp")?.gameObject;

        EntryButton = transform.Find("Bottom/ButtonGroup/Entry_Button")?.GetComponent<Button>();
        //엔트리버튼 연결만 해놓고 팝업창은 따로 만들어야함
        CodexButton = transform.Find("Bottom/ButtonGroup/Codex_Button")?.GetComponent<Button>();       
        basketButton = transform.Find("Bottom/ButtonGroup/basket_Button")?.GetComponent<Button>();
        //휴지통버튼 연결만 해놓고 팝업차은 따로 만들어야함
        UpgradeButton = transform.Find("Bottom/ButtonGroup/Upgrade_Button")?.GetComponent<Button>();
        LevelButton = transform.Find("Top/Level")?.GetComponent<Button>();

        //EntryButton.onClick.AddListener(() => EntryPopUp.SetActive(true));
        CodexButton.onClick.AddListener(()=> DonutCodex.SetActive(true));
        //basketButton.onClick.AddListener(() => BasketPopUp.SetActive(true));
        UpgradeButton.onClick.AddListener(()=> DonutUpgrade.SetActive(true));
        LevelButton.onClick.AddListener(() => PlayerLevelInfo.SetActive(true));
    }

}
