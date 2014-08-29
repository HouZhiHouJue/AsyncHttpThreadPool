using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.TaskQueueControllers;
using AsyncHttpThreadQueue.TaskItems;

namespace AsyncHttpThreadQueue.Thread
{
    public sealed class WorkThread : IDisposable
    {
        private readonly ITaskQueueController _taskQueueController;
        private volatile bool _isRun = true;
        private Increment m_increment;

        public WorkThread(ITaskQueueController taskQueueController, Increment increment)
        {
            if (taskQueueController == null)
                throw new ArgumentNullException("taskQueueController", "taskQueueController is null");
            _taskQueueController = taskQueueController;
            m_increment = increment;
        }

        public System.Threading.Thread Thread { private get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
            _taskQueueController.Dispose();
            Thread.Join();
        }

        #endregion

        public void Start()
        {
            while (_isRun)
            {
                ITaskItem workItem = null;
                workItem = _taskQueueController.Dequeue();
                if (workItem == null)
                    continue;
                m_increment.Increase();
                workItem.DoWork();
            }
        }

        private void Stop()
        {
            _isRun = false;
        }

    }
}
