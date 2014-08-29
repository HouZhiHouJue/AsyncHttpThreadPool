#define trace
#undef trace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Web;
using Microsoft.Win32;
using System.Security;

namespace AsyncHttpThreadQueue.HttpHelper
{
    public class AsyncHttpHelper : IDisposable
    {
        #region 预定义方法或者变更

        public void GetHtmlDataAsync(HttpItems objhttpItem, Action<HttpResults> callBack)
        {
            MyAsyncPara pa = new MyAsyncPara();
#if trace
            TraceDic.AddAsyncUrl(objhttpItem.urlGuid, objhttpItem.URL);
            pa.result.Guid = objhttpItem.urlGuid;
#endif
            pa.objhttpItem = objhttpItem;
            pa.callBack = callBack;
            try
            {
                SetRequest(pa);
                IAsyncResult m_ar = pa.request.BeginGetResponse(GetResponseCallback, pa);
                System.Threading.ThreadPool.RegisterWaitForSingleObject(m_ar.AsyncWaitHandle,
                    TimeoutCallback, pa, MyAsyncPara.DefaultTimeOutSpan, true);
            }
            catch (Exception ex)
            {
                ProcessError(pa, ex);
            }

        }

        //改进  超时改为同步发送
        static void TimeoutCallback(object state, bool timedOut)
        {
            System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString() + "超时。");
            MyAsyncPara pa = state as MyAsyncPara;
            if (timedOut)
            {
                if (System.Threading.Interlocked.Increment(ref pa.m_semaphore) == 1)
                {
                  //  ProcessError(pa);
                    try
                    {
                        pa.response = (HttpWebResponse)pa.request.GetResponse();
                        if (pa.response.ContentEncoding != null && pa.response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                        {
                            //开始读取流并设置编码方式
                            //new GZipStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                            //.net4.0以下写法
                            pa.streamResponse = new GZipStream(pa.response.GetResponseStream(), CompressionMode.Decompress);
                        }
                        else
                        {
                            //开始读取流并设置编码方式
                            //response.GetResponseStream().CopyTo(_stream, 10240);
                            //.net4.0以下写法
                            pa.streamResponse = pa.response.GetResponseStream();
                        }
                        // pa.streamResponse.BeginRead(pa.buffer, 0, pa.Length, ReadResponseStreamCallBack, pa);
                        pa.MemoryStream = GetMemoryStream(pa.streamResponse);
                        System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString() + "改为同步发送后成功。");
                        ProcessData(pa);
                      
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString() + "改为同步发送后失败。");
                        ProcessError(pa, ex);
                    }
                }
            }
        }

        static void GetResponseCallback(IAsyncResult result)
        {
            MyAsyncPara pa = result.AsyncState as MyAsyncPara;
            if (System.Threading.Interlocked.Increment(ref pa.m_semaphore) != 1)
                return;
            try
            {
                pa.response = (HttpWebResponse)pa.request.EndGetResponse(result);
                if (pa.response.ContentEncoding != null && pa.response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                {
                    //开始读取流并设置编码方式
                    //new GZipStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                    //.net4.0以下写法
                    pa.streamResponse = new GZipStream(pa.response.GetResponseStream(), CompressionMode.Decompress);
                }
                else
                {
                    //开始读取流并设置编码方式
                    //response.GetResponseStream().CopyTo(_stream, 10240);
                    //.net4.0以下写法
                    pa.streamResponse = pa.response.GetResponseStream();
                }
                // pa.streamResponse.BeginRead(pa.buffer, 0, pa.Length, ReadResponseStreamCallBack, pa);
                pa.MemoryStream = GetMemoryStream(pa.streamResponse);
                ProcessData(pa);
            }
            catch (Exception ex)
            {
                ProcessError(pa, ex);
            }
        }

        private static MemoryStream GetMemoryStream(Stream streamResponse)
        {
            MemoryStream _stream = new MemoryStream();
            int Length = 1024 * 8;
            Byte[] buffer = new Byte[Length];
            int bytesRead = streamResponse.Read(buffer, 0, Length);
            // write the required bytes  
            while (bytesRead > 0)
            {
                _stream.Write(buffer, 0, bytesRead);
                bytesRead = streamResponse.Read(buffer, 0, Length);
            }
            return _stream;
        }

        //static void ReadResponseStreamCallBack(IAsyncResult result)
        //{
        //    MyAsyncPara pa = result.AsyncState as MyAsyncPara;
        //    lock (pa.m_lock)
        //    {
        //        if (pa.DataProcessed)
        //            return;
        //    }
        //    try
        //    {
        //        int bytesRead = pa.streamResponse.EndRead(result);
        //        if (bytesRead > 0)
        //        {
        //            pa.MemoryStream.Write(pa.buffer, 0, bytesRead);
        //            System.Threading.Thread.Yield();
        //            IAsyncResult ar = pa.streamResponse.BeginRead(pa.buffer, 0, pa.Length, ReadResponseStreamCallBack, pa);
        //            //System.Threading.ThreadPool.RegisterWaitForSingleObject(ar.AsyncWaitHandle, TimeoutCallback, pa, MyAsyncPara.DefaultTimeOutSpan, true);
        //        }
        //        else
        //        {
        //            ProcessData(pa);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessError(pa, ex);
        //    }
        //}

        static void ProcessData(MyAsyncPara pa)
        {
            try
            {
                string urlEncode = string.Empty;
                string htmlData = string.Empty;
                byte[] RawResponse = pa.MemoryStream.ToArray();
                string responseChracterset = pa.response.CharacterSet.Trim();
                //BOM 字节码
                if (RawResponse[0] == 0xEF && RawResponse[1] == 0xBB && RawResponse[2] == 0xBF)
                {
                    urlEncode = "UTF-8";
                }
                if (string.IsNullOrEmpty(urlEncode) && !string.IsNullOrEmpty(pa.response.ContentType))
                {
                    Match meta = Regex.Match(pa.response.ContentType, @"charset[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<char>[^\s\t\r\n""'<>]*)[^<>]*?[\s\t\r\n]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string charter = (meta.Groups.Count >= 1) ? meta.Groups["char"].Value : string.Empty;
                    if (!string.IsNullOrEmpty(charter))
                    {
                        urlEncode = charter;
                        TestEncoding(ref urlEncode);
                    }

                }
                if (string.IsNullOrEmpty(urlEncode))
                {
                    if (!string.IsNullOrEmpty(pa.response.CharacterSet.Trim().ToLower().Replace("iso-8859-1", "")))
                    {
                        urlEncode = pa.response.CharacterSet.Trim().ToLower();
                        TestEncoding(ref urlEncode);
                    }
                }
                if (string.IsNullOrEmpty(urlEncode))
                {
                    string temp = Encoding.Default.GetString(RawResponse, 0, RawResponse.Length);
                    Match meta = Regex.Match(temp, @"<meta\b[^<>]*?charset[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<char>[^\s\t\r\n""'<>]*)[^<>]*?[\s\t\r\n]*>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string charter = (meta.Groups.Count >= 1) ? meta.Groups["char"].Value : string.Empty;
                    if (!string.IsNullOrEmpty(charter))
                    {
                        charter = charter.ToLower().Replace("iso-8859-1", "gbk");
                        urlEncode = charter;
                        TestEncoding(ref urlEncode);
                    }
                }
                if (string.IsNullOrEmpty(urlEncode))
                {
                    if (pa.response.CharacterSet.ToLower().Trim() == "iso-8859-1")
                        urlEncode = "gbk";
                    else
                        urlEncode = "UTF-8";
                }
                if (urlEncode.ToLower() == "utf8")
                    urlEncode = "UTF-8";
                //得到返回的HTML
                htmlData = Encoding.GetEncoding(urlEncode).GetString(RawResponse);
                pa.result.Html = htmlData;
                pa.result.Encode = urlEncode;
            }
            catch (Exception ex)
            {
                ProcessError(pa, ex);
            }
            finally
            {
                pa.MemoryStream.Close();
                pa.response.Close();
                pa.request.Abort();
                pa.callBack(pa.result);
                pa = null;
            }
        }

        private static bool TestEncoding(ref string encoding)
        {
            try
            {
                Encoding e = Encoding.GetEncoding(encoding);
                return true;
            }
            catch
            {
                encoding = string.Empty;
                return false;
            }
        }

        private static void ProcessError(MyAsyncPara pa, Exception ex = null)
        {
            try
            {
                if (ex is WebException)
                    pa.result.WebExceptionStatus = ((WebException)ex).Status;
                pa.result.m_Exception = ex == null ? new WebException("操作超时，程序主动取消。") : ex;
                pa.result.Html = "0";
                if (pa.request != null)
                    pa.request.Abort();
                if (pa.response != null)
                    pa.response.Close();
                if (pa.MemoryStream != null)
                    pa.MemoryStream.Close();
            }
            catch { }
            finally
            {
                pa.callBack(pa.result);
                pa = null;
            }
        }

        /// <summary>
        /// 为请求准备参数
        /// </summary>
        ///<param name="objhttpItem">参数列表</param>
        /// <param name="_Encoding">读取数据时的编码方式</param>
        private void SetRequest(MyAsyncPara pa)
        {
            // 验证证书
            SetCer(pa);
            // 设置代理          
            SetProxy(pa);
            //请求方式Get或者Post
            pa.request.Method = pa.objhttpItem.Method;
            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息
            pa.request.UserAgent = pa.objhttpItem.UserAgent;
            #region 注释
            //string strIPNew = GetRandomIP();
            //request.Headers.Add("X_FORWARDED_FOR", strIPNew);
            //request.Headers.Add("REMOTE_ADDR", strIPNew);
            //request.Headers.Add("VIA", strIPNew);
            //int k = NumberRandom(0, 6);
            //switch (k)
            //{
            //    case 0:
            //        request.UserAgent = "Baiduspider+(+http://www.baidu.com/search/spider.htm)";
            //        break;
            //    case 1:
            //        request.UserAgent = "Googlebot/2.1 (+http://www.google.com/bot.html)";
            //        break;
            //    case 2:
            //        request.UserAgent = @"Sogou web spider/3.0(+http://www.sogou.com/docs/help/webmasters.htm#07)";
            //        break;
            //    default:
            //        request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            //        break;
            //} 
            #endregion
            pa.request.Timeout = pa.objhttpItem.Timeout;
            pa.request.ReadWriteTimeout = pa.objhttpItem.ReadWriteTimeout;
            pa.request.KeepAlive = false;
            //Accept
            pa.request.Accept = pa.objhttpItem.Accept;
            //ContentType返回类型
            pa.request.ContentType = pa.objhttpItem.ContentType;
            // 编码
            //   SetEncoding(objhttpItem);  --暂时不用
            //设置Cookie
            //   SetCookie(objhttpItem);   --暂时不用
            //这个的意思是请求的来源网址
            //有的网站上的图片只允许他们本站的来路显示，放在别处或是放在地址栏里均不能显示，怎么才能伪造来路下载图片？ 
            //HttpWebRequest.Referer = "图片的域名" 
            pa.request.Referer = string.IsNullOrEmpty(pa.objhttpItem.Referer) ? Uri.EscapeUriString(pa.objhttpItem.URL) : Uri.EscapeUriString(pa.objhttpItem.Referer);
            //是否执行跳转功能
            pa.request.AllowAutoRedirect = pa.objhttpItem.Allowautoredirect;
            //设置Post数据
            //    SetPostData(objhttpItem);  --暂时不用
            //设置最大连接
            if (pa.objhttpItem.Connectionlimit > 0)
            {
                pa.request.ServicePoint.ConnectionLimit = pa.objhttpItem.Connectionlimit;
            }
        }

        /// <summary>
        /// 设置证书
        /// </summary>
        /// <param name="objhttpItem"></param>
        private void SetCer(MyAsyncPara pa)
        {
            if (!string.IsNullOrEmpty(pa.objhttpItem.CerPath))
            {
                //这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                //初始化对像，并设置请求的URL地址
                pa.request = (HttpWebRequest)WebRequest.Create(GetUrl(pa.objhttpItem.URL));
                //创建证书文件
                X509Certificate objx509 = new X509Certificate(pa.objhttpItem.CerPath);
                //添加到请求里
                pa.request.ClientCertificates.Add(objx509);
            }
            else
            {
                //初始化对像，并设置请求的URL地址
                pa.request = (HttpWebRequest)WebRequest.Create(GetUrl(pa.objhttpItem.URL));
            }
        }
        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="objhttpItem">Http参数</param>
        private void SetCookie(MyAsyncPara pa)
        {
            if (!string.IsNullOrEmpty(pa.objhttpItem.Cookie))
            {
                //Cookie
                pa.request.Headers[HttpRequestHeader.Cookie] = pa.objhttpItem.Cookie;
            }
            //设置Cookie
            if (pa.objhttpItem.CookieCollection != null)
            {
                pa.request.CookieContainer = new CookieContainer();
                pa.request.CookieContainer.Add(pa.objhttpItem.CookieCollection);
            }
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="objhttpItem">参数对象</param>
        private void SetProxy(MyAsyncPara pa)
        {
            if (string.IsNullOrEmpty(pa.objhttpItem.ProxyUserName) && string.IsNullOrEmpty(pa.objhttpItem.ProxyPwd) && string.IsNullOrEmpty(pa.objhttpItem.ProxyIp))
            {
                //不需要设置
            }
            else
            {
                //设置代理服务器
                WebProxy myProxy = new WebProxy(pa.objhttpItem.ProxyIp, false);
                //建议连接
                myProxy.Credentials = new NetworkCredential(pa.objhttpItem.ProxyUserName, pa.objhttpItem.ProxyPwd);
                //给当前请求对象
                pa.request.Proxy = myProxy;
                //设置安全凭证
                pa.request.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
        }
        /// <summary>
        /// 回调验证证书问题
        /// </summary>
        /// <param name="sender">流对象</param>
        /// <param name="certificate">证书</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            // 总是接受    
            return true;
        }
        #endregion
        #region 普通类型
        /// <summary>    
        /// 传入一个正确或不正确的URl，返回正确的URL
        /// </summary>    
        /// <param name="URL">url</param>   
        /// <returns>
        /// </returns>    
        private static string GetUrl(string URL)
        {
            if (!(URL.Contains("http://") || URL.Contains("https://")))
            {
                URL = "http://" + URL;
            }
            return URL;
        }

        #endregion
        #region Disposable
        //该方法定义在IDisposable接口中。
        public void Dispose()
        {
            //该方法由程序调用，在调用该方法之后对象将被终结。
            //因为我们不希望垃圾回收器再次终结对象，因此需要从终结列表中去除该对象。
            GC.SuppressFinalize(this);//系统不再去调用Finalize方法，以便重复调用
            //因为是由程序调用该方法的，因此参数为true。
            Dispose(true);
        }
        //所有与回收相关的工作都由该方法完成
        protected virtual void Dispose(bool isDisposing)
        {
            lock (this)
            {
                if (isDisposing)
                {
                    //需要程序员完成释放对象占用的资源。
                    //  Console.WriteLine("释放托管资源");　
                }
                //对象将被垃圾回收器终结。在这里添加其它和清除对象相关的代码。
                // Console.WriteLine("释放非托管资源");
                //request = null;
                // response = null;
            }
        }
        ~AsyncHttpHelper()
        {
            //垃圾回收器将调用该方法，因此参数需要为false。
            Dispose(false);
        }
        #endregion
    }
}
