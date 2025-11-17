using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Scr_RankController : MonoBehaviour
{
    [SerializeField, Tooltip("티어 이미지")] private List<Scr_RankInfo> rankInfoList = new List<Scr_RankInfo>();
    [SerializeField] private Transform rankParent;
    
    [Header("에디터용 리스트 채우는 함수")]
    [SerializeField]
    private bool fillRanksInEditor = false;

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


