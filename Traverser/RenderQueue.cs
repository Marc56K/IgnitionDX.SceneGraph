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

using System.Collections.Generic;
using System;
using IgnitionDX.Graphics;

namespace IgnitionDX.SceneGraph
{
    public class RenderQueue
    {
        private RenderTraverser _traverser;
        private List<List<ShapeNode>> _shapeListCache = new List<List<ShapeNode>>();

        public Dictionary<Material, List<ShapeNode>> MaterialGroups { get; private set; }
        public List<ShapeNode> ShapeNodes { get; private set; }
        public List<LightNode> LightNodes { get; private set; }
        public List<ClipPlaneNode> ClipPlaneNodes { get; private set; }
        public SceneNode Root { get; private set; }

        public RenderQueue()
        {
            MaterialGroups = new Dictionary<Material, List<ShapeNode>>();
            ShapeNodes = new List<ShapeNode>();
            LightNodes = new List<LightNode>();
            ClipPlaneNodes = new List<ClipPlaneNode>();

            _traverser = new RenderTraverser(this);
        }

        public void Update(SceneNode root)
        {
            this.Root = root;

            foreach (List<ShapeNode> shapeList in this.MaterialGroups.Values)
            {
                shapeList.Clear();
                _shapeListCache.Add(shapeList);
            }

            this.MaterialGroups.Clear();
            this.ShapeNodes.Clear();
            this.LightNodes.Clear();

            this.Root.Accept(_traverser);

            foreach(List<ShapeNode> shapeList in this.MaterialGroups.Values)
            {
                this.ShapeNodes.AddRange(shapeList);
            }
        }

        private List<ShapeNode> GetOrCreateShapeList()
        {
            if (_shapeListCache.Count > 0)
            {
                List<ShapeNode> list = _shapeListCache[0];
                _shapeListCache.RemoveAt(0);
                return list;
            }

            return new List<ShapeNode>();
        }

        public void Add(ShapeNode node)
        {
            List<ShapeNode> shapeList;
            if (MaterialGroups.ContainsKey(node.Material))
            {
                shapeList = MaterialGroups[node.Material];
            }
            else
            {
                shapeList = GetOrCreateShapeList();
                MaterialGroups.Add(node.Material, shapeList);
            }

            shapeList.Add(node);
        }

        public void Add(LightNode node)
        {
            LightNodes.Add(node);
        }

        public void Add(ClipPlaneNode node)
        {
            ClipPlaneNodes.Add(node);
        }
    }
}
