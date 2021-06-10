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
        string basePath = "";
        public EventHandler<String> CollectorLog;
        public EventHandler<String> ErrorLog;
        SynchronizationContext _syncContext;
        //HttpClient client;

        //线程数
        const int cycleNum = 10;

        string filePath = null;
        
        public Collector(SynchronizationContext formContext,string filePath, string path)
        {
            _syncContext = formContext;
            this.filePath = filePath;
            basePath = path;
        }

        public void Start()
        {
            new Thread(Collect).Start();
        }

        Queue<int> q = new Queue<int>();

        public void Collect()
        {
            for (int i = 1; i <= 250; i++)
            {
                q.Enqueue(i);
            }
            ThreadPool.SetMinThreads(cycleNum, cycleNum);
            ThreadPool.SetMaxThreads(cycleNum, cycleNum);
            for (int i = 1; i <= cycleNum; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadFun));
            }
        }//method 

        public void DownloadFun(object o)
        {
            while (q.Count > 0) {
                int i = q.Dequeue();
                string url = basePath + "/actresses/" + i;
                string html = Download.GetHtml(url);
                MatchCollection mcs = new Regex(@"<img\ssrc=""(.*?)""\stitle=""(.*?)"">").Matches(html);
                foreach (Match m in mcs) {

                    string startName = GetPath(m.Groups[2].Value.Trim());
                    string startPhoto = m.Groups[1].Value.Trim();
                    if (!startPhoto.StartsWith("http")) {
                        startPhoto = basePath + startPhoto;
                    }
                    if (startPhoto.IndexOf("nowprinting") == -1) {
                        string pr = startName.Substring(0, 1);
                        string dirstr = filePath + "\\" + pr + "\\"+ startName+"\\";
                        Download.HttpDownloadFile(startPhoto, dirstr, "poster.jpg");
                        _syncContext.Post(OutLog, startName);
                    }
                }//foreach
            }
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

        private string GetPath(string newfolderName) {
            newfolderName = newfolderName.Replace("\\", "‖");
            newfolderName = newfolderName.Replace("/", "‖");
            newfolderName = newfolderName.Replace(":", "：");
            newfolderName = newfolderName.Replace("*", "※");
            newfolderName = newfolderName.Replace("?", "？");
            newfolderName = newfolderName.Replace("<", "〈");
            newfolderName = newfolderName.Replace(">", "〉");
            newfolderName = newfolderName.Replace("\n", " ");
            newfolderName = newfolderName.Replace("\r", " ");
            //newfolderName = Regex.Replace(newfolderName, @"[/n/r]", " ");

            StringBuilder rBuilder = new StringBuilder(newfolderName);
            foreach (char rInvalidChar in Path.GetInvalidPathChars())
                rBuilder.Replace(rInvalidChar.ToString(), string.Empty);
            return rBuilder.ToString();
        }

    }//class
}
