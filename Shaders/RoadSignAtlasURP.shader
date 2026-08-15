Shader "Custom/RoadSignAtlasURP"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map (Atlas)", 2D) = "white" {}
        [MainColor] _BaseColor ("Color Tint", Color) = (1,1,1,1)

        [Header(Normal Map Settings)]
        [Toggle(_NORMALMAP)] _EnableNormalMap ("Enable Normal Map", Float) = 0
        [Normal] _BumpMap ("Normal Map (Bump Map)", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0, 5)) = 1.0

        [Header(Color Key Background Removal)]
        [Toggle] _UseColorKey ("Remove Solid Background Color", Float) = 0
        _ColorKey ("Background Color to Key Out", Color) = (0,0,0,1)
        _ColorKeyTolerance ("Color Key Tolerance", Range(0, 1)) = 0.1
        _ColorKeySoftness ("Color Key Softness", Range(0, 1)) = 0.05

        [Header(Atlas Region Controls)]
        _SignRect ("Sign UV Rect (X-Offset, Y-Offset, Width, Height)", Vector) = (0.0, 0.0, 1.0, 1.0)
        [Toggle] _UseGrid ("Use Grid Mode", Float) = 0
        _GridCols ("Grid Columns", Int) = 4
        _GridRows ("Grid Rows", Int) = 4
        _CellIndex ("Cell Index (0-based)", Int) = 0

        [Header(Edge Clamping)]
        [Toggle] _ClampToRect ("Clamp Edges (Prevent Bleeding)", Float) = 1
        _EdgeMargin ("Edge Padding (Fraction)", Range(0, 0.05)) = 0.005

        [Header(Surface Properties)]
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.05
        _Smoothness ("Smoothness", Range(0, 1)) = 0.2
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        [Header(Z Offset Settings)]
        _ZOffset ("Vertex Normal Lift Offset", Range(-0.1, 0.1)) = 0.002
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            Offset -1, -1

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local _NORMALMAP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float4 tangentWS    : TEXCOORD3;
                float2 uv           : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _BumpScale;
                float _UseColorKey;
                float4 _ColorKey;
                float _ColorKeyTolerance;
                float _ColorKeySoftness;
                float4 _SignRect; // (OffsetX, OffsetY, Width, Height)
                float _UseGrid;
                int _GridCols;
                int _GridRows;
                int _CellIndex;
                float _ClampToRect;
                float _EdgeMargin;
                float _Cutoff;
                float _Smoothness;
                float _Metallic;
                float _ZOffset;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                // Apply vertex normal offset to prevent Z-fighting on flat surfaces
                float3 posOS = input.positionOS.xyz + input.normalOS * _ZOffset;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(posOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = float4(normalInput.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float rectX = _SignRect.x;
                float rectY = _SignRect.y;
                float rectW = max(0.0001, _SignRect.z);
                float rectH = max(0.0001, _SignRect.w);

                if (_UseGrid > 0.5)
                {
                    int cols = max(1, _GridCols);
                    int rows = max(1, _GridRows);
                    int index = clamp(_CellIndex, 0, cols * rows - 1);

                    int cellX = index % cols;
                    int cellY = index / cols;
                    // Flip Y so index 0 is top-left cell of the texture atlas
                    int gridY = (rows - 1) - cellY;

                    rectW = 1.0 / cols;
                    rectH = 1.0 / rows;
                    rectX = cellX * rectW;
                    rectY = gridY * rectH;
                }

                // Map input mesh UV (0..1) into target atlas UV sub-rectangle
                float2 targetUV = float2(rectX + input.uv.x * rectW, rectY + input.uv.y * rectH);

                if (_ClampToRect > 0.5)
                {
                    // Clamp atlas sampling coordinates inside sub-rectangle with edge padding to avoid neighbor pixel bleeding
                    float marginX = rectW * _EdgeMargin;
                    float marginY = rectH * _EdgeMargin;
                    targetUV.x = clamp(targetUV.x, rectX + marginX, rectX + rectW - marginX);
                    targetUV.y = clamp(targetUV.y, rectY + marginY, rectY + rectH - marginY);
                }

                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, targetUV) * _BaseColor;

                // Color Key (Chroma Key) Solid Background Removal
                if (_UseColorKey > 0.5)
                {
                    float dist = distance(texColor.rgb, _ColorKey.rgb);
                    float softness = max(0.0001, _ColorKeySoftness);
                    float alphaFactor = saturate((dist - (_ColorKeyTolerance - softness)) / softness);
                    texColor.a *= alphaFactor;
                }

                // Alpha clipping
                if (texColor.a < _Cutoff)
                {
                    discard;
                }

                // Normal Map calculation
                float3 normalWS = normalize(input.normalWS);
#if defined(_NORMALMAP)
                float4 bumpSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, targetUV);
                float3 normalTS = UnpackNormalScale(bumpSample, _BumpScale);
                float3 bitangentWS = cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
                half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangentWS, input.normalWS);
                normalWS = normalize(TransformTangentToWorld(normalTS, tangentToWorld));
#endif

                // Lighting computation
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 lighting = mainLight.color * NdotL + SampleSH(normalWS);

                half3 finalColor = texColor.rgb * lighting;

                return half4(finalColor, texColor.a);
            }
            ENDHLSL
        }
    }
    CustomEditor "UniversalStickerAtlas.Editor.RoadSignAtlasShaderGUI"
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
