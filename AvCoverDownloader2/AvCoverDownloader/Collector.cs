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

        //javlibrary
        String basePath = "http://www.w24j.com";
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

                string coverpath = a.Path + "\\cover.jpg";
                    
                string url = basePath + "/cn/vl_searchbyid.php?keyword=" + a.Id;
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
                
                if (html.IndexOf("搜寻没有结果") != -1)
                {
                    _syncContext.Post(OutError, a.Id + " :找不到该番号");
                }
                else
                {
                    a.Html = html;
                    try
                    {
                        DownloadCover(a);
                    }
                    catch (Exception e)
                    {
                        _syncContext.Post(OutError, a.Id + " :采集封面失败");
                        continue;
                     }
                    
                }
            }//while
            _syncContext.Post(OutLog, "线程退出");
        }//method

        public void DownloadCover(Av av)
        {
            Match match = new Regex(@"<img id=""video_jacket_img"" src=""//(?<url>.*?)""").Match(av.Html);
            if (match.Success)
            {
                string url = match.Groups["url"].Value;
                url = "http://" + url;
                Download.HttpDownloadFile(url,av.Path+"\\", "art.jpg");
                url = url.Replace("pl.jpg", "ps.jpg");
                Download.HttpDownloadFile(url, av.Path + "\\", "folder.jpg");

                //修改文件夹名称
                try
                {
                    StringBuilder sb = new StringBuilder();
                    Match startNameMatch = new Regex(@"<td class=""header"">演员:</td>\s*<td class=""text"">(?<name>.*?)</td>").Match(av.Html);
                    if (startNameMatch.Success)
                    {
                        string startName = ReplaceHtmlTag(startNameMatch.Groups["name"].Value);
                        if(startName.Length > 100)
                        {
                            string[] startNames = startName.Trim().Split(' ');
                            int x = 0;
                            int y = 0;
                            foreach (string n in startNames)
                            {
                                if (++x > 20)
                                {
                                    y++; x = 0;
                                    File.Create(av.Path + "\\" + av.Id + " " + sb.ToString() + ".star" + y + ".txt").Close();
                                    sb.Clear();
                                }  
                            }
                            if (sb.Length > 0)
                            {
                                File.Create(av.Path + "\\" + av.Id + " " + sb.ToString() + ".star" + (y == 0 ? "" : y + 1 + "") + ".txt").Close();
                            }
                        }
                        else
                            File.Create(av.Path + "\\" + av.Id + " " + startName.Trim().Replace(' ',',') + ".star.txt").Close();
                    }
                }
                catch (Exception e)
                {
                    _syncContext.Post(OutError, av.Id + " :获取女优失败");
                }

                try
                {
                    //match = new Regex(@"<h3>(?<title>.*?)</h3>[\s\S]*?<p><span class=""header"">發行日期:</span>(?<date>.*?)</p>").Match(av.Html);
                    match = new Regex(@"<div id=""video_title""><h3 class=""post-title text""><a.*?>(?<title>.*?)</a></h3>[.\s\S]*<td class=""header"">发行日期:</td>\s*<td class=""text"">(?<date>.*?)</td>
").Match(av.Html);
                    if (match.Success)
                    {
                        string newfolderName = match.Groups["date"].Value.Trim() + " " + match.Groups["title"].Value.Trim();
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

                        int fg = av.Path.LastIndexOf("\\");
                        string basePath = av.Path.Substring(0, fg).Trim();
                        System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(av.Path);
                        folder.MoveTo(basePath+ "\\" + newfolderName);
                    }
                    else
                    {
                        ExeLog.WriteLog("获取AV名称失败:\r\n" + av.Html);
                        _syncContext.Post(OutError, av.Id + " :获取AV名称失败");
                    }
                }
                catch (Exception e)
                {
                    _syncContext.Post(OutError, av.Id + " :修改AV名称失败");
                }


                _syncContext.Post(OutLog, av.Id);
            }
        }


        private void OutLog(object state)
        {
            CollectorLog?.Invoke(this, state.ToString());
        }

        private void OutError(object state)
        {
            ErrorLog?.Invoke(this, state.ToString());
        }

        public static string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");

            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length);

            return strText;
        }

    }//class
}
