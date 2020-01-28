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

using IgnitionDX.Graphics;
using IgnitionDX.Math;

namespace IgnitionDX.SceneGraph
{
    public class ShadowMap
    {
        private string _shadowMapResourceName;
        private ConstantBuffer<Matrix4> _projectionMatrixBuffer = new ConstantBuffer<Matrix4>(Matrix4.Identity);

        public ViewFrustum CameraSliceFrustum { get; private set; }
        public ViewFrustum LightFrustum { get; private set; }
        public RenderTarget RenderTarget { get; private set; }

        public ShadowMap(IColorBuffer sharedColorBuffer, int index)
        {
            _shadowMapResourceName = "ShadowMap" + index;

            CameraSliceFrustum = new ViewFrustum();
            LightFrustum = new ViewFrustum();

            RenderTarget = new RenderTarget();
            RenderTarget.ColorBuffer.Add(sharedColorBuffer);
            RenderTarget.DepthStencilBuffer = new DepthStencilBuffer(sharedColorBuffer.Width, sharedColorBuffer.Height, 1, true);
        }

        private BoundingBox _frustumBounding = new BoundingBox();
        private BoundingBox _frustumMaxExtend = new BoundingBox();
        public Vector4 PrepareProjection(SceneNode root, Matrix4 camSliceFrustumProjMatrix, CameraNode cam, DirectionalLightNode light)
        {
            CameraSliceFrustum.ProjectionMatrix = camSliceFrustumProjMatrix;
            CameraSliceFrustum.GetBoundingBox(cam.GetWorldTransform(root) ?? Matrix4.Identity, light.GetInverseWorldTransform(root) ?? Matrix4.Identity, _frustumBounding);
            CameraSliceFrustum.GetBoundingBox(_frustumMaxExtend);
            float size = (_frustumMaxExtend.Max.Value - _frustumMaxExtend.Min.Value).Length;

            float worldUnitsPerTexel = size / RenderTarget.DepthStencilBuffer.Width;
            Vector3 min = _frustumBounding.Min.Value;

            min /= worldUnitsPerTexel;
            min = new Vector3((float)System.Math.Floor(min.X), (float)System.Math.Floor(min.Y), (float)System.Math.Floor(min.Z));
            min *= worldUnitsPerTexel;

            LightFrustum.ProjectionMatrix = Matrix4.OrthoOffCenterRH(min.X, min.X + size, min.Y, min.Y + size, light.NearPlane, light.FarPlane);

            Vector4 shadowOffsetAndScale = new Vector4(
                (-0.5f - min.X),
                (-0.5f + min.Y + size),
                1 / size,
                1 / size);

            return shadowOffsetAndScale;
        }

        public void BindProjection(Renderer renderer)
        {
            if (_projectionMatrixBuffer.Value != LightFrustum.ProjectionMatrix)
            {
                _projectionMatrixBuffer.Value = LightFrustum.ProjectionMatrix;
            }
            renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "CameraProjection", _projectionMatrixBuffer.GetBuffer(renderer));
        }

        public void BindShadowMap(Renderer renderer)
        {
            renderer.ActiveShaderProgram.BindShaderResource(renderer, _shadowMapResourceName, this.RenderTarget.DepthStencilBuffer.GetShaderResourceView(renderer));
        }
    }
}
