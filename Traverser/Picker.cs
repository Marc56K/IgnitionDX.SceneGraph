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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IgnitionDX.SceneGraph
{
    public struct PickingResult
    {
        public ShapeNode PickedNode;
        public Vector3 PickingPoint;
    }
    
    public class Picker : ITraverser
    {
        public static PickRay CreatePickRay(SceneNode root, CameraNode cam, float nwx, float nwy)
        {
            nwx = 2 * (nwx - 0.5f);
            nwy = -2 * (nwy - 0.5f);

            Vector4 p1 = new Vector4(nwx, nwy, 1, 1);
            Vector4 p2 = new Vector4(nwx, nwy, 0, 1);

            Matrix4 invProj = cam.ViewFrustum.ProjectionMatrix.Inverse;

            p1 = (invProj * p1).DivW;
            p2 = (invProj * p2).DivW;

            Matrix4 camWorldTrans = cam.GetWorldTransform(root) ?? Matrix4.Identity;

            Vector4 dir = (camWorldTrans * p1) - (camWorldTrans * p2);
            Vector4 origin;

            if (cam is OrthoCameraNode)
            {
                float width = (cam as OrthoCameraNode).ActualWidth;
                float height = (cam as OrthoCameraNode).ActualHeight;
                origin = camWorldTrans * new Vector4(nwx * width * 0.5f, nwy * height * 0.5f, 0, 1);
            }
            else
            {
                origin = camWorldTrans * new Vector4(0, 0, 0, 1);
            }

            return new PickRay()
            {
                Origin = origin.DivW.XYZ,
                Direction = dir.XYZ.Normalized
            };
        }

        public static PickingResult? PickShapeNode(SceneNode root, CameraNode cam, float nwx, float nwy)
        {
            return new Picker().Pick(root, CreatePickRay(root, cam, nwx, nwy));
        }

        private PickRay _pickRay;
        private SceneNode _root;

        private ShapeNode _closestNode;
        private Vector3 _pickPoint;
        private float _closestZDist;

        private PickingResult? Pick(SceneNode root, PickRay ray)
        {
            _pickRay = ray;
            _root = root;
            _closestNode = null;
            _closestZDist = 0;

            root.Accept(this);

            if (_closestNode != null)
            {
                PickingResult result = new PickingResult();
                result.PickedNode = _closestNode;
                result.PickingPoint = _pickPoint;
                return result;
            }

            return null;
        }

        private bool TestIntersects(SceneNode node)
        {
            if (!(node is ShapeNode) && (node.BoundingBox == null || !node.BoundingBox.IsValid))
                return true;

            Matrix4 trans = node.GetWorldTransform(_root).Value;
            Vector3 max = (trans * new Vector4(node.BoundingBox.Max.Value, 1.0f)).DivW.XYZ;
            Vector3 center = (trans * new Vector4(node.BoundingBox.Center.Value, 1.0f)).DivW.XYZ;
            float radius = (center - max).Length;

            // plane equation
            float d = _pickRay.Direction.Dot(center);

            // intersection point on plane
            float s = (d - _pickRay.Direction.Dot(_pickRay.Origin)) / _pickRay.Direction.Dot(_pickRay.Direction);

            if (s >= 0 || (_pickRay.Origin - center).Length < radius)
            {
                Vector3 f = _pickRay.Origin + (_pickRay.Direction * s);
                float dist = (f - center).Length; // distance of ray from sphere center

                if (dist < radius) // ray intersects bounding sphere
                {
                    if (node is ShapeNode)
                    {
                        ShapeNode shapeNode = node as ShapeNode;
                        
                        // transform pick ray to object space
                        Matrix4 invTrans = node.GetInverseWorldTransform(_root).Value;
                        PickRay localPickRay = new PickRay();
                        localPickRay.Origin = (invTrans * new Vector4(_pickRay.Origin, 1)).XYZ;
                        localPickRay.Direction = (invTrans * new Vector4(_pickRay.Direction, 0)).XYZ;

                        // pick triangles in geometry
                        Vector3? localPickPoint = shapeNode.Geometry.Pick(localPickRay);

                        if (localPickPoint != null)
                        {
                            Vector3 pickPoint = (trans * new Vector4(localPickPoint.Value, 1)).DivW.XYZ;
                            float zDist = (_pickRay.Origin - pickPoint).Length;
                            if (_closestNode == null || zDist < _closestZDist)
                            {
                                _closestZDist = zDist;
                                _closestNode = shapeNode;
                                _pickPoint = pickPoint;
                            }
                        }
                    }
                    return true; // is in bounding sphere
                }
            }

            return false;
        }

        public bool Enter(GroupNode node)
        {
            return node.Enabled && TestIntersects(node);
        }

        public bool Leave(GroupNode node)
        {
            return true;
        }

        public bool Visit(CameraNode node)
        {
            return true;
        }

        public bool Visit(ShapeNode node)
        {
            if (node.Enabled)
            {
                TestIntersects(node);
            }

            return true;
        }

        public bool Visit(LightNode node)
        {
            return true;
        }

        public bool Visit(ClipPlaneNode node)
        {
            return true;
        }
    }
}
