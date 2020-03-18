using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AvCoverDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dilog.Description = "请选择路径";
            DialogResult res = dilog.ShowDialog();
            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                textBox1.Text = dilog.SelectedPath;
            }
        }

        List<Av> avs;

        private void button2_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox1.Text))
            {
                MessageBox.Show("请先选择路径");
                return;
            }

            string extNames = ".ts.tp.m2ts.tod.m2t.mts.avi.mov.mpg.mpeg.divx.RA.RM.RMVB.WMV.mkv.mp4.asf.m4v.vob.flv";
            DirectoryInfo fdir = new DirectoryInfo(textBox1.Text);
            FileSystemInfo[] fsinfos = fdir.GetFileSystemInfos();
            foreach (FileSystemInfo fsinfo in fsinfos)
            {
                if (fsinfo is DirectoryInfo)     //判断是否为文件夹
                {
                    continue;
                }
                if (extNames.ToUpper().IndexOf(fsinfo.Extension.ToUpper()) != -1) {
                    Match match = new Regex(@"\d{6,}", RegexOptions.IgnoreCase).Match(fsinfo.Name);
                    if (!match.Success) continue;
                    string dirname = "f2cppv-" + fsinfo.Name.Replace(fsinfo.Extension,"");
                    if (!Directory.Exists(textBox1.Text + "\\" + dirname))
                        Directory.CreateDirectory(textBox1.Text + "\\"+ dirname);

                    File.Move(fsinfo.FullName, textBox1.Text + "\\" + dirname+"\\"+fsinfo.Name);
                }
            }

                avs = new List<Av>();
            HashSet<string> IdSet;
            IdSet = new HashSet<string>();
            string[] folders = Directory.GetDirectories(textBox1.Text);
            foreach (string folder in folders)
            {
                int fgg = folder.LastIndexOf("\\") + 1;
                string name = folder.Substring(fgg, folder.Length - fgg).Trim();
                textBox2.AppendText(name + "\r\n");
                Match match = new Regex(@"\d{6,}", RegexOptions.IgnoreCase).Match(name);
                if (!match.Success) continue;
                string matres = match.Value;
                Av av = new Av();
                av.Id = matres;
                av.Path = folder;
                avs.Add(av);
                textBox2.AppendText("识别:" + matres + "\r\n");
            }
        }//method

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox3.AppendText("\r\n---下载日志---\r\n");

            button3.Enabled = false;
            Collector cl = new Collector(SynchronizationContext.Current,avs);
            cl.CollectorLog += (o, text) =>
            {
                this.textBox4.AppendText(text + "\r\n");
            };
            cl.ErrorLog += (o, text) =>
            {
                this.textBox3.AppendText(text + "\r\n");
            };
            cl.Start();
        }

        int getNumberIndex(string fh)
        {
            char[] chars = fh.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (Char.IsNumber(chars[i]))
                    return i;
            }
            return -1;
        }
    }
}
