using UnityEngine;
using FMODUnity;

public class ButtonSoundPlayer : MonoBehaviour
{
    [SerializeField] private EventReference buttonClickSFX;

    public void PlayButtonClickSound()
    {
        if (!buttonClickSFX.IsNull)
        {
            RuntimeManager.PlayOneShot(buttonClickSFX);
            Debug.Log("🔘 버튼 클릭 사운드 재생");
        }
        else
        {
            Debug.LogWarning("버튼 클릭 사운드가 연결되지 않았습니다.");
        }
    }
}

