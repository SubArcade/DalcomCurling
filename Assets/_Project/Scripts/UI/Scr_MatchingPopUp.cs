using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_MatchingPopUp : MonoBehaviour
{
    [SerializeField] private Button cancleButton;

    void Awake()
    {
        cancleButton = transform.Find("background/CancleButton")?.GetComponent<Button>();
    }
    void Start()
    {
        cancleButton.onClick.AddListener(() =>
        {
            FirebaseMatchmakingManager.Instance.CancelMatchmaking();
            UIManager.Instance.Close(PanelId.MatchingPopUp);
        });
    }
}
