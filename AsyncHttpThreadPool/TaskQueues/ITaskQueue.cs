using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.Thread;
using AsyncHttpThreadQueue.TaskItems;

namespace AsyncHttpThreadQueue.TaskQueues
{
    public interface ITaskQueue
    {
        int Count { get; }
        void Enqueue(ITaskItem item);
        ITaskItem Dequeue();
    }
}
