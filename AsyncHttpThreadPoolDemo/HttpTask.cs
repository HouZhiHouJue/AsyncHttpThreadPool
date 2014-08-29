using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.TaskItems;
using AsyncHttpThreadQueue.HttpHelper;
using AsyncHttpThreadQueue.Thread;

namespace AsyncHttpThreadQueueDemo
{
    public class HttpTask : ITaskItem
    {
        private string m_url;
        HttpItems hi = new HttpItems();
        AsyncHttpHelper wh = new AsyncHttpHelper();

        public HttpTask(string url)
        {
            m_url = url;
        }

        public void DoWork()
        {
            hi.URL = m_url;
            wh.GetHtmlDataAsync(hi, YourCallBack);
        }

        void YourCallBack(HttpResults result)
        {
            try
            {
                //TODO:DO Your Work Here
            }
            finally
            {
                m_release.Release();
            }
        }

        IRelease m_release;

        public void SetRelease(IRelease release)
        {
            m_release = release;
        }
    }
}
