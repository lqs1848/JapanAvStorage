using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nameTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string[] list = textBox1.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (var name in list)
            {
                testName(name);
            }
            
            
        }//method

        private void testName(string name) {

            HashSet<string> IdSet;
            IdSet = new HashSet<string>();
            textBox2.AppendText(name +  "\r\n");
            MatchCollection matchs = new Regex("[A-Za-z0-9]+[-_]?[A-Za-z0-9]+([-_]?[0-9])*").Matches(name);
            foreach (Match match in matchs)
            {
                string fh = match.Groups[0].Value.Trim();
                if (fh.Length < 5) continue;

                int numIndex = getNumberIndex(fh);
                if (numIndex == -1) continue;


                textBox2.AppendText("->" + fh + "\r\n");
                IdSet.Add(fh);

                if (fh.IndexOf("-1") != -1 || fh.IndexOf("-2") != -1)
                {
                    int fg = fh.LastIndexOf("-");
                    fh = fh.Substring(0, fg).Trim();
                    if (fh.Length < 6) continue;
                    if (IdSet.Contains(fh)) continue;
                    textBox2.AppendText("->" + fh + "\r\n");
                    IdSet.Add(fh);
                }
                else if (fh.IndexOf("-") == -1 && fh.IndexOf("_") == -1)
                {
                    fh = fh.Substring(0, numIndex) + "-" + fh.Substring(numIndex, fh.Length - numIndex);
                    if (IdSet.Contains(fh)) continue;
                    textBox2.AppendText("->" + fh + "\r\n");
                    IdSet.Add(fh);
                }
                else if (fh.IndexOf("_") != -1)
                {
                    fh = fh.Replace("_", "-");
                    if (IdSet.Contains(fh)) continue;
                    textBox2.AppendText("->" + fh + "\r\n");
                    IdSet.Add(fh);
                }
                else if (fh.IndexOf("-") != -1)
                {
                    fh = fh.Replace("-", "_");
                    if (IdSet.Contains(fh)) continue;
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
                    textBox2.AppendText("->" + fh + "\r\n");
                    IdSet.Add(fh);
                }
            }

            int heyzo = name.ToLower().IndexOf("heyzo");
            if (heyzo != -1)
            {
                string heyzoStr = name.Substring(heyzo);
                mat = new Regex("[0-9]{1,4}").Match(heyzoStr);
                if (mat.Success) {
                    for (int i = 0; i < mat.Groups.Count; i++)
                    {
                        string num = mat.Groups[i].Value;
                        num = "HEYZO-" + prefixRepairFlag(num,4,"0");
                        if (!IdSet.Contains(num))
                        {
                            textBox2.AppendText("->" + num + "\r\n");
                            IdSet.Add(num);
                        }
                    }//for
                }
            }//if

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

        //前缀补充
        string prefixRepairFlag(string str, int size, string flag) {
            int x = size - str.Length;
            for (int i = 0; i < x; i++)
            {
                str = flag + str;
            }
            return str;
        }
    }//class
}//namespance
