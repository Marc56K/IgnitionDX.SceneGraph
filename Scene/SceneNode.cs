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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace IgnitionDX.SceneGraph
{
    public abstract class SceneNode : Collada14.IDAESceneNode
    {
        [StructLayout(LayoutKind.Sequential)]
        protected struct ModelViewTrans
        {
            public Matrix4 ModelViewMatrix;
            public Matrix4 NormalMatrix;
        }
        
        /// <summary>
        /// The local transformation of this scene node.
        /// </summary>
        private Matrix4 _localTransform = Matrix4.Identity;

        /// <summary>
        /// The world transformations for every known root node.
        /// </summary>
        private Dictionary<SceneNode, Matrix4> _cachedWorldTransforms = new Dictionary<SceneNode, Matrix4>();
        private Dictionary<SceneNode, Matrix4> _inverseWorldTransforms = new Dictionary<SceneNode, Matrix4>();
        private Dictionary<SceneNode, ConstantBuffer<Matrix4>> _worldTransformBuffers = new Dictionary<SceneNode, ConstantBuffer<Matrix4>>();
        private Dictionary<SceneNode, ConstantBuffer<Matrix4>> _inverseWorldTransformBuffers = new Dictionary<SceneNode, ConstantBuffer<Matrix4>>();
        private Dictionary<SceneNode, Dictionary<ICameraNode, ConstantBuffer<ModelViewTrans>>> _modelViewTransformBuffers = new Dictionary<SceneNode, Dictionary<ICameraNode, ConstantBuffer<ModelViewTrans>>>();
        private ConstantBuffer<Matrix4> _boundingBoxTransformBuffer = null;

        /// <summary>
        /// The parent of this scene node.
        /// </summary>
        private SceneNode _parentNode;

        private bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public string Name { get; set; }

        public SceneNode()
        {
            Name = this.GetType().Name;
        }

        public abstract BoundingBox BoundingBox { get; }

        internal virtual void InvalidateBoundingBox()
        {
            if (this.ParentNode != null)
            {
                this.ParentNode.InvalidateBoundingBox();
            }
        }

        public void RenderBoundingBox(Renderer renderer, SceneNode root, IGeometry boundingBoxMesh)
        {
            if (this.BoundingBox != null && this.BoundingBox.IsValid && renderer.ActiveShaderProgram != null)
            {
                // upload world transformation
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "ModelTransformation", this.GetWorldTransformBuffer(root).GetBuffer(renderer));

                // upload local transformation
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "BoundingBoxTransformation", this.GetBoundingBoxTransformBuffer().GetBuffer(renderer));

                // render the geometry
                boundingBoxMesh.Render(renderer);
            }
        }

        public Matrix4 LocalTransform
        {
            get
            {
                return _localTransform;
            }
            set
            {
                if (value != _localTransform)
                {
                    _localTransform = value;
                    InvalidateWorldTransforms();
                    if (this.ParentNode != null)
                    {
                        this.ParentNode.InvalidateBoundingBox();
                    }
                }
            }
        }

        public ConstantBuffer<Matrix4> GetBoundingBoxTransformBuffer()
        {
            if (!this.BoundingBox.IsValid)
                return null;

            Matrix4 trans = this.BoundingBox.Transformation.Value;

            if (_boundingBoxTransformBuffer == null)
            {
                _boundingBoxTransformBuffer = new ConstantBuffer<Matrix4>(trans);
            }
            else if (_boundingBoxTransformBuffer.Value != trans)
            {
                _boundingBoxTransformBuffer.Value = trans;
            }

            return _boundingBoxTransformBuffer;
        }

        public Matrix4? GetWorldTransform(SceneNode root)
        {
            if (root == this)
            {
                // this is the root node
                return LocalTransform;
            }

            if (ParentNode == null)
            {
                // without a parent there is no chance to find the root node
                return null;
            }

            if (!_cachedWorldTransforms.ContainsKey(root))
            {
                // get world transformation of parent node
                Matrix4? parentWorldTrans = ParentNode.GetWorldTransform(root);

                if (parentWorldTrans == null)
                {
                    // there is no path to the root node
                    return null;
                }

                // calculate new world transformation
                Matrix4 newWorldTrans = parentWorldTrans.Value * LocalTransform;

                // cache the transformation
                _cachedWorldTransforms.Add(root, newWorldTrans);

                return newWorldTrans;
            }

            // return cached transformation
            return _cachedWorldTransforms[root];
        }

        public IConstantBuffer GetWorldTransformBuffer(SceneNode root)
        {
            Matrix4 worldTrans = GetWorldTransform(root) ?? Matrix4.Identity;
            ConstantBuffer<Matrix4> cBuffer;
            if (!_worldTransformBuffers.ContainsKey(root))
            {
                cBuffer = new ConstantBuffer<Matrix4>(worldTrans);
                _worldTransformBuffers.Add(root, cBuffer);

                return cBuffer;
            }

            cBuffer = _worldTransformBuffers[root];
            if (cBuffer.Value != worldTrans)
            {
                // update constant buffer
                cBuffer.Value = worldTrans;
            }

            return cBuffer;
        }

        public SceneNode ParentNode
        {
            get
            {
                return _parentNode;
            }
            internal set
            {
                if (value != _parentNode)
                {
                    if (_parentNode != null)
                    {
                        _parentNode.RemoveChildNode(this);
                    }

                    _parentNode = value;
                    InvalidateWorldTransforms();
                }
            }
        }

        public Matrix4? GetInverseWorldTransform(SceneNode root)
        {
            if (!_inverseWorldTransforms.ContainsKey(root))
            {
                Matrix4? m = GetWorldTransform(root);
                if (!m.HasValue)
                    return null;

                Matrix4 result = m.Value.Inverse;
                _inverseWorldTransforms.Add(root, result);
                return result;
            }

            return _inverseWorldTransforms[root];
        }

        protected IConstantBuffer GetInverseWorldTransformBuffer(SceneNode root)
        {
            Matrix4 invWorldTrans = GetInverseWorldTransform(root) ?? Matrix4.Identity;
            ConstantBuffer<Matrix4> cBuffer;
            if (!_inverseWorldTransformBuffers.ContainsKey(root))
            {
                cBuffer = new ConstantBuffer<Matrix4>(invWorldTrans);
                _inverseWorldTransformBuffers.Add(root, cBuffer);

                return cBuffer;
            }

            cBuffer = _inverseWorldTransformBuffers[root];
            if (cBuffer.Value != invWorldTrans)
            {
                // update constant buffer
                cBuffer.Value = invWorldTrans;
            }
            return cBuffer;
        }

        protected IConstantBuffer GetModelViewTransformBuffer(SceneNode root, ICameraNode cam)
        {
            Matrix4 invCamWorldTrans = cam != null ? (cam.GetInverseWorldTransform(root) ?? Matrix4.Identity) : Matrix4.Identity;

            Dictionary<ICameraNode, ConstantBuffer<ModelViewTrans>> camToBufferMap = null;
            if (!_modelViewTransformBuffers.ContainsKey(root))
            {
                camToBufferMap = new Dictionary<ICameraNode, ConstantBuffer<ModelViewTrans>>();
                _modelViewTransformBuffers[root] = camToBufferMap;
            }
            else
            {
                camToBufferMap = _modelViewTransformBuffers[root];
            }

            ConstantBuffer<ModelViewTrans> cbuffer = null;
            if (!camToBufferMap.ContainsKey(cam))
            {
                cbuffer = new ConstantBuffer<ModelViewTrans>();
                camToBufferMap.Add(cam, cbuffer);
            }
            else
            {
                cbuffer = camToBufferMap[cam];
            }

            ModelViewTrans mvt = new ModelViewTrans();
            mvt.ModelViewMatrix = invCamWorldTrans * (this.GetWorldTransform(root) ?? Matrix4.Identity);

            if (cbuffer.Value.ModelViewMatrix != mvt.ModelViewMatrix)
            {
                mvt.NormalMatrix = mvt.ModelViewMatrix.NormalMatrix;
                cbuffer.Value = mvt;
            }

            return cbuffer;
        }

        internal virtual void InvalidateWorldTransforms()
        {
            _cachedWorldTransforms.Clear();
            _inverseWorldTransforms.Clear();
        }

        protected virtual void RemoveChildNode(SceneNode node)
        {
            InvalidateBoundingBox();
        }

        public abstract bool Accept(ITraverser traverser);

        public abstract void Preload(Renderer renderer);

        public override string ToString()
        {
            return this.Name;
        }
    }
}
