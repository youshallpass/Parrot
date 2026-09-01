Shader "Custom/WatercolorSprite"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)

        // Textures
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _PaperTex("Paper Texture", 2D) = "white" {}

        // Texture Scale
        _NoiseScale("Noise Scale", Float) = 1.0
        _PaperScale("Paper Scale", Float) = 1.0

        // Watercolor Wash
        _WashAmount("Wash Amount", Range(0, 2)) = 1
        _WashSize("Wash Size", Range(0, 0.1)) = 0.01
        _WashNoise("Wash Noise", Range(0, 1)) = 0

        // Color Saturation
        _ColorSaturation("Color Saturation", Range(0, 5)) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Structs

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            // Textures

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            TEXTURE2D(_PaperTex);
            SAMPLER(sampler_PaperTex);

            // Material Parameters

            CBUFFER_START(UnityPerMaterial)

                float4 _Color;
                float4 _MainTex_ST;

                float _NoiseScale;
                float _PaperScale;

                float _WashAmount;
                float _WashSize;
                float _WashNoise;

                float _ColorSaturation;

            CBUFFER_END

            // Vertex

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;

                return OUT;
            }

            // Paint Color

            float4 GetCol(float2 pos, float2 mainResolution)
            {
                float2 uv = pos / mainResolution;
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            }

            // Watercolor Color

            float4 GetCol2(float2 pos, float2 mainResolution)
            {
                float2 uv = pos / mainResolution;
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            }

            // Noise

            float4 GetRand(float2 pos, float2 noiseResolution)
            {
                float2 uv = pos / noiseResolution;
                uv *= _NoiseScale;

                return SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv);
            }

            // Gradient

            float2 GetGrad(float2 pos, float delta, float2 mainResolution)
            {
                float2 d = float2(delta, 0.0);

                float3 horizontal = GetCol(pos + d.xy, mainResolution).rgb - GetCol(pos - d.xy, mainResolution).rgb;
                float3 vertical = GetCol(pos + d.yx, mainResolution).rgb - GetCol(pos - d.yx, mainResolution).rgb;

                return float2(
                    dot(horizontal, float3(0.333, 0.333, 0.333)),
                    dot(vertical, float3(0.333, 0.333, 0.333))
                ) / delta;
            }

            // Second Gradient

            float2 GetGrad2(float2 pos, float delta, float2 mainResolution)
            {
                float2 d = float2(delta, 0.0);

                float3 horizontal = GetCol2(pos + d.xy, mainResolution).rgb - GetCol2(pos - d.xy, mainResolution).rgb;
                float3 vertical = GetCol2(pos + d.yx, mainResolution).rgb - GetCol2(pos - d.yx, mainResolution).rgb;

                return float2(
                    dot(horizontal, float3(0.333, 0.333, 0.333)),
                    dot(vertical, float3(0.333, 0.333, 0.333))
                ) / delta;
            }

            // Edge Pattern

            float HtPattern(float2 pos, float2 noiseResolution)
            {
                float4 random = GetRand(pos * 0.4 / 0.7, noiseResolution);
                return saturate(pow(random.x + 0.3, 2.0) - 0.45);
            }

            // Paint Value

            float GetVal(float2 pos, float2 mainResolution)
            {
                float3 c = GetCol(pos, mainResolution).rgb;
                return length(c);
            }

            // Edge Mask

            float GetBWDist(float2 pos, float2 mainResolution, float2 noiseResolution)
            {
                float value = GetVal(pos, mainResolution);
                float pattern = HtPattern(pos * 0.7, noiseResolution);

                return smoothstep(0.9, 1.1, value * 0.9 + pattern);
            }

            // Fragment

            half4 frag(Varyings IN) : SV_Target
            {
                // Texture Resolution

                uint mainWidth;
                uint mainHeight;
                uint noiseWidth;
                uint noiseHeight;

                _MainTex.GetDimensions(mainWidth, mainHeight);
                _NoiseTex.GetDimensions(noiseWidth, noiseHeight);

                float2 Res0 = float2(mainWidth, mainHeight);
                float2 Res1 = float2(noiseWidth, noiseHeight);

                // Pixel Position

                float2 pos = IN.uv * Res0;
                float2 pos2 = pos;
                float2 pos3 = pos;
                float2 pos4 = pos;
                float2 pos0 = pos;

                // Accumulators

                float3 col = float3(0.0, 0.0, 0.0);
                float3 col2 = float3(0.0, 0.0, 0.0);
                float cnt = 0.0;
                float cnt2 = 0.0;

                // Watercolor Samples

                const int SampNum = 24;

                for (int i = 0; i < SampNum; i++)
                {
                    // Outline Gradients

                    float2 gr = GetGrad(pos, 2.0, Res0);
                    gr += 0.0001 * (GetRand(pos, Res1).xy - 0.5);

                    float2 gr2 = GetGrad(pos2, 2.0, Res0);
                    gr2 += 0.0001 * (GetRand(pos2, Res1).xy - 0.5);

                    // Wash Gradients

                    float2 gr3 = GetGrad2(pos3, 2.0, Res0);
                    gr3 += 0.0001 * (GetRand(pos3, Res1).xy - 0.5);

                    float2 gr4 = GetGrad2(pos4, 2.0, Res0);
                    gr4 += 0.0001 * (GetRand(pos4, Res1).xy - 0.5);

                    // Gradient Strength

                    float grl = saturate(10.0 * length(gr));
                    float gr2l = saturate(10.0 * length(gr2));

                    // Gradient Normals

                    float2 normal1 = float2(gr.y, -gr.x);
                    float2 normal2 = float2(gr2.y, -gr2.x);

                    float len1 = length(normal1);
                    float len2 = length(normal2);

                    if (len1 > 0.00001)
                        normal1 /= len1;
                    else
                        normal1 = 0.0;

                    if (len2 > 0.00001)
                        normal2 /= len2;
                    else
                        normal2 = 0.0;

                    // Outline Movement

                    pos += 0.8 * normal1;
                    pos2 -= 0.8 * normal2;

                    // Sample Factor

                    float fact = 1.0 - ((float)i / (float)SampNum);

                    // Edge Samples

                    float bw1 = GetBWDist(pos, Res0, Res1);
                    float bw2 = GetBWDist(pos2, Res0, Res1);

                    // Outline Accumulation

                    col += fact * lerp(
                        float3(1.2, 1.2, 1.2),
                        float3(bw1 * 2.0, bw1 * 2.0, bw1 * 2.0),
                        grl
                    );

                    col += fact * lerp(
                        float3(1.2, 1.2, 1.2),
                        float3(bw2 * 2.0, bw2 * 2.0, bw2 * 2.0),
                        gr2l
                    );

                    // Random Wash Offset

                    float2 washRandom = GetRand(pos0 * 0.07, Res1).xy - 0.5;

                    // Wash Normals

                    float2 normalizedGr3 = normalize(gr3);
                    float2 normalizedGr4 = normalize(gr4);

                    if (length(gr3) < 0.00001)
                        normalizedGr3 = 0.0;

                    if (length(gr4) < 0.00001)
                        normalizedGr4 = 0.0;

                    // Wash Movement

                    pos3 += 0.25 * normalizedGr3 + 0.5 * washRandom;
                    pos4 -= 0.5 * normalizedGr4 + 0.5 * washRandom;

                    // Wash Colors

                    float3 color1 = GetCol2(pos3, Res0).rgb;
                    float3 color2 = GetCol2(pos4, Res0).rgb;

                    // Random Color Variation

                    float3 randomColor1 = GetRand(pos3, Res1).rgb;
                    float3 randomColor2 = GetRand(pos4, Res1).rgb;

                    // Wash Weights

                    float f1 = 3.0 * fact;
                    float f2 = 4.0 * (0.7 - fact);

                    // Wash Accumulation

                    col2 += f1 * (color1 + 0.25 + 0.4 * randomColor1);
                    col2 += f2 * (color2 + 0.25 + 0.4 * randomColor2);

                    cnt2 += f1 + f2;
                    cnt += fact;
                }

                // Normalize

                col /= cnt * 2.5;
                col2 /= cnt2 * 1.65;

                // Outline + Color

                col = saturate(saturate(col * 0.9 + 0.1) * col2);

                // Original Color

                float3 originalColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;

                // Watercolor Result

                float3 watercolor = lerp(originalColor, col, 1.0);

                // Paper Texture

                float2 paperUV = IN.uv * _PaperScale;
                float3 paper = SAMPLE_TEXTURE2D(_PaperTex, sampler_PaperTex, paperUV).rgb;
                float3 paperMix = lerp(paper, float3(1.0, 1.0, 1.0), 0.85);

                watercolor *= paperMix;

                // Paper Grain

                float grain = GetRand(pos0 * 2.5, Res1).x;
                watercolor += 0.15 * grain;

                // Vignette

                float2 centered = IN.uv - 0.5;
                float r = length(centered);
                float vign = 1.0 - r * r * r * r;

                watercolor *= vign;

                // Color Saturation

                float gray = dot(watercolor, float3(0.299, 0.587, 0.114));
                float3 grayColor = float3(gray, gray, gray);

                watercolor = lerp(grayColor, watercolor, _ColorSaturation);

                // Original Alpha

                float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).a;

                // Watercolor Wash

                float2 washX = float2(_WashSize, 0.0);
                float2 washY = float2(0.0, _WashSize);

                float alphaLeft = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - washX).a;
                float alphaRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + washX).a;
                float alphaUp = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + washY).a;
                float alphaDown = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - washY).a;

                // Diagonal Alpha Samples

                float alphaUpLeft = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - washX + washY).a;
                float alphaUpRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + washX + washY).a;
                float alphaDownLeft = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - washX - washY).a;
                float alphaDownRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + washX - washY).a;

                // Surrounding Paint

                float surroundingAlpha = (alphaLeft + alphaRight + alphaUp + alphaDown + alphaUpLeft + alphaUpRight + alphaDownLeft + alphaDownRight) / 8.0;

                // Outside Paint Mask

                float outsidePaint = 1.0 - alpha;
                float washMask = surroundingAlpha * outsidePaint;

                // Wash Noise

                float washNoise = GetRand(pos0 * 0.5, Res1).x;
                washNoise = lerp(1.0, washNoise, _WashNoise);

                // Wash Strength

                float washStrength = saturate(washMask * washNoise * _WashAmount);

                // Original Paint Color

                float3 washColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;

                // Apply Original Color

                watercolor = lerp(watercolor, washColor, washStrength);

                // Extend Alpha

                alpha = max(alpha, washStrength);

                // Final

                return float4(saturate(watercolor), saturate(alpha)) * IN.color;
            }

            ENDHLSL
        }
    }
}