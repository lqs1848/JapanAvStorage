﻿using System;
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

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox3.AppendText("\r\n---下载日志---\r\n");
            button3.Enabled = false;
            Collector cl = new Collector(SynchronizationContext.Current, textBox1.Text, comboBox.Text);
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
            foreach (string p in paths)
            {
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
