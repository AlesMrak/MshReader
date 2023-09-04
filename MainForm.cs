using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

using proj;

namespace MshReader
{
    public partial class MainForm : Form
    {
        
        public MSHReading mshReader = new MSHReading();

        public Mesh Mesh = new Mesh();

        public MainForm()
        {
            InitializeComponent();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;

                mshReader.OpenFile(filename);

                this.Text = filename;

                RefreshData();
            }
        }

        public void RefreshData()
        {
            if (mshReader == null) return;

            StringsView.Items.Clear();
            ThirdView.Items.Clear();
            XVIew.Items.Clear();

            int index = 0;
            foreach (string str in mshReader.Strings)
            {
                ListViewItem item = new ListViewItem(index.ToString());
                item.SubItems.Add(str);
                item.SubItems.Add((string)mshReader.StringFirstValues[index]);
                item.SubItems.Add(String.Format("{0:x}",((MshItem)mshReader.ItemsList[index]).StartOffset));
                item.SubItems.Add(String.Format("{0:x}", ((MshItem)mshReader.ItemsList[index]).EndOffset));
                item.SubItems.Add(String.Format("{0}", ((MshItem)mshReader.ItemsList[index]).Lenght));
                item.Tag = (MshItem)mshReader.ItemsList[index];
                StringsView.Items.Add(item);
                index++;
            }

            AllZeroBox.Text = mshReader.AllZeroCount.ToString();
            RealZeroBox.Text = mshReader.RealZeroCount.ToString();
            CountAllXEdit.Text = mshReader.CountBytesX.ToString();
            FirstZeroEdit.Text = mshReader.FirstZeroCount.ToString();


            index = 0;
            foreach (string str in mshReader.XCount4)
            {
                ListViewItem item = new ListViewItem(index.ToString());
                item.SubItems.Add((string)mshReader.XCount4[index]);
                ThirdView.Items.Add(item);
                index++;
            }

            index = 0;
            //foreach (string str in mshReader.XView)
            for(int i=0;i<mshReader.XView.Count;i++)
            {
                ListViewItem item = new ListViewItem(index.ToString());

                item.SubItems.Add(mshReader.XViewSort[index].ToString());
                //item.SubItems.Add((string)mshReader.XView[index]);
                //byte ind = (byte)mshReader.XViewSort[index];
                byte ind = (byte)mshReader.XView[index];
                item.SubItems.Add(ind.ToString());
                item.SubItems.Add((string)mshReader.Strings[ind]);

                int zcount = -1;
                int zoffset = -1;
                XValue v = null;
                if (ind < mshReader.XHeaderData.Count)
                {
                    v = (XValue)mshReader.XHeaderData[ind];
                    zcount = v.CountZero;
                    zoffset = v.Offset;
                }
                item.SubItems.Add(zcount.ToString());
                item.SubItems.Add(zoffset.ToString());

                XVIew.Items.Add(item);
                index++;
            }

            StringCountBox.Text = mshReader.CountOfStrings.ToString();
            XCountBox.Text = String.Format("{0}", mshReader.CountX);
            FirstOffset.Text = String.Format("{0:x4}", mshReader.FirstOffset);
            SecondOffset.Text = String.Format("{0:x4}", mshReader.SecondOffset);
            ThirdOffset.Text = String.Format("{0:x4}", mshReader.ThirdOffset);
            DataOffset.Text = String.Format("{0:x4}", mshReader.OffsetToData);

            int s21 = mshReader.SecondOffset - mshReader.FirstOffset;
            int s32 = mshReader.ThirdOffset - mshReader.SecondOffset;
            int sd3 = mshReader.OffsetToData - mshReader.ThirdOffset;

            box21.Text = String.Format("{0}", s21);
            box32.Text = String.Format("{0}", s32);
            boxd3.Text = String.Format("{0}", sd3);

            boxdiv1.Text = String.Format("{0}", (float)s21 / 4.0);
            boxdiv2.Text = String.Format("{0}", (float)s32 / 2.0);
            boxdiv3.Text = String.Format("{0}", (float)sd3 / 4.0);

            FirstDivStringBox.Text = String.Format("{0}", (float)s21 / (float)mshReader.CountOfStrings);

            SecondDivStringBox.Text = String.Format("{0}", (float)s32 / (float)mshReader.CountOfStrings);

            AllCountEdit.Text = String.Format("{0}", mshReader.XViewAllCount);
            DataBytesBox.Text = String.Format("{0}", mshReader.AllDataBytes);
        }

        private void extractASCIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = saveFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                string fileName = saveFileDialog1.FileName;

                mshReader.SaveAscii(fileName);

            }
        }

        private void debugCountToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            mshReader.debugSizeOutput = debugCountToolStripMenuItem.Checked;
        }

        private void loadASCIIMshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;

                AsciiMshReader reader = new AsciiMshReader();

                reader.Load(filename);
                reader.Save(filename + ".obj");

                
            }
        }

        private void XVIew_DoubleClick(object sender, EventArgs e)
        {
            if (XVIew.SelectedItems.Count > 0)
            {
                ListViewItem i = XVIew.SelectedItems[0];

                EditBoxForm f = new EditBoxForm();
                f.textBox1.Text = i.SubItems[3].Text;
                f.ShowDialog();
            }
        }

        private void loadMshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Mesh.LoadMsh(openFileDialog1.FileName);
                

            }
            
        }

        private void extractSectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (StringsView.SelectedItems.Count > 0)
            {
                MshItem item = (MshItem)StringsView.SelectedItems[0].Tag;

                mshReader.br.BaseStream.Seek(item.StartOffset,SeekOrigin.Begin);
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    byte[] section = mshReader.br.ReadBytes((int)item.Lenght);
                    FileStream file = File.Create(saveFileDialog1.FileName);
                    file.Write(section, 0, section.Length);
                    file.Close();
                }

            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void load3dsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ModelLoader meshl = new ModelLoader();
                
                FileStream file = File.Open(openFileDialog1.FileName,FileMode.Open);
                BinaryReader rd = new BinaryReader(file);
                meshl.Load(rd);

                List<proj.Mesh> meshes = new List<proj.Mesh>();
                meshl.LoadAllMeshes(meshes, 1);


            }
        }

    }
}