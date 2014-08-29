using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.TaskItems;

namespace AsyncHttpThreadQueue.TaskQueueControllers
{
    public interface ITaskQueueController : IDisposable
    {
        int ConsumersWaiting { get; }
        int Count { get; }
        void Enqueue(ITaskItem item);
        ITaskItem Dequeue();
    }
}
