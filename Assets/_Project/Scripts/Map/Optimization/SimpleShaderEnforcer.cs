using UnityEngine;

public class SimpleShaderEnforcer : MonoBehaviour
{
    //씬의 모든 Renderer를 Simple Lit으로 교체 + 불필요한 Keyword OFF
    [ContextMenu("Apply SimpleLit To All Renderers")]
    void Apply()
    {
        Shader simpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");

        foreach (var r in FindObjectsOfType<Renderer>())
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null) continue;

                mat.shader = simpleLit;
                mat.DisableKeyword("_NORMALMAP");
                mat.DisableKeyword("_EMISSION");
                mat.DisableKeyword("_SPECULARHIGHLIGHTS");
                mat.DisableKeyword("_RECEIVE_SHADOWS_OFF");
            }
        }

        Debug.Log("[SimpleShaderEnforcer] All materials optimized.");
    }
}
