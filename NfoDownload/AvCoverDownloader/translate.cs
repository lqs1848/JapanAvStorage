using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AvCoverDownloader
{
    public static class Translate
    {
        static string TranslateGoogleString(string translateContent, string fromLanguage = "ja", string toLanguage = "zh-CN")
        {
            string transRetHtml = string.Empty;

            string encodedStr = HttpUtility.UrlEncode(translateContent); //URL编码
            string url = string.Format("http://translate.google.cn/translate_a/single?client=t&sl={0}&tl={1}&hl={0}&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&dt=at&ie=UTF-8&oe=UTF-8&ssel=6&tsel=3&kc=0&tk=522626|172097&q={2}", fromLanguage, toLanguage, encodedStr);
            //https://translate.google.cn/translate_a/single?client=webapp&sl={0}&tl={1}&hl={1}&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&otf=1&ssel=3&tsel=6&xid=1782844&kc=1&tk=786665.702189&q={2}
            try
            {
                var bytes = new WebClient().DownloadData(url);
                transRetHtml = Encoding.UTF8.GetString(bytes);

                var index = transRetHtml.IndexOf("]],");
                transRetHtml = transRetHtml.Substring(0, index + 1).Replace("[[", "");
                transRetHtml = transRetHtml = transRetHtml.Replace("[\"", "");
                transRetHtml = transRetHtml.Substring(0, transRetHtml.IndexOf("\""));

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return transRetHtml;
        }


        public static string google(string str) {
            /*str = TranslateGoogleString(str);
            return str;*/
                str = Uri.EscapeDataString(str);
            //string url = string.Format("https://translate.google.cn/translate_a/single?client=webapp&sl={0}&tl={1}&hl={1}&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&otf=1&ssel=3&tsel=6&xid=1782844&kc=1&tk=786665.702189&q={2}", "ja", "zh-CN", "utf-8");
            string html = GetHtml("http://translate.google.cn/translate_a/single?client=gtx&dt=t&dj=1&ie=UTF-8&sl=ja&tl=zh-CN&q=" + str, Encoding.UTF8);
                return new Regex(@"""trans"":""(.*?)""").Match(html).Groups[1].Value;
            }

            public static string youdao(string str) {
                str = Uri.EscapeDataString(str);
                string html = Post("http://fanyi.youdao.com/translate", "doctype=json&type=JA2ZH_CN&i=" + str);
                return new Regex(@"""tgt"":""(.*?)""").Match(html).Groups[1].Value;
            }

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


        public static string Post(string url, string postData)
            {
                return Post(url, postData, "application/x-www-form-urlencoded");
            }

            public static string Post(string url, string postData, string contentType)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = contentType;
                request.Method = "POST";
                request.Timeout = 300000;

                byte[] bytes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = bytes.Length;
                Stream writer = request.GetRequestStream();
                writer.Write(bytes, 0, bytes.Length);
                writer.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8);
                string result = reader.ReadToEnd();
                response.Close();
                return result;
            }

            public static string Post(string url, string postData, string userName, string password)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "text/html; charset=UTF-8";
                request.Method = "POST";

                string usernamePassword = userName + ":" + password;
                CredentialCache credentialCache =
                    new CredentialCache { { new Uri(url), "Basic", new NetworkCredential(userName, password) } };
                request.Credentials = credentialCache;
                request.Headers.Add("Authorization",
                    "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(usernamePassword)));

                byte[] bytes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = bytes.Length;
                Stream writer = request.GetRequestStream();
                writer.Write(bytes, 0, bytes.Length);
                writer.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException(), Encoding.ASCII);
                string result = reader.ReadToEnd();
                response.Close();
                return result;
            }

            //static CookieContainer cookie = new CookieContainer();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="url">请求的servlet地址，不带参数</param>
            /// <param name="postData"></param>
            /// <returns>请求的参数，key=value&key1=value1</returns>
            public static string doHttpPost(string url, string postData)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                SetHeaderValue(request.Headers, "Content-Type", "application/json");
                SetHeaderValue(request.Headers, "Accept", "application/json");
                SetHeaderValue(request.Headers, "Accept-Charset", "utf-8");
                request.Method = "POST";
                request.Timeout = 300000;

                byte[] bytes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = bytes.Length;
                Stream writer = request.GetRequestStream();
                writer.Write(bytes, 0, bytes.Length);
                writer.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8);
                string result = reader.ReadToEnd();
                response.Close();
                return result;
            }

            /// <summary>
            /// 偶发性超时时试看看
            /// </summary>
            /// <param name="url"></param>
            /// <param name="postData"></param>
            /// <returns></returns>
            public static string HttpPostForTimeOut(string url, string postData)
            {
                //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                //watch.Start();
                GC.Collect();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                //request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
                //int a = Encoding.UTF8.GetByteCount(postData);
                request.Timeout = 20 * 600 * 1000;


                ServicePointManager.Expect100Continue = false;
                ServicePointManager.DefaultConnectionLimit = 200;

                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;

                Stream myRequestStream = request.GetRequestStream();
                StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("utf-8")); //如果JSON有中文则是UTF-8
                myStreamWriter.Write(postData);
                myStreamWriter.Close(); //请求中止,是因为长度不够,还没写完就关闭了.

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //watch.Stop();  //停止监视
                //TimeSpan timespan = watch.Elapsed;  //获取当前实例测量得出的总时间
                //System.Diagnostics.Debug.WriteLine("打开窗口代码执行时间：{0}(毫秒)", timespan.TotalMinutes);  //总毫秒数

                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream ?? throw new InvalidOperationException(), Encoding.GetEncoding("utf-8"));
                string registerResult = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return registerResult;
            }


            public static void SetHeaderValue(WebHeaderCollection header, string name, string value)
            {
                var property =
                    typeof(WebHeaderCollection).GetProperty("InnerCollection",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                if (property != null)
                {
                    if (property.GetValue(header, null) is NameValueCollection collection) collection[name] = value;
                }
            }
        }
}
