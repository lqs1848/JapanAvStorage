using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AvCoverDownloader
{
    class Download
    {
        public static void HttpDownloadFile(string url, string path, string fileName)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (File.Exists(path + fileName))
                return;
            
            try
            {
                // 设置参数
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Proxy = null;
                //request.Proxy = new WebProxy("127.0.0.1:1080");

                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                double dataLengthToRead = response.ContentLength;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();

                //创建本地文件写入流
                Stream stream = new FileStream(path + fileName + ".covertemp", FileMode.Create);
                byte[] bArr = new byte[1024 * 512];
                
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    stream.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                stream.Close();
                responseStream.Close();
                File.Move(path + fileName + ".covertemp", path + fileName);
            }
            catch (Exception ex)
            {
                Console.Out.Write(ex.StackTrace);
            }

        }//method

        public static string GetHtml(string url, Encoding ed)
        {
            string Html = string.Empty;//初始化新的webRequst
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
            Request.Proxy = null;
            //Request.Proxy = new WebProxy("127.0.0.1:1080");
            Request.KeepAlive = true;
            Request.ProtocolVersion = HttpVersion.Version11;
            Request.Method = "GET";
            Request.Accept = "*/* ";
            Request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/536.5 (KHTML, like Gecko) Chrome/19.0.1084.56 Safari/536.5";
            Request.Referer = url;

            HttpWebResponse htmlResponse = (HttpWebResponse)Request.GetResponse();
            //从Internet资源返回数据流
            Stream htmlStream = htmlResponse.GetResponseStream();
            //读取数据流
            StreamReader weatherStreamReader = new StreamReader(htmlStream, ed);
            //读取数据

            Html = weatherStreamReader.ReadToEnd();
            weatherStreamReader.Close();
            htmlStream.Close();
            htmlResponse.Close();
            //针对不同的网站查看html源文件
            return Html;
        }

        public static string GetHtml(string url)
        {
            return GetHtml(url, Encoding.UTF8);
        }
    }
}
