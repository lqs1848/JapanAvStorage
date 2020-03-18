using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvCoverDownloader
{
    class ExeLog
    {
        private static StreamWriter sw1;

        /// <summary>
        /// Txt操作
        /// </summary>
        /// <param name="url">更新内容</param>
        /// <param name="name">更新txt名</param>
        /// <param name="fails">更新文件夹</param>
        public static void UrlTxt(string content, string name, string fails)
        {
            //string y = AppDomain.CurrentDomain.BaseDirectory;//获取当前程序的位置
            //  string fileStr1 = y.Replace("\\bin\\Debug\\", fails);
            // fileStr1 += fails;
            // string fileStr1 = "F:\\自有商品获取发布\\"+ fails;
            string fileStr1 = fails;//获取txt所在文件
            System.IO.Directory.CreateDirectory(fileStr1);
            DirectoryInfo dir = new DirectoryInfo(fileStr1);
            dir.Create();//自行判断一下是否存在。
            string fileStr = fileStr1 + "\\" + name + ".txt";
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
                sw1 = File.AppendText(fileStr);
                sw1.WriteLine(content);
                sw1.Close();
            }
        }

        public static void WriteLog(string msg)
        {
            string now = DateTime.Now.ToString("yyyyMMdd");
            //每一天分开保存
            string logpath = AppDomain.CurrentDomain.BaseDirectory;
            string dirPath = logpath + "log" + "\\" + now + "\\";
            //string txtpath = "F:\\FilePath\\PackageSvr\\";
            WriteLog(msg, "Worklog.txt", dirPath);
            //WriteLog(dirPath, "Worklog.txt", txtpath);
        }

        public static void WriteLog(string logName, string msg)
        {
            string now = DateTime.Now.ToString("yyyyMMdd");

            string logpath = AppDomain.CurrentDomain.BaseDirectory;
            string dirPath = logpath + "log" + "\\" + now + "\\";

            WriteLog(msg, logName, dirPath);

        }


        public static void WriteLog(string msg, string logName, string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            string filePath = dirPath + logName;

            using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    //声明数据流文件写入方法  
                    sw.Write(msg);
                }
            }

        }
    }
}
