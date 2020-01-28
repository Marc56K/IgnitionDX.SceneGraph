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
using System.Runtime.InteropServices;

namespace IgnitionDX.SceneGraph
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LigthParameters
    {
        public Color4 DiffuseColor;
        public Color4 SpecularColor;
        public float Intensity;
        public float ConstantAttenuation;
        public float LinearAttenuation;
        public float QuadraticAttenuation;
        public float SpotExponent;
        public float SpotCutOff;
        public float SpotCosCutOff;
        public bool CastShadow;
        public float ShadowBias;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LightTransformation
    {
        public Vector4 Position;
        public Matrix4 ViewProjection;
        public Vector3 SpotDirection;
    }

    public abstract class LightNode : SceneNode
    {
        protected ConstantBuffer<LigthParameters> _lightParamsBuffer = new ConstantBuffer<LigthParameters>(new LigthParameters());
        protected ConstantBuffer<LightTransformation> _lightTransBuffer = new ConstantBuffer<LightTransformation>(new LightTransformation());
        protected LigthParameters _parameters = new LigthParameters();

        public Color4 Color
        {
            set
            {
                DiffuseColor = value;
                SpecularColor = value;
            }
        }

        public Color4 DiffuseColor
        {
            get
            {
                return _parameters.DiffuseColor;
            }
            set
            {
                if (_parameters.DiffuseColor != value)
                {
                    _parameters.DiffuseColor = value;
                    _lightParamsBuffer.Value = _parameters;
                }
            }
        }

        public Color4 SpecularColor
        {
            get
            {
                return _parameters.SpecularColor;
            }
            set
            {
                if (_parameters.SpecularColor != value)
                {
                    _parameters.SpecularColor = value;
                    _lightParamsBuffer.Value = _parameters;
                }
            }
        }

        public float Intensity
        {
            get
            {
                return _parameters.Intensity;
            }
            set
            {
                if (_parameters.Intensity != value)
                {
                    _parameters.Intensity = value;
                    _lightParamsBuffer.Value = _parameters;
                }
            }
        }

        public override bool Accept(ITraverser traverser)
        {
            return traverser.Visit(this);
        }

        public override void Preload(Renderer renderer)
        {
            _lightParamsBuffer.Preload(renderer);
            _lightTransBuffer.Preload(renderer);
        }

        public abstract void Bind(SceneRenderer renderer, SceneNode root);

        public override BoundingBox BoundingBox
        {
            get { return null; }
        }
    }
}
