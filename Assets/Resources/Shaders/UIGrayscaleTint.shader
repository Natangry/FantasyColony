Shader "UI/GrayscaleTint"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        [PerRendererData] _TextureSampleAdd ("Texture Sample Add", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            #pragma multi_compile __ UNITY_UI_ETC1_EXTERNAL_ALPHA

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _ClipRect;
            #ifdef UNITY_UI_ETC1_EXTERNAL_ALPHA
            sampler2D _AlphaTex;
            #endif
            float4 _TextureSampleAdd;

            v2f vert (appdata_t IN)
            {
                v2f OUT;
                OUT.worldPos = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 SampleSpriteTexture(half2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);
            #ifdef UNITY_UI_ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D(_AlphaTex, uv);
                color.a = alpha.r;
            #endif
                // Unity UI atlas fixup (see Unity's default UI shader)
                color.rgb = (color.rgb - _TextureSampleAdd.rgb) * color.a + _TextureSampleAdd.rgb;
                return color;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // sample wood texture (do NOT premultiply by vertex color)
                fixed4 tex = SampleSpriteTexture(IN.texcoord);

                // grayscale from texture only (preserve detail, neutralize hue)
                fixed g = dot(tex.rgb, fixed3(0.299, 0.587, 0.114));

                // tint by Graphic color *and* material color, so both work consistently
                fixed3 tintRGB = IN.color.rgb * _Color.rgb;
                fixed tintA = IN.color.a * _Color.a;
                fixed3 tinted = g * tintRGB;
                fixed a = tex.a * tintA;
                fixed4 outCol = fixed4(tinted, a);

                #ifdef UNITY_UI_CLIP_RECT
                outCol.a *= UnityGet2DClipping(IN.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (outCol.a - 0.001);
                #endif

                return outCol;
            }
            ENDCG
        }
    }
}

