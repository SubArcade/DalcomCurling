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
        cancleBtn.onClick.AddListener(() => UIManager.Instance.Close(PanelId.DeleteAccountPopUp));
        confirmBtn.onClick.AddListener(() => OnConfirmBtn());

    }

    private void OnConfirmBtn() 
    {
        string Text = input.text.Trim();

        if (Text == "DALCOM")
        {
            Debug.Log("계정삭제 완료!!");
            UIManager.Instance.Close(PanelId.DeleteAccountPopUp);
        }
        else 
        {
            Debug.Log("DALCOM을 제대로 입력하세요");
        }
    }
}