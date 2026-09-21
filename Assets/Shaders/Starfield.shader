Shader "TheGreatMarch/Starfield"
{
    Properties
    {
        [Header(Output)]
        _MainTex ("Sprite Texture (unused)", 2D) = "white" {}

        [Header(Size Distribution)]
        _TypicalSize ("Typical Size", Float) = 1.0
        _MinSize ("Min Size", Float) = 0.3
        _MaxSize ("Max Size", Float) = 2.4
        _SizeUpCost ("Size Up Cost", Range(0.1, 8)) = 2.2
        _SizeDownCost ("Size Down Cost", Range(0.1, 8)) = 2.2
        _SizeVariation ("Star Size Jitter", Range(0, 1)) = 0.3

        [Header(Field)]
        _Density ("Density", Range(0, 64)) = 34
        _Rarity ("Rarity (0 = every cell, 1 = almost none)", Range(0, 1)) = 0.3
        _Clumpiness ("Clumpiness", Range(0, 1)) = 0.72
        _ClumpScale ("Clump Scale", Range(0.1, 32)) = 2.4
        _Brightness ("Brightness", Range(0, 4)) = 1.2
        _StarBrightness ("Brightness Variation", Range(0, 1)) = 0.6
        _MinStarBrightness ("Min Star Brightness", Range(0, 1)) = 0.08
        _Softness ("Softness", Range(0.05, 2)) = 0.5

        [Header(Twinkle)]
        _Twinkle ("Twinkle", Range(0, 1)) = 0.15
        _TwinkleSpeed ("Twinkle Speed", Range(0, 20)) = 2.5

        [Header(Parallax Layers (based on camera world position))]
        _LayerCount ("Layer Count", Range(1, 4)) = 3
        _LayerDensity ("Layer Density", Vector) = (1.0, 0.7, 0.5, 0.35)
        _LayerParallax ("Layer Parallax", Vector) = (1.0, 0.5, 0.25, 0.12)
        _LayerSize ("Layer Size Scale", Vector) = (1.25, 0.95, 0.7, 0.5)
        _LayerBrightness ("Layer Brightness", Vector) = (1.0, 0.75, 0.55, 0.4)
        _Parallax ("Parallax Strength", Range(0, 4)) = 0.02
        _ParallaxSizeFalloff ("Parallax Size Falloff (0 = off, 1 = size tracks parallax)", Range(0, 1)) = 0.5
        _Seed ("Seed", Float) = 17
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            // Pre-baked by StarfieldController: a 1D lookup (256 entries) mapping a
            // uniform random [0,1) to a star colour through the mixture weights.
            // rgb = colour, a = per-type variance. One sample replaces the old
            // per-star weight walk (up to 32 texture reads per contributing cell).
            sampler2D _StarColorLUT;
            float _TypicalSize;
            float _MinSize;
            float _MaxSize;
            float _SizeUpCost;
            float _SizeDownCost;
            float _SizeVariation;

            float _Density;
            float _Rarity;
            float _Clumpiness;
            float _ClumpScale;
            float _Brightness;
            float _StarBrightness;
            float _MinStarBrightness;
            float _Softness;

            float _Twinkle;
            float _TwinkleSpeed;

            float _LayerCount;
            float _Parallax;
            float _ParallaxSizeFalloff;
            float _Seed;
            float4 _LayerDensity;
            float4 _LayerParallax;
            float4 _LayerSize;
            float4 _LayerBrightness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            // Hash without sine (Dave Hoskins). The classic frac(dot(...)) hash has
            // axis-aligned correlation artifacts that read as filaments; this one
            // stays visibly random.
            float hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Multi-octave value noise: breaks up the single-scale blobs that made
            // the clumping look structured.
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * valueNoise(p);
                    p = p * 2.03 + 17.13;
                    amplitude *= 0.5;
                }
                return value;
            }

            // Higher cost pushes stars towards the typical size (hard to be far from typical),
            // lower cost flattens the distribution towards the min/max extremes.
            float sampleSize(float r)
            {
                float down = step(r, 0.5);
                float u = lerp((1.0 - r) * 2.0, r * 2.0, down);
                float cost = lerp(_SizeUpCost, _SizeDownCost, down);
                float t = pow(saturate(u), max(cost, 0.0001));
                float extreme = lerp(_MaxSize, _MinSize, down);
                return lerp(_TypicalSize, extreme, t);
            }

            float3 sampleColor(float r, float grain, out float variance)
            {
                float4 col = tex2Dlod(_StarColorLUT, float4(saturate(r), 0.5, 0.0, 0.0));
                variance = col.a;

                float3 jitter = float3(
                    hash21(float2(grain, 11.0)),
                    hash21(float2(grain, 23.0)),
                    hash21(float2(grain, 37.0))
                ) - 0.5;
                return saturate(col.rgb + jitter * 2.0 * variance);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float aspect = max(_ScreenParams.x / max(_ScreenParams.y, 1.0), 0.001);
                float2 fieldUv = float2(i.uv.x * aspect, i.uv.y);
                float2 camPos = _WorldSpaceCameraPos.xy;
                float invScreenH = 1.0 / max(_ScreenParams.y, 1.0);

                float3 accum = float3(0.0, 0.0, 0.0);
                int layers = (int)clamp(_LayerCount, 1.0, 4.0);

                for (int L = 0; L < 4; L++)
                {
                    if (L >= layers) break;

                    float layer = (float)L;
                    float layerParallax = _LayerParallax[L];
                    float layerSize = _LayerSize[L];
                    float layerBrightness = _LayerBrightness[L];
                    float density = max(_Density * _LayerDensity[L], 0.0001);
                    // Never let a star fall below ~1 pixel, otherwise sub-pixel glows
                    // alias into moire line patterns.
                    float minSigma = density * 0.75 * invScreenH;
                    // Farther layers (lower parallax) shrink towards zero as the falloff
                    // rises: 0 keeps sizes independent of depth, 1 makes size
                    // proportional to parallax.
                    float parallaxSize = lerp(1.0, saturate(layerParallax), _ParallaxSizeFalloff);

                    float2 layerSeed = hash22(float2(layer, _Seed));

                    // Smooth parallax offset driven by the camera's world position.
                    float2 luv = fieldUv + layerSeed * 13.0 + camPos * (_Parallax * layerParallax);

                    // Clumping only masks which cells hold a star. The grid density
                    // stays constant: warping the lattice by density (the old version)
                    // produced caustic-like filaments of micro stars.
                    float clump = fbm(luv * _ClumpScale + layerSeed * 7.0);
                    // Never drops to zero: clumping should thin the field, not punch
                    // hard-edged holes in it.
                    float clumpMask = lerp(1.0, 0.35 + 1.65 * clump, _Clumpiness);
                    float existChance = (1.0 - _Rarity) * clumpMask;

                    float2 gv = luv * density;
                    float2 cell = floor(gv);
                    float2 local = frac(gv);

                    // Largest star this layer can produce. Any neighbour cell whose
                    // cell-square is farther than this can't reach the pixel, so we
                    // reject it before hashing or sampling a colour.
                    float maxSize = _MaxSize * layerSize * parallaxSize * (1.0 + _SizeVariation);
                    float maxSigma = max(maxSize * 0.03 * _Softness, minSigma);
                    float pruneDist = maxSigma * 4.0;
                    float pruneDist2 = pruneDist * pruneDist;

                    for (int y = -1; y <= 1; y++)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            float2 neighbor = float2((float)x, (float)y);

                            // Distance from this pixel to the unit square that owns the
                            // neighbour's stars; a lower bound on any star's distance.
                            float2 box = abs(local - (neighbor + 0.5)) - 0.5;
                            box = max(box, 0.0);
                            if (dot(box, box) > pruneDist2) continue;

                            float2 cseed = cell + neighbor + layerSeed * 31.0;
                            float2 h1 = hash22(cseed);

                            // Rarity = chance the cell actually holds a star; clumping
                            // further thins the field in sparse regions. The smoothstep
                            // keeps the transition soft.
                            float exists = smoothstep(0.0, 0.1, existChance - h1.x);
                            if (exists <= 0.001) continue;

                            float2 starPos = neighbor + hash22(cseed + 3.71);
                            float2 diff = local - starPos;

                            float2 h3 = hash22(cseed + 13.0);
                            float size = sampleSize(h1.y) * layerSize * parallaxSize;
                            size *= lerp(1.0 - _SizeVariation, 1.0 + _SizeVariation, h3.x);
                            size = max(size, 0.0001);

                            float sigma = max(size * 0.03 * _Softness, minSigma);
                            float d2 = dot(diff, diff);
                            float glow = exp(-d2 / (2.0 * sigma * sigma));
                            // Outside this radius the glow is below display precision.
                            if (glow <= 0.0005) continue;

                            float variance;
                            float3 col = sampleColor(h3.y, h3.y * 100.0 + layer, variance);

                            float2 h4 = hash22(cseed + 5.5);
                            float brightness = lerp(_MinStarBrightness, 1.0,
                                lerp(1.0 - _StarBrightness, 1.0, h4.x));
                            float twinkle = 1.0 - _Twinkle * 0.5 *
                                (1.0 - sin(_Time.y * _TwinkleSpeed + h4.y * 6.2831853));

                            float intensity = glow * brightness * layerBrightness * _Brightness * twinkle * exists;
                            accum += col * intensity;
                        }
                    }
                }

                accum *= i.color.rgb;
                return fixed4(accum, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
