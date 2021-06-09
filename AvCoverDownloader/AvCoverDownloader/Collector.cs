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
                Av a = _avs.Dequeue();

                string coverpath = a.Path + "\\cover.jpg";
                /*
                if (File.Exists(coverpath))
                {
                    _syncContext.Post(OutLog, "曾经下载过:" + a.Id + " 跳过\r\n");
                    continue;
                }
                */
                    
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
            Match match = new Regex(@"<a class=""bigImage"" href=""(?<url>.*?)"">").Match(av.Html);
            if (match.Success)
            {
                string url = match.Groups["url"].Value;
                if (!url.StartsWith("http")) {
                    url = basePath + url;
                }
                Download.HttpDownloadFile(url,av.Path+"\\", "art.jpg");
                //int fg = url.LastIndexOf("/") + 1;
                //string name = url.Substring(fg, url.Length - fg - 6).Trim();
                url = url.Replace("_b", "");
                string qb = "<li class=\"active\"><a href=\"" + basePath + "/\">";
                if (av.Html.IndexOf(qb) != -1)
                {
                    url = url.Replace("cover", "thumb");
                }
                else
                {
                    url = url.Replace("cover", "thumbs");
                }
                
                Download.HttpDownloadFile(url, av.Path + "\\", "folder.jpg");

                //修改文件夹名称
                try
                {
                    StringBuilder sb = new StringBuilder();
                    MatchCollection startNames = new Regex(@"<div class=""star-name""><a href="".*?"" title="".*?"">(?<name>.*?)</a></div>").Matches(av.Html);
                    int i = 0;
                    int x = 0;
                    int y = 0;
                    foreach (Match m in startNames)
                    {
                        sb.Append(m.Groups["name"].Value);
                        i++;
                        if (i < startNames.Count)
                            sb.Append(",");
                        if (++x > 20) {
                            y++; x = 0;
                            File.Create(av.Path + "\\" + av.Id + " " + sb.ToString() + ".star"+y+".txt").Close();
                            sb.Clear();
                        }
                    }
                    if(sb.Length > 0)
                    {
                        File.Create(av.Path + "\\" + av.Id + " " + sb.ToString()+".star"+(y==0?"":y+1+"")+".txt").Close();
                    }
                }
                catch (Exception e)
                {
                    _syncContext.Post(OutError, av.Id + " :获取女优失败");
                }

                try
                {
                    //match = new Regex(@"<h3>(?<title>.*?)</h3>[\s\S]*?<p><span class=""header"">發行日期:</span>(?<date>.*?)</p>").Match(av.Html);
                    match = new Regex(@"<h3>(?<title>[\s\S]*?)</h3>[\s\S]*?<p><span class=""header"">發行日期:</span>(?<date>.*?)</p>").Match(av.Html);
                    
                    string newPath = null;
                    if (match.Success)
                    {
                        av.date = match.Groups["date"].Value.Trim();
                        av.title = match.Groups["title"].Value.Trim();

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
                        string baseFilePath = av.Path.Substring(0, fg).Trim();
                        System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(av.Path);

                        string apath = "";
                        qb = "<li class=\"active\"><a href=\"" + basePath + "/\">";
                        if (av.Html.IndexOf(qb) != -1)
                        {
                            apath += "骑兵\\";
                        }
                        else
                        {
                            apath += "步兵\\";
                        }
                        MoveFolder(av.Path, baseFilePath + "\\" + apath + newfolderName);
                        Directory.Delete(av.Path, true);
                        //folder.MoveTo();
                        newPath = baseFilePath + "\\" + apath + newfolderName;
                        av.Path = newPath;
                    } 
                    else
                    {
                        ExeLog.WriteLog("获取AV名称失败:\r\n" + av.Html);
                        _syncContext.Post(OutError, av.Id + " :获取AV名称失败");
                        return;
                    }

                    //修改视频名称 改为符合 emby的规则
                    string[] extNames = new string[] { ".avi", ".mov", ".mpg", ".RA", ".RM", ".RMVB", ".WMV", ".mkv", ".mp4", ".asf", ".m4v", ".VOB"};
                    DirectoryInfo fdir = new DirectoryInfo(newPath);
                    FileInfo[] file = fdir.GetFiles();
                    if (file.Length != 0)
                    { //当前目录文件或文件夹不为空                   
                        for (int i = 0; i < extNames.Length; i++)
                        {
                            int x = 0;
                            foreach (FileInfo f in file)
                                if (extNames[i].ToLower().Equals(f.Extension.ToLower()))
                                    x++;
                            int y = 1;
                            foreach (FileInfo f in file) //显示当前目录所有文件   
                                if (extNames[i].ToLower().Equals(f.Extension.ToLower()))
                                {
                                    string newName = newPath + "\\" + av.Id + (x > 1 ? "-cd" + y : "") + f.Extension;
                                    av.CD1 = x > 1;
                                    try
                                    {
                                        //记录文件名称
                                        ChangeNameLogTxt(f.Name, newName);
                                        File.Move(f.FullName, newName);
                                        //File.Create(folder + "\\"+ (x > 1 ? "-cd" + y : "") + f.Name + ".old.name").Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        _syncContext.Post(OutError, av.Id + "-修改视频文件名称错误 \r\n");
                                    }
                                    y++;
                                }//for files
                        }//for ext
                    }//file
                }
                catch (Exception e)
                {
                    _syncContext.Post(OutError, av.Id + " :修改AV名称失败" + e.Message);
                }

                _syncContext.Post(OutLog, av.Id);

                try
                {
                    creNfo(av);
                }
                catch (Exception e)
                {
                    _syncContext.Post(OutLog, av.Id + ":nfo 创建失败" + e.Message);
                }
            }
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


        public static void creNfo(Av av)
        {
            Match mat = null;
            string p = av.title.Replace(av.Id, "").Trim();
            //string plot = "google:"+Translate.google(p) + " youdao:"+Translate.youdao(p);

            string fileStr = av.Path + "\\" + av.Id + (av.CD1 ? "-cd1" : "") + ".nfo";
            FileStream fs1 = new FileStream(fileStr, FileMode.Create, FileAccess.Write);//创建写入文件
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
            sw.WriteLine("<movie>");
            //sw.WriteLine("<plot>" + plot + "</plot>");
            sw.WriteLine("<outline/>");
            sw.WriteLine("<lockdata>false</lockdata>");
            sw.WriteLine("<title>" + av.Id + "</title>");
            sw.WriteLine("<originaltitle>" + p + "</originaltitle>");
            mat = new Regex(@"<p><span class=""header"">導演:</span> <a href="".*?"">(.*?)</a></p>").Match(av.Html);
            if (mat.Success)
            {
                sw.WriteLine("<director>" + mat.Groups[1].Value.Trim() + "</director>");
            }
            sw.WriteLine("<year>" + av.date.Substring(0, 4) + "</year>");
            sw.WriteLine("<premiered>" + av.date + "</premiered>");
            sw.WriteLine("<releasedate>" + av.date + "</releasedate>");

            mat = new Regex(@"<p><span class=""header"">系列:</span> <a href="".*?"">(.*?)</a></p>").Match(av.Html);
            if (mat.Success)
            {
                sw.WriteLine("<genre>" + mat.Groups[1].Value.Trim() + "</genre>");
            }

            mat = new Regex(@"<p><span class=""header"">長度:</span>\s*(\d*)分鐘</p>").Match(av.Html);
            if (mat.Success)
            {
                sw.WriteLine("<runtime>" + mat.Groups[1].Value.Trim() + "</runtime>");
            }

            mat = new Regex(@"<p><span class=""header"">製作商:</span> <a href="".*?"">(.*?)</a>").Match(av.Html);
            if (mat.Success)
            {
                sw.WriteLine("<studio>" + mat.Groups[1].Value.Trim() + "</studio>");
            }

            MatchCollection tags = new Regex(@"<span class=""genre""><a href="".*?"">(.*?)</a></span>").Matches(av.Html);
            foreach (Match m in tags)
            {
                sw.WriteLine("<tag>" + m.Groups[1].Value.Trim() + "</tag>");
            }
            MatchCollection actors = new Regex(@"<div class=""photo-frame"">\s*<img src=""(.*?)"" title=""(.*?)"">\s*</div>\s*<span>(.*?)</span>").Matches(av.Html);
            foreach (Match m in actors)
            {
                sw.WriteLine("<actor>");
                sw.WriteLine("<name>" + m.Groups[2].Value.Trim() + "</name>");
                //sw.WriteLine("<role></role>");
                sw.WriteLine("<type>Actor</type>");
                sw.WriteLine("<thumb>" + m.Groups[1].Value.Trim() + "</thumb>");
                sw.WriteLine("</actor>");
            }
            sw.WriteLine("</movie>");
            sw.Close();
            fs1.Close();
        }

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
