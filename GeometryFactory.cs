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
    public static class GeometryFactory
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct FullscreenQuadVertexDescription : IVertexDescription
        {
            public Vector3 Vertex;
            public Vector3 TexCoord;

            public Vector3 Position
            {
                get { return Vertex; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BoundingBoxVertexDescription : IVertexDescription
        {
            public Vector3 Vertex;

            public Vector3 Position
            {
                get { return Vertex; }
            }
        }

        public static IGeometry CreateFullscreenQuad()
        {
            var vertices = new FullscreenQuadVertexDescription[6];
            vertices[5].Vertex = new Vector3(-1, 1, 0);
            vertices[5].TexCoord = new Vector3(0, 0, 0);

            vertices[4].Vertex = new Vector3(1, 1, 0);
            vertices[4].TexCoord = new Vector3(1, 0, 0);

            vertices[3].Vertex = new Vector3(1, -1, 0);
            vertices[3].TexCoord = new Vector3(1, 1, 0);

            vertices[2].Vertex = new Vector3(1, -1, 0);
            vertices[2].TexCoord = new Vector3(1, 1, 0);

            vertices[1].Vertex = new Vector3(-1, -1, 0);
            vertices[1].TexCoord = new Vector3(0, 1, 0);

            vertices[0].Vertex = new Vector3(-1, 1, 0);
            vertices[0].TexCoord = new Vector3(0, 0, 0);

            return new Geometry<FullscreenQuadVertexDescription>(vertices);
        }

        public static IGeometry CreateBoundingBoxMesh()
        {
            Vector3 min = new Vector3(-0.5f, -0.5f, -0.5f);
            Vector3 max = new Vector3(0.5f, 0.5f, 0.5f);
            BoundingBoxVertexDescription[] vertices = new BoundingBoxVertexDescription[24];

            vertices[0].Vertex = min;
            vertices[1].Vertex = new Vector3(max.X, min.Y, min.Z);

            vertices[2].Vertex = new Vector3(max.X, min.Y, min.Z);
            vertices[3].Vertex = new Vector3(max.X, min.Y, max.Z);

            vertices[4].Vertex = new Vector3(max.X, min.Y, max.Z);
            vertices[5].Vertex = new Vector3(min.X, min.Y, max.Z);

            vertices[6].Vertex = new Vector3(min.X, min.Y, max.Z);
            vertices[7].Vertex = min;

            vertices[8].Vertex = new Vector3(min.X, max.Y, min.Z);
            vertices[9].Vertex = new Vector3(max.X, max.Y, min.Z);

            vertices[10].Vertex = new Vector3(max.X, max.Y, min.Z);
            vertices[11].Vertex = max;

            vertices[12].Vertex = max;
            vertices[13].Vertex = new Vector3(min.X, max.Y, max.Z);

            vertices[14].Vertex = new Vector3(min.X, max.Y, max.Z);
            vertices[15].Vertex = new Vector3(min.X, max.Y, min.Z);

            vertices[16].Vertex = min;
            vertices[17].Vertex = new Vector3(min.X, max.Y, min.Z);

            vertices[18].Vertex = new Vector3(max.X, min.Y, min.Z);
            vertices[19].Vertex = new Vector3(max.X, max.Y, min.Z);

            vertices[20].Vertex = new Vector3(max.X, min.Y, max.Z);
            vertices[21].Vertex = max;

            vertices[22].Vertex = new Vector3(min.X, min.Y, max.Z);
            vertices[23].Vertex = new Vector3(min.X, max.Y, max.Z);

            return new Geometry<BoundingBoxVertexDescription>(vertices, null, SharpDX.Direct3D.PrimitiveTopology.LineList);
        }
    }
}
