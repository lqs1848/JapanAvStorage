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

            avs = new List<Av>();
            HashSet<string> IdSet;
            IdSet = new HashSet<string>();
            string[] folders = Directory.GetDirectories(textBox1.Text);
            foreach (string folder in folders)
            {
               int fgg = folder.LastIndexOf("\\") + 1;
                string name = folder.Substring(fgg, folder.Length - fgg).Trim();
                //Match match = new Regex(@"([A-Za-z0-9]+[-_]*?[A-Za-z0-9]+)\s*").Match(name);
                //Match match = new Regex(@"([A-Za-z0-9]+[-_]*[A-Za-z0-9]+[-_]*[A-Za-z0-9]*)\s*").Match(name);
                /*Match match = new Regex("[0-9]{4}-[0-9]{2}-[0-9]{2}").Match(name);
                //if (match.Success) continue;
                match = new Regex(@"(?:[0-9]{4}-[0-9]{2}-[0-9]{2})?\s?([A-Za-z0-9]+[-_]*[A-Za-z0-9]+[-_]*[A-Za-z0-9]*)\s*").Match(name);
                if (match.Success) {
                    string matRes = match.Groups[1].Value;
                    textBox2.AppendText(matRes + "\r\n");
                    Av av = new Av();
                    av.Id = matRes.Trim();
                    av.Path = folder;
                    avs.Add(av);
                }//match sucess
                else
                    textBox3.AppendText(name + "-无法解析番号\r\n");*/

                textBox2.AppendText(name + "\r\n");
                MatchCollection matchs = new Regex("[A-Za-z0-9]+[-_]?[A-Za-z0-9]+([-_]?[0-9])*").Matches(name);
                foreach (Match match in matchs)
                {
                    string fh = match.Groups[0].Value.Trim();
                    if (fh.Length < 5) continue;
                    //Match mat = new Regex(@"^[A-Za-z0-9-_;]+$").Match(fh);
                    //if (!mat.Success) continue;
                    int numIndex = getNumberIndex(fh);
                    if (numIndex == -1) continue;


                    Av av = new Av();
                    av.Id = fh;
                    av.Path = folder;
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
                        av.Id = fh;
                        av.Path = folder;
                        avs.Add(av);
                        textBox2.AppendText("->" + fh + "\r\n");
                        IdSet.Add(fh);

                    }
                    else if (fh.IndexOf("-") == -1 && fh.IndexOf("_") == -1)
                    {
                        fh = fh.Substring(0, numIndex) + "-" + fh.Substring(numIndex, fh.Length - numIndex);
                        if (IdSet.Contains(fh)) continue;
                        av = new Av();
                        av.Id = fh;
                        av.Path = folder;
                        avs.Add(av);
                        textBox2.AppendText("->" + fh + "\r\n");
                        IdSet.Add(fh);
                    }
                    else if (fh.IndexOf("_") != -1)
                    {
                        fh = fh.Replace("_", "-");
                        if (IdSet.Contains(fh)) continue;
                        av = new Av();
                        av.Id = fh;
                        av.Path = folder;
                        avs.Add(av);
                        textBox2.AppendText("->" + fh + "\r\n");
                        IdSet.Add(fh);
                    }
                    else if (fh.IndexOf("-") != -1)
                    {
                        fh = fh.Replace("-", "_");
                        if (IdSet.Contains(fh)) continue;
                        av = new Av();
                        av.Id = fh;
                        av.Path = folder;
                        avs.Add(av);
                        textBox2.AppendText("->" + fh + "\r\n");
                        IdSet.Add(fh);
                    }
                }//foreach match
                Match mat = new Regex(@"[Nn]\d{3,5}").Match(name);
                if (mat.Success)
                {
                    string fh = mat.Groups[0].Value;
                    if (!IdSet.Contains(fh))
                    {
                        Av av = new Av();
                        av.Id = fh;
                        av.Path = folder;
                        avs.Add(av);
                        textBox2.AppendText("->" + fh + "\r\n");
                        IdSet.Add(fh);
                    }
                }
                mat = new Regex("[A-Za-z]+-[A-Za-z0-9]+").Match(name);
                if (mat.Success)
                {
                    string fh = mat.Groups[0].Value;
                    if (!IdSet.Contains(fh))
                    {
                        Av av = new Av();
                        av.Id = fh;
                        av.Path = folder;
                        avs.Add(av);
                        textBox2.AppendText("->" + fh + "\r\n");
                        IdSet.Add(fh);
                    }
                }
                mat = new Regex("[A-Za-z]+[0-9]{3,8}").Match(name);
                if (mat.Success)
                {
                    string fh = mat.Groups[0].Value;
                    int numIndex = getNumberIndex(fh);
                    fh = fh.Substring(0, numIndex) + "-" + fh.Substring(numIndex, fh.Length - numIndex);
                    if (!IdSet.Contains(fh))
                    {
                        Av av = new Av();
                        av.Id = fh;
                        av.Path = folder;
                        avs.Add(av);
                        textBox2.AppendText("->" + fh + "\r\n");
                        IdSet.Add(fh);
                    }
                }
                mat = new Regex("[0-9]+-[0-9]+[-]?[0-9]+").Match(name);
                if (mat.Success)
                {
                    string fh = mat.Groups[0].Value;
                    if (!IdSet.Contains(fh))
                    {
                        Av av = new Av();
                        av.Id = fh;
                        av.Path = folder;
                        avs.Add(av);
                        textBox2.AppendText("->" + fh + "\r\n");
                        IdSet.Add(fh);
                    }
                }
                mat = new Regex("[A-Za-z]+[-_]?[0-9]{3,8}").Match(name);
                if (mat.Success)
                {
                    string fh = mat.Groups[0].Value;
                    fh = fh.Replace("_", "-");
                    if (!IdSet.Contains(fh))
                    {
                        Av av = new Av();
                        av.Id = fh;
                        av.Path = folder;
                        avs.Add(av);
                        textBox2.AppendText("->" + fh + "\r\n");
                        IdSet.Add(fh);
                    }
                }

            }//for

        }//method

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox3.AppendText("\r\n---下载日志---\r\n");

            button3.Enabled = false;
            Collector cl = new Collector(SynchronizationContext.Current,avs, comboBox.Text);
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

        private void Form1_Load(object sender, EventArgs e)
        {
            IniFiles iniFile = new IniFiles("address.ini");
            string path = iniFile.ReadString("javbus", "path", "https://www.javbus.com");
            string[] paths = path.Split(',');
            foreach (string p in paths) {
                comboBox.Items.Add(p);
            }
            comboBox.Text = comboBox.Items[0].ToString();

            string proxy = iniFile.ReadString("javbus", "proxy", "");
            textBox5.Text = proxy;
            Download.SetProxy(textBox5.Text);
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            Download.SetProxy(textBox5.Text);
        }
    }
}
