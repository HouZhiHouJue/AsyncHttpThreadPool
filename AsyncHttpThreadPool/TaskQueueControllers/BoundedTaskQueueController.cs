using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.TaskQueues;
using AsyncHttpThreadQueue.TaskItems;
using System.Threading;

namespace AsyncHttpThreadQueue.TaskQueueControllers
{
    public sealed class BoundedTaskQueueController : TaskQueueControllerBase
    {
        private int _maxTasksCount;
        private int _producersWaiting;


        public BoundedTaskQueueController(ITaskQueue taskQueue, int _maxTasksCount)
            : base(taskQueue)
        {
            this._maxTasksCount = _maxTasksCount;
        }

        public override void Enqueue(ITaskItem item)
        {
            lock (_locker)
            {
                while (_taskQueue.Count == _maxTasksCount && !_isDisposed)
                {
                    _producersWaiting++;
                    Monitor.Wait(_locker);
                    _producersWaiting--;
                }
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
                if (_producersWaiting > 0)
                    Monitor.PulseAll(_locker);
            }
            return taskItem;
        }
    }
}
