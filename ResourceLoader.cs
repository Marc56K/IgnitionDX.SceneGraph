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
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace IgnitionDX.SceneGraph
{
    public class ResourceLoader
    {
        private Dictionary<string, WeakReference> _cache = new Dictionary<string, WeakReference>();

        private T GetFromCache<T>(string key) where T : class
        {
            T result = null;
            if (_cache.ContainsKey(key))
            {
                object obj = _cache[key].Target;
                if (obj is T)
                {
                    result = obj as T;
                }
                if (obj == null)
                {
                    _cache.Remove(key);
                }
            }

            return result;
        }

        private void AddToCache(string key, object obj)
        {
            _cache.Add(key, new WeakReference(obj));
        }

        private Stream GetResourceStream(string embeddedFileName)
        {
            Stream result = null;
            Assembly assembly = FindAssembly(embeddedFileName);
            if (assembly != null)
            {
                result = assembly.GetManifestResourceStream(embeddedFileName);
            }

            if (result == null)
            {
                throw new Exception("Resource \"" + embeddedFileName + "\" not found.");
            }

            return result;
        }

        public Assembly FindAssembly(string embeddedFileName)
        {
            lock (this)
            {
                var entryAsm = System.Reflection.Assembly.GetEntryAssembly();
                if (embeddedFileName.StartsWith(entryAsm.GetName().Name))
                {
                    foreach (string resName in entryAsm.GetManifestResourceNames())
                    {
                        if (resName == embeddedFileName)
                        {
                            return System.Reflection.Assembly.GetEntryAssembly();
                        }
                    }
                }

                foreach (AssemblyName assemblyName in entryAsm.GetReferencedAssemblies())
                {
                    if (embeddedFileName.StartsWith(assemblyName.Name))
                    {
                        Assembly assembly = System.Reflection.Assembly.Load(assemblyName.ToString());
                        foreach (string resName in assembly.GetManifestResourceNames())
                        {
                            if (resName == embeddedFileName)
                            {
                                return assembly;
                            }
                        }
                    }
                }
                return null;
            }
        }

        #region Shaders

        public VertexShader LoadVertexShaderFromFile(string fileName, Shader.Profile profile)
        {
            lock (this)
            {
                string key = String.Format("Shader [File: {0} Profile: {1}]", fileName, profile);
                VertexShader shader = GetFromCache<VertexShader>(key);
                if (shader == null)
                {
                    shader = new VertexShader(fileName, profile);
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public VertexShader LoadVertexShaderFromResource(string embeddedFileName, Shader.Profile profile)
        {
            lock (this)
            {
                string key = String.Format("Shader [Resource: {0} Profile: {1}]", embeddedFileName, profile);
                VertexShader shader = GetFromCache<VertexShader>(key);
                if (shader == null)
                {
                    shader = new VertexShader(embeddedFileName, GetResourceStream(embeddedFileName), profile);
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public VertexShader LoadVertexShaderFromByteArray(string name, byte[] sourceCode, Shader.Profile profile)
        {
            lock (this)
            {
                string key = String.Format("Shader [ByteArray: {0} Profile: {1}]", name, profile);
                VertexShader shader = GetFromCache<VertexShader>(key);
                if (shader == null)
                {
                    using (var stream = new MemoryStream(sourceCode))
                    {
                        shader = new VertexShader(name, stream, profile);
                    }
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public GeometryShader LoadGeometryShaderFromFile(string fileName, Shader.Profile profile)
        {
            lock (this)
            {
                string key = String.Format("Shader [File: {0} Profile: {1}]", fileName, profile);
                GeometryShader shader = GetFromCache<GeometryShader>(key);
                if (shader == null)
                {
                    shader = new GeometryShader(fileName, profile);
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public GeometryShader LoadGeometryShaderFromResource(string embeddedFileName, Shader.Profile profile)
        {
            lock (this)
            {
                string key = String.Format("Shader [Resource: {0} Profile: {1}]", embeddedFileName, profile);
                GeometryShader shader = GetFromCache<GeometryShader>(key);
                if (shader == null)
                {
                    shader = new GeometryShader(embeddedFileName, GetResourceStream(embeddedFileName), profile);
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public GeometryShader LoadGeometryShaderFromByteArray(string name, byte[] sourceCode, Shader.Profile profile)
        {
            lock (this)
            {
                string key = String.Format("Shader [ByteArray: {0} Profile: {1}]", name, profile);
                GeometryShader shader = GetFromCache<GeometryShader>(key);
                if (shader == null)
                {
                    using (var stream = new MemoryStream(sourceCode))
                    {
                        shader = new GeometryShader(name, stream, profile);
                    }
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public GeometryShader LoadGeometryShaderFromFile(string fileName, Shader.Profile profile, Type streamOutType)
        {
            lock (this)
            {
                string key = String.Format("Shader [File: {0} Profile: {1} StreamOutType: {2}]", fileName, profile, streamOutType);
                GeometryShader shader = GetFromCache<GeometryShader>(key);
                if (shader == null)
                {
                    shader = new GeometryShader(fileName, profile, streamOutType);
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public GeometryShader LoadGeometryShaderFromResource(string embeddedFileName, Shader.Profile profile, Type streamOutType)
        {
            lock (this)
            {
                string key = String.Format("Shader [Resource: {0} Profile: {1} StreamOutType: {2}]", embeddedFileName, profile, streamOutType);
                GeometryShader shader = GetFromCache<GeometryShader>(key);
                if (shader == null)
                {
                    shader = new GeometryShader(embeddedFileName, GetResourceStream(embeddedFileName), profile, streamOutType);
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public GeometryShader LoadGeometryShaderFromByteArray(string name, byte[] sourceCode, Shader.Profile profile, Type streamOutType)
        {
            lock (this)
            {
                string key = String.Format("Shader [ByteArray: {0} Profile: {1} StreamOutType: {2}]", name, profile, streamOutType);
                GeometryShader shader = GetFromCache<GeometryShader>(key);
                if (shader == null)
                {
                    using (var stream = new MemoryStream(sourceCode))
                    {
                        shader = new GeometryShader(name, stream, profile);
                    }
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public PixelShader LoadPixelShaderFromFile(string fileName, Shader.Profile profile)
        {
            lock (this)
            {
                string key = String.Format("Shader [File: {0} Profile: {1}]", fileName, profile);
                PixelShader shader = GetFromCache<PixelShader>(key);
                if (shader == null)
                {
                    shader = new PixelShader(fileName, profile);
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public PixelShader LoadPixelShaderFromResource(string embeddedFileName, Shader.Profile profile)
        {
            lock (this)
            {
                string key = String.Format("Shader [Resource: {0} Profile: {1}]", embeddedFileName, profile);
                PixelShader shader = GetFromCache<PixelShader>(key);
                if (shader == null)
                {
                    shader = new PixelShader(embeddedFileName, GetResourceStream(embeddedFileName), profile);
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        public PixelShader LoadPixelShaderFromByteArray(string name, byte[] sourceCode, Shader.Profile profile)
        {
            lock (this)
            {
                string key = String.Format("Shader [ByteArray: {0} Profile: {1}]", name, profile);
                PixelShader shader = GetFromCache<PixelShader>(key);
                if (shader == null)
                {
                    using (var stream = new MemoryStream(sourceCode))
                    {
                        shader = new PixelShader(name, stream, profile);
                    }
                    AddToCache(key, shader);
                }

                return shader;
            }
        }

        #endregion

        #region Textures

        public Texture2D LoadTexture2DFromFile(string fileName)
        {
            lock (this)
            {
                string key = String.Format("Texture2D [File: {0}]", fileName);
                Texture2D tex = GetFromCache<Texture2D>(key);
                if (tex == null)
                {
                    tex = new Texture2D(fileName);
                    AddToCache(key, tex);
                }

                return tex;
            }
        }

        public Texture2D LoadTexture2DFromResource(string embeddedFileName, Shader.Profile profile)
        {
            lock (this)
            {
                string key = String.Format("Texture2D [Resource: {0}]", embeddedFileName);
                Texture2D tex = GetFromCache<Texture2D>(key);
                if (tex == null)
                {
                    tex = new Texture2D(embeddedFileName, GetResourceStream(embeddedFileName));
                    AddToCache(key, tex);
                }

                return tex;
            }
        }

        #endregion

        #region Collada

        public SceneNode LoadColladaFromFile(string fileName)
        {
            var root = new ColladaLoader(this, ColladaOptions.GetDefaultOptions(this)).Load(fileName);

            if (root is SceneNode)
                return root as SceneNode;

            return null;
        }

        public SceneNode LoadColladaFromFile(string fileName, ColladaOptions options)
        {
            var root = new ColladaLoader(this, options).Load(fileName);

            if (root is SceneNode)
                return root as SceneNode;

            return null;

        }

        #endregion
    }
}
