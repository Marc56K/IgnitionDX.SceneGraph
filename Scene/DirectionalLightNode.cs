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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace IgnitionDX.SceneGraph
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ShadowParameters
    {
        public Vector4 ShadowOffsetAndScale0;
        public Vector4 ShadowOffsetAndScale1;
        public Vector4 ShadowOffsetAndScale2;
        public int ShadowLevels;

        public void SetShadowOffsetAndScale(int idx, Vector4 value)
        {
            switch (idx)
            {
                case 0:
                    ShadowOffsetAndScale0 = value;
                    break;
                case 1:
                    ShadowOffsetAndScale1 = value;
                    break;
                case 2:
                    ShadowOffsetAndScale2 = value;
                    break;
            }
        }

        public Vector4 GetShadowOffsetAndScale(int idx)
        {
            switch (idx)
            {
                case 0:
                    return ShadowOffsetAndScale0;
                case 1:
                    return ShadowOffsetAndScale1;
                case 2:
                    return ShadowOffsetAndScale2;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool operator ==(ShadowParameters left, ShadowParameters right)
        {
            return
                (left.ShadowOffsetAndScale0 == right.ShadowOffsetAndScale0) &&
                (left.ShadowOffsetAndScale1 == right.ShadowOffsetAndScale1) &&
                (left.ShadowOffsetAndScale2 == right.ShadowOffsetAndScale2) &&
                (left.ShadowLevels == right.ShadowLevels);
        }

        public static bool operator !=(ShadowParameters left, ShadowParameters right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return ShadowOffsetAndScale0.GetHashCode() ^ ShadowOffsetAndScale1.GetHashCode() ^ ShadowOffsetAndScale2.GetHashCode() ^ ShadowLevels.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ShadowParameters)
            {
                return this == (ShadowParameters)obj;
            }

            return false;
        }
    }

    public class DirectionalLightNode : LightNode, ICameraNode, Collada14.IDAEDirectionalLightNode
    {
        private List<ShadowMap> _shadowCascades = new List<ShadowMap>();
        private ShadowParameters _shadowParameters = new ShadowParameters();
        private ConstantBuffer<ShadowParameters> _shadowParametersBuffer = new ConstantBuffer<ShadowParameters>(new ShadowParameters());
        private int _currentShadowMapIdx = 0;

        private List<string> _shaderResourceNames = new List<string>();
        private BoundingBox _frustumBounding = new BoundingBox();
        private BoundingBox _frustumMaxExtend = new BoundingBox();

        private Matrix4 _projectionMatrix = Matrix4.Identity;

        private bool _useVfc = true;
        public bool UseViewFrustumCulling
        {
            get { return _useVfc; }
            set { _useVfc = value; }
        }

        private float _nearPlane = 0.1f;
        public float NearPlane
        {
            get
            {
                return _nearPlane;
            }
            set
            {
                if (_nearPlane != value)
                {
                    _nearPlane = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }

        private float _farPlane = 2000.0f;
        public float FarPlane
        {
            get
            {
                return _farPlane;
            }
            set
            {
                if (_farPlane != value)
                {
                    _farPlane = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }

        public Matrix4 ProjectionMatrix
        {
            get
            {
                return _projectionMatrix;
            }
        }

        public float ShadowBias
        {
            get
            {
                return _parameters.ShadowBias;
            }
            set
            {
                if (_parameters.ShadowBias != value)
                {
                    _parameters.ShadowBias = value;
                    _lightParamsBuffer.Value = _parameters;
                }
            }
        }

        public bool CastShadow
        {
            get
            {
                return _parameters.CastShadow;
            }
            set
            {
                if (_parameters.CastShadow != value)
                {
                    _parameters.CastShadow = value;
                    _lightParamsBuffer.Value = _parameters;
                }
            }
        }

        public int NumShadowMaps
        {
            get
            {
                return _shadowParameters.ShadowLevels;
            }
            set
            {
                if (_shadowParameters.ShadowLevels != value)
                {
                    if (value < 1 || value > 3)
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    _shadowParameters.ShadowLevels = value;
                    UpdateShadowMaps();
                }
            }
        }

        private int _shadowMapSize = 1024;
        public int ShadowMapSize
        {
            get
            {
                return _shadowMapSize;
            }
            set
            {
                if (_shadowMapSize != value)
                {
                    _shadowMapSize = value;
                    UpdateShadowMaps();
                }
            }
        }

        public DirectionalLightNode()
        {
            _parameters.DiffuseColor = new Color4(1, 1, 1, 1);
            _parameters.SpecularColor = new Color4(1, 1, 1, 1);
            _parameters.Intensity = 1;
            _parameters.ConstantAttenuation = 1;
            _parameters.LinearAttenuation = 0;
            _parameters.QuadraticAttenuation = 0;
            _parameters.SpotExponent = 1;
            _parameters.SpotCutOff = 0;
            _parameters.SpotCosCutOff = 0;
            _parameters.CastShadow = false;
            _parameters.ShadowBias = 0.0001f;
            _lightParamsBuffer.Value = _parameters;

            this.UpdateProjectionMatrix();

            _shadowParameters.ShadowLevels = 3;
            this.UpdateShadowMaps();
        }

        public override void Bind(SceneRenderer renderer, SceneNode root)
        {
            renderer.ActiveLight = this;

            if (renderer.ActiveShaderProgram != null && renderer.ActiveCamera != null)
            {
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "LightParameters", _lightParamsBuffer.GetBuffer(renderer));

                Matrix4 worldTrans = this.GetWorldTransform(root) ?? Matrix4.Identity;
                Matrix4 invCamTrans = renderer.ActiveCamera.GetInverseWorldTransform(root) ?? Matrix4.Identity;
                Matrix4 finalTrans = invCamTrans * worldTrans;

                Vector4 position = finalTrans * new Vector4(0, 0, -1, 0);
                Matrix4 viewProj = _projectionMatrix * GetInverseWorldTransform(root) ?? Matrix4.Identity;
                if (_lightTransBuffer.Value.Position != position || _lightTransBuffer.Value.ViewProjection != viewProj)
                {
                    _lightTransBuffer.Value = new LightTransformation()
                    {
                        Position = position,
                        ViewProjection = viewProj,
                    };
                }
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "LightTransformation", _lightTransBuffer.GetBuffer(renderer));

                if (this.CastShadow)
                {
                    if (_shadowParametersBuffer.Value != _shadowParameters)
                    {
                        _shadowParametersBuffer.Value = _shadowParameters;
                    }
                    renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "LightShadowParameters", _shadowParametersBuffer.GetBuffer(renderer));

                    for (int i = 0; i < this._shadowCascades.Count; i++)
                    {
                        _shadowCascades[i].BindShadowMap(renderer);
                    }
                }
            }
        }

        private Matrix4 GetSliceFrustumProjection(int shadowMapIdx, PerspectiveCameraNode cam)
        {
            float nearPlane = cam.FarPlane - cam.NearPlane;
            if (shadowMapIdx > 0)
            {
                for (int i = _shadowCascades.Count - 1; i > shadowMapIdx - 1; i--)
                {
                    nearPlane /= 4f;
                }
            }
            else
            {
                nearPlane = cam.NearPlane;
            }

            float farPlane = cam.FarPlane - cam.NearPlane;
            for (int i = _shadowCascades.Count - 1; i > shadowMapIdx; i--)
            {
                farPlane /= 4f;
            }

            return Matrix4.PerspectiveFovRH(cam.Fov, cam.Aspect, nearPlane, farPlane);
        }

        public void PrepareProjection(SceneNode root, PerspectiveCameraNode cam, int shadowMapIdx)
        {
            Matrix4 proj = GetSliceFrustumProjection(shadowMapIdx, cam);
            Vector4 shadowOffsetAndScale = _shadowCascades[shadowMapIdx].PrepareProjection(root, proj, cam, this);
            _shadowParameters.SetShadowOffsetAndScale(shadowMapIdx, shadowOffsetAndScale);
        }

        public void BindLightCamera(SceneRenderer renderer, SceneNode root, int shadowMapIdx)
        {
            renderer.ActiveCamera = this;
            _currentShadowMapIdx = shadowMapIdx;

            if (renderer.ActiveShaderProgram != null)
            {
                // upload projection matrix
                _shadowCascades[_currentShadowMapIdx].BindProjection(renderer);

                // upload view matrix
                IConstantBuffer trans = this.GetInverseWorldTransformBuffer(root);
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "CameraTransformation", trans.GetBuffer(renderer));
            }
        }

        public bool IsInViewFrustum(SceneNode node, SceneNode root)
        {
            if (!UseViewFrustumCulling || node.BoundingBox == null || !node.BoundingBox.IsValid)
            {
                return true;
            }

            Matrix4? invCamTrans = this.GetInverseWorldTransform(root);
            Matrix4? worldTrans = node.GetWorldTransform(root);

            if (!invCamTrans.HasValue || !worldTrans.HasValue)
            {
                return false;
            }

            Matrix4 modelViewTrans = invCamTrans.Value * worldTrans.Value;

            return _shadowCascades[_currentShadowMapIdx].LightFrustum.IsInViewFrustum(modelViewTrans, node.BoundingBox);
        }

        public RenderTarget GetShadowMapRenderTarget(int idx)
        {
            return _shadowCascades[idx].RenderTarget;
        }

        private void UpdateProjectionMatrix()
        {
            _projectionMatrix = Matrix4.OrthoRH(1, 1, _nearPlane, _farPlane);
        }

        private void UpdateShadowMaps()
        {
            IColorBuffer colorBuffer = null;
            if ((_shadowCascades.Count > 0 && _shadowCascades[0].RenderTarget.DepthStencilBuffer.Width != this.ShadowMapSize) || _shadowCascades.Count == 0)
            {
                colorBuffer = new TextureColorBuffer(SharpDX.DXGI.Format.R8_UNorm, this.ShadowMapSize, this.ShadowMapSize, 1, false);
                _shadowCascades.Clear();
            }
            else if (_shadowCascades.Count != this.NumShadowMaps)
            {
                colorBuffer = _shadowCascades[0].RenderTarget.ColorBuffer[0];
                while (this.NumShadowMaps < _shadowCascades.Count)
                {
                    _shadowCascades.RemoveAt(_shadowCascades.Count - 1);
                }
            }

            while (this.NumShadowMaps > _shadowCascades.Count)
            {
                _shadowCascades.Add(new ShadowMap(colorBuffer, _shadowCascades.Count));
            }
        }
    }
}
