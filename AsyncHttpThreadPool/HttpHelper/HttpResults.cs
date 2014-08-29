using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace AsyncHttpThreadQueue.HttpHelper
{


    /// <summary>
    /// Http返回参数类
    /// </summary>
    public class HttpResults
    {
        private string m_redirectUrl;

        public string RedirectUrl
        {
            get { return m_redirectUrl; }
            set { m_redirectUrl = value; }
        }

        private string guid;

        public string Guid
        {
            get { return guid; }
            set { guid = value; }
        }

        private Exception m_exception;

        public Exception m_Exception
        {
            get { return m_exception; }
            set { m_exception = value; }
        }

        private string _encode;

        public string Encode
        {
            get { return _encode; }
            set { _encode = value; }
        }

        string _Cookie = string.Empty;
        /// <summary>
        /// Http请求返回的Cookie
        /// </summary>
        public string Cookie
        {
            get { return _Cookie; }
            set { _Cookie = value; }
        }
        CookieCollection cookiecollection = null;
        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return cookiecollection; }
            set { cookiecollection = value; }
        }
        private string html = string.Empty;
        /// <summary>
        /// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空
        /// </summary>
        public string Html
        {
            get { return html; }
            set { html = value; }
        }
        private byte[] resultbyte = null;
        /// <summary>
        /// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空
        /// </summary>
        public byte[] ResultByte
        {

            get { return resultbyte; }
            set { resultbyte = value; }
        }
        private WebHeaderCollection header = new WebHeaderCollection();
        //header对象
        public WebHeaderCollection Header
        {
            get { return header; }
            set { header = value; }
        }

        public WebExceptionStatus WebExceptionStatus { get; set; }

    }

}
