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
    public class PerspectiveCameraNode : CameraNode
    {
        private float _fov = 1.05f;
        private float _aspect = 4.0f / 3.0f;

        public float Fov
        {
            get
            {
                return _fov;
            }
            set
            {
                if (_fov != value)
                {
                    _fov = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }

        public float Aspect
        {
            get
            {
                return _aspect;
            }
            set
            {
                if (_aspect != value)
                {
                    _aspect = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }

        public PerspectiveCameraNode()
        {
            this.UpdateProjectionMatrix();
        }

        protected override void UpdateProjectionMatrix()
        {
            _projectionMatrix = Matrix4.PerspectiveFovRH(_fov, _aspect, _nearPlane, _farPlane);
            _viewFrustum.ProjectionMatrix = _projectionMatrix;
            _projectionMatrixBuffer.Value = _projectionMatrix;
        }
    }
}
