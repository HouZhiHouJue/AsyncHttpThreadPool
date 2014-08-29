using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.TaskQueues;
using AsyncHttpThreadQueue.TaskItems;
using System.Threading;

namespace AsyncHttpThreadQueue.TaskQueueControllers
{
    public sealed class DefaultTaskQueueController : TaskQueueControllerBase
    {
        public DefaultTaskQueueController(ITaskQueue taskQueue)
            : base(taskQueue)
        {
        }

        #region Overrides of TaskQueueControllerBase

        public override void Enqueue(ITaskItem item)
        {
            lock (_locker)
            {
                _taskQueue.Enqueue(item);
                if (_consumersWaiting > 0)
                    Monitor.PulseAll(_locker);
            }
        }

        public override ITaskItem Dequeue()
        {
            ITaskItem taskItem;
            lock (_locker)
            {
                while (_taskQueue.Count == 0 && !_isDisposed)
                {
                    _consumersWaiting++;
                    Monitor.Wait(_locker);
                    _consumersWaiting--;
                }
                if (_isDisposed)
                    return null;
                taskItem = _taskQueue.Dequeue();
            }
            return taskItem;
        }

        #endregion
    }
}
