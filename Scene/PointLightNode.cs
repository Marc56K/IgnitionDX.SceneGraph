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

using IgnitionDX.Math;

namespace IgnitionDX.SceneGraph
{
    public class PointLightNode : LightNode, Collada14.IDAEPointLightNode
    {
        public float ConstantAttenuation
        {
            get
            {
                return _parameters.ConstantAttenuation;
            }
            set
            {
                if (_parameters.ConstantAttenuation != value)
                {
                    _parameters.ConstantAttenuation = value;
                    _lightParamsBuffer.Value = _parameters;
                }
            }
        }

        public float LinearAttenuation
        {
            get
            {
                return _parameters.LinearAttenuation;
            }
            set
            {
                if (_parameters.LinearAttenuation != value)
                {
                    _parameters.LinearAttenuation = value;
                    _lightParamsBuffer.Value = _parameters;
                }
            }
        }

        public float QuadraticAttenuation
        {
            get
            {
                return _parameters.QuadraticAttenuation;
            }
            set
            {
                if (_parameters.QuadraticAttenuation != value)
                {
                    _parameters.QuadraticAttenuation = value;
                    _lightParamsBuffer.Value = _parameters;
                }
            }
        }

        public PointLightNode()
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

            _lightParamsBuffer.Value = _parameters;
        }

        public override void Bind(SceneRenderer renderer, SceneNode root)
        {
            renderer.ActiveLight = this;

            if (renderer.ActiveShaderProgram != null && renderer.ActiveCamera != null)
            {
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "LightParameters", _lightParamsBuffer.GetBuffer(renderer));

                Matrix4 worldTrans = this.GetWorldTransform(root) ?? Matrix4.Identity;
                Matrix4 invCamTrans = renderer.ActiveCamera.GetInverseWorldTransform(root) ?? Matrix4.Identity;

                Vector4 position = invCamTrans * worldTrans * new Vector4(0, 0, 0, 1);
                position /= position.W;
                if (_lightTransBuffer.Value.Position != position)
                {
                    _lightTransBuffer.Value = new LightTransformation() { Position = position };
                }
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "LightTransformation", _lightTransBuffer.GetBuffer(renderer));
            }
        }
    }
}
