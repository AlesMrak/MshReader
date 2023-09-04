using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

//using Microsoft.DirectX.Direct3D;
//using Microsoft.DirectX;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MshReader
{

    public class XValue
    {
        public int CountZero;
        public int HeaderVal;
        public int Offset;
    }
    public class MshItem
    {
        public long StartOffset = 0;
        public long EndOffset = 0;
        public long Lenght = 0;
        public int Index = 0;
        public int Number1 = 0;
        public int Number2 = 0;
        public int Number3 = 0;
        public byte Number4 = 0;
        public byte Number5 = 0;
    }

    public class Material
    {
        public string Name;
        List<MaterialLayer> Layers = new List<MaterialLayer>();
        public int tfDoubleSide = 0;
        public int tfShouldSort=0;
        public int tfDropShadow=0;
        public int tfGameTimer=1;
        //Light Params
        public double Ambient=1.0;
        public double Diffuse = 1.0;
        public double Specular = 0.4;
        public double SpecularPow = 32;
        public double Shine = 0.0;

    }

    public class MaterialLayer
    {
        public string TextureFileName = string.Empty;
        public string ClassName;
        public int tfModulate=1;
        public int tfNoWriteZ=1;            
        public int tfMinLinear=1;            
        public int tfMagLinear=1;           
        public int tfBlend=1;
        public int tfMipMap=0;
        public int tfNoDegradation = 1;
    }

    public class UV
    {
        public float u;
        public float v;
    }

    public class Face
    {
        public int v1;
        public int v2;
        public int v3;
    }

    public class Group
    {
        public int Id = -1;
        public int VertexStart = 0;
        public int VertexCount = 0;
        public int FaceStart = 0;
        public int FaceCount = 0;
    }

    public class Vertex
    {
        public float posx;
        public float posy;
        public float posz;

        public float u;
        public float v;
        public float z;

        public float nx;
        public float ny;
        public float nz;
    }

    public class MeshCollision
    {
        public string               Name = string.Empty;
        public List<Vertex>         Vertexs = new List<Vertex>();
        public List<Face>           Faces = new List<Face>();
        public List<int>            Neigbours = new List<int>();
    }

    public class Hook
    {
        public string               Name = string.Empty;
        public Matrix               Position;
    }

    public class Mesh
    {
        public Matrix               LocalPosition;
        public Matrix               GlobalPosition;
        /// <summary>
        /// Hier data
        /// </summary>
        public string               ParentName = string.Empty;
        public int                  ParentIndex = -1;
        public string               Name = string.Empty;
        /// <summary>
        /// Msh data
        /// </summary>
        public List<Material>       Materials= new List<Material>();
        public List<Face>           Faces = new List<Face>();
        public List<Vertex>         Vertexs = new List<Vertex>();
        public List<UV>             UVs = new List<UV>();
        public List<Vertex>         TangentSpace = new List<Vertex>();
        public List<Group>          Groups = new List<Group>();
        public List<Hook>           Hooks = new List<Hook>();

        public List<Mesh>           Lods = new List<Mesh>();
        public List<int>            LodsDist = new List<int>();
        public List<MeshCollision>  Collisions = new List<MeshCollision>();

        public List<Mesh>           Children = new List<Mesh>();

        /// <summary>
        /// Private data
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// 
        int AllFileSize = 0;
        int CountX = 0;
        int FirstOffset = 0;
        int SecondOffset = 0;
        int ThirdOffset = 0;
        int OffsetToData = 0;
        int AllZeroCount = 0;
        int RealZeroCount = 0;
        int CountBytesX = 0;
        int FirstZeroCount = 0;
        byte[] SecondData = null;
        byte[] FileSignature = null;
        FileStream file = null;
        BinaryReader br = null;

        public int CountOfStrings = 0;
        public List<string> Strings = new List<string>();
        public List<string> StringFirstValues = new List<string>();
        public List<MshItem> ItemsList = new List<MshItem>();
        public List<string> XCount4 = new List<string>();
        public List<byte> XView = new List<byte>();
        public List<byte> XViewSort = new List<byte>();
        public List<int> ItemMapping = new List<int>();
        public int XViewAllCount = 0;
        public int AllDataBytes = 0;
        public List<XValue> XHeaderData = new List<XValue>();

        public bool LoadMsh(string fileName)
        {
            AllFileSize = 0;
            CountX = 0;
            FirstOffset = 0;
            SecondOffset = 0;
            ThirdOffset = 0;
            OffsetToData = 0;
            AllZeroCount = 0;
            RealZeroCount = 0;
            CountBytesX = 0;
            FirstZeroCount = 0;
            SecondData = null;

            file = File.OpenRead(fileName);
            br = new BinaryReader(file);
            ///Read header
            FileSignature = br.ReadBytes(4);
            
            if (FileSignature[0] != 1 && FileSignature[1] != 66 && FileSignature[2] != 83 && FileSignature[3] != 0)
                return false;

            AllFileSize = br.ReadInt32();
            CountOfStrings = br.ReadInt32();
            CountX = br.ReadInt32();
            FirstOffset = br.ReadInt32();
            SecondOffset = br.ReadInt32();
            ThirdOffset = br.ReadInt32();

            ///Read strings
            string str = string.Empty;
            bool readsize = true;
            while (br.BaseStream.Position < FirstOffset)
            {
                byte b = br.ReadByte();

                if (readsize == true)
                {
                    if (str != string.Empty) Strings.Add(str);
                    str = string.Empty;
                    readsize = false;
                }
                else if (b == 0)
                {
                    readsize = true;
                }
                else
                {
                    str += Convert.ToChar(b);
                }
            }
            if (str != string.Empty) Strings.Add(str);

            //Read data offset
            long oldPos = br.BaseStream.Position;
            br.BaseStream.Seek(this.ThirdOffset, SeekOrigin.Begin);
            OffsetToData = br.ReadInt32();
            br.BaseStream.Seek(oldPos, SeekOrigin.Begin);

            //Read first data
            while (br.BaseStream.Position < this.SecondOffset)
            {
                int num1 = (int)br.ReadInt32();
                int num2 = (int)br.ReadInt32();
                int num3 = br.ReadInt32();
                byte num4 = br.ReadByte();
                byte num5 = br.ReadByte();

                MshItem item = new MshItem();
                item.Number1 = num1;
                item.Number2 = num2;
                item.Number3 = num3;
                item.Number4 = num4;
                item.Number5 = num5;

                ItemsList.Add(item);

                StringFirstValues.Add(String.Format("{0} {1} {2} {3} {4}", num1, num2, num3, num4,num5));
            }

            //Read second data
            long old = br.BaseStream.Position;
            br.BaseStream.Seek(this.SecondOffset, SeekOrigin.Begin);
            this.SecondData = br.ReadBytes(this.ThirdOffset - this.SecondOffset);
            br.BaseStream.Seek(old, SeekOrigin.Begin);
            CountBytesX = this.SecondData.Length - this.CountOfStrings;

            //Read x count header
            int count = 0;
            old = br.BaseStream.Position;
            br.BaseStream.Seek(this.ThirdOffset + 4, SeekOrigin.Begin);
            while (br.BaseStream.Position < this.OffsetToData)
            {
                int num1 = (int)br.ReadInt32();

                ItemMapping.Add(num1);

                AllDataBytes += num1;
                XCount4.Add(String.Format("{0}", num1));
                count++;

            }
            /*
            //Read unknown
            count = 0;
            old = br.BaseStream.Position;
            br.BaseStream.Seek(this.ThirdOffset - this.CountOfStrings, SeekOrigin.Begin);
            while (br.BaseStream.Position < this.ThirdOffset)
            {
                byte b = (byte)br.ReadByte();

                //XView.Add(String.Format("{0}", b));
                XView.Add(b);
                XViewSort.Add(b);
                //XViewAllCount += b;
                count++;
            }
            XViewSort.Reverse();

            /// Other data
            /// 

            int lastPos = -1;
            int countx = 0;

            bool firstVal = false;
            br.BaseStream.Seek(this.SecondOffset, SeekOrigin.Begin);
            int PosX = 0;
            XValue v = null;

            while (br.BaseStream.Position < this.ThirdOffset - this.CountOfStrings)
            {
                byte b = (byte)br.ReadByte();
                if (b == 0)
                {
                    countx++;
                    RealZeroCount++;
                }
                else
                {
                    if (v != null)
                    {
                        v.CountZero = countx;
                        for (int i = 0; i < v.HeaderVal; i++)
                        {
                            XHeaderData.Add(v);
                            AllZeroCount += v.CountZero;
                        }

                    }
                    else if (v == null)
                    {
                        FirstZeroCount = countx;
                        AllZeroCount += countx;
                    }
                    v = new XValue();
                    v.HeaderVal = b;
                    lastPos = b;
                    v.Offset = PosX;
                    countx = 0;
                    XViewAllCount += b;
                }
                PosX++;

            }
            if (v != null)
            {
                v.CountZero = countx;
                for (int i = 0; i < v.HeaderVal; i++)
                {
                    XHeaderData.Add(v);
                    AllZeroCount += v.CountZero;

                }
            }

            br.BaseStream.Seek(old, SeekOrigin.Begin);
            //Sample data
            /*
            count = 0;
            old = br.BaseStream.Position;
            br.BaseStream.Seek(this.OffsetToData, SeekOrigin.Begin);

            for (int j = 0; j < ItemsList.Count; j++)
            {
                MshItem item = (MshItem)ItemsList[j];
                item.StartOffset = br.BaseStream.Position;
                int startItem = item.Number2;
                int countItem = item.Number3;

                for (int i = 0; i < countItem; i++)
                {
                    int size = (int)ItemMapping[startItem + i];
                    byte[] num = br.ReadBytes(size);
                }

                item.EndOffset = br.BaseStream.Position;
                item.Lenght = item.EndOffset - item.StartOffset;
            }

            br.BaseStream.Seek(old, SeekOrigin.Begin);
            /**/
            InternalData();


            return true;
        }

        protected void InternalData()
        {
            int count = 0;
            long old = br.BaseStream.Position;
            br.BaseStream.Seek(this.OffsetToData, SeekOrigin.Begin);
            /// Obsolete
            bool firsVertex = true;
            bool firstUV = true;
            bool firstFaces = true;
            bool firstVertex = true;
            ////

            int allreaddata = 0;
            foreach (MshItem item in ItemsList)
            {
                string def = (string)Strings[count];

                int startItem = item.Number2;
                int countItem = item.Number3;

                if (def == "[Common]")
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);

                        startPos = lastPos;
                        if (Name == "NumBones")
                        {
                            int numBones = GetFirstInt(startPos + 1, num, ref lastPos);
                        }
                        else if (Name == "FramesType")
                        {
                            string frameType = GetFirstString(startPos, num, ref lastPos);
                        }
                        else if (Name == "NumFrames")
                        {
                            int numFrames = GetFirstInt(startPos + 1, num, ref lastPos);
                        }
                        else
                        {

                            string str = string.Empty;
                            for (int j = lastPos; j < num.Length; j++)
                            {
                                str += Convert.ToInt32(num[j]).ToString() + " ";
                            }
                        }
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def == "[Bones]")
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def == "[LOD]")
                {
                    int c = 0;
                    // LOD count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int lastPos = 0;
                        int startPos = 0;
                        int Lod = GetFirstInt(startPos, num, ref lastPos);
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def.Contains("Materials") == true)
                {
                    int c = 0;
                    // Materials count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        /**/
                        string material = ReadString(false);
                        if (material.Length > size)
                        {
                            throw new Exception("Error");
                        }
                        allreaddata += size;
                        c += size;

                    }
                }
                else if (def.Contains("FaceGroups]"))
                {
                    int c = 0;
                    //Face groups count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        string str = string.Empty;
                        for (int j = 0; j < num.Length; j++)
                        {
                            str += String.Format("{0:x2} ", (sbyte)num[j]);
                        }
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def.Contains("Vertices_Frame0]"))
                {
                    int c = 0;
                    //Vertices Frame0 count
                    float[] nums = new float[6];
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        /**/
                        float num1 = 0;

                        /**/
                        int countx = size / 6;
                        if (countx == 4)
                        {

                            for (int j = 0; j < 6; j++)
                            {
                                num1 = br.ReadSingle();
                                nums[j] = num1;
                            }

                            if (firstVertex == true)
                            {
                                Vertex v = new Vertex();
                                v.posx = nums[0];
                                v.posy = nums[1];
                                v.posz = nums[2];

                                v.nx = nums[3];
                                v.ny = nums[4];
                                v.nz = nums[5];
                            }
                            allreaddata += size;
                            c += size;
                        }
                        else if (countx == 3)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                byte[] numb = { 0, 0, 0, 0 };
                                byte[] numb2 = br.ReadBytes(3);
                                numb[2] = numb2[0];
                                numb[1] = numb2[1];
                                numb[0] = numb2[2];

                                int num2 = BitConverter.ToInt16(numb, 0);
                                num1 = (float)num2 / 1000;
                                //num1 = BitConverter.ToSingle(numb, 0);
                            }
                            allreaddata += size;
                            c += size;
                        }
                        else
                        {
                            /**/
                            byte[] num = br.ReadBytes(size);
                            string str = string.Empty;
                            for (int j = 0; j < num.Length; j++)
                            {
                                str += String.Format("{0:x2} ", Convert.ToInt32(num[j]));
                            }
                            allreaddata += num.Length;
                            c += num.Length;
                            /**/
                        }
                    }
                }
                else if (def.Contains("Space_Frame0]"))
                {
                    int c = 0;
                    //Space Frame0 count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];

                        int read = 0;
                        string str = string.Empty;
                        if (size - read > 0)
                        {
                            byte[] num = br.ReadBytes(size - read);
                            for (int j = 0; j < num.Length; j++)
                            {
                                str += Convert.ToInt32(num[j]).ToString() + " ";
                            }

                            allreaddata += num.Length;
                            c += num.Length;
                        }
                    }
                }
                else if (def.Contains("MaterialMapping]"))
                {
                    int c = 0;
                    //MaterialMapping count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        string str = string.Empty;

                        short u = BitConverter.ToInt16(num, 0);
                        short v = BitConverter.ToInt16(num, 3);
                        float fu = (float)u / (float)65536.0;
                        float fv = (float)v / (float)65536.0;

                        if (firstUV == true)
                        {
                            UV uv = new UV();
                            uv.u = fu;
                            uv.v = fv;
                        }
                        str += String.Format("{0} {1} ", fu, fv);
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def.Contains("Faces]"))
                {
                    int c = 0;
                    int lastv = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int startPos = 0;
                        int lastPos = 0;
                        int countf = size / 3;
                        int v1 = 0;
                        int v2 = 0;
                        int v3 = 0;

                        sbyte[] snum = new sbyte[size];
                        System.Buffer.BlockCopy(num, 0, snum, 0, size);

                        if (countf == 1)
                        {
                            v1 = snum[0];
                            v2 = snum[1];
                            v3 = snum[2];

                            lastv = v1 = lastv + v1;
                            lastv = v2 = lastv + v2;
                            lastv = v3 = lastv + v3;

                            if (firstFaces == true)
                            {
                                Face f = new Face();
                                f.v1 = v1;
                                f.v2 = v2;
                                f.v3 = v3;
                            }
                        }
                        else
                        {
                            v1 = GetFirstIntCount(startPos, countf, num, ref lastPos);
                            v2 = GetFirstIntCount(lastPos, countf, num, ref lastPos);
                            v3 = GetFirstIntCount(lastPos, countf, num, ref lastPos);

                            lastv = v1 = lastv + v1;
                            lastv = v2 = lastv + v2;
                            lastv = v3 = lastv + v3;

                            if (firstFaces == true)
                            {
                                Face f = new Face();
                                f.v1 = v1;
                                f.v2 = v2;
                                f.v3 = v3;
                            }
                        }
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def == "[Hooks]")
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        string Val = GetFirstString(lastPos, num, ref lastPos);
                        
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def == "[FrameNames]")
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def.Contains("CoCommon"))
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        startPos = lastPos;
                        if (Name == "NBlocks")
                        {
                            int val = GetFirstInt(startPos + 1, num, ref lastPos);
                        }
                        else if (Name == "NParts")
                        {
                            int val = GetFirstInt(startPos + 1, num, ref lastPos);
                        }
                        else if (Name == "ParentBone")
                        {
                            int val = GetFirstInt(startPos + 1, num, ref lastPos);
                        }
                        else if (Name == "Type")
                        {
                            string val = GetFirstString(startPos, num, ref lastPos);
                        }
                        else if (Name == "NFrames")
                        {
                            int val = GetFirstInt(startPos + 1, num, ref lastPos);
                        }
                        else if (Name == "Name")
                        {
                            string val = GetFirstString(startPos, num, ref lastPos);
                        }
                        else
                        {
                            str = string.Empty;
                            for (int j = lastPos; j < num.Length; j++)
                            {
                                str += Convert.ToInt32(num[j]).ToString() + " ";
                            }
                        }

                        c += num.Length;
                        allreaddata += num.Length;
                    }
                }
                else
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;
                        for (int j = 0; j < num.Length; j++)
                        {
                            str += Convert.ToInt32(num[j]).ToString() + " ";
                        }
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                count++;
            }
            br.BaseStream.Seek(old, SeekOrigin.Begin);
        }


        protected string ReadString(bool nullt)
        {
            string str = string.Empty;
            byte sizeb = br.ReadByte();
            int offset = 0;
            if (nullt == false) offset = 1;
            for (int k = 0; k < sizeb-offset; k++)
            {
                byte b = br.ReadByte();
                if (b != 0)
                    str += Convert.ToChar(b);
            }
            return str;
        }

        
        protected string GetFirstString(int offset,byte[] array,ref int lastpos)
        {
            string res = string.Empty;
            char[] p = { '\0' };
            int len = 0;
            int lenc = 0;
            bool startstr = true;
            string str = string.Empty;
            int i = offset;
            for (i = offset; i < array.Length; i++)
            {
                if (startstr == true)
                {
                    len = array[i];
                    lenc = 0;
                    if (str != string.Empty)
                    {
                        res = res.TrimEnd(p);
                    }
                    startstr = false;
                    res = string.Empty;
                }
                else
                {
                    lenc++;
                    if (lenc >= len)
                    {
                        break;
                    }
                    else
                    {
                        res += Convert.ToChar(array[i]);
                    }
                }

            }
            if (res != string.Empty)
            {
                res.TrimEnd(p);
            }
            lastpos = i;

            return res;
        }

        protected int GetFirstInt(int offset,byte[] array, ref int lasPos)
        {
            int res = 0;

            int count = array.Length - offset;
            if (count > 4)
            {
                throw new Exception("Error");
            }

            if (count == 1)
            {
                res = (int)array[offset];
            }
            else if (count == 2)
            {
                short r = BitConverter.ToInt16(array, offset);
                res = (int)r;
            }
            else if (count == 3)
            {
                byte[] a = { array[offset], array[offset + 1], array[offset+2],0};
                res = BitConverter.ToInt32(a, 0);
            }
            else if (count == 4)
            {
                res = BitConverter.ToInt32(array, offset);
            }
            lasPos += count;

            return res;
        }

        protected int GetFirstIntCount(int offset,int count, byte[] array, ref int lasPos)
        {
            int res = 0;

            if (count > 4)
            {
                throw new Exception("Error");
            }

            if (count == 1)
            {
                byte s = (byte)array[offset];
                res = (int)s;
            }
            else if (count == 2)
            {
                short r = BitConverter.ToInt16(array, offset);
                res = (int)r;
            }
            else if (count == 3)
            {
                byte[] a = { array[offset], array[offset + 1], array[offset + 2], 0 };
                res = BitConverter.ToInt32(a, 0);
            }
            else if (count == 4)
            {
                res = BitConverter.ToInt32(array, offset);
            }
            lasPos += count;

            return res;
        }

        protected float GetFirstFloat(int offset,int count, byte[] array, ref int lasPos)
        {
            float res = 0;

            //int count = array.Length - offset;
            if (count > 4)
            {
                throw new Exception("Error");
            }

            if (count == 1)
            {
                sbyte s = (sbyte)array[offset];
                res = (float)s;
            }
            else if (count == 2)
            {
                byte[] a = { array[offset], array[offset + 1],0, 0 };
                res = BitConverter.ToSingle(a, 0);
            }
            else if (count == 3)
            {
                byte[] a = { 0,array[offset], array[offset + 1], array[offset+2]};
                res = BitConverter.ToSingle(a, 0);
            }
            else if (count == 4)
            {
                res = BitConverter.ToSingle(array, offset);
            }
            lasPos += count;
            return res;
        }
    }

    public class MeshObject
    {
        public Mesh Root = new Mesh();
        public string Name = string.Empty;
        public List<Mesh> Meshes = new List<Mesh>();


        public void LoadHier(string fileName)
        {

        }
               
    }
}
