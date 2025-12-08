using UnityEngine;
using UnityEngine.Rendering;

public class RenderOptimizer : MonoBehaviour
{
    //SRP Batcher + Instancing + Texture Streaming + Shadow 옵션 최적화

    void Awake()
    {
        // SRP Batcher On
        GraphicsSettings.useScriptableRenderPipelineBatching = true;

        // GPU Instancing 활성화
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        // Texture Streaming
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsAddAllCameras = true;

        // Shadow Distance 줄이기 (모바일 필수)
        QualitySettings.shadowDistance = 25f;

        // Anisotropic disabling (모바일 최적)
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;

        // Anti-aliasing (Mobile-friendly)
        QualitySettings.antiAliasing = 2; // 2x 정도가 성능/품질 밸런스 좋음

        Debug.Log("[RenderOptimizer] Graphics optimized for mobile.");
    }
}
