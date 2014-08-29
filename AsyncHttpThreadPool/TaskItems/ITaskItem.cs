using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncHttpThreadQueue.Thread;

namespace AsyncHttpThreadQueue.TaskItems
{
    public interface ITaskItem
    {
        void DoWork();
        void SetRelease(IRelease release);
    }
}
