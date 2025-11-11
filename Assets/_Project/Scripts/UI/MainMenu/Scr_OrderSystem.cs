using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_OrderSystem : MonoBehaviour
{
    [Header("메인주문서")]
    [SerializeField] private GameObject MainOrderGroup;
    [SerializeField] private Button MainOrderClearBtn;

    [Header("서브주문서1")]
    [SerializeField] private GameObject SubOrderGroup1;
    [SerializeField] private Button SubOrderClearBtn1;

    [Header("서브주문서2")]
    [SerializeField] private GameObject SubOrderGroup2;
    [SerializeField] private Button SubOrderClearBtn2;

    [Header("서브주문서1 새로고침 버튼")]
    [SerializeField] private Button SubOrderRefresh1;

    [Header("서브주문서2 새로고침 버튼")]
    [SerializeField] private Button SubOrderRefresh2;

    [Header("갱신된 새로고침 횟수 텍스트")]
    [SerializeField] private TextMeshProUGUI RefreshCountText;

    [Header("주문서에 나타날 무작위 도넛들")]
    [SerializeField] private List<GameObject> DonutList;//자동로딩

    [Header("주문서 클리어 시 나타낼 Complete!")]
    [SerializeField] private GameObject CompeleteObject;


    void Awake()
    {
       
    }
    void Start()
    {
        //주문서 아래에 자식으로 도넛프리팹의 스프라이트만 가져오기.
        //도넛프리팹의 정보에 따라 종류와 단계 텍스트가 갱신되도록 하기.
        //기존의 스프라이트 텍스트 삭제
        //주문서 완료 시 버튼 비활성화
    }
}
