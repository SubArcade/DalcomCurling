using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginSystem : MonoBehaviour
{
    [SerializeField] private Button googlePlayButton;
    [SerializeField] private Button guestButtontest;

    [Header("게스트로그인 팝업")]
    [SerializeField] private GameObject guestPopup;
    [SerializeField] private Button guestPopupButton;
    [SerializeField] private Button guestPoupCancleBtn;
    
    void Start()
    {
        googlePlayButton.onClick.AddListener(() => FirebaseAuthManager.Instance.GooglePlayLoginFirst());
        guestButtontest.onClick.AddListener(AnonymousLogin);

        guestPopupButton.onClick.AddListener(() => guestPopup.SetActive(true)); //게스트 확인 팝업
        guestPoupCancleBtn.onClick.AddListener(() => UIManager.Instance.Close(PanelId.GuestPopup));
        //FirebaseAuthManager.Instance.Init();
    }
    public void LogOut()
    {
        FirebaseAuthManager.Instance.Logout();
    }

    public void AnonymousLogin() //게스트 로그인
    {
        FirebaseAuthManager.Instance.AnonymousLogin();
    }
    
}