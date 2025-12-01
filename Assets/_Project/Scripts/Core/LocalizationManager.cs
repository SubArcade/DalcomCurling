using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


// 로컬라이제이션 josn에서 파싱할 키값
public enum LocalizationKey
{
    None = 0,
    Btn_Start,
    Btn_Exit,
    Title_Main,
    Label_Gold,

    Label_Setting, //환경설정
    Label_Account, //계정
    Label_DeleteAccount, //계정삭제
    Label_AccountLink, //구글 계정 연동
    Label_GameSetting, //게임설정
    Label_AlarmSetting, //알람설정
    Label_AlarmText, //알람설정 바로 아래 텍스트
    Label_AlarmEnergy, //에너지 풀충전 알림
    Label_EventAlarm, //이벤트 메시지 알림
    Label_Midnight, //야간 알림
    Label_Service, //서비스 이용약관
    Label_Protect, //정보보호
    Label_ContactEMail, //문의메일
    Language_Select, //언어선택

    Label_Info, //내정보.. 플레이어정보
    Label_Profile, //프로필
    Label_Rank, //랭킹

    Label_Codex, //도넛 도감(스토리??)
    Label_HardDonut, //단단도넛 
    Label_SoftDonut, //말랑도넛
    Label_MoistDonut, //촉촉도넛

    Label_GeneratorUgrade, //생성기 업그레이드
    Label_UpgradeText, //생성기 레벨 초과X 텍스트
    Btn_Upgrade, //업그레이드 텍스트
    Label_AskUpgrade, // ???

    Label_TitleList, //칭호목록
    Label_TitleSelect, //칭호선택텍스트
    Label_SubTitle, //환생해야 칭호텍스트
    Label_NoTitle, //칭호가없어요

    Label_ChangeNickname, //닉네임 변경
    Btn_FirstFree, //1회 무료
    Label_ChangeText, //2~8자
    Label_AskChange, //변경하시겠습니까?

    Label_Yes,
    Label_No,
    Label_NotEnough, //젬이 없어요

    Label_Reincarnate, //환생라벨
    Label_WarningReincarnate, //환생경고
    Label_AskReincarnate, //환생묻기
    Btn_Reincarnate, //환생버튼
    Label_ReincarnateText, //환생안내

    Label_GoldShop, //일반상점
    Label_CashShop, //캐시상점
    Label_Monthly, //월정액라벨
    Label_PackageTitle, //패키지 라인업텍스트
    Label_RemoveADS, //광고제거 라인업 텍스트
    Label_Currency, //재화라벨
    Label_GemTitle, //보석텍스트

    Label_DonutText, //상점 도넛텍스트
    Label_EffectText, //이펙트 라인업
    Label_CharacterText, //상점 캐릭터텍스트
    Label_MaleText, //남성 텍스트
    Label_FemalText, //여성 텍스트

    Label_Matching, //매칭텍스트
    Btn_CancleMatch, //취소텍스트

    Up_generator, //생성기텍스트
    Up_generatorUP, //확를증가

    Order_Level, //주문서 n단계 도넛텍스트
    Label_askExit, //게임종료

    Btn_cancle, //취소
    Btn_guestLogin, //게스트 로그인
    Label_guestDescr, //게스트 안내
    Btn_confirm, //확인
    
    Label_accountExit, //계정삭제
    Label_accountDeleteWarning, //계정삭제 안내
    Label_accountDeleteConfirm, //계정삭제 확인텍스트
    Label_accountDeleteInsert, //계정삭제 입력

    Label_getreward, //보상획득
    Label_Touch, //계속하려면 터치

    Label_levelUPreward, //레벨업보상
    Label_getText, //획득하기
    Label_adsDouble, //광고보기
    
    Label_gameHelp, //게임 도움말 이하 스텝5까지
    Label_step1,
    Label_step2,
    Label_step3,
    Label_step4,
    Label_step5,

    Label_orderLabel, //주문서
    Label_askRefresh,
    Label_disRefresh,
    Label_reFreshCharge,

    Label_energyCharge, //에너지충전
    Label_donutEnergy,
    Label_energyTime,

    Label_Dalcom, //달콤컬링

    Label_rankingTier, //티어
    Label_bronze,
    Label_silver,
    Label_gold,
    Label_platinum,
    Label_diamond,
    Label_challenger,

    Label_shopEffect1, //젬 상점 이펙트
    Label_shopEffect1Text,
    Label_shopEffect2,
    Label_shopEffect2Text,
    Label_shopEffect3,
    Label_shopEffect3Text,

    Label_shopCharacter1, //젬 상점 캐릭터
    Label_shopCharacter1Text,
    Label_shopCharacter2,
    Label_shopCharacter2Text,

    Label_ads1,  //캐시상점 광고제거 및 일일무료
    Label_ads1Text,
    Label_ads2,
    Label_ads2Text,
    Label_ads3,
    Label_ads3Text,
    Label_gemFree,
    Label_lookAds,
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private Dictionary<string, Dictionary<string, string>> all;
    private Dictionary<string, string> current;
    public string CurrentLanguage { get; private set; } = "ko";

    public event System.Action OnLanguageChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadTables();
        SetLanguage(CurrentLanguage);
    }

    void LoadTables()
    {
        var asset = Resources.Load<TextAsset>("texts");
        if (asset == null)
        {
            Debug.LogError("[Loc] texts.json not found in Resources");
            return;
        }

        all = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(asset.text);
    }

    public void SetLanguage(string lang)
    {
        if (all != null && all.TryGetValue(lang, out var table))
        {
            CurrentLanguage = lang;
            current = table;
            OnLanguageChanged?.Invoke();
        }
        else
        {
            Debug.LogError($"[Loc] Language not found: {lang}");
        }
    }
    
    public string GetText(LocalizationKey key)
    {
        if (key == LocalizationKey.None)
            return string.Empty;

        if (current == null)
            return key.ToString();

        var k = key.ToString(); // JSON key와 동일해야 함
        if (current.TryGetValue(k, out var value))
            return value;

        // 못 찾으면 키 이름 그대로 노출 (디버깅용)
        return k;
    }
}