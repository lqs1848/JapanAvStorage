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
        
        public Collector(SynchronizationContext formContext, List<Av> avs, string path)
        {
            _avs = new Queue<Av>(avs);
            _syncContext = formContext;
            basePath = path;
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
                Av a;
                try
                {
                    a = _avs.Dequeue();
                }
                catch (InvalidOperationException e) {
                    return;
                }
                
                string url = basePath + "/" + a.Id;
                string html = "";
                try
                {
                    html = Download.GetHtml(url);
                }
                catch (Exception e)
                {
                    _syncContext.Post(OutError, a.Id + " :查询网站异常");
                    continue;
                }
                if (html.IndexOf("404 Page Not Found") != -1)
                {
                    _syncContext.Post(OutError, a.Id + " :找不到该番号");
                }
                else
                {
                    int fg = a.Path.LastIndexOf("\\") + 1;
                    string name = a.Path.Substring(fg, a.Path.Length - fg).Trim();
                    System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(a.Path);

                    if (!Directory.Exists(a.ToPath + "\\" + a.Id))
                        Directory.CreateDirectory(a.ToPath + "\\" + a.Id);
                    if (a.Oname.IndexOf("-cd") != -1)
                    {
                        folder.MoveTo(a.ToPath + "\\" + a.Id + "\\" + a.Oname);
                    }
                    else 
                    { 
                        if (File.Exists(a.ToPath + "\\" + a.Id + "\\" + name))
                        {
                            _syncContext.Post(OutLog, a.Path + "出现重复!!!!!!!!!!" +a.Id + "");
                        }
                        else
                        {
                            folder.MoveTo(a.ToPath + "\\" + a.Id + "\\" + name);
                            File.Create(a.ToPath + "\\"+ a.Id + "\\" + folder.Name + ".old.name").Close();
                        }
                    }
                    _syncContext.Post(OutLog, a.Id + "");
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

    }//class
}
