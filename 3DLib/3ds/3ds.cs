using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

using Library3d.Mesh;
using Library3d.Math;

namespace Library3d.Format3ds
{
    public class ChunkData
    {
        
        public ushort       Identifier = 0;
        public uint         Lenght = 0;
        public int          Offset =0;

        public byte[] CustomData = null;

        public int Id = 0;
        public ChunkData Parent = null;
        public List<ChunkData> Children = new List<ChunkData>();
        public ChunkData()
        {

        }

        public ChunkData(ushort i)
        {
            Identifier = i;
        }
    }

    public class Object3ds
    {
        //
        // Constants
        //
        public const uint kTextureFilenameSize = 256;
        public const uint kObjectNameSize = 200;
        public const uint kHandlerHashSize = 131;
        public const uint kFileContextMaxDepth = 16;
        public const uint kMaxStringLength = 255;

        //
        // Chunk definitions
        //
        private const ushort kChunkEmpty = 0x0000;

        private const ushort kMagic3ds = 0x4d4d;
        private const ushort kMagicS = 0x2d2d;
        private const ushort kMagicL = 0x2d3d;
        // mli file
        private const ushort kMagicLib = 0x3daa;
        private const ushort kMagicMat = 0x3dff;
        // prj file
        private const ushort kMagicC = 0xc23d;
        // start of actual objs
        private const ushort kChunkObjects = 0x3d3d;

        private const ushort kVersionMax = 0x0002;
        private const ushort kVersionKF = 0x0005;
        private const ushort kVersionMesh = 0x3d3e;

        private const ushort kColor3f = 0x0010;
        private const ushort kColor24 = 0x0011;
        private const ushort kColorLin24 = 0x0012;
        private const ushort kColorLin3f = 0x0013;
        private const ushort kIntPercent = 0x0030;
        private const ushort kFloatPercent = 0x0031;
        private const ushort kMasterScale = 0x0100;
        private const ushort kImageFile = 0x1100;
        private const ushort kAmbLight = 0x2100;

        // object chunks
        private const ushort kNamedObject = 0x4000;
        private const ushort kObjMesh = 0x4100;
        private const ushort kObjLight = 0x4600;
        private const ushort kObjCamera = 0x4700;

        private const ushort kMeshVerts = 0x4110;
        private const ushort kVertexFlags = 0x4111;
        private const ushort kMeshFaces = 0x4120;
        private const ushort kMeshMaterial = 0x4130;
        private const ushort kMeshTexVert = 0x4140;
        private const ushort kMeshSmoothGroup = 0x4150;
        private const ushort kMeshXFMatrix = 0x4160;
        private const ushort kMeshColorInd = 0x4165;
        private const ushort kMeshTexInfo = 0x4170;
        private const ushort kHeirarchy = 0x4F00;

        private const ushort kLightSpot = 0x4620;


        private const ushort kViewportLayout = 0x7001;
        private const ushort kViewportData = 0x7011;
        private const ushort kViewportData3 = 0x7012;
        private const ushort kViewportSize = 0x7020;


        // material chunks
        private const ushort kMat = 0xAFFF;
        private const ushort kMatName = 0xA000;
        private const ushort kMatAmb = 0xA010;
        private const ushort kMatDiff = 0xA020;
        private const ushort kMatSpec = 0xA030;
        private const ushort kMatShin = 0xA040;
        private const ushort kMatShinPow = 0xA041;
        private const ushort kMatTransparency = 0xA050;
        private const ushort kMatTransFalloff = 0xA052;
        private const ushort kMatRefBlur = 0xA053;
        private const ushort kMatEmis = 0xA080;
        private const ushort kMatTwoSided = 0xA081;
        private const ushort kMatTransAdd = 0xA083;
        private const ushort kMatSelfIlum = 0xA084;
        private const ushort kMatWireOn = 0xA085;
        private const ushort kMatWireThickness = 0xA087;
        private const ushort kMatFaceMap = 0xA088;
        private const ushort kMatTransFalloffIN = 0xA08A;
        private const ushort kMatSoften = 0xA08C;
        private const ushort kMatShader = 0xA100;
        private const ushort kMatTexMap = 0xA200;
        private const ushort kMatTexFLNM = 0xA300;
        private const ushort kMatTexTile = 0xA351;
        private const ushort kMatTexBlur = 0xA353;
        private const ushort kMatTexUscale = 0xA354;
        private const ushort kMatTexVscale = 0xA356;
        private const ushort kMatTexUoffset = 0xA358;
        private const ushort kMatTexVoffset = 0xA35A;
        private const ushort kMatTexAngle = 0xA35C;
        private const ushort kMatTexCol1 = 0xA360;
        private const ushort kMatTexCol2 = 0xA362;
        private const ushort kMatTexColR = 0xA364;
        private const ushort kMatTexColG = 0xA366;
        private const ushort kMatTexColB = 0xA368;

        // keyframe chunks

        // start of keyframe info
        private const ushort kChunkKeyFrame = 0xB000;
        private const ushort kAmbientNodeTag = 0xB001;
        private const ushort kObjectNodeTag = 0xB002;
        private const ushort kCameraNodeTag = 0xB003;
        private const ushort kTargetNodeTag = 0xB004;
        private const ushort kLightNodeTag = 0xB005;
        private const ushort kLTargetNodeTag = 0xB006;
        private const ushort kSpotlightNodeTag = 0xB007;

        private const ushort kKeyFrameSegment = 0xB008;
        private const ushort kKeyFrameCurtime = 0xB009;
        private const ushort kKeyFrameHdr = 0xB00A;
        private const ushort kKeyFrameNodeHdr = 0xB010;
        private const ushort kKeyFrameDummyName = 0xB011; // when tag is $$$DUMMY
        private const ushort kKeyFramePrescale = 0xB012;
        private const ushort kKeyFramePivot = 0xB013;
        private const ushort kBoundingBox = 0xB014;
        private const ushort kMorphSmooth = 0xB015;
        private const ushort kKeyFramePos = 0xB020;
        private const ushort kKeyFrameRot = 0xB021;
        private const ushort kKeyFrameScale = 0xB022;
        private const ushort kKeyFrameFov = 0xB023;
        private const ushort kKeyFrameRoll = 0xB024;
        private const ushort kKeyFrameCol = 0xB025;
        private const ushort kKeyFrameMorph = 0xB026;
        private const ushort kKeyFrameHot = 0xB027;
        private const ushort kKeyFrameFall = 0xB028;
        private const ushort kKeyFrameHide = 0xB029;
        private const ushort kKeyFrameNodeID = 0xB030;

        private const ushort kChunkEnd = 0xffff;

        /// <summary>
        /// My Data
        /// </summary>
        public ChunkData RootChunk = null;
        public List<ChunkData> Chunks = new List<ChunkData>();


        public void SaveMesh(string fileName, List<MeshData> meshes)
        {
            FileStream file = File.Create(fileName);
            BinaryWriter wr = new BinaryWriter(file);

            StreamWriter tr = File.CreateText("Debug3ds.txt");

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            ChunkData   _main = new ChunkData(kMagic3ds);
            ChunkData _version = new ChunkData(kVersionMax);
            _version.CustomData =
            _version.CustomData = BitConverter.GetBytes((uint)3);

            ChunkData   _3deditor = new ChunkData(kChunkObjects);

            Dictionary<string, Material> usedMaterials = new Dictionary<string, Material>();

            
            foreach (MeshData meshExport in meshes)
            {

                SortedDictionary<int, Group> sortDict = new SortedDictionary<int, Group>();
                foreach (Group g in meshExport.Groups)
                {
                    sortDict.Add(g.FaceStart, g);
                }

                ///Materials
                foreach (Material m in meshExport.Materials)
                {

                    if (usedMaterials.ContainsKey(m.Name) == false)
                    {

                        ChunkData _material1 = new ChunkData(kMat);
                        ChunkData _materialName = new ChunkData(kMatName);

                        _materialName.CustomData = encoding.GetBytes(m.Name + "\0");
                        _material1.Children.Add(_materialName);

                        _3deditor.Children.Add(_material1);

                        usedMaterials.Add(m.Name, m);
                    }
                }

                ChunkData _objectBlock = new ChunkData(kNamedObject);
                ChunkData _objectMesh = new ChunkData(kObjMesh);

                _objectBlock.CustomData = encoding.GetBytes(meshExport.Name + "\0");

                //root.CustomData = new byte[reserved.Length + sizeof(uint)];
                //Array.Copy(reserved,root.CustomData,10);
                //Array.Copy(BitConverter.GetBytes(version),0,root.CustomData,10,4);

                //Write vertices //////////////////////////////
                ChunkData _vertices = new ChunkData(kMeshVerts);

                ushort count = (ushort)meshExport.Vertexs.Count;
                _vertices.CustomData = new byte[sizeof(ushort) + meshExport.Vertexs.Count * 3 * 4];
                Array.Copy(BitConverter.GetBytes(count), _vertices.CustomData, sizeof(ushort));
                int offset = sizeof(ushort);
                int c = 0;

                foreach (Vertex v in meshExport.Vertexs)
                {
                    JVector rpos = v.pos;
                    /**
                    if (meshExport.ParentMeshObject != null)
                    {
                        rpos = rpos * meshExport.ParentMeshObject.GlobalPosition;
                    }
                    /**/

                    Array.Copy(BitConverter.GetBytes(rpos.x), 0, _vertices.CustomData, offset, 4);
                    offset += 4;
                    Array.Copy(BitConverter.GetBytes(rpos.y), 0, _vertices.CustomData, offset, 4);
                    offset += 4;
                    Array.Copy(BitConverter.GetBytes(rpos.z), 0, _vertices.CustomData, offset, 4);
                    offset += 4;

                    c++;
                }
                //// Write material mappings
                ChunkData _uvs = new ChunkData(kMeshTexVert);
                _uvs.CustomData = new byte[sizeof(ushort) + meshExport.UVs.Count * 2 * 4];
                count = (ushort)meshExport.UVs.Count;
                Array.Copy(BitConverter.GetBytes(count), _uvs.CustomData, sizeof(ushort));
                offset = sizeof(ushort);

                foreach (UV uv in meshExport.UVs)
                {
                    Array.Copy(BitConverter.GetBytes(uv.u), 0, _uvs.CustomData, offset, 4);
                    offset += 4;
                    Array.Copy(BitConverter.GetBytes(uv.v * -1), 0, _uvs.CustomData, offset, 4);
                    offset += 4;
                }


                //// Write faces////////////////////////////////
                ChunkData _faces = new ChunkData(kMeshFaces);
                count = (ushort)meshExport.Faces.Count;
                _faces.CustomData = new byte[sizeof(ushort) + meshExport.Faces.Count * 4 * 2];
                Array.Copy(BitConverter.GetBytes(count), _faces.CustomData, sizeof(ushort));
                offset = sizeof(ushort);
                c = 0;
                int add = 0;
                foreach (Face f in meshExport.Faces)
                {
                    if (sortDict.ContainsKey(c) == true) add = sortDict[c].VertexStart;
                    Array.Copy(BitConverter.GetBytes((ushort)add + f.v1), 0, _faces.CustomData, offset, 2);
                    offset += 2;
                    Array.Copy(BitConverter.GetBytes((ushort)add + f.v2), 0, _faces.CustomData, offset, 2);
                    offset += 2;
                    Array.Copy(BitConverter.GetBytes((ushort)add + f.v3), 0, _faces.CustomData, offset, 2);
                    offset += 2;
                    Array.Copy(BitConverter.GetBytes((ushort)3), 0, _faces.CustomData, offset, 2);
                    offset += 2;
                    c++;
                }

                ///Groupings
                c = 0;
                foreach (Group g in meshExport.Groups)
                {
                    ChunkData _meshMaterial = new ChunkData(kMeshMaterial);

                    int id = g.Id;
                    if (id >= meshExport.Materials.Count)
                    {
                        id = 0;
                    }
                    byte[] name = encoding.GetBytes(meshExport.Materials[id].Name + "\0");
                    ushort numberOfEntries = (ushort)g.FaceCount;
                    ushort[] entries = new ushort[g.FaceCount];
                    for (int ff = 0; ff < g.FaceCount; ff++)
                    {
                        entries[ff] = (ushort)(g.FaceStart + ff);
                    }
                    _meshMaterial.CustomData = new byte[name.Length + 2 + entries.Length * 2];

                    byte[] centries = new byte[entries.Length * 2];

                    offset = 0;
                    Array.Copy(name, 0, _meshMaterial.CustomData, offset, name.Length);
                    offset += name.Length;
                    Array.Copy(BitConverter.GetBytes(numberOfEntries), 0, _meshMaterial.CustomData, offset, 2);
                    offset += 2;

                    int o = 0;
                    foreach (ushort v in entries)
                    {
                        Array.Copy(BitConverter.GetBytes(v), 0, centries, o, 2);
                        o += 2;
                    }
                    Array.Copy(centries, 0, _meshMaterial.CustomData, offset, centries.Length);


                    _faces.Children.Add(_meshMaterial);
                    c++;
                }

                /// Local Matrix
                /// 
                
                ChunkData _localMatrix = new ChunkData(kMeshXFMatrix);
                _localMatrix.CustomData = new byte[48];

                JMatrix local = new JMatrix();
                /**/
                if (meshExport.ParentMeshObject != null && meshExport.OverrideLocalMatrix == null)
                {
                    //local = meshExport.ParentMeshObject.GlobalPosition;
                    local = meshExport.ParentMeshObject.LocalPosition;
                }
                else
                {
                    local = meshExport.OverrideLocalMatrix;
                }
                /**/
                JVector axisx = local.RowX;
                JVector axisy = local.RowY;
                JVector axisz = local.RowZ;
                JVector pos = local.Pos;
                Array.Copy(axisx.GetBytes3x(), 0, _localMatrix.CustomData, 0, 12);
                Array.Copy(axisy.GetBytes3x(), 0, _localMatrix.CustomData, 12, 12);
                Array.Copy(axisz.GetBytes3x(), 0, _localMatrix.CustomData, 24, 12);
                Array.Copy(pos.GetBytes3x(), 0, _localMatrix.CustomData, 36, 12);

                _objectMesh.Children.Add(_vertices);
                _objectMesh.Children.Add(_uvs);
                _objectMesh.Children.Add(_faces);
                _objectMesh.Children.Add(_localMatrix);

                _objectBlock.Children.Add(_objectMesh);
                _3deditor.Children.Add(_objectBlock);
            }

 
            ChunkData _keyFrameChunk = new ChunkData(kChunkKeyFrame);
            ///Object hierarchy
            foreach (MeshData meshExport in meshes)
            {
                /// Frame segment
                ChunkData _keyFrameSegment = new ChunkData(kObjectNodeTag);

                /**/
                ChunkData _keyObjectNode = new ChunkData(kKeyFrameNodeHdr);

                byte[] name = encoding.GetBytes(meshExport.Name + "\0");

                _keyObjectNode.CustomData = new byte[name.Length + 3 * 2];
                int offset = 0;
                Array.Copy(name, 0, _keyObjectNode.CustomData, offset, name.Length);
                offset += name.Length;
                Array.Copy(BitConverter.GetBytes((short)0), 0, _keyObjectNode.CustomData, offset, sizeof(short));
                offset += 2;
                Array.Copy(BitConverter.GetBytes((short)0), 0, _keyObjectNode.CustomData, offset, sizeof(short));
                offset += 2;
                Array.Copy(BitConverter.GetBytes((short)meshExport.HierarchyLevel), 0, _keyObjectNode.CustomData, offset, sizeof(short));
                offset += 2;


                
                /**/
                ChunkData _keyObjectNodeId = new ChunkData(kKeyFrameNodeID);
                _keyObjectNodeId.CustomData = BitConverter.GetBytes((short)meshExport.ModelNumber);
                _keyFrameSegment.Children.Add(_keyObjectNodeId);
                /**/

                _keyFrameSegment.Children.Add(_keyObjectNode);
                _keyFrameChunk.Children.Add(_keyFrameSegment);
            }


            
            //_3deditor.Children.Add(_keyFrameChunk);
            
            _main.Children.Add(_version);
            _main.Children.Add(_3deditor);
            _main.Children.Add(_keyFrameChunk);
            uint size = WriteChunk(ref _main, wr);


            TraverseChunks(_main, 0, tr);

            wr.Close();
            file.Close();

            tr.Close();
        }

        public void TraverseChunks(ChunkData parent, int level, StreamWriter tr)
        {
            for (int i = 0; i < level; i++)  tr.Write("\t");
            string line = string.Empty;

            line = string.Format("Identifier:{0:x}\r\n", parent.Identifier);
            tr.Write(line);
            line = string.Format("Lenght:{0}\r\n", parent.Lenght);
            for (int i = 0; i < level; i++) tr.Write("\t");
            tr.Write(line);

            for (int j = 0; j < parent.Children.Count; j++)
            {
                ChunkData child = parent.Children[j];
                TraverseChunks(child, level + 1, tr);
            }
        }

        public uint WriteChunk(ref ChunkData parent, BinaryWriter wr)
        {
            wr.Write(parent.Identifier);
            uint len = 0;
            uint lenOffset = (uint)wr.BaseStream.Position;
            wr.Write(len);
            uint custom_lenght = 6;
            if (parent.CustomData != null)
            {
                wr.Write(parent.CustomData);
                custom_lenght += (uint)parent.CustomData.Length;
            }

            uint alllenght = custom_lenght;
            for(int j=0;j<parent.Children.Count;j++)
            {
                ChunkData child = parent.Children[j];
                uint lenght = WriteChunk(ref child, wr);
                alllenght += lenght;
            }
            long curOffset = wr.BaseStream.Position;
            wr.BaseStream.Seek(lenOffset, SeekOrigin.Begin);
            wr.Write(alllenght);
            wr.BaseStream.Seek(curOffset, SeekOrigin.Begin);
            parent.Lenght = (uint)alllenght;

            return alllenght;
        }


        public void LoadAndDumpChunks(string fileName)
        {
            Chunks.Clear();
            FileStream file = File.OpenRead(fileName);

            if (file == null) return;
            BinaryReader br = new BinaryReader(file);
            
            
            while (br.BaseStream.Position != br.BaseStream.Length)
            {
                ChunkData c = new ChunkData();
                c.Id = Chunks.Count;
                c.Parent = null;
                c.Identifier = br.ReadUInt16();
                c.Lenght = br.ReadUInt32();

                Chunks.Add(c);

                int count = 0;
                int i = 0;

                switch (c.Identifier)
                {
                    case 0x4d4d:
                    break;


                    case 0x3d3d:
                    break;

                    case 0x4000:
                    {
                        byte l_char;
                        string object_name = string.Empty;
                        do
                        {
                            l_char = br.ReadByte();
                            object_name += Convert.ToChar(l_char);
                            i++;
                        } while (l_char != '\0' && i < 20);
                    }
                    break;


                    case 0x4100:
                    break; 

                    case 0x4110:
                        count = br.ReadUInt16();

                        for (i = 0; i < count; i++)
                        {
                            float x = br.ReadSingle();
                            float y = br.ReadSingle();
                            float z = br.ReadSingle();
                        }
                    break; 

                    case 0x4120:
                        count = br.ReadUInt16();
                    
                    for (i=0; i<count; i++)
                    {
                        ushort v1   = br.ReadUInt16();
                        ushort v2   = br.ReadUInt16();
                        ushort v3   = br.ReadUInt16();
                        ushort flags = br.ReadUInt16();
                      
                    }
                    break; 

                    case 0x4140:
                        count = br.ReadUInt16();

                        for (i = 0; i < count; i++)
                        {
                            float u = br.ReadSingle();
                            float v = br.ReadSingle();
                        }
                    break; 

                    default:
                        br.BaseStream.Seek(c.Lenght - 6, SeekOrigin.Current);
                    break;
                }
            }
            br.Close();
            file.Close();
        }

       
    }
}
