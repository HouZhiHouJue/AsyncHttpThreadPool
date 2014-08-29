using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.TaskQueues;
using AsyncHttpThreadQueue.TaskItems;
using System.Threading;

namespace AsyncHttpThreadQueue.TaskQueueControllers
{
    public abstract class TaskQueueControllerBase : ITaskQueueController
    {
        protected readonly object _locker = new object();
        protected readonly ITaskQueue _taskQueue;
        protected int _consumersWaiting;
        protected bool _isDisposed;

        protected TaskQueueControllerBase(ITaskQueue taskQueue)
        {
            if (taskQueue == null)
                throw new ArgumentNullException("taskQueue");
            _taskQueue = taskQueue;
        }

        #region ITaskQueueController Members

        public virtual int ConsumersWaiting
        {
            get
            {
                lock (_locker)
                    return _consumersWaiting;
            }
        }

        public abstract void Enqueue(ITaskItem item);
        public abstract ITaskItem Dequeue();

        public virtual void Dispose()
        {
            lock (_locker)
            {
                if (!_isDisposed)
                {
                    GC.SuppressFinalize(this);
                    _isDisposed = true;
                    Monitor.PulseAll(_locker);
                }
            }
        }

        #endregion

        ~TaskQueueControllerBase()
        {
            lock (_locker)
            {
                if (!_isDisposed)
                {
                    _isDisposed = true;
                    Monitor.PulseAll(_locker);
                }
            }
        }


        public int Count
        {
            get
            {
                return _taskQueue.Count;
            }
        }
    }
}
