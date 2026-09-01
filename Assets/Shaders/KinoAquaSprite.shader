Shader "Custom/KinoAquaPaintLayer"
{
    Properties
    {
        [MainTexture] _MainTex("Paint Texture", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)

        [Header(KinoAqua)]

        _NoiseTexture("Noise Texture", 2D) = "white" {}

        _Opacity("Opacity", Range(0,1)) = 1

        _EdgeColor("Edge Color", Color) = (0,0,0,1)
        _EdgeContrast("Edge Contrast", Range(0.01,4)) = 1.2

        _FillColor("Fill Color", Color) = (1,1,1,1)

        _BlurWidth("Blur Width", Range(0,2)) = 1
        _BlurFrequency("Blur Frequency", Range(0,1)) = 0.5
        _HueShift("Hue Shift", Range(0,0.3)) = 0.1

        _Interval("Interval", Range(0.1,5)) = 1
        _Iteration("Iteration", Range(4,32)) = 20

        [Header(Paint Bleeding)]

        _Bleed("Paint Bleed", Range(0,20)) = 4
        _BleedOpacity("Bleed Opacity", Range(0,1)) = 0.45
        _BleedNoise("Bleed Noise", Range(0,1)) = 0.5

        [Header(Overlay)]

        _OverlayMode("Overlay Mode", Range(0,3)) = 0

        _OverlayTexture("Overlay Texture", 2D) = "white" {}
        _OverlayOpacity("Overlay Opacity", Range(0,1)) = 0;
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
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            // ============================================================
            // STRUCTS
            // ============================================================

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


            // ============================================================
            // TEXTURES
            // ============================================================

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTexture);
            SAMPLER(sampler_NoiseTexture);

            TEXTURE2D(_OverlayTexture);
            SAMPLER(sampler_OverlayTexture);


            // ============================================================
            // PARAMETERS
            // ============================================================

            CBUFFER_START(UnityPerMaterial)

                float4 _Color;

                float _Opacity;

                float4 _EdgeColor;
                float _EdgeContrast;

                float4 _FillColor;

                float _BlurWidth;
                float _BlurFrequency;
                float _HueShift;

                float _Interval;
                float _Iteration;

                float _Bleed;
                float _BleedOpacity;
                float _BleedNoise;

                float _OverlayMode;
                float _OverlayOpacity;

            CBUFFER_END


            // ============================================================
            // VERTEX
            // ============================================================

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS =
                    TransformObjectToHClip(
                        IN.positionOS.xyz
                    );

                OUT.uv = IN.uv;

                OUT.color =
                    IN.color * _Color;

                return OUT;
            }


            // ============================================================
            // BASIC MATH
            // ============================================================

            float Luminance(float3 c)
            {
                return dot(
                    c,
                    float3(
                        0.2126,
                        0.7152,
                        0.0722
                    )
                );
            }


            float2 Rotate90(float2 v)
            {
                return v.yx * float2(-1,1);
            }


            // ============================================================
            // HSV → RGB
            // ============================================================

            float3 HsvToRgb(float3 hsv)
            {
                float3 rgb =
                    abs(
                        frac(
                            hsv.x +
                            float3(
                                0,
                                2.0 / 3.0,
                                1.0 / 3.0
                            )
                        )
                        * 6
                        - 3
                    );

                rgb =
                    saturate(
                        rgb - 1
                    );

                return
                    hsv.z *
                    lerp(
                        float3(1,1,1),
                        rgb,
                        hsv.y
                    );
            }


            // ============================================================
            // RESOLUTION
            // ============================================================

            void GetResolution(
                out float2 resolution,
                out float aspect,
                out float aspectRcp
            )
            {
                uint width;
                uint height;

                _MainTex.GetDimensions(
                    width,
                    height
                );

                resolution =
                    float2(
                        max(width,1),
                        max(height,1)
                    );

                aspect =
                    resolution.x /
                    resolution.y;

                aspectRcp =
                    1.0 /
                    aspect;
            }


            // ============================================================
            // COORDINATES
            // ============================================================

            float2 UV2SC(
                float2 uv,
                float aspect
            )
            {
                float2 p =
                    uv - 0.5;

                p.x *= aspect;

                return p;
            }


            float2 SC2UV(
                float2 p,
                float aspectRcp
            )
            {
                p.x *= aspectRcp;

                return p + 0.5;
            }


            // ============================================================
            // PAINT SAMPLE
            // ============================================================

            float4 SamplePaintUV(float2 uv)
            {
                return SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv
                );
            }


            // ============================================================
            // KINO INPUT
            //
            // Transparent PaintLayer pixels are converted to WHITE
            // for the KinoAqua calculation.
            //
            // This makes the input resemble the original KinoAqua
            // situation: white paper + colored paint.
            // ============================================================

            float4 SampleKino(
                float2 p,
                float aspectRcp
            )
            {
                float2 uv =
                    SC2UV(
                        p,
                        aspectRcp
                    );

                float4 paint =
                    SamplePaintUV(
                        uv
                    );

                float3 color =
                    lerp(
                        float3(1,1,1),
                        paint.rgb,
                        paint.a
                    );

                return float4(
                    color,
                    paint.a
                );
            }


            float3 SampleKinoColor(
                float2 p,
                float aspectRcp
            )
            {
                return SampleKino(
                    p,
                    aspectRcp
                ).rgb;
            }


            float SampleKinoLuminance(
                float2 p,
                float aspectRcp
            )
            {
                return Luminance(
                    SampleKinoColor(
                        p,
                        aspectRcp
                    )
                );
            }


            // ============================================================
            // NOISE
            // ============================================================

            float3 SampleNoise(float2 p)
            {
                return SAMPLE_TEXTURE2D(
                    _NoiseTexture,
                    sampler_NoiseTexture,
                    p
                ).rgb;
            }


            // ============================================================
            // KINO GRADIENT
            // ============================================================

            float2 GetGradient(
                float2 p,
                float freq,
                float aspectRcp
            )
            {
                float2 dx =
                    float2(
                        _Interval / 200.0,
                        0
                    );

                float ldx =
                    SampleKinoLuminance(
                        p + dx.xy,
                        aspectRcp
                    )
                    -
                    SampleKinoLuminance(
                        p - dx.xy,
                        aspectRcp
                    );

                float ldy =
                    SampleKinoLuminance(
                        p + dx.yx,
                        aspectRcp
                    )
                    -
                    SampleKinoLuminance(
                        p - dx.yx,
                        aspectRcp
                    );

                float2 n =
                    SampleNoise(
                        p * 0.4 * freq
                    ).gb
                    - 0.5;

                return
                    float2(
                        ldx,
                        ldy
                    )
                    +
                    n * 0.05;
            }


            // ============================================================
            // EDGE PROCESSING
            // ============================================================

            float ProcessEdge(
                inout float2 p,
                float stride,
                float aspectRcp
            )
            {
                float2 grad =
                    GetGradient(
                        p,
                        1,
                        aspectRcp
                    );

                float edge =
                    saturate(
                        length(grad) * 10
                    );

                float pattern =
                    SampleNoise(
                        p * 0.8
                    ).r;

                float gradLength =
                    length(grad);

                float2 direction =
                    float2(0,0);

                if (gradLength > 0.00001)
                {
                    direction =
                        Rotate90(
                            grad /
                            gradLength
                        );
                }

                p +=
                    direction *
                    stride;

                return
                    pattern *
                    edge;
            }


            // ============================================================
            // FILL PROCESSING
            // ============================================================

            float3 ProcessFill(
                inout float2 p,
                float stride,
                float aspectRcp,
                float blurFrequency
            )
            {
                float2 grad =
                    GetGradient(
                        p,
                        blurFrequency,
                        aspectRcp
                    );

                float gradLength =
                    length(grad);

                float2 direction =
                    float2(0,0);

                if (gradLength > 0.00001)
                {
                    direction =
                        grad /
                        gradLength;
                }

                p +=
                    direction *
                    stride;

                float shift =
                    SampleNoise(
                        p * 0.1
                    ).r
                    * 2;

                return
                    SampleKinoColor(
                        p,
                        aspectRcp
                    )
                    *
                    HsvToRgb(
                        float3(
                            shift,
                            _HueShift,
                            1
                        )
                    );
            }


            // ============================================================
            // ORIGINAL KINOAQUA PROCESS
            // ============================================================

            float3 ProcessAt(float2 uv)
            {
                float2 resolution;
                float aspect;
                float aspectRcp;

                GetResolution(
                    resolution,
                    aspect,
                    aspectRcp
                );

                float2 p =
                    UV2SC(
                        uv,
                        aspect
                    );

                float2 p_e_n = p;
                float2 p_e_p = p;

                float2 p_c_n = p;
                float2 p_c_p = p;

                float iteration =
                    clamp(
                        _Iteration,
                        4,
                        32
                    );

                float iterationRcp =
                    1.0 /
                    iteration;

                float stride =
                    0.04 *
                    iterationRcp;

                float blurFrequency =
                    exp(
                        (_BlurFrequency - 0.5)
                        * 6
                    );

                float acc_e = 0;

                float3 acc_c =
                    float3(0,0,0);

                float sum_e = 0;

                float sum_c = 0;


                [loop]
                for (int i = 0; i < 32; i++)
                {
                    if (i >= (int)iteration)
                        break;


                    // ====================================================
                    // EDGE
                    // ====================================================

                    float w_e =
                        1.5 -
                        i *
                        iterationRcp;

                    acc_e +=
                        ProcessEdge(
                            p_e_n,
                            -stride,
                            aspectRcp
                        )
                        *
                        w_e;

                    acc_e +=
                        ProcessEdge(
                            p_e_p,
                            stride,
                            aspectRcp
                        )
                        *
                        w_e;

                    sum_e +=
                        w_e *
                        2;


                    // ====================================================
                    // FILL
                    // ====================================================

                    float w_c =
                        0.2 +
                        i *
                        iterationRcp;

                    acc_c +=
                        ProcessFill(
                            p_c_n,
                            -stride *
                            _BlurWidth,
                            aspectRcp,
                            blurFrequency
                        )
                        *
                        w_c;

                    acc_c +=
                        ProcessFill(
                            p_c_p,
                            stride *
                            _BlurWidth,
                            aspectRcp,
                            blurFrequency
                        )
                        *
                        w_c *
                        0.3;

                    sum_c +=
                        w_c *
                        1.3;
                }


                // ========================================================
                // NORMALIZE
                // ========================================================

                acc_e /=
                    max(
                        sum_e,
                        0.00001
                    );

                acc_c /=
                    max(
                        sum_c,
                        0.00001
                    );


                // ========================================================
                // EDGE CONTRAST
                // ========================================================

                acc_e =
                    saturate(
                        (acc_e - 0.5)
                        *
                        _EdgeContrast
                        +
                        0.5
                    );


                // ========================================================
                // COLORS
                // ========================================================

                float3 rgb_e =
                    lerp(
                        float3(1,1,1),
                        _EdgeColor.rgb,
                        _EdgeColor.a *
                        acc_e
                    );

                float3 rgb_f =
                    lerp(
                        float3(1,1,1),
                        acc_c,
                        _FillColor.a
                    )
                    *
                    _FillColor.rgb;

                return
                    rgb_e *
                    rgb_f;
            }


            // ============================================================
            // PAINT BLEED
            //
            // This does NOT replace KinoAqua.
            //
            // It only determines how far the watercolor is allowed to
            // exist outside the actual painted alpha.
            // ============================================================

            float GetBleedMask(
                float2 uv,
                float2 texelSize
            )
            {
                float original =
                    SamplePaintUV(
                        uv
                    ).a;

                float surrounding = 0;

                const int samples = 16;

                [unroll]
                for (int i = 0; i < samples; i++)
                {
                    float angle =
                        6.2831853 *
                        (
                            (float)i /
                            samples
                        );

                    float2 dir =
                        float2(
                            cos(angle),
                            sin(angle)
                        );

                    float distance =
                        _Bleed *
                        texelSize.x;

                    float a =
                        SamplePaintUV(
                            uv +
                            dir *
                            distance
                        ).a;

                    surrounding += a;
                }

                surrounding /=
                    samples;

                float noise =
                    SampleNoise(
                        uv * 4
                    ).r;

                noise =
                    lerp(
                        1,
                        noise,
                        _BleedNoise
                    );

                float bleed =
                    surrounding *
                    noise *
                    _BleedOpacity;

                // Don't alter already-painted pixels.

                bleed *=
                    1 -
                    original;

                return
                    saturate(
                        bleed
                    );
            }


            // ============================================================
            // OVERLAY
            // ============================================================

            float3 ApplyOverlay(
                float3 c1,
                float3 c2,
                float alpha
            )
            {
                float3 c;

                if (_OverlayMode < 0.5)
                {
                    c = c1;
                }
                else if (_OverlayMode < 1.5)
                {
                    c = c1 * c2;
                }
                else if (_OverlayMode < 2.5)
                {
                    float3 a =
                        c1 *
                        c2 *
                        2;

                    float3 b =
                        1 -
                        (1-c1) *
                        (1-c2) *
                        2;

                    c =
                        lerp(
                            a,
                            b,
                            step(
                                0.5,
                                c1
                            )
                        );
                }
                else
                {
                    c =
                        1 -
                        (1-c1) *
                        (1-c2);
                }

                return
                    lerp(
                        c1,
                        c,
                        alpha
                    );
            }


            // ============================================================
            // FRAGMENT
            // ============================================================

            half4 frag(Varyings IN) : SV_Target
            {
                // --------------------------------------------------------
                // Actual PaintLayer
                // --------------------------------------------------------

                float4 paint =
                    SamplePaintUV(
                        IN.uv
                    );

                float originalAlpha =
                    paint.a;


                // --------------------------------------------------------
                // KinoAqua color
                // --------------------------------------------------------

                float3 watercolor =
                    ProcessAt(
                        IN.uv
                    );


                // --------------------------------------------------------
                // Preserve the actual paint color.
                //
                // This prevents KinoAqua from turning a freshly painted
                // red/green/blue stroke into mostly white.
                // --------------------------------------------------------

                float3 originalColor =
                    paint.rgb;

                watercolor =
                    lerp(
                        originalColor,
                        watercolor,
                        _Opacity
                    );


                // --------------------------------------------------------
                // Bleeding alpha
                // --------------------------------------------------------

                float2 resolution;
                float aspect;
                float aspectRcp;

                GetResolution(
                    resolution,
                    aspect,
                    aspectRcp
                );

                float2 texelSize =
                    1.0 /
                    resolution;

                float bleed =
                    GetBleedMask(
                        IN.uv,
                        texelSize
                    );

                float finalAlpha =
                    max(
                        originalAlpha,
                        bleed
                    );


                // --------------------------------------------------------
                // Overlay
                // --------------------------------------------------------

                float4 overlay =
                    SAMPLE_TEXTURE2D(
                        _OverlayTexture,
                        sampler_OverlayTexture,
                        IN.uv
                    );

                watercolor =
                    ApplyOverlay(
                        watercolor,
                        overlay.rgb,
                        overlay.a *
                        _OverlayOpacity
                    );


                // --------------------------------------------------------
                // Final
                // --------------------------------------------------------

                return
                    float4(
                        saturate(
                            watercolor
                        ),
                        saturate(
                            finalAlpha
                        )
                    )
                    *
                    IN.color;
            }

            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}