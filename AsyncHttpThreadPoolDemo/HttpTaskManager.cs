using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.Thread;

namespace AsyncHttpThreadQueueDemo
{
    public class HttpTaskManager
    {
        ExtendedThreadPool m_pool;

        public HttpTaskManager()
        {
            m_pool = new ExtendedThreadPool(int.MaxValue, 2, 10);
        }

        public void AddTask()
        {
            for (int i = 0; i < 10000; i++)
            {
                HttpTask task = new HttpTask("http://www.baidu.com");
                m_pool.AddTask(task);
            }
        }
    }
}
