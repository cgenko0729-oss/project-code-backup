Shader "Unlit/RingRadialFill"
{
    Properties
    {
        _Color        ("Tint",   Color)             = (1,0,0,0.8)
        _InnerRadius  ("Inner",  Range(0,0.99))      = 0.85   // 0-1, 1 = outer edge
        _Progress     ("Progress", Range(0,1))      = 0      // 0 = empty, 1 = full
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos    : SV_POSITION; float2 uv : TEXCOORD0; };

            float4 _Color;
            float  _InnerRadius;
            float  _Progress;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv * 2 - 1;      // -1 Åc +1 around centre
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = length(i.uv);                 // radial distance (0ÅcÅ„2)
                // keep only ring thickness
                if (dist < _InnerRadius || dist > 1.0) discard;

                // angle 0 Å® 1 from +X axis CCW
                float angle = (atan2(i.uv.y, i.uv.x) + UNITY_PI) / (2 * UNITY_PI);

                // reveal clockwise (swap > for CCW)
                if (angle > _Progress) discard;

                return _Color;
            }
            ENDHLSL
        }
    }
}
