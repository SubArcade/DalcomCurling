using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_RankInfo : MonoBehaviour
{
    [SerializeField, Tooltip("티어")] private GameTier gameTier;
    [SerializeField, Tooltip("티어 이미지")] private Image tierImage;
    [SerializeField, Tooltip("칭호 텍스트")] private TMP_Text titlenameText;
    [SerializeField, Tooltip("닉네임 텍스트")] private TMP_Text nicknameText;
    [SerializeField, Tooltip("스코어 텍스트")] private TMP_Text scoreText;
    [SerializeField, Tooltip("랭킹 숫자 텍스트")] public TMP_Text rankNumText;

    // 정보 설정 함수
    public void SetInfo(GameTier tier, Sprite sprite, string title, string nickname, string score)
    {
        gameTier = tier;
        tierImage.sprite = sprite;
        titlenameText.text = title;
        nicknameText.text = nickname;
        scoreText.text = score;
    }
    
}
