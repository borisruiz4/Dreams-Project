﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'

Shader "Hidden/SEGITraceScene" {
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_EmissionColor("Color", Color) = (0,0,0)
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.333
	}
	SubShader 
	{
		Cull Off
		ZTest Always
		
		Pass
		{
			CGPROGRAM
			
				#pragma target 5.0
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom
				#include "UnityCG.cginc"
				
				#define PI 3.14159265
				
				RWTexture3D<uint> RG0;
				RWTexture3D<uint> BA0;
				
				sampler3D SEGIVolumeLevel0;
				sampler3D SEGIVolumeLevel1;
				sampler3D SEGIVolumeLevel2;
				sampler3D SEGIVolumeLevel3;
				sampler3D SEGIVolumeLevel4;
				sampler3D SEGIVolumeLevel5;
				sampler3D SEGIVolumeLevel6;
				sampler3D SEGIVolumeLevel7;
				
				float4x4 SEGIVoxelViewFront;
				float4x4 SEGIVoxelViewLeft;
				float4x4 SEGIVoxelViewTop;
				
				sampler2D _MainTex;
				float4 _MainTex_ST;
				half4 _EmissionColor;
				float _Cutoff;
				
				struct v2g
				{
					float4 pos : SV_POSITION;
					half4 uv : TEXCOORD0;
					float3 normal : TEXCOORD1;
					float angle : TEXCOORD2;
				};
				
				struct g2f
				{
					float4 pos : SV_POSITION;
					half4 uv : TEXCOORD0;
					float3 normal : TEXCOORD1;
					float angle : TEXCOORD2;
				};
				
				half4 _Color;
				float SEGISecondaryOcclusionStrength;
				
				v2g vert(appdata_full v)
				{
					v2g o;
					
					float4 vertex = v.vertex;
					
					o.normal = UnityObjectToWorldNormal(v.normal);
					float3 absNormal = abs(o.normal);
					
					o.pos = vertex;
					
					o.uv = float4(TRANSFORM_TEX(v.texcoord.xy, _MainTex), 1.0, 1.0);
					
					return o;
				}
				
				int SEGIVoxelResolution;
				
				[maxvertexcount(3)]
				void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
				{
					v2g p[3];
					for (int i = 0; i < 3; i++)
					{
						p[i] = input[i];
						p[i].pos = mul(unity_ObjectToWorld, p[i].pos);						
					}
					
					float3 realNormal = float3(0.0, 0.0, 0.0);
					
					float3 V = p[1].pos.xyz - p[0].pos.xyz;
					float3 W = p[2].pos.xyz - p[0].pos.xyz;
					
					realNormal.x = (V.y * W.z) - (V.z * W.y);
					realNormal.y = (V.z * W.x) - (V.x * W.z);
					realNormal.z = (V.x * W.y) - (V.y * W.x);
					
					float3 absNormal = abs(realNormal);
					

					
					int angle = 0;
					if (absNormal.z > absNormal.y && absNormal.z > absNormal.x)
					{
						angle = 0;
					}
					else if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
					{
						angle = 1;
					}
					else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
					{
						angle = 2;
					}
					else
					{
						angle = 0;
					}
					
					for (int i = 0; i < 3; i ++)
					{
						if (angle == 0)
						{
							p[i].pos = mul(SEGIVoxelViewFront, p[i].pos);					
						}
						else if (angle == 1)
						{
							p[i].pos = mul(SEGIVoxelViewLeft, p[i].pos);					
						}
						else
						{
							p[i].pos = mul(SEGIVoxelViewTop, p[i].pos);		
						}
						
						p[i].pos = mul(UNITY_MATRIX_P, p[i].pos);
						
						p[i].pos.z *= -1.0;
						
						p[i].angle = (float)angle;
					}
					
					triStream.Append(p[0]);
					triStream.Append(p[1]);
					triStream.Append(p[2]);
				}
				

				float4x4 SEGIVoxelToGIProjection;
				float4x4 SEGIVoxelProjectionInverse;
				sampler2D SEGIGIDepthNormalsTexture;
				float4 SEGISunlightVector;
				float4 GISunColor;
				int SEGIFrameSwitch;
				half4 SEGISkyColor;
				float SEGISoftSunlight;
				int SEGISecondaryCones;
				
				sampler3D SEGIVolumeTexture0;
				float SEGIVoxelScaleFactor;
				int SEGIVoxelAA;
				int SEGISphericalSkylight;


				#define VoxelResolution (SEGIVoxelResolution)
					
				float4 ConeTrace(float3 voxelOrigin, float3 kernel, float3 worldNormal)
				{
					float skyVisibility = 1.0;		
					
					float3 gi = float3(0,0,0);	
					
					int numSteps = (int)(7.0 * lerp(SEGIVoxelScaleFactor, 1.0, 0.5));	
					
					float3 adjustedKernel = normalize(kernel + worldNormal * 0.2);			
				
					for (int i = 0; i < numSteps; i++)
					{
						float fi = ((float)i) / numSteps;		
							fi = lerp(fi, 1.0, 0.06);
						
						float coneDistance = (exp2(fi * 4.0) - 0.9) / 8.0; 
										
						float coneSize = fi * 6.0 * lerp(SEGIVoxelScaleFactor, 1.0, 0.5); 
						float3 voxelCheckCoord = voxelOrigin.xyz + adjustedKernel.xyz * (coneDistance * 0.12  + 0.001);
						float4 sample = float4(0.0, 0.0, 0.0, 0.0);
						int mipLevel = floor(coneSize);
						if (mipLevel == 0)
							sample = tex3Dlod(SEGIVolumeLevel1, float4(voxelCheckCoord.xyz, coneSize));
						else if (mipLevel == 1)
							sample = tex3Dlod(SEGIVolumeLevel1, float4(voxelCheckCoord.xyz, coneSize));
						else if (mipLevel == 2)
							sample = tex3Dlod(SEGIVolumeLevel2, float4(voxelCheckCoord.xyz, coneSize));
						else if (mipLevel == 3)
							sample = tex3Dlod(SEGIVolumeLevel3, float4(voxelCheckCoord.xyz, coneSize));
						else if (mipLevel == 4)
							sample = tex3Dlod(SEGIVolumeLevel4, float4(voxelCheckCoord.xyz, coneSize));
						else if (mipLevel == 5)
							sample = tex3Dlod(SEGIVolumeLevel5, float4(voxelCheckCoord.xyz, coneSize));
						else
							sample = float4(1,1,1,0);
						
						float occlusion = skyVisibility * skyVisibility * skyVisibility;
						
						float falloffFix = pow(fi, 1.0) * 4.0 + 0.0456;
						
						gi.rgb += sample.rgb * (coneSize * 1.0 + 1.0) * occlusion * falloffFix;

						sample.a *= SEGISecondaryOcclusionStrength;
						
						skyVisibility *= pow(saturate(1.0 - (sample.a) * (coneSize * 0.2 + 1.0 + coneSize * coneSize * 0.08)), lerp(0.014, 1.0, min(1.0, coneSize / 5.0)));
						
					}
					
					skyVisibility *= saturate(dot(worldNormal, kernel));
					skyVisibility *= lerp(saturate(dot(kernel, float3(0.0, 1.0, 0.0)) * 10.0), 1.0, SEGISphericalSkylight);

					gi *= saturate(dot(worldNormal, kernel));
					
					float3 skyColor = float3(0.0, 0.0, 0.0);
					
					float upGradient = saturate(dot(kernel, float3(0.0, 1.0, 0.0)));
					float sunGradient = saturate(dot(kernel, -SEGISunlightVector.xyz));
					skyColor += lerp(SEGISkyColor.rgb * 2.0, SEGISkyColor.rgb, pow(upGradient, (0.5).xxx));
					skyColor += GISunColor.rgb * pow(sunGradient, (4.0).xxx) * SEGISoftSunlight;
					
					gi *= 0.5;
					
					gi += skyColor * skyVisibility;
					
					return float4(gi.rgb * 0.8, 0.0f);
				}
				
				float2 rand(float3 coord)
				{
					float noiseX = saturate(frac(sin(dot(coord, float3(12.9898, 78.223, 35.3820))) * 43758.5453));
					float noiseY = saturate(frac(sin(dot(coord, float3(12.9898, 78.223, 35.2879)*2.0)) * 43758.5453));
					
					return float2(noiseX, noiseY);
				}
				
				
				uint FloatsToInt(float2 value)
				{
					uint int1 = f32tof16(value.x);
					uint int2 = f32tof16(value.y) * 0x00010000;

					return int1 + int2;
				}

				float2 IntToFloats(uint intval)
				{
					float value1 = f16tof32(intval);
					float value2 = f16tof32(intval / 0x0000FFFF);
					return float2(value1, value2);
				}

				void interlockedAddFloat2(RWTexture3D<uint> destination, int3 coord, float2 value)
				{
					uint writeValue = FloatsToInt(value);
					uint compareValue = 0;
					uint originalValue;

					[allow_uav_condition] for (int i = 0; i < 12; i++)
					{
						InterlockedCompareExchange(destination[coord], compareValue, writeValue, originalValue);
						if (compareValue == originalValue)
							break;
						compareValue = originalValue;
						float2 originalValueFloats = IntToFloats(originalValue);
						writeValue = FloatsToInt(originalValueFloats + value);
					}
				}
				
				
				float4 frag (g2f input) : SV_TARGET
				{
					int3 coord = int3((int)(input.pos.x), (int)(input.pos.y), (int)(input.pos.z * VoxelResolution));
					
					int angle = 0;
					
					angle = (int)input.angle;
					
					if (angle == 1)
					{
						coord.xyz = coord.zyx;
						coord.z = VoxelResolution - coord.z - 1;
					}
					else if (angle == 2)
					{
						coord.xyz = coord.xzy;
						coord.y = VoxelResolution - coord.y - 1;
					}
					
					float3 fcoord = (float3)coord.xyz / VoxelResolution;
					
					float3 gi = (0.0).xxx;
					
					float3 worldNormal = input.normal;
					
					float3 voxelOrigin = (fcoord + worldNormal.xyz * 0.006 * 1.0);
					
					float4 traceResult = float4(0,0,0,0);
					
					float2 dither = rand(fcoord);
					
					const float phi = 1.618033988;
					const float gAngle = phi * PI * 2.0;
					
					
					const int numSamples = SEGISecondaryCones;
					for (int i = 0; i < numSamples; i++)
					{
						float fi = (float)i; 
						float fiN = fi / numSamples;
						float longitude = gAngle * fi;
						float latitude = asin(fiN * 2.0 - 1.0);
						
						float3 kernel;
						kernel.x = cos(latitude) * cos(longitude);
						kernel.z = cos(latitude) * sin(longitude);
						kernel.y = sin(latitude);
						
						kernel = normalize(kernel + worldNormal.xyz);

						if (i == 0)
						{
							kernel = float3(0.0, 1.0, 0.0);
						}

							traceResult += ConeTrace(voxelOrigin.xyz, kernel.xyz, worldNormal.xyz);
					}
					
					traceResult /= numSamples;
					
					
					gi.rgb = traceResult.rgb;
					
					gi.rgb *= 2.3;
					
					gi.rgb += traceResult.a * 1.0 * SEGISkyColor;
					
					
					float4 result = float4(gi.rgb, 1.0);


					interlockedAddFloat2(RG0, coord, result.rg);
					interlockedAddFloat2(BA0, coord, result.ba);
					
					return float4(0.0, 0.0, 0.0, 0.0);
				}
			
			ENDCG
		}
	} 
	FallBack Off
}
