using AvMatchCheck;
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
            dilog.Description = "请选择识别路径";
            DialogResult res = dilog.ShowDialog();
            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                textBox1.Text = dilog.SelectedPath;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            dilog.Description = "请选择目标路径";
            DialogResult res = dilog.ShowDialog();
            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                textBox5.Text = dilog.SelectedPath;
            }
        }

        List<Av> avs;

        private void button2_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox1.Text) || !Directory.Exists(textBox5.Text))
            {
                MessageBox.Show("请先选择路径");
                return;
            }
            avs = new List<Av>();
            Director(textBox1.Text);
        }//method

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

        string extNames = ".ts.tp.m2ts.tod.m2t.mts.avi.mov.mpg.mpeg.divx.RA.RM.RMVB.WMV.mkv.mp4.asf.m4v.vob.flv";

        void Director(string dir)
        {
            DirectoryInfo fdir = new DirectoryInfo(dir);
            FileSystemInfo[] fsinfos = fdir.GetFileSystemInfos();
            HashSet<string> IdSet;
            foreach (FileSystemInfo fsinfo in fsinfos)
            {
                if (fsinfo is DirectoryInfo)     //判断是否为文件夹
                {
                    Director(fsinfo.FullName);//递归调用
                }
                else
                {
                    IdSet = new HashSet<string>();
                    if (extNames.ToUpper().IndexOf(fsinfo.Extension.ToUpper()) != -1)
                    {//视频文件
                        FileInfo ff =  new System.IO.FileInfo(fsinfo.FullName);

                        //太小的视频文件 判断为损坏跳过
                        /*if (ff.Length < 1024) {
                            textBox3.AppendText(fsinfo.Name + ":"+ff.Length+"\r\n");
                            continue;
                        }*/
                        //视频文件 被整理过
                       /* if (fsinfo.Name.IndexOf("-cd") != -1)
                        {
                            textBox3.AppendText(fsinfo.Name + ":" + ff.Length + "\r\n");
                            continue;
                        }*/

                        textBox2.AppendText(fsinfo.Name + "\r\n");
                        string spName = fsinfo.Name.Replace(fsinfo.Extension,"");
                        MatchCollection matchs = new Regex("[A-Za-z0-9]+[-_]?[A-Za-z0-9]+([-_]?[0-9])*").Matches(spName);
                        foreach (Match match in matchs)
                        {
                            string fh = match.Groups[0].Value.Trim();
                            if (fh.Length < 5) continue;
                            //Match mat = new Regex(@"^[A-Za-z0-9-_;]+$").Match(fh);
                            //if (!mat.Success) continue;
                            int numIndex = getNumberIndex(fh);
                            if (numIndex == -1) continue;

                            Av av = new Av();

                            av.Oname = fsinfo.Name;
                            av.file = true;
                            av.Id = fh;
                            av.Path = fsinfo.FullName;
                            av.ToPath = textBox5.Text;
                            avs.Add(av);
                            textBox2.AppendText("->" + fh + "\r\n");
                            IdSet.Add(fh);
                           
                            if (fh.IndexOf("-1") != -1 || fh.IndexOf("-2") != -1)
                            {
                                int fg = fh.LastIndexOf("-");
                                fh = fh.Substring(0, fg).Trim();
                                if (fh.Length < 6) continue;

                                if (IdSet.Contains(fh)) continue;
                                av = new Av();
                                av.Oname = fsinfo.Name;
                                av.Id = fh;
                                av.Path = fsinfo.FullName;
                                av.ToPath = textBox5.Text;
                                avs.Add(av);
                                textBox2.AppendText("->" + fh + "\r\n");
                                IdSet.Add(fh);
                            }
                            else if (fh.IndexOf("-") == -1 && fh.IndexOf("_") == -1)
                            {
                                fh = fh.Substring(0, numIndex) + "-" + fh.Substring(numIndex, fh.Length - numIndex);
                                if (IdSet.Contains(fh)) continue;
                                av = new Av();
                                av.Oname = fsinfo.Name;
                                av.Id = fh;
                                av.Path = fsinfo.FullName;
                                av.ToPath = textBox5.Text;
                                avs.Add(av);
                                textBox2.AppendText("->" + fh + "\r\n");
                                IdSet.Add(fh);
                            }
                            else if (fh.IndexOf("_") != -1) {
                                fh = fh.Replace("_", "-");
                                if (IdSet.Contains(fh)) continue;
                                av = new Av();
                                av.Oname = fsinfo.Name;
                                av.Id = fh;
                                av.Path = fsinfo.FullName;
                                av.ToPath = textBox5.Text;
                                avs.Add(av);
                                textBox2.AppendText("->" + fh + "\r\n");
                                IdSet.Add(fh);
                            }
                            else if (fh.IndexOf("-") != -1)
                            {
                                fh = fh.Replace("-", "_");
                                if (IdSet.Contains(fh)) continue;
                                av = new Av();
                                av.Oname = fsinfo.Name;
                                av.Id = fh;
                                av.Path = fsinfo.FullName;
                                av.ToPath = textBox5.Text;
                                avs.Add(av);
                                textBox2.AppendText("->" + fh + "\r\n");
                                IdSet.Add(fh);
                            }
                        }//foreach match
                        Match mat = new Regex(@"[Nn]\d{3,5}").Match(spName);
                        if (mat.Success)
                        {
                            string fh = mat.Groups[0].Value;
                            if (!IdSet.Contains(fh))
                            {
                                Av av = new Av();
                                av.Oname = fsinfo.Name;
                                av.Id = fh;
                                av.Path = fsinfo.FullName;
                                av.ToPath = textBox5.Text;
                                avs.Add(av);
                                textBox2.AppendText("->" + fh + "\r\n");
                                IdSet.Add(fh);
                            }
                        }
                        mat = new Regex("[A-Za-z]+-[A-Za-z0-9]+").Match(spName);
                        if (mat.Success)
                        {
                            string fh = mat.Groups[0].Value;
                            if (!IdSet.Contains(fh))
                            {
                                Av av = new Av();
                                av.Oname = fsinfo.Name;
                                av.Id = fh;
                                av.Path = fsinfo.FullName;
                                av.ToPath = textBox5.Text;
                                avs.Add(av);
                                textBox2.AppendText("->" + fh + "\r\n");
                                IdSet.Add(fh);
                            }
                        }
                        mat = new Regex("[A-Za-z]+[0-9]{3,8}").Match(spName);
                        if (mat.Success)
                        {
                            string fh = mat.Groups[0].Value;
                            int numIndex = getNumberIndex(fh);
                            fh = fh.Substring(0, numIndex) + "-" + fh.Substring(numIndex, fh.Length - numIndex);
                            if (!IdSet.Contains(fh))
                            {
                                Av av = new Av();
                                av.Oname = fsinfo.Name;
                                av.Id = fh;
                                av.Path = fsinfo.FullName;
                                av.ToPath = textBox5.Text;
                                avs.Add(av);
                                textBox2.AppendText("->" + fh + "\r\n");
                                IdSet.Add(fh);
                            }
                        }
                        mat = new Regex("[0-9]+-[0-9]+[-]?[0-9]+").Match(spName);
                        if (mat.Success)
                        {
                            string fh = mat.Groups[0].Value;
                            if (!IdSet.Contains(fh))
                            {
                                Av av = new Av();
                                av.Oname = fsinfo.Name;
                                av.Id = fh;
                                av.Path = fsinfo.FullName;
                                av.ToPath = textBox5.Text;
                                avs.Add(av);
                                textBox2.AppendText("->" + fh + "\r\n");
                                IdSet.Add(fh);
                            }
                        }
                        mat = new Regex("[A-Za-z]+[-_]?[0-9]{3,8}").Match(spName);
                        if (mat.Success)
                        {
                            string fh = mat.Groups[0].Value;
                            fh = fh.Replace("_", "-");
                            if (!IdSet.Contains(fh))
                            {
                                Av av = new Av();
                                av.Oname = fsinfo.Name;
                                av.Id = fh;
                                av.Path = fsinfo.FullName;
                                av.ToPath = textBox5.Text;
                                avs.Add(av);
                                textBox2.AppendText("->" + fh + "\r\n");
                                IdSet.Add(fh);
                            }
                        }

                    }//if 视频文件
                }//foreach
            }//if

        }//method


        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox3.AppendText("\r\n---执行日志---\r\n");

            button3.Enabled = false;
            Collector cl = new Collector(SynchronizationContext.Current,avs,comboBox.Text);
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

        private void button5_Click(object sender, EventArgs e)
        {
            this.textBox4.Text = "";
            this.textBox2.Text = "";
            this.textBox3.Text = "";
            button3.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IniFiles iniFile = new IniFiles("address.ini");
            string path = iniFile.ReadString("javbus", "path", "https://www.javbus.com");
            string[] paths = path.Split(',');
            foreach (string p in paths)
            {
                comboBox.Items.Add(p);
            }
            comboBox.Text = comboBox.Items[0].ToString();

            string proxy = iniFile.ReadString("javbus", "proxy", "");
            textBox6.Text = proxy;
            Download.SetProxy(textBox6.Text);
        }
    }
}
