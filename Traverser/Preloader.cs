﻿/* MIT License (MIT)
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IgnitionDX.SceneGraph
{
    public class Preloader : ITraverser
    {
        public static void LoadGpuResources(Renderer renderer, SceneNode root)
        {
            new Preloader(renderer, root);
        }

        private Renderer _renderer;
        private Preloader(Renderer renderer, SceneNode root)
        {
            _renderer = renderer;
            root.Accept(this);
        }
        
        public bool Enter(GroupNode node)
        {
            node.Preload(_renderer);
            return true;
        }

        public bool Leave(GroupNode node)
        {
            return true;
        }

        public bool Visit(CameraNode node)
        {
            node.Preload(_renderer);
            return true;
        }

        public bool Visit(ShapeNode node)
        {
            node.Preload(_renderer);
            return true;
        }

        public bool Visit(LightNode node)
        {
            node.Preload(_renderer);
            return true;
        }

        public bool Visit(ClipPlaneNode node)
        {
            node.Preload(_renderer);
            return true;
        }
    }
}
