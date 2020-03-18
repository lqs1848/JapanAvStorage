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
            string[] folders = Directory.GetDirectories(textBox1.Text);
            foreach (string folder in folders)
            {
                int fg = folder.LastIndexOf("\\") + 1;
                string name = folder.Substring(fg, folder.Length - fg).Trim();
                //Match match = new Regex(@"([A-Za-z0-9]+[-_]*?[A-Za-z0-9]+)\s*").Match(name);
                //Match match = new Regex(@"([A-Za-z0-9]+[-_]*[A-Za-z0-9]+[-_]*[A-Za-z0-9]*)\s*").Match(name);
                Match match = match = new Regex(@"(?:[0-9]{4}-[0-9]{2}-[0-9]{2})?\s?([A-Za-z0-9]+[-_]*[A-Za-z0-9]+[-_]*[A-Za-z0-9]*)\s*").Match(name);
                if (match.Success) {
                    string fhid = match.Groups[1].Value;
                    textBox2.AppendText(fhid + "\r\n");
                    Av av = new Av();
                    av.Id = fhid.Trim();
                    av.Path = folder;
                    avs.Add(av);
                }
                else
                    textBox3.AppendText(name + "\r\n");
            }//for

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
    }
}
