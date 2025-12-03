using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_MatchingPopUp : MonoBehaviour
{
    [SerializeField] private Button cancleButton;

    void Start()
    {
        cancleButton.onClick.AddListener(() =>
        {
            FirebaseMatchmakingManager.Instance.CancelMatchmaking();
            UIManager.Instance.Close(PanelId.MatchingPopUp);
            
            // 애널리틱스 이펙트 변경
            AnalyticsManager.Instance.MatchSearchCancel();
        });
    }
}
