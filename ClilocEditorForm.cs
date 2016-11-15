using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClilocEditor
{
    public partial class ClilocEditorForm : Form
    {
        private Thread Read;
        private Stream t;
        private Thread Write;
        private BinaryReader r;
        private BinaryWriter w;

        public ClilocEditorForm()
        {
            InitializeComponent();
        }

        #region Form Button Selections

        private void LoadFileButton_Click(object sender, EventArgs e)
        {
            this.Read = new Thread(new ThreadStart(this.LoadFile));
            this.Read.Start();
            while (!this.Read.IsAlive){}
        }

        private void SaveFileButton_Click(object sender, EventArgs e)
        {
            this.Write = new Thread(new ThreadStart(this.SaveFile));
            this.Write.Start();
            while (!this.Write.IsAlive){}
        }

        private void FindFileButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                SelectedPath = this.ProjectPath.Text
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.ProjectPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        #endregion

        #region Load And Save Routines

        public void LoadFile()
        {
            unsafe
            {
                FileInfo fileInfo = new FileInfo(string.Concat(this.ProjectPath.Text, "\\cliloc.enu"));
                try
                {
                    this.t = new FileStream(fileInfo.FullName, FileMode.Open);
                }
                catch
                {
                    this.StatusLabel1.Text = "ERROR: Bad Directory!";
                    return;
                }
                this.StatusLabel1.Text = "Loading...";
                this.r = new BinaryReader(this.t, Encoding.UTF8);
                this.r.BaseStream.Seek((long)6, SeekOrigin.Begin);
                int num = 0;

                List<ListViewItem> items = new List<ListViewItem>();

                while (this.r.BaseStream.Length != this.r.BaseStream.Position)
                {
                    uint num1 = this.r.ReadUInt32();
                    this.r.ReadByte();
                    ListViewItem item = new ListViewItem(Encoding.UTF8.GetString(this.r.ReadBytes((int)this.r.ReadUInt16())));
                    item.SubItems.Add(num1.ToString());

                    items.Add(item);
                    num++;
                }
                this.t.Close();
                this.r.Close();

                this.listView1.Items.AddRange(items.ToArray());

                this.StatusLabel1.Text = string.Concat("Load completed, ", num, " records");
            }
        }

        public void SaveFile()
        {
            FileInfo fileInfo = new FileInfo(string.Concat(this.ProjectPath.Text, "\\cliloc.enu"));
            try
            {
                this.t = new FileStream(fileInfo.FullName, FileMode.Create);
            }
            catch
            {
                this.StatusLabel1.Text = "ERROR: Bad Directory!";
                return;
            }
            this.StatusLabel1.Text = "Saving...";
            this.w = new BinaryWriter(this.t);
            byte[] numArray = new byte[] { 2, 0, 0, 0, 1, 0 };
            this.w.Write(numArray);
            for (int i = 0; i < this.listView1.Items.Count; i++)
            {
                this.w.Write(Convert.ToUInt32(this.listView1.Items[i].SubItems[1].Text));
                this.w.Write((byte)0);
                byte[] bytes = Encoding.UTF8.GetBytes(this.listView1.Items[i].Text);
                int length = bytes.GetLength(0);
                this.w.Write(Convert.ToUInt16(length));
                this.w.Write(bytes);
            }
            this.t.Close();
            this.w.Close();
            this.StatusLabel1.Text = "Save completed";
        }

        #endregion

        private void ClilocEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.t != null)
            {
                this.t.Close();
            }
            if (this.r != null)
            {
                this.r.Close();
            }
            if (this.Read != null)
            {
                this.Read.Abort();
            }
        }

        private void ClilocEditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
