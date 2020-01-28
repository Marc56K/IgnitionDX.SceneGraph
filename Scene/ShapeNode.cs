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

namespace IgnitionDX.SceneGraph
{
    public class ShapeNode : SceneNode, Collada14.IDAEShapeNode
    {
        public Material Material { get; set; }
        public IGeometry Geometry { get; set; }

        public ShapeNode()
        {
        }

        public bool Render(SceneRenderer renderer, SceneNode root)
        {
            if (this.Geometry != null && renderer.ActiveShaderProgram != null)
            {
                if (renderer.ActiveCamera != null && this.Geometry.IsCreated(renderer) && !renderer.ActiveCamera.IsInViewFrustum(this, root))
                {
                    // geometry is not in the view frustum of the camera
                    return false;
                }

                // upload model matrix
                IConstantBuffer trans = this.GetWorldTransformBuffer(root);
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "ModelTransformation", trans.GetBuffer(renderer));

                if (renderer.ActiveCamera != null)
                {
                    // upload model-view and normal matrix
                    trans = this.GetModelViewTransformBuffer(root, renderer.ActiveCamera);
                    renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "ModelViewTransformation", trans.GetBuffer(renderer));
                }

                // render the geometry
                this.Geometry.Render(renderer);

                return true;
            }

            return false;
        }

        public bool RenderInstanced(SceneRenderer renderer, SceneNode root, int numInstances)
        {
            if (this.Geometry != null && renderer.ActiveShaderProgram != null)
            {
                // upload model matrix
                IConstantBuffer trans = this.GetWorldTransformBuffer(root);
                renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "ModelTransformation", trans.GetBuffer(renderer));

                if (renderer.ActiveCamera != null)
                {
                    // upload model-view and normal matrix
                    trans = this.GetModelViewTransformBuffer(root, renderer.ActiveCamera);
                    renderer.ActiveShaderProgram.BindConstantBuffer(renderer, "ModelViewTransformation", trans.GetBuffer(renderer));
                }

                // render the geometry
                this.Geometry.RenderInstanced(renderer, numInstances);

                return true;
            }

            return false;
        }

        public override bool Accept(ITraverser traverser)
        {
            return traverser.Visit(this);
        }

        public override void Preload(Renderer renderer)
        {
            if (this.Material != null)
                this.Material.Preload(renderer);

            if (this.Geometry != null)
                this.Geometry.Preload(renderer);
        }

        public override BoundingBox BoundingBox
        {
            get { return this.Geometry != null ? this.Geometry.BoundingBox : null; }
        }
    }
}
