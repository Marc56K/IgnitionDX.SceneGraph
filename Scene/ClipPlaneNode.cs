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
    public struct ClipPlaneParameters
    {
        public Matrix4 ClipPlaneWorldMatrix;
        public bool UseClipPlane;
    }

    public class ClipPlaneNode : SceneNode
    {
        private ConstantBuffer<ClipPlaneParameters> _cbuffer = new ConstantBuffer<ClipPlaneParameters>(new ClipPlaneParameters());

        public void Bind(SceneRenderer renderer, SceneNode root)
        {
            if (renderer.ActiveShaderProgram != null)
            {
                ClipPlaneParameters parameters = new ClipPlaneParameters();
                parameters.ClipPlaneWorldMatrix = this.GetWorldTransform(root).Value;
                parameters.UseClipPlane = true;
                _cbuffer.Value = parameters;

                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "ClipPlaneParameters", _cbuffer.GetBuffer(renderer));
            }
        }

        public override BoundingBox BoundingBox
        {
            get { return null; }
        }

        public override bool Accept(ITraverser traverser)
        {
            return traverser.Visit(this);
        }

        public override void Preload(Renderer renderer)
        {
            _cbuffer.Preload(renderer);
        }
    }
}
