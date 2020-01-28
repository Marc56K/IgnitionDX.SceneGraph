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
using System.IO;
using System.Reflection;

namespace IgnitionDX.SceneGraph
{
    public static class MaterialFactory
    {
        public static Material CreateBoundingBoxMaterial(ResourceLoader resourceLoader)
        {
            Material mat = new Material();
            mat.SetShaders("BBOX_PASS", resourceLoader.LoadVertexShaderFromResource("IgnitionDX.SceneGraph.Shaders.BoundingBox.vs", Shader.Profile.vs_4_0), resourceLoader.LoadPixelShaderFromResource("IgnitionDX.SceneGraph.Shaders.BoundingBox.ps", Shader.Profile.ps_4_0));
            mat.SetConstantBufferParam<Color4>("BBOX_PASS", "LineParameters", "LineColor", new Color4(1, 1, 0, 1));
            return mat;
        }
    }
}
