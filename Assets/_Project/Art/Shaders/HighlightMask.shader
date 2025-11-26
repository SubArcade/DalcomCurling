Shader "UI/HighlightMask"
{
    Properties
    {
        _Color ("Overlay Color", Color) = (0,0,0,0.7)
        _HoleCenter ("Hole Center", Vector) = (0.5,0.5,0,0)
        _HoleRadius ("Hole Radius", float) = 0.15
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float2 _HoleCenter;
            float _HoleRadius;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy; 
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenUV = i.vertex.xy / _ScreenParams.xy;

                float dist = distance(screenUV, _HoleCenter);

                if (dist < _HoleRadius)
                    return float4(0,0,0,0);

                return _Color;
            }
            ENDCG
        }
    }
}