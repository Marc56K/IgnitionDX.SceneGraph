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

namespace IgnitionDX.SceneGraph
{
    public class GroupNode : SceneNode, Collada14.IDAEGroupNode
    {
        private List<SceneNode> _childNodes = new List<SceneNode>();
        private BoundingBox _boundingBox = null;

        public GroupNode()
        {
        }

        public GroupNode(params SceneNode[] nodes)
        {
            Add(nodes);
        }

        internal override void InvalidateWorldTransforms()
        {
            // invalidate world transforms of this node
            base.InvalidateWorldTransforms();

            // invalidate world transforms of child nodes
            for (int i = 0; i < _childNodes.Count; i++)
            {
                _childNodes[i].InvalidateWorldTransforms();
            }
        }

        internal override void InvalidateBoundingBox()
        {
            _boundingBox = null;
            base.InvalidateBoundingBox();
        }

        /// <summary>
        /// Overrides SceneNode.RemoveChildNode and is called before node changes its parent.
        /// </summary>
        /// <param name="node"></param>
        protected override void RemoveChildNode(SceneNode node)
        {
            _childNodes.Remove(node);
            base.RemoveChildNode(node);
        }

        public void RemoveAll()
        {
            while (_childNodes.Count > 0)
            {
                Remove(_childNodes[0]);
            }
        }

        public void Remove(params SceneNode[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                SceneNode node = nodes[i];
                if (node != null && _childNodes.Contains(node))
                {
                    node.ParentNode = null;
                    _childNodes.Remove(node);
                    node.InvalidateWorldTransforms();
                    InvalidateBoundingBox();
                }
            }
        }

        public void Add(params SceneNode[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                SceneNode node = nodes[i];
                if (node != null && !_childNodes.Contains(node))
                {
                    node.ParentNode = this;
                    _childNodes.Add(node);
                    node.InvalidateWorldTransforms();
                    InvalidateBoundingBox();
                }
            }
        }

        public SceneNode this[int index]
        {
            get { return _childNodes[index]; }
        }

        public int Count
        {
            get { return _childNodes.Count; }
        }

        public override bool Accept(ITraverser traverser)
        {
            if (traverser.Enter(this))
            {
                for (int i = 0; i < _childNodes.Count; i++)
                {
                    if (!_childNodes[i].Accept(traverser))
                    {
                        break;
                    }
                }
            }

            return traverser.Leave(this);
        }

        public override void Preload(Renderer renderer)
        {
        }

        public override BoundingBox BoundingBox
        {
            get
            {
                if (_boundingBox == null)
                {
                    if (this.Count > 0)
                    {
                        _boundingBox = new BoundingBox();
                        for (int i = 0; i < _childNodes.Count; i++)
                        {
                            var bBox = _childNodes[i].BoundingBox;
                            if (bBox != null && bBox.IsValid)
                            {
                                _boundingBox.Grow(_childNodes[i].LocalTransform, bBox);
                            }
                        }
                    }
                    else
                    {
                        _boundingBox = new BoundingBox(new Vector3(), new Vector3());
                    }
                }

                return _boundingBox;
            }
        }
    }
}
