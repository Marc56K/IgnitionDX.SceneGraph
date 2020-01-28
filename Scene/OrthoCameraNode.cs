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
    public class OrthoCameraNode : CameraNode
    {
        private float _width;
        private float _height;
        private float _zoom = 1;

        public float Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }

        public float ActualWidth
        {
            get { return _width - _zoom * _width; }
        }

        public float Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }

        public float ActualHeight
        {
            get { return _height - _zoom * _height; }
        }

        public float Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                if (_zoom != value)
                {
                    _zoom = System.Math.Min(1, System.Math.Max(0, value));
                    this.UpdateProjectionMatrix();
                }
            }
        }

        public OrthoCameraNode()
        {
            this.UpdateProjectionMatrix();
        }

        protected override void UpdateProjectionMatrix()
        {
            _projectionMatrix = Matrix4.OrthoRH(ActualWidth, ActualHeight, _nearPlane, _farPlane);
            _viewFrustum.ProjectionMatrix = _projectionMatrix;
            _projectionMatrixBuffer.Value = _projectionMatrix;
        }
    }
}
