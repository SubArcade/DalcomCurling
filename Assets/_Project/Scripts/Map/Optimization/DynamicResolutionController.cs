using UnityEngine;

public class DynamicResolutionController : MonoBehaviour
{
    // 게임이 버벅거릴 때 GPU 로드만 줄여주고 화면 품질은 거의 그대로 유지.
    // 바로적용 금지 > 인게임 요소랑 충돌남

    [Range(0.5f, 1f)]
    public float minScale = 0.7f;
    public float maxScale = 1f;

    void Update()
    {
        float fps = 1f / Time.deltaTime;

        if (fps < 45f)
        {
            ScalableBufferManager.ResizeBuffers(minScale, minScale);
        }
        else if (fps > 55f)
        {
            ScalableBufferManager.ResizeBuffers(maxScale, maxScale);
        }
    }
}
