using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_DeleteAccountPopUp : MonoBehaviour
{
    [Header("취소 확인 버튼")]
    [SerializeField] private Button cancleBtn;
    [SerializeField] private Button confirmBtn;

    [Header("입력칸")]
    [SerializeField] private TMP_InputField input;

    void Start()
    {
        cancleBtn.onClick.AddListener(() => 
        {
            UIManager.Instance.Close(PanelId.DeleteAccountPopUp);
            UIManager.Instance.Open(PanelId.DetailedSettingsPanel);
            
        });

        confirmBtn.onClick.AddListener(() => OnConfirmBtn());

    }

    private async void OnConfirmBtn() 
    {
        string text = input.text.Trim();

        if (text.Equals("DALCOM", StringComparison.OrdinalIgnoreCase))
        {
            //Debug.Log("계정삭제 완료!!");
            await FirebaseAuthManager.Instance.DeleteAccountAsync();
            UIManager.Instance.Close(PanelId.DeleteAccountPopUp);
            UIManager.Instance.Open(PanelId.LoginPanel);
            input.text = string.Empty;
            BoardManager.Instance.isCompleted = false;
            // BoardManager.Instance.SetBoard();
        }
        else 
        {
            //Debug.Log("DALCOM을 제대로 입력하세요");
        }
    }
}