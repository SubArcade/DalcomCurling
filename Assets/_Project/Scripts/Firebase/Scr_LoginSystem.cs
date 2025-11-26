using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginSystem : MonoBehaviour
{
    [SerializeField] private Button googlePlayButton;
    [SerializeField] private Button guestButtontest;

    [Header("게스트로그인 팝업")]
    [SerializeField] private GameObject guestPopup;
    [SerializeField] private Button guestPopupButton;
    
    void Start()
    {
        googlePlayButton.onClick.AddListener(() => FirebaseAuthManager.Instance.GooglePlayLoginFirst());
        guestButtontest.onClick.AddListener(AnonymousLogin);

        guestPopupButton.onClick.AddListener(() => guestPopup.SetActive(true)); //게스트 확인 팝업
        
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