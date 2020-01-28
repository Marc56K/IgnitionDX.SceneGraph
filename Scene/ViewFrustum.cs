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
    public class ViewFrustum
    {
        private Matrix4 _projectionMatrix = Matrix4.Identity;
        private Vector4[] _planes = new Vector4[6];
        private Vector4[] _points = new Vector4[8];

        public Matrix4 ProjectionMatrix
        {
            get
            {
                return _projectionMatrix;
            }
            set
            {
                if (_projectionMatrix != value)
                {
                    _projectionMatrix = value;
                    Update();
                }
            }
        }

        public Vector4[] FrustumPoints
        {
            get { return _points; }
        }

        public ViewFrustum()
        {
        }

        private void Update()
        {
            Matrix4 m = _projectionMatrix.Transpose;

            // near plane
            float a = m[3] + m[2];
            float b = m[7] + m[6];
            float c = m[11] + m[10];
            float d = m[15] + m[14];
            Vector3 abc = new Vector3(a, b, c);
            _planes[0] = new Vector4(abc.Normalized, d / abc.Length);

            // far plane
            a = m[3] - m[2];
            b = m[7] - m[6];
            c = m[11] - m[10];
            d = m[15] - m[14];
            abc = new Vector3(a, b, c);
            _planes[1] = new Vector4(abc.Normalized, d / abc.Length);

            // left plane
            a = m[3] + m[0];
            b = m[7] + m[4];
            c = m[11] + m[8];
            d = m[15] + m[12];
            abc = new Vector3(a, b, c);
            _planes[2] = new Vector4(abc.Normalized, d / abc.Length);

            // right plane
            a = m[3] - m[0];
            b = m[7] - m[4];
            c = m[11] - m[8];
            d = m[15] - m[12];
            abc = new Vector3(a, b, c);
            _planes[3] = new Vector4(abc.Normalized, d / abc.Length);

            // bottom plane
            a = m[3] + m[1];
            b = m[7] + m[5];
            c = m[11] + m[9];
            d = m[15] + m[13];
            abc = new Vector3(a, b, c);
            _planes[4] = new Vector4(abc.Normalized, d / abc.Length);

            // top plane
            a = m[3] - m[1];
            b = m[7] - m[5];
            c = m[11] - m[9];
            d = m[15] - m[13];
            abc = new Vector3(a, b, c);
            _planes[5] = new Vector4(abc.Normalized, d / abc.Length);

            Matrix4 invProj = _projectionMatrix.Inverse;
            _points[0] = (invProj * new Vector4(-1, -1, 1, 1)).DivW;
            _points[1] = (invProj * new Vector4(1, -1, 1, 1)).DivW;
            _points[2] = (invProj * new Vector4(-1, 1, 1, 1)).DivW;
            _points[3] = (invProj * new Vector4(1, 1, 1, 1)).DivW;
            _points[4] = (invProj * new Vector4(-1, -1, 0, 1)).DivW;
            _points[5] = (invProj * new Vector4(1, -1, 0, 1)).DivW;
            _points[6] = (invProj * new Vector4(1, 1, 0, 1)).DivW;
            _points[7] = (invProj * new Vector4(-1, 1, 0, 1)).DivW;
        }

        public void GetBoundingBox(Matrix4 camWorldTrans, Matrix4 targetTrans, BoundingBox result)
        {
            result.Min = null;
            result.Max = null;
            Matrix4 trans = targetTrans * camWorldTrans;
            for (int i = 0; i < _points.Length; i++)
            {
                Vector4 p = (trans * _points[i]).DivW;
                result.Grow(p.XYZ);
            }
        }

        public void GetBoundingBox(BoundingBox result)
        {
            result.Min = null;
            result.Max = null;
            for (int i = 0; i < _points.Length; i++)
            {
                result.Grow(_points[i].XYZ);
            }
        }

        public bool IsInViewFrustum(Matrix4 modelViewTrans, BoundingBox bBox)
        {
            if (!bBox.IsValid)
                return true;

            Vector4 center = new Vector4(bBox.Center.Value, 1);
            center = modelViewTrans * center;
            center /= center.W;

            Vector4 max = new Vector4(bBox.Max.Value, 1);
            max = modelViewTrans * max;
            max /= max.W;

            float radius = (center - max).Length;
            for (int i = 0; i < _planes.Length; i++)
            {
                if (_planes[i].Dot(center) + radius < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
