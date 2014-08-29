using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.Thread;

namespace AsyncHttpThreadQueueDemo
{
    public class ThreadTaskManager
    {
        ExtendedThreadPool m_pool;

        public ThreadTaskManager()
        {
            m_pool = new ExtendedThreadPool(int.MaxValue, 2, 10);
        }

        public void AddTask()
        {
            ThreadTask task = new ThreadTask();
            m_pool.AddTask(task);
        }

        public bool IsNull()
        {
            return m_pool.IsNull();
        }
    }
}
