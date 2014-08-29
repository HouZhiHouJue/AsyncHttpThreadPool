using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace AsyncHttpThreadQueue.HttpHelper
{
    public sealed class MyAsyncPara
    {
        public const int DefaultTimeOutSpan = 30 * 1000;
        public HttpResults result = new HttpResults();
        //HttpWebRequest对象用来发起请求
        public HttpWebRequest request = null;
        //获取影响流的数据对象
        public HttpWebResponse response = null;
        //响应流对象
        public Stream streamResponse;
        public int Length = 8 * 1024;
        public Byte[] buffer = new Byte[8 * 1024];
        public Action<HttpResults> callBack;
        public MemoryStream MemoryStream = new MemoryStream();
        public HttpItems objhttpItem;
        public int m_semaphore = 0;

    }
}
