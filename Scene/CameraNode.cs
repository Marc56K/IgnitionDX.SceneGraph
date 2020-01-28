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
    public abstract class CameraNode : SceneNode, ICameraNode
    {
        protected ConstantBuffer<Matrix4> _projectionMatrixBuffer = new ConstantBuffer<Matrix4>(Matrix4.Identity);
        protected Matrix4 _projectionMatrix = Matrix4.Identity;
        protected float _nearPlane = 0.1f;
        protected float _farPlane = 2000.0f;
        protected bool _useVfc = true;
        protected ViewFrustum _viewFrustum = new ViewFrustum();

        public ViewFrustum ViewFrustum
        {
            get { return _viewFrustum; }
        }

        public bool UseViewFrustumCulling
        {
            get { return _useVfc; }
            set { _useVfc = value; }
        }

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

        public void Bind(SceneRenderer renderer, SceneNode root)
        {
            renderer.ActiveCamera = this;

            if (renderer.ActiveShaderProgram != null)
            {
                // upload projection matrix
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "CameraProjection", _projectionMatrixBuffer.GetBuffer(renderer));

                // upload view matrix
                IConstantBuffer trans = this.GetInverseWorldTransformBuffer(root);
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "CameraTransformation", trans.GetBuffer(renderer));
            }
        }

        public override bool Accept(ITraverser traverser)
        {
            return traverser.Visit(this);
        }

        public override void Preload(Renderer renderer)
        {
            _projectionMatrixBuffer.Preload(renderer);
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

            return this._viewFrustum.IsInViewFrustum(modelViewTrans, node.BoundingBox);
        }

        public override BoundingBox BoundingBox
        {
            get { return null; }
        }

        protected abstract void UpdateProjectionMatrix();
    }
}
