
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace proj
{
    [Serializable]
    struct VertexPositionNormal
    {
        public static VertexElement[] VertexElements = new VertexElement[] {
            new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0, 12, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0)
        };

        public static int SizeInBytes
        {
            get
            {
                return 24;
            }
        }


        public Vector3 Position;
        public Vector3 Normal;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
    }

    [Serializable]
    struct VertexPositionNormalColor
    {
        public static VertexElement[] VertexElements = new VertexElement[] {
            new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0, 12, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0),
            new VertexElement(0, 24, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0)
        };

        public static int SizeInBytes {
            get
            {
                return 28;
            }
        }


        public Vector3  Position;
        public Vector3  Normal;
        public Color    Color;

        public VertexPositionNormalColor(Vector3 position, Vector3 normal, Color color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }
    }


    static class Utils
    {
        private const float CTOF = 1.0f / 255.0f;

        public static Vector3 ToVector3(Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector4 ToVector4(Color c)
        {
            return new Vector4(c.R * CTOF, c.G * CTOF, c.B * CTOF, c.A * CTOF);
        }
    }

    class EffectWrapper
    {
        Effect _effect;
        EffectParameter _worldViewParam;
        EffectParameter _worldParam;
        EffectParameter _invTransposeWorldParam;
        EffectParameter _lightDirParam;
        EffectParameter _eyePositionParam;
        EffectParameter _ambientColorParam;
        EffectParameter _diffuseColorParam;
        EffectParameter _specularColorParam;

        public EffectWrapper(Effect effect)
        {
            _effect = effect;

            _worldViewParam = _effect.Parameters["WorldViewProj"];
            _worldParam = _effect.Parameters["World"];
            _invTransposeWorldParam = _effect.Parameters["InvTransposeWorld"];
            _lightDirParam = _effect.Parameters["LightDir"];
            _eyePositionParam = _effect.Parameters["EyePosition"];
            _ambientColorParam = _effect.Parameters["AmbientColor"];
            _diffuseColorParam = _effect.Parameters["DiffuseColor"];
            _specularColorParam = _effect.Parameters["SpecularColor"];
        }

        public void UpdateSceneParameters(Matrix world, Matrix mvp, Vector3 eyePos, Vector3 lightDir)
        {
            Matrix invTransposeWorld = Matrix.Invert(Matrix.Transpose(world));
            lightDir.Normalize();

            _worldParam.SetValue(world);
            _lightDirParam.SetValue(lightDir);
            _eyePositionParam.SetValue(eyePos);
            _worldViewParam.SetValue(mvp);
            _invTransposeWorldParam.SetValue(invTransposeWorld);
        }


        public void UpdateEffectParameters(Color ambient, Color diffuse, Color specular)
        {
            _ambientColorParam.SetValue(Utils.ToVector4(ambient));
            _diffuseColorParam.SetValue(Utils.ToVector4(diffuse));
            _specularColorParam.SetValue(Utils.ToVector4(specular));
            _effect.CommitChanges();
        }

        public void Begin()
        {
            _effect.Begin();
            _effect.Techniques[0].Passes[0].Begin();
        }

        public void End()
        {
            _effect.Techniques[0].Passes[0].End();
            _effect.End();
        }

        public void Dispose()
        {
            _effect.Dispose();
            _effect = null;
        }
    }

    class MeshMaterialGroup
    {
        public short[] tris;
        public Color ambient;
        public Color diffuse;
        public Color specular;

        public IndexBuffer indexBuffer;
    }

    class Mesh
    {
        Matrix WorldMatrix;

        public VertexBuffer vertexBuffer;
        public VertexDeclaration vertexDecl;
        public IndexBuffer indexBuffer;
        public VertexPositionNormal[] verts;
        public short[] tris;

        private List<MeshMaterialGroup> _materialGroups;

        
        public Mesh()
        {
            WorldMatrix = Matrix.Identity;
            _materialGroups = new List<MeshMaterialGroup>();
        }

        public void Allocate(uint nVerts, uint nFaces)
        {
            verts = new VertexPositionNormal[nVerts];
            tris = new short[nFaces * 3];
        }

        public void AddMaterialGroup(short [] faces, Color ambient, Color diffuse, Color specular)
        {
            MeshMaterialGroup matGroup = new MeshMaterialGroup();
            matGroup.ambient = ambient;
            matGroup.diffuse = diffuse;
            matGroup.specular = specular;

            // Expand all faces into vertex indexes for submission to the gpu
            matGroup.tris = new short[faces.Length * 3];
            int di = 0;
            for (int i = 0; i < faces.Length; ++i)
            {
                short index = (short)(faces[i] * 3);
                matGroup.tris[di++] = tris[index + 0];
                matGroup.tris[di++] = tris[index + 1];
                matGroup.tris[di++] = tris[index + 2];
            }

            _materialGroups.Add(matGroup);
        }

        public void SetVertexPosition(uint i, Vector3 pos)
        {
            verts[i].Position = pos;
        }

        public void SetVertexNormal(uint i, Vector3 norm)
        {
            verts[i].Normal = norm;
        }

        public void SetFaceIndicies(uint i, ushort a, ushort b, ushort c)
        {
            tris[(i * 3) + 0] = (short)a;
            tris[(i * 3) + 1] = (short)b;
            tris[(i * 3) + 2] = (short)c;
        }

        public void SubmitPrimitives(GraphicsDevice gd, EffectWrapper effect)
        {
            if(null == vertexBuffer)
            {
                // Create the vertex buffer
                vertexBuffer = new VertexBuffer(gd, verts.Length * VertexPositionNormal.SizeInBytes, BufferUsage.None);
                vertexBuffer.SetData<VertexPositionNormal>(verts);
                vertexDecl = new VertexDeclaration(gd, VertexPositionNormal.VertexElements);

                if (_materialGroups.Count > 0)
                {
                    // Create index buffers for each material group
                    foreach (MeshMaterialGroup matGroup in _materialGroups)
                    {
                        if (null != matGroup.indexBuffer)
                        {
                            matGroup.indexBuffer.Dispose();
                        }


                        matGroup.indexBuffer = new IndexBuffer(gd, 2 * matGroup.tris.Length, BufferUsage.None,  IndexElementSize.SixteenBits);
                        matGroup.indexBuffer.SetData<short>(matGroup.tris);
                    }
                }
                else
                {
                    // Create global index buffer
                    indexBuffer = new IndexBuffer(gd, 2 * tris.Length, BufferUsage.None,IndexElementSize.SixteenBits);
                    indexBuffer.SetData<short>(tris);
                }

            }

            gd.VertexDeclaration = vertexDecl;
            gd.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionNormal.SizeInBytes);
 
            // Render the triangles using material groups if they are provided
            if (_materialGroups.Count > 0)
            {
                foreach(MeshMaterialGroup matGroup in _materialGroups)
                {
                    effect.UpdateEffectParameters(matGroup.ambient, matGroup.diffuse, matGroup.specular);
                    gd.Indices = matGroup.indexBuffer;
                    gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, verts.Length, 0, matGroup.tris.Length / 3);
                }
            }
            else
            {
                gd.Indices = indexBuffer;   
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, verts.Length, 0, tris.Length / 3);
            }
        }
    }
}

