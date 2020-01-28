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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IgnitionDX.SceneGraph
{
    public class Finder<T> : ITraverser where T : SceneNode
    {
        private SceneNode _root;
        private string _namePattern;
        private bool _findAll;
        private bool _continue = true;
        private List<T> _result = new List<T>();
        
        public static T Find(SceneNode root, string namePattern)
        {
            var finder = new Finder<T>(root, namePattern, false);
            root.Accept(finder);

            if (finder._result.Count > 0)
                return finder._result[0];

            return null;
        }

        public static List<T> FindAll(SceneNode root, string namePattern)
        {
            var finder = new Finder<T>(root, namePattern, true);
            root.Accept(finder);

            if (finder._result.Count > 0)
                return finder._result;

            return null;
        }

        private Finder(SceneNode root, string namePattern, bool findAll)
        {
            _root = root;
            _namePattern = namePattern;
            _findAll = findAll;
        }
        
        public bool Enter(GroupNode node)
        {
            CheckNode(node);
            return _continue;
        }

        public bool Leave(GroupNode node)
        {
            return _continue;
        }

        public bool Visit(CameraNode node)
        {
            CheckNode(node);
            return _continue;
        }

        public bool Visit(ShapeNode node)
        {
            CheckNode(node);
            return _continue;
        }

        public bool Visit(LightNode node)
        {
            CheckNode(node);
            return _continue;
        }

        public bool Visit(ClipPlaneNode node)
        {
            CheckNode(node);
            return _continue;
        }

        private void CheckNode(SceneNode node)
        {
            if (node is T && Regex.IsMatch(node.Name, _namePattern))
            {
                _result.Add(node as T);
                _continue = !_findAll;
            }
        }
    }
}
