using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.TaskItems;

namespace AsyncHttpThreadQueue.Thread
{
    interface IThreadPool
    {
        /// <summary>
        /// 添加线程
        /// </summary>
        void AddTask(ITaskItem thread);
    }
}
