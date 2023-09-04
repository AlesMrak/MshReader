using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using Scanning;
using System.Globalization;

namespace MshReader
{
    
    public class AsciiMshReader
    {
        public ArrayList Materials =new ArrayList();
        public ArrayList Faces = new ArrayList();
        public ArrayList Vertexs = new ArrayList();
        public ArrayList UVs = new ArrayList();

        public void Clear()
        {
            Materials.Clear();
            Faces.Clear();
            Vertexs.Clear();
            UVs.Clear();

        }

        public void Load(string filename)
        {
            CultureInfo ci = new CultureInfo("en-us");

            StreamReader sr = File.OpenText(filename);

            while (sr.EndOfStream==false)
            {
                string line = sr.ReadLine();

                if (line.Contains("Materials"))
                {
                    line = sr.ReadLine();
                    line=line.Trim();
                    while(line!=string.Empty)
                    {
                        Material m = new Material();
                        m.Name = line;
                        Materials.Add(m);
                        line = sr.ReadLine();
                        line = line.Trim();
                    }
                    
                }
                else if (line.Contains("Vertices_Frame0"))
                {
                    Scanner scanner = new Scanner();
                    object[] targets = new object[6];
                    targets[0] = new Single();
                    targets[1] = new Single();
                    targets[2] = new Single();
                    targets[3] = new Single();
                    targets[4] = new Single();
                    targets[5] = new Single();

                    line = sr.ReadLine();
                    line = line.Trim();
                    while (line != string.Empty)
                    {
                        Vertex m = new Vertex();

                        string[] tokens = line.Split(' ');
                        ArrayList l = new ArrayList();
                        foreach (string s in tokens)
                        {
                            float f=0;
                            Single.TryParse(s, (NumberStyles.Float | NumberStyles.AllowThousands), ci, out f);

                            l.Add(f);
                        }

                        m.posx = (float)l[0];
                        m.posy = (float)l[1];
                        m.posz = (float)l[2];

                        m.u = (float)l[3];
                        m.v = (float)l[4];
                        m.z = (float)l[5];

                        Vertexs.Add(m);
                        line = sr.ReadLine();
                        line = line.Trim();
                    }
                }
                else if (line.Contains("MaterialMapping"))
                {
                    Scanner scanner = new Scanner();
                    object[] targets = new object[6];
                    targets[0] = new Single();
                    targets[1] = new Single();

                    line = sr.ReadLine();
                    line = line.Trim();
                    while (line != string.Empty)
                    {
                        UV m = new UV();

                        string[] tokens = line.Split(' ');
                        ArrayList l = new ArrayList();
                        foreach (string s in tokens)
                        {
                            float f=0;
                            Single.TryParse(s, (NumberStyles.Float | NumberStyles.AllowThousands), ci, out f);

                            l.Add(f);
                        }

                        m.u = (float)l[0];
                        m.v = (float)l[1];

                        UVs.Add(m);
                        line = sr.ReadLine();
                        line = line.Trim();
                    }
                }
                else if (line.Contains("Faces"))
                {
                    Scanner scanner = new Scanner();
                    object[] targets = new object[3];
                    targets[0] = new Int32();
                    targets[1] = new Int32();
                    targets[2] = new Int32();

                    line = sr.ReadLine();
                    line = line.Trim();
                    while (line != string.Empty)
                    {
                        Face m = new Face();
                        scanner.Scan(line,"{0} {1} {2}", targets);

                        m.v1 = (int)targets[0];
                        m.v2 = (int)targets[1];
                        m.v3 = (int)targets[2];

                        Faces.Add(m);
                        line = sr.ReadLine();
                        line = line.Trim();
                    }
                }

            }
            sr.Close();
        }

        public void Save(string filename)
        {
            CultureInfo ci = new CultureInfo("en-us");
            StreamWriter sw = File.CreateText(filename);

            sw.WriteLine("mtllib {0}", GetName(filename) + ".mtl");

            int gindex = 0;
            sw.WriteLine("g{0}", gindex);

            foreach (Vertex v in Vertexs)
            {
                sw.WriteLine("v {0} {1} {2}", v.posx.ToString(ci), v.posy.ToString(ci), v.posz.ToString(ci));
            }

            foreach (UV v in UVs)
            {
                sw.WriteLine("vt {0} {1}", v.u, v.v);
            }

            foreach (Vertex v in Vertexs)
            {
                sw.WriteLine("vn {0} {1} {2}", v.nx,v.ny,v.nz);
            }

            foreach (Face f in Faces)
            {
                //sw.WriteLine("f {0}/{1}/{2}  {3}/{4}/{5} {6}/{7}/{8}", f.v1 + 1, f.v1 + 1, f.v1 + 1, f.v2 + 1, f.v2 + 1, f.v2 + 1, f.v3 + 1, f.v3 + 1, f.v3 + 1);
                sw.WriteLine("f {0}/{1}/{6}  {2}/{3}/{7} {4}/{5}/{8}", f.v1 + 1, f.v1 + 1, f.v2 + 1, f.v2 + 1, f.v3 + 1, f.v3 + 1, f.v1 + 1, f.v2 + 1, f.v3 + 1);
            }

            sw.Close();

            sw = File.CreateText(GetName(filename) + ".mtl");
            foreach (Material m in Materials)
            {
                sw.WriteLine("\r\nnewmtl {0}", GetName(m.Name));
                sw.WriteLine("Ka 0.5 0.5 0.5");
                sw.WriteLine("Kd 0.5 0.5 0.5");
                sw.WriteLine("Ks 0.5 0.5 0.5");
                sw.WriteLine("illum 2");
                sw.WriteLine("Ns 8");
                sw.WriteLine("map_Kd {0}", GetName(m.Name));
            }

            sw.Close();

        }

        public string GetName(string path)
        {
            int count = 0;
            for (int i = path.Length - 1; i >= 0; i--)
            {

                if (path[i] == '\\' || path[i] == '/')
                {
                    break;
                }
                count++;
            }
            if (count > 0 && count < path.Length)
            {
                return path.Remove(0, path.Length - count);
            }
            return string.Empty;
        }

    }
}

