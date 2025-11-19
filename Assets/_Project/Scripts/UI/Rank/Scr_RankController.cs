using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Scr_RankController : MonoBehaviour
{
    [SerializeField, Tooltip("티어 이미지")] private List<Scr_RankInfo> rankInfoList = new List<Scr_RankInfo>();
    
    [SerializeField, Tooltip("티어별 스프라이트 SO")]
    private Scr_TierSpriteSO tierSpriteSO;

    [SerializeField, Tooltip("랭킹 모드")]
    private GameMode gameMode = GameMode.SOLO;

    // 버튼에서 직접 호출해도 되고, OnEnable에서 호출해도 됨
    // public async void RefreshRankUI()
    // {
    //     try
    //     {
    //         // Firestore에서 랭킹 가져오기 (점수 내림차순, rankInfoList 수만큼)
    //         var db   = DataManager.Instance.Db;  // 혹시 db가 public 아니면 DataManager에 랩핑함수 만들면 됨
    //         var col  = DataManager.Instance.LeadersCol(gameMode); // rank/season_X/solo/leaders 같은 컬렉션
    //         var query = col.OrderByDescending("score").Limit(rankInfoList.Count);
    //
    //         var snap = await query.GetSnapshotAsync();
    //
    //         int index = 0;
    //
    //         foreach (var doc in snap.Documents)
    //         {
    //             if (index >= rankInfoList.Count) break;
    //
    //             var data = doc.ToDictionary();
    //
    //             // nickname
    //             string nickname = data.TryGetValue("nickname", out var nickObj)
    //                 ? nickObj as string ?? "NoName"
    //                 : "NoName";
    //
    //             // score
    //             int score = 0;
    //             if (data.TryGetValue("score", out var scoreObj))
    //                 score = Convert.ToInt32(scoreObj);
    //
    //             // tier (string → GameTier)
    //             GameTier tier = GameTier.Bronze;
    //             if (data.TryGetValue("tier", out var tierObj))
    //             {
    //                 string tierStr = tierObj as string ?? "bronze";
    //                 // 대소문자 구분 없이 enum 변환
    //                 if (!Enum.TryParse<GameTier>(tierStr, true, out tier))
    //                 {
    //                     tier = GameTier.Bronze;
    //                 }
    //             }
    //
    //             // 티어 스프라이트 가져오기
    //             Sprite tierSprite = tierSpriteSO != null
    //                 ? tierSpriteSO.GetSprite(tier)
    //                 : null;
    //
    //             // 타이틀: 1위 / 2위 / 3위 ... 이런 식으로
    //             int rank = index + 1;
    //             string title = $"{rank}위";
    //
    //             // UI 한 줄 세팅
    //             rankInfoList[index].SetInfo(
    //                 tier: tier,
    //                 sprite: tierSprite,
    //                 title: title,
    //                 nickname: nickname,
    //                 score: score.ToString()
    //             );
    //             rankInfoList[index].gameObject.SetActive(true);
    //
    //             index++;
    //         }
    //
    //         // 받아온 랭킹 수보다 슬롯이 많으면 나머지 비활성화
    //         for (; index < rankInfoList.Count; index++)
    //         {
    //             rankInfoList[index].gameObject.SetActive(false);
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError($"[Scr_RankController] 랭킹 로딩 실패: {e}");
    //     }
    // }
    
    
    
    
    
    
    // 에디터 함수
    [SerializeField] private Transform rankParent;
    [Header("에디터용 리스트 채우는 함수")]
    [SerializeField] private bool fillRanksInEditor = false;
    
#if UNITY_EDITOR
    // 인스펙터 값이 바뀔 때마다 호출됨
    private void OnValidate()
    {
        // 체크박스가 false -> true로 바뀌었을 때만 실행
        if (!fillRanksInEditor) return;

        fillRanksInEditor = false;          // 다시 꺼주고
        AutoFillRankNumbers();              // 실제 작업 실행

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    /// <summary>
    /// rankParent 하위의 Scr_RankInfo들을 찾아서
    /// rankNumText가 null이 아닌 애들에 4,5,6... 번호를 채운다.
    /// </summary>
    public void AutoFillRankNumbers()
    {
        if (rankParent == null)
        {
            Debug.LogError("[Scr_RankController] rankParent가 설정되어 있지 않습니다!");
            return;
        }

        // rankParent 하위 Scr_RankInfo 자동 수집
        rankInfoList.Clear();
        rankInfoList.AddRange(rankParent.GetComponentsInChildren<Scr_RankInfo>(true));

        int num = 4; // 4등부터 시작

        foreach (var info in rankInfoList)
        {
            if (info == null)
                continue;

            // 1,2,3등은 rankNumText가 null이라 패스
            if (info.rankNumText == null)
                continue;

            info.rankNumText.text = num.ToString();
            num++;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(info.rankNumText);
            UnityEditor.EditorUtility.SetDirty(info);
#endif
        }

        //Debug.Log($"[Scr_RankController] 순위 번호 자동 채우기 완료! (4등 ~ {num - 1}등)");
    }
}


