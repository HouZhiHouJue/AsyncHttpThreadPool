
using AsyncHttpThreadQueue.TaskItems;
using AsyncHttpThreadQueue.TaskQueues;
using AsyncHttpThreadQueue.TaskQueueControllers;
using System.Collections.Generic;


namespace AsyncHttpThreadQueue.Thread
{
    public class ExtendedThreadPool : IThreadPool, IRelease, Increment
    {
        private ITaskQueueController TaskQueueController;
        private readonly List<WorkThread> _workThreads = new List<WorkThread>();
        private readonly object m_threadCreateLocker = new object();
        private readonly object m_countLocker = new object();
        private readonly int maxThreadCount;
        private readonly int m_tcpConnectionMaxCount;
        private int m_tcpConnectionCount;

        public override string ToString()
        {
            return TaskQueueController.Count.ToString();
        }

        public ExtendedThreadPool(int tcpConnectionMaxCount, int maxThreadCount, int maxQueueCount)
        {
            TaskQueueController = new BoundedTaskQueueController(new DefaultTaskQueue(), maxQueueCount);
            this.maxThreadCount = maxThreadCount;
            this.m_tcpConnectionMaxCount = tcpConnectionMaxCount;
        }

        public void AddTask(ITaskItem task)
        {
            task.SetRelease(this);
            TaskQueueController.Enqueue(task);
            CreateWorkThread();
        }

        private void CreateWorkThread()
        {
            if (_workThreads.Count >= maxThreadCount)//双检锁
                return;
            lock (m_threadCreateLocker)
            {
                if (_workThreads.Count >= maxThreadCount)
                    return;
                var workThread = new WorkThread(TaskQueueController, this);
                _workThreads.Add(workThread);
                var thread = new System.Threading.Thread(workThread.Start)
                {
                    Name = string.Format("WorkThread {0}", _workThreads.Count),
                    IsBackground = true,
                };
                workThread.Thread = thread;
                thread.Start();
            }
        }

        public bool IsNull()
        {
            return TaskQueueController.Count == 0;
        }

        public void Increase()
        {
            lock (m_countLocker)
            {
                m_tcpConnectionCount++;
                if (m_tcpConnectionCount >= m_tcpConnectionMaxCount)
                    System.Threading.Monitor.Wait(m_countLocker);
            }
        }

        public void Release()
        {
            lock (m_countLocker)
            {
                m_tcpConnectionCount--;
                if (m_tcpConnectionCount < m_tcpConnectionMaxCount)
                    System.Threading.Monitor.Pulse(m_countLocker);
            }
        }
    }
}
