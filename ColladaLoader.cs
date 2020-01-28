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

using Collada14;
using IgnitionDX.Graphics;
using IgnitionDX.Math;
using IgnitionDX.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace IgnitionDX.SceneGraph
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ColladaAmbientLightParameters
    {
        public Color4 Color;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColladaMaterialParameters
    {
        public Color4 AmbientColor;
        public Color4 DiffuseColor;
        public Color4 SpecularColor;
        public float Shininess;
        public bool UseColorMap;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColladaVertexDescription : IVertexDescription
    {
        public Vector3 Vertex;
        public Vector3 TexCoord;
        public Vector3 Normal;
        public Vector3 TexTangent;
        public Vector3 TexBinormal;

        public Vector3 Position
        {
            get { return Vertex; }
        }
    }
    
    public class ColladaOptions
    {
        public Dictionary<string, List<Shader>> Shaders { get; private set; }
        public TextureFilter ShadowMapFilter { get; set; }
        public TextureFilter TextureFilter { get; set; }
        public ConstantBuffer<ColladaAmbientLightParameters> AmbientLight { get; private set; }

        public ColladaOptions()
        {
            this.Shaders = new Dictionary<string, List<Shader>>();
            this.TextureFilter = TextureFilter.CreateLinearMipmapped(SharpDX.Direct3D11.TextureAddressMode.Wrap);
            this.ShadowMapFilter = TextureFilter.CreatePCF();
        }

        public void AddShader(string pass, Shader shader)
        {
            if (!Shaders.ContainsKey(pass))
            {
                Shaders.Add(pass, new List<Shader>());
            }

            Shaders[pass].Add(shader);
        }

        public static ColladaOptions GetDefaultOptions(ResourceLoader resLoader)
        {
            var options = new ColladaOptions();
            options.AddShader("AMBIENT_PASS", resLoader.LoadPixelShaderFromResource("IgnitionDX.SceneGraph.Shaders.Ambient.ps", Shader.Profile.ps_4_0));
            options.AddShader("AMBIENT_PASS", resLoader.LoadVertexShaderFromResource("IgnitionDX.SceneGraph.Shaders.Ambient.vs", Shader.Profile.vs_4_0));
            options.AddShader("LIGHTING_PASS", resLoader.LoadPixelShaderFromResource("IgnitionDX.SceneGraph.Shaders.PhongLighting.ps", Shader.Profile.ps_4_0));
            options.AddShader("LIGHTING_PASS", resLoader.LoadVertexShaderFromResource("IgnitionDX.SceneGraph.Shaders.PhongLighting.vs", Shader.Profile.vs_4_0));
            options.AddShader("DIFFUSE_PASS", resLoader.LoadPixelShaderFromResource("IgnitionDX.SceneGraph.Shaders.DiffuseColor.ps", Shader.Profile.ps_4_0));
            options.AddShader("DIFFUSE_PASS", resLoader.LoadVertexShaderFromResource("IgnitionDX.SceneGraph.Shaders.Ambient.vs", Shader.Profile.vs_4_0));
            options.AddShader("BBOX_PASS", resLoader.LoadPixelShaderFromResource("IgnitionDX.SceneGraph.Shaders.BoundingBox.ps", Shader.Profile.ps_4_0));
            options.AddShader("BBOX_PASS", resLoader.LoadVertexShaderFromResource("IgnitionDX.SceneGraph.Shaders.BoundingBox.vs", Shader.Profile.vs_4_0));
            options.TextureFilter = TextureFilter.CreateLinearMipmapped(SharpDX.Direct3D11.TextureAddressMode.Wrap);

            ColladaAmbientLightParameters ambientParams = new ColladaAmbientLightParameters();
            ambientParams.Color = new Color4(0.1f, 0.1f, 0.1f, 1);
            options.AmbientLight = new ConstantBuffer<ColladaAmbientLightParameters>(ambientParams);

            return options;
        }
    }
    
    public class ColladaLoader : DAELoaderContext
    {
        internal ResourceLoader ResourceLoader { get; set; }
        internal ColladaOptions Options { get; set; }

        internal static Color4 GetColor4(DAEVector4 col)
        {
            return new Color4((float)col.X, (float)col.Y, (float)col.Z, (float)col.W);
        }

        internal static Vector4 GetVector4(DAEVector4 vec)
        {
            return new Vector4((float)vec.X, (float)vec.Y, (float)vec.Z, (float)vec.W);
        }

        internal static Vector3 GetVector3(DAEVector3 vec)
        {
            return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
        }

        internal static Matrix4 GetMatrix4(DAEMatrix4 matrix)
        {
            Matrix4 mat = new Matrix4();
            mat.M11 = (float)matrix.M11;
            mat.M12 = (float)matrix.M12;
            mat.M13 = (float)matrix.M13;
            mat.M14 = (float)matrix.M14;
            mat.M21 = (float)matrix.M21;
            mat.M22 = (float)matrix.M22;
            mat.M23 = (float)matrix.M23;
            mat.M24 = (float)matrix.M24;
            mat.M31 = (float)matrix.M31;
            mat.M32 = (float)matrix.M32;
            mat.M33 = (float)matrix.M33;
            mat.M34 = (float)matrix.M34;
            mat.M41 = (float)matrix.M41;
            mat.M42 = (float)matrix.M42;
            mat.M43 = (float)matrix.M43;
            mat.M44 = (float)matrix.M44;
            return mat;
        }

        internal ColladaLoader(ResourceLoader resLoader, ColladaOptions options)
        {
            this.ResourceLoader = resLoader;
            this.Options = options;
        }
        
        public override void LogInfo(object sender, string message)
        {
            Logger.LogInfo(sender, message);
        }

        public override void LogError(object sender, string message)
        {
            Logger.LogError(sender, message);
        }

        public override IDAEMaterial CreateMaterial(DAEVector4 ambient, DAEVector4 diffuse, DAEVector4 specular, double shininess, bool isTransparent, string texturePath)
        {
            SceneMaterial m = new SceneMaterial();

            foreach (KeyValuePair<string, List<Shader>> shaderPass in Options.Shaders)
            {
                m.SetShaders(shaderPass.Key, shaderPass.Value.ToArray());

                if (!String.IsNullOrEmpty(texturePath))
                {
                    var tex = ResourceLoader.LoadTexture2DFromFile(texturePath);
                    m.SetTexture(shaderPass.Key, "ColorMap", tex);
                    isTransparent |= tex.IsSemiTransparent;
                }

                ColladaMaterialParameters matDesc = new ColladaMaterialParameters();
                matDesc.UseColorMap = !String.IsNullOrEmpty(texturePath);
                matDesc.AmbientColor = GetColor4(ambient);
                matDesc.DiffuseColor = GetColor4(diffuse);
                matDesc.SpecularColor = GetColor4(specular);
                matDesc.Shininess = (float)shininess;

                IConstantBuffer matBuffer = new ConstantBuffer<ColladaMaterialParameters>(matDesc);
                m.SetConstantBuffer(shaderPass.Key, "MaterialParameters", matBuffer);
                m.SetConstantBuffer(shaderPass.Key, "AmbientLightParameters", Options.AmbientLight);

                m.SetTextureFilter(shaderPass.Key, "ColorMapSamplerState", Options.TextureFilter);
                m.SetTextureFilter(shaderPass.Key, "ShadowMapSamplerState", Options.ShadowMapFilter);
            }

            if (isTransparent)
            {
                m.MaterialClass = "Translucent";
            }

            return m;
        }

        public override IDAEGeometry CreateGeometry(DAEVertexDescription[] vertices)
        {
            if (vertices.Length == 0)
                return null;
            
            ColladaVertexDescription[] verts = new ColladaVertexDescription[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                verts[i].Vertex = GetVector3(vertices[i].Vertex);
                verts[i].TexCoord = GetVector3(vertices[i].TexCoord);
                verts[i].Normal = GetVector3(vertices[i].Normal);
                verts[i].TexTangent = GetVector3(vertices[i].TexTangent);
                verts[i].TexBinormal = GetVector3(vertices[i].TexBinormal);
            }

            return new SceneGeometry(verts);
        }

        public override IDAEGroupNode CreateGroupNode(string name, DAEMatrix4 localTransformation, params IDAESceneNode[] childNodes)
        {
            var group = new GroupNode();
            group.Name = name;
            group.LocalTransform = GetMatrix4(localTransformation);
            foreach (var child in childNodes)
            {
                group.Add(child as SceneNode);
            }
            return group;
        }

        public override IDAEShapeNode CreateShapeNode(string name, IDAEMaterial material, IDAEGeometry geometry)
        {
            var shape = new ShapeNode();
            shape.Name = name;
            shape.Material = material as Material;
            shape.Geometry = geometry as IGeometry;
            return shape;
        }

        public override IDAEAmbientLightNode CreateAmbientLightNode(string name, DAEVector4 color)
        {
            return null;
        }

        public override IDAEDirectionalLightNode CreateDirectionalLightNode(string name, DAEVector4 color)
        {
            var light = new DirectionalLightNode();
            light.Name = name;
            light.Color = GetColor4(color);
            return light;
        }

        public override IDAEPointLightNode CreatePointLightNode(string name, DAEVector4 color, double constantAttenuation, double linearAttenuation, double quadraticAttenuation)
        {
            var light = new PointLightNode();
            light.Name = name;
            light.Color = GetColor4(color);
            light.ConstantAttenuation = (float)constantAttenuation;
            light.LinearAttenuation = (float)linearAttenuation;
            light.QuadraticAttenuation = (float)quadraticAttenuation;
            return light;
        }

        public override IDAESpotLightNode CreateSpotLightNode(string name, DAEVector4 color, double constantAttenuation, double linearAttenuation, double quadraticAttenuation, double spotExponent, double spotCutOff)
        {
            var light = new SpotLightNode();
            light.Name = name;
            light.Color = GetColor4(color);
            light.ConstantAttenuation = (float)constantAttenuation;
            light.LinearAttenuation = (float)linearAttenuation;
            light.QuadraticAttenuation = (float)quadraticAttenuation;
            light.SpotExponent = (float)spotExponent;
            light.SpotCutOff = (float)spotCutOff;
            return light;
        }
    }
}
