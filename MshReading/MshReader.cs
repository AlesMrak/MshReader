using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;


namespace MshReader
{
    

    public class MSHReading
    {
        AsciiMshReader OBJWriter = new AsciiMshReader();
        bool firstFaces = true;
        bool firstVertex = true;
        bool firstUV = true;

        public FileStream file = null;
        public BinaryReader br = null;

        public int CountOfStrings = 0;
        public ArrayList Strings = new ArrayList();
        public ArrayList StringFirstValues = new ArrayList();
        public ArrayList ItemsList = new ArrayList();
        public ArrayList XCount4 = new ArrayList();
        public ArrayList XView = new ArrayList();
        public ArrayList XViewSort = new ArrayList();
        public ArrayList ItemMapping = new ArrayList();
        public int XViewAllCount = 0;
        public int AllDataBytes = 0;

        public ArrayList XHeaderData = new ArrayList();

        public int AllFileSize = 0;
        public int CountX = 0;
        public int FirstOffset = 0;
        public int SecondOffset = 0;
        public int ThirdOffset = 0;
        public int OffsetToData = 0;
        public int AllZeroCount = 0;
        public int RealZeroCount = 0;
        public int CountBytesX = 0;
        public int FirstZeroCount = 0;

        public byte[] SecondData = null;
        

        public byte[] FileSignature = null;

        public bool debugSizeOutput = true;

        public void Init()
        {
            XHeaderData.Clear();
            XViewSort.Clear();
            SecondData = null;
            AllDataBytes = 0;
            FileSignature = null;
            Strings.Clear();
            StringFirstValues.Clear();
            ItemsList.Clear();
            ItemMapping.Clear();
            XView.Clear();
            XCount4.Clear();
            XViewAllCount = 0;
            CountOfStrings = 0;
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

        }
        public void CloseFile()
        {
            if (br != null)
                br.Close();
            if (file != null)
                file.Close();

            Init();
        }

        public bool ReaderHeader()
        {
            if (br == null) return false;

            FileSignature = br.ReadBytes(4);

            if (FileSignature[0] != 1 && FileSignature[1] != 66 && FileSignature[2] != 83 && FileSignature[3] != 0)
                return false;
            AllFileSize = br.ReadInt32();
            CountOfStrings = br.ReadInt32();
            CountX = br.ReadInt32();
            FirstOffset = br.ReadInt32();
            SecondOffset = br.ReadInt32();
            ThirdOffset = br.ReadInt32();
            return true;

        }

        public void ReadStrings()
        {
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
            

        }

        public void OpenFile(string filename)
        {
            CloseFile();

            file = File.OpenRead(filename);
            br = new BinaryReader(file);

            bool res = ReaderHeader();
            if (res == false) return;
            ReadStrings();
            ReadDataOffset();
            ReadFirstData();
            ReadSecondData();
            ReadXcountHeader();
            ReadUknown();

            SampleData();
            
        }

        public void ReadDataOffset()
        {
            long oldPos = br.BaseStream.Position;

            br.BaseStream.Seek(this.ThirdOffset, SeekOrigin.Begin);

            OffsetToData = br.ReadInt32();
            br.BaseStream.Seek(oldPos, SeekOrigin.Begin);
        }

        public void ReadSecondData()
        {
            long old = br.BaseStream.Position;
            br.BaseStream.Seek(this.SecondOffset, SeekOrigin.Begin);
            this.SecondData = br.ReadBytes(this.ThirdOffset - this.SecondOffset);
            br.BaseStream.Seek(old, SeekOrigin.Begin);
            CountBytesX = this.SecondData.Length-this.CountOfStrings;

            /*
            //// Analize Second Data ////////
            for (int i = 0; i <  SecondData.Length; i++)
            {
                byte b = SecondData[i];
            }
            /**/
        }

        public void ReadFirstData()
        {
            while (br.BaseStream.Position < this.SecondOffset)
            {
                int num1 = (int)br.ReadInt32();
                int num2 = (int)br.ReadInt32();
                short num3 = br.ReadInt16();
                byte num4 = br.ReadByte();
                byte num5 = br.ReadByte();

                MshItem item = new MshItem();
                item.Number1 = num1;
                item.Number2 = num2;
                item.Number3 = num3;
                item.Number4 = num4;
                item.Number5 = num5;

                ItemsList.Add(item);

                StringFirstValues.Add(String.Format("{0} {1} {2} {3} {4}",num1,num2,num3,num4,num5));
            }

        }

        public void ReadXcountHeader()
        {
            int count = 0;
            long old = br.BaseStream.Position;
            br.BaseStream.Seek(this.ThirdOffset+4, SeekOrigin.Begin);
            while (br.BaseStream.Position < this.OffsetToData)
            {
                int num1 = (int)br.ReadInt32();

                ItemMapping.Add(num1);

                AllDataBytes += num1;
                XCount4.Add(String.Format("{0}",num1));
                count++;

            }
            //if (count != this.CountX) throw new Exception("Error");

            br.BaseStream.Seek(old, SeekOrigin.Begin);
        }

        public void ReadUknown()
        {
            int count = 0;
            long old = br.BaseStream.Position;
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
                    RealZeroCount ++;
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
        }

        public void SampleData()
        {
            int count = 0;
            long old = br.BaseStream.Position;
            br.BaseStream.Seek(this.OffsetToData, SeekOrigin.Begin);
            /**/
            for (int j = 0; j < ItemsList.Count;j++ )
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
            /**/
            br.BaseStream.Seek(old, SeekOrigin.Begin);
        }

        public void SaveAscii(string filename)
        {
            TextWriter tw = new StreamWriter(filename);

            OBJWriter.Clear();
            firstUV = true;
            firstFaces = true;
            firstVertex = true;

            int count = 0;
            long old = br.BaseStream.Position;
            br.BaseStream.Seek(this.OffsetToData, SeekOrigin.Begin);

            int allreaddata = 0;
            foreach (MshItem item in ItemsList)
            {
                string def = (string)Strings[count];

                int startItem = item.Number2;
                int countItem = item.Number3;

                if (def == "[Common]")
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("[Common] //counts {0}",countItem);
                    
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int lastPos=0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos,num, ref lastPos);

                        startPos = lastPos;
                        if(Name=="NumBones")
                        {
                            int numBones = GetFirstInt(startPos+1, num, ref lastPos);
                            tw.Write("{0} {1}", Name, numBones);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }
                        else if(Name=="FramesType")
                        {
                            string frameType = GetFirstString(startPos, num, ref lastPos);
                            tw.Write("{0} {1}", Name, frameType);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }
                        else if (Name == "NumFrames")
                        {
                            int numFrames = GetFirstInt(startPos + 1, num, ref lastPos);
                            tw.Write("{0} {1}", Name, numFrames);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }
                        else
                        {
                            
                            string str = string.Empty;
                            for (int j = lastPos; j < num.Length; j++)
                            {
                                str += Convert.ToInt32(num[j]).ToString() + " ";
                            }
                            tw.Write("{0} {1}", Name,str);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                    if(debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                }
                else if (def == "[Bones]")
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("[Bones] //counts {0}", countItem);

                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        tw.Write("{0}", Name);
                        if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                        tw.Write("\r\n");
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                }
                else if (def == "[LOD]")
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("[LOD] //counts {0}", countItem);
                    // LOD count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int lastPos = 0;
                        int startPos = 0;
                        int Lod = GetFirstInt(startPos, num, ref lastPos);
                        tw.Write("{0}", Lod);
                        if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                        tw.Write("\r\n");

                        allreaddata += num.Length;
                        c += num.Length;

                        
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                }
                else if (def.Contains("Materials") == true)
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("{0} //counts {1}", def,countItem);
                    // Materials count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];

                        /**/
                        string material = ReadString(false);
                        tw.Write("{0}", material);
                        if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                        tw.Write("\r\n");
                        if (material.Length > size)
                        {
                            throw new Exception("Error");
                        }
                        allreaddata += size;
                        c += size;

                        Material m = new Material();
                        m.Name = material;
                        OBJWriter.Materials.Add(m);
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                }
                else if (def.Contains("FaceGroups]"))
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("{0} //counts {1}",def, countItem);
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
                        tw.Write(str);
                        if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                        tw.Write("\r\n");

                        allreaddata += num.Length;
                        c += num.Length;
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                }
                else if (def.Contains("Vertices_Frame0]"))
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("{0} //counts {1}",def, countItem);
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
                                tw.Write("{0} ", num1);
                                nums[j] = num1;
                                byte[] b = BitConverter.GetBytes(num1);
                                int dd = 1;
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
                                this.OBJWriter.Vertexs.Add(v);
                            }

                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                            allreaddata += size;
                            c += size;
                        }
                        else if (countx == 3)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                byte[] numb = { 0, 0, 0, 0 };
                                byte[] numb2 = br.ReadBytes(3);
                                numb[0] = numb2[0];
                                numb[1] = numb2[1];
                                numb[2] = numb2[2];

                                int num2 = BitConverter.ToInt32(numb, 0);
                                num1 = 65536/ (float)num2;
                                //num1 = BitConverter.ToSingle(numb, 0);
                                tw.Write("{0} ", num1);
                            }

                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
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
                            tw.Write(str);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                            allreaddata += num.Length;
                            c += num.Length;
                            /**/
                        }
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                    firstVertex = false;
                }
                else if (def.Contains("Space_Frame0]"))
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("{0} //counts {1}",def, countItem);
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
                        tw.Write(str);
                        if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                        tw.Write("\r\n");
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                }
                else if (def.Contains("MaterialMapping]"))
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("{0} //counts {1}",def, countItem);
                    //MaterialMapping count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        string str = string.Empty;

                        short u = BitConverter.ToInt16(num, 0);
                        short v = BitConverter.ToInt16(num, 3);
                        //byte[] ub2 = BitConverter.GetBytes(u);
                        //byte[] vb2 = BitConverter.GetBytes(v);

                        //System.Buffer.BlockCopy(num, 0, ub, 0, 2);
                        //System.Buffer.BlockCopy(num, 3, vb, 0, 2);

                        //byte t = ub[0];
                        //ub[0] = ub[1];
                        //ub[1] = t;

                        float fu = (float)u / (float)65536.0;
                        float fv = (float)v / (float)65536.0;

                        if (firstUV == true)
                        {
                            UV uv = new UV();
                            uv.u = fu;
                            uv.v = fv;
                            OBJWriter.UVs.Add(uv);
                        }

                        

                        str += String.Format("{0} {1} ", fu, fv);
                        /*
                        for (int j = 0; j < num.Length; j++)
                        {
                            str += String.Format("{0:x2} ",(sbyte)num[j]);
                        }
                        /**/
                        tw.Write(str);
                        if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                        tw.Write("\r\n");

                        allreaddata += num.Length;
                        c += num.Length;
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                    firstUV = false;
                }
                else if (def.Contains("Faces]"))
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("{0} //counts {1}",def, countItem);
                    //Faces count

                    int lastv = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int startPos = 0;
                        int lastPos = 0;
                        int countf = size / 3;
                        int v1=0;
                        int v2=0;
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

                                OBJWriter.Faces.Add(f);
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

                                OBJWriter.Faces.Add(f);
                            }
                        }
                        tw.Write("{0} {1} {2}", v1, v2,v3);
                        if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                        tw.Write("\r\n");
                        /*
                        string str = string.Empty;
                        for (int j = 0; j < num.Length; j++)
                        {
                            str += Convert.ToInt32(num[j]).ToString() + " ";
                        }
                        tw.WriteLine(str);
                        /**/
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                    firstFaces = false;
                }
                else if (def == "[Hooks]")
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("[Hooks] //counts {0}", countItem);

                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        string Val = GetFirstString(lastPos, num, ref lastPos);
                        tw.Write("{0} {1}", Name,Val);
                        if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                        tw.Write("\r\n");
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                }
                else if (def == "[FrameNames]")
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("[FrameNames] //counts {0}", countItem);

                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        tw.Write("{0}", Name);
                        if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                        tw.Write("\r\n");
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                }
                else if (def.Contains("CoCommon"))
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("{0} //counts {1}",def,countItem);

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
                            int val = GetFirstInt(startPos+1 , num, ref lastPos);
                            tw.Write("{0} {1}", Name, val);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }
                        else if (Name == "NParts")
                        {
                            int val = GetFirstInt(startPos+1, num, ref lastPos);
                            tw.Write("{0} {1}", Name, val);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }
                        else if (Name == "ParentBone")
                        {
                            int val = GetFirstInt(startPos + 1, num, ref lastPos);
                            tw.Write("{0} {1}", Name, val);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }
                        else if (Name == "Type")
                        {
                            string val = GetFirstString(startPos, num, ref lastPos);
                            tw.Write("{0} {1}", Name, val);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }
                        else if (Name == "NFrames")
                        {
                            int val = GetFirstInt(startPos+1, num, ref lastPos);
                            tw.Write("{0} {1}", Name, val);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }
                        else if (Name == "Name")
                        {
                            string val = GetFirstString(startPos, num, ref lastPos);
                            tw.Write("{0} {1}", Name, val);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }
                        else
                        {
                            tw.WriteLine("{0} ", Name);
                            str = string.Empty;
                            for (int j = lastPos; j < num.Length; j++)
                            {
                                str += Convert.ToInt32(num[j]).ToString() + " ";
                            }
                            tw.Write("{0}", Name);
                            if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                            tw.Write("\r\n");
                        }

                        c+=num.Length;
                        allreaddata += num.Length;
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                }
                else
                {
                    int c = 0;
                    tw.WriteLine("");
                    tw.WriteLine("{0} //counts {1}", def,countItem);
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;
                        for (int j = 0; j < num.Length; j++)
                        {
                            str += Convert.ToInt32(num[j]).ToString() + " ";
                        }
                        tw.Write(str);
                        if (debugSizeOutput) tw.Write(" //bytes {0}", size);
                        tw.Write("\r\n");

                        allreaddata += num.Length;
                        c+=num.Length;
                    }
                    if (debugSizeOutput) tw.WriteLine("//total size: {0} bytes", c);
                }

                count++;
            }

            if (allreaddata != this.AllDataBytes)
            {
                throw new Exception("Error");
            }

            br.BaseStream.Seek(old, SeekOrigin.Begin);
            tw.Close();

            FileStream fstr = File.Create(filename+".SecondOffsetData.bin");

            BinaryWriter wr = new BinaryWriter(fstr);
            wr.Write(SecondData);
            wr.Close();
            fstr.Dispose();

            OBJWriter.Save(filename + ".OBJ");

        }

        public string ReadString(bool nullt)
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

        
        public string GetFirstString(int offset,byte[] array,ref int lastpos)
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

        public int GetFirstInt(int offset,byte[] array, ref int lasPos)
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

        public int GetFirstIntCount(int offset,int count, byte[] array, ref int lasPos)
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

        public float GetFirstFloat(int offset,int count, byte[] array, ref int lasPos)
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
}


