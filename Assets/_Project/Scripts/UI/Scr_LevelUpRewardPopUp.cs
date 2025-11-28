using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_LevelUpRewardPopUp : MonoBehaviour
{
    [Header("버튼들")]
    [SerializeField] private Button getBtn;
    [SerializeField] private Button adsBtn;

    private void Awake()
    {
        getBtn.onClick.AddListener(OnClickGetBtn);
    }
    private void OnClickGetBtn()
    {
        GiveGiftBox();
        //GiveGiftBox();
        UIManager.Instance.Close(PanelId.LevelUpRewardPopUp);
    }

    private void OnclickAdsBtn() 
    {
        //광고 봤으면 묻고 더블로 가야하는 로직
    }

    //지금은 그냥 바로 기프트박스 2개 나오도록 되어있는데 컨플루언스대로 레벨마다 다른 박스 얻도록 해야함
    private void GiveGiftBox()
    {
        // GiftBox Level 1 데이터 가져오기
        var giftData = DataManager.Instance.GetGiftBoxData(1);
        if (giftData == null)
        {
            return;
        }
        // 빈 칸 찾기
        var cell = BoardManager.Instance.FindEmptyActiveCell();
        if (cell == null)
        {
            // 빈 칸이 없으면 임시보관칸에 추가
            BoardManager.Instance.tempStorageSlot.Add(giftData);
            return;
        }
        // 빈 칸이 있으면 보드에 생성
        BoardManager.Instance.SpawnFromTempStorage(giftData);
    }

}
