﻿/* MIT License (MIT)
 *
 * Copyright (c) 2020 Marc Roßbach
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

cbuffer CameraProjection
{
	matrix ProjectionMatrix;
}

cbuffer CameraTransformation
{
	matrix ViewMatrix;
}

cbuffer ModelTransformation
{
	matrix ModelMatrix;
}

cbuffer ModelViewTransformation
{
	matrix ModelViewMatrix;
	matrix NormalMatrix;
}

cbuffer LightParameters
{
    float4 LightDiffuseColor;
    float4 LightSpecularColor;
    float LightIntensity;
    float LightConstantAttenuation;
    float LightLinearAttenuation;
    float LightQuadraticAttenuation;
    float LightSpotExponent;
    float LightSpotCutOff;
    float LightSpotCosCutOff;
    bool LightCastShadow;
}

cbuffer LightTransformation
{
    float4 LightPosition;
    matrix LightViewProjectionMatrix;
    float3 LightSpotDirection;
}

struct VS_INPUT
{
	float3 Vertex : VERTEX;
	float3 TexCoord : TEXCOORD;
	float3 Normal : NORMAL;
	float3 TexTangent : TEXTANGENT;
	float3 TexBinormal : TEXBINORMAL;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD;
	float3 Normal : NORMAL;
	float3 EyeVector : EYEVECTOR;
	float3 LightDir : LIGHTDIR;
	float Attenuation : ATTENUATION;
	float4 ShadowCoord : SHADOWCOORD;
};

PS_INPUT main( VS_INPUT input )
{
    PS_INPUT result;
	float4 pos = float4(input.Vertex.xyz, 1);
	matrix modelView = ModelViewMatrix;
	matrix modelViewProj = mul(ProjectionMatrix, modelView);
	float3x3 normalMatrix = (float3x3)NormalMatrix;
	
	result.Position = mul(modelViewProj, pos);
	result.TexCoord = input.TexCoord;
	result.Normal = normalize(mul(normalMatrix, input.Normal));

	float4 v = mul(modelView, pos);
	result.EyeVector = -v.xyz;
	
	if (LightPosition.w > 0.0)
	{
		// point light or spot light
		result.LightDir = LightPosition.xyz - v.xyz;
		float dist = length(result.LightDir);
		result.Attenuation = 1.0 / (LightQuadraticAttenuation * dist * dist + LightLinearAttenuation * dist + LightConstantAttenuation);
	}
	else
    {
		// directional light
		result.LightDir = -LightPosition.xyz;
		result.Attenuation = 1.0;
    }

	if(LightCastShadow)
	{
		// Moving from [-1,1] to [0,1]
		matrix DCtoTC = matrix(	0.5, 0.0, 0.0, 0.5,
								0.0, -0.5, 0.0, 0.5,
								0.0, 0.0, 1, 0.0,
								0.0, 0.0, 0.0, 1.0);

		matrix shadowTrans = mul(DCtoTC, LightViewProjectionMatrix);
		shadowTrans = mul(shadowTrans, ModelMatrix);
		result.ShadowCoord = mul(shadowTrans, pos);
	}

	return result;
}