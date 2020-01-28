/* MIT License (MIT)
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

cbuffer BoundingBoxTransformation
{
	matrix BoundingBoxMatrix;
}

struct VS_INPUT
{
	float3 Vertex : VERTEX;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
};

PS_INPUT main( VS_INPUT input )
{
    PS_INPUT result;
	float4 pos = float4(input.Vertex.xyz, 1);
	matrix modelView = mul(ViewMatrix, mul(ModelMatrix, BoundingBoxMatrix));
	matrix modelViewProj = mul(ProjectionMatrix, modelView);
	
	result.Position = mul(modelViewProj, pos);

	return result;
}