using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AvCoverDownloader
{
    class Collector
    {
        String basePath = "https://www.dmmsee.cloud";
        public EventHandler<String> CollectorLog;
        public EventHandler<String> ErrorLog;
        SynchronizationContext _syncContext;
        //HttpClient client;

        //线程数
        const int cycleNum = 10;

        private Queue<Av> _avs;
        
        public Collector(SynchronizationContext formContext, List<Av> avs)
        {
            _avs = new Queue<Av>(avs);
            _syncContext = formContext;
        }

        public void Start()
        {
            //client = new HttpClient();
            new Thread(Collect).Start();
        }

        public void Collect()
        {
            ThreadPool.SetMinThreads(cycleNum, cycleNum);
            ThreadPool.SetMaxThreads(cycleNum, cycleNum);
            for (int i = 1; i <= cycleNum; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadFun));
            }
        }//method 

        public void DownloadFun(object o)
        {
            while(_avs.Count > 0)
            {
                Av a = _avs.Dequeue();

                string url = basePath + "/" + a.Id;
                string html = "";
                try
                {
                    //html = client.GetStringAsync(url).Result;
                    html = Download.GetHtml(url);
                }
                catch (Exception e)
                {
                    _syncContext.Post(OutError, a.Id + " :查询网站异常");
                    continue;
                }
                
                if (html.IndexOf("404 Page Not Found") != -1)
                {
                    // /search/111&parent=ce
                    _syncContext.Post(OutError, a.Id + " :找不到该番号");
                }
                else
                {
                    int fg = a.Path.LastIndexOf("\\") + 1;
                    string name = a.Path.Substring(fg, a.Path.Length - fg).Trim();
                    string apath = a.Path.Substring(0, fg);
                    a.Html = html;
                    string qb = "<li class=\"active\"><a href=\"" + basePath + "/\">";
                    if (html.IndexOf(qb) != -1)
                    {
                        apath += "骑兵";
                    }
                    else
                    {
                        apath += "步兵";
                    }
                    apath += "\\" + name;
                    MoveFolder(a.Path, apath);
                    Directory.Delete(a.Path, true);

                }
            }//while
            _syncContext.Post(OutLog, "线程退出");
        }//method


        private void OutLog(object state)
        {
            CollectorLog?.Invoke(this, state.ToString());
        }

        private void OutError(object state)
        {
            ErrorLog?.Invoke(this, state.ToString());
        }


        /// <summary>
        /// 移动文件夹中的所有文件夹与文件到另一个文件夹  
        /// </summary>
        /// <param name="sourcePath">源文件夹</param>
        /// <param name="destPath">目标文件夹</param>
        public static void MoveFolder(string sourcePath, string destPath)
        {
            if (Directory.Exists(sourcePath))
            {
                if (!Directory.Exists(destPath))
                {
                    //目标目录不存在则创建
                    try
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("创建目标目录失败：" + ex.Message);
                    }
                }
                //获得源文件下所有文件
                List<string> files = new List<string>(Directory.GetFiles(sourcePath));
                files.ForEach(c =>
                {
                    string destFile = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //覆盖模式
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }
                    File.Move(c, destFile);
                });
                //获得源文件下所有目录文件
                List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));

                folders.ForEach(c =>
                {
                    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //Directory.Move必须要在同一个根目录下移动才有效，不能在不同卷中移动。
                    //Directory.Move(c, destDir);

                    //采用递归的方法实现
                    MoveFolder(c, destDir);
                });
            }
            else
            {
                throw new DirectoryNotFoundException("源目录不存在！");
            }
        }


    }//class
}
