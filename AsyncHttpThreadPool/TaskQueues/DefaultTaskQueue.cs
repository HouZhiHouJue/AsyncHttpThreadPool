using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.TaskItems;

namespace AsyncHttpThreadQueue.TaskQueues
{
    public sealed class DefaultTaskQueue : ITaskQueue
    {
        private readonly Queue<ITaskItem> _queue = new Queue<ITaskItem>();

        #region Implementation of ITaskQueue

        public int Count
        {
            get { return _queue.Count; }
        }

        public void Enqueue(ITaskItem item)
        {
            _queue.Enqueue(item);
        }

        public ITaskItem Dequeue()
        {
            return _queue.Dequeue();
        }

        #endregion
    }
}
