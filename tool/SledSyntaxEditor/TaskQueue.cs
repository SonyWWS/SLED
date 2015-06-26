/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Threading;

namespace Sce.Sled.SyntaxEditor
{
    // A class that holds a queue of tasks to be executed either on a
    // background worker thread or on the UI thread.
    public class TaskQueue
    {
        public enum Thread
        {
            Worker,
            UI
        }

        // A single task to be executed on either the worker or UI thread
        protected class Task
        {
            public Action Action;  // The work load
            public object Key;     // A key used to identify the type of task
            public Thread Thread;  // Should this be executed on the worker or UI thread?

            public Task(Action action, Thread thread, object key)
            {
                Action = action;
                Thread = thread;
                Key = key;
            }
        }

        protected System.Threading.Thread m_thread;
        protected volatile bool m_bThreadStop;
        protected Semaphore m_actionPending = new Semaphore(0, Int32.MaxValue);
        protected EventWaitHandle m_threadExit = new EventWaitHandle(false, EventResetMode.AutoReset);
        protected Queue<Task> m_tasks = new Queue<Task>();
        protected Object m_lock = new Object();
        protected SynchronizationContext m_uiSyncContext = SynchronizationContext.Current;

        public void Start()
        {
            Stop();
            m_bThreadStop = false;
            m_thread = new System.Threading.Thread(Main);
            m_thread.Start();
        }

        public void Stop()
        {
            if (null != m_thread)
            {
                m_bThreadStop = true;      // Indicate we want the thread to stop
                m_actionPending.Release(); // Inform the thread there's something to do
                m_threadExit.WaitOne();    // Wait on the thread to finish
                m_thread = null;
                m_tasks.Clear();
                m_actionPending = new Semaphore(0, Int32.MaxValue);
            }
        }

        public void AddTask(Action action, Thread thread, object key)
        {
            Task task = new Task(action, thread, key);

            lock (m_lock)
            {
                // Add the task to the queue
                m_tasks.Enqueue(task);
            }

            // Inform the thread there is something to do
            m_actionPending.Release();
        }

        public void AddTask(Action action, Thread thread)
        {
            AddTask(action, thread, null);
        }

        public void RemoveTasks(object key)
        {
            lock (m_lock)
            {
                Task nopTask = new Task(delegate() { }, Thread.Worker, null);

                // We can't actually remove the tasks as it will screw the semaphore count
                // so nop the task instead.
                Queue<Task> filtered = new Queue<Task>();
                while (m_tasks.Count > 0)
                {
                    Task task = m_tasks.Dequeue();
                    bool keysMatch = (null != task.Key) && task.Key.Equals(key);
                    filtered.Enqueue(keysMatch ? nopTask : task);
                }

                m_tasks = filtered;
            }
        }

        public bool HasTasks(object key)
        {
            lock (m_lock)
            {
                foreach (Task task in m_tasks)
                {
                    bool keysMatch = (null != task.Key) && task.Key.Equals(key);
                    if (keysMatch)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // The worker thread entry point
        protected void Main()
        {
            while (m_actionPending.WaitOne()) // Wait for an action to do
            {
                if (m_bThreadStop)
                {
                    break;
                }

                Task task;

                lock (m_lock)
                {
                    // Grab a task off the queue
                    task = m_tasks.Dequeue();
                }

                switch (task.Thread)
                {
                    case Thread.Worker:
                        // We're already on the worker thread. Just invoke the delegate.
                        task.Action();
                        break;

                    case Thread.UI:
                        EventWaitHandle complete = new EventWaitHandle(false, EventResetMode.AutoReset);

                        // Post a message to the UI thread's sync context
                        // We can't use the synchronous Send function as we can deadlock when the
                        // UI thread is closing.
                        m_uiSyncContext.Post((object t) =>
                        {
                            // Perform the task on the UI thread.
                            task.Action();

                            // Indicate that the task has been done.
                            complete.Set();

                        }, null);

                        while (false == complete.WaitOne(1000))
                        {
                            // Task has still not been performed.
                            // Are we shutting down?
                            if (m_bThreadStop)
                            {
                                break;
                            }
                        }
                        break;
                }
            }

            m_threadExit.Set();
        }
    }
}
