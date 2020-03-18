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
                Match match = new Regex("[0-9]{4}-[0-9]{2}-[0-9]{2}").Match(name);
                if (match.Success) continue;
                match = new Regex(@"(?:[0-9]{4}-[0-9]{2}-[0-9]{2})?\s?([A-Za-z0-9]+[-_]*[A-Za-z0-9]+[-_]*[A-Za-z0-9]*)\s*").Match(name);
                if (match.Success) {
                    string matRes = match.Groups[1].Value;
                    textBox2.AppendText(matRes + "\r\n");
                    Av av = new Av();
                    av.Id = matRes.Trim();
                    av.Path = folder;
                    avs.Add(av);

                    //修改视频名称 改为符合 emby的规则
                    string[] extNames = new string[] { ".avi", ".mov", ".mpg", ".RA", ".RM", ".RMVB", ".WMV", ".mkv", ".mp4", ".asf", ".m4v" };
                    DirectoryInfo fdir = new DirectoryInfo(folder);
                    FileInfo[] file = fdir.GetFiles();
                    if (file.Length != 0){ //当前目录文件或文件夹不为空                   
                        for (int i = 0; i < extNames.Length; i++){
                            int x = 0;
                            foreach (FileInfo f in file)
                                if (extNames[i].ToLower().Equals(f.Extension.ToLower()))
                                    x++;
                            int y = 1;
                            foreach (FileInfo f in file) //显示当前目录所有文件   
                                if (extNames[i].ToLower().Equals(f.Extension.ToLower())){
                                    string newName = folder + "\\" + av.Id + (x > 1 ? "-cd" + y : "") + f.Extension;
                                    try
                                    {
                                        //记录文件名称
                                        ChangeNameLogTxt(f.Name, newName);
                                        File.Move(f.FullName, newName);
                                        //File.Create(folder + "\\"+ (x > 1 ? "-cd" + y : "") + f.Name + ".old.name").Close();
                                    }
                                    catch (Exception ex){
                                        textBox3.AppendText(av.Id + "-修改视频文件名称错误 \r\n");
                                    }
                                    y++;
                            }//for files
                        }//for ext
                    }//file

                }//match sucess
                else
                    textBox3.AppendText(name + "-无法解析番号\r\n");
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

        /// <summary>
        /// 旧文件名记录操作
        /// </summary>
        public static void ChangeNameLogTxt(string content, string fileName)
        {
            string fileStr = fileName + ".oldame.txt";
            if (!File.Exists(fileStr))
            {
                FileStream fs1 = new FileStream(fileStr, FileMode.Create, FileAccess.Write);//创建写入文件
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine(content);
                sw.Close();
                fs1.Close();
            }
            else
            {
                StreamWriter sw1;
                sw1 = File.AppendText(fileStr);
                sw1.WriteLine(content);
                sw1.Close();
            }
        }
    }
}
