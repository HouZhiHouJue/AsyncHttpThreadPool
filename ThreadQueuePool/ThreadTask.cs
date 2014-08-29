using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.TaskItems;
using AsyncHttpThreadQueue.Thread;

namespace AsyncHttpThreadQueueDemo
{
    public class ThreadTask : ITaskItem
    {
        public void DoWork()
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
