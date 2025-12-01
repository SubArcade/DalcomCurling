using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 게임 시작 시 플레이어 또는 상대방의 닉네임을 TextMeshProUGUI에 표시하는 스크립트입니다.
/// 인스펙터를 통해 어떤 플레이어의 닉네임을 표시할지 설정할 수 있습니다./// 
/// </summary>
public class UINicknameDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText; // 닉네임을 표시할 TextMeshProUGUI 객체
    [SerializeField] private bool displayMyNickname = true; // true면 내 닉네임, false면 상대방 닉네임 표시

    //private bool _isProfilesLoaded = false; // 프로필 로딩 완료 여부

    void OnEnable()
    {
        // FirebaseGameManager 인스턴스가 준비되었는지 확인하고, 이벤트 구독
        // Instance가 null이 아닐 때만 구독을 시도합니다.
        if (FirebaseGameManager.Instance != null)
        {
            FirebaseGameManager.Instance.OnProfilesLoaded += SetNicknameOnUI;
            
            // 이미 프로필이 로드된 상태일 수 있으므로, 바로 시도해봅니다.
            if (FirebaseGameManager.Instance.HasLoadedProfiles())
            {
                SetNicknameOnUI(); // 이미 로드된 경우 즉시 처리 (이 안에서 구독 해제됨)
            }
        }
        else
        {
            // GameManager가 아직 초기화되지 않은 경우, 경고를 남깁니다.
            // 이 경우, 이 스크립트가 GameManager보다 먼저 활성화된 상황입니다.
            // GameManager가 초기화된 후 OnEnable이 다시 호출되거나,
            // GameManager의 OnProfilesLoaded 이벤트가 발생할 때 SetNicknameOnUI가 호출될 것입니다.
            Debug.LogWarning("UINicknameDisplay: FirebaseGameManager.Instance가 아직 준비되지 않았습니다. 프로필 로딩을 기다립니다.");
        }
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독을 해제합니다.
        if (FirebaseGameManager.Instance != null)
        {
            FirebaseGameManager.Instance.OnProfilesLoaded -= SetNicknameOnUI;
        }
    }

    private void SetNicknameOnUI()
    {
        // 이벤트 수신 후 즉시 구독을 해제하여 중복 호출을 방지합니다.
        if (FirebaseGameManager.Instance != null)
        {
            FirebaseGameManager.Instance.OnProfilesLoaded -= SetNicknameOnUI;
        }

        if (nicknameText == null)
        {
            Debug.LogWarning("UINicknameDisplay: nicknameText가 할당되지 않았습니다.");
            return;
        }

        var gameManager = FirebaseGameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("UINicknameDisplay: FirebaseGameManager.Instance가 null입니다.");
            return;
        }

        PlayerProfile targetProfile = null;
        if (displayMyNickname)
        {
            targetProfile = gameManager.GetPlayerProfile(FirebaseAuthManager.Instance.UserId);
        }
        else
        {
            string opponentId = gameManager.GetOpponentId();
            if (!string.IsNullOrEmpty(opponentId))
            {
                targetProfile = gameManager.GetPlayerProfile(opponentId);
            }
        }

        if (targetProfile != null)
        {
            nicknameText.text = targetProfile.Nickname;
        }
        else
        {
            nicknameText.text = displayMyNickname ? "내 닉네임 없음" : "상대 닉네임 없음";
            Debug.LogWarning($"UINicknameDisplay: {(displayMyNickname ? "내" : "상대방")} 닉네임 프로필을 찾을 수 없습니다.");
        }
    }
}
