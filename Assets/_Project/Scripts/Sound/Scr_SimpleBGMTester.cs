using FMODUnity;
using FMOD.Studio;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private EventReference testBGM;
    private EventInstance bgmInstance;

    void Start()
    {
        bgmInstance = RuntimeManager.CreateInstance(testBGM);

        // ✅ 위치 정보 설정 (현재 오브젝트 위치 기준)
        bgmInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));

        bgmInstance.start();
    }

    void OnDestroy()
    {
        if (bgmInstance.isValid())
        {
            bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            bgmInstance.release();
        }
    }
}
